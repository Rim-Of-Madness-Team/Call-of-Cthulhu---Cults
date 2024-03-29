﻿// ----------------------------------------------------------------------
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
    public class JobDriver_AttendSacrifice : JobDriver
    {
        private readonly TargetIndex Build = TargetIndex.A;
        private readonly TargetIndex Facing = TargetIndex.B;
        private readonly TargetIndex Spot = TargetIndex.C;


        private string report = "";

        private Pawn setExecutioner;

        protected Building_SacrificialAltar Altar => (Building_SacrificialAltar) job.GetTarget(ind: TargetIndex.A).Thing;

        protected Pawn ExecutionerPawn
        {
            get
            {
                if (setExecutioner != null)
                {
                    return setExecutioner;
                }

                if (Altar.SacrificeData.Executioner != null)
                {
                    setExecutioner = Altar.SacrificeData.Executioner;
                    return Altar.SacrificeData.Executioner;
                }

                foreach (var executionerPawn in pawn.Map.mapPawns.FreeColonistsSpawned)
                {
                    if (executionerPawn.CurJob.def != CultsDefOf.Cults_HoldSacrifice)
                    {
                        continue;
                    }

                    setExecutioner = executionerPawn;
                    return executionerPawn;
                }

                return null;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        public override void ExposeData()
        {
            Scribe_References.Look(refee: ref setExecutioner, label: "setExecutioner");
            base.ExposeData();
        }

        public override string GetReport()
        {
            return report != "" ? ReportStringProcessed(str: report) : base.GetReport();
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            rotateToFace = Facing;


            AddEndCondition(newEndCondition: delegate
            {
                if (ExecutionerPawn?.CurJob == null)
                {
                    return JobCondition.Incompletable;
                }

                if (ExecutionerPawn.CurJob.def == CultsDefOf.Cults_ReflectOnResult)
                {
                    return JobCondition.Succeeded;
                }

                if (ExecutionerPawn.CurJob.def != CultsDefOf.Cults_HoldSacrifice)
                {
                    return JobCondition.Incompletable;
                }

                return JobCondition.Ongoing;
            });

            this.EndOnDespawnedOrNull(ind: Spot);
            this.EndOnDespawnedOrNull(ind: Build);


            yield return Toils_Reserve.Reserve(ind: Spot);

            //Toil 1: Go to the locations
            var gotoExecutioner = TargetC.HasThing
                ? Toils_Goto.GotoThing(ind: Spot, peMode: PathEndMode.OnCell)
                : Toils_Goto.GotoCell(ind: Spot, peMode: PathEndMode.OnCell);

            yield return gotoExecutioner;

            //Toil 2: 'Attend'
            var altarToil = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = CultUtility.ritualDuration
            };
            altarToil.AddPreTickAction(newAct: () =>
            {
                pawn.GainComfortFromCellIfPossible();
                pawn.rotationTracker.FaceCell(c: TargetB.Cell);
                if (report == "")
                {
                    report = "Cults_AttendingSacrifice".Translate();
                }

                if (ExecutionerPawn?.CurJob == null)
                {
                    return;
                }

                if (ExecutionerPawn.CurJob.def != CultsDefOf.Cults_HoldSacrifice)
                {
                    ReadyForNextToil();
                }
            });
            altarToil.JumpIf(jumpCondition: () => ExecutionerPawn.CurJob.def == CultsDefOf.Cults_HoldSacrifice, jumpToil: altarToil);
            yield return altarToil;

            //ToDo -- Add random Ia! Ia!
            yield return new Toil
            {
                initAction = delegate
                {
                    //Do something? Ia ia!
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };

            //Toil 3 Reflect on worship
            var reflectingTime = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = CultUtility.reflectDuration
            };
            reflectingTime.AddPreTickAction(newAct: () => report = "Cults_ReflectingOnSacrifice".Translate());
            yield return reflectingTime;

            //Toil 3 Reset the altar and clear variables.
            yield return new Toil
            {
                initAction = delegate
                {
                    if (Altar == null)
                    {
                        return;
                    }

                    if (Altar.currentSacrificeState != Building_SacrificialAltar.SacrificeState.finished)
                    {
                        Altar.ChangeState(type: Building_SacrificialAltar.State.sacrificing,
                            sacrificeState: Building_SacrificialAltar.SacrificeState.finished);
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };


            AddFinishAction(newAct: () =>
            {
                //When the ritual is finished -- then let's give the thoughts
                /*
                if (Altar.currentSacrificeState == Building_SacrificialAltar.SacrificeState.finished)
                {
                    if (this.pawn == null) return;
                    if (Altar.sacrifice != null)
                    {                        
                        CultUtility.AttendSacrificeTickCheckEnd(this.pawn, Altar.sacrifice);
                    }
                    else
                    {
                        CultUtility.AttendSacrificeTickCheckEnd(this.pawn, null);
                    }
                }
                */
                if (TargetC.Cell.GetEdifice(map: pawn.Map) != null)
                {
                    if (pawn.Map.reservationManager.ReservedBy(target: TargetC.Cell.GetEdifice(map: pawn.Map), claimant: pawn))
                    {
                        pawn.ClearAllReservations(); // this.pawn.Map.reservationManager.Release(this.TargetC.Cell.GetEdifice(this.pawn.Map), pawn);
                    }
                }
                else
                {
                    if (pawn.Map.reservationManager.ReservedBy(target: TargetC.Cell.GetEdifice(map: pawn.Map), claimant: pawn))
                    {
                        pawn.ClearAllReservations(); //this.pawn.Map.reservationManager.Release(this.job.targetC.Cell, this.pawn);
                    }
                }
            });
        }
    }
}