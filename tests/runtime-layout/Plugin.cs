using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BetterPortalsLayoutProbe
{
    // Diagnostic assembly only: never include it in player/server release packages.
    [BepInPlugin("com.fraileywoodworks.betterportals.layout-probe", "BetterPortals Layout Probe", "1.0.0")]
    [BepInDependency("com.fraileywoodworks.portalselector")]
    public sealed class Plugin : BaseUnityPlugin
    {
        private const BindingFlags Members = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private readonly StringBuilder report = new StringBuilder();
        private readonly List<Button> rows = new List<Button>();
        private GameObject host;
        private Component controller;
        private Type controllerType;
        private GameObject panel;
        private RectTransform panelRect;
        private RectTransform canvasRect;
        private Canvas canvas;
        private TMP_Text title;
        private int checks;
        private int failures;

        private void Awake()
        {
            if (!Application.isBatchMode || Environment.GetEnvironmentVariable("BETTERPORTALS_LAYOUT_PROBE") != "1")
            {
                Logger.LogInfo("Layout probe disabled: requires batch mode and BETTERPORTALS_LAYOUT_PROBE=1.");
                return;
            }
            StartCoroutine(GuardedRun());
        }

        private IEnumerator GuardedRun()
        {
            var tests = Run();
            while (true)
            {
                object next;
                try
                {
                    if (!tests.MoveNext()) break;
                    next = tests.Current;
                }
                catch (Exception error)
                {
                    Check(false, "Unhandled probe exception: " + error);
                    break;
                }
                yield return next;
            }
            Cleanup();
            var summary = $"RESULT {(failures == 0 ? "PASS" : "FAIL")} checks={checks} failures={failures}";
            report.AppendLine(summary);
            var path = Environment.GetEnvironmentVariable("BETTERPORTALS_LAYOUT_REPORT");
            if (string.IsNullOrWhiteSpace(path)) path = Path.Combine(Path.GetTempPath(), "BetterPortals-layout-probe.txt");
            File.WriteAllText(path, report.ToString(), new UTF8Encoding(false));
            Logger.LogInfo(summary + " report=" + path);
        }

        private IEnumerator Run()
        {
            report.AppendLine($"utc={DateTime.UtcNow:o} unity={Application.unityVersion} batch={Application.isBatchMode} screen={Screen.width}x{Screen.height}");
            var deadline = Time.realtimeSinceStartup + 90f;
            while ((!Menu.instance || !Menu.instance.m_continueButton) && Time.realtimeSinceStartup < deadline)
                yield return new WaitForSecondsRealtime(.25f);
            if (!Menu.instance || !Menu.instance.m_continueButton)
                throw new InvalidOperationException("Native Menu/Continue template did not become available within 90 seconds.");

            var assembly = AppDomain.CurrentDomain.GetAssemblies().Single(item => item.GetName().Name == "FraileyPortalSelector");
            report.AppendLine("productionAssembly=" + assembly.FullName);
            controllerType = assembly.GetType("FraileyPortalSelector.PortalController", true);
            if (controllerType.GetField("Instance", Members).GetValue(null) != null)
                throw new InvalidOperationException("Refusing to replace an existing PortalController instance.");
            Check(Player.m_localPlayer == null, "No local player exists during isolated batch layout probe");

            host = new GameObject("BetterPortals Isolated Layout Probe");
            host.SetActive(false);
            controller = host.AddComponent(controllerType);
            ((Behaviour)controller).enabled = false;
            Invoke(controller, "EnsureUi");
            panel = Field<GameObject>("panel");
            panelRect = Field<RectTransform>("panelRect");
            title = Field<TMP_Text>("titleText");
            canvas = panel.GetComponentInParent<Canvas>(true);
            canvasRect = (RectTransform)canvas.transform;

            // Call only UI construction. No Begin, directory/travel, portal, world or player method is invoked.
            foreach (var name in new[] { "A", "Field", "A deliberately long portal name" })
                rows.Add((Button)Invoke(controller, "AddRow", name, "Meadows", ZDOID.None, (Action)(() => { }), (Action)(() => { }), false));
            Invoke(controller, "ApplyVisualSettings");
            panel.SetActive(true);
            host.SetActive(true);
            for (var frame = 0; frame < 8; frame++) yield return null;
            SettleLayout();
            Capture("initial-no-home");

            foreach (var row in rows)
            {
                SetHome(row, true);
                for (var frame = 0; frame < 4; frame++) yield return null;
                SettleLayout();
                Capture("home-" + row.name);
                SetHome(row, false);
            }

            for (var cycle = 1; cycle <= 3; cycle++)
            {
                panel.SetActive(false);
                for (var frame = 0; frame < 2; frame++) yield return null;
                panel.SetActive(true);
                SetHome(rows[cycle % rows.Count], true);
                for (var frame = 0; frame < 4; frame++) yield return null;
                SettleLayout();
                Capture("reopen-" + cycle);
                SetHome(rows[cycle % rows.Count], false);
            }

            // Selected-row animation must not move the visual center or reactivate the native layout.
            foreach (var row in rows)
            {
                var hover = Hover(row);
                Invoke(hover, "SetHighlighted", true);
                yield return new WaitForSecondsRealtime(.15f);
                SettleLayout();
                Capture("hover-" + row.name);
                Invoke(hover, "SetHighlighted", false);
            }
            Check(!((Behaviour)controller).enabled, "Controller Update stayed disabled throughout probe");
            Check(Player.m_localPlayer == null, "Probe ended without creating a player");
        }

        private void Capture(string phase)
        {
            var headingX = Center(title.rectTransform).x;
            var panelCenter = Center(panelRect);
            report.AppendLine($"PHASE {phase} headingX={F(headingX)} panel=({F(panelCenter.x)},{F(panelCenter.y)}) canvas=({F(canvasRect.rect.width)},{F(canvasRect.rect.height)})");
            Check(Near(headingX, panelCenter.x), phase + " heading centered on panel");
            Check(Near(panelCenter.x, canvasRect.rect.center.x), phase + " panel horizontally centered on canvas");
            Check(Near(panelCenter.y, canvasRect.rect.center.y), phase + " panel vertically centered on canvas at default offset");
            var scaler = canvas.GetComponent<CanvasScaler>();
            var nativeScaler = Menu.instance.GetComponentInParent<CanvasScaler>();
            var nativeCanvas = Menu.instance.GetComponentInParent<Canvas>();
            Check(canvas.GetComponent<GuiScaler>() && canvas.GetComponent<GuiScaler>().enabled, phase + " native GuiScaler exists and is enabled");
            Check(scaler.uiScaleMode == CanvasScaler.ScaleMode.ConstantPixelSize, phase + " native constant-pixel scaling mode");
            Check(Near(scaler.scaleFactor, nativeScaler.scaleFactor, .001f), phase + $" scaler matches native: portal={F(scaler.scaleFactor)} native={F(nativeScaler.scaleFactor)}");
            Check(Near(canvas.scaleFactor, nativeCanvas.scaleFactor, .001f), phase + $" canvas scale matches native: portal={F(canvas.scaleFactor)} native={F(nativeCanvas.scaleFactor)}");

            foreach (var row in rows)
            {
                var root = (RectTransform)row.transform.Find("BetterPortals Label Group");
                var stack = root.Find("Portal Name and Biome");
                var name = stack.GetComponentsInChildren<TMP_Text>(true).First(item => item.text == row.name);
                name.ForceMeshUpdate();
                var labelX = Center(root).x;
                var nameX = Center(name.rectTransform).x;
                var renderedCenter = canvasRect.InverseTransformPoint(name.transform.TransformPoint(name.textBounds.center));
                var activeLayouts = row.GetComponents<LayoutGroup>().Count(layout => layout.enabled);
                report.AppendLine($"ROW {row.name} labelX={F(labelX)} nameRectX={F(nameX)} renderedX={F(renderedCenter.x)} rootLayouts={activeLayouts} anchors=({F(root.anchorMin.x)},{F(root.anchorMin.y)}) anchored=({F(root.anchoredPosition.x)},{F(root.anchoredPosition.y)})");
                Check(Near(labelX, headingX), phase + " / " + row.name + " visual-group center equals heading");
                Check(Near(nameX, headingX), phase + " / " + row.name + " name-rectangle center equals heading");
                Check(Near(renderedCenter.x, headingX, 2.5f), phase + " / " + row.name + " rendered name centered within font-bearing tolerance");
                Check(activeLayouts == 0, phase + " / " + row.name + " no enabled inherited root layout");
            }
        }

        private void SettleLayout()
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);
            Canvas.ForceUpdateCanvases();
        }

        private void SetHome(Button row, bool value) => Invoke(Hover(row), "SetHome", value);
        private static Component Hover(Button row) => row.GetComponents<Component>().Single(item => item.GetType().Name == "PortalEntryHover");
        private T Field<T>(string name) => (T)controllerType.GetField(name, Members).GetValue(controller);
        private static object Invoke(object target, string name, params object[] arguments) => target.GetType().GetMethod(name, Members).Invoke(target, arguments);
        private Vector3 Center(RectTransform rect) => canvasRect.InverseTransformPoint(rect.TransformPoint(rect.rect.center));
        private static bool Near(float actual, float expected, float tolerance = .25f) => !float.IsNaN(actual) && !float.IsInfinity(actual) && Mathf.Abs(actual - expected) <= tolerance;
        private static string F(float value) => value.ToString("0.###", CultureInfo.InvariantCulture);

        private void Check(bool passed, string message)
        {
            checks++;
            if (!passed) failures++;
            var line = (passed ? "PASS " : "FAIL ") + message;
            report.AppendLine(line);
            if (!passed) Logger.LogWarning(line);
        }

        private void Cleanup()
        {
            if (controllerType != null && controller != null && ReferenceEquals(controllerType.GetField("Instance", Members).GetValue(null), controller))
                controllerType.GetField("Instance", Members).SetValue(null, null);
            if (host) Destroy(host);
        }
    }
}
