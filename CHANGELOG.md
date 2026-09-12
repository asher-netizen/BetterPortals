# Changelog

## 0.4.7

- Restricted creation-order observation to Valheim's recognized portal prefabs. Ordinary objects received from players no longer initialize, enter, or trigger writes to the portal ledger.
- Preserved the tested 0.4.6 selector appearance, controls, detection range, and responsiveness unchanged.

## 0.4.6

- Restored native menu sizing after prewarming by using Valheim's live GuiScaler, including resolution changes and the player's GUI Scale setting.
- Disabled the copied Continue button's internal layout driver and excluded decorative children from layout, keeping portal titles centered under the heading.
- Preserved the prewarmed menu, cached asset lookups and highlighted-row-only text measurement that removed the entry hitch.
- Increased the default portal detection range by another 25% (1.25 to 1.5625), migrating the old default and increasing the server distance limit proportionally.
- Removed the full character/map/cloud save from favoriting clicks. Home changes persist with normal character saves, and only changed Home markers rebuild their appearance.

## 0.4.5

- Removed the portal-entry hitch introduced in 0.4.4 by prebuilding the static selector shell, caching native Home/cursor assets, and measuring text only for the highlighted row instead of every destination on every frame.
- Corrected the remaining horizontal list drift by centering layout-controlled row anchors, and changed the default panel offset from 120 to 0 so the complete selector is vertically centered.
- Mirrored the right native knot so the left and right hover arrows face opposite directions.
- Blocked the actual `Chat.InputText` open path while BetterPortals owns input, preventing Enter from reopening chat regardless of component update order.

## 0.4.4

- Centered the complete selector and every destination row by normalizing the cloned native button geometry and making all rows use the same full-width layout.
- Rebuilt each row's hover ornaments as an isolated, normalized left/right pair, including the first row and repeated selector openings.
- Made portal names white by default, reduced biome subtitles by 40%, and reduced the Home marker by 40%.
- Replaced the single Home marker with matching native favorite stars on both sides of the portal name so the title remains centered.
- Centered the mouse cursor when the selector opens and explicitly returns it to hidden gameplay capture on walk-away, Escape, selection, and every other close path.
- Made the first unmodified Enter/controller-confirm action prefer the saved Home row while preserving explicit mouse or controller destination selection.
- Removed the top-left Home-set and Home-cleared notifications.

## 0.4.3

- Fixed the unequal native knot-image bounds that put one selection arrow far from the row while the other overlapped its name; the two knots now anchor by their inner edges around each row's rendered title.
- Removed the orange biome group headings and moved each biome into a centered white subtitle beneath its portal name at half the default name size.
- Replaced the Home bed marker with Valheim's native build-menu favorite-star icon and a text-star fallback.
- Prevented Enter from opening Valheim chat while the portal selector owns input.
- Confirmed Home remains local to the player's character and scoped to the current world rather than being shared with other players.

## 0.4.2

- Fixed private-world and listen-server portal directory and travel requests by validating the local host player when the routed request originates from the server's own UID.
- Made each row calculate knot spacing from only its own active Home marker, portal name, biome subtitle, and spacing rather than inheriting the widest sibling layout.
- Kept knot placement stable across closing and reopening the selector by removing the dependency on transient shared layout measurements.

## 0.4.1

- Anchored native row knots just outside the complete rendered destination label instead of inheriting fixed positions from Valheim's Continue-button prefab.
- Made knot placement follow the eased hover scale and adapt to portal-name length, biome text, the Home marker, display resolution, and UI scale.
- Added a configurable `Row Knot Gap` visual setting.

## 0.4.0

- Added a saved per-character, per-world Home portal. Right-click, press keyboard H, or press controller X on a selected row to set or clear it; Valheim's native Bed icon marks the row.
- Added Home-first focus and scrolling. Enter and controller A share the normal confirm path, so they travel Home immediately on open or travel another row after the player changes selection.
- Added optional biome group headings and smaller biome subtitles, sorted by Meadows, Black Forest, Swamp, Mountains, Plains, Mistlands, Ashlands, Deep North, Ocean, then Other.
- Added a server-side BetterPortals ledger that gives legacy portals a stable bootstrap order and preserves the creation order of portals added afterward without modifying the Valheim world save.
- Added configuration for group headings, biome subtitles, Home-first focus, adaptive control hints, keyboard Home shortcut, biome typography, and Home icon size.
- Added an explicit v2 protocol field and versioned RPC names. The server only authorizes destinations issued in that selector session, bounds directory records, revalidates current portal data, and rejects malformed or mixed-version traffic without changing client state.

## 0.3.3

- Disabled selector text outlines by default while keeping outline color and width configurable.
- Added the MIT License for the first public release.
- Retained the bounded first-entry readiness retry introduced in 0.3.2.
- Retained eased hover expansion and contraction, fast fades, stable cursor capture, walk-away closing, native-style rows and decorations, and the 1.25x portal trigger range.

## 0.3.2

- Added comprehensive client-local visual configuration.
- Added eased hover animation and an initialization retry for the first portal entry after joining a world.

## 0.3.1

- Reworked the selector around Valheim's native pause-menu rows and decorations.
- Improved cursor, input, fade, and trigger-exit behavior.

## 0.3.0

- Fixed TextMesh Pro font/material initialization.
- Restored Valheim's normal E-to-rename interaction with a 32-character limit.

## 0.1.0

- Initial private test build.
