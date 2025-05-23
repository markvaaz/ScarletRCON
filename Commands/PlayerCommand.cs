using ProjectM;
using ScarletRCON.Services;
using ScarletRCON.CommandSystem;
using Stunlock.Core;
using ScarletRCON.Systems;
using ProjectM.Network;
using Unity.Transforms;
using ProjectM.Shared;
using Unity.Mathematics;

namespace ScarletRCON.Commands;

public static class PlayerCommand {
  private static PrefabGUID FreezeBuffGUID = new(-1527408583);
  private static PrefabGUID WoundedBuffGUID = new(-1992158531);
  [RconCommand("playerinfo", "Show info about a connected player.")]
  public static string PlayerInfo(string playerName) {
    if (!PlayerService.TryGetByName(playerName, out var player)) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    var result = $"- Character Name: {player.Name}\n";
    result += $"- Steam ID: {player.PlatformID}\n";
    result += $"- Profile URL: https://steamcommunity.com/profiles/{player.PlatformID}\n";

    if (player.IsOnline) {
      result += $"- Connected Since: {player.ConnectedSince}\n";
    }

    result += $"- {(player.IsOnline ? "Online" : "Offline")}\n";

    return result;
  }

  [RconCommand("kick", "Kick a connected player.")]
  public static string Kick(string playerName) {
    if (!PlayerService.TryGetByName(playerName, out var player)) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    SystemMessages.SendAll($"{player.Name} got ~kicked~ by an admin.");
    KickBanService.Kick(player.PlatformID, player.UserEntity.Index);

    return $"Kicked {playerName}.";
  }

  [RconCommand("ban", " Ban a player by ID.")]
  public static string BanById(ulong playerId) {
    if (KickBanService.IsBanned(playerId)) {
      return $"Player '{playerId}' is already banned.";
    }

    KickBanService.AddBan(playerId);

    return $"Banned {playerId} by ID.";
  }

  [RconCommand("ban", "Ban a connected player by name.")]
  public static string BanByName(string playerName) {
    if (!PlayerService.TryGetByName(playerName, out var player)) {
      return $"Player '{playerName}' was not found or is not connected.\nTry using the ID instead.";
    }

    if (KickBanService.IsBanned(player.PlatformID)) {
      return $"Player '{playerName}' is already banned.";
    }

    SystemMessages.SendAll($"{player.Name} got ~banned~ by an admin.");

    KickBanService.AddBan(player.PlatformID);

    return $"Banned {playerName} by name.";
  }

  [RconCommand("unban", "Unban a player by ID.")]
  public static string Unban(ulong playerId) {
    if (!KickBanService.IsBanned(playerId)) {
      return $"Player '{playerId}' is not banned.";
    }

    KickBanService.RemoveBan(playerId);

    return $"Unbanned {playerId}.";
  }

  [RconCommand("addadmin", "Add a player as admin by ID.")]
  public static string AddAdmin(ulong playerId) {
    if (AdminService.IsAdmin(playerId)) {
      return $"Player '{playerId}' is already an admin.";
    }

    AdminService.AddAdmin(playerId);

    return $"Added {playerId} as admin.";
  }

  [RconCommand("addadmin", "Add a connected player as admin.")]
  public static string AddAdmin(string playerName) {
    if (!PlayerService.TryGetByName(playerName, out var player)) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    if (player.IsAdmin) {
      return $"Player '{playerName}' is already an admin.";
    }

    AdminService.AddAdmin(player.PlatformID);

    return $"Added {playerName} as admin.";
  }

  [RconCommand("removeadmin", "Remove a player as admin by ID.")]
  public static string RemoveAdmin(ulong playerId) {
    if (!AdminService.IsAdmin(playerId)) {
      return $"Player '{playerId}' is not an admin.";
    }

    AdminService.RemoveAdmin(playerId);

    return $"Removed {playerId} as admin.";
  }

  [RconCommand("removeadmin", "Remove a connected player as admin.")]
  public static string RemoveAdmin(string playerName) {
    if (!PlayerService.TryGetByName(playerName, out var player)) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    if (!player.IsAdmin) {
      return $"Player '{playerName}' is not an admin.";
    }

    AdminService.RemoveAdmin(player.PlatformID);

    return $"Removed {playerName} as admin.";
  }

