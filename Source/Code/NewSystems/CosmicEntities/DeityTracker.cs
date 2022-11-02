using Verse;

namespace CultOfCthulhu
{
    public static class DeityTracker
    {
        public static WorldComponent_CosmicDeities Get => Find.World.GetComponent<WorldComponent_CosmicDeities>();
    }
}