using RimWorld;
using Verse;

namespace CultOfCthulhu
{
    public class CompProperties_WorshipCaller : CompProperties
    {
        public SoundDef hitSound = SoundDefOf.TinyBell;
        public float rangeRadius = 5.9f;

        public CompProperties_WorshipCaller()
        {
            compClass = typeof(CompWorshipCaller);
        }
    }
}