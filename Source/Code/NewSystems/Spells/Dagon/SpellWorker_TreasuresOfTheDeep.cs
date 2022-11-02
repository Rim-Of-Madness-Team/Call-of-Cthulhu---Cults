// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using RimWorld;
using Verse;

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
    public class SpellWorker_TreasuresOfTheDeep : SpellWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            //Cthulhu.Utility.DebugReport("
            //: " + this.def.defName);
            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!(parms.target is Map map))
            {
                return false;
            }

            if (!CultUtility.TryFindDropCell(nearLoc: map.Center, map: map, maxDist: 999999, pos: out var intVec))
            {
                return false;
            }

            //this.EndOnDespawnedOrNull(this.pawn, JobCondition.Incompletable);
            for (var i = 0; i < Rand.Range(min: 1, max: 3); i++)
            {
                var thing = (Building_TreasureChest) ThingMaker.MakeThing(def: CultsDefOf.Cults_TreasureChest);
                GenPlace.TryPlaceThing(thing: thing, center: intVec.RandomAdjacentCell8Way(), map: map, mode: ThingPlaceMode.Near);
            }

            map.GetComponent<MapComponent_SacrificeTracker>().lastLocation = intVec;
            Messages.Message(text: "Treasures from the deep mysteriously appear.", lookTargets: new TargetInfo(cell: intVec, map: map),
                def: MessageTypeDefOf.PositiveEvent);
            return true;
        }
    }
}