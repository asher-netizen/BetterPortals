using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
    public const string Version = "0.4.6";
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
    internal const float DefaultBiomeFontSize = 9.3f;
    internal const float DefaultHomeIconSize = 13.2f;
    internal const float DefaultOutlineWidth = 0f;
    internal const float DefaultHoverScale = 1.10f;
    internal const float DefaultHoverExpandSeconds = .08f;
    internal const float DefaultHoverContractSeconds = .10f;
    internal const float DefaultPanelWidth = 620f;
    internal const float DefaultPanelYOffset = 0f;
    internal const float DefaultRowHeight = 46f;
    internal const float DefaultRowSpacing = 2f;
    internal const float DefaultMaxListHeight = 414f;
    internal const float DefaultBackdropOpacity = 1f;
    internal const float DefaultFadeInSeconds = .06f;
    internal const float DefaultFadeOutSeconds = .05f;
    internal const float DefaultTriggerMultiplier = 1.5625f;
    internal const float DefaultRowKnotGap = 18f;

    private const string DefaultTitleColor = "#FF8F00FF";
    private const string DefaultEntryColor = "#FFFFFFFF";
    private const string DefaultHintColor = "#B8B8B3FF";
    private const string DefaultBiomeColor = "#FFFFFFFF";
    private const string DefaultOutlineColor = "#000000D9";
    private const string DefaultOrnamentColor = "#FF8F00FF";

    private static ConfigEntry<float> titleFontSize;
    private static ConfigEntry<float> entryFontSize;
    private static ConfigEntry<float> hintFontSize;
    private static ConfigEntry<float> biomeFontSize;
    private static ConfigEntry<float> homeIconSize;
    private static ConfigEntry<string> titleColor;
    private static ConfigEntry<string> entryColor;
    private static ConfigEntry<string> hintColor;
    private static ConfigEntry<string> biomeColor;
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
    private static ConfigEntry<float> rowKnotGap;
    private static ConfigEntry<float> fadeInSeconds;
    private static ConfigEntry<float> fadeOutSeconds;
    private static ConfigEntry<float> triggerMultiplier;
    private static ConfigEntry<bool> showBiomeSubtitle;
    private static ConfigEntry<bool> preferHomeOnOpen;
    private static ConfigEntry<bool> showControlHints;
    private static ConfigEntry<KeyboardShortcut> setHomeKeyboardShortcut;

    internal static void Bind(ConfigFile config)
    {
        titleFontSize = BindRange(config, "Visual - Typography", "Title Font Size", DefaultTitleFontSize, 12f, 96f,
            "Portal selector heading size.");
        entryFontSize = BindRange(config, "Visual - Typography", "Entry Font Size", DefaultEntryFontSize, 12f, 72f,
            "Destination row text size.");
        hintFontSize = BindRange(config, "Visual - Typography", "Hint Font Size", DefaultHintFontSize, 8f, 48f,
            "Size of the close-instruction text below the heading.");
        biomeFontSize = BindRange(config, "Visual - Typography", "Biome Font Size", DefaultBiomeFontSize, 8f, 48f,
            "Size of the optional biome subtitle beneath each portal name.");
        homeIconSize = BindRange(config, "Visual - Typography", "Home Icon Size", DefaultHomeIconSize, 12f, 36f,
            "Size of Valheim's native favorite-star icon beside the selected Home portal.");
        titleColor = BindColor(config, "Visual - Typography", "Title Color", DefaultTitleColor,
            "Heading color as #RRGGBB or #RRGGBBAA.");
        entryColor = BindColor(config, "Visual - Typography", "Entry Color", DefaultEntryColor,
            "Destination text color as #RRGGBB or #RRGGBBAA.");
        hintColor = BindColor(config, "Visual - Typography", "Hint Color", DefaultHintColor,
            "Instruction text color as #RRGGBB or #RRGGBBAA.");
        biomeColor = BindColor(config, "Visual - Typography", "Biome Color", DefaultBiomeColor,
            "Biome subtitle color as #RRGGBB or #RRGGBBAA.");
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
        rowKnotGap = BindRange(config, "Visual - Ornaments", "Row Knot Gap", DefaultRowKnotGap, 0f, 100f,
            "Horizontal gap between the rendered destination label and each native knot mark.");

        fadeInSeconds = BindRange(config, "Visual - Animation", "Fade In Duration", DefaultFadeInSeconds, .01f, 1f,
            "Seconds used for the selector fade-in.");
        fadeOutSeconds = BindRange(config, "Visual - Animation", "Fade Out Duration", DefaultFadeOutSeconds, .01f, 1f,
            "Seconds used for the selector fade-out.");
        triggerMultiplier = BindRange(config, "Portal Interaction", "Portal Trigger Multiplier", DefaultTriggerMultiplier, 1f, 2f,
            "Client-local trigger and open-menu range multiplier. The server enforces a fixed 9.375-metre safety limit.");
        showBiomeSubtitle = config.Bind("Portal List", "Show Biome Subtitle", true,
            "Show the portal's biome in smaller white text beneath its name.");
        preferHomeOnOpen = config.Bind("Portal List", "Prefer Home On Open", true,
            "Select and scroll to the Home portal when the list opens.");
        showControlHints = config.Bind("Portal List", "Show Control Hints", true,
            "Show short keyboard or controller instructions above the destination list.");
        setHomeKeyboardShortcut = config.Bind("Portal List", "Set Home Keyboard Shortcut", new KeyboardShortcut(KeyCode.H),
            "Keyboard shortcut that sets or clears Home on the selected destination.");

        MigrateLegacyVisualDefaults(config);

        Watch(titleFontSize); Watch(entryFontSize); Watch(hintFontSize); Watch(biomeFontSize); Watch(homeIconSize);
        Watch(titleColor); Watch(entryColor); Watch(hintColor); Watch(biomeColor);
        Watch(outlineColor); Watch(outlineWidth); Watch(hoverScale); Watch(hoverExpandSeconds); Watch(hoverContractSeconds);
        Watch(panelWidth); Watch(panelYOffset); Watch(rowHeight); Watch(rowSpacing); Watch(maxListHeight); Watch(backdropOpacity);
        Watch(ornamentColor); Watch(showMenuOrnament); Watch(showRowKnots); Watch(rowKnotGap); Watch(fadeInSeconds); Watch(fadeOutSeconds);
        Watch(triggerMultiplier); Watch(showBiomeSubtitle); Watch(preferHomeOnOpen);
        Watch(showControlHints); Watch(setHomeKeyboardShortcut);
    }

    private static void MigrateLegacyVisualDefaults(ConfigFile config)
    {
        var changed = false;
        if (Mathf.Approximately(biomeFontSize.Value, 15.5f)) { biomeFontSize.Value = DefaultBiomeFontSize; changed = true; }
        if (Mathf.Approximately(homeIconSize.Value, 22f)) { homeIconSize.Value = DefaultHomeIconSize; changed = true; }
        if (Mathf.Approximately(panelYOffset.Value, 120f)) { panelYOffset.Value = DefaultPanelYOffset; changed = true; }
        if (Mathf.Approximately(triggerMultiplier.Value, 1.25f)) { triggerMultiplier.Value = DefaultTriggerMultiplier; changed = true; }
        if (string.Equals(entryColor.Value?.Trim(), "#DBDBDBFF", StringComparison.OrdinalIgnoreCase))
        {
            entryColor.Value = DefaultEntryColor;
            changed = true;
        }
        if (changed) config.Save();
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
    internal static float BiomeFontSize => Clamp(biomeFontSize, DefaultBiomeFontSize, 8f, 48f);
    internal static float HomeIconSize => Clamp(homeIconSize, DefaultHomeIconSize, 12f, 36f);
    internal static Color TitleColor => ParseColor(titleColor, DefaultTitleColor);
    internal static Color EntryColor => ParseColor(entryColor, DefaultEntryColor);
    internal static Color HintColor => ParseColor(hintColor, DefaultHintColor);
    internal static Color BiomeColor => ParseColor(biomeColor, DefaultBiomeColor);
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
    internal static float RowKnotGap => Clamp(rowKnotGap, DefaultRowKnotGap, 0f, 100f);
    internal static float FadeInSeconds => Clamp(fadeInSeconds, DefaultFadeInSeconds, .01f, 1f);
    internal static float FadeOutSeconds => Clamp(fadeOutSeconds, DefaultFadeOutSeconds, .01f, 1f);
    internal static float TriggerMultiplier => Clamp(triggerMultiplier, DefaultTriggerMultiplier, 1f, 2f);
    internal static bool ShowBiomeSubtitle => showBiomeSubtitle?.Value ?? true;
    internal static bool PreferHomeOnOpen => preferHomeOnOpen?.Value ?? true;
    internal static bool ShowControlHints => showControlHints?.Value ?? true;
    internal static KeyboardShortcut SetHomeKeyboardShortcut => setHomeKeyboardShortcut?.Value ?? new KeyboardShortcut(KeyCode.H);
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

internal static class PortalHome
{
    private const string KeyPrefix = Plugin.Guid + ".home.v1.";

    private static string CurrentKey()
    {
        var world = ZNet.instance;
        return world == null ? null : KeyPrefix + world.GetWorldUID();
    }

    internal static bool TryGet(out ZDOID id)
    {
        id = ZDOID.None;
        var player = Player.m_localPlayer;
        var key = CurrentKey();
        if (!player || string.IsNullOrEmpty(key) || player.m_customData == null || !player.m_customData.TryGetValue(key, out var raw)) return false;
        var parts = raw.Split(':');
        if (parts.Length != 2 ||
            !long.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var user) ||
            !uint.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)) return false;
        id = new ZDOID(user, value);
        return id != ZDOID.None;
    }

    internal static bool Set(ZDOID id)
    {
        var player = Player.m_localPlayer;
        var key = CurrentKey();
        if (!player || string.IsNullOrEmpty(key) || id == ZDOID.None) return false;
        player.m_customData ??= new Dictionary<string, string>();
        player.m_customData[key] = id.UserID.ToString(CultureInfo.InvariantCulture) + ":" + id.ID.ToString(CultureInfo.InvariantCulture);
        return true;
    }

    internal static bool Clear()
    {
        var player = Player.m_localPlayer;
        var key = CurrentKey();
        return player && !string.IsNullOrEmpty(key) && player.m_customData != null && player.m_customData.Remove(key);
    }

    // Player.Save serializes m_customData during normal autosave and logout.
    // Forcing SavePlayerProfile here also serializes the map and synchronizes
    // cloud saves/achievements on the UI thread, causing a hitch on each click.
}

