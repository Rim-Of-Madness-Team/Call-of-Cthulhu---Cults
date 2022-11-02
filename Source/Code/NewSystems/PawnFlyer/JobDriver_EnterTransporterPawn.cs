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
                var thing = job.GetTarget(ind: TransporterInd).Thing;
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
            this.FailOnDespawnedOrNull(ind: TransporterInd);
            yield return Toils_Reserve.Reserve(ind: TransporterInd);
            yield return Toils_Goto.GotoThing(ind: TransporterInd, peMode: PathEndMode.Touch);
            yield return new Toil
            {
                initAction = delegate
                {
                    Utility.DebugReport(x: "EnterTransporterPawn Called");
                    var transporter = Transporter;
                    pawn.DeSpawn();
                    transporter.GetDirectlyHeldThings().TryAdd(item: pawn);
                    transporter.Notify_PawnEnteredTransporterOnHisOwn(p: pawn);
                }
            };
        }
    }
}