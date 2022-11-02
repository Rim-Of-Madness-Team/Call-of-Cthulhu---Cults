using CultOfCthulhu;
using RimWorld;
using Verse;

namespace BastCult
{
    /// <summary>
    ///     Game ending spell of Bast.
    /// </summary>
    public class SpellWorker_PassageToAaru : SpellWorker_GameEndingEffect
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return true;
        }

        public override bool CanSummonNow(Map map)
        {
            return true;
        }

        public override float GetDelay()
        {
            return 10f;
        }

        public override string GetEndScreenText()
        {
            return "Cults_BastGameOver".Translate();
        }
    }
}