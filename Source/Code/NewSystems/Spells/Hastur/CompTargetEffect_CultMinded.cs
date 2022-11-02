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

            Utility.ApplySanityLoss(pawn: pawn, sanityLoss: 0.9f);
            CultUtility.AffectCultMindedness(pawn: pawn, amount: 0.99f);
            Messages.Message(text: "CompTargetEffectCultMinded".Translate(
                arg1: pawn.Label
            ), def: MessageTypeDefOf.NeutralEvent);
            //pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, null, false, false, null);
            Find.BattleLog.Add(entry: new BattleLogEntry_ItemUsed(caster: user, target: target, itemUsed: this.parent.def, eventDef: RulePackDefOf.Event_ItemUsed));
        }
    }
}