using System;
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
