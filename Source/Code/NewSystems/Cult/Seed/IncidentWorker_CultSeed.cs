using RimWorld;
using Verse;

namespace CultOfCthulhu
{
    internal class IncidentWorker_CultSeed : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            var map = (Map) parms.target;
            var tracker = GetTracker(map: map);
            return tracker.CurrentSeedState <= CultSeedState.NeedSeed;
        }

        private MapComponent_LocalCultTracker GetTracker(Map map)
        {
            var result = map.GetComponent<MapComponent_LocalCultTracker>();
            if (map.GetComponent<MapComponent_LocalCultTracker>() != null)
            {
                return result;
            }

            result = new MapComponent_LocalCultTracker(map: map);
            map.components.Add(item: result);

            return result;
        }
    }
}