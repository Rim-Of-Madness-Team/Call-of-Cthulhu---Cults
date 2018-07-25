using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace CultOfCthulhu
{
    class IncidentWorker_CultSeed_NightmareTree : IncidentWorker_CultSeed
    {
        public static bool TryFindRandomSpawnCellForPawnNear(IntVec3 root, Map map, out IntVec3 result, int firstTryWithRadius = 4)
        {
            bool result2;
            if (root.Standable(map) && root.GetFirstPawn(map) == null)
            {
                result = root;
                result2 = true;
            }
            else
            {
                bool rootFogged = root.Fogged(map);
                int num = firstTryWithRadius;
                for (int i = 0; i < 3; i++)
                {
                    if (CellFinder.TryFindRandomReachableCellNear(root, map, (float)num, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), (IntVec3 c) => c.Standable(map) && (rootFogged || !c.Fogged(map)) && c.GetFirstPawn(map) == null, null, out result, 999999))
                    {
                        return true;
                    }
                    num *= 2;
                }
                num = firstTryWithRadius + 1;
                while (!CellFinder.TryRandomClosewalkCellNear(root, map, num, out result, null))
                {
                    if (num > map.Size.x / 2 && num > map.Size.z / 2)
                    {
                        result = root;
                        return false;
                    }
                    num *= 2;
                }
                result2 = true;
            }
            return result2;
        }
        
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = parms.target as Map;
            //Create a spawn point for our nightmare Tree
            IntVec3 intVec;
            if (!Cthulhu.Utility.TryFindSpawnCell(CultsDefOf.Cults_MonolithNightmare, map.Center, map, 60, out intVec))
            {
                Log.Warning("Failed to find spawn point for nightmare tree.");

                return false;
            }
            //Spawn in the nightmare tree.
            Plant thing = (Plant)ThingMaker.MakeThing(CultsDefOf.Cults_PlantTreeNightmare, null);
            thing.Growth = 1f;
            GenSpawn.Spawn(thing, intVec.RandomAdjacentCell8Way(), map);
            //GenPlace.TryPlaceThing(thing, intVec.RandomAdjacentCell8Way(), map, ThingPlaceMode.Near);

            ////Find the best researcher
            //Pawn researcher = CultUtility.DetermineBestResearcher(map);

            ////Clear all jobs for the researcher.
            ////Give them a new job to investigate the nightmare tree.
            //if (ModSettings_Data.cultsForcedInvestigation) //If forced investigation is allowed.
            //{
            //    Job J = new Job(CultsDefOf.Cults_Investigate, researcher, thing);
            //    researcher.jobs.TryTakeOrderedJob(J);
            //    //researcher.jobs.EndCurrentJob(JobCondition.InterruptForced);
            //}
            Find.World.GetComponent<WorldComponent_GlobalCultTracker>().currentSeedState = CultSeedState.NeedSeeing;
            //map.GetComponent<MapComponent_LocalCultTracker>().CurrentSeedPawn = researcher;
            //map.GetComponent<MapComponent_LocalCultTracker>().CurrentSeedTarget = thing;
            
            return true;
        }
    }
}
