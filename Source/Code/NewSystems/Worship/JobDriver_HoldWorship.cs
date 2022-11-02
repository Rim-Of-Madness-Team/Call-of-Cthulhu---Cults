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

        protected Building_SacrificialAltar DropAltar => (Building_SacrificialAltar) job.GetTarget(ind: TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        public override string GetReport()
        {
            return report != "" ? ReportStringProcessed(str: report) : base.GetReport();
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            //Commence fail checks!
            this.FailOnDestroyedOrNull(ind: TargetIndex.A);

            yield return Toils_Reserve.Reserve(ind: AltarIndex, maxPawns: Building_SacrificialAltar.LyingSlotsCount);

            yield return new Toil
            {
                initAction = delegate
                {
                    DropAltar.ChangeState(type: Building_SacrificialAltar.State.worshipping,
                        worshipState: Building_SacrificialAltar.WorshipState.gathering);
                }
            };

            //Who are we worshipping today?
            var deitySymbol = ((CosmicEntityDef) DropAltar.currentWorshipDeity.def).Symbol;
            var deityLabel = DropAltar.currentWorshipDeity.Label;

            var goToAltar = Toils_Goto.GotoThing(ind: TargetIndex.A, peMode: PathEndMode.InteractionCell);

            //Toil 0: Activate any nearby Worship Callers.
            yield return new Toil
            {
                initAction = delegate
                {
                    bool validator(Thing x)
                    {
                        return x.TryGetComp<CompWorshipCaller>() != null;
                    }

                    var worshipCaller = GenClosest.ClosestThingReachable(root: DropAltar.Position, map: DropAltar.Map,
                        thingReq: ThingRequest.ForGroup(@group: ThingRequestGroup.BuildingArtificial), peMode: PathEndMode.ClosestTouch,
                        traverseParams: TraverseParms.For(pawn: pawn, maxDanger: Danger.None), maxDistance: 9999, validator: validator);
                    if (worshipCaller != null)
                    {
                        WorshipCaller = worshipCaller;
                        job.SetTarget(ind: TargetIndex.B, pack: worshipCaller);
                    }
                    else
                    {
                        JumpToToil(to: goToAltar);
                    }
                }
            };

            yield return Toils_Goto.GotoThing(ind: TargetIndex.B, peMode: PathEndMode.ClosestTouch)
                .JumpIfDespawnedOrNullOrForbidden(ind: TargetIndex.B, jumpToil: goToAltar);
            yield return new Toil
            {
                initAction = delegate { WorshipCaller.TryGetComp<CompWorshipCaller>().Use(forced: Forced); }
            }.JumpIfDespawnedOrNullOrForbidden(ind: TargetIndex.B, jumpToil: goToAltar);

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
                    DropAltar.ChangeState(type: Building_SacrificialAltar.State.worshipping,
                        worshipState: Building_SacrificialAltar.WorshipState.worshipping);
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
                        arg1: deityLabel
                    );
                    if (deitySymbol != null)
                    {
                        MoteMaker.MakeInteractionBubble(initiator: pawn, recipient: null, interactionMote: ThingDefOf.Mote_Speech, symbol: deitySymbol);
                    }
                },
                tickAction = delegate
                {
                    var actor = pawn;
                    actor.skills.Learn(sDef: SkillDefOf.Social, xp: 0.25f);
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
            chantingTime.WithProgressBarToilDelay(ind: TargetIndex.A);
            chantingTime.PlaySustainerOrSound(soundDef: CultsDefOf.RitualChanting);
            chantingTime.initAction = delegate
            {
                report = "Cults_PrayingTo".Translate(
                    arg1: deityLabel
                );
                if (deitySymbol != null)
                {
                    MoteMaker.MakeInteractionBubble(initiator: pawn, recipient: null, interactionMote: ThingDefOf.Mote_Speech, symbol: deitySymbol);
                }
            };
            chantingTime.tickAction = delegate
            {
                var actor = pawn;
                actor.skills.Learn(sDef: SkillDefOf.Social, xp: 0.25f);
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
                    CultUtility.WorshipComplete(preacher: pawn, altar: DropAltar, deity: DropAltar.currentWorshipDeity);
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
                        DropAltar.ChangeState(type: Building_SacrificialAltar.State.worshipping,
                            worshipState: Building_SacrificialAltar.WorshipState.finished);
                        //Map.GetComponent<MapComponent_SacrificeTracker>().ClearVariables();
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };


            AddFinishAction(newAct: () =>
            {
                //When the ritual is finished -- then let's give the thoughts
                if (DropAltar.currentWorshipState != Building_SacrificialAltar.WorshipState.finishing &&
                    DropAltar.currentWorshipState != Building_SacrificialAltar.WorshipState.finished)
                {
                    return;
                }

                Utility.DebugReport(x: "Called end tick check");
                CultUtility.HoldWorshipTickCheckEnd(preacher: pawn);
            });
        }
    }
}