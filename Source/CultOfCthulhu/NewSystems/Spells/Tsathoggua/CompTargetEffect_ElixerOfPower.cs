using RimWorld;
using Verse;

namespace CultOfCthulhu
{
    public class CompTargetEffect_ElixerOfPower : CompTargetEffect
    {
        public override void DoEffectOn(Pawn user, Thing target)
        {
            var pawn = (Pawn) target;
            if (pawn.Dead)
            {
                return;
            }

            HealthUtility.AdjustSeverity(pawn, HediffDef.Named("Cults_BlackIchor"), 1.0f);
        }
    }
}