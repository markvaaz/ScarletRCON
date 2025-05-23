using ProjectM.Network;
using Unity.Entities;

namespace ScarletRCON.Services;

public static class KickBanService {
  public static void AddBan(ulong platformID) {
    if (PlayerService.TryGetById(platformID, out var player) || player.IsOnline) {
      var entityEvent = Core.EntityManager.CreateEntity(
        ComponentType.ReadWrite<FromCharacter>(),
        ComponentType.ReadWrite<BanEvent>()
      );

      entityEvent.Write(new FromCharacter() {
        Character = player.CharacterEntity,
        User = player.UserEntity
      });
    }

    Core.KickBanSystem._LocalBanList.Add(platformID);
    Core.KickBanSystem._LocalBanList.Save();
    Core.KickBanSystem._LocalBanList.Refresh();
  }

  public static void RemoveBan(ulong platformID) {
    Core.KickBanSystem._LocalBanList.Remove(platformID);
    Core.KickBanSystem._LocalBanList.Save();
    Core.KickBanSystem._LocalBanList.Refresh();
  }

  public static bool IsBanned(ulong platformID) {
    return Core.KickBanSystem.IsBanned(platformID);
  }

  public static void Kick(ulong platformID, int userIndex) {
    Entity eventEntity = Core.EntityManager.CreateEntity([
      ComponentType.ReadOnly<NetworkEventType>(),
      ComponentType.ReadOnly<SendEventToUser>(),
      ComponentType.ReadOnly<KickEvent>()
    ]);

    eventEntity.Write(new KickEvent {
      PlatformId = platformID
    });

    eventEntity.Write(new NetworkEventType {
      EventId = NetworkEvents.EventId_KickEvent,
      IsAdminEvent = false,
      IsDebugEvent = false
    });
  }
}