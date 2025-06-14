using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace ScarletRCON.CommandSystem;

/// <summary>
/// Handles command registration, discovery, and execution for both synchronous and asynchronous RCON commands.
/// Synchronous commands are queued for frame synchronization, while async commands execute directly.
/// </summary>
public static class CommandHandler {

  /// <summary>
  /// Dictionary containing all registered commands, indexed by command name.
  /// </summary>
  internal static Dictionary<string, List<RconCommandDefinition>> Commands { get; private set; } = [];

  /// <summary>
  /// Dictionary containing commands grouped by their categories.
  /// </summary>
  public static Dictionary<string, List<RconCommandDefinition>> CommandCategories { get; private set; } = [];

  /// <summary>
  /// Dictionary containing external commands grouped by their custom categories.
  /// </summary>
  public static Dictionary<string, List<RconCommandDefinition>> CustomCommandCategories { get; private set; } = [];
  /// <summary>
  /// Initializes the command handler by registering all commands from the executing assembly.
  /// </summary>
  internal static void Initialize() {
    RegisterAll(Assembly.GetExecutingAssembly());
  }

  /// <summary>
  /// Registers all commands from the calling assembly.
  /// </summary>
  public static void RegisterAll() {
    RegisterAll(Assembly.GetCallingAssembly());
  }

  /// <summary>
  /// Registers all commands from the specified assembly.
  /// Commands are discovered via reflection by looking for methods with RconCommandAttribute.
  /// </summary>
  /// <param name="asm">The assembly to scan for commands</param>
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
        string fullCommandName = string.Join(".", $"{prefix}{baseName}".Split(" ")); string usage = attr.Usage;
        if (string.IsNullOrWhiteSpace(usage)) {
          var parameters = method.GetParameters();
          usage = parameters.Length == 0
              ? ""
              : string.Join(" ", parameters.Select(p =>
                  p.ParameterType == typeof(List<string>) ? "<...text>" : $"<{p.Name}>"));
        }

        // Automatically detect if the method is async based on return type
        bool isAsync = IsAsyncMethod(method);

        var def = new RconCommandDefinition(fullCommandName, attr.Description, usage, method, null, isAsync);

        if (!Commands.ContainsKey(fullCommandName))
          Commands[fullCommandName] = [];

        Commands[fullCommandName].Add(def);

        if (!CommandCategories.ContainsKey(group))
          CommandCategories[group] = [];

        CommandCategories[group].Add(def);