internal static class PortalBiomes
{
    internal static Heightmap.Biome Normalize(int value)
    {
        var biome = (Heightmap.Biome)value;
        return Rank(biome) < 9 ? biome : Heightmap.Biome.None;
    }

    internal static int Rank(Heightmap.Biome biome) => biome switch
    {
        Heightmap.Biome.Meadows => 0,
        Heightmap.Biome.BlackForest => 1,
        Heightmap.Biome.Swamp => 2,
        Heightmap.Biome.Mountain => 3,
        Heightmap.Biome.Plains => 4,
        Heightmap.Biome.Mistlands => 5,
        Heightmap.Biome.AshLands => 6,
        Heightmap.Biome.DeepNorth => 7,
        Heightmap.Biome.Ocean => 8,
        _ => 9
    };

    internal static string DisplayName(Heightmap.Biome biome) => biome switch
    {
        Heightmap.Biome.Meadows => "Meadows",
        Heightmap.Biome.BlackForest => "Black Forest",
        Heightmap.Biome.Swamp => "Swamp",
        Heightmap.Biome.Mountain => "Mountains",
        Heightmap.Biome.Plains => "Plains",
        Heightmap.Biome.Mistlands => "Mistlands",
        Heightmap.Biome.AshLands => "Ashlands",
        Heightmap.Biome.DeepNorth => "Deep North",
        Heightmap.Biome.Ocean => "Ocean",
        _ => "Other"
    };
}

internal static class PortalOrderLedger
{
    private static readonly Dictionary<ZDOID, long> Orders = new();
    private static long loadedWorld;
    private static long nextOrder = 1L;
    private static bool initialized;
    private static bool loadingWorld;

    internal static void BeginWorldLoad() => loadingWorld = true;

    internal static void EndWorldLoad(ZDOMan manager)
    {
        if (!loadingWorld) return;
        loadingWorld = false;
        if (manager != null) Ensure(manager.GetPortalList());
    }

    internal static void PrepareObservation()
    {
        if (loadingWorld || initialized || ZNet.instance == null || !ZNet.instance.IsServer()) return;
        Ensure(ZDOMan.instance?.GetPortalList() ?? Enumerable.Empty<ZDO>());
    }

    internal static void Ensure(IEnumerable<ZDO> portals)
    {
        if (ZNet.instance == null || !ZNet.instance.IsServer()) return;
        LoadCurrentWorld();
        var changed = false;
        foreach (var id in portals.Where(z => z != null && z.IsValid()).Select(z => z.m_uid)
                     .Distinct().OrderBy(id => unchecked((ulong)id.UserID)).ThenBy(id => id.ID))
        {
            if (Orders.ContainsKey(id)) continue;
            Orders[id] = nextOrder++;
            changed = true;
        }
        initialized = true;
        if (changed) Save();
    }

    internal static long Get(ZDOID id) => Orders.TryGetValue(id, out var order) ? order : long.MaxValue;

    internal static void Observe(ZDO zdo)
    {
        if (loadingWorld || !initialized || ZNet.instance == null || !ZNet.instance.IsServer() || zdo == null || !zdo.IsValid()) return;
        if (ZNet.instance.GetWorldUID() != loadedWorld)
        {
            initialized = false;
            return;
        }
        if (Orders.ContainsKey(zdo.m_uid)) return;
        Orders[zdo.m_uid] = nextOrder++;
        Save();
    }

    private static void LoadCurrentWorld()
    {
        var world = ZNet.instance.GetWorldUID();
        if (initialized && loadedWorld == world) return;
        loadedWorld = world;
        nextOrder = 1L;
        initialized = false;
        Orders.Clear();
        try
        {
            var path = CurrentPath();
            if (!File.Exists(path)) return;
            foreach (var line in File.ReadAllLines(path))
            {
                var parts = line.Split('\t');
                if (parts.Length != 3 ||
                    !long.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var user) ||
                    !uint.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) ||
                    !long.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var order) || order <= 0L) continue;
                Orders[new ZDOID(user, id)] = order;
                nextOrder = Math.Max(nextOrder, order + 1L);
            }
        }
        catch (Exception error)
        {
            Plugin.LogDebug($"Could not read portal creation-order ledger: {error.GetType().Name}");
        }
    }

    private static string CurrentPath() => Path.Combine(Paths.ConfigPath, $"BetterPortals.portal-order.{loadedWorld}.tsv");

    private static void Save()
    {
        var path = CurrentPath();
        var temporary = path + ".tmp";
        try
        {
            File.WriteAllLines(temporary, Orders.OrderBy(pair => pair.Value).Select(pair =>
                pair.Key.UserID.ToString(CultureInfo.InvariantCulture) + "\t" +
                pair.Key.ID.ToString(CultureInfo.InvariantCulture) + "\t" +
                pair.Value.ToString(CultureInfo.InvariantCulture)));
            if (File.Exists(path))
            {
                try { File.Replace(temporary, path, null); }
                catch (PlatformNotSupportedException) { File.Delete(path); File.Move(temporary, path); }
            }
            else File.Move(temporary, path);
        }
        catch (Exception error)
        {
            Plugin.LogDebug($"Could not save portal creation-order ledger: {error.GetType().Name}");
        }
        finally
        {
            try { if (File.Exists(temporary)) File.Delete(temporary); }
            catch { }
        }
    }
}

