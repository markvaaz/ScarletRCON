using ProjectM;
using System;
using System.Diagnostics;
using ScarletRCON.CommandSystem;
using ScarletCore.Services;
using System.Reflection;
using Unity.Entities;
using Unity.Collections;
using ScarletCore.Systems;
using System.Threading.Tasks;

namespace ScarletRCON.Commands;

[RconCommandCategory("Server Administration")]
public static class ServerCommands {

  [RconCommand("save", "Save the game")]
  public static string Save() {
    var saveName = "Save_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".save";
    GameSystems.TriggerPersistenceSaveSystem.TriggerSave(SaveReason.ManualSave, saveName, GetServerRuntimeSettings());
    return $"Game saved as '{saveName}'.";
  }

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

  [RconCommand("listadmins", "List all admins.")]
  public static string ListAdmins() {
    var admins = PlayerService.GetAdmins();
    string result = "";
    int count = 0;

    foreach (var player in admins) {
      count++;
      string status = player.IsOnline ? "\u001b[32m●\u001b[0m" : "\u001b[31m●\u001b[0m";
      result += $"- \x1b[97m{player.Name}\u001b[0m \u001b[90m({player.PlatformId})\u001b[0m {status}\n";
    }

    result += $"Total admins: {count}";

    return result;
  }

  [RconCommand("listplayers", "List all players.")]
  public static string ListAllPlayers() {
    string result = "";
    int count = 0;

    foreach (var player in PlayerService.AllPlayers) {
      count++;
      string status = player.IsOnline ? "\u001b[32m●\u001b[0m" : "\u001b[31m●\u001b[0m";
      result += $"- \x1b[97m{player.Name}\u001b[0m \u001b[90m({player.PlatformId})\u001b[0m {status}\n";
    }

    result += $"Total players: {count}";

    return result;
  }

  [RconCommand("listclans", "List all clans.")]
  public static string ListClans() {
    string result = "Clans:\n";
    string white = "\x1b[97m";
    string gray = "\u001b[90m";
    string reset = "\x1b[0m";

    var clans = ClanService.GetClanTeams();

    if (clans.Count == 0) return "No clans found.";

    foreach (var clan in clans) {
      var clanName = clan.Name.ToString();

      result += $"-{white} Clan Name{reset}: {gray}{clanName}{reset}\n";
    }

    return result;
  }

  private static ServerRuntimeSettings GetServerRuntimeSettings() {
    var query = GameSystems.EntityManager.CreateEntityQuery(
      ComponentType.ReadWrite<ServerRuntimeSettings>()
    );

    var settings = query.ToEntityArray(Allocator.Temp)[0].Read<ServerRuntimeSettings>();

    query.Dispose();

    return settings;
  }
}