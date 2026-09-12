using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FraileyPortalSelector;

[BepInPlugin(Guid, Name, Version)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string Guid = "com.fraileywoodworks.portalselector";
    public const string Name = "BetterPortals";
    public const string Version = "0.3.3";
    internal const int MaxNameLength = 32;
    internal static Plugin Instance;
    internal static readonly Harmony Harmony = new(Guid);

    private void Awake()
    {
        Instance = this;
        Harmony.PatchAll();
        if (!Application.isBatchMode)
        {
            PortalVisualSettings.Bind(Config);
            gameObject.AddComponent<PortalController>();
        }
        Logger.LogInfo($"{Name} {Version} loaded; portal names capped at {MaxNameLength} characters");
    }

    private void OnDestroy() => Harmony.UnpatchSelf();

    internal static void LogDebug(string message) => Instance?.Logger.LogDebug(message);
}

internal static class PortalVisualSettings
{
    internal const float DefaultTitleFontSize = 40f;
    internal const float DefaultEntryFontSize = 31f;
    internal const float DefaultHintFontSize = 17f;
    internal const float DefaultOutlineWidth = 0f;
    internal const float DefaultHoverScale = 1.10f;
    internal const float DefaultHoverExpandSeconds = .08f;
    internal const float DefaultHoverContractSeconds = .10f;
    internal const float DefaultPanelWidth = 620f;
    internal const float DefaultPanelYOffset = 120f;
    internal const float DefaultRowHeight = 46f;
    internal const float DefaultRowSpacing = 2f;
    internal const float DefaultMaxListHeight = 414f;
    internal const float DefaultBackdropOpacity = 1f;
    internal const float DefaultFadeInSeconds = .06f;
    internal const float DefaultFadeOutSeconds = .05f;
    internal const float DefaultTriggerMultiplier = 1.25f;

    private const string DefaultTitleColor = "#FF8F00FF";
    private const string DefaultEntryColor = "#DBDBDBFF";
    private const string DefaultHintColor = "#B8B8B3FF";
    private const string DefaultOutlineColor = "#000000D9";
    private const string DefaultOrnamentColor = "#FF8F00FF";

    private static ConfigEntry<float> titleFontSize;
    private static ConfigEntry<float> entryFontSize;
    private static ConfigEntry<float> hintFontSize;
    private static ConfigEntry<string> titleColor;
    private static ConfigEntry<string> entryColor;
    private static ConfigEntry<string> hintColor;
    private static ConfigEntry<string> outlineColor;
    private static ConfigEntry<float> outlineWidth;
    private static ConfigEntry<float> hoverScale;
    private static ConfigEntry<float> hoverExpandSeconds;
    private static ConfigEntry<float> hoverContractSeconds;
    private static ConfigEntry<float> panelWidth;
    private static ConfigEntry<float> panelYOffset;
    private static ConfigEntry<float> rowHeight;
    private static ConfigEntry<float> rowSpacing;
    private static ConfigEntry<float> maxListHeight;
    private static ConfigEntry<float> backdropOpacity;
    private static ConfigEntry<string> ornamentColor;
    private static ConfigEntry<bool> showMenuOrnament;
    private static ConfigEntry<bool> showRowKnots;
    private static ConfigEntry<float> fadeInSeconds;
    private static ConfigEntry<float> fadeOutSeconds;
    private static ConfigEntry<float> triggerMultiplier;

    internal static void Bind(ConfigFile config)
    {
        titleFontSize = BindRange(config, "Visual - Typography", "Title Font Size", DefaultTitleFontSize, 12f, 96f,
            "Portal selector heading size.");
        entryFontSize = BindRange(config, "Visual - Typography", "Entry Font Size", DefaultEntryFontSize, 12f, 72f,
            "Destination row text size.");
        hintFontSize = BindRange(config, "Visual - Typography", "Hint Font Size", DefaultHintFontSize, 8f, 48f,
            "Size of the close-instruction text below the heading.");
        titleColor = BindColor(config, "Visual - Typography", "Title Color", DefaultTitleColor,
            "Heading color as #RRGGBB or #RRGGBBAA.");
        entryColor = BindColor(config, "Visual - Typography", "Entry Color", DefaultEntryColor,
            "Destination text color as #RRGGBB or #RRGGBBAA.");
        hintColor = BindColor(config, "Visual - Typography", "Hint Color", DefaultHintColor,
            "Instruction text color as #RRGGBB or #RRGGBBAA.");
        outlineColor = BindColor(config, "Visual - Typography", "Font Outline Color", DefaultOutlineColor,
            "Text outline color as #RRGGBB or #RRGGBBAA.");
        outlineWidth = BindRange(config, "Visual - Typography", "Font Outline Width", DefaultOutlineWidth, 0f, 1f,
            "TextMesh Pro outline width for all selector text.");

        hoverScale = BindRange(config, "Visual - Hover", "Hover Scale", DefaultHoverScale, 1f, 1.5f,
            "Scale of the selected or hovered destination label.");
        hoverExpandSeconds = BindRange(config, "Visual - Hover", "Hover Expand Duration", DefaultHoverExpandSeconds, .01f, 1f,
            "Seconds used for the eased hover expansion.");
        hoverContractSeconds = BindRange(config, "Visual - Hover", "Hover Contract Duration", DefaultHoverContractSeconds, .01f, 1f,
            "Seconds used for the eased hover contraction.");

        panelWidth = BindRange(config, "Visual - Layout", "Panel Width", DefaultPanelWidth, 320f, 1400f,
            "Width of the centered portal selector in native UI units.");
        panelYOffset = BindRange(config, "Visual - Layout", "Panel Y Offset", DefaultPanelYOffset, -500f, 500f,
            "Vertical offset from screen center in native UI units.");
        rowHeight = BindRange(config, "Visual - Layout", "Row Height", DefaultRowHeight, 28f, 96f,
            "Height of each destination row.");
        rowSpacing = BindRange(config, "Visual - Layout", "Row Spacing", DefaultRowSpacing, 0f, 24f,
            "Vertical spacing between destination rows.");
        maxListHeight = BindRange(config, "Visual - Layout", "Maximum List Height", DefaultMaxListHeight, 80f, 900f,
            "Maximum visible destination-list height before scrolling.");
        backdropOpacity = BindRange(config, "Visual - Layout", "Dark Backdrop Opacity", DefaultBackdropOpacity, 0f, 1f,
            "Opacity multiplier for Valheim's native dark backdrop; 1 preserves the v20 appearance.");

        ornamentColor = BindColor(config, "Visual - Ornaments", "Ornament and Knot Color", DefaultOrnamentColor,
            "Menu ornament and row-knot color as #RRGGBB or #RRGGBBAA.");
        showMenuOrnament = config.Bind("Visual - Ornaments", "Show Menu Ornament", true,
            "Show Valheim's native menu ornament behind the selector.");
        showRowKnots = config.Bind("Visual - Ornaments", "Show Row Knots", true,
            "Show the native knot marks beside a hovered or controller-selected destination.");

        fadeInSeconds = BindRange(config, "Visual - Animation", "Fade In Duration", DefaultFadeInSeconds, .01f, 1f,
            "Seconds used for the selector fade-in.");
        fadeOutSeconds = BindRange(config, "Visual - Animation", "Fade Out Duration", DefaultFadeOutSeconds, .01f, 1f,
            "Seconds used for the selector fade-out.");
        triggerMultiplier = BindRange(config, "Portal Interaction", "Portal Trigger Multiplier", DefaultTriggerMultiplier, 1f, 1.5f,
            "Client-local trigger and open-menu range multiplier. The server still enforces its fixed 7.5-metre safety limit.");

        Watch(titleFontSize); Watch(entryFontSize); Watch(hintFontSize); Watch(titleColor); Watch(entryColor); Watch(hintColor);
        Watch(outlineColor); Watch(outlineWidth); Watch(hoverScale); Watch(hoverExpandSeconds); Watch(hoverContractSeconds);
        Watch(panelWidth); Watch(panelYOffset); Watch(rowHeight); Watch(rowSpacing); Watch(maxListHeight); Watch(backdropOpacity);
        Watch(ornamentColor); Watch(showMenuOrnament); Watch(showRowKnots); Watch(fadeInSeconds); Watch(fadeOutSeconds);
        Watch(triggerMultiplier);
    }

