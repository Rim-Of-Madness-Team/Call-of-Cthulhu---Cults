using System;
using Verse;
using RimWorld;
using System.Collections.Generic;

namespace CultOfCthulhu
{
    /// <summary>
    /// Cultist shares ideas with a non-cultist successfully.
    /// </summary>
    public class InteractionWorker_SafePreach : InteractionWorker
    {

        //How great the effect is on the cultminded values.
        public const float CULTMINDED_EFFECT_MIN = 0.025f;
        public const float CULTMINDED_EFFECT_MAX = 0.05f;

        //Very common interaction
        private const float BaseSelectionWeight = 1f;

        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel,
            out LetterDef letterDef)
        {
            base.Interacted(initiator, recipient, extraSentencePacks, out letterText, out letterLabel, out letterDef);
            CultUtility.AffectCultMindedness(recipient, Rand.Range(CULTMINDED_EFFECT_MIN, CULTMINDED_EFFECT_MAX));
            CultUtility.AffectCultMindedness(initiator, Rand.Range(CULTMINDED_EFFECT_MIN, CULTMINDED_EFFECT_MAX));
        }

        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            //We need two individuals that are part of the colony
            if (!initiator.IsColonist || !initiator.IsPrisoner) return 0f;
            if (!recipient.IsColonist || !recipient.IsPrisoner) return 0f;

            //If they are sleeping, don't do this.
            if (initiator.jobs.curDriver.asleep) return 0f;
            if (recipient.jobs.curDriver.asleep) return 0f;

            //The recipient must not be cult-minded.
            if (CultUtility.IsCultMinded(recipient)) return 0f;

            //The initiator must be cult-minded.
            if (!CultUtility.IsCultMinded(initiator)) return 0f;

            //If they have a good relationship, increase the chances of the interaction.
            if (initiator.relations.OpinionOf(recipient) > 0) return Rand.Range(0.8f, 1f);
            return 0f;
        }
    }
}
