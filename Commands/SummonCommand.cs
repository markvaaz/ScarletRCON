using ScarletRCON.CommandSystem;
using ScarletCore.Services;
using Stunlock.Core;
using Unity.Mathematics;
using ProjectM;

namespace ScarletRCON.Commands;

[RconCommandCategory("Summon")]
public static class SummonCommand {
  [RconCommand("summon", "Summon an entity at specific coordinates.")]
  public static string Summon(string prefabGUID, float x, float y, float z, int quantity, int lifeTime, bool disableWhenNoPlayersInRange) {
    if (!PrefabGUID.TryParse(prefabGUID, out var guid)) {
      return "Invalid Prefab GUID.";
    }

    var entities = UnitSpawnerService.ImmediateSpawn(guid, new(x, y, z), count: quantity, lifeTime: lifeTime);

    if (disableWhenNoPlayersInRange) {
      foreach (var entity in entities) {
        if (entity.Has<DisableWhenNoPlayersInRange>()) {
          entity.Remove<DisableWhenNoPlayersInRange>();
        }

        if (entity.Has<DisableWhenNoPlayersInRangeOfChunk>()) {
          entity.Remove<DisableWhenNoPlayersInRangeOfChunk>();
        }
      }
    }

    return $"Summoned {guid.GuidHash} at ({x}, {y}, {z}).";
  }

  [RconCommand("summon", "Summon an entity at a connected player's location.")]
  public static unsafe string Summon(string prefabGUID, string playerName, int quantity, int lifeTime, bool disableWhenNoPlayersInRange) {
    if (!PrefabGUID.TryParse(prefabGUID, out var guid)) {
      return "Invalid Prefab GUID.";
    }

    if (!PlayerService.TryGetByName(playerName, out var player) || !player.IsOnline) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    var entities = UnitSpawnerService.ImmediateSpawn(guid, player.CharacterEntity.Position(), count: quantity, lifeTime: lifeTime);

    if (disableWhenNoPlayersInRange) {
      foreach (var entity in entities) {
        if (entity.Has<DisableWhenNoPlayersInRange>()) {
          entity.Remove<DisableWhenNoPlayersInRange>();
        }

        if (entity.Has<DisableWhenNoPlayersInRangeOfChunk>()) {
          entity.Remove<DisableWhenNoPlayersInRangeOfChunk>();
        }
      }
    }

    return $"Summoned {guid.GuidHash} at {playerName}'s location.";
  }
}