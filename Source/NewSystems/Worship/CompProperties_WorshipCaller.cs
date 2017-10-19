using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace CultOfCthulhu
{
    public class CompProperties_WorshipCaller : CompProperties
    {
        public float rangeRadius = 5.9f;
        public SoundDef hitSound = SoundDefOf.TinyBell;

        public CompProperties_WorshipCaller()
        {
            this.compClass = typeof(CompWorshipCaller);
        }
    }
}
