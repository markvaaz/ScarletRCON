using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using ScarletRCON.Utils;

namespace ScarletRCON.Systems;

public static class SystemMessages {
  public static void Send(User user, string message) {
    var messageBytes = new FixedString512Bytes(message.Format());
    ServerChatUtils.SendSystemMessageToClient(Core.EntityManager, user, ref messageBytes);
  }

  public static void SendAll(string message) {
    var messageBytes = new FixedString512Bytes(message.Format());
    ServerChatUtils.SendSystemMessageToAllClients(Core.EntityManager, ref messageBytes);
  }
}