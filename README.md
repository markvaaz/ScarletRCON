# ScarletRCON

**ScarletRCON** is a flexible and powerful RCON command framework mod for V Rising that enhances server management by providing a rich set of built-in commands beyond the default RCON capabilities. It serves as a standalone RCON command handler that also offers an easy-to-use API for modders to dynamically create and register custom commands, enabling seamless integration across multiple mods

## What is RCON?

**RCON** (Remote Console) is a protocol used by server administrators to remotely execute commands and manage game servers in real time. For V Rising, RCON allows you to control your server, perform administrative actions, and automate tasks without needing direct access to the server’s hardware.

ScarletRCON builds on the standard RCON functionality by offering a more advanced command framework. It simplifies server management, supports custom commands from other mods, and enables powerful automation and integration—all through a unified RCON interface.

## Scarlet RCON Client

If you need a client to interact with ScarletRCON, check out the [ScarletRCON Client](https://thunderstore.io/c/v-rising/p/ScarletMods/ScarletRCON/wiki/3511-scarlet-rcon-client-optional/). This open-source tool provides a convenient way to send RCON commands to your V Rising server.

Have ideas or feedback? [Leave your suggestion on GitHub](https://github.com/markvaaz/ScarletRCON).

---

## Support & Donations

<a href="https://www.patreon.com/bePatron?u=30093731" data-patreon-widget-type="become-patron-button"><img height='36' style='border:0px;height:36px;' src='https://i.imgur.com/o12xEqi.png' alt='Become a Patron' /></a>  <a href='https://ko-fi.com/F2F21EWEM7' target='_blank'><img height='36' style='border:0px;height:36px;' src='https://storage.ko-fi.com/cdn/kofi6.png?v=6' alt='Buy Me a Coffee at ko-fi.com' /></a>

---

## New Features

- Added save command.
- Added support for defining RCON commands without requiring ScarletRCON as a direct dependency.
- Added command categories for better organization.

## Features

* Provides a simple API to register RCON commands.
* Supports multiple overloads and parameter parsing.
* Handles command invocation safely with detailed error messages.
* Ships with a solid set of built-in default commands covering common server administration tasks.
* Allows other mods to register commands under their own namespace automatically based on assembly name.

## For more information, please visit the [ScarletRCON Wiki on Thunderstore](https://thunderstore.io/c/v-rising/p/ScarletMods/ScarletRCON/wiki/).

## Wiki Index

- [Built-in Commands](https://thunderstore.io/c/v-rising/p/ScarletMods/ScarletRCON/wiki/3498-built-in-commands/)
- [Custom Commands](https://thunderstore.io/c/v-rising/p/ScarletMods/ScarletRCON/wiki/3496-custom-commands/)
- [Installation](https://thunderstore.io/c/v-rising/p/ScarletMods/ScarletRCON/wiki/3500-installation/)
- [Integration](https://thunderstore.io/c/v-rising/p/ScarletMods/ScarletRCON/wiki/3497-integration/)
- [Using RCON](https://thunderstore.io/c/v-rising/p/ScarletMods/ScarletRCON/wiki/3499-using-rcon/)


## Installation

### Requirements

This mod requires the following dependencies:

* **[BepInEx](https://wiki.vrisingmods.com/user/bepinex_install.html)**

Make sure BepInEx is installed and loaded **before** installing ScarletRCON.

### Manual Installation

1. Download the latest release of **ScarletRCON**.

2. Extract the contents into your `BepInEx/plugins` folder:

   `<V Rising Server Directory>/BepInEx/plugins/`

   Your folder should now include:

   `BepInEx/plugins/ScarletRCON.dll`

3. Start or restart your server.
