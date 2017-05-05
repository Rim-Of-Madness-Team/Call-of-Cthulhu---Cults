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
    public class SpellWorker_RelicsoftheDeep : SpellWorker
    {
        protected override bool CanFireNowSub()
        {

            //CthulhuUtility.DebugReport("CanFire: " + this.def.defName);
            return true;
        }
        public override bool CanSummonNow()
        {
            return true;
        }

        public override bool TryExecute(IncidentParms parms)
        {
            IntVec3 intVec;
            //Find a drop spot
            if (!ShipChunkDropCellFinder.TryFindShipChunkDropCell(out intVec, Find.Map.Center, 999999))
            {
                return false;
            }
            //Spawn 1 relic
                Building_TreasureChest thing = (Building_TreasureChest)ThingMaker.MakeThing(CultDefOfs.TreasureChest_Relic, null);
                GenPlace.TryPlaceThing(thing, intVec.RandomAdjacentCell8Way(), ThingPlaceMode.Near);
            //Spawn 1 treasure chest
                Building_TreasureChest thing2 = (Building_TreasureChest)ThingMaker.MakeThing(CultDefOfs.TreasureChest, null);
                GenPlace.TryPlaceThing(thing2, intVec.RandomAdjacentCell8Way(), ThingPlaceMode.Near);

            MapComponent_SacrificeTracker.Get.lastLocation = intVec;
            Messages.Message("Treasures from the deep mysteriously appear.", intVec, MessageSound.Benefit);
            return true;
        }
    }
}