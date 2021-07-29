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
    public class SpellWorker_SunkenShip : SpellWorker
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

            if (!CultUtility.TryFindDropCell(map.Center, map, 999999, out var intVec))
            {
                return false;
            }

            GenSpawn.Spawn(CultsDefOf.Cults_SunkenShipChunk, intVec, map);

            map.GetComponent<MapComponent_SacrificeTracker>().lastLocation = intVec;
            Messages.Message("MessageSunkenShipChunkDrop".Translate(), new TargetInfo(intVec, map),
                MessageTypeDefOf.NeutralEvent);

            Utility.ApplyTaleDef("Cults_SpellSunkenShip", map);

            return true;
        }
    }
}