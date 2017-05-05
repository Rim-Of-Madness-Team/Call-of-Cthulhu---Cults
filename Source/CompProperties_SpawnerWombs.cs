using System;
using Verse;
using RimWorld;

namespace CultOfCthulhu
{
    public class CompProperties_SpawnerWombs : CompProperties
    {
        public float WombSpawnPreferredMinDist = 3.5f;

        public float WombSpawnRadius = 10f;

        public FloatRange WombSpawnIntervalDays = new FloatRange(1.6f, 2.1f);

        public CompProperties_SpawnerWombs()
        {
            this.compClass = typeof(CompSpawnerWombs);
        }
    }
}
