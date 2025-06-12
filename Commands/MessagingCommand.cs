using System.Collections.Generic;
using ScarletRCON.CommandSystem;
using ScarletCore.Services;
using ProjectM.Network;

namespace ScarletRCON.Commands;

[RconCommandCategory("Messaging")]
public static class MessagingCommands {
  [RconCommand("announce", "Send a message to all online players.", "<message>")]
  public static string Announce(List<string> args) {
    string message = string.Join(" ", args);

    MessageService.SendAll(message);

    return $"Sent announcement: {message}";
  }

  [RconCommand("announcerestart", "Announce server restart in X minutes.", "<minutes>")]
  public static string AnnounceRestart(int minutes) {
    string message = $"The server will restart in {minutes} minutes!";

    MessageService.SendAll(message);

    return $"Sent announcement: {message}";
  }
  [RconCommand("private", "Send a private message to a player.", "<playerName> <message>")]
  public static string PrivateMessage(string playerName, string message) {
    if (!PlayerService.TryGetByName(playerName, out var player)) {
      return $"Player '{playerName}' was not found.";
    }

    if (!player.IsOnline) {
      return $"Player '{playerName}' is not currently online.";
    }

    MessageService.Send(player.UserEntity.Read<User>(), message);

    return $"Sent private message to {player.Name}: {message}";
  }
}
