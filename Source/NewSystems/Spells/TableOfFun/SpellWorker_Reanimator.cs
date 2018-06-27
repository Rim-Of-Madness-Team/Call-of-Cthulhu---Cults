﻿// ----------------------------------------------------------------------
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
    public class SpellWorker_Reanimator : SpellWorker
    {

        protected Pawn innerSacrifice(Map map)
        {
                Corpse c = map.thingGrid.ThingAt<Corpse>(altar(map).Position);
                return c.InnerPawn;
        }

        protected override bool CanFireNowSub(IncidentParms parms)
        {

            //Cthulhu.Utility.DebugReport("CanFire: " + this.def.defName);
            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = parms.target as Map;

            //Generate the zombie
            ReanimatedPawn pawn = ReanimatedPawnUtility.DoGenerateZombiePawnFromSource(innerSacrifice(map));
            IntVec3 intVec = innerSacrifice(map).Position.RandomAdjacentCell8Way();
            GenSpawn.Spawn(pawn, intVec, map);
            innerSacrifice(map).Corpse.Destroy(0);
            //Destroy the corpse
            //Replace the innerSacrifice with the new pawn just in-case
            //altar.innerSacrifice = thing;
            map.GetComponent<MapComponent_SacrificeTracker>().lastLocation = intVec;
            Messages.Message("The innerSacrifice reanimates and attacks.", MessageTypeDefOf.ThreatBig);
            Cthulhu.Utility.ApplyTaleDef("Cults_SpellReanimator", pawn);
            return true;
        }
    }
}