[HarmonyPatch(typeof(ZDOMan), nameof(ZDOMan.Load))]
internal static class PortalOrderLoadPatch
{
    private static void Prefix() => PortalOrderLedger.BeginWorldLoad();
    private static void Postfix(ZDOMan __instance) => PortalOrderLedger.EndWorldLoad(__instance);
    private static Exception Finalizer(Exception __exception, ZDOMan __instance)
    {
        PortalOrderLedger.EndWorldLoad(__instance);
        return __exception;
    }
}

[HarmonyPatch(typeof(ZDOMan), "AddIfPortal")]
internal static class PortalOrderObservationPatch
{
    private static void Prefix() => PortalOrderLedger.PrepareObservation();
    private static void Postfix(ZDO __0) => PortalOrderLedger.Observe(__0);
}

internal static class PortalRanges
{
    internal const float MaximumServerDistance = 9.375f;
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
        if (!PortalController.WantsCursor) return true;
        PortalController.ApplySelectorCursor(false);
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

[HarmonyPatch(typeof(Chat), "Update")]
internal static class PortalChatInputPatch
{
    private static bool Prepare() => !Application.isBatchMode;

    // Unity exposes Return to every Update method for the entire frame. Pause
    // Chat while this selector owns input so one press cannot travel and open
    // the chat field at the same time.
    private static bool Prefix() => Application.isBatchMode || !PortalController.CapturesInput;
}

[HarmonyPatch(typeof(Chat), "InputText")]
internal static class PortalChatOpenPatch
{
    private static bool Prepare() => !Application.isBatchMode;

    // Blocking the actual focus/open method makes Enter suppression independent
    // of whether Chat.Update runs before or after PortalController.Update.
    private static bool Prefix() => Application.isBatchMode || !PortalController.CapturesInput;
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
    private const int ProtocolVersion = 2;
    private const string DirectoryRequest = Plugin.Guid + ".v2.directory.request";
    private const string DirectoryResponse = Plugin.Guid + ".v2.directory.response";
    private const string TravelRequest = Plugin.Guid + ".v2.travel.request";
    private const string TravelResponse = Plugin.Guid + ".v2.travel.response";
    private const float ServerSessionLifetime = 60f;
    private static readonly Dictionary<long, RequestWindow> Windows = new();
    private static readonly Dictionary<long, ServerSession> Sessions = new();
    private static ZRoutedRpc registeredOn;

    internal static bool Register()
    {
        var rpc = ZRoutedRpc.instance;
        if (rpc == null) return false;
        if (ReferenceEquals(registeredOn, rpc)) return true;
        rpc.Register<ZPackage>(DirectoryRequest, OnDirectoryRequest);
        rpc.Register<ZPackage>(DirectoryResponse, OnDirectoryResponse);
        rpc.Register<ZPackage>(TravelRequest, OnTravelRequest);
        rpc.Register<ZPackage>(TravelResponse, OnTravelResponse);
        registeredOn = rpc;
        return true;
    }

    internal static bool AskDirectory(ZDOID source, ZDOID savedHome, long nonce)
    {
        if (!Register() || !TryServerId(out var server)) return false;
        var package = new ZPackage();
        package.Write(ProtocolVersion);
        package.Write(source);
        package.Write(savedHome != ZDOID.None);
        if (savedHome != ZDOID.None) package.Write(savedHome);
        package.Write(nonce);
        ZRoutedRpc.instance.InvokeRoutedRPC(server, DirectoryRequest, package);
        return true;
    }

