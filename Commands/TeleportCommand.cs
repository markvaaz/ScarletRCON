using ScarletRCON.CommandSystem;
using ScarletCore.Services;
using Unity.Mathematics;

namespace ScarletRCON.Commands;

[RconCommandCategory("Teleport & Location")]
public static class TeleportCommand {
  [RconCommand("whereis", "Get position of a player.")]
  public static string WhereIs(string playerName) {
    if (!PlayerService.TryGetByName(playerName, out var player)) {
      return $"Player '{playerName}' was not found.";
    }

    var position = player.CharacterEntity.Position();

    return $"- {player.Name}: ({position.x} {position.y} {position.z})\n";
  }
  [RconCommand("teleport", "Teleport a player to coordinates.")]
  public static string Teleport(string playerName, float x, float y, float z) {
    if (!PlayerService.TryGetByName(playerName, out var player)) {
      return $"Player '{playerName}' was not found.";
    }

    if (!player.IsOnline) {
      return $"Player '{playerName}' is not currently online.";
    }

    TeleportService.TeleportToPosition(player.CharacterEntity, new float3(x, y, z));

    return $"Teleported {player.Name} to ({x}, {y}, {z})";
  }
  [RconCommand("teleport", "Teleport a player to another player.")]
  public static string Teleport(string playerName, string targetPlayerName) {

    if (!PlayerService.TryGetByName(playerName, out var player)) {
      return $"Player '{playerName}' was not found.";
    }

    if (!player.IsOnline) {
      return $"Player '{playerName}' is not currently online.";
    }

    if (!PlayerService.TryGetByName(targetPlayerName, out var target)) {
      return $"Target player '{targetPlayerName}' was not found.";
    }

    if (!target.IsOnline) {
      return $"Target player '{targetPlayerName}' is not currently online.";
    }

    TeleportService.TeleportToEntity(player.CharacterEntity, target.CharacterEntity);

    return $"Teleported {player.Name} to {target.Name}'s position.";
  }

  [RconCommand("teleportall", "Teleport all online players to coordinates.")]
  public static string TeleportAll(float x, float y, float z) {
    foreach (var player in PlayerService.AllPlayers) {
      if (!player.IsOnline) continue;

      TeleportService.TeleportToPosition(player.CharacterEntity, new float3(x, y, z));
    }

    return $"Teleported all players to ({x}, {y}, {z})";
  }

  [RconCommand("teleportall", "Teleport all online players to another player.")]
  public static string TeleportAll(string playerName) {
    if (!PlayerService.TryGetByName(playerName, out var targetPlayer)) {
      return $"Player '{playerName}' was not found.";
    }

    if (!targetPlayer.IsOnline) {
      return $"Target player '{playerName}' is not currently online.";
    }

    var position = targetPlayer.CharacterEntity.Position();

    foreach (var player in PlayerService.AllPlayers) {
      if (!player.IsOnline) continue;

      TeleportService.TeleportToEntity(player.CharacterEntity, targetPlayer.CharacterEntity);
    }

    return $"Teleported all players to ({position.x}, {position.y}, {position.z})";
  }
}