using System;
using Verse;
using RimWorld;

namespace CultOfCthulhu
{
    public class CompTargetEffect_ElixerOfPower : CompTargetEffect
    {
        public override void DoEffectOn(Pawn user, Thing target)
        {
            Pawn pawn = (Pawn)target;
            if (pawn.Dead)
            {
                return;
            }
            HealthUtility.AdjustSeverity(pawn, HediffDef.Named("Cults_BlackIchor"), 1.0f);
        }
    }
}
