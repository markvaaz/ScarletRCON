using System;
using ProjectM;
using ProjectM.Network;
using ProjectM.Shared;
using Stunlock.Core;
using Unity.Entities;

namespace ScarletRCON.Systems;

public static class BuffUtilitySystem {
  public static bool TryApplyBuff(Entity entity, PrefabGUID prefabGUID, float duration = -1) {
    try {
      ApplyBuffDebugEvent applyBuffDebugEvent = new() {
        BuffPrefabGUID = prefabGUID,
      };

      FromCharacter fromCharacter = new() {
        Character = entity,
        User = entity
      };

      Core.DebugEventsSystem.ApplyBuff(fromCharacter, applyBuffDebugEvent);

      if (!BuffUtility.TryGetBuff(Core.EntityManager, entity, prefabGUID, out var buff)) {
        return false;
      }

      if (buff.Has<CreateGameplayEventsOnSpawn>()) {
        buff.Remove<CreateGameplayEventsOnSpawn>();
      }

      if (buff.Has<GameplayEventListeners>()) {
        buff.Remove<GameplayEventListeners>();
      }

      if (duration != -1) {
        var lifeTime = buff.Read<LifeTime>();

        lifeTime.Duration = duration;
        lifeTime.EndAction = LifeTimeEndAction.Destroy;

        buff.Write(lifeTime);
      }

      if (duration == -1 && buff.Has<LifeTime>()) {
        var lifetime = buff.Read<LifeTime>();
        lifetime.EndAction = LifeTimeEndAction.None;
        buff.Write(lifetime);
      }

      return true;
    } catch (Exception e) {
      Core.Log.LogError($"An error occurred while applying buff: {e.Message}");
      return false;
    }
  }

  public static bool HasBuff(Entity entity, PrefabGUID prefabGUID) {
    return BuffUtility.HasBuff(Core.EntityManager, entity, prefabGUID);
  }

  public static void RemoveBuff(Entity entity, PrefabGUID prefabGUID) {
    if (BuffUtility.TryGetBuff(Core.EntityManager, entity, prefabGUID, out var buff)) {
      DestroyUtility.Destroy(Core.EntityManager, buff, DestroyDebugReason.TryRemoveBuff);
    }
  }
}