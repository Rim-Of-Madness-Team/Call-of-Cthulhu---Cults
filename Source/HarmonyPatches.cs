using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using Verse;
using RimWorld;

namespace CultOfCthulhu
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {

        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.jecrell.cthulhu.cults");
            //harmony.Patch(AccessTools.Method(typeof(ThingWithComps), "InitializeComps"), null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("InitializeComps_PostFix")), null);
            harmony.Patch(AccessTools.Method(typeof(Pawn), "get_BodySize"), null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("get_BodySize_PostFix")));
            harmony.Patch(AccessTools.Method(typeof(Pawn), "get_KindLabel"), null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("get_KindLabel_PostFix")));
            harmony.Patch(AccessTools.Method(typeof(Pawn), "get_KindLabelPlural"), null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("get_KindLabelPlural_PostFix")));
        }

        public static void get_KindLabel_PostFix(Pawn __instance, ref string __result)
        {
           if (__instance != null)
            {
                CompTransmogrified compTrans = __instance.GetComp<CompTransmogrified>();
                if (compTrans != null)
                {
                    if (compTrans.isTransmogrified)
                    {
                        string newResult = "Cults_Monstrous".Translate(new object[]
                            {
                                GenLabel.BestKindLabel(__instance, false, false, false)
                            });
                        __result = newResult;
                    }
                }
            }
        }

        public static void get_KindLabelPlural_PostFix(Pawn __instance, ref string __result)
        {
            if (__instance != null)
            {
                CompTransmogrified compTrans = __instance.GetComp<CompTransmogrified>();
                if (compTrans != null)
                {
                    if (compTrans.isTransmogrified)
                    {
                        string newResult = "Cults_Monstrous".Translate(new object[]
                            {
                                GenLabel.BestKindLabel(__instance, false, false, true)
                            });
                        __result = newResult;
                    }
                }
            }
        }

        public static void get_BodySize_PostFix(Pawn __instance, ref float __result)
        {
            if (__instance != null)
            {
                CompTransmogrified compTrans = __instance.GetComp<CompTransmogrified>();
                if (compTrans != null)
                {
                    if (compTrans.isTransmogrified) __result = __result * 3;
                }
            }
        }


        //public static void InitializeComps_PostFix(ThingWithComps __instance)
        //{
        //    //Log.Message("1");
        //    if (__instance != null)
        //    {
        //        Pawn p = __instance as Pawn;
        //        if (p != null)
        //        {
        //            if (p.RaceProps != null && (p.RaceProps.Humanlike))
        //            {
        //                ThingComp thingComp = (ThingComp)Activator.CreateInstance(typeof(CompPsionicUser));
        //                thingComp.parent = __instance;
        //                var comps = AccessTools.Field(typeof(ThingWithComps), "comps").GetValue(__instance);
        //                if (comps != null)
        //                {
        //                    ((List<ThingComp>)comps).Add(thingComp);
        //                }
        //                thingComp.Initialize(null);
        //            }
        //            if (p.RaceProps != null && (p.RaceProps.Animal))
        //            {
        //                ThingComp thingComp = (ThingComp)Activator.CreateInstance(typeof(CompTransmogrified));
        //                thingComp.parent = __instance;
        //                var comps = AccessTools.Field(typeof(ThingWithComps), "comps").GetValue(__instance);
        //                if (comps != null)
        //                {
        //                    ((List<ThingComp>)comps).Add(thingComp);
        //                }
        //                thingComp.Initialize(null);
        //            }
        //        }
        //    }
        //}
    }
}
