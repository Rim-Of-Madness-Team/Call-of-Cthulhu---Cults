using Verse;

namespace CultOfCthulhu
{
    public class CultistExperience : IExposable
    {
        private int sacrificeCount;
        private int preachCount;

        public int SacrificeCount
        {
            get => sacrificeCount;
            set => sacrificeCount = value;
        }

        public int PreachCount
        {
            get => preachCount;
            set => preachCount = value;
        }

        public CultistExperience()
        {
            
        }

        public CultistExperience(CultistExperience copy)
        {
            this.sacrificeCount = copy.SacrificeCount;
            this.preachCount = copy.PreachCount;
        }

        public CultistExperience(int sacrificeCount, int preachCount)
        {
            this.sacrificeCount = sacrificeCount;
            this.preachCount = preachCount;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref sacrificeCount, "sacrificeCount", 0);
            Scribe_Values.Look(ref preachCount, "preachCount", 0);
        }
    }
}