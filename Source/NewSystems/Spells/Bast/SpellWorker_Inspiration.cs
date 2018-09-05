using CultOfCthulhu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace BastCult
{
    /// <summary>
    /// Inspires all colonists with random inspirations.
    /// </summary>
    public class SpellWorker_Inspiration : SpellWorker
    {
        public override bool CanSummonNow(Map map)
        {
            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = parms.target as Map;

            List<InspirationDef> inspirations = DefDatabase<InspirationDef>.AllDefsListForReading;

            //Grab all colonists.
            foreach(Pawn colonist in map.PlayerPawnsForStoryteller)
            {
                //Try twice.
                if (!colonist.mindState.inspirationHandler.TryStartInspiration(inspirations[Rand.Range(0, inspirations.Count)]))
                    colonist.mindState.inspirationHandler.TryStartInspiration(inspirations[Rand.Range(0, inspirations.Count)]);
            }

            return true;
        }
    }
}
