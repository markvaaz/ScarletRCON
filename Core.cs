using System;
using System.Linq;
using Unity.Entities;
using ProjectM.Scripting;
using BepInEx.Logging;
using ProjectM;

namespace ScarletRCON;

internal static class Core {
  public static World Server => GetServerWorld() ?? throw new Exception("There is no Server world (yet)...");
  public static EntityManager EntityManager => Server.EntityManager;
  public static ServerGameManager GameManager => Server.GetExistingSystemManaged<ServerScriptMapper>().GetServerGameManager();
  public static ServerBootstrapSystem ServerBootstrapSystem => Server.GetExistingSystemManaged<ServerBootstrapSystem>();
  public static AdminAuthSystem AdminAuthSystem => Server.GetExistingSystemManaged<AdminAuthSystem>();
  public static KickBanSystem_Server KickBanSystem => Server.GetExistingSystemManaged<KickBanSystem_Server>();
  public static UnitSpawnerUpdateSystem UnitSpawnerUpdateSystem => Server.GetExistingSystemManaged<UnitSpawnerUpdateSystem>();
  public static EntityCommandBufferSystem EntityCommandBufferSystem => Server.GetExistingSystemManaged<EntityCommandBufferSystem>();
  public static DebugEventsSystem DebugEventsSystem => Server.GetExistingSystemManaged<DebugEventsSystem>();
  public static TriggerPersistenceSaveSystem TriggerPersistenceSaveSystem => Server.GetExistingSystemManaged<TriggerPersistenceSaveSystem>();
  public static bool hasInitialized = false;
  public static ManualLogSource Log => Plugin.LogInstance;

  public static void Initialize() {
    if (hasInitialized) return;

    hasInitialized = true;
  }

  static World GetServerWorld() {
    return World.s_AllWorlds.ToArray().FirstOrDefault(world => world.Name == "Server");
  }
}