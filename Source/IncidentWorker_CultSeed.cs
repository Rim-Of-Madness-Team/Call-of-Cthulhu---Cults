using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace CultOfCthulhu
{
    class IncidentWorker_CultSeed : IncidentWorker
    {
        protected override bool CanFireNowSub(IIncidentTarget target)
        {
            Map map = target as Map;
            MapComponent_LocalCultTracker tracker = GetTracker(map);
            if (tracker.CurrentSeedState > CultSeedState.NeedSeed) return false;
            else return true;
        }

        private MapComponent_LocalCultTracker GetTracker(Map map)
        {
            MapComponent_LocalCultTracker result = map.GetComponent<MapComponent_LocalCultTracker>();
            if (map.GetComponent<MapComponent_LocalCultTracker>() == null)
            {
                result = new MapComponent_LocalCultTracker(map);
                map.components.Add(result);
            }
            return result;
        }
    }
}
