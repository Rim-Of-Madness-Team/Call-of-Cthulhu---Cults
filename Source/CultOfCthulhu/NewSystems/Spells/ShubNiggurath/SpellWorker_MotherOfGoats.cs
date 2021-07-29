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
    public class SpellWorker_MotherOfGoats : SpellWorker
    {
        public override bool CanSummonNow(Map map)
        {
            return true;
        }

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            //Cthulhu.Utility.DebugReport("CanFire: " + this.def.defName);
            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!(parms.target is Map map))
            {
                return false;
            }

            //Get a random cell.
            var intVec = DropCellFinder.RandomDropSpot((Map) parms.target);

            //Spawn Black Ibex
            Utility.SpawnPawnsOfCountAt(CultsDefOf.Cults_BlackGoat, intVec, map, Rand.Range(6, 10));

            //Spawn some Black Ibex as player pets
            Utility.SpawnPawnsOfCountAt(CultsDefOf.Cults_BlackGoat, intVec, map, Rand.Range(1, 2), Faction.OfPlayer);

            Messages.Message("A herd of black ibex have appeared on the overworld map", MessageTypeDefOf.PositiveEvent);
            map.GetComponent<MapComponent_SacrificeTracker>().lastLocation = intVec;

            Utility.ApplyTaleDef("Cults_SpellMotherOfGoats", map);
            return true;
        }
    }
}