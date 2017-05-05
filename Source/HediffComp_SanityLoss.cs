using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace CultOfCthulhu
{
    public class HediffComp_SanityLoss : HediffComp_SeverityPerDay
    {
        public override void CompPostTick()
        {
            base.CompPostTick();
            if (base.Pawn != null)
            {
                if (base.Pawn.RaceProps != null)
                {
                    if (base.Pawn.RaceProps.IsMechanoid)
                    {
                        MakeSane();
                    }
                }
            }

            if (Cthulhu.Utility.IsCosmicHorrorsLoaded())
            {
                if (base.Pawn.GetType().ToString() == "CosmicHorrorPawn")
                {
                    MakeSane();
                }
            }
        }

        public void MakeSane()
        {
            this.parent.Severity -= 1f;
            base.Pawn.health.Notify_HediffChanged(this.parent);
        }
    }

}
