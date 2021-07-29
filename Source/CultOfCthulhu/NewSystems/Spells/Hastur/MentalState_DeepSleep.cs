using RimWorld;
using Verse.AI;

namespace CultOfCthulhu
{
    public class MentalState_DeepSleepCarcosa : MentalState
    {
        public override RandomSocialMode SocialModeMax()
        {
            return RandomSocialMode.Off;
        }
    }
}