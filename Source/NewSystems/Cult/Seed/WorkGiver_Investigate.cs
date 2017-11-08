using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;
using System.Linq;

namespace CultOfCthulhu
{
    public class WorkGiver_Investigate : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);
            }
        }

        public IEnumerable<Thing> MysteriousObjects(Pawn pawn)
        {
            List<Thing> thingsToCheck = new List<Thing>(from Thing things in pawn.Map.spawnedThings
                                                        where things.def == CultsDefOf.Cults_PlantTreeNightmare ||
                                                              things.def == CultsDefOf.Cults_MonolithNightmare
                                                        select things);
            return thingsToCheck;

        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return MysteriousObjects(pawn);
        }

        public override bool ShouldSkip(Pawn pawn)
        {

            return MysteriousObjects(pawn).Count<Thing>() == 0;
        }

        public override bool Prioritized => true;

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            //Log.Message("1");
            MapComponent_LocalCultTracker cultTracker = pawn.MapHeld.GetComponent<MapComponent_LocalCultTracker>();
            if (t == null)
            {

                return false;
            }
            //Log.Message("2");

            if (cultTracker != null && cultTracker.CurrentSeedState > CultSeedState.NeedSeeing)
            {
                return false;
            }
            //Log.Message("3");

            if (CultUtility.AreCultObjectsAvailable(pawn.MapHeld) == false)
            {
                if (CultUtility.IsSomeoneInvestigating(pawn.MapHeld))
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

            if (!pawn.CanReserveAndReach(t, PathEndMode.ClosestTouch, Danger.None)) return false;// pawn.Map.reservationManager.IsReserved(t, pawn.Faction)) return false;
            //Log.Message("6");

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Log.Message("JobOnThing");

            pawn.MapHeld.GetComponent<MapComponent_LocalCultTracker>().CurrentSeedPawn = pawn;
            pawn.MapHeld.GetComponent<MapComponent_LocalCultTracker>().CurrentSeedTarget = t;
            return new Job(CultsDefOf.Cults_Investigate, pawn, t);
        }
    }
}