    private static ConfigEntry<float> BindRange(ConfigFile config, string section, string key, float value, float minimum, float maximum, string description) =>
        config.Bind(section, key, value, new ConfigDescription(description, new AcceptableValueRange<float>(minimum, maximum)));

    private static ConfigEntry<string> BindColor(ConfigFile config, string section, string key, string value, string description) =>
        config.Bind(section, key, value, new ConfigDescription(description));

    private static void Watch<T>(ConfigEntry<T> entry) => entry.SettingChanged += OnSettingChanged;

    private static void OnSettingChanged(object sender, EventArgs args) => PortalController.Instance?.MarkVisualsDirty();

    private static float Clamp(ConfigEntry<float> entry, float fallback, float minimum, float maximum) =>
        Mathf.Clamp(entry?.Value ?? fallback, minimum, maximum);

    private static Color ParseColor(ConfigEntry<string> entry, string fallback)
    {
        var raw = entry?.Value;
        if (!string.IsNullOrWhiteSpace(raw))
        {
            raw = raw.Trim();
            if (!raw.StartsWith("#", StringComparison.Ordinal)) raw = "#" + raw;
            if (ColorUtility.TryParseHtmlString(raw, out var parsed)) return parsed;
        }
        ColorUtility.TryParseHtmlString(fallback, out var defaultColor);
        return defaultColor;
    }

    internal static float TitleFontSize => Clamp(titleFontSize, DefaultTitleFontSize, 12f, 96f);
    internal static float EntryFontSize => Clamp(entryFontSize, DefaultEntryFontSize, 12f, 72f);
    internal static float HintFontSize => Clamp(hintFontSize, DefaultHintFontSize, 8f, 48f);
    internal static Color TitleColor => ParseColor(titleColor, DefaultTitleColor);
    internal static Color EntryColor => ParseColor(entryColor, DefaultEntryColor);
    internal static Color HintColor => ParseColor(hintColor, DefaultHintColor);
    internal static Color OutlineColor => ParseColor(outlineColor, DefaultOutlineColor);
    internal static float OutlineWidth => Clamp(outlineWidth, DefaultOutlineWidth, 0f, 1f);
    internal static float HoverScale => Clamp(hoverScale, DefaultHoverScale, 1f, 1.5f);
    internal static float HoverExpandSeconds => Clamp(hoverExpandSeconds, DefaultHoverExpandSeconds, .01f, 1f);
    internal static float HoverContractSeconds => Clamp(hoverContractSeconds, DefaultHoverContractSeconds, .01f, 1f);
    internal static float PanelWidth => Clamp(panelWidth, DefaultPanelWidth, 320f, 1400f);
    internal static float PanelYOffset => Clamp(panelYOffset, DefaultPanelYOffset, -500f, 500f);
    internal static float RowHeight => Clamp(rowHeight, DefaultRowHeight, 28f, 96f);
    internal static float RowSpacing => Clamp(rowSpacing, DefaultRowSpacing, 0f, 24f);
    internal static float MaxListHeight => Clamp(maxListHeight, DefaultMaxListHeight, 80f, 900f);
    internal static float BackdropOpacity => Clamp(backdropOpacity, DefaultBackdropOpacity, 0f, 1f);
    internal static Color OrnamentColor => ParseColor(ornamentColor, DefaultOrnamentColor);
    internal static bool ShowMenuOrnament => showMenuOrnament?.Value ?? true;
    internal static bool ShowRowKnots => showRowKnots?.Value ?? true;
    internal static float FadeInSeconds => Clamp(fadeInSeconds, DefaultFadeInSeconds, .01f, 1f);
    internal static float FadeOutSeconds => Clamp(fadeOutSeconds, DefaultFadeOutSeconds, .01f, 1f);
    internal static float TriggerMultiplier => Clamp(triggerMultiplier, DefaultTriggerMultiplier, 1f, 1.5f);
}

internal static class PortalNames
{
    internal static string Sanitize(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        var clean = new string(value.Where(c => c >= ' ' && c != '\u007f').Take(Plugin.MaxNameLength).ToArray());
        if (clean.Length > 0 && char.IsHighSurrogate(clean[clean.Length - 1])) clean = clean.Substring(0, clean.Length - 1);
        return clean;
    }
}

internal static class PortalRanges
{
    internal const float MaximumServerDistance = 7.5f;
    internal static float TriggerMultiplier => PortalVisualSettings.TriggerMultiplier;

    internal static float ClientDistance(TeleportWorld portal) =>
        portal && portal.m_activationRange > 0f
            ? Mathf.Min(MaximumServerDistance, portal.m_activationRange * TriggerMultiplier)
            : MaximumServerDistance;
}

[HarmonyPatch(typeof(TextInput), nameof(TextInput.RequestText))]
internal static class PortalNameInputPatch
{
    private static void Prefix(TextReceiver __0, ref int __2)
    {
        if (__0 is TeleportWorld) __2 = Plugin.MaxNameLength;
    }
}

[HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.SetText))]
internal static class PortalNameSendPatch
{
    private static void Prefix(ref string __0) => __0 = PortalNames.Sanitize(__0);
}

[HarmonyPatch(typeof(TeleportWorld), "RPC_SetTag")]
internal static class PortalNameReceivePatch
{
    private static void Prefix(ref string __1) => __1 = PortalNames.Sanitize(__1);
}

[HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.GetHoverText))]
internal static class PortalHoverTextPatch
{
    private static bool Prepare() => !Application.isBatchMode;

    private static void Postfix(ref string __result)
    {
        if (string.IsNullOrEmpty(__result)) return;
        var lineBreak = __result.IndexOf('\n');
        var firstLine = lineBreak >= 0 ? __result.Substring(0, lineBreak) : __result;
        var statusStart = firstLine.LastIndexOf(" [", StringComparison.Ordinal);
        if (statusStart < 0 || !firstLine.EndsWith("]", StringComparison.Ordinal)) return;
        __result = firstLine.Substring(0, statusStart) + (lineBreak >= 0 ? __result.Substring(lineBreak) : string.Empty);
    }
}

[HarmonyPatch(typeof(Game), "Start")]
internal static class RpcRegistrationPatch
{
    private static void Postfix() => PortalRpc.Register();
}

[HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.Teleport))]
internal static class PortalTravelPatch
{
    private static bool Prepare() => !Application.isBatchMode;

    private static bool Prefix(TeleportWorld __instance, Player __0)
    {
        if (Application.isBatchMode || __0 != Player.m_localPlayer || PortalController.BypassOnce) return true;
        var view = __instance.GetComponent<ZNetView>();
        if (!view || !view.IsValid() || view.GetZDO() == null) return true;
        PortalController.Instance.Begin(__instance, view.GetZDO().m_uid);
        return false;
    }
}

[HarmonyPatch(typeof(GameCamera), nameof(GameCamera.UpdateMouseCapture))]
internal static class PortalCursorPatch
{
    private static bool Prepare() => !Application.isBatchMode;

    private static bool Prefix()
    {
        if (Application.isBatchMode || !PortalController.CapturesInput) return true;
        ZCursor.LockState = ZInput.IsMouseActive() ? CursorLockMode.None : CursorLockMode.Locked;
        ZCursor.Show();
        return false;
    }
}

