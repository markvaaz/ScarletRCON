using ProjectM;
using ProjectM.Network;
using ScarletRCON.Services;
using Stunlock.Core;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using ScarletRCON.CommandSystem;

namespace ScarletRCON.Commands;

public static class PlayersCommand {
  [RconCommand("playerinfo", "Get information about a player")]
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

  [RconCommand("listadmins", "List all connected admins")]
  public static string ListAdmins() {
    var admins = PlayerService.GetAdmins();
    string result = "";

    foreach (var player in admins) {
      if (!player.IsOnline) continue;
      result += $"- \x1b[97m{player.Name}\u001b[0m \u001b[90m({player.PlatformID})\u001b[0m\n";
    }

    return result;
  }

  [RconCommand("listplayers", "List all connected players")]
  public static string ListAllPlayers() {
    string result = "";

    foreach (var player in PlayerService.AllPlayers) {
      if (!player.IsOnline) continue;
      result += $"- \x1b[97m{player.Name}\u001b[0m \u001b[90m({player.PlatformID})\u001b[0m\n";
    }

    result += $"Total players: {PlayerService.AllPlayers.Count}";

    return result;
  }

  [RconCommand("whereis", "Get the position of a player")]
  public static string WhereIs(string playerName) {
    if (!PlayerService.TryGetByName(playerName, out var player)) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    var position = player.CharacterEntity.GetPosition();

    return $"- {player.Name}: ({position.x}, {position.y}, {position.z})\n";
  }

  [RconCommand("teleport", "Teleport a player to a specific position")]
  public static string Teleport(string playerName, float x, float y, float z) {
    if (!PlayerService.TryGetByName(playerName, out var player) || !player.IsOnline) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    TeleportToPosition(player.CharacterEntity, new float3(x, y, z));

    return $"Teleported {player.Name} to ({x}, {y}, {z})";
  }

  [RconCommand("teleport", "Teleport a player to another player's position")]
  public static string Teleport(string playerName, string targetPlayerName) {

    if (!PlayerService.TryGetByName(playerName, out var player) || !player.IsOnline) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    if (!PlayerService.TryGetByName(targetPlayerName, out var target) || !target.IsOnline) {
      return $"Player '{targetPlayerName}' was not found or is not connected.";
    }

    var position = target.CharacterEntity.GetPosition();

    TeleportToPosition(player.CharacterEntity, position);

    return $"Teleported {player.Name} to {target.Name}'s position.";
  }

  [RconCommand("teleportall", "Teleport all players to a specific position")]
  public static string TeleportAll(float x, float y, float z) {
    foreach (var player in PlayerService.AllPlayers) {
      if (!player.IsOnline) continue;

      TeleportToPosition(player.CharacterEntity, new float3(x, y, z));
    }

    return $"Teleported all players to ({x}, {y}, {z})";
  }


  [RconCommand("teleportall", "Teleport all players to another player's position")]
  public static string TeleportAll(string playerName) {
    if (!PlayerService.TryGetByName(playerName, out var targetPlayer)) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    var position = targetPlayer.CharacterEntity.GetPosition();

    foreach (var player in PlayerService.AllPlayers) {
      if (!player.IsOnline) continue;

      TeleportToPosition(player.CharacterEntity, position);
    }

    return $"Teleported all players to ({position.x}, {position.y}, {position.z})";
  }

  [RconCommand("give", "Give an item to a player")]
  public static string Give(string playerName, string prefabGUID, int amount) {
    if (!PlayerService.TryGetByName(playerName, out var player) || !player.IsOnline) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    if (!PrefabGUID.TryParse(prefabGUID, out var guid)) {
      return "Invalid PrefabGUID.";
    }

    InventoryService.AddItemToInventory(player.CharacterEntity, guid, amount);

    return $"Given {amount} {guid.GuidHash} to {playerName}.";
  }

  [RconCommand("giveall", "Give an item to all players")]
  public static string GiveAll(int prefabGUID, int amount) {
    if (!PrefabGUID.TryParse(prefabGUID.ToString(), out var guid)) {
      return "Invalid Prefab GUID.";
    }

    foreach (var player in PlayerService.AllPlayers) {
      if (!player.IsOnline) continue;
      InventoryService.AddItemToInventory(player.CharacterEntity, guid, amount);
    }

    return $"Given {amount} {guid} to all players.";
  }

  [RconCommand("kick", "Kick a player from the server")]
  public static string Kick(string playerName) {
    if (!PlayerService.TryGetByName(playerName, out var player)) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    var entityEvent = Core.EntityManager.CreateEntity(
      ComponentType.ReadOnly<NetworkEventType>(),
      ComponentType.ReadOnly<KickEvent>()
    );

    Core.EntityManager.SetComponentData(entityEvent, new KickEvent() {
      PlatformId = player.PlatformID
    });

    return $"Kicked {playerName}.";
  }

  public static void TeleportToPosition(Entity entity, float3 position) {
    if (entity.Has<SpawnTransform>()) {
      var spawnTransform = entity.Read<SpawnTransform>();
      spawnTransform.Position = position;
      entity.Write(spawnTransform);
    }

    if (entity.Has<Height>()) {
      var height = entity.Read<Height>();
      height.LastPosition = position;
      entity.Write(height);
    }

    if (entity.Has<LocalTransform>()) {
      var localTransform = entity.Read<LocalTransform>();
      localTransform.Position = position;
      entity.Write(localTransform);
    }

    if (entity.Has<Translation>()) {
      var translation = entity.Read<Translation>();
      translation.Value = position;
      entity.Write(translation);
    }

    if (entity.Has<LastTranslation>()) {
      var lastTranslation = entity.Read<LastTranslation>();
      lastTranslation.Value = position;
      entity.Write(lastTranslation);
    }
  }
}
