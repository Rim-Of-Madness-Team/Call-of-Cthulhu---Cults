using System;
using Verse;
using RimWorld;

namespace CultOfCthulhu
{
    public class CompProperties_LaunchablePawn : CompProperties
    {
        public CompProperties_LaunchablePawn()
        {
            this.compClass = typeof(CultOfCthulhu.CompLaunchablePawn);
        }
    }
}
