using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CultOfCthulhu
{
    public class IncidentWorker_MakeCultMapCondition : IncidentWorker_MakeMapCondition
    {
        protected override bool CanFireNowSub(IIncidentTarget target)
        {
            Map map = (Map)target;
            MapComponent_LocalCultTracker localCultTracker = map.GetComponent<MapComponent_LocalCultTracker>();
            MapCondition_StarsAreRight starsAreRight = map.mapConditionManager.GetActiveCondition<MapCondition_StarsAreRight>();
            MapCondition_StarsAreWrong starsAreWrong = map.mapConditionManager.GetActiveCondition<MapCondition_StarsAreWrong>();
            bool cultAvailable = false;
            bool cultConditionInctive = false;
            if (localCultTracker != null)
            {
                if (localCultTracker.DoesCultExist)
                {
                    cultAvailable = true;
                }
            }
            if (starsAreRight != null || starsAreWrong != null)
            {
                cultConditionInctive = false;
            }
            return cultAvailable && cultConditionInctive && base.CanFireNowSub(target);
        }
    }
}
