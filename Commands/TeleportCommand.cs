using ScarletRCON.CommandSystem;
using ScarletRCON.Services;
using ScarletRCON.Systems;
using Unity.Mathematics;

namespace ScarletRCON.Commands;

public static class TeleportCommand {
  [RconCommand("whereis", "Get position of a connected player.")]
  public static string WhereIs(string playerName) {
    if (!PlayerService.TryGetByName(playerName, out var player)) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    var position = player.CharacterEntity.GetPosition();

    return $"- {player.Name}: ({position.x} {position.y} {position.z})\n";
  }

  [RconCommand("teleport", "Teleport a connected player to coordinates.")]
  public static string Teleport(string playerName, float x, float y, float z) {
    if (!PlayerService.TryGetByName(playerName, out var player) || !player.IsOnline) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    TeleportSystem.TeleportToPosition(player.CharacterEntity, new float3(x, y, z));

    return $"Teleported {player.Name} to ({x}, {y}, {z})";
  }

  [RconCommand("teleport", "Teleport a connected player to another connected player.")]
  public static string Teleport(string playerName, string targetPlayerName) {

    if (!PlayerService.TryGetByName(playerName, out var player) || !player.IsOnline) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    if (!PlayerService.TryGetByName(targetPlayerName, out var target) || !target.IsOnline) {
      return $"Player '{targetPlayerName}' was not found or is not connected.";
    }

    TeleportSystem.TeleportToEntity(player.CharacterEntity, target.CharacterEntity);

    return $"Teleported {player.Name} to {target.Name}'s position.";
  }

  [RconCommand("teleportall", "Teleport all connected players to coordinates.")]
  public static string TeleportAll(float x, float y, float z) {
    foreach (var player in PlayerService.AllPlayers) {
      if (!player.IsOnline) continue;

      TeleportSystem.TeleportToPosition(player.CharacterEntity, new float3(x, y, z));
    }

    return $"Teleported all players to ({x}, {y}, {z})";
  }


  [RconCommand("teleportall", "Teleport all connected players to another connected player.")]
  public static string TeleportAll(string playerName) {
    if (!PlayerService.TryGetByName(playerName, out var targetPlayer)) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    var position = targetPlayer.CharacterEntity.GetPosition();

    foreach (var player in PlayerService.AllPlayers) {
      if (!player.IsOnline) continue;

      TeleportSystem.TeleportToEntity(player.CharacterEntity, targetPlayer.CharacterEntity);
    }

    return $"Teleported all players to ({position.x}, {position.y}, {position.z})";
  }
}