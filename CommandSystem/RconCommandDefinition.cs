using System.Reflection;

namespace ScarletRCON.CommandSystem;

public class RconCommandDefinition {
  public string Name { get; }
  public string Description { get; }
  public string Usage { get; }
  public MethodInfo Method { get; }
  public object TargetInstance { get; }

  public RconCommandDefinition(string name, string description, string usage, MethodInfo method, object instance) {
    Name = name;
    Description = description;
    Usage = usage;
    Method = method;
    TargetInstance = instance;
  }
}