    internal static bool AskTravel(ZDOID source, ZDOID destination, long nonce)
    {
        if (!Register() || !TryServerId(out var server)) return false;
        var package = new ZPackage();
        package.Write(ProtocolVersion);
        package.Write(source);
        package.Write(destination);
        package.Write(nonce);
        ZRoutedRpc.instance.InvokeRoutedRPC(server, TravelRequest, package);
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
        var net = ZNet.instance;
        if (net == null || source == null) return false;
        if (net.IsServer() && sender == ZNet.GetUID())
        {
            var local = Player.m_localPlayer;
            return local && Vector3.Distance(local.transform.position, source.GetPosition()) <= PortalRanges.MaximumServerDistance;
        }

        var peer = net.GetPeer(sender);
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

    private static void OnDirectoryRequest(long sender, ZPackage request)
    {
        if (ZNet.instance == null || !ZNet.instance.IsServer()) return;
        try
        {
            if (request.ReadInt() != ProtocolVersion) return;
            var sourceId = request.ReadZDOID();
            var savedHome = request.ReadBool() ? request.ReadZDOID() : ZDOID.None;
            var nonce = request.ReadLong();
            if (!RateAllowed(sender) || !TryPortal(sourceId, out var source) || !NearSource(sender, source)) return;

            var generator = WorldGenerator.instance;
            var portalZdos = ZDOMan.instance.GetPortalList().Where(z => z != null && z.IsValid()).ToList();
            PortalOrderLedger.Ensure(portalZdos);
            var allPortals = portalZdos.Where(z => z.m_uid != sourceId)
                .Select(z => new PortalInfo(
                    z.m_uid,
                    PortalNames.Sanitize(z.GetString("tag")),
                    generator != null ? generator.GetBiome(z.GetPosition()) : Heightmap.Biome.None,
                    PortalOrderLedger.Get(z.m_uid)))
                .Where(p => !string.IsNullOrWhiteSpace(p.Name))
                .OrderBy(p => PortalBiomes.Rank(p.Biome))
                .ThenBy(p => p.CreationOrder)
                .ThenBy(p => unchecked((ulong)p.Id.UserID))
                .ThenBy(p => p.Id.ID)
                .ToList();

            var homeValid = savedHome != ZDOID.None && portalZdos.Any(zdo => zdo.m_uid == savedHome &&
                !string.IsNullOrWhiteSpace(PortalNames.Sanitize(zdo.GetString("tag"))));
            var portals = allPortals.Take(512).ToList();
            if (homeValid && savedHome != sourceId && !portals.Any(portal => portal.Id == savedHome))
            {
                portals[portals.Count - 1] = allPortals.First(portal => portal.Id == savedHome);
                portals = portals.OrderBy(p => PortalBiomes.Rank(p.Biome))
                    .ThenBy(p => p.CreationOrder)
                    .ThenBy(p => unchecked((ulong)p.Id.UserID))
                    .ThenBy(p => p.Id.ID).ToList();
            }

            Sessions[sender] = new ServerSession(sourceId, nonce, Time.realtimeSinceStartup + ServerSessionLifetime,
                portals.Select(portal => portal.Id));
            var response = new ZPackage();
            response.Write(ProtocolVersion);
            response.Write(nonce);
            response.Write(homeValid);
            response.Write(portals.Count);
            foreach (var portal in portals)
            {
                response.Write(portal.Id);
                response.Write(portal.Name);
                response.Write((int)portal.Biome);
                response.Write(portal.CreationOrder);
            }
            ZRoutedRpc.instance.InvokeRoutedRPC(sender, DirectoryResponse, response);
        }
        catch (Exception error)
        {
            Plugin.LogDebug($"Rejected malformed portal directory request: {error.GetType().Name}");
        }
    }

    private static void OnDirectoryResponse(long sender, ZPackage package)
    {
        if (!TryServerId(out var server) || sender != server) return;
        try
        {
            if (package.ReadInt() != ProtocolVersion) return;
            var nonce = package.ReadLong();
            var homeValid = package.ReadBool();
            var count = package.ReadInt();
            if (count < 0 || count > 512) throw new InvalidDataException("Portal directory count was outside the protocol bounds.");
            var list = new List<PortalInfo>(count);
            for (var i = 0; i < count; i++)
            {
                var id = package.ReadZDOID();
                var name = PortalNames.Sanitize(package.ReadString());
                var biome = PortalBiomes.Normalize(package.ReadInt());
                var creationOrder = package.ReadLong();
                if (id == ZDOID.None || string.IsNullOrWhiteSpace(name) || creationOrder <= 0L)
                    throw new InvalidDataException("Portal directory record was invalid.");
                list.Add(new PortalInfo(id, name, biome, creationOrder));
            }
            PortalController.Instance?.Show(nonce, homeValid, list);
        }
        catch (Exception error)
        {
            Plugin.LogDebug($"Rejected malformed portal directory response: {error.GetType().Name}");
        }
    }

    private static void OnTravelRequest(long sender, ZPackage request)
    {
        if (ZNet.instance == null || !ZNet.instance.IsServer()) return;
        try
        {
            if (request.ReadInt() != ProtocolVersion) return;
            var sourceId = request.ReadZDOID();
            var destinationId = request.ReadZDOID();
            var nonce = request.ReadLong();
            var validSession = Sessions.TryGetValue(sender, out var session) && session.Source == sourceId &&
                session.Nonce == nonce && session.Expires >= Time.realtimeSinceStartup && session.Destinations.Contains(destinationId);
            Sessions.Remove(sender);
            if (!RateAllowed(sender) || !validSession || !TryPortal(sourceId, out var source) || !NearSource(sender, source) ||
                !TryPortal(destinationId, out var destination) || destinationId == sourceId ||
                string.IsNullOrWhiteSpace(PortalNames.Sanitize(destination.GetString("tag"))))
            {
                SendTravelResponse(sender, nonce, false, null);
                return;
            }
            SendTravelResponse(sender, nonce, true, destination);
        }
        catch (Exception error)
        {
            Plugin.LogDebug($"Rejected malformed portal travel request: {error.GetType().Name}");
        }
    }

    private static void OnTravelResponse(long sender, ZPackage package)
    {
        if (!TryServerId(out var server) || sender != server) return;
        try
        {
            if (package.ReadInt() != ProtocolVersion) return;
            var nonce = package.ReadLong();
            var approved = package.ReadBool();
            PortalController.Instance?.Complete(nonce, approved, approved ? package.ReadVector3() : default,
                approved ? package.ReadQuaternion() : default);
        }
        catch (Exception error)
        {
            Plugin.LogDebug($"Rejected malformed portal travel response: {error.GetType().Name}");
        }
    }

    private static void SendTravelResponse(long sender, long nonce, bool approved, ZDO destination)
    {
        var response = new ZPackage();
        response.Write(ProtocolVersion);
        response.Write(nonce);
        response.Write(approved);
        if (approved && destination != null)
        {
            response.Write(destination.GetPosition());
            response.Write(destination.GetRotation());
        }
        ZRoutedRpc.instance.InvokeRoutedRPC(sender, TravelResponse, response);
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
        internal readonly HashSet<ZDOID> Destinations;
        internal ServerSession(ZDOID source, long nonce, float expires, IEnumerable<ZDOID> destinations)
        {
            Source = source;
            Nonce = nonce;
            Expires = expires;
            Destinations = new HashSet<ZDOID>(destinations);
        }
    }
}

internal readonly struct PortalInfo
{
    internal readonly ZDOID Id;
    internal readonly string Name;
    internal readonly Heightmap.Biome Biome;
    internal readonly long CreationOrder;
    internal PortalInfo(ZDOID id, string name, Heightmap.Biome biome, long creationOrder)
    {
        Id = id;
        Name = name;
        Biome = PortalBiomes.Normalize((int)biome);
        CreationOrder = Math.Max(1L, creationOrder);
    }
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
    internal static bool WantsCursor => Instance != null && Instance.panel && Instance.panel.activeSelf &&
        Instance.fadeTarget > 0f && !Instance.travelPending;

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
    private Button homeSelectable;
    private bool? previousMouseInput;
    private Vector3 openingMousePosition;
    private bool mouseChoiceArmed;
    private int cursorCenterFrames;
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
    private readonly List<PortalEntryHover> portalRows = new();
    private readonly List<PortalInfo> currentPortals = new();
    private static TMP_FontAsset nativeFont;
    private static Material nativeFontMaterial;
    private static Sprite nativeHomeSprite;
    private static int nativeHomeSpriteSearchFrame = -1;
    private static bool cursorWarpPrepared;
    private static System.Reflection.PropertyInfo inputMouseCurrentProperty;
    private static System.Reflection.MethodInfo inputMouseWarpMethod;

    private void Awake()
    {
        Instance = this;
        visualsDirty = true;
        PrepareCursorWarp();
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
        PortalHome.TryGet(out var savedHome);
        if (!PortalRpc.AskDirectory(source, savedHome, nonce))
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
        PortalHome.TryGet(out var savedHome);
        if (!PortalRpc.AskDirectory(source, savedHome, nonce)) nextDeferredAttempt = now + FirstEntryRetryInterval;
    }

    private void ClearDeferredBegin()
    {
        deferredPortal = null;
        deferredSource = ZDOID.None;
        deferredStarted = 0f;
        deferredDeadline = 0f;
        nextDeferredAttempt = 0f;
    }

    internal void Show(long responseNonce, bool homeValid, List<PortalInfo> portals)
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
        portalRows.Clear();
        currentPortals.Clear();
        firstSelectable = null;
        homeSelectable = null;
        currentPortals.AddRange(portals.OrderBy(p => PortalBiomes.Rank(p.Biome))
            .ThenBy(p => p.CreationOrder)
            .ThenBy(p => unchecked((ulong)p.Id.UserID))
            .ThenBy(p => p.Id.ID));
        var duplicateTotals = currentPortals.GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);
        var duplicateIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var hadSavedHome = PortalHome.TryGet(out var home);
        if (hadSavedHome && (!homeValid || (home != source && !currentPortals.Any(portal => portal.Id == home))))
        {
            PortalHome.Clear();
            home = ZDOID.None;
            ShowStatus("Home portal unavailable — choose a new Home");
        }
        foreach (var portal in currentPortals)
        {
            duplicateIndex.TryGetValue(portal.Name, out var index); duplicateIndex[portal.Name] = ++index;
            var label = duplicateTotals[portal.Name] > 1 ? $"{portal.Name}  ({index}/{duplicateTotals[portal.Name]})" : portal.Name;
            var destination = portal;
            var button = AddRow(label, PortalBiomes.DisplayName(portal.Biome), portal.Id,
                () => ActivateDestination(destination.Id), () => ToggleHome(destination), portal.Id == home);
            if (!firstSelectable) firstSelectable = button;
            if (portal.Id == home) homeSelectable = button;
        }
        if (currentPortals.Count == 0) AddRow("No named destinations found", string.Empty, ZDOID.None, null, null, false);

