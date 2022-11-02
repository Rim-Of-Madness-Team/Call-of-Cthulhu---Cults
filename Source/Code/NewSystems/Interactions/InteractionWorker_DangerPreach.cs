using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace CultOfCthulhu
{
    /// <summary>
    ///     Cultist shares ideas with a non-cultist for massive failure.
    /// </summary>
    public class InteractionWorker_DangerPreach : InteractionWorker
    {
        //How great the effect is on the cultminded values.
        public const float CULTMINDED_EFFECT_MIN = -0.15f;
        public const float CULTMINDED_EFFECT_MAX = -0.2f;

        //Almost three times the chance
        private const float BaseSelectionWeight = 0.8f;

        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks,
            out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            base.Interacted(initiator: initiator, recipient: recipient, extraSentencePacks: extraSentencePacks, letterText: out letterText, letterLabel: out letterLabel, letterDef: out letterDef,
                lookTargets: out lookTargets);
            CultUtility.AffectCultMindedness(pawn: recipient, amount: Rand.Range(min: CULTMINDED_EFFECT_MIN, max: CULTMINDED_EFFECT_MAX));
        }

        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            //We need two individuals that are part of the colony
            if (!initiator.IsColonist || !initiator.IsPrisonerOfColony || !initiator.IsSlaveOfColony)
            {
                return 0f;
            }

            if (!recipient.IsColonist || !recipient.IsPrisonerOfColony || !initiator.IsSlaveOfColony )
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

            //We need them to have different mindsets.
            if (CultUtility.IsCultMinded(pawn: recipient))
            {
                return 0f;
            }

            if (!CultUtility.IsCultMinded(pawn: initiator))
            {
                return 0f;
            }

            //Normally, it's double chance of happening.
            var math = 2f;
            //Subtract the social skill of the initiator by 10.
            //A social skill of 20 will return a 0 chance of this happening.
            math -= (float) initiator.skills.GetSkill(skillDef: SkillDefOf.Social).Level / 10;
            //Throw in random chance.
            math += Rand.Range(min: -0.5f, max: 0.5f);

            //Especially if they don't like the other guy.
            return initiator.relations.OpinionOf(other: recipient) < 15 ? Mathf.Clamp(value: math, min: 0f, max: 2f) : 0f;
        }
    }
}