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
    public class SpellWorker_TreasuresOfTheDeep : SpellWorker
    {

        protected override bool CanFireNowSub(IIncidentTarget target)
        {

            //Cthulhu.Utility.DebugReport("
            //: " + this.def.defName);
            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = parms.target as Map;
            IntVec3 intVec;
            if (!CultUtility.TryFindDropCell(map.Center, map, 999999, out intVec))
            {
                return false;
            }
            //this.EndOnDespawnedOrNull(this.pawn, JobCondition.Incompletable);
            for (int i = 0; i < Rand.Range(1,3); i++)
            {
                Building_TreasureChest thing = (Building_TreasureChest)ThingMaker.MakeThing(CultsDefOf.Cults_TreasureChest, null);
                GenPlace.TryPlaceThing(thing, intVec.RandomAdjacentCell8Way(), map, ThingPlaceMode.Near);
            }

            map.GetComponent<MapComponent_SacrificeTracker>().lastLocation = intVec;
            Messages.Message("Treasures from the deep mysteriously appear.", new TargetInfo(intVec, map), MessageTypeDefOf.PositiveEvent);
            return true;
        }

    }
}