        ConfigureRowNavigation();
        UpdateHintText();
        ApplyVisualSettings();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        fadeTarget = 1f;
        resetAfterFade = false;
        panel.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)panel.transform);
        previousMouseInput = null;
        mouseChoiceArmed = false;
        SynchronizeInputSelection(true);
        if (homeSelectable && PortalVisualSettings.PreferHomeOnOpen) ScrollIntoView(homeSelectable.gameObject);
    }

    internal void Complete(long responseNonce, bool approved, Vector3 position, Quaternion rotation)
    {
        if (responseNonce != nonce) return;
        travelPending = false;
        if (!approved || Player.m_localPlayer == null)
        {
            RequestClose(true);
            return;
        }
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
        // Build the static selector shell while the main menu is already loaded,
        // keeping native asset discovery out of the first portal-entry frame.
        if (!panel && Menu.instance && Menu.instance.m_continueButton) EnsureUi();
        AdvanceDeferredBegin();
        if (visualsDirty) ApplyVisualSettings();
        if (nonce != 0 && !SourceIsUsable()) RequestClose(true);
        if (panel && panel.activeSelf)
        {
            SynchronizeInputSelection();
            AdvanceCursorCentering();
            ArmMouseChoiceAfterMovement();
            if (PortalVisualSettings.SetHomeKeyboardShortcut.IsDown() || ZInput.GetButtonDown("JoyButtonX")) ToggleSelectedHome();
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || ZInput.GetButtonDown("JoyButtonA")) ConfirmSelected();
            else if (Input.GetKeyDown(KeyCode.Escape) || ZInput.GetButtonDown("JoyButtonB") || AnotherModalIsVisible()) RequestClose(true);
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
        var canvasRect = canvasObject.GetComponent<RectTransform>();
        if (canvasRect)
        {
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.pivot = new Vector2(.5f, .5f);
            canvasRect.anchoredPosition = Vector2.zero;
            canvasRect.sizeDelta = Vector2.zero;
        }
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
        contentLayout.childControlWidth = true;
        contentLayout.childForceExpandHeight = false;
        contentLayout.childForceExpandWidth = true;
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
        UpdateHintText();

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
        var visibleRows = rows.Where(row => row && row.activeSelf).ToList();
        var count = Mathf.Max(1, visibleRows.Count);
        var contentHeight = visibleRows.Sum(row => row.GetComponent<LayoutElement>()?.preferredHeight ?? PortalVisualSettings.RowHeight) +
            Mathf.Max(0, count - 1) * PortalVisualSettings.RowSpacing;
        var minimum = Mathf.Min(PortalVisualSettings.MaxListHeight, Mathf.Max(54f, PortalVisualSettings.RowHeight));
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

    private void SynchronizeInputSelection(bool centerMouse = false)
    {
        var eventSystem = EventSystem.current;
        if (!eventSystem) return;
        var mouseInput = ZInput.IsMouseActive();
        if (previousMouseInput.HasValue && previousMouseInput.Value == mouseInput) return;
        var changedToMouse = previousMouseInput.HasValue && !previousMouseInput.Value && mouseInput;
        previousMouseInput = mouseInput;

        var selected = eventSystem.currentSelectedGameObject;
        if (mouseInput)
        {
            ApplySelectorCursor(centerMouse || changedToMouse);
            if (centerMouse || changedToMouse)
            {
                cursorCenterFrames = 1;
                openingMousePosition = new Vector3(Screen.width * .5f, Screen.height * .5f, 0f);
                mouseChoiceArmed = false;
            }
            if (selected && panel && selected.transform.IsChildOf(panel.transform)) eventSystem.SetSelectedGameObject(null);
        }
        else if (firstSelectable)
        {
            ApplySelectorCursor(false);
            var preferred = PortalVisualSettings.PreferHomeOnOpen && homeSelectable ? homeSelectable : firstSelectable;
            eventSystem.SetSelectedGameObject(preferred.gameObject);
            ScrollIntoView(preferred.gameObject);
        }
        UpdateHintText();
    }

    private void AdvanceCursorCentering()
    {
        if (cursorCenterFrames <= 0 || !ZInput.IsMouseActive()) return;
        ApplySelectorCursor(true);
        cursorCenterFrames--;
        if (cursorCenterFrames == 0) openingMousePosition = Input.mousePosition;
    }

    private void ArmMouseChoiceAfterMovement()
    {
        if (mouseChoiceArmed || cursorCenterFrames > 0 || !ZInput.IsMouseActive()) return;
        if ((Input.mousePosition - openingMousePosition).sqrMagnitude > 9f) mouseChoiceArmed = true;
    }

    private void KeepControllerSelectionVisible()
    {
        if (ZInput.IsMouseActive() || !destinationScroll || !destinationScroll.viewport || !destinationScroll.content) return;
        var selected = EventSystem.current?.currentSelectedGameObject;
        ScrollIntoView(selected);
    }

    private void ScrollIntoView(GameObject selected)
    {
        if (!selected || !destinationScroll || !destinationScroll.viewport || !destinationScroll.content ||
            !selected.transform.IsChildOf(content)) return;

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

    private void ToggleHome(PortalInfo portal)
    {
        var clearing = PortalHome.TryGet(out var current) && current == portal.Id;
        var changed = clearing ? PortalHome.Clear() : PortalHome.Set(portal.Id);
        if (!changed) return;
        foreach (var row in portalRows) row.SetHome(!clearing && row.PortalId == portal.Id);
        homeSelectable = clearing ? null : portalRows.FirstOrDefault(row => row.PortalId == portal.Id)?.GetComponent<Button>();
        UpdateHintText();
        Plugin.LogDebug(clearing ? "Home portal cleared." : $"Home portal set to {portal.Name}.");
    }

    private PortalEntryHover SelectedPortalRow()
    {
        var selected = EventSystem.current?.currentSelectedGameObject;
        var selectedRow = selected?.GetComponent<PortalEntryHover>();
        return selectedRow ?? portalRows.FirstOrDefault(row => row.PointerInside) ??
            (PortalVisualSettings.PreferHomeOnOpen && homeSelectable ? homeSelectable.GetComponent<PortalEntryHover>() :
                firstSelectable?.GetComponent<PortalEntryHover>());
    }

    private void ToggleSelectedHome()
    {
        SelectedPortalRow()?.SetAsHome();
    }

    private void ConfirmSelected()
    {
        if (travelPending) return;
        var row = ZInput.IsMouseActive() && !mouseChoiceArmed && homeSelectable
            ? homeSelectable.GetComponent<PortalEntryHover>()
            : SelectedPortalRow();
        row?.Activate();
    }

    private void ActivateDestination(ZDOID destination)
    {
        SelectDestination(destination);
    }

    private void UpdateHintText()
    {
        if (!hintText) return;
        hintText.gameObject.SetActive(PortalVisualSettings.ShowControlHints);
        if (!PortalVisualSettings.ShowControlHints) return;
        hintText.text = ZInput.IsMouseActive()
            ? $"Enter Travel     {PortalVisualSettings.SetHomeKeyboardShortcut} Set Home     Esc Close"
            : "A Travel     X Set Home     B Close";
    }

    private static void ShowStatus(string message)
    {
        var hud = MessageHud.instance;
        if (hud) hud.ShowMessage(MessageHud.MessageType.TopLeft, message, 0, ResolveHomeSprite(), false, false);
    }

    internal static void ApplySelectorCursor(bool center)
    {
        if (Application.isBatchMode) return;
        if (!ZInput.IsMouseActive())
        {
            if (ZCursor.IsVisible) ZCursor.Hide();
            ZCursor.LockState = CursorLockMode.Locked;
            return;
        }

        ZCursor.LockState = CursorLockMode.None;
        if (!ZCursor.IsVisible) ZCursor.Show();
        if (center) CenterPointer();
    }

    private static void PrepareCursorWarp()
    {
        if (cursorWarpPrepared) return;
        cursorWarpPrepared = true;
        try
        {
            var mouseType = AccessTools.TypeByName("UnityEngine.InputSystem.Mouse");
            if (mouseType == null) return;
            inputMouseCurrentProperty = AccessTools.Property(mouseType, "current");
            inputMouseWarpMethod = AccessTools.Method(mouseType, "WarpCursorPosition", new[] { typeof(Vector2) });
        }
        catch (Exception error)
        {
            Plugin.LogDebug($"Input-system cursor warp preparation failed: {error.GetType().Name}");
        }
    }

    private static void CenterPointer()
    {
        var center = new Vector2(Screen.width * .5f, Screen.height * .5f);
        try
        {
            PrepareCursorWarp();
            var current = inputMouseCurrentProperty?.GetValue(null, null);
            if (current != null && inputMouseWarpMethod != null)
            {
                inputMouseWarpMethod.Invoke(current, new object[] { center });
                return;
            }
        }
        catch (Exception error)
        {
            Plugin.LogDebug($"Input-system cursor centering fell back to lock/unlock: {error.GetType().Name}");
        }

        // Locking briefly is Unity's platform-neutral legacy cursor warp.
        ZCursor.LockState = CursorLockMode.Locked;
        ZCursor.LockState = CursorLockMode.None;
        if (!ZCursor.IsVisible) ZCursor.Show();
    }

    private static void ReleaseSelectorCursor()
    {
        if (Application.isBatchMode) return;
        if (ZCursor.IsVisible) ZCursor.Hide();
        ZCursor.LockState = CursorLockMode.Locked;
        GameCamera.instance?.UpdateMouseCapture();
    }

    internal static Sprite ResolveHomeSprite()
    {
        if (nativeHomeSprite) return nativeHomeSprite;
        if (nativeHomeSpriteSearchFrame == Time.frameCount) return null;
        nativeHomeSpriteSearchFrame = Time.frameCount;
        foreach (var button in Resources.FindObjectsOfTypeAll<BuildUiPieceButton>())
        {
            if (!button) continue;
            var favorite = Traverse.Create(button).Field<Image>("m_favoriteStar").Value;
            if (favorite && favorite.sprite)
            {
                nativeHomeSprite = favorite.sprite;
                return nativeHomeSprite;
            }
        }
        return null;
    }

    private Button AddRow(string label, string biome, ZDOID portalId, Action action, Action setHome, bool isHome)
    {
        var template = Menu.instance?.m_continueButton;
        if (!template) return AddFallbackRow(label, biome, portalId, action, setHome, isHome);

        var row = UnityEngine.Object.Instantiate(template.gameObject, content, false);
        row.name = label;
        row.SetActive(false);
        NormalizeRowRect(row);
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
        rowLayout.minWidth = 0f;
        rowLayout.preferredWidth = -1f;
        rowLayout.flexibleWidth = 1f;

        var nativeText = row.GetComponentInChildren<TMP_Text>(true);
        if (!nativeText) { Destroy(row); return AddFallbackRow(label, biome, portalId, action, setHome, isHome); }
        nativeText.gameObject.SetActive(false);
        var labelRoot = AddLabelGroup(row.transform, label, biome, out var text, out var biomeText, out var homeIcons, out var homeFallbacks);
        var nativeKnots = row.GetComponentsInChildren<Image>(true)
            .Where(image => image && image != background && !homeIcons.Contains(image) && image.name.IndexOf("Knot", StringComparison.OrdinalIgnoreCase) >= 0)
            .ToArray();
        var knots = CreateNormalizedRowKnots(row.transform, nativeKnots);
        var hover = row.GetComponent<PortalEntryHover>() ?? row.AddComponent<PortalEntryHover>();
        hover.Initialize(text, biomeText, labelRoot, background, knots, homeIcons, homeFallbacks, portalId, action != null, setHome, isHome);
        rows.Add(row);
        if (portalId != ZDOID.None) portalRows.Add(hover);
        row.SetActive(true);
        return button;
    }

    private Button AddFallbackRow(string label, string biome, ZDOID portalId, Action action, Action setHome, bool isHome)
    {
        var row = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement), typeof(PortalEntryHover));
        row.transform.SetParent(content, false);
        NormalizeRowRect(row);
        var rowLayout = row.GetComponent<LayoutElement>();
        rowLayout.preferredHeight = PortalVisualSettings.RowHeight;
        rowLayout.minWidth = 0f;
        rowLayout.preferredWidth = -1f;
        rowLayout.flexibleWidth = 1f;
        var background = row.GetComponent<Image>();
        background.color = Color.clear;
        background.raycastTarget = true;
        var labelRoot = AddLabelGroup(row.transform, label, biome, out var text, out var biomeText, out var homeIcons, out var homeFallbacks);
        var button = row.GetComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.interactable = action != null;
        if (action != null) button.onClick.AddListener(() => action());
        var hover = row.GetComponent<PortalEntryHover>();
        hover.Initialize(text, biomeText, labelRoot, background, Array.Empty<Image>(), homeIcons, homeFallbacks, portalId, action != null, setHome, isHome);
        rows.Add(row);
        if (portalId != ZDOID.None) portalRows.Add(hover);
        return button;
    }

    private static Transform AddLabelGroup(Transform parent, string label, string biome, out TMP_Text nameText, out TMP_Text biomeText,
        out Image[] homeIcons, out TMP_Text[] homeFallbacks)
    {
        var group = new GameObject("BetterPortals Label Group", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter), typeof(LayoutElement));
        group.transform.SetParent(parent, false);
        group.GetComponent<LayoutElement>().ignoreLayout = true;
        var rect = (RectTransform)group.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
        rect.pivot = new Vector2(.5f, .5f);
        rect.anchoredPosition = Vector2.zero;
        var layout = group.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 8f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        var fitter = group.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var leftHomeIcon = AddHomeIcon(group.transform, "Left");
        var leftHomeFallback = AddText(group.transform, "★", PortalVisualSettings.HomeIconSize, FontStyles.Normal,
            PortalVisualSettings.EntryColor);
        leftHomeFallback.name = "Left Home Favorite Star Fallback";
        leftHomeFallback.gameObject.SetActive(false);

        var textStack = new GameObject("Portal Name and Biome", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        textStack.transform.SetParent(group.transform, false);
        var textLayout = textStack.GetComponent<VerticalLayoutGroup>();
        textLayout.spacing = 0f;
        textLayout.childAlignment = TextAnchor.MiddleCenter;
        textLayout.childControlWidth = true;
        textLayout.childControlHeight = true;
        textLayout.childForceExpandWidth = false;
        textLayout.childForceExpandHeight = false;
        var textFitter = textStack.GetComponent<ContentSizeFitter>();
        textFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        textFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        nameText = AddText(textStack.transform, label, PortalVisualSettings.EntryFontSize, FontStyles.Normal, PortalVisualSettings.EntryColor);
        biomeText = AddText(textStack.transform, string.IsNullOrWhiteSpace(biome) ? string.Empty : $"({biome})",
            PortalVisualSettings.BiomeFontSize, FontStyles.Normal, PortalVisualSettings.BiomeColor);

        var rightHomeIcon = AddHomeIcon(group.transform, "Right");
        var rightHomeFallback = AddText(group.transform, "★", PortalVisualSettings.HomeIconSize, FontStyles.Normal,
            PortalVisualSettings.EntryColor);
        rightHomeFallback.name = "Right Home Favorite Star Fallback";
        rightHomeFallback.gameObject.SetActive(false);
        homeIcons = new[] { leftHomeIcon, rightHomeIcon };
        homeFallbacks = new[] { leftHomeFallback, rightHomeFallback };
        return group.transform;
    }

    private static Image AddHomeIcon(Transform parent, string side)
    {
        var iconObject = new GameObject($"{side} Home Favorite Star", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        iconObject.transform.SetParent(parent, false);
        var icon = iconObject.GetComponent<Image>();
        icon.sprite = ResolveHomeSprite();
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        icon.color = Color.white;
        return icon;
    }

    private static void NormalizeRowRect(GameObject row)
    {
        // Continue carries a HorizontalLayoutGroup. Disabling only its fitter
        // leaves that group moving our label after every layout rebuild.
        foreach (var inheritedLayout in row.GetComponents<LayoutGroup>()) inheritedLayout.enabled = false;
        var aspectFitter = row.GetComponent<AspectRatioFitter>();
        if (aspectFitter) aspectFitter.enabled = false;
        var rect = (RectTransform)row.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1f);
        rect.pivot = new Vector2(.5f, .5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0f, PortalVisualSettings.RowHeight);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        var fitter = row.GetComponent<ContentSizeFitter>();
        if (fitter) fitter.enabled = false;
    }

    private static Image[] CreateNormalizedRowKnots(Transform row, Image[] nativeKnots)
    {
        var candidates = (nativeKnots ?? Array.Empty<Image>()).Where(image => image && image.sprite).ToArray();
        if (candidates.Length == 0) return Array.Empty<Image>();

        var ordered = candidates.OrderBy(image => row.InverseTransformPoint(image.rectTransform.position).x).ToArray();
        var left = candidates.FirstOrDefault(image => image.name.IndexOf("left", StringComparison.OrdinalIgnoreCase) >= 0) ?? ordered.First();
        var right = candidates.FirstOrDefault(image => image != left && image.name.IndexOf("right", StringComparison.OrdinalIgnoreCase) >= 0) ??
            ordered.LastOrDefault(image => image != left) ?? left;
        foreach (var native in nativeKnots)
        {
            if (!native) continue;
            native.enabled = false;
            native.raycastTarget = false;
            native.gameObject.SetActive(false);
        }

        return new[]
        {
            CreateRowKnot(row, "BetterPortals Left Knot", left.sprite),
            CreateRowKnot(row, "BetterPortals Right Knot", right.sprite)
        };
    }

    private static Image CreateRowKnot(Transform parent, string name, Sprite sprite)
    {
        var knotObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        knotObject.transform.SetParent(parent, false);
        knotObject.GetComponent<LayoutElement>().ignoreLayout = true;
        var knot = knotObject.GetComponent<Image>();
        knot.sprite = sprite;
        knot.type = Image.Type.Simple;
        knot.preserveAspect = true;
        knot.raycastTarget = false;
        var color = PortalVisualSettings.OrnamentColor;
        knot.color = new Color(color.r, color.g, color.b, 0f);
        return knot;
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
            scaler.referenceResolution = native.referenceResolution;
            scaler.screenMatchMode = native.screenMatchMode;
            scaler.matchWidthOrHeight = native.matchWidthOrHeight;
            scaler.referencePixelsPerUnit = native.referencePixelsPerUnit;
            scaler.dynamicPixelsPerUnit = native.dynamicPixelsPerUnit;
        }

        // Menu's GuiScaler recalculates scale from resolution and the player's
        // GUI Scale setting. A snapshot taken during prewarm is still 1 and
        // stays tiny at high resolutions. Use the same live scaler as Menu.
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        if (!scaler.GetComponent<GuiScaler>()) scaler.gameObject.AddComponent<GuiScaler>();
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
        cursorCenterFrames = 0;
        ReleaseSelectorCursor();
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
        mouseChoiceArmed = false;
        ReleaseSelectorCursor();
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

