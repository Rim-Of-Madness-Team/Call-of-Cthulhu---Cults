using Cthulhu;
using RimWorld;
using Verse;

namespace CultOfCthulhu
{
    public class SpellWorker_SummonByakhee : SpellWorker
    {
        public override bool CanSummonNow(Map map)
        {
            return true;
        }

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            //Cthulhu.Utility.DebugReport("CanFire: " + this.def.defName);
            return true;
        }


        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!(parms.target is Map map))
            {
                return false;
            }

            //Find a drop spot
            if (!CultUtility.TryFindDropCell(nearLoc: map.Center, map: map, maxDist: 70, pos: out var intVec))
            {
                return false;
            }

            parms.spawnCenter = intVec;
            Utility.SpawnPawnsOfCountAt(kindDef: CultsDefOf.Cults_ByakheeRace, at: intVec, map: map, count: 1, fac: Faction.OfPlayer);

            map.GetComponent<MapComponent_SacrificeTracker>().lastLocation = intVec;
            return true;
        }
    }
}