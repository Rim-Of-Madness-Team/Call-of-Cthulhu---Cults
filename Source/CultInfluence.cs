using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace CultOfCthulhu
{
    public class CultInfluence : IExposable
    {
        public Settlement settlement = null;

        public float influence = 0f;

        public bool dominant = false;

        public CultInfluence()
        {

        }

        public CultInfluence(Settlement newSettlement, float newInfluence)
        {
            settlement = newSettlement;
            influence = newInfluence;
            if (newInfluence == 1.0f) dominant = true;
        }

        public void ExposeData()
        {
            Scribe_References.Look<Settlement>(ref this.settlement, "settlement", false);
            Scribe_Values.Look<float>(ref this.influence, "influence", 0f, false);
            Scribe_Values.Look<bool>(ref this.dominant, "dominant", false, false);
        }

        public override string ToString()
        {
            return string.Concat(new object[]
            {
                "(",
                this.settlement,
                ", influence=",
                this.influence.ToString("F1"),
                (!this.dominant) ? string.Empty : " dominant",
                ")"
            });
        }
    }
}
