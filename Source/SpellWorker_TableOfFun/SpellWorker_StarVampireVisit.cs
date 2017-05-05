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
    public class SpellWorker_StarVampireVisit : SpellWorker
    {

        public override bool CanSummonNow(Map map)
        {
            if (!Cthulhu.Utility.IsCosmicHorrorsLoaded())
            {
                Messages.Message("Note: Cosmic Horrors mod isn't loaded. Megaspiders will be summoned instead.", MessageSound.Standard);
            }

            //Cthulhu.Utility.DebugReport("CanFire: " + this.def.defName);
            return true;
        }

        public override bool TryExecute(IncidentParms parms)
        {
            Map map = parms.target as Map;
            //Spawn a Dark Young
            if (Cthulhu.Utility.IsCosmicHorrorsLoaded())
            {
                Cthulhu.Utility.SpawnPawnsOfCountAt(DefDatabase<PawnKindDef>.GetNamed("CosmicHorror_StarVampire"), altar(map).Position, map, 1, Find.World.factionManager.FirstFactionOfDef(FactionDefOf.SpacerHostile));
            }
            else
            {
                Cthulhu.Utility.SpawnPawnsOfCountAt(PawnKindDefOf.Megaspider, altar(map).Position, map, Rand.Range(1, 2), Find.FactionManager.FirstFactionOfDef(FactionDefOf.SpacerHostile), true);
            }
            Messages.Message("A star vampire is unleashed.", MessageSound.SeriousAlert);

            Cthulhu.Utility.ApplyTaleDef("SpellStarVampireVisit", map);

            return true;
        }
    }
}
