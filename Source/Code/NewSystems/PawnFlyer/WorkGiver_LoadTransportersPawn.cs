using RimWorld;
using Verse;
using Verse.AI;

namespace CultOfCthulhu
{
    public class WorkGiver_LoadTransportersPawn : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(@group: ThingRequestGroup.Pawn);

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

            if (!pawn.CanReserveAndReach(target: t, peMode: PathEndMode.ClosestTouch, maxDanger: Danger.Deadly))
            {
                return false;
            }

            var transporter = t.TryGetComp<CompTransporterPawn>();
            return transporter != null && LoadTransportersPawnJobUtility.HasJobOnTransporter(pawn: pawn, transporter: transporter);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var transporter = t.TryGetComp<CompTransporterPawn>();
            return t == null ? null : LoadTransportersPawnJobUtility.JobOnTransporter(p: pawn, transporter: transporter);
        }
    }
}