using ProjectM.Scripting;
using Stunlock.Core;
using Unity.Entities;
using Unity.Mathematics;

namespace ScarletRCON.Systems;

public static class EntitySpawnerSystem {
  private static Entity _entity = new();

  public static void Spawn(PrefabGUID prefabGUID, float3 position, int count = 1, int minRange = 1, int maxRange = 8, float lifeTime = 10f) {
    Core.UnitSpawnerUpdateSystem.SpawnUnit(_entity, prefabGUID, position, count, minRange, maxRange, lifeTime);
  }
}