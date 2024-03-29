﻿using RimWorld;
using Verse;
using Verse.AI;

namespace CultOfCthulhu
{
    public class WorkGiver_Investigate : WorkGiver_Scanner
    {
        //public static IEnumerable<Thing> MysteriousObjects(Pawn pawn)
        //{
        //    List<Thing> thingsToCheck = new List<Thing>(from Thing things in pawn.Map.spawnedThings
        //                                                where things.def == CultsDefOf.Cults_PlantTreeNightmare ||
        //                                                      things.def == CultsDefOf.Cults_MonolithNightmare
        //                                                select things);
        //    return thingsToCheck;

        //}

        public override PathEndMode PathEndMode => PathEndMode.Touch;

        //public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        //{
        //    return MysteriousObjects(pawn);
        //}

        public override ThingRequest PotentialWorkThingRequest =>
            ThingRequest.ForDef(singleDef: CultsDefOf.Cults_MonolithNightmare);

        //public override bool ShouldSkip(Pawn pawn)
        //{
        //    return MysteriousObjects(pawn).Count<Thing>() == 0;
        //}

        public override bool Prioritized => true;

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t == null)
            {
                return false;
            }

            if (t.def != CultsDefOf.Cults_PlantTreeNightmare &&
                t.def != CultsDefOf.Cults_MonolithNightmare)
            {
                return false;
            }

            //Log.Message("1");

            //Log.Message("2");

            var cultTracker = pawn.MapHeld.GetComponent<MapComponent_LocalCultTracker>();
            if (cultTracker != null && cultTracker.CurrentSeedState > CultSeedState.NeedSeeing)
            {
                return false;
            }
            //Log.Message("3");

            if (CultUtility.AreCultObjectsAvailable(map: pawn.MapHeld) == false)
            {
                if (CultUtility.IsSomeoneInvestigating(map: pawn.MapHeld))
                {
                    return false;
                }
            }
            //Log.Message("4");

            if (pawn.Faction != Faction.OfPlayerSilentFail)
            {
                return false;
            }
            //if (pawn.Faction == Faction.OfPlayer && !pawn.Map.areaManager.Home[t.Position])
            //{
            //    JobFailReason.Is(WorkGiver_FixBrokenDownBuilding.NotInHomeAreaTrans);
            //    return false;
            //}
            //Log.Message("5");

            if (!pawn.CanReserveAndReach(target: t, peMode: PathEndMode.ClosestTouch, maxDanger: Danger.None))
            {
                return false; // pawn.Map.reservationManager.IsReserved(t, pawn.Faction)) return false;
            }
            //Log.Message("6");

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            //Log.Message("JobOnThing");

            pawn.MapHeld.GetComponent<MapComponent_LocalCultTracker>().CurrentSeedPawn = pawn;
            pawn.MapHeld.GetComponent<MapComponent_LocalCultTracker>().CurrentSeedTarget = t;
            return new Job(def: CultsDefOf.Cults_Investigate, targetA: pawn, targetB: t);
        }
    }
}