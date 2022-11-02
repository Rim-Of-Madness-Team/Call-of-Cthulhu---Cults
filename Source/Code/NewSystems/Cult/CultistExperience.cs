using Verse;

namespace CultOfCthulhu
{
    public class CultistExperience : IExposable
    {
        private int preachCount;
        private int sacrificeCount;

        public CultistExperience()
        {
        }

        public CultistExperience(CultistExperience copy)
        {
            sacrificeCount = copy.SacrificeCount;
            preachCount = copy.PreachCount;
        }

        public CultistExperience(int sacrificeCount, int preachCount)
        {
            this.sacrificeCount = sacrificeCount;
            this.preachCount = preachCount;
        }

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

        public void ExposeData()
        {
            Scribe_Values.Look(value: ref sacrificeCount, label: "sacrificeCount");
            Scribe_Values.Look(value: ref preachCount, label: "preachCount");
        }
    }
}