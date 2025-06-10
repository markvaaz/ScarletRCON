using ProjectM;
using ScarletRCON.CommandSystem;
using Stunlock.Core;
using Unity.Transforms;
using ProjectM.Shared;
using Unity.Mathematics;
using ScarletCore.Services;
using ScarletCore.Systems;

namespace ScarletRCON.Commands;

[RconCommandCategory("Player Administration")]
public static class PlayerCommand {
  private static PrefabGUID FreezeBuffGUID = new(-1527408583);
  private static PrefabGUID WoundedBuffGUID = new(-1992158531);

  [RconCommand("playerinfo", "Show info about a connected player.")]
  public static string PlayerInfo(string playerName) {
    if (!PlayerService.TryGetByName(playerName, out var player)) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    var result = $"- Character Name: {player.Name}\n";
    result += $"- Steam ID: {player.PlatformId}\n";
    result += $"- Profile URL: https://steamcommunity.com/profiles/{player.PlatformId}\n";

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

    MessageService.SendAll($"{player.Name} got ~kicked~ by an admin.");
    KickBanService.Kick(player.PlatformId);

    return $"Kicked {playerName}.";
  }

  [RconCommand("ban", " Ban a player by ID.")]
  public static string BanById(ulong playerId) {
    if (KickBanService.IsBanned(playerId)) {
      return $"Player '{playerId}' is already banned.";
    }

    KickBanService.Ban(playerId);

    return $"Banned {playerId} by ID.";
  }

  [RconCommand("ban", "Ban a connected player by name.")]
  public static string BanByName(string playerName) {
    if (!PlayerService.TryGetByName(playerName, out var player)) {
      return $"Player '{playerName}' was not found or is not connected.\nTry using the ID instead.";
    }

    if (KickBanService.IsBanned(player.PlatformId)) {
      return $"Player '{playerName}' is already banned.";
    }

    MessageService.SendAll($"{player.Name} got ~banned~ by an admin.");

    KickBanService.Ban(player.PlatformId);

    return $"Banned {playerName} by name.";
  }

