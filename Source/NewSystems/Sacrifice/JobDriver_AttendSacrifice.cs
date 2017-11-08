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
    public class JobDriver_AttendSacrifice : JobDriver
    {
        public override bool TryMakePreToilReservations()
        {
            return true;
        }
        private TargetIndex Build = TargetIndex.A;
        private TargetIndex Facing = TargetIndex.B;
        private TargetIndex Spot = TargetIndex.C;

        private Pawn setExecutioner = null;

        protected Building_SacrificialAltar Altar
        {
            get
            {
                return (Building_SacrificialAltar)base.job.GetTarget(TargetIndex.A).Thing;
            }
        }

        protected Pawn ExecutionerPawn
        {
            get
            {
                if (setExecutioner != null) return setExecutioner;
                if (Altar.executioner != null) { setExecutioner = Altar.executioner; return Altar.executioner; }
                else
                {
                    foreach (Pawn pawn in this.pawn.Map.mapPawns.FreeColonistsSpawned)
                    {
                        if (pawn.CurJob.def == CultsDefOf.Cults_HoldSacrifice) { setExecutioner = pawn; return pawn; }
                    }
                }
                return null;
            }
        }

        public override void ExposeData()
        {
            Scribe_References.Look<Pawn>(ref this.setExecutioner, "setExecutioner");
            base.ExposeData();
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
        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            rotateToFace = Facing;


            this.AddEndCondition(delegate
            {
                if (ExecutionerPawn == null)
                {
                    return JobCondition.Incompletable;
                }
                if (ExecutionerPawn.CurJob == null)
                {
                    return JobCondition.Incompletable;
                }

                if (ExecutionerPawn.CurJob.def == CultsDefOf.Cults_ReflectOnResult)
                {
                    return JobCondition.Succeeded;
                }
                else if (ExecutionerPawn.CurJob.def != CultsDefOf.Cults_HoldSacrifice)
                {
                    return JobCondition.Incompletable;
                }
                return JobCondition.Ongoing;
            });
            
            this.EndOnDespawnedOrNull(Spot, JobCondition.Incompletable);
            this.EndOnDespawnedOrNull(Build, JobCondition.Incompletable);


            yield return Toils_Reserve.Reserve(Spot, 1, -1);

            //Toil 1: Go to the locations
            Toil gotoExecutioner;
            if (this.TargetC.HasThing)
            {
                gotoExecutioner = Toils_Goto.GotoThing(Spot, PathEndMode.OnCell);
            }
            else
            {
                gotoExecutioner = Toils_Goto.GotoCell(Spot, PathEndMode.OnCell);
            }
            yield return gotoExecutioner;
            
            //Toil 2: 'Attend'
            var altarToil = new Toil();
            altarToil.defaultCompleteMode = ToilCompleteMode.Delay;
            altarToil.defaultDuration = CultUtility.ritualDuration;
            altarToil.AddPreTickAction(() =>
            {
                this.pawn.GainComfortFromCellIfPossible();
                this.pawn.rotationTracker.FaceCell(TargetB.Cell);
                if (report == "") report = "Cults_AttendingSacrifice".Translate();
                if (ExecutionerPawn != null)
                {
                    if (ExecutionerPawn.CurJob != null)
                    {
                        if (ExecutionerPawn.CurJob.def != CultsDefOf.Cults_HoldSacrifice)
                        {
                            this.ReadyForNextToil();
                        }
                    }
                }
            });
            altarToil.JumpIf(() => ExecutionerPawn.CurJob.def == CultsDefOf.Cults_HoldSacrifice, altarToil);
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
            Toil reflectingTime = new Toil();
            reflectingTime.defaultCompleteMode = ToilCompleteMode.Delay;
            reflectingTime.defaultDuration = CultUtility.reflectDuration;
            reflectingTime.AddPreTickAction(() =>
            {
                report = "Cults_ReflectingOnSacrifice".Translate();
            });
            yield return reflectingTime;

            //Toil 3 Reset the altar and clear variables.
            yield return new Toil
            {
                initAction = delegate
                {
                    if (Altar != null)
                    {
                        if (Altar.currentSacrificeState != Building_SacrificialAltar.SacrificeState.finished)
                        {
                            Altar.ChangeState(Building_SacrificialAltar.State.sacrificing, Building_SacrificialAltar.SacrificeState.finished);
                        }
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };


            this.AddFinishAction(() =>
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
                if (this.TargetC.Cell.GetEdifice(this.pawn.Map) != null)
                {
                    if (this.pawn.Map.reservationManager.ReservedBy(this.TargetC.Cell.GetEdifice(this.pawn.Map), this.pawn))
                        this.pawn.ClearAllReservations(); // this.pawn.Map.reservationManager.Release(this.TargetC.Cell.GetEdifice(this.pawn.Map), pawn);
                }
                else
                {
                    if (this.pawn.Map.reservationManager.ReservedBy(this.TargetC.Cell.GetEdifice(this.pawn.Map), this.pawn))
                        this.pawn.ClearAllReservations();  //this.pawn.Map.reservationManager.Release(this.job.targetC.Cell, this.pawn);
                }
            });
        }
    }
}
