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
        static void HarmonyPatches_FlightHauler(Harmony harmony)
        {
            // Adds a check for Pawns with Active Drop Pod information, such as our Byakhee flying haulers
            harmony.Patch(
                original: AccessTools.Method(type: typeof(PawnUtility), name: nameof(PawnUtility.IsTravelingInTransportPodWorldObject)), prefix: null,
                postfix: new HarmonyMethod(methodType: typeof(HarmonyPatches),
                    methodName: nameof(IsTravelingInTransportPodWorldObject_PostFix)));
            DebugMessage(
                s: "PawnUtility.IsTravelingInTransportPodWorldObject.IsTravelingInTransportPodWorldObject_PostFix Passed");
            
            // Makes sure not to delete a map if a byakhee is the only thing left
            harmony.Patch(original: AccessTools.Property(type: typeof(MapPawns),  name: nameof(MapPawns.AnyPawnBlockingMapRemoval)).GetGetMethod(), prefix: null, postfix: new HarmonyMethod(
                methodType: typeof(HarmonyPatches),
                methodName: nameof(AnyPawnBlockingMapRemoval_ByakheePatch)));
            DebugMessage(s: "MapPawns.AnyPawnBlockingMapRemoval Passed");
        }

        // Adds a check for Pawns with Active Drop Pod information, such as our Byakhee flying haulers
        // RimWorld.PawnUtility
        public static void IsTravelingInTransportPodWorldObject_PostFix(Pawn pawn, ref bool __result)
        {
            __result = __result || ThingOwnerUtility.AnyParentIs<ActiveDropPodInfo>(thing: pawn);
        }
        
        // Makes sure not to delete a map if a byakhee is the only thing left
        public static void AnyPawnBlockingMapRemoval_ByakheePatch(MapPawns __instance, ref bool __result)
        {
            if (__result == false)
            {
                Faction ofPlayer = Faction.OfPlayer;
                if (__instance?.AllPawnsSpawned?.FirstOrDefault(predicate: x => x?.Faction == ofPlayer && x?.def?.defName == "Cults_ByakheeRace") is Pawn byakhee)
                {
                    __result = true;
                }

                Map map = Traverse.Create(root: __instance).Field(name: "map").GetValue<Map>();
                if (map?.listerThings?.ThingsOfDef(def: ThingDef.Named(defName: "ByakheeLeaving"))?.FirstOrDefault() != null)
                {
                    __result = true;
                }
            }
        }

        
    }
}