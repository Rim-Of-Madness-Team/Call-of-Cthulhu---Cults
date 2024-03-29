﻿// ----------------------------------------------------------------------
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
    public class JobDriver_AttendWorship : JobDriver
    {
        private readonly TargetIndex Build = TargetIndex.A;
        private readonly TargetIndex Facing = TargetIndex.B;
        private readonly TargetIndex Spot = TargetIndex.C;

        private Pawn setPreacher;

        protected Building_SacrificialAltar Altar => (Building_SacrificialAltar) job.GetTarget(ind: TargetIndex.A).Thing;

        protected Pawn PreacherPawn
        {
            get
            {
                if (setPreacher != null)
                {
                    return setPreacher;
                }

                if (Altar.preacher != null)
                {
                    setPreacher = Altar.preacher;
                    return Altar.preacher;
                }

                foreach (var preacherPawn in pawn.Map.mapPawns.FreeColonistsSpawned)
                {
                    if (preacherPawn.CurJob.def != CultsDefOf.Cults_HoldWorship)
                    {
                        continue;
                    }

                    setPreacher = preacherPawn;
                    return preacherPawn;
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
            Scribe_References.Look(refee: ref setPreacher, label: "setPreacher");
            base.ExposeData();
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            rotateToFace = Facing;

            AddEndCondition(newEndCondition: delegate
            {
                if (PreacherPawn.CurJob.def == CultsDefOf.Cults_ReflectOnWorship)
                {
                    return JobCondition.Succeeded;
                }

                if (PreacherPawn.CurJob.def != CultsDefOf.Cults_HoldWorship)
                {
                    return JobCondition.Incompletable;
                }

                return JobCondition.Ongoing;
            });
            this.EndOnDespawnedOrNull(ind: Spot);
            this.EndOnDespawnedOrNull(ind: Build);


            yield return Toils_Reserve.Reserve(ind: Spot);
            var gotoPreacher = TargetC.HasThing
                ? Toils_Goto.GotoThing(ind: Spot, peMode: PathEndMode.OnCell)
                : Toils_Goto.GotoCell(ind: Spot, peMode: PathEndMode.OnCell);

            yield return gotoPreacher;

            var altarToil = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = CultUtility.ritualDuration
            };
            altarToil.AddPreTickAction(newAct: () =>
            {
                pawn.GainComfortFromCellIfPossible();
                pawn.rotationTracker.FaceCell(c: TargetB.Cell);
                if (PreacherPawn.CurJob.def != CultsDefOf.Cults_HoldWorship)
                {
                    ReadyForNextToil();
                }
            });
            yield return altarToil;
            yield return Toils_Jump.JumpIf(jumpTarget: altarToil, condition: () => PreacherPawn.CurJob.def == CultsDefOf.Cults_HoldWorship);
            yield return Toils_Reserve.Release(ind: Spot);

            AddFinishAction(newAct: () =>
            {
                //When the ritual is finished -- then let's give the thoughts
                if (Altar.currentWorshipState == Building_SacrificialAltar.WorshipState.finishing ||
                    Altar.currentWorshipState == Building_SacrificialAltar.WorshipState.finished)
                {
                    CultUtility.AttendWorshipTickCheckEnd(preacher: PreacherPawn, pawn: pawn);
                    Utility.DebugReport(x: "Called end tick check");
                }

                pawn.ClearAllReservations();
                //if (this.TargetC.HasThing && TargetC.Thing is Thing t)
                //{
                //    if (pawn.Res Map.reservationManager.IsReserved(this.job.targetC.Thing, Faction.OfPlayer))
                //        Map.reservationManager.Release(this.job.targetC.Thing, pawn);
                //}
                //else
                //{
                //    if (Map.reservationManager.IsReserved(this.job.targetC.Cell, Faction.OfPlayer))
                //        Map.reservationManager.Release(this.job.targetC.Cell, this.pawn);
                //}
            });
        }
    }
}