using System;
using Verse;
using RimWorld;

namespace CultOfCthulhu
{
    public class CompTargetEffect_CultMinded : CompTargetEffect
    {
        public override void DoEffectOn(Pawn user, Thing target)
        {
            Pawn pawn = (Pawn)target;
            if (pawn.Dead)
            {
                return;
            }
            Cthulhu.Utility.ApplySanityLoss(pawn, 0.9f);
            CultUtility.AffectCultMindedness(pawn, 0.99f, 0.99f);
            Messages.Message("CompTargetEffectCultMinded".Translate(new object[]
            {
                pawn.Label
            }), MessageSound.Standard);
            //pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, null, false, false, null);
        }
    }
}
