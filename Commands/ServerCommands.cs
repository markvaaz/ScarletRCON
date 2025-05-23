using ProjectM;
using System;
using System.Diagnostics;
using ScarletRCON.CommandSystem;
using ScarletRCON.Services;
using System.Reflection;
using Unity.Entities;
using Unity.Collections;

namespace ScarletRCON.Commands;

public static class ServerCommands {
  [RconCommand("serverstats", "Show server statistics.")]
  public static string ServerStats() {
    var players = PlayerService.GetAllConnected();
    var uptime = DateTime.Now - Process.GetCurrentProcess().StartTime;

    string result = $"Server Stats\n";
    var process = Process.GetCurrentProcess();
    result += $"- Players Online: {players.Count}\n";
    result += $"- Uptime: {uptime:hh\\:mm\\:ss}\n";
    result += $"- Memory Usage: {process.WorkingSet64 / (1024 * 1024)} MB\n";

    return result;
  }

  [RconCommand("settings", "Live-updates a server setting. Does not persist after restart at the moment. This is an experimental feature, use at your own risk.")]
  public static string Settings(string settingsPath, string settingsValue) {
    object root = null;

    if (settingsPath.StartsWith("serverhostsettings.", StringComparison.OrdinalIgnoreCase)) {
      root = SettingsManager.ServerHostSettings;
      settingsPath = settingsPath["serverhostsettings.".Length..];
    } else if (settingsPath.StartsWith("servergamesettings.", StringComparison.OrdinalIgnoreCase)) {
      root = SettingsManager.ServerGameSettings;
      settingsPath = settingsPath["servergamesettings.".Length..];
    } else {
      return "Invalid root setting: must start with 'serverhostsettings.' or 'servergamesettings.'";
    }

    var pathParts = settingsPath.Split('.');
    var current = root;
    PropertyInfo property = null;

    for (int i = 0; i < pathParts.Length; i++) {
      var type = current.GetType();

      property = type.GetProperty(pathParts[i], BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

      if (property == null) {
        return $"Invalid property: '{pathParts[i]}' not found in '{type.Name}'";
      }

      if (i < pathParts.Length - 1) {
        current = property.GetValue(current);
        if (current == null) {
          return $"Null reference: '{pathParts[i]}' is null.";
        }
      }
    }

    try {
      var convertedValue = Convert.ChangeType(settingsValue, property.PropertyType);
      property.SetValue(current, convertedValue);
      return $"Setting '{settingsPath}' changed to '{settingsValue}'";
    } catch (Exception ex) {
      return $"Failed to set property: {ex.Message}";
    }
  }

  [RconCommand("settings", "Get the value of a server setting.")]
  public static string GetSettings(string settingsPath) {
    object root;

    if (settingsPath.StartsWith("serverhostsettings.", StringComparison.OrdinalIgnoreCase)) {
      root = SettingsManager.ServerHostSettings;
      settingsPath = settingsPath["serverhostsettings.".Length..];
    } else if (settingsPath.StartsWith("servergamesettings.", StringComparison.OrdinalIgnoreCase)) {
      root = SettingsManager.ServerGameSettings;
      settingsPath = settingsPath["servergamesettings.".Length..];
    } else {
      return "Invalid root setting: must start with 'serverhostsettings.' or 'servergamesettings.'";
    }

    var pathParts = settingsPath.Split('.');
    var current = root;

    for (int i = 0; i < pathParts.Length; i++) {
      var type = current.GetType();
      PropertyInfo property = type.GetProperty(pathParts[i], BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

      if (property == null) {
        return $"Invalid property: '{pathParts[i]}' not found in '{type.Name}'";
      }

      current = property.GetValue(current);
      if (current == null) {
        return $"Null reference: '{pathParts[i]}' is null.";
      }
    }

    return $"Setting '{settingsPath}' is '{current}'";
  }


  [RconCommand("listadmins", "List all connected admins.")]
  public static string ListAdmins() {
    var admins = PlayerService.GetAdmins();
    string result = "";
    int count = 0;

    foreach (var player in admins) {
      if (!player.IsOnline) continue;
      count++;
      result += $"- \x1b[97m{player.Name}\u001b[0m \u001b[90m({player.PlatformID})\u001b[0m\n";
    }

    result += $"Total connected admins: {count}";

    return result;
  }

  [RconCommand("listplayers", "List all connected players.")]
  public static string ListAllPlayers() {
    string result = "";
    int count = 0;

    foreach (var player in PlayerService.AllPlayers) {
      if (!player.IsOnline) continue;
      count++;
      result += $"- \x1b[97m{player.Name}\u001b[0m \u001b[90m({player.PlatformID})\u001b[0m\n";
    }

    result += $"Total connected players: {count}";

    return result;
  }

  [RconCommand("listclans", "List all clans.")]
  public static string ListClans() {
    var entityQuery = Core.EntityManager.CreateEntityQuery(
      ComponentType.ReadOnly<ClanTeam>()
    ).ToEntityArray(Allocator.TempJob);

    if (entityQuery.Length == 0) return "No clans found.";

    string result = "Clans:\n";
    string white = "\x1b[97m";
    string gray = "\u001b[90m";
    string reset = "\x1b[0m";

    foreach (var entity in entityQuery) {
      if (!entity.Has<ClanTeam>()) continue;

      var clan = entity.Read<ClanTeam>();
      var clanName = clan.Name.ToString();

      result += $"-{white} Clan Name{reset}: {gray}{clanName}{reset}\n";
    }

    result += $"Total clans: {entityQuery.Length}";

    entityQuery.Dispose();

    return result;
  }
}