using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace CultOfCthulhu
{
    public class CompTransmogrified : ThingComp
    {
        public bool isTransmogrified = false;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref this.isTransmogrified, "isTransmogrified", false);
        }
    }
}
