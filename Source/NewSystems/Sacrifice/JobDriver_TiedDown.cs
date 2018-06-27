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
                return (Building_SacrificialAltar)base.job.GetTarget(TargetIndex.A).Thing;
            }
        }


        protected override IEnumerable<Toil> MakeNewToils()
        {

            yield return new Toil
            {
                initAction = delegate
                {
                    this.pawn.Reserve(this.pawn.Position, this.job);// De ReserveDestinationFor(this.pawn, this.pawn.Position);
                    this.pawn.pather.StopDead();
                    JobDriver curDriver = this.pawn.jobs.curDriver;
                    pawn.jobs.posture = PawnPosture.LayingOnGroundFaceUp;
                    curDriver.asleep = false;
                },
                tickAction = delegate
                {
                    if (this.job.expiryInterval == -1 && this.job.def == JobDefOf.Wait_Combat && !this.pawn.Drafted)
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