using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ScarletRCON.Shared;

[AttributeUsage(AttributeTargets.Class)]
public class RconCommandCategoryAttribute(string groupName) : Attribute {
  public string Name { get; } = groupName;
}

[AttributeUsage(AttributeTargets.Method)]
public class RconCommandAttribute(string name, string description = null, string usage = null) : Attribute {
  public string Name { get; set; } = name;
  public string Description { get; set; } = description;
  public string Usage { get; set; } = usage;
}

public static class RconCommandRegistrar {
  private static bool? _isScarletRconAvailable;

  public static bool IsScarletRconAvailable() {
    _isScarletRconAvailable ??= Type.GetType("ScarletRCON.CommandSystem.CommandHandler, ScarletRCON") != null;
    return _isScarletRconAvailable.Value;
  }

  public static void RegisterAll() {
    if (!IsScarletRconAvailable())
      return;

    Assembly assembly = Assembly.GetCallingAssembly();
    var prefix = $"{assembly.GetName().Name.ToLowerInvariant()}.";

    // Coletar todos os comandos de uma vez
    var commandsToRegister = new List<(string, string, MethodInfo, string, string, string)>();

    foreach (var type in assembly.GetTypes()) {
      var groupAttr = type.GetCustomAttribute<RconCommandCategoryAttribute>();
      string group = groupAttr?.Name ?? "Uncategorized";

      foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) {
        var attr = method.GetCustomAttribute<RconCommandAttribute>();
        if (attr == null) continue;

        string usage = attr.Usage;
        if (string.IsNullOrWhiteSpace(usage)) {
          var parameters = method.GetParameters();
          usage = parameters.Length == 0
              ? ""
              : string.Join(" ", parameters.Select(p =>
                  p.ParameterType == typeof(List<string>) ? "<...text>" : $"<{p.Name}>"));
        }

        commandsToRegister.Add((group, prefix, method, attr.Name, attr.Description, usage));
      }
    }

    // Registrar todos os comandos de uma vez
    var commandHandlerType = Type.GetType("ScarletRCON.CommandSystem.CommandHandler, ScarletRCON");
    var registerMethod = commandHandlerType.GetMethod(
        "RegisterExternalCommandsBatch",
        BindingFlags.Public | BindingFlags.Static
    );

    registerMethod.Invoke(null, [commandsToRegister]);
  }
}