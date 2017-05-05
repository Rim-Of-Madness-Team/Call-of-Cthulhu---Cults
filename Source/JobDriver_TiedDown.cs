using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse.AI;
using Verse;
using RimWorld;
using UnityEngine;

namespace CultOfCthulhu
{
    public class JobDriver_TiedDown : JobDriver_Wait
    {

        protected Building_SacrificialAltar DropAltar
        {
            get
            {
                return (Building_SacrificialAltar)base.CurJob.GetTarget(TargetIndex.A).Thing;
            }
        }


        protected override IEnumerable<Toil> MakeNewToils()
        {

            yield return new Toil
            {
                initAction = delegate
                {
                    this.pawn.Map.pawnDestinationManager.ReserveDestinationFor(this.pawn, this.pawn.Position);
                    this.pawn.pather.StopDead();
                    JobDriver curDriver = this.pawn.jobs.curDriver;
                    curDriver.layingDown = true;
                    curDriver.asleep = false;
                },
                tickAction = delegate
                {
                    if (this.CurJob.expiryInterval == -1 && this.CurJob.def == JobDefOf.WaitCombat && !this.pawn.Drafted)
                    {
                        Log.Error(this.pawn + " in eternal WaitCombat without being drafted.");
                        this.ReadyForNextToil();
                        return;
                    }
                    if ((Find.TickManager.TicksGame + this.pawn.thingIDNumber) % 4 == 0)
                    {
                        //base.CheckForAutoAttack();
                    }
                    
                },
                defaultCompleteMode = ToilCompleteMode.Never
            };
        }
    }
}