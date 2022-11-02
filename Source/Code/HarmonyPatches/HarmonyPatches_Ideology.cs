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
        static void HarmonyPatches_Ideology(Harmony harmony)
        {
            // Allows meditation to take place at the nightmare tree
            harmony.Patch(original: AccessTools.Method(type: typeof(JobDriver_Meditate), name: "MeditationTick"), prefix: null, postfix: new HarmonyMethod(
                methodType: typeof(HarmonyPatches),
                methodName: nameof(MeditationTick_PostFix)));
            DebugMessage(s: "JobDriver.MeditationTick Passed");
        }

        // Allows meditation to take place at the nightmare tree
        public static void MeditationTick_PostFix(JobDriver_Meditate __instance)
        {
            var pawn = Traverse.Create(root: __instance).Field(name: "pawn").GetValue<Pawn>();

            if (ModsConfig.RoyaltyActive && DefDatabase<MeditationFocusDef>.GetNamed(defName: "Morbid").CanPawnUse(p: pawn))
            {
                int num = GenRadial.NumCellsInRadius(radius: MeditationUtility.FocusObjectSearchRadius);
                for (int i = 0; i < num; i++)
                {
                    IntVec3 c = pawn.Position + GenRadial.RadialPattern[i];
                    if (c.InBounds(map: pawn.Map))
                    {
                        Plant plant = c.GetPlant(map: pawn.Map);
                        if (plant != null && plant.def == ThingDef.Named(defName: "Cults_PlantTreeNightmare"))
                        {
                            CompSpawnSubplant compSpawnSubplant = plant.TryGetComp<CompSpawnSubplant>();
                            if (compSpawnSubplant != null)
                            {
                                compSpawnSubplant.AddProgress(progress: JobDriver_Meditate.AnimaTreeSubplantProgressPerTick, ignoreMultiplier: false);
                            }
                        }
                    }
                }
            }
        }

        
    }
}