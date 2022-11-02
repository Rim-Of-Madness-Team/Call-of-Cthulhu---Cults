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
            var numberToSleep = Mathf.CeilToInt(f: Mathf.Clamp(value: math, min: 1, max: colonistCount));

            var sleepers = new List<Pawn>(collection: map.mapPawns.FreeColonistsSpawned.InRandomOrder());
            for (var i = 0; i < numberToSleep; i++)
            {
                sleepers[index: i].mindState.mentalStateHandler.TryStartMentalState(stateDef: CultsDefOf.Cults_DeepSleepCarcosa,
                    reason: "Sacrifice".Translate(), forceWake: false, causedByMood: true);
            }

            return true;
        }
    }
}