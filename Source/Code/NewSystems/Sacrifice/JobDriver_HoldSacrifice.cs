// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;
using Cthulhu;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

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
    public class JobDriver_HoldSacrifice : JobDriver
    {
        private const TargetIndex TakeeIndex = TargetIndex.A;
        private const TargetIndex AltarIndex = TargetIndex.B;

        protected Pawn Takee => (Pawn) job.GetTarget(ind: TargetIndex.A).Thing;

        protected Building_SacrificialAltar DropAltar => (Building_SacrificialAltar) job.GetTarget(ind: TargetIndex.B).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            //Commence fail checks!

            this.FailOnDestroyedOrNull(ind: TargetIndex.A);
            this.FailOnDestroyedOrNull(ind: TargetIndex.B);
            this.FailOnAggroMentalState(ind: TargetIndex.A);

            yield return Toils_Reserve.Reserve(ind: TakeeIndex);
            yield return Toils_Reserve.Reserve(ind: AltarIndex, maxPawns: Building_SacrificialAltar.LyingSlotsCount);

            yield return new Toil
            {
                initAction = delegate
                {
                    DropAltar.ChangeState(type: Building_SacrificialAltar.State.sacrificing,
                        sacrificeState: Building_SacrificialAltar.SacrificeState.gathering);
                }
            };

            //Toil 1: Go to prisoner.
            yield return Toils_Goto.GotoThing(ind: TargetIndex.A, peMode: PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden(ind: TargetIndex.A).FailOnDespawnedNullOrForbidden(ind: TargetIndex.B)
                .FailOn(condition: () => job.def == JobDefOf.Arrest && !Takee.CanBeArrestedBy(arrester: pawn))
                .FailOn(condition: () =>
                    !pawn.CanReach(dest: DropAltar, peMode: PathEndMode.OnCell, maxDanger: Danger.Deadly))
                .FailOnSomeonePhysicallyInteracting(ind: TargetIndex.A);
            yield return new Toil
            {
                initAction = delegate
                {
                    if (!job.def.makeTargetPrisoner)
                    {
                        return;
                    }

                    var targetAThing = (Pawn) job.targetA.Thing;
                    var lord = targetAThing.GetLord();
                    lord?.Notify_PawnAttemptArrested(victim: targetAThing);

                    GenClamor.DoClamor(source: targetAThing, radius: 10f, type: ClamorDefOf.Harm);
                    if (job.def == JobDefOf.Arrest && !targetAThing.CheckAcceptArrest(arrester: pawn))
                    {
                        pawn.jobs.EndCurrentJob(condition: JobCondition.Incompletable);
                    }
                }
            };
            //Toil 2: Carry prisoner.
            yield return Toils_Haul.StartCarryThing(haulableInd: TargetIndex.A);
            //Toil 3: Go to the altar.
            yield return Toils_Goto.GotoThing(ind: TargetIndex.B, peMode: PathEndMode.InteractionCell);
            //Toil 4: Release the prisoner.
            yield return Toils_Reserve.Release(ind: TargetIndex.B);
            //Toil 5: Restrain the prisoner.
            yield return new Toil
            {
                initAction = delegate
                {
                    //In-case this fails...
                    var position = DropAltar.Position;
                    pawn.carryTracker.TryDropCarriedThing(dropLoc: position, mode: ThingPlaceMode.Direct, resultingThing: out _);
                    if (DropAltar.Destroyed || !DropAltar.AnyUnoccupiedLyingSlot)
                    {
                        return;
                    }

                    Takee.Position = DropAltar.GetLyingSlotPos();
                    Takee.Notify_Teleported(endCurrentJob: false);
                    Takee.stances.CancelBusyStanceHard();
                    var newJob = new Job(def: CultsDefOf.Cults_WaitTiedDown, targetA: DropAltar);
                    Takee.jobs.StartJob(newJob: newJob);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };

            //Toil 6: Time to chant ominously
            var chantingTime = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = CultUtility.ritualDuration
            };
            chantingTime.WithProgressBarToilDelay(ind: TargetIndex.A);
            chantingTime.PlaySustainerOrSound(soundDef: CultsDefOf.RitualChanting);
            var deitySymbol = ((CosmicEntityDef) DropAltar.SacrificeData.Entity.def).Symbol;
            chantingTime.initAction = delegate
            {
                if (deitySymbol != null)
                {
                    MoteMaker.MakeInteractionBubble(initiator: pawn, recipient: null, interactionMote: ThingDefOf.Mote_Speech, symbol: deitySymbol);
                }


                //STATE - SACRIFICING
                DropAltar.ChangeState(type: Building_SacrificialAltar.State.sacrificing,
                    sacrificeState: Building_SacrificialAltar.SacrificeState.sacrificing);
            };

            yield return chantingTime;

            //Toil 8: Execution of Prisoner
            yield return new Toil
            {
                initAction = delegate
                {
                    //BodyPartDamageInfo value = new BodyPartDamageInfo(this.Takee.health.hediffSet.GetBrain(), false, quiet);
                    Takee.TakeDamage(dinfo: new DamageInfo(def: DamageDefOf.ExecutionCut, amount: 99999, armorPenetration: 0f, angle: -1f, instigator: pawn,
                        hitPart: Utility.GetHeart(set: Takee.health.hediffSet)));
                    if (!Takee.Dead)
                    {
                        Takee.Kill(dinfo: null);
                    }

                    //ThoughtUtility.GiveThoughtsForPawnExecuted(this.Takee, PawnExecutionKind.GenericHumane);
                    TaleRecorder.RecordTale(def: TaleDefOf.ExecutedPrisoner, pawn, Takee);
                    CultUtility.SacrificeExecutionComplete(altar: DropAltar);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };

            AddFinishAction(newAct: () =>
            {
                //It's a day to remember
                var taleToAdd = TaleDef.Named(str: "HeldSermon");
                if ((pawn.IsColonist || pawn.IsSlaveOfColony || pawn.HostFaction == Faction.OfPlayer) && taleToAdd != null)
                {
                    TaleRecorder.RecordTale(def: taleToAdd, pawn);
                }

                //When the ritual is finished -- then let's give the thoughts
                /*
                if (DropAltar.currentSacrificeState == Building_SacrificialAltar.SacrificeState.finished)
                {
                    if (this.pawn == null) return;
                    if (DropAltar.sacrifice != null)
                    {
                        CultUtility.AttendSacrificeTickCheckEnd(this.pawn, DropAltar.sacrifice, true);
                    }
                    else
                    {
                        CultUtility.AttendSacrificeTickCheckEnd(this.pawn, null);
                    }
                }
                */
            });
        }
    }
}