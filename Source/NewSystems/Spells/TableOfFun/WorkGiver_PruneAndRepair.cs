using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;
using System.Linq;

namespace CultOfCthulhu
{
    public class WorkGiver_PruneAndRepair : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);
            }
        }

        public IEnumerable<Thing> NightmareAltars(Pawn pawn)
        {
                List<Thing> thingsToCheck = new List<Thing>(from Thing things in pawn.Map.listerBuildings.allBuildingsColonist
                                                            where things.def.defName == "Cult_NightmareSacrificeAltar"
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
            return NightmareAltars(pawn);
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return NightmareAltars(pawn).Count<Thing>() == 0;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced=false)
        {
            Building_SacrificialAltar building = t as Building_SacrificialAltar;
            if (building == null)
            {
                return false;
            }
            if (!building.toBePrunedAndRepaired)
            {
                return false;
            }
            if (t.Faction != pawn.Faction)
            {
                return false;
            }
            if (pawn.Faction == Faction.OfPlayer && !pawn.Map.areaManager.Home[t.Position])
            {
                JobFailReason.Is(WorkGiver_FixBrokenDownBuilding.NotInHomeAreaTrans);
                return false;
            }
            if (!pawn.CanReserve(t)) return false;// pawn.Map.reservationManager.IsReserved(t, pawn.Faction)) return false;
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return new Job(CultsDefOf.Cults_PruneAndRepair, t);
        }
    }
}
