using RimWorld;
using Verse;
using Verse.AI;

namespace CultOfCthulhu
{
    public class MentalState_Disillusioned : MentalState
    {
        public override RandomSocialMode SocialModeMax()
        {
            return RandomSocialMode.Off;
        }

        public override void MentalStateTick()
        {
            base.MentalStateTick();
            if (pawn.IsHashIntervalTick(1000))
            {
                CultUtility.AffectCultMindedness(pawn, -0.05f);
                //Cthulhu.Utility.ApplySanityLoss(this.pawn, -0.05f);
            }
        }
    }
}