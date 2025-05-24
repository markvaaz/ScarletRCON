using System;

namespace ScarletRCON.CommandSystem;

[AttributeUsage(AttributeTargets.Method)]
public class RconCommandAttribute(string name, string description = "", string usage = null) : Attribute {
  public string Name { get; } = name;
  public string Description { get; } = description;
  public string Usage { get; } = usage;
}
