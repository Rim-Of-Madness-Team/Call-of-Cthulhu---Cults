// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using System.Collections.Generic;
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
    public class JobDriver_MidnightInquisition : JobDriver
    {
        private readonly TargetIndex InquisitorIndex = TargetIndex.A;
        private readonly TargetIndex PreacherIndex = TargetIndex.B;


        private bool firstHit = true;
        private bool notifiedPlayer;

        private Pawn Preacher => job.GetTarget(ind: TargetIndex.B).Thing as Pawn;

        protected Pawn Inquisitor => (Pawn) job.GetTarget(ind: TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            //
            var toil = new Toil
            {
                initAction = delegate
                {
                    //Empty
                }
            };


            this.EndOnDespawnedOrNull(ind: InquisitorIndex);
            this.EndOnDespawnedOrNull(ind: PreacherIndex);
            //this.EndOnDespawnedOrNull(Build, JobCondition.Incompletable);
            yield return Toils_Reserve.Reserve(ind: PreacherIndex, maxPawns: job.def.joyMaxParticipants);
            var gotoPreacher = Toils_Goto.GotoThing(ind: PreacherIndex, peMode: PathEndMode.ClosestTouch);
            yield return gotoPreacher;

            if (Preacher.jobs.curDriver.asleep)
            {
                var watchToil = new Toil
                {
                    defaultCompleteMode = ToilCompleteMode.Delay,
                    defaultDuration = job.def.joyDuration
                };
                watchToil.AddPreTickAction(newAct: () =>
                {
                    pawn.rotationTracker.FaceCell(c: Preacher.Position);
                    pawn.GainComfortFromCellIfPossible();
                });
                yield return watchToil;
            }

            void hitAction()
            {
                var prey = Preacher;
                var surpriseAttack = firstHit;
                if (pawn.meleeVerbs.TryMeleeAttack(target: prey, verbToUse: job.verbToUse, surpriseAttack: surpriseAttack))
                {
                    if (!notifiedPlayer && PawnUtility.ShouldSendNotificationAbout(p: prey))
                    {
                        notifiedPlayer = true;
                        if (Prefs.AutomaticPauseMode > AutomaticPauseMode.Never && !Find.TickManager.Paused)
                        {
                            Find.TickManager.TogglePaused();
                        }

                        Messages.Message(text: "MessageAttackedByPredator".Translate(
                            arg1: prey.LabelShort,
                            arg2: pawn.LabelShort
                        ).CapitalizeFirst(), lookTargets: prey, def: MessageTypeDefOf.ThreatBig);
                    }

                    pawn.Map.attackTargetsCache.UpdateTarget(t: pawn);
                }

                firstHit = false;
            }

            yield return Toils_Combat.FollowAndMeleeAttack(targetInd: TargetIndex.A, hitAction: hitAction)
                .JumpIfDespawnedOrNull(ind: TargetIndex.A, jumpToil: toil).FailOn(condition: () =>
                    Find.TickManager.TicksGame > startTick + 5000 &&
                    (job.GetTarget(ind: TargetIndex.A).Cell - pawn.Position).LengthHorizontalSquared > 4f);
            yield return toil;

            AddFinishAction(newAct: () =>
            {
                //if (this.TargetB.HasThing)
                //{
                //    Find.Reservations.Release(this.job.targetC.Thing, pawn);
                //}
            });
        }
    }
}