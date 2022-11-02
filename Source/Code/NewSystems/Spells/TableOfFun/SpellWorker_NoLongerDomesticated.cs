// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
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
    public class SpellWorker_NoLongerDomesticated : SpellWorker
    {
        protected IEnumerable<Pawn> Animals(Map map)
        {
            return from Pawn animal in map.mapPawns.AllPawns
                where animal.RaceProps.Animal && animal.Faction == Faction.OfPlayer
                select animal;
        }

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            //Cthulhu.Utility.DebugReport("CanFire: " + this.def.defName);
            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            for (var i = 0; i < Rand.Range(min: 3, max: 6); i++)
            {
                if (Animals(map: (Map) parms.target).Count() != 0)
                {
                    if (Animals(map: (Map) parms.target).TryRandomElement(result: out var animal))
                    {
                        //Cthulhu.Utility.DebugReport("Destroyed: " + item.ToString());
                        animal.mindState.mentalStateHandler.TryStartMentalState(stateDef: MentalStateDefOf.Manhunter);
                    }
                }
                else
                {
                    Utility.DebugReport(x: "No animals to drive insane.");
                }
            }

            return true;
        }
    }
}