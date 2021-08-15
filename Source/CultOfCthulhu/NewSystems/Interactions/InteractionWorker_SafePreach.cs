using System.Collections.Generic;
using RimWorld;
using Verse;

namespace CultOfCthulhu
{
    /// <summary>
    ///     Cultist shares ideas with a non-cultist successfully.
    /// </summary>
    public class InteractionWorker_SafePreach : InteractionWorker
    {
        //How great the effect is on the cultminded values.
        public const float CULTMINDED_EFFECT_MIN = 0.025f;
        public const float CULTMINDED_EFFECT_MAX = 0.05f;

        //Very common interaction
        private const float BaseSelectionWeight = 1f;

        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks,
            out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            base.Interacted(initiator, recipient, extraSentencePacks, out letterText, out letterLabel, out letterDef,
                out lookTargets);

            CultUtility.AffectCultMindedness(recipient, Rand.Range(CULTMINDED_EFFECT_MIN, CULTMINDED_EFFECT_MAX));
            CultUtility.AffectCultMindedness(initiator, Rand.Range(CULTMINDED_EFFECT_MIN, CULTMINDED_EFFECT_MAX));
        }


        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            //We need two individuals that are part of the colony
            if (!initiator.IsColonist || !initiator.IsPrisonerOfColony || !initiator.IsSlaveOfColony)
            {
                return 0f;
            }

            if (!recipient.IsColonist || !recipient.IsPrisonerOfColony || !initiator.IsSlaveOfColony)
            {
                return 0f;
            }

            //If they are sleeping, don't do this.
            if (initiator.jobs.curDriver.asleep)
            {
                return 0f;
            }

            if (recipient.jobs.curDriver.asleep)
            {
                return 0f;
            }

            //The recipient must not be cult-minded.
            if (CultUtility.IsCultMinded(recipient))
            {
                return 0f;
            }

            //The initiator must be cult-minded.
            if (!CultUtility.IsCultMinded(initiator))
            {
                return 0f;
            }

            //If they have a good relationship, increase the chances of the interaction.
            return initiator.relations.OpinionOf(recipient) > 0 ? Rand.Range(0.8f, 1f) : 0f;
        }
    }
}