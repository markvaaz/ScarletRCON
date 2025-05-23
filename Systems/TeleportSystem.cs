using ProjectM;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ScarletRCON.Systems;

class TeleportSystem {

  public static void TeleportToEntity(Entity entity, Entity target) {
    var position = target.GetPosition();

    TeleportToPosition(entity, position);
  }

  public static void TeleportToPosition(Entity entity, float3 position) {
    if (entity.Has<SpawnTransform>()) {
      var spawnTransform = entity.Read<SpawnTransform>();
      spawnTransform.Position = position;
      entity.Write(spawnTransform);
    }

    if (entity.Has<Height>()) {
      var height = entity.Read<Height>();
      height.LastPosition = position;
      entity.Write(height);
    }

    if (entity.Has<LocalTransform>()) {
      var localTransform = entity.Read<LocalTransform>();
      localTransform.Position = position;
      entity.Write(localTransform);
    }

    if (entity.Has<Translation>()) {
      var translation = entity.Read<Translation>();
      translation.Value = position;
      entity.Write(translation);
    }

    if (entity.Has<LastTranslation>()) {
      var lastTranslation = entity.Read<LastTranslation>();
      lastTranslation.Value = position;
      entity.Write(lastTranslation);
    }
  }

}