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
    public class SpellWorker_ElixerOfPower : SpellWorker
    {

        public override bool CanSummonNow(Map map)
        {
            return true;
        }

        protected override bool CanFireNowSub(IIncidentTarget target)
        {

            //Cthulhu.Utility.DebugReport("CanFire: " + this.def.defName);
            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = parms.target as Map;
            //Spawn some goats
            //Cthulhu.Utility.SpawnPawnsOfCountAt(CultDefOfs.BlackIbex, altar.Position, Rand.Range(2, 5), Faction.OfPlayer);

            //Spawn a fertility idol.
            Cthulhu.Utility.SpawnThingDefOfCountAt(CultsDefOf.Cults_FertilityTotem, 1, new TargetInfo(altar(map).RandomAdjacentCell8Way(), map));

            //Spawn a 
            Messages.Message("An idol of fertility rises from the corpse.", MessageTypeDefOf.PositiveEvent);

            Cthulhu.Utility.ApplyTaleDef("Cults_SpellFertilityRitual", map);
            return true;
        }
    }
}
