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
    public class SpellWorker_Reincarnation : SpellWorker
    {
        private static Map map;
        private static IntVec3 pos;
        private static Pawn exSacrifice;
        private static Corpse deadBody;

        protected Corpse corpse(Map map)
        {
                Corpse c = map.thingGrid.ThingAt<Corpse>(altar(map).Position);
                return c;
        }

        protected override bool CanFireNowSub(IncidentParms parms)
        {

            //Cthulhu.Utility.DebugReport("
            //: " + this.def.defName);
            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            map = parms.target as Map;
            pos = altar(map).Position;
            exSacrifice = corpse(map).InnerPawn;
            deadBody = corpse(map);

            LongEventHandler.QueueLongEvent(delegate
            {

                //Throw some smoke
                MoteMaker.ThrowDustPuff(SpellWorker_Reincarnation.pos, SpellWorker_Reincarnation.map, 2f);

                //Make the body strip and despawn around the altar
                SpellWorker_Reincarnation.deadBody.Strip();
                SpellWorker_Reincarnation.deadBody.DeSpawn();

                //Trigger the nightmare event on the altar
                altar(SpellWorker_Reincarnation.map).NightmareEvent();

                Cthulhu.Utility.ApplyTaleDef("Cults_SpellReincarnation", deadBody.InnerPawn);

            }, "Cults_SpellReincarnation", false, null);
            return true;
        }
    }
}
