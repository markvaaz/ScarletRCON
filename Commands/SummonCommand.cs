using ScarletRCON.CommandSystem;
using ScarletRCON.Services;
using Stunlock.Core;
using ScarletRCON.Systems;
using Unity.Mathematics;

namespace ScarletRCON.Commands;

public static class SummonCommand {
  [RconCommand("summon", "Summon an entity at specific coordinates.")]
  public static string Summon(string prefabGUID, float x, float y, float z, int quantity, int lifeTime) {
    if (!PrefabGUID.TryParse(prefabGUID, out var guid)) {
      return "Invalid Prefab GUID.";
    }

    EntitySpawnerSystem.Spawn(guid, new float3(x, y, z), count: quantity, lifeTime: lifeTime);

    return $"Summoned {guid.GuidHash} at ({x}, {y}, {z}).";
  }

  [RconCommand("summon", "Summon an entity at a connected player's location.")]
  public static unsafe string Summon(string prefabGUID, string playerName, int quantity, int lifeTime) {
    if (!PrefabGUID.TryParse(prefabGUID, out var guid)) {
      return "Invalid Prefab GUID.";
    }

    if (!PlayerService.TryGetByName(playerName, out var player) || !player.IsOnline) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    EntitySpawnerSystem.Spawn(guid, player.CharacterEntity.GetPosition(), count: quantity, lifeTime: lifeTime);

    return $"Summoned {guid.GuidHash} at {playerName}'s location.";
  }
}