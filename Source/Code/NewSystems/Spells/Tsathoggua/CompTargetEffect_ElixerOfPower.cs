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

            HealthUtility.AdjustSeverity(pawn: pawn, hdDef: HediffDef.Named(defName: "Cults_BlackIchor"), sevOffset: 1.0f);
        }
    }
}