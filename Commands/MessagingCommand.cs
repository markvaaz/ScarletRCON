using System.Collections.Generic;
using ScarletRCON.Systems;
using ScarletRCON.CommandSystem;
using ScarletRCON.Services;
using ProjectM.Network;

namespace ScarletRCON.Commands;

[RconCommandCategory("Messaging")]
public static class MessagingCommands {
  [RconCommand("announce", "Send a message to all connected players.", "<message>")]
  public static string Announce(List<string> args) {
    string message = string.Join(" ", args);

    SystemMessages.SendAll(message);

    return $"Sent announcement: {message}";
  }

  [RconCommand("announcerestart", "Announce server restart in X minutes.", "<minutes>")]
  public static string AnnounceRestart(int minutes) {
    string message = $"The server will restart in {minutes} minutes!";

    SystemMessages.SendAll(message);

    return $"Sent announcement: {message}";
  }

  [RconCommand("private", "Send a private message to a connected player.", "<playerName> <message>")]
  public static string PrivateMessage(string playerName, string message) {
    if (!PlayerService.TryGetByName(playerName, out var player) || !player.IsOnline) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    SystemMessages.Send(player.UserEntity.Read<User>(), message);

    return $"Sent private message to {player.Name}: {message}";
  }
}
