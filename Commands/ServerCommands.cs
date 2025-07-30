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
using UnityEngine;

namespace ScarletRCON.Commands;

[RconCommandCategory("Server Administration")]
public static class ServerCommands {
  [RconCommand("shutdown", "Shuts down the server gracefully.")]
  public static string Shutdown() {
    if (!GameSystems.Initialized) {
      return "Game systems not initialized. Cannot shutdown.";
    }

    GameSystems.TriggerPersistenceSaveSystem.TriggerAutoSave(GetServerRuntimeSettings());
    ShutdownAsync();

    string white = "\x1b[97m";
    string gray = "\u001b[90m";
    string reset = "\x1b[0m";

    return $"{white}Game saved successfully!{reset}\n" +
           $"{gray}Server will shut down in 3 seconds...{reset}\n" +
           $"{gray}Server context window will close in 10 seconds.{reset}";
  }


  private static async void ShutdownAsync() {
    await Task.Delay(TimeSpan.FromSeconds(3));
    Application.Quit();
    await Task.Delay(TimeSpan.FromSeconds(7));
    try {
      Process.GetCurrentProcess().Kill();
    } catch { }
  }

  [RconCommand("save", "Save the game")]
  public static string Save() {
    GameSystems.TriggerPersistenceSaveSystem.TriggerAutoSave(GetServerRuntimeSettings());
    return $"Game saved'.";
  }

  [RconCommand("serverstats", "Show server statistics.")]
  public static string ServerStats() {
    var players = PlayerService.AllPlayers;
    var count = 0;

    foreach (var player in players) {
      if (player.IsOnline) count++;
    }

    var uptime = DateTime.Now - Process.GetCurrentProcess().StartTime;

    string result = $"Server Stats\n";
    var process = Process.GetCurrentProcess();
    result += $"- Players Online: {count}\n";
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

  [RconCommand("listplayers", "List online players.")]
  public static string ListOnlinePlayers() {
    string result = "";
    int count = 0;

    foreach (var player in PlayerService.AllPlayers) {
      if (!player.IsOnline) continue;
      count++;
      string status = player.IsOnline ? "\u001b[32m●\u001b[0m" : "\u001b[31m●\u001b[0m";
      result += $"- \x1b[97m{player.Name}\u001b[0m \u001b[90m({player.PlatformId})\u001b[0m {status}\n";
    }

    if (count == 0) {
      return "No players online.";
    } else {
      result += $"Total online players: {count}";
    }

    return result;
  }

  [RconCommand("listallplayers", "List all players.")]
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