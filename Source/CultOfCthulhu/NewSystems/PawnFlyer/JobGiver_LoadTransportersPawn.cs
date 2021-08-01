using System.Collections.Generic;
using Cthulhu;
using Verse;
using Verse.AI;

namespace CultOfCthulhu
{
    public class JobGiver_LoadTransportersPawn : ThinkNode_JobGiver
    {
        private static readonly List<CompTransporterPawn> tmpTransporters = new List<CompTransporterPawn>();

        protected override Job TryGiveJob(Pawn pawn)
        {
            Utility.DebugReport("JobGiver_LoadTransportersPawn Called");
            var transportersGroup = pawn.mindState.duty.transportersGroup;
            LoadTransportersPawnJobUtility.GetTransportersInGroup(transportersGroup, pawn.Map, tmpTransporters);
            foreach (var transporter in tmpTransporters)
            {
                if (LoadTransportersPawnJobUtility.HasJobOnTransporter(pawn, transporter))
                {
                    return LoadTransportersPawnJobUtility.JobOnTransporter(pawn, transporter);
                }
            }

            return null;
        }
    }
}