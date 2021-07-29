using RimWorld;
using Verse;

namespace CultOfCthulhu
{
    public class IncidentWorker_MakeCultGameCondition : IncidentWorker_MakeGameCondition
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            //Map map = (Map)parms.target;
            _ = Find.Maps;
            var cultConditionActive =
                Find.World.GameConditionManager.ConditionIsActive(CultsDefOf.CultgameCondition_StarsAreWrong) ||
                Find.World.GameConditionManager.ConditionIsActive(CultsDefOf.CultgameCondition_StarsAreRight);
            var cultAvailable = CultTracker.Get.PlayerCult != null && CultTracker.Get.PlayerCult.active;
            return cultAvailable && !cultConditionActive && base.CanFireNowSub(parms);
        }
    }
}