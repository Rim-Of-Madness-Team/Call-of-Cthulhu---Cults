using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using Verse;

namespace CultOfCthulhu
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {

        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.jecrell.cthulhu.cults");
            harmony.Patch(AccessTools.Method(typeof(ThingWithComps), "InitializeComps"), null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("InitializeComps_PostFix")), null);
        }

        public static void InitializeComps_PostFix(ThingWithComps __instance)
        {
            //Log.Message("1");
            if (__instance != null)
            {
                Pawn p = __instance as Pawn;
                if (p != null)
                {
                    if (p.RaceProps != null && (p.RaceProps.Humanlike))
                    {
                        ThingComp thingComp = (ThingComp)Activator.CreateInstance(typeof(CompPsionicUser));
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
