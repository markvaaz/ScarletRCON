using System.Reflection;

namespace ScarletRCON.CommandSystem;

public class RconCommandDefinition(string name, string description, string usage, MethodInfo method, object instance, bool isAsync = false) {
  public string Name { get; } = name;
  public string Description { get; } = description;
  public string Usage { get; } = usage;
  public MethodInfo Method { get; } = method;
  public object TargetInstance { get; } = instance;
  public bool IsAsync { get; } = isAsync;
}
