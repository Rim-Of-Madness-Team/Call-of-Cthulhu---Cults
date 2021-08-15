using RimWorld;
using UnityEngine;
using Verse;

namespace CultOfCthulhu
{
    // <summary>
    // Hawtcrab insanity spread social interaction mechanic
    // Pawn encounters Strange Thing ==> Aquires insanity ==> Talks to other Pawn
    //
    // Then either:
    // Reacts positively ===> Pawn loses insanity ==> other Pawn gains smaller amount of insanity
    // Reacts negatively ===> Pawn gains insanity ==> other Pawn does not gain insanity
    // </summary>
    public class InteractionWorker_SpreadInsanityFailure : InteractionWorker
    {
        //Almost three times the chance
        private const float BaseSelectionWeight = 0.8f;

        //How great the effect is on the sanity values.
        public static readonly FloatRange SANITY_IMPACT = new FloatRange(0.15f, 0.2f);

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

            //We need them to have different mindsets.

            //Normally, it's double chance of happening.
            var math = 2f;
            //Subtract the social skill of the initiator by 10.
            //A social skill of 20 will return a 0 chance of this happening.
            math -= (float) initiator.skills.GetSkill(SkillDefOf.Social).Level / 10;
            //Throw in random chance.
            math += Rand.Range(-0.5f, 0.5f);

            //Especially if they don't like the other guy.
            return initiator.relations.OpinionOf(recipient) < 15 ? Mathf.Clamp(math, 0f, 2f) : 0f;
        }
    }
}