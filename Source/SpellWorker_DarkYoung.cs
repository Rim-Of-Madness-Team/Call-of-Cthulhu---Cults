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
    public class SpellWorker_DarkYoung : SpellWorker  
    {
        protected Building_SacrificialAltar altar
        {
            get
            {
                return MapComponent_SacrificeTracker.Get.lastUsedAltar;
            }
        }

        protected Pawn pawn
        {
            get
            {
                return MapComponent_SacrificeTracker.Get.lastUsedAltar.executioner;
            }
        }

        public override bool CanSummonNow()
        {
            if (!CthulhuUtility.IsCosmicHorrorsLoaded())
            {
                Messages.Message("Note: Cosmic Horrors mod isn't loaded. Megaspiders will be summoned instead.", MessageSound.Standard);
            }
            return true;
        }

        public override bool TryExecute(IncidentParms parms)
        {
            ////Spawn a Dark Young
            //if (CthulhuUtility.IsCosmicHorrorsLoaded())
            //{ 
            //    CthulhuUtility.SpawnPawnsOfCountAt(DefDatabase<PawnKindDef>.GetNamed("DarkYoung"), altar.Position, 1, Faction.OfPlayer);
            //}
            //else
            //{
            //    CthulhuUtility.SpawnPawnsOfCountAt(PawnKindDefOf.Megaspider, altar.Position, Rand.Range(1,2), Faction.OfPlayer);
            //}
            //Messages.Message("The Dark Young of Shub Niggurath coils out from under the sacrifice.", MessageSound.Benefit);

            //return true;

            IntVec3 intVec;
            //Find a drop spot
            if (!ShipChunkDropCellFinder.TryFindShipChunkDropCell(out intVec, Find.Map.Center, 999999))
            {
                return false;
            }
            //Spawn 1 Womb Between Worlds
            Building_WombBetweenWorlds thing = (Building_WombBetweenWorlds)ThingMaker.MakeThing(CultDefOfs.WombBetweenWorlds, null);
            thing.SetFaction(Faction.OfPlayer);
            GenPlace.TryPlaceThing(thing, intVec.RandomAdjacentCell8Way(), ThingPlaceMode.Near);

            MapComponent_SacrificeTracker.Get.lastLocation = intVec;
            Messages.Message(".", intVec, MessageSound.Benefit);
            return true;
        }
    }
}
