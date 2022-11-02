using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace CultOfCthulhu
{
    public class JobDriver_TiedDown : JobDriver_Wait
    {
        protected Building_SacrificialAltar DropAltar => (Building_SacrificialAltar) job.GetTarget(ind: TargetIndex.A).Thing;


        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return new Toil
            {
                initAction = delegate
                {
                    pawn.Reserve(target: pawn.Position, job: job); // De ReserveDestinationFor(this.pawn, this.pawn.Position);
                    pawn.pather.StopDead();
                    var curDriver = pawn.jobs.curDriver;
                    pawn.jobs.posture = PawnPosture.LayingOnGroundFaceUp;
                    curDriver.asleep = false;
                },
                tickAction = delegate
                {
                    if (job.expiryInterval == -1 && job.def == JobDefOf.Wait_Combat && !pawn.Drafted)
                    {
                        Log.Error(text: pawn + " in eternal WaitCombat without being drafted.");
                        ReadyForNextToil();
                        return;
                    }

                    if ((Find.TickManager.TicksGame + pawn.thingIDNumber) % 4 == 0)
                    {
                        //base.CheckForAutoAttack();
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Never
            };
        }
    }
}