using Verse;

namespace CultOfCthulhu
{
    public static class DeityTracker
    {
        public static WorldComponent_CosmicDeities Get
        {
            get
            {
                return Find.World.GetComponent<WorldComponent_CosmicDeities>();
            }
        }
    }
}