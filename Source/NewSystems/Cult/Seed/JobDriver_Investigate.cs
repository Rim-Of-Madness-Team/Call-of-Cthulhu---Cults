// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

// ----------------------------------------------------------------------
// These are RimWorld-specific usings. Activate/Deactivate what you need:
// ----------------------------------------------------------------------
using UnityEngine;         // Always needed
//using VerseBase;         // Material/Graphics handling functions are found here
using Verse;               // RimWorld universal objects are here (like 'Building')
using Verse.AI;          // Needed when you do something with the AI
using Verse.AI.Group;
using Verse.Sound;       // Needed when you do something with Sound
using Verse.Noise;       // Needed when you do something with Noises
using RimWorld;            // RimWorld specific functions are found here (like 'Building_Battery')
using RimWorld.Planet;   // RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

namespace CultOfCthulhu
{

    public class JobDriver_Investigate : JobDriver
    {
        public override bool TryMakePreToilReservations()
        {
            return true;
        }

        private TargetIndex InvestigatorIndex = TargetIndex.A;
        private TargetIndex InvestigateeIndex = TargetIndex.B;

        protected Thing Investigatee
        {
            get
            {
                return base.job.GetTarget(TargetIndex.B).Thing;
            }
        }

        protected Pawn Investigator
        {
            get
            {
                return (Pawn)base.job.GetTarget(TargetIndex.A).Thing;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.EndOnDespawnedOrNull(InvestigatorIndex, JobCondition.Incompletable);
            this.EndOnDespawnedOrNull(InvestigateeIndex, JobCondition.Incompletable);
            //this.EndOnDespawnedOrNull(Build, JobCondition.Incompletable);
            yield return Toils_Reserve.Reserve(InvestigateeIndex, this.job.def.joyMaxParticipants);
            Toil gotoInvestigatee;
            gotoInvestigatee = Toils_Goto.GotoThing(InvestigateeIndex, PathEndMode.ClosestTouch);
            yield return gotoInvestigatee;

            yield return Toils_Goto.GotoCell(Investigatee.InteractionCell, PathEndMode.OnCell);

            Toil watchToil = new Toil();
            watchToil.defaultCompleteMode = ToilCompleteMode.Delay;
            watchToil.defaultDuration = this.job.def.joyDuration;
            watchToil.WithProgressBarToilDelay(InvestigatorIndex);
            watchToil.AddPreTickAction(() =>
            {
                this.pawn.rotationTracker.FaceCell(this.TargetB.Cell);
                this.pawn.GainComfortFromCellIfPossible();
            });
            watchToil.AddFinishAction(() =>
            {
                Map.GetComponent<MapComponent_LocalCultTracker>().CurrentSeedState = CultSeedState.FinishedSeeing;
            });
            yield return watchToil;

            this.AddFinishAction(() =>
            {
                //When the investigation is finished, apply effects.
                if (Map.GetComponent<MapComponent_LocalCultTracker>().CurrentSeedState == CultSeedState.FinishedSeeing)
                {
                    CultUtility.InvestigatedCultSeed(Investigator, Investigatee);
                    Cthulhu.Utility.DebugReport("Called end tick check");
                }
                //if (this.TargetB.HasThing)
                //{
                //    Find.Reservations.Release(this.job.targetC.Thing, pawn);
                //}
            });
        }
    }
}
