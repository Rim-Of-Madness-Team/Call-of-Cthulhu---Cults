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
    public class JobDriver_MidnightInquisition : JobDriver
    {
        private TargetIndex InquisitorIndex = TargetIndex.A;
        private TargetIndex PreacherIndex = TargetIndex.B;

        protected Pawn Preacher
        {
            get
            {
                return base.CurJob.GetTarget(TargetIndex.B).Thing as Pawn;
            }
        }

        protected Pawn Inquisitor
        {
            get
            {
                return (Pawn)base.CurJob.GetTarget(TargetIndex.A).Thing;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }


        private bool firstHit = true;
        private bool notifiedPlayer = false;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            //
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                //Empty
            };


            this.EndOnDespawnedOrNull(InquisitorIndex, JobCondition.Incompletable);
            this.EndOnDespawnedOrNull(PreacherIndex, JobCondition.Incompletable);
            //this.EndOnDespawnedOrNull(Build, JobCondition.Incompletable);
            yield return Toils_Reserve.Reserve(PreacherIndex, this.CurJob.def.joyMaxParticipants);
            Toil gotoPreacher;
            gotoPreacher = Toils_Goto.GotoThing(PreacherIndex, PathEndMode.ClosestTouch);
            yield return gotoPreacher;

            if (Preacher.jobs.curDriver.asleep)
            {
                Toil watchToil = new Toil();
                watchToil.defaultCompleteMode = ToilCompleteMode.Delay;
                watchToil.defaultDuration = this.CurJob.def.joyDuration;
                watchToil.AddPreTickAction(() =>
                {
                    this.pawn.Drawer.rotator.FaceCell(Preacher.Position);
                    this.pawn.GainComfortFromCellIfPossible();
                });
                yield return watchToil;
            }

            Action hitAction = delegate
            {
                Pawn prey = Preacher;
                bool surpriseAttack = this.firstHit;
                if (pawn.meleeVerbs.TryMeleeAttack(prey, this.CurJob.verbToUse, surpriseAttack))
                {
                    if (!this.notifiedPlayer && PawnUtility.ShouldSendNotificationAbout(prey))
                    {
                        this.notifiedPlayer = true;
                        if (Prefs.PauseOnUrgentLetter && !Find.TickManager.Paused)
                        {
                            Find.TickManager.TogglePaused();
                        }
                        Messages.Message("MessageAttackedByPredator".Translate(new object[]
                        {
                            prey.LabelShort,
                            this.pawn.LabelShort,
                        }).CapitalizeFirst(), prey, MessageSound.SeriousAlert);
                    }
                    this.pawn.Map.attackTargetsCache.UpdateTarget(this.pawn);
                }
                this.firstHit = false;
            };
            yield return Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, hitAction).JumpIfDespawnedOrNull(TargetIndex.A, toil).FailOn(() => Find.TickManager.TicksGame > this.startTick + 5000 && (this.CurJob.GetTarget(TargetIndex.A).Cell - this.pawn.Position).LengthHorizontalSquared > 4f);
            yield return toil;

            this.AddFinishAction(() =>
            {
                //if (this.TargetB.HasThing)
                //{
                //    Find.Reservations.Release(this.CurJob.targetC.Thing, pawn);
                //}
            });
        }
    }
}
