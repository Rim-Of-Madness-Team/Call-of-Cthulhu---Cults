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
    public class JobDriver_WriteTheBook : JobDriver
    {

        private TargetIndex Executioner = TargetIndex.A;
        private bool atTypeWriter = false;

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override bool TryMakePreToilReservations()
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.EndOnDespawnedOrNull(Executioner, JobCondition.Incompletable);
            if (CultUtility.AreOccultGrimoiresAvailable(pawn.Map))
            {
                pawn.Map.GetComponent<MapComponent_LocalCultTracker>().CurrentSeedState = CultSeedState.NeedTable;
            }
            else
            {
                IntVec3 destination = IntVec3.Invalid;

                //First, let's try and find a typewriter.
                //If we find one, let's go to it and start typing.
                Thing Typewriter = null;
                if (Cthulhu.Utility.IsIndustrialAgeLoaded())
                {
                    Cthulhu.Utility.DebugReport("Industrial age check");
                    if (this.pawn.Map != null)
                    {
                        if (this.pawn.Map.listerBuildings != null)
                        {
                            foreach (Building thing in this.pawn.Map.listerBuildings.allBuildingsColonist)
                            {
                                if (thing.def.defName == "Estate_TableTypewriter")
                                {
                                    Typewriter = thing;
                                    Cthulhu.Utility.DebugReport("Found typewriter");
                                    Toil gotoDestination = Toils_Goto.GotoCell(Typewriter.InteractionCell, PathEndMode.OnCell);
                                    atTypeWriter = true;
                                    yield return gotoDestination;
                                    goto SkipRoom;
                                }
                            }
                        }
                    }
                }

                //If we don't have a typewriter, then let's go to our personal room or near our bed.
                Room destinationRoom = this.pawn.ownership.OwnedRoom;
                Building destinationBed = this.pawn.ownership.OwnedBed;
                if (destinationRoom != null)
                {
                    if (destinationRoom.Cells.TryRandomElement<IntVec3>(out destination))
                    {
                        IntVec3 cellInsideRoom = IntVec3.Invalid;
                        if (Cthulhu.Utility.IsRandomWalkable8WayAdjacentOf(destination, Map, out cellInsideRoom))
                        {
                            Toil gotoRoom;
                            gotoRoom = Toils_Goto.GotoCell(cellInsideRoom, PathEndMode.OnCell);
                            yield return gotoRoom;
                            goto SkipRoom;
                        }
                    }
                }
                else if (destinationBed != null)
                {
                    IntVec3 cellNearBed = IntVec3.Invalid;
                    if (Cthulhu.Utility.IsRandomWalkable8WayAdjacentOf(destinationBed.Position, Map, out cellNearBed))
                    {
                        Toil gotoBedArea;
                        gotoBedArea = Toils_Goto.GotoCell(cellNearBed, PathEndMode.OnCell);
                    }
                }

                SkipRoom:

                Toil altarToil = new Toil();
                altarToil.defaultCompleteMode = ToilCompleteMode.Delay;
                if (atTypeWriter) altarToil.PlaySustainerOrSound(SoundDef.Named("Estate_SoundManualTypewriter"));
                else altarToil.PlaySustainerOrSound(SoundDef.Named("PencilWriting"));
                altarToil.WithProgressBarToilDelay(TargetIndex.A);
                altarToil.defaultDuration = this.job.def.joyDuration;
                altarToil.AddPreTickAction(() =>
                {
                    if (Typewriter != null)
                    {
                        this.pawn.rotationTracker.FaceCell(Typewriter.Position);
                        this.pawn.GainComfortFromCellIfPossible();
                    }
                });
                altarToil.AddPreInitAction(() =>
                {
                    Messages.Message(this.pawn.LabelCap + "WritingStrangeSymbols".Translate(), MessageTypeDefOf.NeutralEvent);
                });
                yield return altarToil;

                Toil finishedAction = new Toil();
                finishedAction.defaultCompleteMode = ToilCompleteMode.Instant;
                finishedAction.initAction = delegate
                {
                    Map.GetComponent<MapComponent_LocalCultTracker>().CurrentSeedState = CultSeedState.FinishedWriting;
                };
                yield return finishedAction;

                this.AddFinishAction(() =>
                {
                    if (Map.GetComponent<MapComponent_LocalCultTracker>().CurrentSeedState == CultSeedState.FinishedWriting)
                        CultUtility.FinishedTheBook(this.pawn);
                });
            }
        }
    }
}
