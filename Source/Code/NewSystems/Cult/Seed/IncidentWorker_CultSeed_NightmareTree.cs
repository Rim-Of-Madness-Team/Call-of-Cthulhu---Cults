using Cthulhu;
using RimWorld;
using Verse;

namespace CultOfCthulhu
{
    internal class IncidentWorker_CultSeed_NightmareTree : IncidentWorker_CultSeed
    {
        public static bool TryFindRandomSpawnCellForPawnNear(IntVec3 root, Map map, out IntVec3 result,
            int firstTryWithRadius = 4)
        {
            if (root.Standable(map: map) && root.GetFirstPawn(map: map) == null)
            {
                result = root;
            }
            else
            {
                var rootFogged = root.Fogged(map: map);
                var num = firstTryWithRadius;
                for (var i = 0; i < 3; i++)
                {
                    if (CellFinder.TryFindRandomReachableCellNear(root: root, map: map, radius: num,
                        traverseParms: TraverseParms.For(mode: TraverseMode.NoPassClosedDoors),
                        cellValidator: c => c.Standable(map: map) && (rootFogged || !c.Fogged(map: map)) && c.GetFirstPawn(map: map) == null, regionValidator: null,
                        result: out result))
                    {
                        return true;
                    }

                    num *= 2;
                }

                num = firstTryWithRadius + 1;
                while (!CellFinder.TryRandomClosewalkCellNear(root: root, map: map, radius: num, result: out result))
                {
                    if (num > map.Size.x / 2 && num > map.Size.z / 2)
                    {
                        result = root;
                        return false;
                    }

                    num *= 2;
                }
            }

            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!(parms.target is Map map))
            {
                return false;
            }

            //Create a spawn point for our nightmare Tree
            if (!Utility.TryFindSpawnCell(def: CultsDefOf.Cults_MonolithNightmare, nearLoc: map.Center, map: map, maxDist: 60, pos: out var intVec))
            {
                Log.Warning(text: "Failed to find spawn point for nightmare tree.");

                return false;
            }

            //Spawn in the nightmare tree.
            var thing = (Plant) ThingMaker.MakeThing(def: CultsDefOf.Cults_PlantTreeNightmare);
            thing.Growth = 1f;
            GenSpawn.Spawn(newThing: thing, loc: intVec.RandomAdjacentCell8Way(), map: map);
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