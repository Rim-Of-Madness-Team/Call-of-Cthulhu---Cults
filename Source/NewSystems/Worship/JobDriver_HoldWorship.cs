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
    public class JobDriver_HoldWorship : JobDriver
    {
        private const TargetIndex AltarIndex = TargetIndex.A;

        protected Building_SacrificialAltar DropAltar
        {
            get
            {
                return (Building_SacrificialAltar)base.CurJob.GetTarget(TargetIndex.A).Thing;
            }
        }


        private string report = "";
        public override string GetReport()
        {
            if (report != "")
            {
                return base.ReportStringProcessed(report);
            }
            return base.GetReport();
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            //Commence fail checks!
            this.FailOnDestroyedOrNull(TargetIndex.A);
            
            yield return Toils_Reserve.Reserve(AltarIndex, this.DropAltar.LyingSlotsCount);

            yield return new Toil
            {
                initAction = delegate
                {
                    DropAltar.ChangeState(Building_SacrificialAltar.State.worshipping, Building_SacrificialAltar.WorshipState.gathering);
                }
            };

            //Who are we worshipping today?
            var deitySymbol = ((CosmicEntityDef)DropAltar.currentWorshipDeity.def).Symbol;
            string deityLabel = DropAltar.currentWorshipDeity.Label;

            //Toil 1: Go to the altar.
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

            //Toil 2: Wait a bit for stragglers.
            Toil waitingTime = new Toil();
            waitingTime.defaultCompleteMode = ToilCompleteMode.Delay;
            CultUtility.remainingDuration = CultUtility.ritualDuration;
            waitingTime.defaultDuration = CultUtility.remainingDuration - 360;
            waitingTime.initAction = delegate
            {
                report = "Cults_WaitingToStartSermon".Translate();
                DropAltar.ChangeState(Building_SacrificialAltar.State.worshipping, Building_SacrificialAltar.WorshipState.worshipping);
            };

            yield return waitingTime;

            //Toil 3: Preach the sermon.
            Toil preachingTime = new Toil();
            preachingTime.defaultCompleteMode = ToilCompleteMode.Delay;
            CultUtility.remainingDuration = CultUtility.ritualDuration;
            preachingTime.defaultDuration = CultUtility.remainingDuration - 360;
            preachingTime.initAction = delegate
            {
                report = "Cults_PreachingAbout".Translate(new object[]
                {
                    deityLabel
                });
                if (deitySymbol != null)
                    MoteMaker.MakeInteractionBubble(this.pawn, null, ThingDefOf.Mote_Speech, deitySymbol);
            };
            preachingTime.tickAction = delegate
            {
                Pawn actor = this.pawn;
                actor.skills.Learn(SkillDefOf.Social, 0.25f);
                actor.GainComfortFromCellIfPossible();
            };

            yield return preachingTime;

            //Toil 4: Time to pray
            Toil chantingTime = new Toil();
            chantingTime.defaultCompleteMode = ToilCompleteMode.Delay;
            CultUtility.remainingDuration = CultUtility.ritualDuration;
            chantingTime.defaultDuration = CultUtility.remainingDuration - 360;
            chantingTime.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            chantingTime.PlaySustainerOrSound(CultsDefOf.RitualChanting);
            chantingTime.initAction = delegate
            {
                report = "Cults_PrayingTo".Translate(new object[]
                    {
                        deityLabel
                    });
                if (deitySymbol != null)
                    MoteMaker.MakeInteractionBubble(this.pawn, null, ThingDefOf.Mote_Speech, deitySymbol);
            };
            chantingTime.tickAction = delegate
            {
                Pawn actor = this.pawn;
                actor.skills.Learn(SkillDefOf.Social, 0.25f);
                actor.GainComfortFromCellIfPossible();
            };

            yield return chantingTime;

            //Toil 8: Execution of Prisoner
            yield return new Toil
            {
                initAction = delegate
                {
                    //TaleRecorder.RecordTale(
                    // Of.ExecutedPrisoner, new object[]
                    //{
                    //    this.pawn,
                    //    this.Takee
                    //});
                    CultUtility.WorshipComplete(this.pawn, DropAltar, DropAltar.currentWorshipDeity);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };

            yield return new Toil
            {
                initAction = delegate
                {
                    if (DropAltar != null)
                    {
                        if (DropAltar.currentWorshipState != Building_SacrificialAltar.WorshipState.finished)
                        {
                            DropAltar.ChangeState(Building_SacrificialAltar.State.worshipping, Building_SacrificialAltar.WorshipState.finished);
                            //Map.GetComponent<MapComponent_SacrificeTracker>().ClearVariables();
                        }
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };


            this.AddFinishAction(() =>
            {
                //When the ritual is finished -- then let's give the thoughts
                if (DropAltar.currentWorshipState == Building_SacrificialAltar.WorshipState.finishing ||
                    DropAltar.currentWorshipState == Building_SacrificialAltar.WorshipState.finished)
                {
                    Cthulhu.Utility.DebugReport("Called end tick check");
                    CultUtility.HoldWorshipTickCheckEnd(this.pawn);
                }

            });

            yield break;


        }
    }
}
