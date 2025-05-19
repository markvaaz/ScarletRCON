using System.Collections.Generic;
using ScarletRCON.Systems;
using ScarletRCON.CommandSystem;

namespace ScarletRCON.Commands;

public static class AnnounceCommands {
  [RconCommand("announce", "Sends a message to all players connected to the server.", "<message>")]
  public static string Announce(List<string> args) {
    string message = string.Join(" ", args);

    SystemMessages.SendAll(message);

    return message;
  }

  [RconCommand("announcerestart", "Sends a pre-configured message that announces server restart in x minutes.", "<minutes>")]
  public static string AnnounceRestart(int minutes) {
    string message = $"The server will restart in {minutes} minutes!";

    SystemMessages.SendAll(message);

    return message;
  }
}
