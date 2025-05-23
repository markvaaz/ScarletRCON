using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace ScarletRCON.CommandSystem;

public static class CommandHandler {
  internal static Dictionary<string, List<RconCommandDefinition>> Commands { get; private set; } = [];

  internal static void Initialize() {
    RegisterAll(Assembly.GetExecutingAssembly());
  }

  public static void RegisterAll() {
    RegisterAll(Assembly.GetCallingAssembly());
  }

  public static void RegisterAll(Assembly asm) {
    var prefix = $"{asm.GetName().Name.ToLowerInvariant()}.";

    if (prefix == "scarletrcon.") {
      prefix = string.Empty;
    }

    Type[] types;
    try {
      types = asm.GetTypes();
    } catch (ReflectionTypeLoadException e) {
      types = e.Types.Where(t => t != null).ToArray();
    } catch {
      return;
    }

    foreach (var type in types) {
      foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) {
        var attr = method.GetCustomAttribute<RconCommandAttribute>();
        if (attr != null) {
          string baseName = attr.Name.ToLowerInvariant();
          string fullCommandName = $"{prefix}{baseName}";

          string usage = attr.Usage;

          if (string.IsNullOrWhiteSpace(usage)) {
            var parameters = method.GetParameters();
            if (parameters.Length == 0) {
              usage = "";
            } else {
              usage = string.Join(" ", parameters.Select(p =>
                p.ParameterType == typeof(List<string>) ? "<...text>" : $"<{p.Name}>"
              ));
            }
          }

          if (!Commands.ContainsKey(fullCommandName)) Commands[fullCommandName] = new List<RconCommandDefinition>();

          Commands[fullCommandName].Add(new RconCommandDefinition(fullCommandName, attr.Description, usage, method, null));
        }
      }
    }

    // Sort commands
    Commands = Commands.Values
      .SelectMany(c => c)
      .OrderBy(c => c.Name)
      .GroupBy(c => c.Name)
      .ToDictionary(g => g.Key, g => g.ToList());
  }

  public static void UnregisterAssembly() {
    UnregisterAssembly(Assembly.GetCallingAssembly());
  }

  public static void UnregisterAssembly(Assembly asm) {
    var toRemove = new List<string>();

    foreach (var kvp in Commands) {
      kvp.Value.RemoveAll(cmdDef => cmdDef.Method.DeclaringType.Assembly == asm);

      if (kvp.Value.Count == 0) {
        toRemove.Add(kvp.Key);
      }
    }

    foreach (var key in toRemove) {
      Commands.Remove(key);
    }
  }


  internal static string HandleCommand(string cmd, List<string> args) {
    if (!TryGetCommand(cmd, args, out var def, out var error)) {
      return error;
    }

    var parameters = def.Method.GetParameters();
    if (args.Count < parameters.Length - (parameters.LastOrDefault()?.ParameterType == typeof(List<string>) ? 1 : 0))
      return $"Usage: {cmd} {def.Usage}";

    object[] parsedArgs = new object[parameters.Length];
    for (int i = 0; i < parameters.Length; i++) {
      var paramType = parameters[i].ParameterType;

      if (i == parameters.Length - 1 && paramType == typeof(List<string>)) {
        parsedArgs[i] = args.Skip(i).ToList();
        break;
      }

      if (i >= args.Count)
        return $"Missing value for parameter '{parameters[i].Name}'.";

      try {
        parsedArgs[i] = Convert.ChangeType(args[i], paramType);
      } catch {
        return $"Invalid value for '{parameters[i].Name}', expected {paramType.Name}";
      }
    }

    try {
      return CommandExecutor.Enqueue(() => {
        object result = def.Method.Invoke(def.TargetInstance, parsedArgs);
        return result?.ToString() ?? "Command executed.";
      });
    } catch (Exception e) {
      return $"Error: {e.InnerException?.Message ?? e.Message}";
    }
  }

  internal static bool TryGetCommand(string name, List<string> args, out RconCommandDefinition command, out string error) {
    command = null;
    error = null;

    if (!Commands.TryGetValue(name, out var defs)) {
      error = $"Unknown command '{name}'";
      return false;
    }

    var candidates = defs.Where(d => {
      var ps = d.Method.GetParameters();
      if (ps.Length == 0 && args.Count == 0) return true;

      if (ps.Length > 0 && ps[^1].ParameterType == typeof(List<string>)) {
        return args.Count >= ps.Length - 1;
      }

      return ps.Length == args.Count;
    }).ToList();

    if (candidates.Count == 0) {
      error = $"Invalid arguments for command '{name}'. try:\n{string.Join("\n", defs.Select(d => $"\u001b[90m{d.Name} {d.Usage}\u001b[0m"))}";
      return false;
    }

    foreach (var def in candidates) {
      var parameters = def.Method.GetParameters();
      var parsedArgs = new object[parameters.Length];
      bool canInvoke = true;

      for (int i = 0; i < parameters.Length; i++) {
        var paramType = parameters[i].ParameterType;
        if (i == parameters.Length - 1 && paramType == typeof(List<string>)) {
          parsedArgs[i] = args.Skip(i).ToList();
          break;
        }

        if (i >= args.Count) {
          canInvoke = false;
          break;
        }

        try {
          parsedArgs[i] = Convert.ChangeType(args[i], paramType);
        } catch {
          canInvoke = false;
          break;
        }
      }

      if (canInvoke) {
        command = def;
        return true;
      }
    }

    error = $"Invalid arguments for command '{name}'. try:\n\u001b[90m{string.Join("\n", defs.Select(d => $"{d.Name} {d.Usage}"))}\u001b[0m";
    return false;
  }

  public static bool TryGetCommands(string name, out List<RconCommandDefinition> commands) => Commands.TryGetValue(name, out commands);
}
