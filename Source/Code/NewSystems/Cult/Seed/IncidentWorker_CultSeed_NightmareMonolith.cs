using Cthulhu;
using RimWorld;
using Verse;

namespace CultOfCthulhu
{
    internal class IncidentWorker_CultSeed_NightmareMonolith : IncidentWorker_CultSeed
    {
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            //Create a spawn point for our nightmare Tree
            if (!(parms.target is Map map))
            {
                return false;
            }

            if (!Utility.TryFindSpawnCell(def: CultsDefOf.Cults_MonolithNightmare, nearLoc: map.Center, map: map, maxDist: 60, pos: out var intVec))
            {
                return false;
            }

            //Spawn in the nightmare tree.
            var thing = (Building) ThingMaker.MakeThing(def: CultsDefOf.Cults_MonolithNightmare);
            //thing.Growth = 1f;
            GenPlace.TryPlaceThing(thing: thing, center: intVec.RandomAdjacentCell8Way(), map: map, mode: ThingPlaceMode.Near);

            ////Find the best researcher
            //Pawn researcher = CultUtility.DetermineBestResearcher(map);

            ////Clear all jobs for the researcher.
            ////Give them a new job to investigate the nightmare tree.
            //if (ModSettings_Data.cultsForcedInvestigation) //ModSettings.cultsForcedInvestigation()) //If forced investigation is allowed.
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