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
            if (root.Standable(map) && root.GetFirstPawn(map) == null)
            {
                result = root;
            }
            else
            {
                var rootFogged = root.Fogged(map);
                var num = firstTryWithRadius;
                for (var i = 0; i < 3; i++)
                {
                    if (CellFinder.TryFindRandomReachableCellNear(root, map, num,
                        TraverseParms.For(TraverseMode.NoPassClosedDoors),
                        c => c.Standable(map) && (rootFogged || !c.Fogged(map)) && c.GetFirstPawn(map) == null, null,
                        out result))
                    {
                        return true;
                    }

                    num *= 2;
                }

                num = firstTryWithRadius + 1;
                while (!CellFinder.TryRandomClosewalkCellNear(root, map, num, out result))
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
            if (!Utility.TryFindSpawnCell(CultsDefOf.Cults_MonolithNightmare, map.Center, map, 60, out var intVec))
            {
                Log.Warning("Failed to find spawn point for nightmare tree.");

                return false;
            }

            //Spawn in the nightmare tree.
            var thing = (Plant) ThingMaker.MakeThing(CultsDefOf.Cults_PlantTreeNightmare);
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