using System.Collections.Generic;
using Verse;

namespace CultOfCthulhu
{
    public class CompFavoredObject : ThingComp
    {
        public List<FavoredEntry> Deities => Props.deities;

        public CompProperties_FavoredObject Props => (CompProperties_FavoredObject) props;
    }
}