        CommandCategories[group] = [.. CommandCategories[group].OrderBy(c => c.Name)];
      }
    }
    SortCommands();
  }

  /// <summary>
  /// Determines if a method is async by checking its return type.
  /// </summary>
  /// <param name="method">The method to check</param>
  /// <returns>True if the method is async, false otherwise</returns>
  private static bool IsAsyncMethod(MethodInfo method) {
    var returnType = method.ReturnType;
    return returnType == typeof(Task) ||
           (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<string>));
  }

  /// <summary>
  /// Registers a batch of external commands with their metadata.
  /// Automatically detects if methods are async based on their return type.
  /// These commands are added to custom command categories.
  /// </summary>
  /// <param name="commands">Collection of command metadata tuples</param>
  public static void RegisterExternalCommandsBatch(IEnumerable<(string Group, string Prefix, MethodInfo Method, string Name, string Description, string Usage)> commands) {
    foreach (var (group, prefix, method, name, description, usage) in commands) {
      string fullCommandName = string.Join(".", $"{prefix}{name.ToLowerInvariant()}".Split(" "));

      bool isAsync = IsAsyncMethod(method);

      var def = new RconCommandDefinition(fullCommandName, description, usage, method, null, isAsync);

      if (!Commands.ContainsKey(fullCommandName))
        Commands[fullCommandName] = [];

      Commands[fullCommandName].Add(def);

      if (!CustomCommandCategories.ContainsKey(group))
        CustomCommandCategories[group] = [];

      CustomCommandCategories[group].Add(def);

      CustomCommandCategories[group] = [.. CustomCommandCategories[group].OrderBy(c => c.Name)];
    }

    SortCommands();
  }

  /// <summary>
  /// Sorts all commands alphabetically by name and rebuilds the command dictionary.
  /// </summary>
  public static void SortCommands() {
    Commands = Commands.Values
      .SelectMany(c => c)
      .OrderBy(c => c.Name)
      .GroupBy(c => c.Name)
      .ToDictionary(g => g.Key, g => g.ToList());
  }

  /// <summary>
  /// Gets a user-friendly type name for display purposes.
  /// </summary>
  /// <param name="type">The type to get the name for</param>
  /// <returns>A simplified type name</returns>
  private static string GetTypeName(Type type) {
    if (type == typeof(string)) return "string";
    if (type == typeof(int)) return "int";
    if (type == typeof(float)) return "float";
    if (type == typeof(bool)) return "bool";
    if (type == typeof(List<string>)) return "text[]";
    return type.Name.ToLower();
  }
  /// <summary>
  /// Unregisters all commands from the calling assembly.
  /// </summary>
  public static void UnregisterAssembly() {
    UnregisterAssembly(Assembly.GetCallingAssembly());
  }

  /// <summary>
  /// Unregisters all commands from the specified assembly.
  /// </summary>
  /// <param name="asm">The assembly whose commands should be unregistered</param>
  public static void UnregisterAssembly(Assembly asm) {
    Console.WriteLine($"[UnregisterAssembly] Called for assembly: {asm.FullName}");

    var assemblyName = asm.GetName().Name.ToLowerInvariant();
    var prefix = assemblyName == "scarletrcon" ? string.Empty : $"{assemblyName}.";

    Console.WriteLine($"[UnregisterAssembly] Using prefix: '{prefix}'");

    var commandsToRemove = Commands.Where(kvp =>
      kvp.Value.Any(cmd => cmd.Method.DeclaringType?.Assembly == asm))
      .Select(kvp => kvp.Key)
      .ToList();

    Console.WriteLine($"[UnregisterAssembly] Commands to remove: {string.Join(", ", commandsToRemove)}");

    foreach (var commandName in commandsToRemove) {
      int before = Commands[commandName].Count;
      Commands[commandName].RemoveAll(cmd => cmd.Method.DeclaringType?.Assembly == asm);
      int after = Commands.ContainsKey(commandName) ? Commands[commandName].Count : 0;
      Console.WriteLine($"[UnregisterAssembly] Removed {before - after} commands from '{commandName}'");
      if (Commands[commandName].Count == 0) {
        Commands.Remove(commandName);
        Console.WriteLine($"[UnregisterAssembly] Removed command key '{commandName}'");
      }
    }

    foreach (var group in CommandCategories.Keys.ToList()) {
      int before = CommandCategories[group].Count;
      CommandCategories[group].RemoveAll(cmd => cmd.Method.DeclaringType?.Assembly == asm);
      int after = CommandCategories.ContainsKey(group) ? CommandCategories[group].Count : 0;
      if (before != after)
        Console.WriteLine($"[UnregisterAssembly] Removed {before - after} commands from group '{group}'");
      if (CommandCategories[group].Count == 0) {
        CommandCategories.Remove(group);
        Console.WriteLine($"[UnregisterAssembly] Removed empty group '{group}'");
      }
    }

    foreach (var group in CustomCommandCategories.Keys.ToList()) {
      int before = CustomCommandCategories[group].Count;
      CustomCommandCategories[group].RemoveAll(cmd => cmd.Method.DeclaringType?.Assembly == asm);
      int after = CustomCommandCategories.ContainsKey(group) ? CustomCommandCategories[group].Count : 0;
      if (before != after)
        Console.WriteLine($"[UnregisterAssembly] Removed {before - after} custom commands from group '{group}'");
      if (CustomCommandCategories[group].Count == 0) {
        CustomCommandCategories.Remove(group);
        Console.WriteLine($"[UnregisterAssembly] Removed empty custom group '{group}'");
      }
    }
  }

  /// <summary>
  /// Unregisters all commands that start with the specified prefix.
  /// </summary>
  /// <param name="prefix">The prefix to match for command removal</param>
  public static void UnregisterByPrefix(string prefix) {
    var toRemove = Commands.Keys.Where(k => k.StartsWith(prefix)).ToList();
    foreach (var key in toRemove) {
      Commands.Remove(key);
    }

    foreach (var group in CommandCategories.Keys.ToList()) {
      CommandCategories[group].RemoveAll(cmd => cmd.Name.StartsWith(prefix));
      if (CommandCategories[group].Count == 0) {
        CommandCategories.Remove(group);
      }
    }
  }

  /// <summary>
  /// Handles command execution, routing to either synchronous or asynchronous processing.
  /// Synchronous commands are queued for frame synchronization, async commands execute directly.
  /// </summary>
  /// <param name="cmd">The command name to execute</param>
  /// <param name="args">The command arguments</param>
  /// <returns>The command execution result or error message</returns>
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
      }      // Execute async command directly without using the synchronization queue
      if (def.IsAsync) {
        return HandleAsyncCommand(def, parsedArgs);
      }

      // Synchronous commands continue using the queue for frame synchronization
      return ActionExecutor.Enqueue(() => {
        try {
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

  /// <summary>
  /// Handles the execution of asynchronous commands.
  /// Waits for Task completion and returns the actual result.
  /// </summary>
  /// <param name="def">The command definition</param>
  /// <param name="parsedArgs">The parsed command arguments</param>
  /// <returns>The command execution result</returns>
  private static string HandleAsyncCommand(RconCommandDefinition def, object[] parsedArgs) {
    try {
      var result = def.Method.Invoke(def.TargetInstance, parsedArgs);

      if (result == null) {
        return "Async command executed (null result).";
      }

      // Check specifically for Task<string>
      if (result is Task<string> stringTask) {
        var taskResult = stringTask.GetAwaiter().GetResult();
        return taskResult ?? "Async command completed.";
      }

      // Check for non-generic Task (void)
      if (result is Task task) {
        task.GetAwaiter().GetResult();
        return "Async command completed.";
      }

      return result.ToString() ?? "Async command executed.";
    } catch (TargetInvocationException e) {
      return $"Command error: {e.InnerException?.Message ?? e.Message}";
    } catch (Exception e) {
      return $"Execution error: {e.Message}";
    }
  }

  /// <summary>
  /// Helper method for converting string arguments to their target types.
  /// Handles special cases like booleans, enums, and nullable types.
  /// </summary>
  /// <param name="value">The string value to convert</param>
  /// <param name="targetType">The target type to convert to</param>
  /// <returns>The converted value</returns>
  private static object ConvertArg(string value, Type targetType) {
    if (targetType == typeof(bool)) {
      return ConvertToBool(value);
    }

    if (targetType.IsEnum) {
      return Enum.Parse(targetType, value, true);
    }    // Handle nullable types
    var underlyingType = Nullable.GetUnderlyingType(targetType);
    if (underlyingType != null) {
      return string.IsNullOrWhiteSpace(value) ? null : Convert.ChangeType(value, underlyingType);
    }

    return Convert.ChangeType(value, targetType);
  }

  /// <summary>
  /// Converts a string value to boolean with flexible parsing.
  /// Supports standard boolean parsing, numeric values (0=false, non-zero=true),
  /// and treats non-empty strings as true.
  /// </summary>
  /// <param name="value">The string value to convert</param>
  /// <returns>The boolean result</returns>
  private static bool ConvertToBool(string value) {
    if (bool.TryParse(value, out bool result)) return result;
    if (int.TryParse(value, out int num)) return num != 0;
    return !string.IsNullOrWhiteSpace(value);
  }

  /// <summary>
  /// Attempts to find and validate a command for execution.
  /// Matches command name and validates argument count and types.
  /// </summary>
  /// <param name="name">The command name to find</param>
  /// <param name="args">The command arguments</param>
  /// <param name="command">The found command definition (if successful)</param>
  /// <param name="error">Error message (if unsuccessful)</param>
  /// <returns>True if command was found and validated, false otherwise</returns>
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
