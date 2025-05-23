using ProjectM;
using ProjectM.Network;
using ScarletRCON.Data;
using Unity.Entities;

namespace ScarletRCON.Services;

public static class AdminService {


  public static void AddAdmin(ulong playerId) {
    if (PlayerService.TryGetById(playerId, out var player)) {
      var entityEvent = Core.EntityManager.CreateEntity(
        ComponentType.ReadWrite<FromCharacter>(),
        ComponentType.ReadWrite<AdminAuthEvent>()
      );

      entityEvent.Write(new FromCharacter() {
        Character = player.CharacterEntity,
        User = player.UserEntity
      });
    }

    Core.AdminAuthSystem._LocalAdminList.Add(playerId);
    Core.AdminAuthSystem._LocalAdminList.Save();
    Core.AdminAuthSystem._LocalAdminList.Refresh();
  }

  public static void RemoveAdmin(ulong playerId) {
    if (PlayerService.TryGetById(playerId, out var player)) {
      if (player.UserEntity.Has<AdminUser>()) {
        player.UserEntity.Remove<AdminUser>();
      }

      player.UserEntity.Write(player.UserEntity.Read<User>());

      var entity = Core.EntityManager.CreateEntity(
        ComponentType.ReadWrite<FromCharacter>(),
        ComponentType.ReadWrite<DeauthAdminEvent>()
      );
      entity.Write(new FromCharacter() {
        Character = player.CharacterEntity,
        User = player.UserEntity
      });
    }

    Core.AdminAuthSystem._LocalAdminList.Remove(playerId);
    Core.AdminAuthSystem._LocalAdminList.Save();
    Core.AdminAuthSystem._LocalAdminList.Refresh();
  }

  public static bool IsAdmin(ulong platformID) {
    return Core.AdminAuthSystem._LocalAdminList.Contains(platformID);
  }
}