  [RconCommand("buff", "Apply a buff to a connected player.")]
  public static string BuffPlayer(string playerName, string prefabGUID, int duration) {
    if (!PlayerService.TryGetByName(playerName, out var player)) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    if (!PrefabGUID.TryParse(prefabGUID, out var guid)) {
      return "Invalid PrefabGUID.";
    }

    if (BuffUtilitySystem.HasBuff(player.CharacterEntity, guid)) {
      return $"Player {playerName} already has buff {guid.GuidHash}.";
    }

    if (!BuffUtilitySystem.TryApplyBuff(player.CharacterEntity, guid, duration)) {
      return $"Failed to apply buff {prefabGUID} to {playerName}.";
    }

    return $"Applied buff {prefabGUID} to {playerName} for {duration} seconds.";
  }

  [RconCommand("debuff", "Remove a buff from a connected player.")]
  public static string RemoveBuff(string playerName, string prefabGUID) {
    if (!PlayerService.TryGetByName(playerName, out var player)) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    if (!PrefabGUID.TryParse(prefabGUID, out var guid)) {
      return "Invalid PrefabGUID.";
    }

    if (!BuffUtilitySystem.HasBuff(player.CharacterEntity, guid)) {
      return $"Player {playerName} does not have buff {guid.GuidHash}.";
    }

    BuffUtilitySystem.RemoveBuff(player.CharacterEntity, guid);

    return $"Removed buff {prefabGUID} from {playerName}.";
  }

  [RconCommand("freeze", "Freeze a connected player for x seconds.")]
  public static string Freeze(string playerName, int duration) {
    if (!PlayerService.TryGetByName(playerName, out var player)) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    if (BuffUtilitySystem.HasBuff(player.CharacterEntity, FreezeBuffGUID)) {
      return $"Player {playerName} is already frozen.";
    }

    if (!BuffUtilitySystem.TryApplyBuff(player.CharacterEntity, FreezeBuffGUID, duration)) {
      return $"Failed to freeze {playerName}.";
    }

    SystemMessages.Send(player.UserEntity.Read<User>(), $"You’ve been ~frozen~ in place for ~{duration}~ seconds by an admin!");

    return $"Frozen {playerName}.";
  }

  [RconCommand("unfreeze", "Unfreeze a connected player.")]
  public static string Unfreeze(string playerName) {
    if (!PlayerService.TryGetByName(playerName, out var player)) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    if (!BuffUtilitySystem.HasBuff(player.CharacterEntity, FreezeBuffGUID)) {
      return $"Player {playerName} is not frozen.";
    }

    BuffUtilitySystem.RemoveBuff(player.CharacterEntity, FreezeBuffGUID);

    SystemMessages.Send(player.UserEntity.Read<User>(), "You have been ~unfrozen~ by an admin!");

    return $"Unfroze {playerName}.";
  }

  [RconCommand("heal", "Full heal a connected player.", "<playerName>")]
  public static string Heal(string playerName) {
    if (!PlayerService.TryGetByName(playerName, out var player) || !player.IsOnline) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    var character = player.CharacterEntity;

    var health = character.Read<Health>();

    health.Value = health.MaxHealth;
    health.MaxRecoveryHealth = health.MaxHealth;

    character.Write(health);

    SystemMessages.Send(player.UserEntity.Read<User>(), "You have been ~healed~ by an admin!");

    return $"Healed {player.Name}.";
  }

  [RconCommand("wound", "Wound a connected player.", "<playerName>")]
  public static string Wound(string playerName) {
    if (!PlayerService.TryGetByName(playerName, out var player) || !player.IsOnline) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    if (!BuffUtilitySystem.TryApplyBuff(player.CharacterEntity, WoundedBuffGUID)) {
      return $"Failed to wound {playerName}.";
    }

    SystemMessages.Send(player.UserEntity.Read<User>(), "You have been ~wounded~ by an admin!");

    return $"Healed {player.Name}.";
  }

  [RconCommand("kill", "Kill a connected player.", "<playerName>")]
  public static string Kill(string playerName) {
    if (!PlayerService.TryGetByName(playerName, out var player) || !player.IsOnline) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    StatChangeUtility.KillEntity(Core.EntityManager, player.CharacterEntity, player.CharacterEntity, Core.GameManager.ServerTime, StatChangeReason.Default, true);

    SystemMessages.Send(player.UserEntity.Read<User>(), "You have been ~killed~ by an admin!");

    return $"Healed {player.Name}.";
  }

