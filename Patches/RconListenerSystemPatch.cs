using System;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppSystem.Collections.Generic;
using ProjectM;
using RCONServerLib;
using ScarletRCON.CommandSystem;
using Il2CppSystem.Net.Sockets;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace ScarletRCON.Patches;

[HarmonyPatch(typeof(RconListenerSystem), nameof(RconListenerSystem.OnCreate))]
class RconInitializePatch {

  static RemoteConServer server;

  static void Postfix(RconListenerSystem __instance) {
    server = __instance._Server;
    RemoteConServer.CommandEventHandler handler = DelegateSupport.ConvertDelegate<RemoteConServer.CommandEventHandler>(OnCommandReceived);

    server.UseCustomCommandHandler = true;

    CommandHandler.Initialize();

    server.add_OnCommandReceived(handler);

    server.TimeoutSeconds = 900;
    server.MaxPasswordTries = 3;
    server.BanMinutes = 30;
    server.MaxConnections = 1;
  }

  public static Socket GetSocket() {

    foreach (var tcpClient in server._clients) {
      if (!tcpClient.Connected) continue;
      return tcpClient.GetStream()._streamSocket;
    }

    return null;
  }

  static void OnCommandReceived(string cmd, IList<string> args) {
    try {
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

      var fullResponse = CommandHandler.HandleCommand(cmd.ToLower(), argsResult);

      Send(fullResponse);
    } catch (Exception ex) {
      Console.WriteLine($"RCON command error: {ex}");
    }
  }

  public static void Send(string message) {
    var socket = GetSocket();
    if (socket == null) return;

    SendRconPacket(socket, message);
  }

  static void SendRconPacket(Socket socket, string payload) {
    var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);

    int size = 4 + 4 + payloadBytes.Length + 2;

    var packet = new byte[4 + size];

    BitConverter.GetBytes(size).CopyTo(packet, 0);
    BitConverter.GetBytes(0).CopyTo(packet, 4);
    BitConverter.GetBytes(0).CopyTo(packet, 8);

    Array.Copy(payloadBytes, 0, packet, 12, payloadBytes.Length);

    packet[12 + payloadBytes.Length] = 0;
    packet[13 + payloadBytes.Length] = 0;

    var il2cppArray = new Il2CppStructArray<byte>(packet.Length);
    for (int i = 0; i < packet.Length; i++)
      il2cppArray[i] = packet[i];

    socket.Send(il2cppArray, 0, packet.Length, SocketFlags.None);
  }
}

[HarmonyPatch(typeof(RconListenerSystem), nameof(RconListenerSystem.OnUpdate))]
class RconUpdatePatch {
  static void Postfix() {
    if (CommandExecutor.Count == 0) return;
    CommandExecutor.ProcessQueue();
  }
}

static class StringExtensions {
  public static IntPtr ToIntPtr(this string str) {
    return IL2CPP.ManagedStringToIl2Cpp(str);
  }
}

static class Test {
  public static void Testa() {
  }
}