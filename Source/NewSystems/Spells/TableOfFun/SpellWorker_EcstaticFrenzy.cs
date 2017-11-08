// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

// ----------------------------------------------------------------------
// These are RimWorld-specific usings. Activate/Deactivate what you need:
// ----------------------------------------------------------------------
using UnityEngine;         // Always needed
//using VerseBase;         // Material/Graphics handling functions are found here
using Verse;               // RimWorld universal objects are here (like 'Building')
using Verse.AI;          // Needed when you do something with the AI
using Verse.AI.Group;
using Verse.Sound;       // Needed when you do something with Sound
using Verse.Noise;       // Needed when you do something with Noises
using RimWorld;            // RimWorld specific functions are found here (like 'Building_Battery')
using RimWorld.Planet;   // RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

namespace CultOfCthulhu
{
    public class SpellWorker_EcstaticFrenzy : SpellWorker
    {

        protected override bool CanFireNowSub(IIncidentTarget target)
        {

            //Cthulhu.Utility.DebugReport("CanFire: " + this.def.defName);
            return true;
        }
        
        protected IEnumerable<Pawn> Colonists(Map map)
        {
            return from Pawn colonist in map.mapPawns.FreeColonists
                   where !colonist.RaceProps.Animal && !(colonist.Downed || colonist.Dead) && colonist.Faction == Faction.OfPlayer
                   select colonist;
        } 

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            for (int i = 0; i < Rand.Range(1, 2); i++)
            {
                if (Colonists((Map)parms.target).Count<Pawn>() != 0)
                {
                    Pawn colonist;
                    if (Colonists((Map)parms.target).TryRandomElement<Pawn>(out colonist))
                    {
                        if (colonist != null)
                        {
                            //Cthulhu.Utility.DebugReport("Destroyed: " + item.ToString());
                            colonist.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, null, false);
                        }
                    }
                }
                else
                {
                    Cthulhu.Utility.DebugReport("No colonists to drive insane.");
                }
            }
            
            return true;
        }

    }
}