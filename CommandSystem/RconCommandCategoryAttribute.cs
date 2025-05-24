using System;

namespace ScarletRCON.CommandSystem;

[AttributeUsage(AttributeTargets.Class)]
public class RconCommandCategoryAttribute(string groupName) : Attribute {
  public string Name { get; } = groupName;
}
