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
    public class SpellWorker_RatsInTheWalls : SpellWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {

            //Cthulhu.Utility.DebugReport("
            //: " + this.def.defName);
            return true;
        }

        private List<IntVec3> AvailableFloors = new List<IntVec3>();

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = parms.target as Map;
            int num = 0;
            int countToSpawn = 10;
            for (int i = 0; i < countToSpawn; i++)
            {
                //Find floors in the home area
                IntVec3 intVec = IntVec3.Invalid; 
                intVec = CellFinderLoose.RandomCellWith((IntVec3 c) => c.InBounds(map) &&
                                                                       c.Standable(map) &&
                                                                       !c.InNoBuildEdgeArea(map) &&
                                                                       map.terrainGrid.TerrainAt(c).layerable && 
                                                                       map.areaManager.Home.ActiveCells.Contains<IntVec3>(c) &&
                                                                       !c.Fogged(map), map, 1000);
                if (intVec == IntVec3.Invalid || intVec == null)
                {
                    //Find smoothed floors in the home area
                    intVec = CellFinderLoose.RandomCellWith((IntVec3 c) => c.InBounds(map) &&
                                                                           c.Standable(map) &&
                                                                           !c.InNoBuildEdgeArea(map) &&
                                                                           map.terrainGrid.TerrainAt(c).defName.Contains("_Smooth") &&
                                                                           map.areaManager.Home.ActiveCells.Contains<IntVec3>(c) &&
                                                                           !c.Fogged(map), map, 1000);
                    if (intVec == IntVec3.Invalid || intVec == null)
                    {
                        //Find floors... anywhere
                        intVec = CellFinderLoose.RandomCellWith((IntVec3 c) => c.InBounds(map) &&
                                                                               c.Standable(map) &&
                                                                               !c.InNoBuildEdgeArea(map) &&
                                                                               map.terrainGrid.TerrainAt(c).layerable &&
                                                                               !c.Fogged(map), map, 1000);
                        if (intVec == IntVec3.Invalid || intVec == null)
                        {
                            //Find the ground near the players then.
                            if (intVec == IntVec3.Invalid || intVec == null)
                            {
                                intVec = CellFinderLoose.RandomCellWith((IntVec3 c) => c.InBounds(map) &&
                                                                                       c.Standable(map) &&
                                                                                       !c.InNoBuildEdgeArea(map) &&
                                                                                       map.areaManager.Home.ActiveCells.Contains<IntVec3>(c) &&
                                                                                       !c.Fogged(map), map, 1000);
                                if (intVec == IntVec3.Invalid || intVec == null)
                                {
                                    Cthulhu.Utility.DebugReport("Error: Can't assign cell for Rats in the Walls spell.");
                                    continue;
                                }
                            }
                        }
                    }
                }
                //Throw some smoke
                MoteMaker.ThrowDustPuff(intVec, map, 1f);

                //Break the floor
                if (intVec.InBounds(map) && map.terrainGrid.TerrainAt(intVec).layerable)
                    map.terrainGrid.RemoveTopLayer(intVec, false);

                //Spawn the rat
                Cthulhu.Utility.SpawnPawnsOfCountAt(CultsDefOf.Rat, intVec, map, Rand.Range(1, 5), null, false, true);
                
                num++;
            }
            if (num > 0)
            {
                Find.CameraDriver.shaker.DoShake(1f);
                Find.LetterStack.ReceiveLetter(this.def.letterLabel, this.def.letterText, this.def.letterDef, new TargetInfo(map.GetComponent<MapComponent_SacrificeTracker>().lastLocation, map), null);
                Messages.Message("Cults_RatsMessage".Translate(), MessageTypeDefOf.NegativeEvent);
                Cthulhu.Utility.ApplyTaleDef("Cults_SpellRatsInTheWalls", map);
            }
            return num > 0;
        }
    }
}
