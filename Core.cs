using System;
using System.Linq;
using Unity.Entities;
using ProjectM.Scripting;
using BepInEx.Logging;
using ProjectM;

namespace ScarletRCON;

internal static class Core {
  public static World Server { get; } = GetServerWorld() ?? throw new Exception("There is no Server world (yet)...");
  public static PrefabCollectionSystem PrefabCollectionSystem => Server.GetExistingSystemManaged<PrefabCollectionSystem>();
  public static EntityManager EntityManager => Server.EntityManager;
  public static ServerGameManager GameManager => Server.GetExistingSystemManaged<ServerScriptMapper>().GetServerGameManager();
  public static SystemHandle BootstrapSystem = Server.GetExistingSystem<ServerBootstrapSystem>();
  public static bool hasInitialized = false;
  public static ManualLogSource Log { get; } = Plugin.LogInstance;

  public static void Initialize() {
    if (hasInitialized) return;

    hasInitialized = true;
  }

  static World GetServerWorld() {
    return World.s_AllWorlds.ToArray().FirstOrDefault(world => world.Name == "Server");
  }
}