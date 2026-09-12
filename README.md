# BetterPortals

BetterPortals replaces one-to-one portal pairing with a Valheim-styled destination menu. Walk into a named portal, choose another named portal, and travel directly to it.

## Features

- Lists named portals in a lightweight menu based on Valheim's native pause-menu styling.
- Keeps the mouse cursor stable while the menu is open and supports controller navigation.
- Closes automatically when you leave the source portal or complete a trip.
- Preserves the nearby glow on named portals even when they are not paired by matching tags.
- Allows portal names up to 32 characters.
- Validates the source, destination, distance, request lifetime, and rate limit on the server.
- Does not permanently rewire Valheim's portal connections.
- Provides client-local controls for typography, colors, layout, backdrop, hover timing, fade timing, and trigger range. Text outlines are disabled by default.

## Requirements

- Valheim 1.0.12 / network version 40.
- BepInExPack Valheim 5.4.2350.
- The identical BetterPortals version must be installed on the dedicated server and every participating client.

Configuration Manager is optional. If installed, press **F1**, select **BetterPortals**, and use its built-in **Reset to Default** action whenever you want to restore the packaged defaults.

## Installation

Install with r2modman or the Thunderstore Mod Manager. For a dedicated server, install the same package version in the server's BepInEx environment as well.

## Use

1. Give every portal a unique name with Valheim's normal **E** interaction.
2. Walk into a named portal.
3. Choose a destination from the list.
4. Press **Escape** or the controller back button to close the menu without travelling.

## Notes

- Destination names are read from the current server world. Empty names are omitted.
- A headless dedicated-server start, existing-world load, save path, and server/client package identity have been validated on Valheim 1.0.12. Steam Deck/controller multiplayer validation is still in progress.
- Removing the mod does not require converting portal data because BetterPortals does not store a replacement connection graph.

## Building from source

The repository does not include Valheim, Unity, or BepInEx binaries. Install Valheim and BepInExPack Valheim 5.4.2350 legally, then run:

```powershell
dotnet build -c Release
```

The project defaults to Steam's standard Windows Valheim directory. A different installation can be supplied with the `GameDir` and `BepInExDir` MSBuild properties.

The assembly, namespace, plug-in GUID, and output DLL retain the original `FraileyPortalSelector` identifiers for compatibility with existing installations. The public plug-in and package name is BetterPortals.

## License

BetterPortals is released under the MIT License. Copyright (c) 2026 AsherFrailey.

## Credits

Created by the **AsherFrailey** Thunderstore team.
