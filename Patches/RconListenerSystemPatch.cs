using System;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppSystem.Collections.Generic;
using ProjectM;
using RCONServerLib;
using ScarletRCON.CommandSystem;

namespace ScarletRCON.Patches;

[HarmonyPatch(typeof(RconListenerSystem), nameof(RconListenerSystem.OnCreate))]
class RconInitializePatch {
  static void Postfix(RconListenerSystem __instance) {
    var server = __instance._Server;
    RemoteConServer.CommandEventHandler handler = DelegateSupport.ConvertDelegate<RemoteConServer.CommandEventHandler>(OnCommandReceived);

    server.UseCustomCommandHandler = true;

    CommandHandler.Initialize();

    server.add_OnCommandReceived(handler);
  }

  static IntPtr OnCommandReceived(string cmd, IList<string> args) {
    try {
      // Il2CppSystem.Collections.Generic.IList does not expose Count or enumerator,
      // so I iterate by index until an exception is thrown (out of range).
      // I don't know if this is the best way to do it, but it works.
      var argsResult = new System.Collections.Generic.List<string>();

      int i = 0;

      while (true) {
        try {
          var item = args[i];
          if (item == null) break;
          argsResult.Add(item);
          i++;
        } catch {
          break;
        }
      }

      var response = CommandHandler.HandleCommand(cmd.ToLower(), argsResult);
      return response.ToIntPtr();
    } catch (Exception ex) {
      Console.WriteLine($"RCON command error: {ex}");
      return ex.Message.ToIntPtr();
    }
  }
}

static class StringExtensions {
  public static IntPtr ToIntPtr(this string str) {
    return IL2CPP.ManagedStringToIl2Cpp(str);
  }
}

