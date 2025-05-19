using System;

namespace ScarletRCON.CommandSystem;

[AttributeUsage(AttributeTargets.Method)]
public class RconCommandAttribute : Attribute {
  public string Name { get; }
  public string Description { get; }
  public string Usage { get; }

  public RconCommandAttribute(string name, string description = "", string usage = null) {
    Name = name;
    Description = description;
    Usage = usage;
  }
}
