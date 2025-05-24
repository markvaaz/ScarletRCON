using ScarletRCON.CommandSystem;

namespace ScarletRCON.Commands;

[RconCommandCategory("Help")]
public static class HelpCommand {
  [RconCommand("help", "Show info about a specific command.")]
  public static string Help(string commandName) {
    if (CommandHandler.TryGetCommands(commandName, out var commands)) {
      string white = "\x1b[97m";
      string gray = "\u001b[90m";
      string reset = "\x1b[0m";

      var result = "";

      foreach (var command in commands) {
        result += $"{white}{command.Name}{reset}: {gray}{command.Description}{reset}\n";

        if (string.IsNullOrEmpty(command.Usage)) {
          result += $"{gray}- this command has no parameters{reset}\n";
        } else result += $"{gray}- usage: {command.Usage}{reset}\n";
      }

      return result;
    }

    return $"Unknown command '{commandName}'";
  }

  [RconCommand("help", "List all commands.")]
  public static string ListAll() {
    string result = "";

    foreach (var commands in CommandHandler.CommandGroups) {
      result += $"\n\x1b[97m{commands.Key}\u001b[0m:\n";
      foreach (var command in commands.Value) {
        result += $"\u001b[37m- {command.Name}\u001b[0m:\u001b[90m{(string.IsNullOrEmpty(command.Usage) ? "" : " " + command.Usage)} - {command.Description}\u001b[0m\n";
      }
    }

    foreach (var commands in CommandHandler.CustomCommandGroups) {
      result += $"\n\x1b[97m{commands.Key}\u001b[0m:\n";
      foreach (var command in commands.Value) {
        result += $"\u001b[37m- {command.Name}\u001b[0m:\u001b[90m{(string.IsNullOrEmpty(command.Usage) ? "" : " " + command.Usage)} - {command.Description}\u001b[0m\n";
      }
    }

    result += "\nTotal commands: " + CommandHandler.Commands.Count;

    return result;
  }
}