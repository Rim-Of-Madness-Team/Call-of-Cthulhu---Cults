using CultOfCthulhu;
using RimWorld;
using Verse;

namespace BastCult
{
    /// <summary>
    ///     Inspires all colonists with random inspirations.
    /// </summary>
    public class SpellWorker_Inspiration : SpellWorker
    {
        public override bool CanSummonNow(Map map)
        {
            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var map = parms.target as Map;

            var inspirations = DefDatabase<InspirationDef>.AllDefsListForReading;

            //Grab all colonists.
            if (map?.PlayerPawnsForStoryteller == null)
            {
                return true;
            }

            foreach (var colonist in map.PlayerPawnsForStoryteller)
            {
                //Try twice.
                if (!colonist.mindState.inspirationHandler.TryStartInspiration(
                    def: inspirations[index: Rand.Range(min: 0, max: inspirations.Count)]))
                {
                    colonist.mindState.inspirationHandler.TryStartInspiration(
                        def: inspirations[index: Rand.Range(min: 0, max: inspirations.Count)]);
                }
            }

            return true;
        }
    }
}