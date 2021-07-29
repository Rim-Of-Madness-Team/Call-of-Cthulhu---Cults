using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace CultOfCthulhu
{
    public class Hediff_Transmogrified : Hediff_Implant
    {
        public static float tickMax = 2f;
        public static bool tickUp = true;
        public static int tickRate = 8;
        public float graphicDiv = 0.75f;
        public float UndulationTicks { get; set; } = 0.01f;

        public override string TipStringExtra
        {
            get
            {
                var s = new StringBuilder();
                s.Append(base.TipStringExtra);
                s.AppendLine("Cults_Trans_HI_Body".Translate("300%"));
                s.AppendLine("Cults_Trans_HI_Health".Translate("300%"));
                return s.ToString();
            }
        }

        public override bool ShouldRemove => !pawn.TryGetComp<CompTransmogrified>().IsTransmogrified;

        public override void Tick()
        {
            if (Part == null)
            {
                Part = pawn.health.hediffSet.GetNotMissingParts()
                    .FirstOrDefault(x => x.def == pawn.RaceProps.body.corePart.def);
            }

            if (Find.TickManager.TicksGame % tickRate != 0)
            {
                return;
            }

            if (tickUp)
            {
                UndulationTicks += 0.01f;
            }
            else
            {
                UndulationTicks -= 0.01f;
            }

            if (UndulationTicks > tickMax)
            {
                tickUp = false;
            }
            else if (UndulationTicks <= 0.01f)
            {
                tickUp = true;
            }

            UndulationTicks = Mathf.Clamp(UndulationTicks, 0.01f, tickMax);
        }
    }
}