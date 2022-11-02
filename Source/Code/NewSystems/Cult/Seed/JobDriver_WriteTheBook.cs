// ----------------------------------------------------------------------
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
    public class JobDriver_WriteTheBook : JobDriver
    {
        private readonly TargetIndex Executioner = TargetIndex.A;
        private bool atTypeWriter;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.EndOnDespawnedOrNull(ind: Executioner);
            if (CultUtility.AreOccultGrimoiresAvailable(map: pawn.Map))
            {
                pawn.Map.GetComponent<MapComponent_LocalCultTracker>().CurrentSeedState = CultSeedState.NeedTable;
            }
            else
            {
                //First, let's try and find a typewriter.
                //If we find one, let's go to it and start typing.
                Thing Typewriter = null;
                if (Utility.IsIndustrialAgeLoaded())
                {
                    Utility.DebugReport(x: "Industrial age check");
                    if (pawn.Map?.listerBuildings != null)
                    {
                        foreach (var thing in pawn.Map.listerBuildings.allBuildingsColonist)
                        {
                            if (thing.def.defName != "Estate_TableTypewriter")
                            {
                                continue;
                            }

                            Typewriter = thing;
                            Utility.DebugReport(x: "Found typewriter");
                            var gotoDestination =
                                Toils_Goto.GotoCell(cell: Typewriter.InteractionCell, peMode: PathEndMode.OnCell);
                            atTypeWriter = true;
                            yield return gotoDestination;
                            goto SkipRoom;
                        }
                    }
                }

                //If we don't have a typewriter, then let's go to our personal room or near our bed.
                var destinationRoom = pawn.ownership.OwnedRoom;
                Building destinationBed = pawn.ownership.OwnedBed;
                if (destinationRoom != null)
                {
                    if (destinationRoom.Cells.TryRandomElement(result: out var destination))
                    {
                        if (Utility.IsRandomWalkable8WayAdjacentOf(cell: destination, map: Map, resultCell: out var cellInsideRoom))
                        {
                            var gotoRoom = Toils_Goto.GotoCell(cell: cellInsideRoom, peMode: PathEndMode.OnCell);
                            yield return gotoRoom;
                        }
                    }
                }
                else if (destinationBed != null)
                {
                    if (Utility.IsRandomWalkable8WayAdjacentOf(cell: destinationBed.Position, map: Map, resultCell: out var cellNearBed))
                    {
                        var gotoBedArea = Toils_Goto.GotoCell(cell: cellNearBed, peMode: PathEndMode.OnCell);
                        yield return gotoBedArea;
                    }
                }

                SkipRoom:

                var altarToil = new Toil {defaultCompleteMode = ToilCompleteMode.Delay};
                altarToil.PlaySustainerOrSound(soundDef: atTypeWriter
                    ? SoundDef.Named(defName: "Estate_SoundManualTypewriter")
                    : SoundDef.Named(defName: "PencilWriting"));

                altarToil.WithProgressBarToilDelay(ind: TargetIndex.A);
                altarToil.defaultDuration = job.def.joyDuration;
                altarToil.AddPreTickAction(newAct: () =>
                {
                    if (Typewriter == null)
                    {
                        return;
                    }

                    pawn.rotationTracker.FaceCell(c: Typewriter.Position);
                    pawn.GainComfortFromCellIfPossible();
                });
                altarToil.AddPreInitAction(newAct: () => Messages.Message(text: pawn.LabelCap + "WritingStrangeSymbols".Translate(),
                    def: MessageTypeDefOf.NeutralEvent));
                yield return altarToil;

                var finishedAction = new Toil
                {
                    defaultCompleteMode = ToilCompleteMode.Instant,
                    initAction = delegate
                    {
                        Map.GetComponent<MapComponent_LocalCultTracker>().CurrentSeedState =
                            CultSeedState.FinishedWriting;
                    }
                };
                yield return finishedAction;

                AddFinishAction(newAct: () =>
                {
                    if (Map.GetComponent<MapComponent_LocalCultTracker>().CurrentSeedState ==
                        CultSeedState.FinishedWriting)
                    {
                        CultUtility.FinishedTheBook(pawn: pawn);
                    }
                });
            }
        }
    }
}