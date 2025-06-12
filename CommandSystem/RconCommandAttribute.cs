using System;

namespace ScarletRCON.CommandSystem;

[AttributeUsage(AttributeTargets.Method)]
public class RconCommandAttribute(string name, string description = "", string usage = null, bool isAsync = false) : Attribute {
  public string Name { get; } = name;
  public string Description { get; } = description;
  public string Usage { get; } = usage;
  public bool IsAsync { get; } = isAsync;
}
