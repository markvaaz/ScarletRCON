using ScarletRCON.CommandSystem;
using ScarletCore.Services;
using Stunlock.Core;

namespace ScarletRCON.Commands;

[RconCommandCategory("Inventory Management")]
public static class InventoryCommands {
  [RconCommand("giveitem", "Give an item to a connected player.")]
  public static string Give(string playerName, string prefabGUID, int amount) {
    if (!PlayerService.TryGetByName(playerName, out var player) || !player.IsOnline) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    if (!PrefabGUID.TryParse(prefabGUID, out var guid)) {
      return "Invalid PrefabGUID.";
    }

    if (InventoryService.IsFull(player.CharacterEntity)) {
      return $"Player '{playerName}' has a full inventory.";
    }

    InventoryService.AddItem(player.CharacterEntity, guid, amount);

    return $"Given {amount} {guid.GuidHash} to {playerName}.";
  }

  [RconCommand("giveitemall", "Give an item to all connected players.")]
  public static string GiveAll(int prefabGUID, int amount) {
    if (!PrefabGUID.TryParse(prefabGUID.ToString(), out var guid)) {
      return "Invalid Prefab GUID.";
    }

    foreach (var player in PlayerService.AllPlayers) {
      if (!player.IsOnline || InventoryService.IsFull(player.CharacterEntity)) continue;
      InventoryService.AddItem(player.CharacterEntity, guid, amount);
    }

    return $"Given {amount} {guid} to all players.";
  }

  [RconCommand("removeitem", "Remove an item from a connected player's inventory.")]
  public static string RemoveItem(string playerName, string prefabGUID, int amount) {
    if (!PlayerService.TryGetByName(playerName, out var player) || !player.IsOnline) {
      return $"Player '{playerName}' was not found or is not connected.";
    }

    if (!PrefabGUID.TryParse(prefabGUID, out var guid)) {
      return "Invalid PrefabGUID.";
    }

    if (!InventoryService.HasAmount(player.CharacterEntity, guid, amount)) {
      return $"Player '{playerName}' does not have {amount} {guid.GuidHash} in their inventory.";
    }

    InventoryService.RemoveItem(player.CharacterEntity, guid, amount);

    return $"Removed {amount} {guid.GuidHash} from {playerName}'s inventory.";
  }

  [RconCommand("removeitemall", "Remove an item from all connected players' inventories.")]
  public static string RemoveItemAll(int prefabGUID, int amount) {
    if (!PrefabGUID.TryParse(prefabGUID.ToString(), out var guid)) {
      return "Invalid Prefab GUID.";
    }

    foreach (var player in PlayerService.AllPlayers) {
      if (!player.IsOnline || !InventoryService.HasAmount(player.CharacterEntity, guid, amount)) continue;
      InventoryService.RemoveItem(player.CharacterEntity, guid, amount);
    }

    return $"Removed {amount} {guid} from all players' inventories.";
  }
}