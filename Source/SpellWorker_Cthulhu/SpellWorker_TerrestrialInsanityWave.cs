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
    public class SpellWorker_TerrestrialInsanityWave : SpellWorker
    {
        protected override bool CanFireNowSub(IIncidentTarget target)
        {
            
            //Cthulhu.Utility.DebugReport("
            //: " + this.def.defName);
            return true;
        }
        public override bool CanSummonNow(Map map)
        {
            return true;
        }

        public override bool TryExecute(IncidentParms parms)
        {
            Map map = parms.target as Map;
            Pawn pawn = null;
            List<Pawn> listeners = map.mapPawns.AllPawnsSpawned.FindAll(x => x.RaceProps.intelligence == Intelligence.Humanlike);
            bool[] flag = new bool[listeners.Count];
            for (int i = 0; i < listeners.Count; i++)
            {
                    pawn = listeners[i];
                    if (pawn.Faction == Faction.OfPlayer || (!pawn.Faction.HostileTo(Faction.OfPlayer)) || pawn.guest.IsPrisoner)
                    {
                        Cthulhu.Utility.ApplySanityLoss(pawn, Rand.Range(0.2f, 0.8f));
                    }
                    else
                    {
                        MentalStateDef defaultState = MentalStateDefOf.Berserk;
                        int tempRand = Rand.Range(1, 10);
                        switch (tempRand)
                        {
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                            case 6:
                                break;
                            case 7:
                            case 8:
                            case 9:
                                defaultState = MentalStateDefOf.PanicFlee;
                                break;
                            case 10:
                                defaultState = CultsDefOf.FireStartingSpree;
                                break;
                        }
                        Cthulhu.Utility.ApplySanityLoss(pawn, 1.0f);
                        pawn.mindState.mentalStateHandler.TryStartMentalState(defaultState, null, false);
                    }
            }

            return true;
        }
    }
}
