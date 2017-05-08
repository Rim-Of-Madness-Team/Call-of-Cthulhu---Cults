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
    public class JobDriver_AttendWorship : JobDriver
    {
        private TargetIndex Build = TargetIndex.A;
        private TargetIndex Facing = TargetIndex.B;
        private TargetIndex Spot = TargetIndex.C;

        private Pawn setPreacher = null;

        protected Building_SacrificialAltar Altar
        {
            get
            {
                return (Building_SacrificialAltar)base.CurJob.GetTarget(TargetIndex.A).Thing;
            }
        }

        protected Pawn PreacherPawn
        {
            get
            {
                if (setPreacher != null) return setPreacher;
                if (Altar.preacher != null) { setPreacher = Altar.preacher; return Altar.preacher; }
                else
                {
                    foreach (Pawn pawn in this.pawn.Map.mapPawns.FreeColonistsSpawned)
                    {
                        if (pawn.CurJob.def.defName == "HoldWorship") { setPreacher = pawn; return pawn; }
                    }
                }
                return null;
            }
        }

        public override void ExposeData()
        {
            Scribe_References.Look<Pawn>(ref this.setPreacher, "setPreacher");
            base.ExposeData();
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            rotateToFace = Facing;

            this.AddEndCondition(delegate
            {
                if (PreacherPawn.CurJob.def.defName == "ReflectOnWorship")
                {
                    return JobCondition.Succeeded;
                }
                else if (PreacherPawn.CurJob.def.defName != "HoldWorship")
                {
                    return JobCondition.Incompletable;
                }
                return JobCondition.Ongoing;
            });
            this.EndOnDespawnedOrNull(Spot, JobCondition.Incompletable);
            this.EndOnDespawnedOrNull(Build, JobCondition.Incompletable);


            yield return Toils_Reserve.Reserve(Spot, this.CurJob.def.joyMaxParticipants);
            Toil gotoPreacher;
            if (this.TargetC.HasThing)
            {
                gotoPreacher = Toils_Goto.GotoThing(Spot, PathEndMode.OnCell);
            }
            else
            {
                gotoPreacher = Toils_Goto.GotoCell(Spot, PathEndMode.OnCell);
            }
            yield return gotoPreacher;

            Toil altarToil = new Toil();
            altarToil.defaultCompleteMode = ToilCompleteMode.Delay;
            altarToil.defaultDuration = 9999;
            altarToil.AddPreTickAction(() =>
            {
                this.pawn.GainComfortFromCellIfPossible();
                this.ticksLeftThisToil = 9999;
                this.pawn.Drawer.rotator.FaceCell(TargetB.Cell);
                if (PreacherPawn.CurJob.def.defName != "HoldWorship")
                {
                    this.ticksLeftThisToil = -1;
                }
            });
            yield return altarToil;
            //yield return Toils_Jump.JumpIf(altarToil, () => PreacherPawn.CurJob.def.defName == "HoldWorship");
            yield return Toils_Reserve.Release(Spot);

            this.AddFinishAction(() =>
            {
                //When the ritual is finished -- then let's give the thoughts
                if (Altar.currentWorshipState == Building_SacrificialAltar.WorshipState.finishing ||
                    Altar.currentWorshipState == Building_SacrificialAltar.WorshipState.finished)
                {
                    CultUtility.AttendWorshipTickCheckEnd(PreacherPawn, this.pawn);
                    Cthulhu.Utility.DebugReport("Called end tick check");
                }
                if (this.TargetC.HasThing)
                {
                    if (Map.reservationManager.IsReserved(this.CurJob.targetC.Thing, Faction.OfPlayer))
                        Map.reservationManager.Release(this.CurJob.targetC.Thing, pawn);
                }
                else
                {
                    if (Map.reservationManager.IsReserved(this.CurJob.targetC.Cell, Faction.OfPlayer))
                        Map.reservationManager.Release(this.CurJob.targetC.Cell, this.pawn);
                }


            });
            yield break;
        }
    }
}
