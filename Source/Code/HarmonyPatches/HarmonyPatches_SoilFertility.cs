using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CultOfCthulhu
{
    internal static partial class HarmonyPatches
    {
        static void HarmonyPatches_SoilFertility(Harmony harmony)
        {
            harmony.Patch(original: AccessTools.Method(type: typeof(FertilityGrid), name: "CalculateFertilityAt"), prefix: null, postfix: new HarmonyMethod(
                methodType: typeof(HarmonyPatches),
                methodName: nameof(CalculateFertilityAt)));
            DebugMessage(s: "FertilityGrid.CalculateFertilityAt Passed");

            harmony.Patch(original: AccessTools.Method(type: typeof(MouseoverReadout), name: "MouseoverReadoutOnGUI"), prefix: new HarmonyMethod(
                methodType: typeof(HarmonyPatches),
                methodName: nameof(MouseoverReadoutOnGUI)));
            DebugMessage(s: "MouseoverReadout.MouseoverReadoutOnGUI Passed");
        }
        
        
        private static string SpeedPercentString(float extraPathTicks)
        {
            var f = 13f / (extraPathTicks + 13f);
            return f.ToStringPercent();
        }

        // Checks for fertility mod components added by cults
        // RimWorld.FertilityGrid
        public static void CalculateFertilityAt(FertilityGrid __instance, IntVec3 loc, ref float __result)
        {
            var map = Traverse.Create(root: __instance).Field(name: "map").GetValue<Map>();
            if (!(map.GetComponent<MapComponent_FertilityMods>().Get is MapComponent_FertilityMods comp))
            {
                return;
            }

            if (comp.ActiveCells.Contains(item: loc))
            {
                __result *= 2;
            }
        }
        
        
        public static bool MouseoverReadoutOnGUI(MouseoverReadout __instance)
        {
            var c = UI.MouseCell();
            if (!c.InBounds(map: Find.CurrentMap) ||
                Event.current.type != EventType.Repaint ||
                Find.MainTabsRoot.OpenTab != null)
            {
                return false;
            }

            //Don't patch this readout if there isn't a fertility mod for this map
            if (!(Find.CurrentMap.GetComponent<MapComponent_FertilityMods>().Get is MapComponent_FertilityMods fert) ||
                !fert.ActiveCells.Contains(item: c))
            {
                return true;
            }

            //Original Variables
            var BotLeft = new Vector2(x: 15f, y: 65f);

            GenUI.DrawTextWinterShadow(rect: new Rect(x: 256f, y: UI.screenHeight - 256, width: -256f, height: 256f));
            Text.Font = GameFont.Small;
            GUI.color = new Color(r: 1f, g: 1f, b: 1f, a: 0.8f);

            var num = 0f;
            Rect rect;
            if (c.Fogged(map: Find.CurrentMap))
            {
                rect = new Rect(x: BotLeft.x, y: UI.screenHeight - BotLeft.y - num, width: 999f, height: 999f);
                Widgets.Label(rect: rect, label: "Undiscovered".Translate());
                GUI.color = Color.white;
                return false;
            }

            rect = new Rect(x: BotLeft.x, y: UI.screenHeight - BotLeft.y - num, width: 999f, height: 999f);
            var num2 = Mathf.RoundToInt(f: Find.CurrentMap.glowGrid.GameGlowAt(c: c) * 100f);
            var glowStrings = Traverse.Create(root: __instance).Field(name: "glowStrings").GetValue<string[]>();
            Widgets.Label(rect: rect, label: glowStrings[num2]);
            num += 19f;
            rect = new Rect(x: BotLeft.x, y: UI.screenHeight - BotLeft.y - num, width: 999f, height: 999f);
            var terrain = c.GetTerrain(map: Find.CurrentMap);
            //string SpeedPercentString = Traverse.Create(__instance).Method("SpeedPercentString", (float)terrain.pathCost).GetValue<string>();
            //TerrainDef cachedTerrain = Traverse.Create(__instance).Field("cachedTerrain").GetValue<TerrainDef>();
            _ =
                Traverse.Create(root: __instance).Field(name: "cachedTerrainString").GetValue<string>();

            //if (terrain != cachedTerrain)
            //{
            var fertNum = Find.CurrentMap.fertilityGrid.FertilityAt(loc: c);
            string str = fertNum <= 0.0001
                ? TaggedString.Empty
                : " " + "FertShort".Translate() + " " + fertNum.ToStringPercent();
            string cachedTerrainString = terrain.LabelCap + (terrain.passability == Traversability.Impassable
                ? null
                : " (" + "WalkSpeed".Translate(arg1: SpeedPercentString(extraPathTicks: terrain.pathCost) + str + ")"));
            //cachedTerrain = terrain;
            //}
            Widgets.Label(rect: rect, label: cachedTerrainString);
            num += 19f;
            var zone = c.GetZone(map: Find.CurrentMap);
            if (zone != null)
            {
                rect = new Rect(x: BotLeft.x, y: UI.screenHeight - BotLeft.y - num, width: 999f, height: 999f);
                var label = zone.label;
                Widgets.Label(rect: rect, label: label);
                num += 19f;
            }

            var depth = Find.CurrentMap.snowGrid.GetDepth(c: c);
            if (depth > 0.03f)
            {
                rect = new Rect(x: BotLeft.x, y: UI.screenHeight - BotLeft.y - num, width: 999f, height: 999f);
                var snowCategory = SnowUtility.GetSnowCategory(snowDepth: depth);
                string label2 = SnowUtility.GetDescription(category: snowCategory) +
                                " (" +
                                "WalkSpeed".Translate(
                                    arg1: SpeedPercentString(extraPathTicks: SnowUtility.MovementTicksAddOn(category: snowCategory))) +
                                ")";
                Widgets.Label(rect: rect, label: label2);
                num += 19f;
            }

            var thingList = c.GetThingList(map: Find.CurrentMap);
            foreach (var thing in thingList)
            {
                if (thing.def.category == ThingCategory.Mote)
                {
                    continue;
                }

                rect = new Rect(x: BotLeft.x, y: UI.screenHeight - BotLeft.y - num, width: 999f, height: 999f);
                var labelMouseover = thing.LabelMouseover;
                Widgets.Label(rect: rect, label: labelMouseover);
                num += 19f;
            }

            var roof = c.GetRoof(map: Find.CurrentMap);
            if (roof != null)
            {
                rect = new Rect(x: BotLeft.x, y: UI.screenHeight - BotLeft.y - num, width: 999f, height: 999f);
                Widgets.Label(rect: rect, label: roof.LabelCap);
            }

            GUI.color = Color.white;
            return false;
        }

    }
}