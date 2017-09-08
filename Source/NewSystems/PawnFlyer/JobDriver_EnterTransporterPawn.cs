using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;
using RimWorld;

namespace CultOfCthulhu
{
    //RimWorld.JobDriver_EnterTransporter
    public class JobDriver_EnterTransporterPawn : JobDriver
    {
        private TargetIndex TransporterInd = TargetIndex.A;

        private CompTransporterPawn Transporter
        {
            get
            {
                Thing thing = base.CurJob.GetTarget(this.TransporterInd).Thing;
                if (thing == null)
                {
                    return null;
                }
                return thing.TryGetComp<CompTransporterPawn>();
            }
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(this.TransporterInd);
            yield return Toils_Reserve.Reserve(this.TransporterInd, 1);
            yield return Toils_Goto.GotoThing(this.TransporterInd, PathEndMode.Touch);
            yield return new Toil
            {
                initAction = delegate
                {
                    Cthulhu.Utility.DebugReport("EnterTransporterPawn Called");
                    CompTransporterPawn transporter = this.Transporter;
                    this.pawn.DeSpawn();
                    transporter.GetDirectlyHeldThings().TryAdd(this.pawn, true);
                    transporter.Notify_PawnEnteredTransporterOnHisOwn(this.pawn);
                }
            };
            yield break;
        }
    }
}
