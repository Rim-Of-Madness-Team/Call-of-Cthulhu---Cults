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
    public class JobDriver_LayDownAltar : JobDriver
    {
        
        //Target A -- This altar
        public Building_SacrificialAltar Altar
        {
            get
            {
                return (Building_SacrificialAltar)base.CurJob.GetTarget(TargetIndex.A).Thing;
            }
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            bool hasThing = this.pawn.CurJob.GetTarget(TargetIndex.A).HasThing;
            if (hasThing)
            {
                yield return Toils_Reserve.Reserve(TargetIndex.A, this.Altar.LyingSlotsCount);
                //yield return Toils_Altar.ClaimAltarIfNonMedical(TargetIndex.A, TargetIndex.None);
                Toil GoToAltar = new Toil();
                GoToAltar.initAction = delegate
                {
                    Pawn actor = this.pawn;
                    IntVec3 AltarLyingSlotPosFor = Altar.GetLyingSlotPos();
                    if (actor.Position == AltarLyingSlotPosFor)
                    {
                        actor.jobs.curDriver.ReadyForNextToil();
                    }
                    else
                    {
                        actor.pather.StartPath(AltarLyingSlotPosFor, PathEndMode.OnCell);
                    }
                };
                yield return GoToAltar;
            }
            else
            {
                yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            }
            yield return Toils_LayDownAltar.LayDown(TargetIndex.A);
            yield break;
        }

        public override string GetReport()
        {
            if (this.asleep)
            {
                return "ReportLying".Translate();
            }
            return "ReportResting".Translate();
        }
    }
}
