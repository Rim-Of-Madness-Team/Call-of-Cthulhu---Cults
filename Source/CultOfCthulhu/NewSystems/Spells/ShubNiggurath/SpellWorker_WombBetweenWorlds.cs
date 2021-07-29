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
                Messages.Message("Note: Cosmic Horrors mod isn't loaded. Megaspiders will be summoned instead.",
                    MessageTypeDefOf.NeutralEvent);
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
            if (!CultUtility.TryFindDropCell(map.Center, map, 999999, out var intVec))
            {
                return false;
            }

            //Spawn 1 Womb Between Worlds
            var thing = (Building_WombBetweenWorlds) ThingMaker.MakeThing(CultsDefOf.Cults_WombBetweenWorlds);
            thing.SetFaction(Faction.OfPlayer);
            GenPlace.TryPlaceThing(thing, intVec.RandomAdjacentCell8Way(), map, ThingPlaceMode.Near);

            map.GetComponent<MapComponent_SacrificeTracker>().lastLocation = intVec;
            //Messages.Message(".", intVec, MessageTypeDefOf.PositiveEvent);
            return true;
        }
    }
}