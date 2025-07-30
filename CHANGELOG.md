
## Update 1.2.6 & 1.2.7

- Added `shutdown` command to safely shut down the server via RCON.
- Fixed the `serverstats` command to correctly display the count of online players.

## Update 1.2.5

- Fixed InventoryService reference to use the new ScarletCore version.

## Update 1.2.4

- Fixed async command detection.

## Update 1.2.2

- Remove manual `isAsync` parameter from `RconCommandAttribute` 

## Update 1.2.1

- Fix RegisterExternalCommandsBatch ambiguity with automatic async detection

## Update 1.2.0

- Added support for asynchronous commands in CommandHandler.
- Updated RconCommandAttribute to include IsAsync property.
- Modified RconCommandDefinition to store async command information.
- Improved error messages for player-related commands to clarify online status.
- Updated command descriptions for clarity and consistency.
- Refactored command registration and execution logic for better maintainability.

## Update 1.1.7

- Update ConnectedSince property to use DateTime in local time and improve socket retrieval in RconInitializePatch

## Update 1.1.6

- Removed `Destroy_TravelBuffSystem.OnUpdate` patch, as it was causing lag. 4