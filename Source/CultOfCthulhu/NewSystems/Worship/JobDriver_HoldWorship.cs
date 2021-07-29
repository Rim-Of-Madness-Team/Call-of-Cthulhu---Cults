// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;
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
    public class JobDriver_HoldWorship : JobDriver
    {
        private const TargetIndex AltarIndex = TargetIndex.A;

        private string report = "";

        public int TicksLeftInService = int.MaxValue;

        private Thing WorshipCaller;

        public bool Forced => job.playerForced;

        protected Building_SacrificialAltar DropAltar => (Building_SacrificialAltar) job.GetTarget(TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        public override string GetReport()
        {
            return report != "" ? ReportStringProcessed(report) : base.GetReport();
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            //Commence fail checks!
            this.FailOnDestroyedOrNull(TargetIndex.A);

            yield return Toils_Reserve.Reserve(AltarIndex, Building_SacrificialAltar.LyingSlotsCount);

            yield return new Toil
            {
                initAction = delegate
                {
                    DropAltar.ChangeState(Building_SacrificialAltar.State.worshipping,
                        Building_SacrificialAltar.WorshipState.gathering);
                }
            };

            //Who are we worshipping today?
            var deitySymbol = ((CosmicEntityDef) DropAltar.currentWorshipDeity.def).Symbol;
            var deityLabel = DropAltar.currentWorshipDeity.Label;

            var goToAltar = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

            //Toil 0: Activate any nearby Worship Callers.
            yield return new Toil
            {
                initAction = delegate
                {
                    bool validator(Thing x)
                    {
                        return x.TryGetComp<CompWorshipCaller>() != null;
                    }

                    var worshipCaller = GenClosest.ClosestThingReachable(DropAltar.Position, DropAltar.Map,
                        ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.ClosestTouch,
                        TraverseParms.For(pawn, Danger.None), 9999, validator);
                    if (worshipCaller != null)
                    {
                        WorshipCaller = worshipCaller;
                        job.SetTarget(TargetIndex.B, worshipCaller);
                    }
                    else
                    {
                        JumpToToil(goToAltar);
                    }
                }
            };

            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch)
                .JumpIfDespawnedOrNullOrForbidden(TargetIndex.B, goToAltar);
            yield return new Toil
            {
                initAction = delegate { WorshipCaller.TryGetComp<CompWorshipCaller>().Use(Forced); }
            }.JumpIfDespawnedOrNullOrForbidden(TargetIndex.B, goToAltar);

            //Toil 1: Go to the altar.
            yield return goToAltar;

            //Toil 2: Wait a bit for stragglers.
            var waitingTime = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = CultUtility.ritualDuration,
                initAction = delegate
                {
                    report = "Cults_WaitingToStartSermon".Translate();
                    DropAltar.ChangeState(Building_SacrificialAltar.State.worshipping,
                        Building_SacrificialAltar.WorshipState.worshipping);
                }
            };

            yield return waitingTime;

            //Toil 3: Preach the sermon.
            var preachingTime = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = CultUtility.ritualDuration,
                initAction = delegate
                {
                    report = "Cults_PreachingAbout".Translate(
                        deityLabel
                    );
                    if (deitySymbol != null)
                    {
                        MoteMaker.MakeInteractionBubble(pawn, null, ThingDefOf.Mote_Speech, deitySymbol);
                    }
                },
                tickAction = delegate
                {
                    var actor = pawn;
                    actor.skills.Learn(SkillDefOf.Social, 0.25f);
                    actor.GainComfortFromCellIfPossible();
                }
            };

            yield return preachingTime;

            //Toil 4: Time to pray
            var chantingTime = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = CultUtility.ritualDuration
            };
            chantingTime.WithProgressBarToilDelay(TargetIndex.A);
            chantingTime.PlaySustainerOrSound(CultsDefOf.RitualChanting);
            chantingTime.initAction = delegate
            {
                report = "Cults_PrayingTo".Translate(
                    deityLabel
                );
                if (deitySymbol != null)
                {
                    MoteMaker.MakeInteractionBubble(pawn, null, ThingDefOf.Mote_Speech, deitySymbol);
                }
            };
            chantingTime.tickAction = delegate
            {
                var actor = pawn;
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
                    CultUtility.WorshipComplete(pawn, DropAltar, DropAltar.currentWorshipDeity);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };

            yield return new Toil
            {
                initAction = delegate
                {
                    if (DropAltar == null)
                    {
                        return;
                    }

                    if (DropAltar.currentWorshipState != Building_SacrificialAltar.WorshipState.finished)
                    {
                        DropAltar.ChangeState(Building_SacrificialAltar.State.worshipping,
                            Building_SacrificialAltar.WorshipState.finished);
                        //Map.GetComponent<MapComponent_SacrificeTracker>().ClearVariables();
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };


            AddFinishAction(() =>
            {
                //When the ritual is finished -- then let's give the thoughts
                if (DropAltar.currentWorshipState != Building_SacrificialAltar.WorshipState.finishing &&
                    DropAltar.currentWorshipState != Building_SacrificialAltar.WorshipState.finished)
                {
                    return;
                }

                Utility.DebugReport("Called end tick check");
                CultUtility.HoldWorshipTickCheckEnd(pawn);
            });
        }
    }
}