  [RconCommand("revive", "Revive a connected player.", "<playerName>")]
  public static string Revive(string playerName) {
    if (!PlayerService.TryGetByName(playerName, out var player) || !player.IsOnline) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    var character = player.CharacterEntity;

    var health = character.Read<Health>();

    if (BuffUtility.TryGetBuff(Core.EntityManager, character, WoundedBuffGUID, out var buff)) {

      DestroyUtility.Destroy(Core.EntityManager, buff, DestroyDebugReason.TryRemoveBuff);

      health.Value = health.MaxHealth;
      health.MaxRecoveryHealth = health.MaxHealth;

      character.Write(health);
    }

    if (health.IsDead) {
      var pos = character.Read<LocalToWorld>().Position;

      var buffer = Core.EntityCommandBufferSystem.CreateCommandBuffer();
      Core.ServerBootstrapSystem.RespawnCharacter(buffer, player.UserEntity, new() { value = pos }, character);
    }

    SystemMessages.Send(player.UserEntity.Read<User>(), "You've been ~revived~ by an admin");

    return $"Revived {player.Name}.";
  }

  [RconCommand("reviveall", "Revive all connected players.")]
  public static string ReviveAll() {
    foreach (var player in PlayerService.AllPlayers) {
      var character = player.CharacterEntity;

      var health = character.Read<Health>();

      if (BuffUtility.TryGetBuff(Core.EntityManager, character, WoundedBuffGUID, out var buff)) {

        DestroyUtility.Destroy(Core.EntityManager, buff, DestroyDebugReason.TryRemoveBuff);

        health.Value = health.MaxHealth;
        health.MaxRecoveryHealth = health.MaxHealth;

        character.Write(health);
      }

      if (health.IsDead) {
        var pos = character.Read<LocalToWorld>().Position;

        var buffer = Core.EntityCommandBufferSystem.CreateCommandBuffer();
        Core.ServerBootstrapSystem.RespawnCharacter(buffer, player.UserEntity, new() { value = pos }, character);
      }

      SystemMessages.Send(player.UserEntity.Read<User>(), "You've been ~revived~ by an admin");
    }

    return "Revived all players.";
  }

  [RconCommand("reviveradius", "Revive all connected players within a radius.")]
  public static string ReviveRadius(int x, int y, int z, int radius) {
    foreach (var player in PlayerService.AllPlayers) {
      var character = player.CharacterEntity;
      var position = player.CharacterEntity.GetPosition();

      var distance = math.distance(new float3(x, 0, z), new float3(position.x, 0, position.z));

      if (distance > radius) continue;

      var health = character.Read<Health>();

      if (BuffUtility.TryGetBuff(Core.EntityManager, character, WoundedBuffGUID, out var buff)) {

        DestroyUtility.Destroy(Core.EntityManager, buff, DestroyDebugReason.TryRemoveBuff);

        health.Value = health.MaxHealth;
        health.MaxRecoveryHealth = health.MaxHealth;

        character.Write(health);
      }

      if (health.IsDead) {
        var pos = character.Read<LocalToWorld>().Position;

        var buffer = Core.EntityCommandBufferSystem.CreateCommandBuffer();
        Core.ServerBootstrapSystem.RespawnCharacter(buffer, player.UserEntity, new() { value = pos }, character);
      }

      SystemMessages.Send(player.UserEntity.Read<User>(), "You've been ~revived~ by an admin");
    }

    return "Revived all players.";
  }

  [RconCommand("reviveradius", "Revive all connected players within a player's radius.")]
  public static string ReviveRadius(string playerName, int radius) {
    if (!PlayerService.TryGetByName(playerName, out var targetPlayer) || !targetPlayer.IsOnline) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    var targetPos = targetPlayer.CharacterEntity.GetPosition();

    foreach (var player in PlayerService.AllPlayers) {
      var character = player.CharacterEntity;
      var position = player.CharacterEntity.GetPosition();

      var distance = math.distance(new float3(targetPos.x, 0, targetPos.z), new float3(position.x, 0, position.z));

      if (distance > radius) continue;

      var health = character.Read<Health>();

      if (BuffUtility.TryGetBuff(Core.EntityManager, character, WoundedBuffGUID, out var buff)) {

        DestroyUtility.Destroy(Core.EntityManager, buff, DestroyDebugReason.TryRemoveBuff);

        health.Value = health.MaxHealth;
        health.MaxRecoveryHealth = health.MaxHealth;

        character.Write(health);
      }

      if (health.IsDead) {
        var pos = character.Read<LocalToWorld>().Position;

        var buffer = Core.EntityCommandBufferSystem.CreateCommandBuffer();
        Core.ServerBootstrapSystem.RespawnCharacter(buffer, player.UserEntity, new() { value = pos }, character);
      }

      SystemMessages.Send(player.UserEntity.Read<User>(), "You've been ~revived~ by an admin");
    }

    return "Revived all players.";
  }
}
