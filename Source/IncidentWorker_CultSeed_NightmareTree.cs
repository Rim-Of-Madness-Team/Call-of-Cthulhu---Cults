﻿using System;
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
        public override bool TryExecute(IncidentParms parms)
        {
            Map map = parms.target as Map;
            //Create a spawn point for our nightmare Tree
            IntVec3 intVec;
            

            if (!Cthulhu.Utility.TryFindSpawnCell(CultDefOfs.PlantTreeNightmare, map.Center, map, 60, out intVec))
            {
                return false;
            }

            //Spawn in the nightmare tree.
            Plant thing = (Plant)ThingMaker.MakeThing(CultDefOfs.PlantTreeNightmare, null);
            thing.Growth = 1f;
            GenPlace.TryPlaceThing(thing, intVec.RandomAdjacentCell8Way(), map, ThingPlaceMode.Near);

            //Find the best researcher
            Pawn researcher = CultUtility.DetermineBestResearcher(map);

            //Clear all jobs for the researcher.
            //Give them a new job to investigate the nightmare tree.
            if (HugsModOptionalCode.cultsForcedInvestigation()) //If forced investigation is allowed.
            {
                Job J = new Job(CultDefOfs.Investigate, researcher, thing);
                researcher.QueueJob(J);
                researcher.jobs.EndCurrentJob(JobCondition.InterruptForced);
            }
            Cthulhu.UtilityWorldObjectManager.GetUtilityWorldObject<UtilityWorldObject_GlobalCultTracker>().currentSeedState = CultSeedState.NeedSeeing;
            map.GetComponent<MapComponent_LocalCultTracker>().CurrentSeedPawn = researcher;
            map.GetComponent<MapComponent_LocalCultTracker>().CurrentSeedTarget = thing;
            
            return true;
        }
    }
}
