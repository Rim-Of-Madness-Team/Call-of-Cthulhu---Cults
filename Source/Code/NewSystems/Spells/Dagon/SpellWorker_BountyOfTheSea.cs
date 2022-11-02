// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using Cthulhu;
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
    public class SpellWorker_BountyOfTheSea : SpellWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            //Cthulhu.Utility.DebugReport("CanFire: " + this.def.defName);
            return true;
        }

        public override bool CanSummonNow(Map map)
        {
            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!(parms.target is Map map))
            {
                return false;
            }

            //Find a drop spot
            if (!CultUtility.TryFindDropCell(nearLoc: map.Center, map: map, maxDist: 999999, pos: out var intVec))
            {
                return false;
            }

            //Spawn 1 relic
            var thing = (Building_LandedShip) ThingMaker.MakeThing(def: CultsDefOf.Cults_LandedShip);
            GenPlace.TryPlaceThing(thing: thing, center: intVec.RandomAdjacentCell8Way(), map: map, mode: ThingPlaceMode.Near);

            //Spawn 2 treasure chest
            var thing2 = (Building_TreasureChest) ThingMaker.MakeThing(def: CultsDefOf.Cults_TreasureChest);
            GenPlace.TryPlaceThing(thing: thing2, center: intVec.RandomAdjacentCell8Way(), map: map, mode: ThingPlaceMode.Near);
            var thing3 = (Building_TreasureChest) ThingMaker.MakeThing(def: CultsDefOf.Cults_TreasureChest);
            GenPlace.TryPlaceThing(thing: thing3, center: intVec.RandomAdjacentCell8Way(), map: map, mode: ThingPlaceMode.Near);

            map.GetComponent<MapComponent_SacrificeTracker>().lastLocation = intVec;
            Messages.Message(text: "Treasures from the deep mysteriously appear.", lookTargets: new TargetInfo(cell: intVec, map: map),
                def: MessageTypeDefOf.PositiveEvent);
            Utility.ApplyTaleDef(defName: "Cults_SpellBountyOfTheSea", map: map);
            return true;
        }
    }
}