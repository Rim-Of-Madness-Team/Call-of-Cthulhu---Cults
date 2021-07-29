using RimWorld;
using Verse;
using Verse.AI;

namespace CultOfCthulhu
{
    public class WorkGiver_LoadTransportersPawn : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t == null)
            {
                return false;
            }

            if (!(t is Pawn pawn2) || pawn2 == pawn)
            {
                return false;
            }

            if (!pawn.CanReserveAndReach(t, PathEndMode.ClosestTouch, Danger.Deadly))
            {
                return false;
            }

            var transporter = t.TryGetComp<CompTransporterPawn>();
            return transporter != null && LoadTransportersPawnJobUtility.HasJobOnTransporter(pawn, transporter);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var transporter = t.TryGetComp<CompTransporterPawn>();
            return t == null ? null : LoadTransportersPawnJobUtility.JobOnTransporter(pawn, transporter);
        }
    }
}