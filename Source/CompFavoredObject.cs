using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace CultOfCthulhu
{
    public class CompFavoredObject : ThingComp
    {
        public List<FavoredEntry> Deities
        {
            get
            {
                return this.Props.deities;
            }
        }

        public CompProperties_FavoredObject Props
        {
            get
            {
                return (CompProperties_FavoredObject)this.props;
            }
        }
    }
}
