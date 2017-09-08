using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;

namespace CultOfCthulhu
{
    public class JobGiver_LoadTransportersPawn : ThinkNode_JobGiver
    {
        private static List<CompTransporterPawn> tmpTransporters = new List<CompTransporterPawn>();

        protected override Job TryGiveJob(Pawn pawn)
        {
            Cthulhu.Utility.DebugReport("JobGiver_LoadTransportersPawn Called");
            int transportersGroup = pawn.mindState.duty.transportersGroup;
            LoadTransportersPawnJobUtility.GetTransportersInGroup(transportersGroup, pawn.Map, JobGiver_LoadTransportersPawn.tmpTransporters);
            for (int i = 0; i < JobGiver_LoadTransportersPawn.tmpTransporters.Count; i++)
            {
                CompTransporterPawn transporter = JobGiver_LoadTransportersPawn.tmpTransporters[i];
                if (LoadTransportersPawnJobUtility.HasJobOnTransporter(pawn, transporter))
                {
                    return LoadTransportersPawnJobUtility.JobOnTransporter(pawn, transporter);
                }
            }
            return null;
        }

    }
}
