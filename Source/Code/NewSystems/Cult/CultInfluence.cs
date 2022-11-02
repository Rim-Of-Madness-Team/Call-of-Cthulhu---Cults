using RimWorld.Planet;
using Verse;

namespace CultOfCthulhu
{
    public class CultInfluence : IExposable
    {
        public bool dominant;

        public float influence;
        public Settlement settlement;

        public CultInfluence()
        {
        }

        public CultInfluence(Settlement newSettlement, float newInfluence)
        {
            settlement = newSettlement;
            influence = newInfluence;
            if (newInfluence == 1.0f)
            {
                dominant = true;
            }
        }

        public void ExposeData()
        {
            Scribe_References.Look(refee: ref settlement, label: "settlement");
            Scribe_Values.Look(value: ref influence, label: "influence");
            Scribe_Values.Look(value: ref dominant, label: "dominant");
        }

        public override string ToString()
        {
            return string.Concat("(", settlement, ", influence=", influence.ToString(format: "F1"),
                !dominant ? string.Empty : " dominant", ")");
        }
    }
}