using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace CultOfCthulhu
{
    internal class SpellWorker_VisionsOfCarcosa : SpellWorker
    {
        public override bool CanSummonNow(Map map)
        {
            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!(parms.target is Map map))
            {
                return false;
            }

            var colonistCount = (float) map.mapPawns.FreeColonistsSpawned.Count;
            var sleeperPercent = 0.8f;
            var math = colonistCount * sleeperPercent;
            var numberToSleep = Mathf.CeilToInt(Mathf.Clamp(math, 1, colonistCount));

            var sleepers = new List<Pawn>(map.mapPawns.FreeColonistsSpawned.InRandomOrder());
            for (var i = 0; i < numberToSleep; i++)
            {
                sleepers[i].mindState.mentalStateHandler.TryStartMentalState(CultsDefOf.Cults_DeepSleepCarcosa,
                    "Sacrifice".Translate(), false, true);
            }

            return true;
        }
    }
}