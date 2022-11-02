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
    public class SpellWorker_EcstaticFrenzy : SpellWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            //Cthulhu.Utility.DebugReport("CanFire: " + this.def.defName);
            return true;
        }

        protected IEnumerable<Pawn> Colonists(Map map)
        {
            return from Pawn colonist in map.mapPawns.FreeColonists
                where !colonist.RaceProps.Animal && !(colonist.Downed || colonist.Dead) &&
                      colonist.Faction == Faction.OfPlayer
                select colonist;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            for (var i = 0; i < Rand.Range(min: 1, max: 2); i++)
            {
                if (Colonists(map: (Map) parms.target).Count() != 0)
                {
                    if (!Colonists(map: (Map) parms.target).TryRandomElement(result: out var colonist))
                    {
                        continue;
                    }

                    //Cthulhu.Utility.DebugReport("Destroyed: " + item.ToString());
                    colonist?.mindState.mentalStateHandler.TryStartMentalState(stateDef: MentalStateDefOf.Berserk);
                }
                else
                {
                    Utility.DebugReport(x: "No colonists to drive insane.");
                }
            }

            return true;
        }
    }
}