  [RconCommand("unban", "Unban a player by ID.")]
  public static string Unban(ulong playerId) {
    if (!KickBanService.IsBanned(playerId)) {
      return $"Player '{playerId}' is not banned.";
    }

    KickBanService.Unban(playerId);

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

    AdminService.AddAdmin(player.PlatformId);

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

    AdminService.RemoveAdmin(player.PlatformId);

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

    if (BuffService.HasBuff(player.CharacterEntity, guid)) {
      return $"Player {playerName} already has buff {guid.GuidHash}.";
    }

    if (!BuffService.TryApplyBuff(player.CharacterEntity, guid, duration)) {
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

    if (!BuffService.HasBuff(player.CharacterEntity, guid)) {
      return $"Player {playerName} does not have buff {guid.GuidHash}.";
    }

    BuffService.TryRemoveBuff(player.CharacterEntity, guid);

    return $"Removed buff {prefabGUID} from {playerName}.";
  }

  [RconCommand("freeze", "Freeze a connected player for x seconds.")]
  public static string Freeze(string playerName, int duration) {
    if (!PlayerService.TryGetByName(playerName, out var player)) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    if (BuffService.HasBuff(player.CharacterEntity, FreezeBuffGUID)) {
      return $"Player {playerName} is already frozen.";
    }

    if (!BuffService.TryApplyBuff(player.CharacterEntity, FreezeBuffGUID, duration)) {
      return $"Failed to freeze {playerName}.";
    }

    MessageService.Send(player.User, $"You’ve been ~frozen~ in place for ~{duration}~ seconds by an admin!");

    return $"Frozen {playerName}.";
  }

  [RconCommand("unfreeze", "Unfreeze a connected player.")]
  public static string Unfreeze(string playerName) {
    if (!PlayerService.TryGetByName(playerName, out var player)) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    if (!BuffService.HasBuff(player.CharacterEntity, FreezeBuffGUID)) {
      return $"Player {playerName} is not frozen.";
    }

    BuffService.TryRemoveBuff(player.CharacterEntity, FreezeBuffGUID);

    MessageService.Send(player.User, "You have been ~unfrozen~ by an admin!");

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

    MessageService.Send(player.User, "You have been ~healed~ by an admin!");

    return $"Healed {player.Name}.";
  }

  [RconCommand("wound", "Wound a connected player.", "<playerName>")]
  public static string Wound(string playerName) {
    if (!PlayerService.TryGetByName(playerName, out var player) || !player.IsOnline) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    if (!BuffService.TryApplyBuff(player.CharacterEntity, WoundedBuffGUID)) {
      return $"Failed to wound {playerName}.";
    }

    MessageService.Send(player.User, "You have been ~wounded~ by an admin!");

    return $"Healed {player.Name}.";
  }

  [RconCommand("kill", "Kill a connected player.", "<playerName>")]
  public static string Kill(string playerName) {
    if (!PlayerService.TryGetByName(playerName, out var player) || !player.IsOnline) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    StatChangeUtility.KillEntity(GameSystems.EntityManager, player.CharacterEntity, player.CharacterEntity, GameSystems.ServerGameManager.ServerTime, StatChangeReason.Default, true);

    MessageService.Send(player.User, "You have been ~killed~ by an admin!");

    return $"Healed {player.Name}.";
  }

  [RconCommand("revive", "Revive a connected player.", "<playerName>")]
  public static string Revive(string playerName) {
    if (!PlayerService.TryGetByName(playerName, out var player) || !player.IsOnline) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    var character = player.CharacterEntity;

    var health = character.Read<Health>();

    if (BuffUtility.TryGetBuff(GameSystems.EntityManager, character, WoundedBuffGUID, out var buff)) {

      DestroyUtility.Destroy(GameSystems.EntityManager, buff, DestroyDebugReason.TryRemoveBuff);

      health.Value = health.MaxHealth;
      health.MaxRecoveryHealth = health.MaxHealth;

      character.Write(health);
    }

    if (health.IsDead) {
      var pos = character.Read<LocalToWorld>().Position;

      var buffer = GameSystems.EntityCommandBufferSystem.CreateCommandBuffer();
      GameSystems.ServerBootstrapSystem.RespawnCharacter(buffer, player.UserEntity, new() { value = pos }, character);
    }

    MessageService.Send(player.User, "You've been ~revived~ by an admin");

    return $"Revived {player.Name}.";
  }

  [RconCommand("reviveall", "Revive all connected players.")]
  public static string ReviveAll() {
    foreach (var player in PlayerService.AllPlayers) {
      var character = player.CharacterEntity;

      var health = character.Read<Health>();

      if (BuffUtility.TryGetBuff(GameSystems.EntityManager, character, WoundedBuffGUID, out var buff)) {

        DestroyUtility.Destroy(GameSystems.EntityManager, buff, DestroyDebugReason.TryRemoveBuff);

        health.Value = health.MaxHealth;
        health.MaxRecoveryHealth = health.MaxHealth;

        character.Write(health);
      }

      if (health.IsDead) {
        var pos = character.Read<LocalToWorld>().Position;

        var buffer = GameSystems.EntityCommandBufferSystem.CreateCommandBuffer();
        GameSystems.ServerBootstrapSystem.RespawnCharacter(buffer, player.UserEntity, new() { value = pos }, character);
      }

      MessageService.Send(player.User, "You've been ~revived~ by an admin");
    }

    return "Revived all players.";
  }

  [RconCommand("reviveradius", "Revive all connected players within a radius.")]
  public static string ReviveRadius(int x, int y, int z, int radius) {
    foreach (var player in PlayerService.AllPlayers) {
      var character = player.CharacterEntity;
      var position = player.CharacterEntity.Position();

      var distance = math.distance(new float3(x, 0, z), new float3(position.x, 0, position.z));

      if (distance > radius) continue;

      var health = character.Read<Health>();

      if (BuffUtility.TryGetBuff(GameSystems.EntityManager, character, WoundedBuffGUID, out var buff)) {

        DestroyUtility.Destroy(GameSystems.EntityManager, buff, DestroyDebugReason.TryRemoveBuff);

        health.Value = health.MaxHealth;
        health.MaxRecoveryHealth = health.MaxHealth;

        character.Write(health);
      }

      if (health.IsDead) {
        var pos = character.Read<LocalToWorld>().Position;

        var buffer = GameSystems.EntityCommandBufferSystem.CreateCommandBuffer();
        GameSystems.ServerBootstrapSystem.RespawnCharacter(buffer, player.UserEntity, new() { value = pos }, character);
      }

      MessageService.Send(player.User, "You've been ~revived~ by an admin");
    }

    return "Revived all players.";
  }

  [RconCommand("reviveradius", "Revive all connected players within a player's radius.")]
  public static string ReviveRadius(string playerName, int radius) {
    if (!PlayerService.TryGetByName(playerName, out var targetPlayer) || !targetPlayer.IsOnline) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    var targetPos = targetPlayer.CharacterEntity.Position();

    foreach (var player in PlayerService.AllPlayers) {
      var character = player.CharacterEntity;
      var position = player.CharacterEntity.Position();

      var distance = math.distance(new float3(targetPos.x, 0, targetPos.z), new float3(position.x, 0, position.z));

      if (distance > radius) continue;

      var health = character.Read<Health>();

      if (BuffUtility.TryGetBuff(GameSystems.EntityManager, character, WoundedBuffGUID, out var buff)) {

        DestroyUtility.Destroy(GameSystems.EntityManager, buff, DestroyDebugReason.TryRemoveBuff);

        health.Value = health.MaxHealth;
        health.MaxRecoveryHealth = health.MaxHealth;

        character.Write(health);
      }

      if (health.IsDead) {
        var pos = character.Read<LocalToWorld>().Position;

        var buffer = GameSystems.EntityCommandBufferSystem.CreateCommandBuffer();
        GameSystems.ServerBootstrapSystem.RespawnCharacter(buffer, player.UserEntity, new() { value = pos }, character);
      }

      MessageService.Send(player.User, "You've been ~revived~ by an admin");
    }

    return "Revived all players.";
  }

  /*



    MAP REVEAL COMMANDS



  */

  [RconCommand("revealmap", "Reveal the map for a connected player.")]
  public static string RevealMap(string playerName) {
    if (!PlayerService.TryGetByName(playerName, out var player) || !player.IsOnline) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    RevealMapService.RevealFullMap(player);

    MessageService.Send(player.User, "Your map has been ~revealed~ by an admin!");

    return $"Revealed map for {player.Name}.";
  }

  [RconCommand("revealmapradius", "Reveal the map within a radius of specific coordinates for a connected player.")]
  public static string RevealMapRadius(string playerName, float x, float z, float radius) {
    if (!PlayerService.TryGetByName(playerName, out var player) || !player.IsOnline) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    if (radius <= 0) {
      return "Radius must be greater than 0.";
    }

    RevealMapService.RevealMapRadius(player, new(x, 0, z), radius);

    return $"Revealed map for {player.Name} within radius {radius} around ({x}, {z}).";
  }

  [RconCommand("revealmapradius", "Reveal the map within a radius around another player.")]
  public static string RevealMapRadius(string playerName, string targetPlayerName, float radius) {
    if (!PlayerService.TryGetByName(playerName, out var player) || !player.IsOnline) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    if (!PlayerService.TryGetByName(targetPlayerName, out var centerPlayer) || !centerPlayer.IsOnline) {
      return $"Center player '{targetPlayerName}' was not found or is not connected.";
    }

    if (radius <= 0) {
      return "Radius must be greater than 0.";
    }

    RevealMapService.RevealMapRadius(player, centerPlayer.CharacterEntity.Position(), radius);

    return $"Revealed map for {player.Name} within radius {radius} around {targetPlayerName}.";
  }

  [RconCommand("hidemap", "Hide/obscure the map for a connected player.")]
  public static string HideMap(string playerName) {
    if (!PlayerService.TryGetByName(playerName, out var player) || !player.IsOnline) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    RevealMapService.HideFullMap(player);

    MessageService.Send(player.User, "Your map has been ~hidden~ by an admin!");

    return $"Hidden map for {player.Name}.";
  }
}