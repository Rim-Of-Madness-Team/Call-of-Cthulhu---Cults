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
    public class SpellWorker_DarkEmissary : SpellWorker
    {
        private const float RelationWithColonistWeight = 20f;

        protected override bool CanFireNowSub(IIncidentTarget target)
        {
            //Cthulhu.Utility.DebugReport("CanFire: " + this.def.defName);
            return true;
        }

        public override bool TryExecute(IncidentParms parms)
        {
            Map map = parms.target as Map;
            for (int i = 0; i < 2; i++)
            {
                if (!CultUtility.TrySpawnWalkInCultist(map, CultUtility.CultistType.DarkEmmisary, false))
                {
                    //Log.Messag("Failed to spawn walk in cultist");
                    return false;
                }
            }
            return true;
        }
    }
}
