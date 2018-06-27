using System;
using Verse;
using RimWorld;
using UnityEngine;
using System.Collections.Generic;

namespace CultOfCthulhu
{
    /// <summary>
    /// Cultist shares ideas with a non-cultist for massive failure.
    /// </summary>
    public class InteractionWorker_DangerPreach : InteractionWorker
    {
        //How great the effect is on the cultminded values.
        public const float CULTMINDED_EFFECT_MIN = -0.15f;
        public const float CULTMINDED_EFFECT_MAX = -0.2f;

        //Almost three times the chance
        private const float BaseSelectionWeight = 0.8f;

        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel,
            out LetterDef letterDef)
        {
            base.Interacted(initiator, recipient, extraSentencePacks, out letterText, out letterLabel, out letterDef);
            CultUtility.AffectCultMindedness(recipient, Rand.Range(CULTMINDED_EFFECT_MIN, CULTMINDED_EFFECT_MAX));
        }

        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            //We need two individuals that are part of the colony
            if (!initiator.IsColonist || !initiator.IsPrisoner) return 0f;
            if (!recipient.IsColonist || !recipient.IsPrisoner) return 0f;

            //If they are sleeping, don't do this.
            if (initiator.jobs.curDriver.asleep) return 0f;
            if (recipient.jobs.curDriver.asleep) return 0f;

            //We need them to have different mindsets.
            if (CultUtility.IsCultMinded(recipient)) return 0f;
            if (!CultUtility.IsCultMinded(initiator)) return 0f;

            //Normally, it's double chance of happening.
            float math = 2f;
            //Subtract the social skill of the initiator by 10.
            //A social skill of 20 will return a 0 chance of this happening.
            math -= ((float)(initiator.skills.GetSkill(SkillDefOf.Social).Level) / 10);
            //Throw in random chance.
            math += Rand.Range(-0.5f, 0.5f);

            //Especially if they don't like the other guy.
            if (initiator.relations.OpinionOf(recipient) < 15) return Mathf.Clamp(math, 0f, 2f);
            return 0f;
        }
    }
}
