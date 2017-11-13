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
            //Map map = (Map)target;
            List<Map> maps = Find.Maps;
            bool cultConditionActive = 
                Find.World.GameConditionManager.ConditionIsActive(CultsDefOf.CultgameCondition_StarsAreWrong) ||
                Find.World.GameConditionManager.ConditionIsActive(CultsDefOf.CultgameCondition_StarsAreRight);
            bool cultAvailable = CultTracker.Get.PlayerCult != null && CultTracker.Get.PlayerCult.active;
            return cultAvailable && !cultConditionActive && base.CanFireNowSub(target);
        }
    }
}
