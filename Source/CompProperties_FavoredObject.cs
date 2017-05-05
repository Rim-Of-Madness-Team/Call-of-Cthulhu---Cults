using System;
using Verse;
using RimWorld;
using System.Collections.Generic;

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
            this.compClass = typeof(CompFavoredObject);
        }
    }
}
