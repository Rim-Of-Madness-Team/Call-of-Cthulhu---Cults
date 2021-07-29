using Cthulhu;
using RimWorld;
using Verse;

namespace CultOfCthulhu
{
    public class SpellWorker_FormlessSpawn : SpellWorker
    {
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!(parms.target is Map map))
            {
                return false;
            }

            //Find a drop spot
            if (!CultUtility.TryFindDropCell(map.Center, map, 70, out var intVec))
            {
                return false;
            }

            parms.spawnCenter = intVec;
            Utility.SpawnPawnsOfCountAt(CultsDefOf.Cults_FormlessSpawn, intVec, map, 1, Faction.OfPlayer);

            map.GetComponent<MapComponent_SacrificeTracker>().lastLocation = intVec;
            return true;
        }
    }
}