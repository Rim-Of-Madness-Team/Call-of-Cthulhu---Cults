using System.Collections.Generic;
using Verse;

namespace CultOfCthulhu
{
    public class FavoredEntry
    {
        public string deityDef;
        public float favorBonus = 0;
    }

    public class CompProperties_FavoredObject : CompProperties
    {
        public List<FavoredEntry> deities;

        public CompProperties_FavoredObject()
        {
            compClass = typeof(CompFavoredObject);
        }
    }
}