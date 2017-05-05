using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;

namespace CultOfCthulhu
{
    public static class Toils_LayDownAltar
    {
        private const int GetUpOrStartJobWhileInBedCheckInterval = 211;

        public static Toil LayDown(TargetIndex bedOrRestSpotIndex)
        {
            Log.Message("Started Laying down job");
            Toil layDown = new Toil();
            layDown.initAction = delegate
            {
                layDown.actor.pather.StopDead();
                JobDriver curDriver = layDown.actor.jobs.curDriver;
                curDriver.layingDown = true;
                curDriver.asleep = false;
            };
            layDown.tickAction = delegate
            {
                Pawn actor = layDown.actor;
                Job curJob = actor.CurJob;
                JobDriver curDriver = actor.jobs.curDriver;
                if (actor.Downed)
                {
                    actor.Position = CellFinder.RandomClosewalkCellNear(actor.Position, 1);
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                }
                return;
                
            };
            layDown.defaultDuration = 360;
            if (CultUtility.remainingDuration != 0) layDown.defaultDuration = CultUtility.remainingDuration;
            layDown.defaultCompleteMode = ToilCompleteMode.Delay;
            layDown.FailOnDestroyedNullOrForbidden(bedOrRestSpotIndex);
            
            layDown.AddFinishAction(delegate
            {
                Pawn actor = layDown.actor;
                JobDriver curDriver = actor.jobs.curDriver;
                curDriver.layingDown = false;
                curDriver.layingDownBed = null;
                curDriver.asleep = false;
            });
            return layDown;
        }
    }
}