internal sealed class PortalEntryHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler
{
    private TMP_Text label;
    private TMP_Text biomeLabel;
    private Transform visualRoot;
    private Image background;
    private Image[] knots = Array.Empty<Image>();
    private float[] knotSides = Array.Empty<float>();
    private Image[] homeIcons = Array.Empty<Image>();
    private TMP_Text[] homeFallbacks = Array.Empty<TMP_Text>();
    private LayoutElement[] homeIconLayouts = Array.Empty<LayoutElement>();
    private LayoutElement rowLayout;
    private Color normalColor;
    private Action setHomeAction;
    private bool isHome;
    private bool interactive;
    private bool selected;
    private bool pointerInside;
    private bool animating;
    private bool knotsVisible;
    private float currentScale = 1f;
    private float startScale = 1f;
    private float targetScale = 1f;
    private float transitionStarted;
    private float transitionDuration;
    private readonly Vector3[] rectCorners = new Vector3[4];

    internal ZDOID PortalId { get; private set; }
    internal bool PointerInside => pointerInside;

    internal void Initialize(TMP_Text rowLabel, TMP_Text rowBiomeLabel, Transform rowVisualRoot, Image rowBackground,
        Image[] rowKnots, Image[] rowHomeIcons, TMP_Text[] rowHomeFallbacks, ZDOID portalId, bool canInteract, Action onSetHome, bool home)
    {
        label = rowLabel;
        biomeLabel = rowBiomeLabel;
        visualRoot = rowVisualRoot;
        background = rowBackground;
        knots = rowKnots ?? Array.Empty<Image>();
        PrepareKnots();
        homeIcons = rowHomeIcons ?? Array.Empty<Image>();
        homeFallbacks = rowHomeFallbacks ?? Array.Empty<TMP_Text>();
        homeIconLayouts = homeIcons.Select(icon => icon ? icon.GetComponent<LayoutElement>() : null).ToArray();
        rowLayout = GetComponent<LayoutElement>();
        PortalId = portalId;
        setHomeAction = onSetHome;
        isHome = home;
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
        if (biomeLabel)
        {
            PortalController.ApplyTextAppearance(biomeLabel, PortalVisualSettings.BiomeFontSize, PortalVisualSettings.BiomeColor);
            biomeLabel.gameObject.SetActive(interactive && PortalVisualSettings.ShowBiomeSubtitle && !string.IsNullOrWhiteSpace(biomeLabel.text));
        }
        var homeSprite = PortalController.ResolveHomeSprite();
        for (var i = 0; i < homeIcons.Length; i++)
        {
            var homeIcon = homeIcons[i];
            if (!homeIcon) continue;
            if (!homeIcon.sprite) homeIcon.sprite = homeSprite;
            var size = PortalVisualSettings.HomeIconSize;
            var homeIconLayout = i < homeIconLayouts.Length ? homeIconLayouts[i] : null;
            if (homeIconLayout)
            {
                homeIconLayout.preferredWidth = size;
                homeIconLayout.preferredHeight = size;
            }
            homeIcon.gameObject.SetActive(isHome && homeIcon.sprite);
        }
        var hasNativeHomeStars = homeIcons.Any(icon => icon && icon.sprite);
        foreach (var homeFallback in homeFallbacks)
        {
            if (!homeFallback) continue;
            PortalController.ApplyTextAppearance(homeFallback, PortalVisualSettings.HomeIconSize, PortalVisualSettings.EntryColor);
            homeFallback.gameObject.SetActive(isHome && !hasNativeHomeStars);
        }
        background.color = Color.clear;
        if (rowLayout)
        {
            var stackedTextHeight = biomeLabel && biomeLabel.gameObject.activeSelf
                ? PortalVisualSettings.EntryFontSize + PortalVisualSettings.BiomeFontSize + 6f
                : PortalVisualSettings.EntryFontSize + 10f;
            rowLayout.preferredHeight = Mathf.Max(PortalVisualSettings.RowHeight, stackedTextHeight);
        }
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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!interactive || setHomeAction == null || eventData.button != PointerEventData.InputButton.Right) return;
        setHomeAction();
        eventData.Use();
    }

    internal void SetAsHome()
    {
        if (interactive) setHomeAction?.Invoke();
    }

    internal void Activate()
    {
        if (!interactive) return;
        GetComponent<Button>()?.onClick.Invoke();
    }

    internal void SetHome(bool home)
    {
        if (isHome == home) return;
        isHome = home;
        ApplyVisualSettings();
    }

    public void OnSelect(BaseEventData eventData) { selected = interactive; SetHighlighted(pointerInside || selected); }
    public void OnDeselect(BaseEventData eventData) { selected = false; SetHighlighted(pointerInside || selected); }

    private void Update()
    {
        if (!animating || !visualRoot) return;
        var progress = Mathf.Clamp01((Time.unscaledTime - transitionStarted) / Mathf.Max(.001f, transitionDuration));
        var eased = Mathf.SmoothStep(0f, 1f, progress);
        currentScale = Mathf.Lerp(startScale, targetScale, eased);
        ApplyScale(currentScale);
        if (progress >= 1f) animating = false;
    }

    private void LateUpdate()
    {
        if (knotsVisible) PositionKnots();
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
        if (visualRoot) visualRoot.localScale = Vector3.one * scale;
        if (knotsVisible) PositionKnots();
    }

    private void PrepareKnots()
    {
        knotSides = new float[knots.Length];
        for (var i = 0; i < knots.Length; i++)
        {
            var knot = knots[i];
            if (!knot) continue;
            var knotRect = knot.rectTransform;
            var name = knot.name ?? string.Empty;
            var side = name.IndexOf("left", StringComparison.OrdinalIgnoreCase) >= 0 ? -1f :
                name.IndexOf("right", StringComparison.OrdinalIgnoreCase) >= 0 ? 1f :
                i % 2 == 0 ? -1f : 1f;
            knotSides[i] = side;
            knotRect.SetParent(transform, false);
            knotRect.anchorMin = knotRect.anchorMax = new Vector2(.5f, .5f);
            knotRect.pivot = new Vector2(.5f, .5f);
            var spriteSize = knot.sprite ? knot.sprite.rect.size : new Vector2(36f, 18f);
            var aspect = spriteSize.y > .01f ? spriteSize.x / spriteSize.y : 2f;
            const float height = 18f;
            knotRect.sizeDelta = new Vector2(Mathf.Clamp(height * aspect, 18f, 64f), height);
            knot.type = Image.Type.Simple;
            knot.preserveAspect = true;
            knotRect.localScale = new Vector3(side < 0f ? 1f : -1f, 1f, 1f);
            knotRect.localRotation = Quaternion.identity;
            knotRect.SetAsLastSibling();
        }
    }

    private void PositionKnots()
    {
        if (knots.Length == 0 || !visualRoot) return;
        if (!TryGetRenderedTitleBounds(out var left, out var right, out var centerY)) return;

        for (var i = 0; i < knots.Length; i++)
        {
            var knot = knots[i];
            if (!knot) continue;
            var knotRect = knot.rectTransform;
            var side = i < knotSides.Length && knotSides[i] < 0f ? -1f : 1f;
            var halfWidth = Mathf.Abs(knotRect.rect.width) * .5f;
            var centerX = side < 0f
                ? left - PortalVisualSettings.RowKnotGap - halfWidth
                : right + PortalVisualSettings.RowKnotGap + halfWidth;
            knotRect.anchoredPosition = new Vector2(centerX, centerY);
        }
    }

    private bool TryGetRenderedTitleBounds(out float left, out float right, out float centerY)
    {
        left = float.PositiveInfinity;
        right = float.NegativeInfinity;
        var bottom = float.PositiveInfinity;
        var top = float.NegativeInfinity;

        foreach (var homeIcon in homeIcons)
        {
            if (homeIcon && homeIcon.gameObject.activeSelf)
                IncludeRectBounds(homeIcon.rectTransform, ref left, ref right, ref bottom, ref top);
        }
        foreach (var homeFallback in homeFallbacks)
        {
            if (homeFallback && homeFallback.gameObject.activeSelf)
                IncludeTextBounds(homeFallback, ref left, ref right, ref bottom, ref top);
        }
        if (label && label.gameObject.activeSelf)
        {
            IncludeTextBounds(label, ref left, ref right, ref bottom, ref top);
        }

        centerY = float.IsInfinity(bottom) || float.IsInfinity(top) ? 0f : (bottom + top) * .5f;
        return !float.IsInfinity(left) && !float.IsInfinity(right) && right > left;
    }

    private void IncludeTextBounds(TMP_Text text, ref float left, ref float right, ref float bottom, ref float top)
    {
        text.ForceMeshUpdate();
        var bounds = text.textBounds;
        if (bounds.size.x <= .01f || bounds.size.y <= .01f)
        {
            IncludeRectBounds(text.rectTransform, ref left, ref right, ref bottom, ref top);
            return;
        }
        IncludePoint(text.transform.TransformPoint(new Vector3(bounds.min.x, bounds.min.y, 0f)), ref left, ref right, ref bottom, ref top);
        IncludePoint(text.transform.TransformPoint(new Vector3(bounds.max.x, bounds.max.y, 0f)), ref left, ref right, ref bottom, ref top);
    }

    private void IncludeRectBounds(RectTransform rect, ref float left, ref float right, ref float bottom, ref float top)
    {
        rect.GetWorldCorners(rectCorners);
        for (var i = 0; i < rectCorners.Length; i++) IncludePoint(rectCorners[i], ref left, ref right, ref bottom, ref top);
    }

    private void IncludePoint(Vector3 worldPoint, ref float left, ref float right, ref float bottom, ref float top)
    {
        var local = transform.InverseTransformPoint(worldPoint);
        left = Mathf.Min(left, local.x);
        right = Mathf.Max(right, local.x);
        bottom = Mathf.Min(bottom, local.y);
        top = Mathf.Max(top, local.y);
    }

    private void ApplyKnots(bool highlighted)
    {
        var ornament = PortalVisualSettings.OrnamentColor;
        var enabledByConfig = PortalVisualSettings.ShowRowKnots;
        knotsVisible = enabledByConfig && highlighted;
        foreach (var knot in knots)
        {
            if (!knot) continue;
            knot.gameObject.SetActive(enabledByConfig);
            knot.color = new Color(ornament.r, ornament.g, ornament.b, knotsVisible ? ornament.a : 0f);
        }
        if (knotsVisible) PositionKnots();
    }
}
