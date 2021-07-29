// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using System.Collections.Generic;
using Cthulhu;
using RimWorld;
using Verse;
using Verse.AI;

// ----------------------------------------------------------------------
// These are RimWorld-specific usings. Activate/Deactivate what you need:
// ----------------------------------------------------------------------
// Always needed
//using VerseBase;         // Material/Graphics handling functions are found here
// RimWorld universal objects are here (like 'Building')
// Needed when you do something with the AI
// Needed when you do something with Sound
// Needed when you do something with Noises
// RimWorld specific functions are found here (like 'Building_Battery')

// RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

namespace CultOfCthulhu
{
    public class JobDriver_Investigate : JobDriver
    {
        private readonly TargetIndex InvestigateeIndex = TargetIndex.B;

        private readonly TargetIndex InvestigatorIndex = TargetIndex.A;

        protected Thing Investigatee => job.GetTarget(TargetIndex.B).Thing;

        protected Pawn Investigator => (Pawn) job.GetTarget(TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.EndOnDespawnedOrNull(InvestigatorIndex);
            this.EndOnDespawnedOrNull(InvestigateeIndex);
            //this.EndOnDespawnedOrNull(Build, JobCondition.Incompletable);
            yield return Toils_Reserve.Reserve(InvestigateeIndex, job.def.joyMaxParticipants);
            var gotoInvestigatee = Toils_Goto.GotoThing(InvestigateeIndex, PathEndMode.ClosestTouch);
            yield return gotoInvestigatee;

            yield return Toils_Goto.GotoCell(Investigatee.InteractionCell, PathEndMode.OnCell);

            var watchToil = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = job.def.joyDuration
            };
            watchToil.WithProgressBarToilDelay(InvestigatorIndex);
            watchToil.AddPreTickAction(() =>
            {
                pawn.rotationTracker.FaceCell(TargetB.Cell);
                pawn.GainComfortFromCellIfPossible();
            });
            watchToil.AddFinishAction(() =>
                Map.GetComponent<MapComponent_LocalCultTracker>().CurrentSeedState = CultSeedState.FinishedSeeing);
            yield return watchToil;

            AddFinishAction(() =>
            {
                //When the investigation is finished, apply effects.
                if (Map.GetComponent<MapComponent_LocalCultTracker>().CurrentSeedState != CultSeedState.FinishedSeeing)
                {
                    return;
                }

                CultUtility.InvestigatedCultSeed(Investigator, Investigatee);
                Utility.DebugReport("Called end tick check");

                //if (this.TargetB.HasThing)
                //{
                //    Find.Reservations.Release(this.job.targetC.Thing, pawn);
                //}
            });
        }
    }
}