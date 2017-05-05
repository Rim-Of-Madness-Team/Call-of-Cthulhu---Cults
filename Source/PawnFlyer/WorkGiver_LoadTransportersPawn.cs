using System;
using Verse;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace CultOfCthulhu
{
    public class WorkGiver_LoadTransportersPawn : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest. ForGroup(ThingRequestGroup.Pawn);
            }
        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t)
        {
            if (t == null) return false;

            Pawn pawn2 = t as Pawn;
            if (pawn2 == null || pawn2 == pawn)
            {
                return false;
            }
            if (!pawn.CanReserveAndReach(t, PathEndMode.ClosestTouch, Danger.Deadly, 1))
            {
                return false;
            }

            CompTransporterPawn transporter = t.TryGetComp<CompTransporterPawn>();
            if (transporter == null) return false;
            return LoadTransportersPawnJobUtility.HasJobOnTransporter(pawn, transporter);
        }

        public override Job JobOnThing(Pawn pawn, Thing t)
        {
            CompTransporterPawn transporter = t.TryGetComp<CompTransporterPawn>();
            if (t == null) return null;
            return LoadTransportersPawnJobUtility.JobOnTransporter(pawn, transporter);
        }


    }


}