[HarmonyPatch(typeof(Hud), "UpdateCrosshair")]
internal static class PortalCrosshairPatch
{
    private static bool Prepare() => !Application.isBatchMode;

    private static bool Prefix(Hud __instance)
    {
        if (Application.isBatchMode || !PortalController.CapturesInput) return true;
        if (__instance.m_crosshair) __instance.m_crosshair.gameObject.SetActive(false);
        if (__instance.m_crosshairBow) __instance.m_crosshairBow.gameObject.SetActive(false);
        if (__instance.m_hoverName) __instance.m_hoverName.text = string.Empty;
        return false;
    }
}

[HarmonyPatch(typeof(PlayerController), "InInventoryEtc")]
internal static class PortalPlayerInputPatch
{
    private static bool Prepare() => !Application.isBatchMode;

    private static void Postfix(ref bool __result)
    {
        if (!Application.isBatchMode && PortalController.CapturesInput) __result = true;
    }
}

[HarmonyPatch(typeof(Player), "TakeInput")]
internal static class PortalCharacterInputPatch
{
    private static bool Prepare() => !Application.isBatchMode;

    private static void Postfix(Player __instance, ref bool __result)
    {
        if (!Application.isBatchMode && __instance == Player.m_localPlayer && PortalController.CapturesInput) __result = false;
    }
}

[HarmonyPatch(typeof(Menu), nameof(Menu.Show))]
internal static class PortalEscapeMenuPatch
{
    private static bool Prepare() => !Application.isBatchMode;

    private static bool Prefix()
    {
        if (Application.isBatchMode || !PortalController.CapturesInput) return true;
        PortalController.Instance?.DismissFromMenuInput();
        return false;
    }
}

[HarmonyPatch(typeof(TeleportWorldTrigger), "Awake")]
internal static class PortalExitWatcherPatch
{
    private static bool Prepare() => !Application.isBatchMode;

    private static void Postfix(TeleportWorldTrigger __instance)
    {
        if (Application.isBatchMode) return;
        var watcher = __instance.GetComponent<PortalExitWatcher>() ?? __instance.gameObject.AddComponent<PortalExitWatcher>();
        watcher.Initialize(__instance.GetComponentInParent<TeleportWorld>(), __instance);
    }
}

[HarmonyPatch(typeof(TeleportWorld), "UpdatePortal")]
internal static class PortalGlowUpdateContextPatch
{
    [ThreadStatic] private static TeleportWorld updating;

    private static bool Prepare() => !Application.isBatchMode;
    internal static bool IsUpdating(TeleportWorld portal) => updating == portal;
    private static void Prefix(TeleportWorld __instance)
    {
        if (!Application.isBatchMode) updating = __instance;
    }
    private static void Postfix() => updating = null;
    private static Exception Finalizer(Exception __exception)
    {
        updating = null;
        return __exception;
    }
}

[HarmonyPatch(typeof(TeleportWorld), "TargetFound")]
internal static class UnpairedPortalGlowPatch
{
    private static bool Prepare() => !Application.isBatchMode;

    private static void Postfix(TeleportWorld __instance, ref bool __result)
    {
        if (Application.isBatchMode || __result || !PortalGlowUpdateContextPatch.IsUpdating(__instance)) return;
        var player = Player.m_localPlayer;
        var view = __instance.GetComponent<ZNetView>();
        if (!player || !view || !view.IsValid() || view.GetZDO() == null || !__instance.m_proximityRoot) return;
        if (string.IsNullOrWhiteSpace(view.GetZDO().GetString("tag"))) return;
        if (Vector3.Distance(player.transform.position, __instance.m_proximityRoot.position) > __instance.m_activationRange) return;
        if (player.IsTeleportable(__instance.m_allowAllItems)) __result = true;
    }
}

internal sealed class PortalExitWatcher : MonoBehaviour
{
    private readonly HashSet<Collider> localColliders = new();
    private TeleportWorld portal;
    private BoxCollider[] boxes = Array.Empty<BoxCollider>();
    private Vector3[] originalBoxSizes = Array.Empty<Vector3>();
    private SphereCollider[] spheres = Array.Empty<SphereCollider>();
    private float[] originalSphereRadii = Array.Empty<float>();
    private CapsuleCollider[] capsules = Array.Empty<CapsuleCollider>();
    private float[] originalCapsuleRadii = Array.Empty<float>();
    private float[] originalCapsuleHeights = Array.Empty<float>();
    private float appliedMultiplier = -1f;
    private bool rangesCaptured;

    internal void Initialize(TeleportWorld sourcePortal, TeleportWorldTrigger trigger)
    {
        portal = sourcePortal;
        if (!rangesCaptured && trigger)
        {
            boxes = trigger.GetComponents<BoxCollider>();
            originalBoxSizes = boxes.Select(box => box.size).ToArray();
            spheres = trigger.GetComponents<SphereCollider>();
            originalSphereRadii = spheres.Select(sphere => sphere.radius).ToArray();
            capsules = trigger.GetComponents<CapsuleCollider>();
            originalCapsuleRadii = capsules.Select(capsule => capsule.radius).ToArray();
            originalCapsuleHeights = capsules.Select(capsule => capsule.height).ToArray();
            rangesCaptured = true;
        }
        ApplyTriggerMultiplier();
    }

    private void Update()
    {
        if (!Mathf.Approximately(appliedMultiplier, PortalRanges.TriggerMultiplier)) ApplyTriggerMultiplier();
    }

    private void ApplyTriggerMultiplier()
    {
        if (!rangesCaptured) return;
        var multiplier = PortalRanges.TriggerMultiplier;
        for (var i = 0; i < boxes.Length; i++) if (boxes[i]) boxes[i].size = originalBoxSizes[i] * multiplier;
        for (var i = 0; i < spheres.Length; i++) if (spheres[i]) spheres[i].radius = originalSphereRadii[i] * multiplier;
        for (var i = 0; i < capsules.Length; i++)
        {
            if (!capsules[i]) continue;
            capsules[i].radius = originalCapsuleRadii[i] * multiplier;
            capsules[i].height = originalCapsuleHeights[i] * multiplier;
        }
        appliedMultiplier = multiplier;
    }

    private void OnTriggerEnter(Collider other)
    {
        localColliders.RemoveWhere(collider => !collider);
        if (IsLocalPlayer(other)) localColliders.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsLocalPlayer(other)) return;
        localColliders.RemoveWhere(collider => !collider);
        localColliders.Remove(other);
        if (localColliders.Count == 0) PortalController.Instance?.CloseIfSource(portal);
    }

    private void OnDestroy()
    {
        localColliders.Clear();
        PortalController.Instance?.CloseIfSource(portal);
    }

    private static bool IsLocalPlayer(Collider collider)
    {
        var player = collider.GetComponent<Player>() ?? collider.GetComponentInParent<Player>();
        return player && player == Player.m_localPlayer;
    }
}

internal static class PortalRpc
{
    private const string DirectoryRequest = Plugin.Guid + ".directory.request";
    private const string DirectoryResponse = Plugin.Guid + ".directory.response";
    private const string TravelRequest = Plugin.Guid + ".travel.request";
    private const string TravelResponse = Plugin.Guid + ".travel.response";
    private const float ServerSessionLifetime = 60f;
    private static readonly Dictionary<long, RequestWindow> Windows = new();
    private static readonly Dictionary<long, ServerSession> Sessions = new();
    private static ZRoutedRpc registeredOn;

    internal static bool Register()
    {
        var rpc = ZRoutedRpc.instance;
        if (rpc == null) return false;
        if (ReferenceEquals(registeredOn, rpc)) return true;
        rpc.Register<ZDOID, long>(DirectoryRequest, OnDirectoryRequest);
        rpc.Register<ZPackage>(DirectoryResponse, OnDirectoryResponse);
        rpc.Register<ZDOID, ZDOID, long>(TravelRequest, OnTravelRequest);
        rpc.Register<ZPackage>(TravelResponse, OnTravelResponse);
        registeredOn = rpc;
        return true;
    }

