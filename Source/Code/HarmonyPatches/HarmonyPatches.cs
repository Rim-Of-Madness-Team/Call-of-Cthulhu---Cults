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
    internal static partial class HarmonyPatches
    {
        public static readonly bool DebugMode = true;

        static HarmonyPatches()
        {
            var harmony = new Harmony(id: "rimworld.jecrell.cthulhu.cults");

            HarmonyPatches_FlightHauler(harmony);
            HarmonyPatches_Ideology(harmony);
            HarmonyPatches_SoilFertility(harmony);
            HarmonyPatches_TransmogrifiedPawns(harmony);
        }
        public static void DebugMessage(string s)
        {
            if (DebugMode)
            {
                Log.Message(text: s);
            }
        }

    }
}