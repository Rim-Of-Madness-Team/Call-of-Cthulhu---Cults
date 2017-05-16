using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace CultOfCthulhu
{
    class SpellWorker_VisionsOfCarcosa : SpellWorker
    {
        
        public override bool CanSummonNow(Map map)
        {
            return true;
        }
        public override bool TryExecute(IncidentParms parms)
        {
            Map map = parms.target as Map;

            float colonistCount = (float)map.mapPawns.FreeColonistsSpawned.Count<Pawn>();
            float sleeperPercent = 0.8f;
            float math = colonistCount * sleeperPercent;
            int numberToSleep = Mathf.CeilToInt(Mathf.Clamp(math, 1, colonistCount));

            List<Pawn> sleepers = new List<Pawn>(map.mapPawns.FreeColonistsSpawned.InRandomOrder<Pawn>());
            for (int i = 0; i < numberToSleep; i++)
            {
                 sleepers[i].mindState.mentalStateHandler.TryStartMentalState(CultsDefOf.Cults_DeepSleepCarcosa, "Sacrifice".Translate(), false, true);
            }
            return true;
        }
    }
}
