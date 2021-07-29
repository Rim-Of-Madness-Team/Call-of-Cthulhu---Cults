using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace CultOfCthulhu
{
    public class WorkGiver_PruneAndRepair : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest =>
            ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);

        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public IEnumerable<Thing> NightmareAltars(Pawn pawn)
        {
            var thingsToCheck = new List<Thing>(from Thing things in pawn.Map.listerBuildings.allBuildingsColonist
                where things.def.defName == "Cult_NightmareSacrificeAltar"
                select things);
            return thingsToCheck;
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return NightmareAltars(pawn);
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return !NightmareAltars(pawn).Any();
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Building_SacrificialAltar building))
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

            if (!pawn.CanReserve(t))
            {
                return false; // pawn.Map.reservationManager.IsReserved(t, pawn.Faction)) return false;
            }

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return new Job(CultsDefOf.Cults_PruneAndRepair, t);
        }
    }
}