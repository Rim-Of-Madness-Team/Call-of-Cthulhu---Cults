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
    public partial class MapComponent_SacrificeTracker : MapComponent
    {

        /// <summary>
        /// This keeps track of dead colonists who have taken the Unspeakable Oath.
        /// They will need to be resurrected. This begins the process.
        /// </summary>
        public void ResolveHasturOathtakers()
        {
            try
            {

                if (Find.TickManager.TicksGame % 100 != 0) return;
                if (unspeakableOathPawns.NullOrEmpty()) return;
                List<Pawn> tempOathList = new List<Pawn>(unspeakableOathPawns);
                foreach (Pawn oathtaker in tempOathList)
                {
                    if (oathtaker.Dead)
                    {
                        if (unspeakableOathPawns != null) unspeakableOathPawns.Remove(oathtaker);
                        if ((toBeResurrected?.Count ?? 0) > 0) toBeResurrected = new List<Pawn>();
                        toBeResurrected.Add(oathtaker);
                        Cthulhu.Utility.DebugReport("Started Resurrection Process");
                        ticksUntilResurrection = resurrectionTicks;
                    }
                }
            }
            catch (NullReferenceException)
            { }
        }

        /// <summary>
        /// When Oathtakers die, they need to be resurrected after a period of time.
        /// 
        /// </summary>
        public void ResolveHasturResurrections()
        {
            //Check ticks

            if (ticksUntilResurrection == -999) return;

            if (ticksUntilResurrection > 0)
            {
                ticksUntilResurrection--;
                return;
            }
            ticksUntilResurrection = -999;


            //Ticks passed. Commence resurrection!
            HasturResurrection();

            //Do we still have colonists that need resurrection? If so, let's proceed with another round of resurrection.
            //Reset the timer, and let's get to work.
            if ((toBeResurrected?.Count ?? 0) > 0)
            {
                ticksUntilResurrection = resurrectionTicks;
            }
        }

        public void HasturResurrection()
        {
            Pawn sourceCorpse = toBeResurrected.RandomElement();
            IntVec3 spawnLoc = IntVec3.Invalid;
            Map map = null;

            if (sourceCorpse.Corpse != null)
            {
                map = sourceCorpse.MapHeld;
                spawnLoc = sourceCorpse.PositionHeld;
                if (spawnLoc == IntVec3.Invalid) spawnLoc = sourceCorpse.Position;
                if (spawnLoc == IntVec3.Invalid) spawnLoc = DropCellFinder.RandomDropSpot(map);

                ReanimatedPawn newPawn = ReanimatedPawnUtility.DoGenerateZombiePawnFromSource(sourceCorpse, false, true);

                //Hops / Other storage buildings
                if (StoreUtility.StoringBuilding(sourceCorpse.Corpse) is Building building)
                {
                    if (building is Building_Storage buildingS)
                    {
                        buildingS.Notify_LostThing(sourceCorpse);
                    }

                }
                if (sourceCorpse?.Corpse?.holdingOwner?.Owner is Building_Casket casket)
                {
                    casket.EjectContents();
                    Cthulhu.Utility.DebugReport("Resurection:: Casket/grave/sarcophogi opened.");
                }

                Messages.Message("ReanimatedOath".Translate(new object[] {
                    sourceCorpse.Name
                }), MessageTypeDefOf.ThreatBig);
                //Log.Message(newPawn.NameStringShort);
                //Log.Message(spawnLoc.ToString());
                //Log.Message(map.ToString());

                GenSpawn.Spawn(newPawn, spawnLoc, map);
                sourceCorpse.Destroy(0);
            }
        }

    }
}
