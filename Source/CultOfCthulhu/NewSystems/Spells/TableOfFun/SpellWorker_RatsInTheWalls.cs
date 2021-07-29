// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Cthulhu;
using RimWorld;
using Verse;

// ----------------------------------------------------------------------
// These are RimWorld-specific usings. Activate/Deactivate what you need:
// ----------------------------------------------------------------------
// Always needed
//using VerseBase;         // Material/Graphics handling functions are found here
// RimWorld universal objects are here (like 'Building')
// Needed when you do something with the AI
// Needed when you do something with Sound
// Needed when you do something with Noises
// RimWorld specific functions are found here (like 'Building_Battery')

// RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

namespace CultOfCthulhu
{
    public class SpellWorker_RatsInTheWalls : SpellWorker
    {
        private readonly List<IntVec3> AvailableFloors = new List<IntVec3>();

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            //Cthulhu.Utility.DebugReport("
            //: " + this.def.defName);
            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!(parms.target is Map map))
            {
                return false;
            }

            var num = 0;
            var countToSpawn = 10;
            for (var i = 0; i < countToSpawn; i++)
            {
                //Find floors in the home area
                var intVec = CellFinderLoose.RandomCellWith(c => c.InBounds(map) &&
                                                                 c.Standable(map) &&
                                                                 !c.InNoBuildEdgeArea(map) &&
                                                                 map.terrainGrid.TerrainAt(c).layerable &&
                                                                 map.areaManager.Home.ActiveCells.Contains(c) &&
                                                                 !c.Fogged(map), map);
                if (intVec == IntVec3.Invalid)
                {
                    //Find smoothed floors in the home area
                    intVec = CellFinderLoose.RandomCellWith(c => c.InBounds(map) &&
                                                                 c.Standable(map) &&
                                                                 !c.InNoBuildEdgeArea(map) &&
                                                                 map.terrainGrid.TerrainAt(c).defName
                                                                     .Contains("_Smooth") &&
                                                                 map.areaManager.Home.ActiveCells.Contains(c) &&
                                                                 !c.Fogged(map), map);
                    if (intVec == IntVec3.Invalid)
                    {
                        //Find floors... anywhere
                        intVec = CellFinderLoose.RandomCellWith(c => c.InBounds(map) &&
                                                                     c.Standable(map) &&
                                                                     !c.InNoBuildEdgeArea(map) &&
                                                                     map.terrainGrid.TerrainAt(c).layerable &&
                                                                     !c.Fogged(map), map);
                        if (intVec == IntVec3.Invalid)
                        {
                            //Find the ground near the players then.
                            if (intVec == IntVec3.Invalid)
                            {
                                intVec = CellFinderLoose.RandomCellWith(c => c.InBounds(map) &&
                                                                             c.Standable(map) &&
                                                                             !c.InNoBuildEdgeArea(map) &&
                                                                             map.areaManager.Home.ActiveCells.Contains(
                                                                                 c) &&
                                                                             !c.Fogged(map), map);
                                if (intVec == IntVec3.Invalid)
                                {
                                    Utility.DebugReport("Error: Can't assign cell for Rats in the Walls spell.");
                                    continue;
                                }
                            }
                        }
                    }
                }

                //Throw some smoke
                FleckMaker.ThrowDustPuff(intVec, map, 1f);

                //Break the floor
                if (intVec.InBounds(map) && map.terrainGrid.TerrainAt(intVec).layerable)
                {
                    map.terrainGrid.RemoveTopLayer(intVec, false);
                }

                //Spawn the rat
                Utility.SpawnPawnsOfCountAt(CultsDefOf.Rat, intVec, map, Rand.Range(1, 5), null, false, true);

                num++;
            }

            if (num <= 0)
            {
                return false;
            }

            Find.CameraDriver.shaker.DoShake(1f);
            Find.LetterStack.ReceiveLetter(def.letterLabel, def.letterText, def.letterDef,
                new TargetInfo(map.GetComponent<MapComponent_SacrificeTracker>().lastLocation, map));
            Messages.Message("Cults_RatsMessage".Translate(), MessageTypeDefOf.NegativeEvent);
            Utility.ApplyTaleDef("Cults_SpellRatsInTheWalls", map);

            return true;
        }
    }
}