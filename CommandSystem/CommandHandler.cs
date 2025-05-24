using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace ScarletRCON.CommandSystem;

public static class CommandHandler {
  internal static Dictionary<string, List<RconCommandDefinition>> Commands { get; private set; } = [];
  public static Dictionary<string, List<RconCommandDefinition>> CommandGroups { get; private set; } = [];


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
      var groupAttr = type.GetCustomAttribute<RconCommandCategoryAttribute>();
      string group = groupAttr?.Name ?? "Uncategorized";

      foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) {
        var attr = method.GetCustomAttribute<RconCommandAttribute>();
        if (attr == null) continue;

        string baseName = attr.Name.ToLowerInvariant();
        string fullCommandName = string.Join(".", $"{prefix}{baseName}".Split(" "));

        string usage = attr.Usage;
        if (string.IsNullOrWhiteSpace(usage)) {
          var parameters = method.GetParameters();
          usage = parameters.Length == 0
              ? ""
              : string.Join(" ", parameters.Select(p =>
                  p.ParameterType == typeof(List<string>) ? "<...text>" : $"<{p.Name}>"));
        }

        var def = new RconCommandDefinition(fullCommandName, attr.Description, usage, method, null);

        if (!Commands.ContainsKey(fullCommandName))
          Commands[fullCommandName] = [];

        Commands[fullCommandName].Add(def);

        if (!CommandGroups.ContainsKey(group))
          CommandGroups[group] = [];

        CommandGroups[group].Add(def);
      }
    }

    SortCommands();
  }
  public static void RegisterExternalCommandsBatch(IEnumerable<(string Group, string Prefix, MethodInfo Method, string Name, string Description, string Usage)> commands) {
    foreach (var (group, prefix, method, name, description, usage) in commands) {
      string fullCommandName = string.Join(".", $"{prefix}{name.ToLowerInvariant()}".Split(" "));

      var def = new RconCommandDefinition(fullCommandName, description, usage, method, null);

      if (!Commands.ContainsKey(fullCommandName))
        Commands[fullCommandName] = new List<RconCommandDefinition>();

      Commands[fullCommandName].Add(def);

      if (!CommandGroups.ContainsKey(group))
        CommandGroups[group] = new List<RconCommandDefinition>();

      CommandGroups[group].Add(def);
    }

    SortCommands();
  }

  private static string GetTypeName(Type type) {
    if (type == typeof(string)) return "string";
    if (type == typeof(int)) return "int";
    if (type == typeof(float)) return "float";
    if (type == typeof(bool)) return "bool";
    if (type == typeof(List<string>)) return "text[]";
    return type.Name.ToLower();
  }

  public static void SortCommands() {
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
      kvp.Value.RemoveAll(cmdDef =>
          cmdDef.Method.DeclaringType?.Assembly == asm ||
          (cmdDef.Method.DeclaringType == null && cmdDef.Method.Module.Assembly == asm));

      if (kvp.Value.Count == 0) {
        toRemove.Add(kvp.Key);
      }
    }

    foreach (var key in toRemove) {
      Commands.Remove(key);
    }

    // Limpar grupos vazios
    var emptyGroups = CommandGroups.Where(g => g.Value.Count == 0).Select(g => g.Key).ToList();
    foreach (var group in emptyGroups) {
      CommandGroups.Remove(group);
    }
  }

  public static void UnregisterByPrefix(string prefix) {
    var toRemove = Commands.Keys.Where(k => k.StartsWith(prefix)).ToList();
    foreach (var key in toRemove) {
      Commands.Remove(key);
    }

    foreach (var group in CommandGroups.Keys.ToList()) {
      CommandGroups[group].RemoveAll(cmd => cmd.Name.StartsWith(prefix));
      if (CommandGroups[group].Count == 0) {
        CommandGroups.Remove(group);
      }
    }
  }

  internal static string HandleCommand(string cmd, List<string> args) {
    if (!TryGetCommand(cmd, args, out var def, out var error)) {
      return error;
    }

    try {
      var parameters = def.Method.GetParameters();
      object[] parsedArgs = new object[parameters.Length];

      for (int i = 0; i < parameters.Length; i++) {
        var paramType = parameters[i].ParameterType;

        if (i == parameters.Length - 1 && paramType == typeof(List<string>)) {
          parsedArgs[i] = args.Skip(i).ToList();
          break;
        }

        if (i >= args.Count) {
          parsedArgs[i] = paramType.IsValueType ? Activator.CreateInstance(paramType) : null;
          continue;
        }

        try {
          parsedArgs[i] = ConvertArg(args[i], paramType);
        } catch {
          return $"Invalid value for '{parameters[i].Name}', expected {GetTypeName(paramType)}";
        }
      }

      return CommandExecutor.Enqueue(() => {
        try {
          // Permite chamar métodos de outros assemblies
          var result = def.Method.Invoke(def.TargetInstance, parsedArgs);
          return result?.ToString() ?? "Command executed.";
        } catch (TargetInvocationException e) {
          return $"Command error: {e.InnerException?.Message ?? e.Message}";
        } catch (Exception e) {
          return $"Execution error: {e.Message}";
        }
      });
    } catch (Exception e) {
      return $"Error processing command: {e.Message}";
    }
  }

  // Método auxiliar para conversão de argumentos
  private static object ConvertArg(string value, Type targetType) {
    if (targetType == typeof(bool)) {
      return ConvertToBool(value);
    }

    if (targetType.IsEnum) {
      return Enum.Parse(targetType, value, true);
    }

    // Handle nullable types
    var underlyingType = Nullable.GetUnderlyingType(targetType);
    if (underlyingType != null) {
      return string.IsNullOrWhiteSpace(value) ? null : Convert.ChangeType(value, underlyingType);
    }

    return Convert.ChangeType(value, targetType);
  }

  private static bool ConvertToBool(string value) {
    if (bool.TryParse(value, out bool result)) return result;
    if (int.TryParse(value, out int num)) return num != 0;
    return !string.IsNullOrWhiteSpace(value);
  }

  internal static bool TryGetCommand(string name, List<string> args, out RconCommandDefinition command, out string error) {
    command = null;
    error = null;
    try {

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
    } catch (Exception e) {
      Console.WriteLine($"Error processing command: {e.Message}");
      return false;
    }
  }

  public static bool TryGetCommands(string name, out List<RconCommandDefinition> commands) => Commands.TryGetValue(name, out commands);
}
