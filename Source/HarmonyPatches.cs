﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using Verse;
using RimWorld;
using UnityEngine;

namespace CultOfCthulhu
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {

        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.jecrell.cthulhu.cults");
            harmony.Patch(AccessTools.Method(typeof(ThingWithComps), "InitializeComps"), null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("InitializeComps_PostFix")), null);
            harmony.Patch(AccessTools.Method(typeof(Pawn), "get_BodySize"), null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("get_BodySize_PostFix")));
            harmony.Patch(AccessTools.Method(typeof(Pawn), "get_HealthScale"), null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("get_HealthScale_PostFix")));
            harmony.Patch(AccessTools.Method(typeof(GenLabel), "BestKindLabel"), null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("BestKindLabel_PostFix")), null);
            harmony.Patch(AccessTools.Method(typeof(Pawn_DrawTracker), "DrawAt"), null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("DrawAt_PostFix")), null);
            harmony.Patch(AccessTools.Method(typeof(PawnUtility), "IsTravelingInTransportPodWorldObject"), null, new HarmonyMethod(typeof(HarmonyPatches),
                nameof(IsTravelingInTransportPodWorldObject_PostFix)));
            harmony.Patch(AccessTools.Method(typeof(FertilityGrid), "CalculateFertilityAt"), null, new HarmonyMethod(typeof(HarmonyPatches),
                nameof(CalculateFertilityAt)));
            harmony.Patch(AccessTools.Method(typeof(MouseoverReadout), "MouseoverReadoutOnGUI"), new HarmonyMethod(typeof(HarmonyPatches),
                nameof(MouseoverReadoutOnGUI)), null);
        }

        public static string SpeedPercentString(float extraPathTicks)
        {
            float f = 13f / (extraPathTicks + 13f);
            return f.ToStringPercent();
        }

        public static bool MouseoverReadoutOnGUI(MouseoverReadout __instance)
        {
            IntVec3 c = UI.MouseCell();
            if (!c.InBounds(Find.VisibleMap) ||
                Event.current.type != EventType.Repaint ||
                Find.MainTabsRoot.OpenTab != null)
            {
                return false;
            }
            
            if (Find.VisibleMap.GetComponent<MapComponent_FertilityMods>().Get is MapComponent_FertilityMods fert &&
                fert.ActiveCells.Contains(c))
            {
                //Original Variables
                Vector2 BotLeft = new Vector2(15f, 65f);

                GenUI.DrawTextWinterShadow(new Rect(256f, (float)(UI.screenHeight - 256), -256f, 256f));
                Text.Font = GameFont.Small;
                GUI.color = new Color(1f, 1f, 1f, 0.8f);

                float num = 0f;
                Rect rect;
                if (c.Fogged(Find.VisibleMap))
                {
                    rect = new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - num, 999f, 999f);
                    Widgets.Label(rect, "Undiscovered".Translate());
                    GUI.color = Color.white;
                    return false;
                }
                rect = new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - num, 999f, 999f);
                int num2 = Mathf.RoundToInt(Find.VisibleMap.glowGrid.GameGlowAt(c) * 100f);
                string[] glowStrings = Traverse.Create(__instance).Field("glowStrings").GetValue<string[]>();
                Widgets.Label(rect, glowStrings[num2]);
                num += 19f;
                rect = new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - num, 999f, 999f);
                TerrainDef terrain = c.GetTerrain(Find.VisibleMap);
                //string SpeedPercentString = Traverse.Create(__instance).Method("SpeedPercentString", (float)terrain.pathCost).GetValue<string>();
                //TerrainDef cachedTerrain = Traverse.Create(__instance).Field("cachedTerrain").GetValue<TerrainDef>();
                string cachedTerrainString = Traverse.Create(__instance).Field("cachedTerrainString").GetValue<string>();

                //if (terrain != cachedTerrain)
                //{
                    float fertNum = Find.VisibleMap.fertilityGrid.FertilityAt(c);
                    string str = ((double)fertNum <= 0.0001) ? string.Empty : (" " + "FertShort".Translate() + " " + fertNum.ToStringPercent());
                    cachedTerrainString = terrain.LabelCap + ((terrain.passability == Traversability.Impassable) ? null : (" (" + "WalkSpeed".Translate(new object[]
                    {
                    SpeedPercentString((float)terrain.pathCost)
                    }) + str + ")"));
                    //cachedTerrain = terrain;
                //}
                Widgets.Label(rect, cachedTerrainString);
                num += 19f;
                Zone zone = c.GetZone(Find.VisibleMap);
                if (zone != null)
                {
                    rect = new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - num, 999f, 999f);
                    string label = zone.label;
                    Widgets.Label(rect, label);
                    num += 19f;
                }
                float depth = Find.VisibleMap.snowGrid.GetDepth(c);
                if (depth > 0.03f)
                {
                    rect = new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - num, 999f, 999f);
                    SnowCategory snowCategory = SnowUtility.GetSnowCategory(depth);
                    string label2 = SnowUtility.GetDescription(snowCategory) + " (" + "WalkSpeed".Translate(new object[]
                    {
                    SpeedPercentString((float)SnowUtility.MovementTicksAddOn(snowCategory))
                    }) + ")";
                    Widgets.Label(rect, label2);
                    num += 19f;
                }
                List<Thing> thingList = c.GetThingList(Find.VisibleMap);
                for (int i = 0; i < thingList.Count; i++)
                {
                    Thing thing = thingList[i];
                    if (thing.def.category != ThingCategory.Mote)
                    {
                        rect = new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - num, 999f, 999f);
                        string labelMouseover = thing.LabelMouseover;
                        Widgets.Label(rect, labelMouseover);
                        num += 19f;
                    }
                }
                RoofDef roof = c.GetRoof(Find.VisibleMap);
                if (roof != null)
                {
                    rect = new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - num, 999f, 999f);
                    Widgets.Label(rect, roof.LabelCap);
                    num += 19f;
                }
                GUI.color = Color.white;
                return false;
            }
            return true;

        }


        // RimWorld.FertilityGrid
        public static void CalculateFertilityAt(ref float __result, FertilityGrid __instance, IntVec3 loc)
        {
            Map map = Traverse.Create(__instance).Field("map").GetValue<Map>();
            if (map.GetComponent<MapComponent_FertilityMods>().Get is MapComponent_FertilityMods comp)
            {
                if (comp.ActiveCells.Contains(loc))
                {
                    //Log.Message("3");

                    __result *= 2;
                }
            }
        }


        // RimWorld.PawnUtility
        public static void IsTravelingInTransportPodWorldObject_PostFix(ref bool __result, Pawn pawn)
        {
            __result = __result || ThingOwnerUtility.AnyParentIs<ActiveDropPodInfo>(pawn);
        }


        //RenderPawnAt_PostFix
        // Verse.PawnRenderer
        public static void DrawAt_PostFix(Pawn_DrawTracker __instance, Vector3 loc)
        {
            Pawn pawn = (Pawn)AccessTools.Field(typeof(Pawn_DrawTracker), "pawn").GetValue(__instance);
            if (pawn?.GetComp<CompTransmogrified>() is CompTransmogrified compTrans && compTrans.IsTransmogrified)
            {
                Material matSingle;
                matSingle = CultsDefOf.Cults_TransmogAura.graphicData.Graphic.MatSingle;

                Vector3 s = new Vector3(pawn.Drawer.renderer.graphics.nakedGraphic.drawSize.x, 1f, pawn.Drawer.renderer.graphics.nakedGraphic.drawSize.y);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(loc, Quaternion.AngleAxis(1, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10Back, matrix, matSingle, 0);
            }
        }

        // RimWorld.GenLabel
        public static void BestKindLabel_PostFix(ref string __result, Pawn pawn, bool mustNoteGender = false, bool mustNoteLifeStage = false, bool plural = false)
        {
            if (pawn?.GetComp<CompTransmogrified>() is CompTransmogrified compTrans && compTrans.IsTransmogrified)
            {
                __result = "Cults_Monstrous".Translate(__result);
            }
        }
            

        public static void get_BodySize_PostFix(Pawn __instance, ref float __result)
        {
            if (__instance?.GetComp<CompTransmogrified>() is CompTransmogrified compTrans && compTrans.IsTransmogrified)
            {
                __result = __result * 3;
            }
        }

        public static void get_HealthScale_PostFix(Pawn __instance, ref float __result)
        {
            if (__instance?.GetComp<CompTransmogrified>() is CompTransmogrified compTrans && compTrans.IsTransmogrified)
            {
                __result = __result * 3;
            }
        }


        public static void InitializeComps_PostFix(ThingWithComps __instance)
        {
            //Log.Message("1");
            if (__instance != null)
            {
                Pawn p = __instance as Pawn;
                if (p != null)
                {
                    if (p.RaceProps != null && (p.RaceProps.Animal))
                    {
                        ThingComp thingComp = (ThingComp)Activator.CreateInstance(typeof(CompTransmogrified));
                        thingComp.parent = __instance;
                        var comps = AccessTools.Field(typeof(ThingWithComps), "comps").GetValue(__instance);
                        if (comps != null)
                        {
                            ((List<ThingComp>)comps).Add(thingComp);
                        }
                        thingComp.Initialize(null);
                    }
                }
            }
        }
    }
}
