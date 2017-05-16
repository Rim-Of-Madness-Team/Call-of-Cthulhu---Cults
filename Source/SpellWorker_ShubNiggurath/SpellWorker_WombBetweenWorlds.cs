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
    public class SpellWorker_WombBetweenWorlds : SpellWorker
    {

        public override bool CanSummonNow(Map map)
        {
            if (!Cthulhu.Utility.IsCosmicHorrorsLoaded())
            {
                Messages.Message("Note: Cosmic Horrors mod isn't loaded. Megaspiders will be summoned instead.", MessageSound.Standard);
            }
            return true;
        }

        public override bool TryExecute(IncidentParms parms)
        {
            Map map = parms.target as Map;
            IntVec3 intVec;
            //Find a drop spot
            if (!ShipChunkDropCellFinder.TryFindShipChunkDropCell(map.Center, map, 999999, out intVec))
            {
                return false;
            }
            //Spawn 1 Womb Between Worlds
            Building_WombBetweenWorlds thing = (Building_WombBetweenWorlds)ThingMaker.MakeThing(CultsDefOf.Cults_WombBetweenWorlds, null);
            thing.SetFaction(Faction.OfPlayer);
            GenPlace.TryPlaceThing(thing, intVec.RandomAdjacentCell8Way(), map, ThingPlaceMode.Near);

            map.GetComponent<MapComponent_SacrificeTracker>().lastLocation = intVec;
            //Messages.Message(".", intVec, MessageSound.Benefit);
            return true;
        }
    }
}