    internal static bool AskDirectory(ZDOID source, long nonce)
    {
        if (!Register() || !TryServerId(out var server)) return false;
        ZRoutedRpc.instance.InvokeRoutedRPC(server, DirectoryRequest, source, nonce);
        return true;
    }

    internal static bool AskTravel(ZDOID source, ZDOID destination, long nonce)
    {
        if (!Register() || !TryServerId(out var server)) return false;
        ZRoutedRpc.instance.InvokeRoutedRPC(server, TravelRequest, source, destination, nonce);
        return true;
    }

    private static bool TryServerId(out long server)
    {
        server = 0;
        if (ZNet.instance == null) return false;
        if (ZNet.instance.IsServer())
        {
            server = ZNet.GetUID();
            return true;
        }
        var peer = ZNet.instance.GetServerPeer();
        if (peer == null) return false;
        server = peer.m_uid;
        return true;
    }

    internal static bool TryPortal(ZDOID id, out ZDO portal)
    {
        portal = ZDOMan.instance?.GetPortalList()?.FirstOrDefault(z => z != null && z.IsValid() && z.m_uid == id);
        return portal != null;
    }

    private static bool NearSource(long sender, ZDO source)
    {
        var peer = ZNet.instance?.GetPeer(sender);
        return peer != null && Vector3.Distance(peer.m_refPos, source.GetPosition()) <= PortalRanges.MaximumServerDistance;
    }

    private static bool RateAllowed(long sender)
    {
        var now = Time.realtimeSinceStartup;
        if (Windows.TryGetValue(sender, out var window) && now - window.Start < 2f)
        {
            if (window.Count >= 4) return false;
            Windows[sender] = new RequestWindow(window.Start, window.Count + 1);
        }
        else Windows[sender] = new RequestWindow(now, 1);
        return true;
    }

    private static void OnDirectoryRequest(long sender, ZDOID sourceId, long nonce)
    {
        if (!ZNet.instance.IsServer() || !RateAllowed(sender) || !TryPortal(sourceId, out var source) || !NearSource(sender, source)) return;
        Sessions[sender] = new ServerSession(sourceId, nonce, Time.realtimeSinceStartup + ServerSessionLifetime);
        var portals = ZDOMan.instance.GetPortalList()
            .Where(z => z != null && z.IsValid() && z.m_uid != sourceId)
            .Select(z => new PortalInfo(z.m_uid, PortalNames.Sanitize(z.GetString("tag"))))
            .Where(p => !string.IsNullOrWhiteSpace(p.Name)).OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).Take(512).ToList();
        var package = new ZPackage();
        package.Write(nonce); package.Write(portals.Count);
        foreach (var p in portals) { package.Write(p.Id); package.Write(p.Name); }
        ZRoutedRpc.instance.InvokeRoutedRPC(sender, DirectoryResponse, package);
    }

    private static void OnDirectoryResponse(long sender, ZPackage package)
    {
        if (!TryServerId(out var server) || sender != server) return;
        var nonce = package.ReadLong();
        var count = Mathf.Clamp(package.ReadInt(), 0, 512);
        var list = new List<PortalInfo>(count);
        for (var i = 0; i < count; i++) list.Add(new PortalInfo(package.ReadZDOID(), PortalNames.Sanitize(package.ReadString())));
        PortalController.Instance.Show(nonce, list);
    }

    private static void OnTravelRequest(long sender, ZDOID sourceId, ZDOID destinationId, long nonce)
    {
        var response = new ZPackage(); response.Write(nonce);
        var validSession = Sessions.TryGetValue(sender, out var session) && session.Source == sourceId && session.Nonce == nonce && session.Expires >= Time.realtimeSinceStartup;
        Sessions.Remove(sender);
        if (!ZNet.instance.IsServer() || !RateAllowed(sender) || !validSession || !TryPortal(sourceId, out var source) || !NearSource(sender, source) ||
            !TryPortal(destinationId, out var destination) || destinationId == sourceId || string.IsNullOrWhiteSpace(destination.GetString("tag")))
        { response.Write(false); ZRoutedRpc.instance.InvokeRoutedRPC(sender, TravelResponse, response); return; }
        response.Write(true); response.Write(destination.GetPosition()); response.Write(destination.GetRotation());
        ZRoutedRpc.instance.InvokeRoutedRPC(sender, TravelResponse, response);
    }

    private static void OnTravelResponse(long sender, ZPackage package)
    {
        if (!TryServerId(out var server) || sender != server) return;
        var nonce = package.ReadLong(); var approved = package.ReadBool();
        PortalController.Instance.Complete(nonce, approved, approved ? package.ReadVector3() : default, approved ? package.ReadQuaternion() : default);
    }

    private readonly struct RequestWindow
    {
        internal readonly float Start;
        internal readonly int Count;
        internal RequestWindow(float start, int count) { Start = start; Count = count; }
    }
    private readonly struct ServerSession
    {
        internal readonly ZDOID Source;
        internal readonly long Nonce;
        internal readonly float Expires;
        internal ServerSession(ZDOID source, long nonce, float expires) { Source = source; Nonce = nonce; Expires = expires; }
    }
}

internal readonly struct PortalInfo
{
    internal readonly ZDOID Id;
    internal readonly string Name;
    internal PortalInfo(ZDOID id, string name) { Id = id; Name = name; }
}

internal sealed class PortalController : MonoBehaviour
{
    private const float DirectoryTimeout = 5f;
    private const float SessionLifetime = 60f;
    private const float TravelTimeout = 3f;
    private const float FirstEntryRetryWindow = 6f;
    private const float FirstEntryRetryInterval = .10f;
    private const float DirectoryResponseRetryInterval = .75f;

    internal static PortalController Instance;
    internal static bool BypassOnce;
    internal static bool CapturesInput => Instance != null && ((Instance.panel && Instance.panel.activeSelf) || Instance.travelPending);

    private ZDOID source;
    private TeleportWorld sourcePortal;
    private long nonce;
    private float requestStarted;
    private float sessionDeadline;
    private float nextOpenAllowed;
    private float travelDeadline;
    private GameObject panel;
    private CanvasGroup canvasGroup;
    private Transform content;
    private LayoutElement scrollLayout;
    private ScrollRect destinationScroll;
    private Button firstSelectable;
    private bool? previousMouseInput;
    private float fadeTarget;
    private bool resetAfterFade;
    private bool travelPending;
    private bool directoryReady;
    private bool pendingTeleport;
    private Vector3 pendingPosition;
    private Quaternion pendingRotation;
    private TeleportWorld deferredPortal;
    private ZDOID deferredSource;
    private float deferredStarted;
    private float deferredDeadline;
    private float nextDeferredAttempt;
    private RectTransform panelRect;
    private VerticalLayoutGroup panelLayout;
    private VerticalLayoutGroup contentLayout;
    private TMP_Text titleText;
    private TMP_Text hintText;
    private GameObject darkBackdrop;
    private CanvasGroup darkBackdropGroup;
    private GameObject menuOrnament;
    private Graphic[] menuOrnamentGraphics = Array.Empty<Graphic>();
    private float[] menuOrnamentBaseAlphas = Array.Empty<float>();
    private bool visualsDirty;
    private readonly List<GameObject> rows = new();
    private static TMP_FontAsset nativeFont;
    private static Material nativeFontMaterial;

    private void Awake()
    {
        Instance = this;
        visualsDirty = true;
    }

    internal void MarkVisualsDirty() => visualsDirty = true;

