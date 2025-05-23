using System;
using HarmonyLib;
using ProjectM;
using ProjectM.Gameplay.WarEvents;


namespace ScarletRCON.Patches;

[HarmonyPatch(typeof(SpawnTeamSystem_OnPersistenceLoad), nameof(SpawnTeamSystem_OnPersistenceLoad.OnUpdate))]
public static class InitializationPatch {
  [HarmonyPostfix]
  public static void OneShot_AfterLoad_InitializationPatch() {
    Core.Initialize();
    Plugin.Harmony.Unpatch(typeof(SpawnTeamSystem_OnPersistenceLoad).GetMethod("OnUpdate"), typeof(InitializationPatch).GetMethod("OneShot_AfterLoad_InitializationPatch"));
  }
}