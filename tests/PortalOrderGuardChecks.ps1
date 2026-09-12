$ErrorActionPreference = 'Stop'
$source = Get-Content -Raw -LiteralPath (Join-Path (Split-Path -Parent $PSScriptRoot) 'Plugin.cs')
$patch = [regex]::Match($source, '(?s)\[HarmonyPatch\(typeof\(ZDOMan\), "AddIfPortal"\)\]\s*internal static class PortalOrderObservationPatch\s*\{.*?\r?\n\}\s*(?=internal static class PortalRanges)')
if (-not $patch.Success) { throw 'Could not find the complete production portal-observation patch.' }

# Execute the production patch unchanged against isolated call-counting stubs.
# No game assembly, world, character, ledger file, or game process is used.
$harness = @'
using System;
using System.Collections.Generic;
using System.Reflection;
namespace BetterPortalsGuardRegression
{
    [AttributeUsage(AttributeTargets.Class)]
    sealed class HarmonyPatch : Attribute { public HarmonyPatch(Type type, string method) { } }
    sealed class ZDOMan { }
    sealed class ZDO { }
    sealed class Game
    {
        public static Game instance;
        public List<int> PortalPrefabHash;
        public static implicit operator bool(Game value) => value != null;
    }
    static class PortalOrderLedger
    {
        public static int Prepares, Observations;
        public static ZDO LastObserved;
        public static void PrepareObservation() { Prepares++; }
        public static void Observe(ZDO zdo) { Observations++; LastObserved = zdo; }
    }
    public static class Checks
    {
        public static string[] Run()
        {
            var results = new List<string>();
            var flags = BindingFlags.Static | BindingFlags.NonPublic;
            var prefix = typeof(PortalOrderObservationPatch).GetMethod("Prefix", flags);
            var postfix = typeof(PortalOrderObservationPatch).GetMethod("Postfix", flags);
            Action<int, ZDO> observe = (hash, zdo) =>
            {
                var arguments = new object[] { hash, false };
                prefix.Invoke(null, arguments);
                postfix.Invoke(null, new object[] { zdo, arguments[1] });
            };
            Action<bool, string> check = (passed, message) =>
            {
                if (!passed) throw new InvalidOperationException(message);
                results.Add("PortalOrderGuard: PASS " + message);
            };
            var ordinary = new ZDO();
            Game.instance = null;
            observe(101, ordinary);
            check(PortalOrderLedger.Prepares == 0 && PortalOrderLedger.Observations == 0,
                "missing Game fails closed without initializing or observing");
            Game.instance = new Game();
            observe(101, ordinary);
            check(PortalOrderLedger.Prepares == 0 && PortalOrderLedger.Observations == 0,
                "missing portal-prefab list fails closed");
            Game.instance.PortalPrefabHash = new List<int> { 101, 202 };
            for (var i = 0; i < 10000; i++) observe(303, ordinary);
            check(PortalOrderLedger.Prepares == 0 && PortalOrderLedger.Observations == 0,
                "10000 ordinary object callbacks do not initialize or write the ledger");
            var portal = new ZDO();
            observe(101, portal);
            check(PortalOrderLedger.Prepares == 1 && PortalOrderLedger.Observations == 1 &&
                ReferenceEquals(PortalOrderLedger.LastObserved, portal),
                "recognized portal initializes and observes the exact supplied object once");
            observe(202, portal);
            check(PortalOrderLedger.Prepares == 2 && PortalOrderLedger.Observations == 2,
                "additional native portal prefab is accepted");
            observe(303, ordinary);
            check(PortalOrderLedger.Prepares == 2 && PortalOrderLedger.Observations == 2,
                "ordinary object remains rejected after portal observation");
            Game.instance.PortalPrefabHash.Clear();
            observe(101, ordinary);
            check(PortalOrderLedger.Prepares == 2 && PortalOrderLedger.Observations == 2,
                "empty current prefab list fails closed");
            return results.ToArray();
        }
    }
__PRODUCTION_PATCH__
}
'@
Add-Type -TypeDefinition ($harness.Replace('__PRODUCTION_PATCH__', $patch.Value))
[BetterPortalsGuardRegression.Checks]::Run()