    internal void Begin(TeleportWorld portal, ZDOID sourceId)
    {
        if (!portal || CapturesInput || nonce != 0 || deferredPortal || Time.realtimeSinceStartup < nextOpenAllowed || AnotherModalIsVisible()) return;
        if (!PortalCandidateIsUsable(portal, sourceId)) return;
        deferredPortal = portal;
        deferredSource = sourceId;
        deferredStarted = Time.realtimeSinceStartup;
        deferredDeadline = deferredStarted + FirstEntryRetryWindow;
        nextDeferredAttempt = deferredStarted + FirstEntryRetryInterval;

        if (!TryStartRequest(portal, sourceId))
        {
            Plugin.LogDebug($"Portal entry deferred for up to {FirstEntryRetryWindow:0.0}s while the world portal registry and routed RPC finish initializing.");
            return;
        }

        if (deferredSource != ZDOID.None && !directoryReady)
        {
            nextDeferredAttempt = Time.realtimeSinceStartup + DirectoryResponseRetryInterval;
            Plugin.LogDebug("Portal directory request sent; waiting for server acceptance.");
        }
    }

    private bool TryStartRequest(TeleportWorld portal, ZDOID sourceId)
    {
        if (!PortalCandidateIsUsable(portal, sourceId) || !PortalRpc.Register() || !PortalRpc.TryPortal(sourceId, out _)) return false;

        sourcePortal = portal;
        source = sourceId;
        if (!SourceIsUsable())
        {
            sourcePortal = null;
            source = ZDOID.None;
            return false;
        }

        nonce = DateTime.UtcNow.Ticks;
        directoryReady = false;
        requestStarted = Time.realtimeSinceStartup;
        sessionDeadline = requestStarted + SessionLifetime - 2f;
        if (!PortalRpc.AskDirectory(source, nonce))
        {
            sourcePortal = null;
            source = ZDOID.None;
            nonce = 0;
            requestStarted = 0f;
            sessionDeadline = 0f;
            return false;
        }
        return true;
    }

    private static bool PortalCandidateIsUsable(TeleportWorld portal, ZDOID sourceId)
    {
        var player = Player.m_localPlayer;
        if (!portal || !player || sourceId == ZDOID.None) return false;
        var view = portal.GetComponent<ZNetView>();
        if (!view || !view.IsValid() || view.GetZDO() == null || view.GetZDO().m_uid != sourceId) return false;
        var center = portal.m_proximityRoot ? portal.m_proximityRoot.position : view.GetZDO().GetPosition();
        return Vector3.Distance(player.transform.position, center) <= PortalRanges.ClientDistance(portal);
    }

    private void AdvanceDeferredBegin()
    {
        if (deferredSource == ZDOID.None) return;
        if (!deferredPortal)
        {
            ClearDeferredBegin();
            return;
        }
        var now = Time.realtimeSinceStartup;
        if (now >= deferredDeadline)
        {
            Plugin.LogDebug($"Deferred portal entry timed out after {Mathf.Max(0f, now - deferredStarted):0.00}s; leave and re-enter the portal to retry.");
            ResetSession(0f);
            return;
        }
        if (now < nextOpenAllowed || AnotherModalIsVisible() || !PortalCandidateIsUsable(deferredPortal, deferredSource))
        {
            Plugin.LogDebug("Deferred portal entry canceled because the player left range, opened another interface, or entered the post-travel cooldown.");
            ResetSession(0f);
            return;
        }
        if (now < nextDeferredAttempt) return;

        if (nonce == 0)
        {
            nextDeferredAttempt = now + FirstEntryRetryInterval;
            if (TryStartRequest(deferredPortal, deferredSource))
            {
                nextDeferredAttempt = now + DirectoryResponseRetryInterval;
                Plugin.LogDebug("Deferred portal directory request sent; waiting for server acceptance.");
            }
            return;
        }

        if (directoryReady) return;
        nextDeferredAttempt = now + DirectoryResponseRetryInterval;
        if (!PortalRpc.AskDirectory(source, nonce)) nextDeferredAttempt = now + FirstEntryRetryInterval;
    }

    private void ClearDeferredBegin()
    {
        deferredPortal = null;
        deferredSource = ZDOID.None;
        deferredStarted = 0f;
        deferredDeadline = 0f;
        nextDeferredAttempt = 0f;
    }

