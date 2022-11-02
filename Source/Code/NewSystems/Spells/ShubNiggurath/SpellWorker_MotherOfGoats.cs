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
            var intVec = DropCellFinder.RandomDropSpot(map: (Map) parms.target);

            //Spawn Black Ibex
            Utility.SpawnPawnsOfCountAt(kindDef: CultsDefOf.Cults_BlackGoat, at: intVec, map: map, count: Rand.Range(min: 6, max: 10));

            //Spawn some Black Ibex as player pets
            Utility.SpawnPawnsOfCountAt(kindDef: CultsDefOf.Cults_BlackGoat, at: intVec, map: map, count: Rand.Range(min: 1, max: 2), fac: Faction.OfPlayer);

            Messages.Message(text: "A herd of black ibex have appeared on the overworld map", def: MessageTypeDefOf.PositiveEvent);
            map.GetComponent<MapComponent_SacrificeTracker>().lastLocation = intVec;

            Utility.ApplyTaleDef(defName: "Cults_SpellMotherOfGoats", map: map);
            return true;
        }
    }
}