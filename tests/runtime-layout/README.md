# Headless layout regression probe

This diagnostic assembly exercises the production selector against Valheim's real Menu prefab, TextMesh Pro and Unity layout engine in an isolated dedicated-server runtime. It is not shipped with BetterPortals.

Build `BetterPortalsLayoutProbe.csproj` with `dotnet build -c Release`. Override `RuntimeDir` if the disposable runtime is elsewhere. The main BetterPortals project must exclude `tests/**/*.cs`.

Install the resulting probe DLL alongside the exact candidate BetterPortals DLL in the disposable runtime only. Set `BETTERPORTALS_LAYOUT_PROBE=1` and optionally `BETTERPORTALS_LAYOUT_REPORT` to an absolute report path before the owner agent starts the headless server. Without both batch mode and explicit opt-in, the probe does nothing. Remove the diagnostic DLL after shutdown.

The probe waits for the native Menu template, creates a disabled PortalController, and invokes only selector UI construction/settings methods. It never invokes portal entry, directory/travel RPC, character creation, teleportation or world edits. It compares actual post-layout heading, name, label-group and panel centers; toggles Home on short and long names; reopens the list repeatedly; exercises hover scaling; and checks native GuiScaler/canvas scale agreement. It emits `RESULT PASS` or `RESULT FAIL` with per-case coordinates and destroys its temporary objects.

This checks runtime geometry and scaling under Unity, not visual appearance on an interactive display. Owner gameplay and screenshot verification remain separate.
