using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CultOfCthulhu
{
    public class IncidentWorker_MakeCultGameCondition : IncidentWorker_MakeGameCondition
    {
        protected override bool CanFireNowSub(IIncidentTarget target)
        {
            Map map = (Map)target;
            MapComponent_LocalCultTracker localCultTracker = map.GetComponent<MapComponent_LocalCultTracker>();
            GameCondition_StarsAreRight starsAreRight = map.GameConditionManager.GetActiveCondition<GameCondition_StarsAreRight>();
            GameCondition_StarsAreWrong starsAreWrong = map.GameConditionManager.GetActiveCondition<GameCondition_StarsAreWrong>();
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