    internal void Show(long responseNonce, List<PortalInfo> portals)
    {
        if (responseNonce != nonce || travelPending || directoryReady) return;
        if (!SourceIsUsable()) { ResetSession(0f); return; }
        directoryReady = true;
        if (deferredSource != ZDOID.None)
        {
            var waited = Mathf.Max(0f, Time.realtimeSinceStartup - deferredStarted);
            Plugin.LogDebug($"Deferred portal entry started successfully after {waited:0.00}s and server acceptance.");
            ClearDeferredBegin();
        }
        EnsureUi();
        foreach (var row in rows) Destroy(row); rows.Clear();
        firstSelectable = null;
        var duplicateTotals = portals.GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);
        var duplicateIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var portal in portals)
        {
            duplicateIndex.TryGetValue(portal.Name, out var index); duplicateIndex[portal.Name] = ++index;
            var label = duplicateTotals[portal.Name] > 1 ? $"{portal.Name}  ({index}/{duplicateTotals[portal.Name]})" : portal.Name;
            var destination = portal.Id;
            var button = AddRow(label, () => SelectDestination(destination));
            if (!firstSelectable) firstSelectable = button;
        }
        if (portals.Count == 0) AddRow("No named destinations found", null);

        ConfigureRowNavigation();
        ApplyVisualSettings();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        fadeTarget = 1f;
        resetAfterFade = false;
        panel.SetActive(true);
        ZCursor.LockState = ZInput.IsMouseActive() ? CursorLockMode.None : CursorLockMode.Locked;
        ZCursor.Show();
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)panel.transform);
        previousMouseInput = null;
        SynchronizeInputSelection();
    }

    internal void Complete(long responseNonce, bool approved, Vector3 position, Quaternion rotation)
    {
        if (responseNonce != nonce) return;
        travelPending = false;
        if (!approved || Player.m_localPlayer == null) { RequestClose(true); return; }
        pendingTeleport = true;
        pendingPosition = position;
        pendingRotation = rotation;
        BeginFadeOut(false);
        if (!panel || !panel.activeSelf) PerformPendingTeleport();
    }

    internal void CloseIfSource(TeleportWorld portal)
    {
        if (portal && portal == deferredPortal)
        {
            Plugin.LogDebug("Deferred portal entry canceled when the player left the source trigger.");
            ClearDeferredBegin();
        }
        if (portal && portal == sourcePortal && nonce != 0) RequestClose(true);
    }

    internal void DismissFromMenuInput() => RequestClose(true);

    private void Update()
    {
        AdvanceDeferredBegin();
        if (visualsDirty) ApplyVisualSettings();
        if (nonce != 0 && !SourceIsUsable()) RequestClose(true);
        if (panel && panel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || ZInput.GetButtonDown("JoyButtonB") || AnotherModalIsVisible()) RequestClose(true);
            SynchronizeInputSelection();
            KeepControllerSelectionVisible();
            AdvanceFade();
        }

        if (nonce != 0 && Time.realtimeSinceStartup >= sessionDeadline) RequestClose(true);
        if (travelPending && Time.realtimeSinceStartup >= travelDeadline) RequestClose(true);
        if (nonce != 0 && deferredSource == ZDOID.None && !travelPending && (!panel || !panel.activeSelf) && Time.realtimeSinceStartup - requestStarted >= DirectoryTimeout) ResetSession(0f);
        if (pendingTeleport && (!panel || !panel.activeSelf)) PerformPendingTeleport();
    }

    private void EnsureUi()
    {
        if (panel) return;
        var canvasObject = new GameObject("BetterPortals Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        var nativeCanvas = Menu.instance?.GetComponentInParent<Canvas>();
        canvas.sortingOrder = nativeCanvas ? nativeCanvas.sortingOrder + 1 : 1701;
        ConfigureScaler(canvasObject.GetComponent<CanvasScaler>());

        panel = new GameObject("Floating Portal Menu", typeof(RectTransform), typeof(CanvasGroup), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        panel.transform.SetParent(canvasObject.transform, false);
        panelRect = (RectTransform)panel.transform;
        panelRect.anchorMin = panelRect.anchorMax = new Vector2(.5f, .5f);
        panelRect.pivot = new Vector2(.5f, .5f);
        canvasGroup = panel.GetComponent<CanvasGroup>();
        panelLayout = panel.GetComponent<VerticalLayoutGroup>();
        panelLayout.padding = new RectOffset(16, 16, 12, 12);
        panelLayout.spacing = 4f;
        panelLayout.childAlignment = TextAnchor.MiddleCenter;
        panelLayout.childControlHeight = true;
        panelLayout.childControlWidth = true;
        panelLayout.childForceExpandHeight = false;
        panelLayout.childForceExpandWidth = true;
        panel.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        darkBackdrop = CloneNativeDecoration("darken", panel.transform, 0);
        if (darkBackdrop) darkBackdropGroup = darkBackdrop.GetComponent<CanvasGroup>() ?? darkBackdrop.AddComponent<CanvasGroup>();
        menuOrnament = CloneNativeDecoration("ornament", panel.transform, 1);
        if (menuOrnament)
        {
            menuOrnamentGraphics = menuOrnament.GetComponentsInChildren<Graphic>(true);
            menuOrnamentBaseAlphas = menuOrnamentGraphics.Select(graphic => graphic.color.a).ToArray();
        }
        titleText = AddText(panel.transform, "CHOOSE DESTINATION", PortalVisualSettings.TitleFontSize, FontStyles.Normal, PortalVisualSettings.TitleColor);
        hintText = AddText(panel.transform, "Walk away or press Escape to close", PortalVisualSettings.HintFontSize, FontStyles.Normal, PortalVisualSettings.HintColor);

        var scrollObject = new GameObject("Destinations", typeof(RectTransform), typeof(ScrollRect), typeof(LayoutElement));
        scrollObject.transform.SetParent(panel.transform, false);
        scrollLayout = scrollObject.GetComponent<LayoutElement>();
        scrollLayout.preferredHeight = 320f;
        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
        viewport.transform.SetParent(scrollObject.transform, false);
        Stretch((RectTransform)viewport.transform);
        var contentObject = new GameObject("Content", typeof(VerticalLayoutGroup), typeof(ContentSizeFitter)); contentObject.transform.SetParent(viewport.transform, false);
        content = contentObject.transform;
        var cr = (RectTransform)content;
        cr.anchorMin = new Vector2(0, 1);
        cr.anchorMax = new Vector2(1, 1);
        cr.pivot = new Vector2(.5f, 1);
        cr.anchoredPosition = Vector2.zero;
        cr.sizeDelta = Vector2.zero;
        contentLayout = contentObject.GetComponent<VerticalLayoutGroup>();
        contentLayout.spacing = 2f;
        contentLayout.childAlignment = TextAnchor.UpperCenter;
        contentLayout.childControlHeight = true;
        contentLayout.childControlWidth = false;
        contentLayout.childForceExpandHeight = false;
        contentLayout.childForceExpandWidth = false;
        contentObject.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        destinationScroll = scrollObject.GetComponent<ScrollRect>();
        destinationScroll.viewport = (RectTransform)viewport.transform;
        destinationScroll.content = cr;
        destinationScroll.horizontal = false;
        destinationScroll.vertical = true;
        destinationScroll.movementType = ScrollRect.MovementType.Clamped;
        destinationScroll.scrollSensitivity = 32f;

        panel.SetActive(false);
        ApplyVisualSettings();
    }

    private static GameObject CloneNativeDecoration(string name, Transform parent, int siblingIndex)
    {
        var template = Menu.instance?.m_menuDialog?.Find(name);
        if (!template) return null;
        var decoration = UnityEngine.Object.Instantiate(template.gameObject, parent, false);
        decoration.name = "Portal " + name;
        decoration.SetActive(true);
        var layout = decoration.GetComponent<LayoutElement>() ?? decoration.AddComponent<LayoutElement>();
        layout.ignoreLayout = true;
        foreach (var graphic in decoration.GetComponentsInChildren<Graphic>(true)) graphic.raycastTarget = false;
        decoration.transform.SetSiblingIndex(Mathf.Clamp(siblingIndex, 0, parent.childCount - 1));
        return decoration;
    }

    private void ApplyVisualSettings()
    {
        visualsDirty = false;
        if (!panel) return;

        panelRect.anchoredPosition = new Vector2(0f, PortalVisualSettings.PanelYOffset);
        panelRect.sizeDelta = new Vector2(PortalVisualSettings.PanelWidth, 0f);
        if (contentLayout) contentLayout.spacing = PortalVisualSettings.RowSpacing;
        if (destinationScroll) destinationScroll.scrollSensitivity = Mathf.Max(16f, PortalVisualSettings.RowHeight * .7f);

        ApplyTextAppearance(titleText, PortalVisualSettings.TitleFontSize, PortalVisualSettings.TitleColor);
        ApplyTextAppearance(hintText, PortalVisualSettings.HintFontSize, PortalVisualSettings.HintColor);
        foreach (var row in rows)
        {
            if (!row) continue;
            row.GetComponent<PortalEntryHover>()?.ApplyVisualSettings();
        }

        if (darkBackdrop)
        {
            darkBackdrop.SetActive(PortalVisualSettings.BackdropOpacity > .001f);
            if (darkBackdropGroup) darkBackdropGroup.alpha = PortalVisualSettings.BackdropOpacity;
        }
        if (menuOrnament)
        {
            menuOrnament.SetActive(PortalVisualSettings.ShowMenuOrnament);
            var color = PortalVisualSettings.OrnamentColor;
            for (var i = 0; i < menuOrnamentGraphics.Length; i++)
            {
                var graphic = menuOrnamentGraphics[i];
                if (!graphic) continue;
                graphic.color = new Color(color.r, color.g, color.b, menuOrnamentBaseAlphas[i] * color.a);
            }
        }

        UpdateListHeight();
        if (panel.activeSelf) LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);
    }

    private void UpdateListHeight()
    {
        if (!scrollLayout) return;
        var count = Mathf.Max(1, rows.Count);
        var rowHeight = PortalVisualSettings.RowHeight;
        var contentHeight = count * rowHeight + Mathf.Max(0, count - 1) * PortalVisualSettings.RowSpacing;
        var minimum = Mathf.Min(PortalVisualSettings.MaxListHeight, Mathf.Max(54f, rowHeight));
        scrollLayout.preferredHeight = Mathf.Clamp(contentHeight, minimum, PortalVisualSettings.MaxListHeight);
    }

    internal static void ApplyTextAppearance(TMP_Text text, float size, Color color)
    {
        if (!text) return;
        text.fontSize = size;
        text.color = color;
        text.outlineColor = PortalVisualSettings.OutlineColor;
        text.outlineWidth = PortalVisualSettings.OutlineWidth;
    }

    private void ConfigureRowNavigation()
    {
        var buttons = rows.Select(row => row ? row.GetComponent<Button>() : null)
            .Where(button => button && button.interactable).ToList();
        for (var i = 0; i < buttons.Count; i++)
        {
            var navigation = buttons[i].navigation;
            navigation.mode = Navigation.Mode.Explicit;
            navigation.selectOnUp = buttons[(i + buttons.Count - 1) % buttons.Count];
            navigation.selectOnDown = buttons[(i + 1) % buttons.Count];
            buttons[i].navigation = navigation;
        }
    }

    private void SynchronizeInputSelection()
    {
        var eventSystem = EventSystem.current;
        if (!eventSystem) return;
        var mouseInput = ZInput.IsMouseActive();
        if (previousMouseInput.HasValue && previousMouseInput.Value == mouseInput) return;
        previousMouseInput = mouseInput;

        var selected = eventSystem.currentSelectedGameObject;
        if (mouseInput)
        {
            if (selected && panel && selected.transform.IsChildOf(panel.transform)) eventSystem.SetSelectedGameObject(null);
        }
        else if (firstSelectable)
        {
            eventSystem.SetSelectedGameObject(firstSelectable.gameObject);
        }
    }

    private void KeepControllerSelectionVisible()
    {
        if (ZInput.IsMouseActive() || !destinationScroll || !destinationScroll.viewport || !destinationScroll.content) return;
        var selected = EventSystem.current?.currentSelectedGameObject;
        if (!selected || !selected.transform.IsChildOf(content)) return;

        Canvas.ForceUpdateCanvases();
        var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(destinationScroll.viewport, selected.transform);
        var viewportRect = destinationScroll.viewport.rect;
        var verticalAdjustment = 0f;
        if (bounds.min.y < viewportRect.yMin) verticalAdjustment = viewportRect.yMin - bounds.min.y;
        else if (bounds.max.y > viewportRect.yMax) verticalAdjustment = viewportRect.yMax - bounds.max.y;
        if (Mathf.Approximately(verticalAdjustment, 0f)) return;

        var position = destinationScroll.content.anchoredPosition;
        position.y += verticalAdjustment;
        destinationScroll.content.anchoredPosition = position;
    }

    private Button AddRow(string label, Action action)
    {
        var template = Menu.instance?.m_continueButton;
        if (!template) return AddFallbackRow(label, action);

        var row = UnityEngine.Object.Instantiate(template.gameObject, content, false);
        row.name = label;
        row.SetActive(false);
        var animator = row.GetComponent<Animator>();
        if (animator) animator.enabled = false;
        var button = row.GetComponent<Button>();
        button.onClick = new Button.ButtonClickedEvent();
        button.transition = Selectable.Transition.None;
        button.interactable = action != null;
        if (action != null) button.onClick.AddListener(() => action());

        var background = row.GetComponent<Image>() ?? row.AddComponent<Image>();
        background.color = Color.clear;
        background.raycastTarget = true;
        button.targetGraphic = background;
        var rowLayout = row.GetComponent<LayoutElement>() ?? row.AddComponent<LayoutElement>();
        rowLayout.preferredHeight = PortalVisualSettings.RowHeight;

        var text = row.GetComponentInChildren<TMP_Text>(true);
        if (!text) { Destroy(row); return AddFallbackRow(label, action); }
        text.text = label;
        text.raycastTarget = false;
        text.transform.localScale = Vector3.one;
        ApplyTextAppearance(text, PortalVisualSettings.EntryFontSize,
            action == null ? new Color(.55f, .55f, .53f, 1f) : PortalVisualSettings.EntryColor);
        var knots = row.GetComponentsInChildren<Image>(true)
            .Where(image => image && image != background && image.name.IndexOf("Knot", StringComparison.OrdinalIgnoreCase) >= 0)
            .ToArray();
        var knotColor = PortalVisualSettings.OrnamentColor;
        foreach (var knot in knots)
        {
            knot.color = new Color(knotColor.r, knotColor.g, knotColor.b, 0f);
            knot.raycastTarget = false;
        }
        var hover = row.GetComponent<PortalEntryHover>() ?? row.AddComponent<PortalEntryHover>();
        hover.Initialize(text, background, knots, action != null);
        rows.Add(row);
        row.SetActive(true);
        return button;
    }

    private Button AddFallbackRow(string label, Action action)
    {
        var row = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement), typeof(PortalEntryHover));
        row.transform.SetParent(content, false);
        row.GetComponent<LayoutElement>().preferredHeight = PortalVisualSettings.RowHeight;
        var background = row.GetComponent<Image>();
        background.color = Color.clear;
        background.raycastTarget = true;
        var textColor = action == null ? new Color(.55f, .55f, .53f, 1f) : PortalVisualSettings.EntryColor;
        var text = AddText(row.transform, label, PortalVisualSettings.EntryFontSize, FontStyles.Normal, textColor);
        Stretch((RectTransform)text.transform);
        var button = row.GetComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.interactable = action != null;
        if (action != null) button.onClick.AddListener(() => action());
        row.GetComponent<PortalEntryHover>().Initialize(text, background, Array.Empty<Image>(), action != null);
        rows.Add(row);
        return button;
    }

    private static TextMeshProUGUI AddText(Transform parent, string value, float size, FontStyles style, Color color)
    {
        if (!TryResolveNativeTextStyle(out var font, out var material))
        {
            throw new InvalidOperationException("No loaded Valheim TextMesh Pro font and material were available for the portal selector.");
        }

        // TextMeshProUGUI runs its enable-time validation as soon as it is added to an
        // active GameObject. Build the label inactive so it never tries to fall back to
        // TMP's absent LiberationSans default before Valheim's native font is assigned.
        var go = new GameObject("Label", typeof(RectTransform));
        go.SetActive(false);
        go.transform.SetParent(parent, false);
        var text = go.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontSharedMaterial = material;
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = TextAlignmentOptions.Center;
        text.richText = false;
        text.color = color;
        text.raycastTarget = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.outlineColor = PortalVisualSettings.OutlineColor;
        text.outlineWidth = PortalVisualSettings.OutlineWidth;
        go.SetActive(true);
        return text;
    }

    private static bool TryResolveNativeTextStyle(out TMP_FontAsset font, out Material material)
    {
        if (nativeFont && nativeFontMaterial)
        {
            font = nativeFont;
            material = nativeFontMaterial;
            return true;
        }

        var candidates = new TMP_Text[]
        {
            Menu.instance?.m_continueButton?.GetComponentInChildren<TMP_Text>(true),
            Menu.instance?.lastSaveText,
            Hud.instance?.m_hoverName,
            Hud.instance?.m_buildSelection,
            MessageHud.instance?.m_messageText,
            TextInput.instance?.m_topic
        };
        var native = candidates.FirstOrDefault(IsUsableTextTemplate) ??
                     Resources.FindObjectsOfTypeAll<TMP_Text>().FirstOrDefault(IsUsableTextTemplate);
        if (native)
        {
            nativeFont = native.font;
            nativeFontMaterial = native.fontSharedMaterial ? native.fontSharedMaterial : native.font.material;
        }

        if (!nativeFont || !nativeFontMaterial)
        {
            var asset = Resources.FindObjectsOfTypeAll<TMP_FontAsset>()
                .FirstOrDefault(candidate => candidate && candidate.material);
            if (asset)
            {
                nativeFont = asset;
                nativeFontMaterial = asset.material;
            }
        }

        font = nativeFont;
        material = nativeFontMaterial;
        return font && material;
    }

    private static bool IsUsableTextTemplate(TMP_Text candidate) =>
        candidate && candidate.font && (candidate.fontSharedMaterial || candidate.font.material);

    private static void AddSeparator(Transform parent)
    {
        var line = new GameObject("Amber Divider", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        line.transform.SetParent(parent, false);
        var image = line.GetComponent<Image>();
        image.color = new Color(1f, .48f, .12f, .72f);
        image.raycastTarget = false;
        line.GetComponent<LayoutElement>().preferredHeight = 2f;
    }

    private static void ConfigureScaler(CanvasScaler scaler)
    {
        var native = Menu.instance?.GetComponentInParent<CanvasScaler>();
        if (native)
        {
            scaler.uiScaleMode = native.uiScaleMode;
            scaler.scaleFactor = native.scaleFactor;
            scaler.referenceResolution = native.referenceResolution;
            scaler.screenMatchMode = native.screenMatchMode;
            scaler.matchWidthOrHeight = native.matchWidthOrHeight;
            scaler.referencePixelsPerUnit = native.referencePixelsPerUnit;
            scaler.dynamicPixelsPerUnit = native.dynamicPixelsPerUnit;
            return;
        }
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = .5f;
    }

    private void SelectDestination(ZDOID destination)
    {
        if (travelPending || nonce == 0 || !SourceIsUsable()) return;
        travelPending = true;
        travelDeadline = Time.realtimeSinceStartup + TravelTimeout;
        if (canvasGroup)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        if (!PortalRpc.AskTravel(source, destination, nonce))
        {
            RequestClose(true);
            return;
        }
        BeginFadeOut(false);
    }

    private bool SourceIsUsable()
    {
        if (Player.m_localPlayer == null || source == ZDOID.None || !PortalRpc.TryPortal(source, out var sourceZdo)) return false;
        var center = sourceZdo.GetPosition();
        var range = PortalRanges.MaximumServerDistance;
        if (sourcePortal)
        {
            var view = sourcePortal.GetComponent<ZNetView>();
            if (!view || !view.IsValid() || view.GetZDO() == null || view.GetZDO().m_uid != source) return false;
            if (sourcePortal.m_proximityRoot) center = sourcePortal.m_proximityRoot.position;
            range = PortalRanges.ClientDistance(sourcePortal);
        }
        return Vector3.Distance(Player.m_localPlayer.transform.position, center) <= range;
    }

    private static bool AnotherModalIsVisible() =>
        Menu.IsVisible() || TextInput.IsVisible() || InventoryGui.IsVisible() || StoreGui.IsVisible() || Minimap.IsOpen() || Console.IsVisible();

    private void RequestClose(bool invalidateRequest)
    {
        if (invalidateRequest)
        {
            nonce = 0;
            travelPending = false;
            pendingTeleport = false;
            resetAfterFade = true;
        }
        BeginFadeOut(invalidateRequest);
    }

    private void BeginFadeOut(bool resetWhenHidden)
    {
        resetAfterFade |= resetWhenHidden;
        if (!panel || !panel.activeSelf)
        {
            if (pendingTeleport) PerformPendingTeleport();
            else if (resetAfterFade) ResetSession(0f);
            return;
        }
        if (canvasGroup)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        fadeTarget = 0f;
    }

    private void AdvanceFade()
    {
        if (!canvasGroup) return;
        var duration = fadeTarget > canvasGroup.alpha ? PortalVisualSettings.FadeInSeconds : PortalVisualSettings.FadeOutSeconds;
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, fadeTarget, Time.unscaledDeltaTime / duration);
        if (fadeTarget <= 0f && Mathf.Approximately(canvasGroup.alpha, 0f)) FinishHidden();
    }

    private void FinishHidden()
    {
        if (!panel) return;
        var selected = EventSystem.current?.currentSelectedGameObject;
        if (selected && selected.transform.IsChildOf(panel.transform)) EventSystem.current.SetSelectedGameObject(null);
        panel.SetActive(false);
        previousMouseInput = null;
        GameCamera.instance?.UpdateMouseCapture();
        if (pendingTeleport) PerformPendingTeleport();
        else if (resetAfterFade) ResetSession(0f);
    }

    private void PerformPendingTeleport()
    {
        if (!pendingTeleport) return;
        var player = Player.m_localPlayer;
        var position = pendingPosition;
        var rotation = pendingRotation;
        ResetSession(1.5f);
        GameCamera.instance?.UpdateMouseCapture();
        if (!player) return;
        BypassOnce = true;
        try { player.TeleportTo(position + rotation * Vector3.forward * 2f, rotation, true); }
        finally { BypassOnce = false; }
    }

    private void ResetSession(float cooldown)
    {
        ClearDeferredBegin();
        source = ZDOID.None;
        sourcePortal = null;
        nonce = 0;
        requestStarted = 0f;
        sessionDeadline = 0f;
        travelDeadline = 0f;
        travelPending = false;
        directoryReady = false;
        pendingTeleport = false;
        resetAfterFade = false;
        nextOpenAllowed = Time.realtimeSinceStartup + cooldown;
        if (!panel || !panel.activeSelf) GameCamera.instance?.UpdateMouseCapture();
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }
}

