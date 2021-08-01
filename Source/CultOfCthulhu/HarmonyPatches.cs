using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CultOfCthulhu
{
    [StaticConstructorOnStartup]
    internal static class HarmonyPatches
    {
        public static readonly bool DebugMode = false;

        static HarmonyPatches()
        {
            var harmony = new Harmony("rimworld.jecrell.cthulhu.cults");
            harmony.Patch(AccessTools.Method(typeof(ThingWithComps), nameof(ThingWithComps.InitializeComps)), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(InitializeComps_PostFix)));
            DebugMessage("ThingWithComps.InitializeComps_PostFix Passed");

            harmony.Patch(AccessTools.Property(typeof(Pawn), nameof(Pawn.BodySize)).GetGetMethod(), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(get_BodySize_PostFix)));
            DebugMessage("Pawn.BodySize.get_BodySize_PostFix Passed");


            harmony.Patch(AccessTools.Property(typeof(Pawn), nameof(Pawn.HealthScale)).GetGetMethod(), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(get_HealthScale_PostFix)));
            DebugMessage("Pawn.HealthScale.get_HealthScale_PostFix Passed");

            harmony.Patch(
                AccessTools.Method(typeof(GenLabel), "BestKindLabel",
                    new[] {typeof(Pawn), typeof(bool), typeof(bool), typeof(bool), typeof(int)}), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(BestKindLabel_PostFix)));
            DebugMessage("GenLabel.BestKindLabel_PostFix Passed");

            harmony.Patch(AccessTools.Method(typeof(Pawn_DrawTracker), nameof(Pawn_DrawTracker.DrawAt)), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(DrawAt_PostFix)));
            DebugMessage("Pawn_DrawTracker.DrawAt_PostFix Passed");

            harmony.Patch(
                AccessTools.Method(typeof(PawnUtility), nameof(PawnUtility.IsTravelingInTransportPodWorldObject)), null,
                new HarmonyMethod(typeof(HarmonyPatches),
                    nameof(IsTravelingInTransportPodWorldObject_PostFix)));
            DebugMessage(
                "PawnUtility.IsTravelingInTransportPodWorldObject.IsTravelingInTransportPodWorldObject_PostFix Passed");

            harmony.Patch(AccessTools.Method(typeof(FertilityGrid), "CalculateFertilityAt"), null, new HarmonyMethod(
                typeof(HarmonyPatches),
                nameof(CalculateFertilityAt)));
            DebugMessage("FertilityGrid.CalculateFertilityAt Passed");

            harmony.Patch(AccessTools.Method(typeof(MouseoverReadout), "MouseoverReadoutOnGUI"), new HarmonyMethod(
                typeof(HarmonyPatches),
                nameof(MouseoverReadoutOnGUI)));
            DebugMessage("MouseoverReadout.MouseoverReadoutOnGUI Passed");

            harmony.Patch(AccessTools.Method(typeof(JobDriver_Meditate), "MeditationTick"), null, new HarmonyMethod(
                typeof(HarmonyPatches),
                nameof(MeditationTick_PostFix)));
            DebugMessage("JobDriver.MeditationTick Passed");

            harmony.Patch(AccessTools.Property(typeof(MapPawns),  nameof(MapPawns.AnyPawnBlockingMapRemoval)).GetGetMethod(), null, new HarmonyMethod(
                typeof(HarmonyPatches),
                nameof(AnyPawnBlockingMapRemoval_ByakheePatch)));
            DebugMessage("MapPawns.AnyPawnBlockingMapRemoval Passed");
        }

        public static void AnyPawnBlockingMapRemoval_ByakheePatch(MapPawns __instance, ref bool __result)
        {
            if (__result == false)
            {
                Faction ofPlayer = Faction.OfPlayer;
                if (__instance?.AllPawnsSpawned?.FirstOrDefault(x => x?.Faction == ofPlayer && x?.def?.defName == "Cults_ByakheeRace") is Pawn byakhee)
                {
                    __result = true;
                }

                Map map = Traverse.Create(__instance).Field("map").GetValue<Map>();
                if (map?.listerThings?.ThingsOfDef(ThingDef.Named("ByakheeLeaving"))?.FirstOrDefault() != null)
                {
                    __result = true;
                }
            }
        }

        public static void MeditationTick_PostFix(JobDriver_Meditate __instance)
        {
            var pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();

            if (ModsConfig.RoyaltyActive && DefDatabase<MeditationFocusDef>.GetNamed("Morbid").CanPawnUse(pawn))
            {
                int num = GenRadial.NumCellsInRadius(MeditationUtility.FocusObjectSearchRadius);
                for (int i = 0; i < num; i++)
                {
                    IntVec3 c = pawn.Position + GenRadial.RadialPattern[i];
                    if (c.InBounds(pawn.Map))
                    {
                        Plant plant = c.GetPlant(pawn.Map);
                        if (plant != null && plant.def == ThingDef.Named("Cults_PlantTreeNightmare"))
                        {
                            CompSpawnSubplant compSpawnSubplant = plant.TryGetComp<CompSpawnSubplant>();
                            if (compSpawnSubplant != null)
                            {
                                compSpawnSubplant.AddProgress(JobDriver_Meditate.AnimaTreeSubplantProgressPerTick, false);
                            }
                        }
                    }
                }
            }
        }
        public static void DebugMessage(string s)
        {
            if (DebugMode)
            {
                Log.Message(s);
            }
        }

        private static string SpeedPercentString(float extraPathTicks)
        {
            var f = 13f / (extraPathTicks + 13f);
            return f.ToStringPercent();
        }

        public static bool MouseoverReadoutOnGUI(MouseoverReadout __instance)
        {
            var c = UI.MouseCell();
            if (!c.InBounds(Find.CurrentMap) ||
                Event.current.type != EventType.Repaint ||
                Find.MainTabsRoot.OpenTab != null)
            {
                return false;
            }

            if (!(Find.CurrentMap.GetComponent<MapComponent_FertilityMods>().Get is MapComponent_FertilityMods fert) ||
                !fert.ActiveCells.Contains(c))
            {
                return true;
            }

            //Original Variables
            var BotLeft = new Vector2(15f, 65f);

            GenUI.DrawTextWinterShadow(new Rect(256f, UI.screenHeight - 256, -256f, 256f));
            Text.Font = GameFont.Small;
            GUI.color = new Color(1f, 1f, 1f, 0.8f);

            var num = 0f;
            Rect rect;
            if (c.Fogged(Find.CurrentMap))
            {
                rect = new Rect(BotLeft.x, UI.screenHeight - BotLeft.y - num, 999f, 999f);
                Widgets.Label(rect, "Undiscovered".Translate());
                GUI.color = Color.white;
                return false;
            }

            rect = new Rect(BotLeft.x, UI.screenHeight - BotLeft.y - num, 999f, 999f);
            var num2 = Mathf.RoundToInt(Find.CurrentMap.glowGrid.GameGlowAt(c) * 100f);
            var glowStrings = Traverse.Create(__instance).Field("glowStrings").GetValue<string[]>();
            Widgets.Label(rect, glowStrings[num2]);
            num += 19f;
            rect = new Rect(BotLeft.x, UI.screenHeight - BotLeft.y - num, 999f, 999f);
            var terrain = c.GetTerrain(Find.CurrentMap);
            //string SpeedPercentString = Traverse.Create(__instance).Method("SpeedPercentString", (float)terrain.pathCost).GetValue<string>();
            //TerrainDef cachedTerrain = Traverse.Create(__instance).Field("cachedTerrain").GetValue<TerrainDef>();
            _ =
                Traverse.Create(__instance).Field("cachedTerrainString").GetValue<string>();

            //if (terrain != cachedTerrain)
            //{
            var fertNum = Find.CurrentMap.fertilityGrid.FertilityAt(c);
            string str = fertNum <= 0.0001
                ? TaggedString.Empty
                : " " + "FertShort".Translate() + " " + fertNum.ToStringPercent();
            string cachedTerrainString = terrain.LabelCap + (terrain.passability == Traversability.Impassable
                ? null
                : " (" + "WalkSpeed".Translate(SpeedPercentString(terrain.pathCost) + str + ")"));
            //cachedTerrain = terrain;
            //}
            Widgets.Label(rect, cachedTerrainString);
            num += 19f;
            var zone = c.GetZone(Find.CurrentMap);
            if (zone != null)
            {
                rect = new Rect(BotLeft.x, UI.screenHeight - BotLeft.y - num, 999f, 999f);
                var label = zone.label;
                Widgets.Label(rect, label);
                num += 19f;
            }

            var depth = Find.CurrentMap.snowGrid.GetDepth(c);
            if (depth > 0.03f)
            {
                rect = new Rect(BotLeft.x, UI.screenHeight - BotLeft.y - num, 999f, 999f);
                var snowCategory = SnowUtility.GetSnowCategory(depth);
                string label2 = SnowUtility.GetDescription(snowCategory) +
                                " (" +
                                "WalkSpeed".Translate(
                                    SpeedPercentString(SnowUtility.MovementTicksAddOn(snowCategory))) +
                                ")";
                Widgets.Label(rect, label2);
                num += 19f;
            }

            var thingList = c.GetThingList(Find.CurrentMap);
            foreach (var thing in thingList)
            {
                if (thing.def.category == ThingCategory.Mote)
                {
                    continue;
                }

                rect = new Rect(BotLeft.x, UI.screenHeight - BotLeft.y - num, 999f, 999f);
                var labelMouseover = thing.LabelMouseover;
                Widgets.Label(rect, labelMouseover);
                num += 19f;
            }

            var roof = c.GetRoof(Find.CurrentMap);
            if (roof != null)
            {
                rect = new Rect(BotLeft.x, UI.screenHeight - BotLeft.y - num, 999f, 999f);
                Widgets.Label(rect, roof.LabelCap);
            }

            GUI.color = Color.white;
            return false;
        }


        // RimWorld.FertilityGrid
        public static void CalculateFertilityAt(FertilityGrid __instance, IntVec3 loc, ref float __result)
        {
            var map = Traverse.Create(__instance).Field("map").GetValue<Map>();
            if (!(map.GetComponent<MapComponent_FertilityMods>().Get is MapComponent_FertilityMods comp))
            {
                return;
            }

            if (comp.ActiveCells.Contains(loc))
            {
                __result *= 2;
            }
        }


        // RimWorld.PawnUtility
        public static void IsTravelingInTransportPodWorldObject_PostFix(Pawn pawn, ref bool __result)
        {
            __result = __result || ThingOwnerUtility.AnyParentIs<ActiveDropPodInfo>(pawn);
        }


        //RenderPawnAt_PostFix
        // Verse.PawnRenderer
        public static void DrawAt_PostFix(Pawn_DrawTracker __instance, Vector3 loc)
        {
            var pawn = (Pawn) AccessTools.Field(typeof(Pawn_DrawTracker), "pawn").GetValue(__instance);
            if (!(pawn?.GetComp<CompTransmogrified>() is CompTransmogrified {IsTransmogrified: true} compTrans) ||
                !pawn.Spawned)
            {
                return;
            }

            var matSingle = CultsDefOf.Cults_TransmogAura.graphicData.Graphic.MatSingle;
            var angle = pawn.Rotation.AsAngle + (compTrans.Hediff.UndulationTicks * 100);

            var xCap = pawn.kindDef.lifeStages[0].bodyGraphicData.drawSize.x + 0.5f;
            var zCap = pawn.kindDef.lifeStages[0].bodyGraphicData.drawSize.y + 0.5f;

            var x = pawn.kindDef.lifeStages[0].bodyGraphicData.drawSize.x;
            var z = pawn.kindDef.lifeStages[0].bodyGraphicData.drawSize.y;
            var drawX = Mathf.Clamp((x + compTrans.Hediff.UndulationTicks) * compTrans.Hediff.graphicDiv, 0.01f,
                xCap);
            var drawY = AltitudeLayer.Terrain.AltitudeFor();
            var drawZ = Mathf.Clamp((z + compTrans.Hediff.UndulationTicks) * compTrans.Hediff.graphicDiv, 0.01f,
                zCap);
            var s = new Vector3(drawX, drawY, drawZ);
            Matrix4x4 matrix = default;
            matrix.SetTRS(loc, Quaternion.AngleAxis(angle, Vector3.up), s);
            Graphics.DrawMesh(MeshPool.plane10Back, matrix, matSingle, 0);
        }

        // RimWorld.GenLabel
        public static void BestKindLabel_PostFix(Pawn pawn, bool mustNoteGender,
            bool mustNoteLifeStage, bool plural, int pluralCount, ref string __result)
        {
            if (pawn?.GetComp<CompTransmogrified>() is CompTransmogrified {IsTransmogrified: true})
            {
                __result = "Cults_Monstrous".Translate(__result);
            }
        }


        public static void get_BodySize_PostFix(Pawn __instance, ref float __result)
        {
            if (__instance?.GetComp<CompTransmogrified>() is CompTransmogrified {IsTransmogrified: true})
            {
                __result *= 3;
            }
        }

        public static void get_HealthScale_PostFix(Pawn __instance, ref float __result)
        {
            if (__instance?.GetComp<CompTransmogrified>() is CompTransmogrified {IsTransmogrified: true})
            {
                __result *= 3;
            }
        }


        public static void InitializeComps_PostFix(ThingWithComps __instance)
        {
            if (!(__instance is Pawn p))
            {
                return;
            }

            if (p.RaceProps == null || !p.RaceProps.Animal)
            {
                return;
            }

            var thingComp = (ThingComp) Activator.CreateInstance(typeof(CompTransmogrified));
            thingComp.parent = __instance;
            var comps = AccessTools.Field(typeof(ThingWithComps), "comps").GetValue(__instance);
            ((List<ThingComp>) comps)?.Add(thingComp);

            thingComp.Initialize(null);
        }
    }
}