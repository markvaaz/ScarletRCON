# ScarletRCON

ScarletRCON is a flexible RCON command framework mod for *V Rising* that enables easy creation and management of server commands. It works as a standalone RCON command handler which other mods can integrate with to register custom commands dynamically.

## Features

* Provides a simple API to register RCON commands.
* Supports multiple overloads and parameter parsing.
* Handles command invocation safely with detailed error messages.
* Ships with a solid set of built-in default commands covering common server administration tasks.
* Allows other mods to register commands under their own namespace automatically based on assembly name.

## Usage

You can use ScarletRCON to register your own RCON commands in your mod by defining static methods annotated with `[RconCommand("commandName")]` and implementing the logic you need. The framework handles parsing arguments, invoking your methods, and returning command output.

Commands can have multiple overloads and support optional trailing parameters of type `List<string>` for variable-length argument lists.

### Command Namespacing

When you register a command from another mod, it is automatically prefixed using the mod's assembly name in lowercase.

For example, if your mod is called `TestMod` and you define:

```csharp
[RconCommand("hello")]
public static void HelloCommand(string name) {
    // logic here
}
```

The final command name becomes:

`testmod.hello`

This avoids name conflicts and helps keep commands organized per mod.

If the command is defined within ScarletRCON itself, no prefix is added (e.g., just `help`, `give`, etc.).

### Registering Commands

Inside your mod, you only need to call this during initialization:

```csharp
CommandHandler.RegisterAll();
```

This will register all `[RconCommand]` methods from your assembly automatically.

## For more information, please visit the [ScarletRCON Wiki on Thunderstore](https://thunderstore.io/c/v-rising/p/ScarletMods/ScarletRCON/wiki/).

# Default Commands

The following commands come built-in with ScarletRCON and are ready to use:

* `announce <message>`
  Sends a message to all players connected to the server.

* `announcerestart <minutes>`
  Sends a pre-configured message that announces a server restart in the specified number of minutes.

* `help <commandName>`
  Displays detailed information about a specific command.

* `help`
  Lists all available commands when called without parameters.

* `playerinfo <playerName>`
  Displays information about the specified player.

* `listadmins`
  Lists all connected admins.

* `listplayers`
  Lists all connected players.

* `whereis <playerName>`
  Shows the location coordinates of the specified player.

* `teleport <playerName> <x> <y> <z>`
  Teleports the specified player to the given coordinates.

* `teleport <playerName> <targetPlayerName>`
  Teleports the specified player to another player.

* `teleportall <x> <y> <z>`
  Teleports all connected players to the given coordinates.

* `teleportall <playerName>`
  Teleports all connected players to the specified player.

* `give <playerName> <prefabGUID> <amount>`
  Gives the specified amount of an item (identified by prefabGUID) to the player.

* `giveall <prefabGUID> <amount>`
  Gives the specified amount of an item to all players.

* `kick <playerName>`
  Kicks the specified player from the server.

## Installation

### Requirements

This mod requires the following dependencies:

* **[BepInEx (RC2)](https://wiki.vrisingmods.com/user/bepinex_install.html)**

Make sure BepInEx is installed and loaded **before** installing ScarletRCON.

### Manual Installation

1. Download the latest release of **ScarletRCON**.

2. Extract the contents into your `BepInEx/plugins` folder:

   `<V Rising Server Directory>/BepInEx/plugins/`

   Your folder should now include:

   `BepInEx/plugins/ScarletRCON.dll`

3. Start or restart your server.
