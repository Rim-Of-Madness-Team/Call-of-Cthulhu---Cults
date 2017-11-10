using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace CultOfCthulhu
{
    public class Hediff_Transmogrified : Hediff_Implant
    {
        private float undulationTicks = 0.01f;
        public float UndulationTicks { get => undulationTicks; set => undulationTicks = value; }

        public static float tickMax = 2f;
        public float graphicDiv = 0.75f;
        public static bool tickUp = true;
        public static int tickRate = 8;

        public override void Tick()
        {
            if (this.Part == null)
                this.Part = this.pawn.health.hediffSet.GetNotMissingParts().FirstOrDefault(x => x.def == this.pawn.RaceProps.body.corePart.def);

            if (Find.TickManager.TicksGame % tickRate == 0)
            {
                if (tickUp)
                    undulationTicks += 0.01f;
                else
                    undulationTicks -= 0.01f;

                if (undulationTicks > tickMax)
                {
                    tickUp = false;
                }
                else if (undulationTicks <= 0.01f)
                {
                    tickUp = true;
                }
                undulationTicks = Mathf.Clamp(undulationTicks, 0.01f, tickMax);
            }
        }

        public override string TipStringExtra
        {
            get
            {
                StringBuilder s = new StringBuilder();
                s.Append(base.TipStringExtra);
                s.AppendLine("Cults_Trans_HI_Body".Translate("300%"));
                s.AppendLine("Cults_Trans_HI_Health".Translate("300%"));
                return s.ToString();
            }
        }

        public override bool ShouldRemove => !this.pawn.TryGetComp<CompTransmogrified>().IsTransmogrified;
    }
}