internal sealed class PortalEntryHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    private TMP_Text label;
    private Image background;
    private Image[] knots = Array.Empty<Image>();
    private LayoutElement rowLayout;
    private Color normalColor;
    private bool interactive;
    private bool selected;
    private bool pointerInside;
    private bool animating;
    private float currentScale = 1f;
    private float startScale = 1f;
    private float targetScale = 1f;
    private float transitionStarted;
    private float transitionDuration;

    internal void Initialize(TMP_Text rowLabel, Image rowBackground, Image[] rowKnots, bool canInteract)
    {
        label = rowLabel;
        background = rowBackground;
        knots = rowKnots ?? Array.Empty<Image>();
        rowLayout = GetComponent<LayoutElement>();
        interactive = canInteract;
        currentScale = startScale = targetScale = 1f;
        ApplyScale(1f);
        ApplyVisualSettings();
    }

    internal void ApplyVisualSettings()
    {
        if (!label || !background) return;
        normalColor = interactive ? PortalVisualSettings.EntryColor : new Color(.55f, .55f, .53f, 1f);
        PortalController.ApplyTextAppearance(label, PortalVisualSettings.EntryFontSize, normalColor);
        background.color = Color.clear;
        if (rowLayout) rowLayout.preferredHeight = PortalVisualSettings.RowHeight;
        SetHighlighted(pointerInside || selected);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!interactive) return;
        pointerInside = true;
        SetHighlighted(pointerInside || selected);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!interactive) return;
        pointerInside = false;
        SetHighlighted(pointerInside || selected);
    }

    public void OnSelect(BaseEventData eventData) { selected = interactive; SetHighlighted(pointerInside || selected); }
    public void OnDeselect(BaseEventData eventData) { selected = false; SetHighlighted(pointerInside || selected); }

    private void Update()
    {
        if (!animating || !label) return;
        var progress = Mathf.Clamp01((Time.unscaledTime - transitionStarted) / Mathf.Max(.001f, transitionDuration));
        var eased = Mathf.SmoothStep(0f, 1f, progress);
        currentScale = Mathf.Lerp(startScale, targetScale, eased);
        ApplyScale(currentScale);
        if (progress >= 1f) animating = false;
    }

    private void OnDisable()
    {
        pointerInside = false;
        selected = false;
        animating = false;
        currentScale = startScale = targetScale = 1f;
        ApplyScale(1f);
        ApplyKnots(false);
    }

    private void SetHighlighted(bool highlighted)
    {
        if (!label || !background) return;
        label.color = normalColor;
        background.color = Color.clear;
        var desiredScale = highlighted && interactive ? PortalVisualSettings.HoverScale : 1f;
        if (!Mathf.Approximately(desiredScale, targetScale) || (!animating && !Mathf.Approximately(currentScale, desiredScale)))
        {
            startScale = currentScale;
            targetScale = desiredScale;
            transitionStarted = Time.unscaledTime;
            transitionDuration = desiredScale > currentScale
                ? PortalVisualSettings.HoverExpandSeconds
                : PortalVisualSettings.HoverContractSeconds;
            animating = true;
        }
        ApplyKnots(highlighted && interactive);
    }

    private void ApplyScale(float scale)
    {
        if (label) label.transform.localScale = Vector3.one * scale;
    }

    private void ApplyKnots(bool highlighted)
    {
        var ornament = PortalVisualSettings.OrnamentColor;
        var enabledByConfig = PortalVisualSettings.ShowRowKnots;
        foreach (var knot in knots)
        {
            if (!knot) continue;
            knot.gameObject.SetActive(enabledByConfig);
            knot.color = new Color(ornament.r, ornament.g, ornament.b, enabledByConfig && highlighted ? ornament.a : 0f);
        }
    }
}
