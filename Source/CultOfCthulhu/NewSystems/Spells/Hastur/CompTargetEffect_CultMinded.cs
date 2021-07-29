using Cthulhu;
using RimWorld;
using Verse;

namespace CultOfCthulhu
{
    public class CompTargetEffect_CultMinded : CompTargetEffect
    {
        public override void DoEffectOn(Pawn user, Thing target)
        {
            var pawn = (Pawn) target;
            if (pawn.Dead)
            {
                return;
            }

            Utility.ApplySanityLoss(pawn, 0.9f);
            CultUtility.AffectCultMindedness(pawn, 0.99f);
            Messages.Message("CompTargetEffectCultMinded".Translate(
                pawn.Label
            ), MessageTypeDefOf.NeutralEvent);
            //pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, null, false, false, null);
        }
    }
}