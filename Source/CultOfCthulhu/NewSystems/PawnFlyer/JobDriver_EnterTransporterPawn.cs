using System.Collections.Generic;
using System.Diagnostics;
using Cthulhu;
using Verse;
using Verse.AI;

namespace CultOfCthulhu
{
    //RimWorld.JobDriver_EnterTransporter
    public class JobDriver_EnterTransporterPawn : JobDriver
    {
        private readonly TargetIndex TransporterInd = TargetIndex.A;

        private CompTransporterPawn Transporter
        {
            get
            {
                var thing = job.GetTarget(TransporterInd).Thing;
                return thing?.TryGetComp<CompTransporterPawn>();
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TransporterInd);
            yield return Toils_Reserve.Reserve(TransporterInd);
            yield return Toils_Goto.GotoThing(TransporterInd, PathEndMode.Touch);
            yield return new Toil
            {
                initAction = delegate
                {
                    Utility.DebugReport("EnterTransporterPawn Called");
                    var transporter = Transporter;
                    pawn.DeSpawn();
                    transporter.GetDirectlyHeldThings().TryAdd(pawn);
                    transporter.Notify_PawnEnteredTransporterOnHisOwn(pawn);
                }
            };
        }
    }
}