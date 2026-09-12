# BetterPortals

BetterPortals replaces one-to-one portal pairing with a Valheim-styled destination menu. Walk into a named portal, choose another named portal, and travel directly to it.

## Features

- Lists named portals in a centered, prewarmed lightweight menu based on Valheim's native pause-menu styling.
- Centers and stabilizes the mouse cursor while the menu is open, restores normal hidden gameplay capture on close, and supports controller navigation.
- Closes automatically when you leave the source portal or complete a trip.
- Preserves the nearby glow on named portals even when they are not paired by matching tags.
- Allows portal names up to 32 characters.
- Lets each character mark one Home portal per world with matching Valheim-native favorite stars on both sides of its centered name.
- Supports right-click, keyboard H, or controller X to set or clear Home. Home is initially selected when the list opens, so Enter or controller A travels there immediately unless you select another destination first.
- Shows an optional white biome subtitle beneath each portal name, then sorts portals by biome and stable creation order.
- Validates the protocol, source, issued destination set, distance, request lifetime, and rate limit on the server.
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

To set a Home portal, right-click its destination row, highlight it and press **H**, or highlight it with a controller and press **X**. Matching Valheim-native favorite stars appear on both sides of the centered portal name. Repeat the same action to clear Home. By default, a valid Home row is selected and scrolled into view when the list opens; press **Enter** or controller **A** to travel, or explicitly select any other row first to travel there instead. BetterPortals owns Enter while the selector is open, so confirming a destination does not also open chat.

## Notes

- Destination names are read from the current server world. Empty names are omitted.
- A headless dedicated-server start, existing-world load, save path, and server/client package identity have been validated on Valheim 1.0.12. Steam Deck/controller multiplayer validation is still in progress.
- Removing the mod does not require converting portal data because BetterPortals does not store a replacement connection graph.
- Home selection is stored in the character's normal custom data and scoped to the current world. It persists with Valheim's normal character autosave or logout; favoriting does not force an immediate full character/cloud save. Entering the Home portal itself does not erase the assignment.
- Portal creation order is stored in a separate server-side `BetterPortals.portal-order.<worldUID>.tsv` ledger under the BepInEx config directory; the mod does not add fields to portal ZDOs or otherwise modify the world save. Existing portals receive a deterministic initial order because Valheim does not retain their historical creation timestamps. Portals created after ledger initialization retain their actual observed order. Back up or restore that small ledger together with its matching world when exact list ordering matters.
- Portal names default to white. Biome subtitles default to visible beneath portal names at 30% of the name's default size. Subtitle visibility, size, color, Home preference, Home icon size, Home shortcut, control hints, and responsive row-knot spacing are available in the BetterPortals configuration alongside the existing visual controls.

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
