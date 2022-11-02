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
    public class SpellWorker_WombBetweenWorlds : SpellWorker
    {
        public override bool CanSummonNow(Map map)
        {
            if (!Utility.IsCosmicHorrorsLoaded())
            {
                Messages.Message(text: "Note: Cosmic Horrors mod isn't loaded. Megaspiders will be summoned instead.",
                    def: MessageTypeDefOf.NeutralEvent);
            }

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

            //Spawn 1 Womb Between Worlds
            var thing = (Building_WombBetweenWorlds) ThingMaker.MakeThing(def: CultsDefOf.Cults_WombBetweenWorlds);
            thing.SetFaction(newFaction: Faction.OfPlayer);
            GenPlace.TryPlaceThing(thing: thing, center: intVec.RandomAdjacentCell8Way(), map: map, mode: ThingPlaceMode.Near);

            map.GetComponent<MapComponent_SacrificeTracker>().lastLocation = intVec;
            //Messages.Message(".", intVec, MessageTypeDefOf.PositiveEvent);
            return true;
        }
    }
}