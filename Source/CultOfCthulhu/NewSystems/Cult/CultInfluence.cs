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
            Scribe_References.Look(ref settlement, "settlement");
            Scribe_Values.Look(ref influence, "influence");
            Scribe_Values.Look(ref dominant, "dominant");
        }

        public override string ToString()
        {
            return string.Concat("(", settlement, ", influence=", influence.ToString("F1"),
                !dominant ? string.Empty : " dominant", ")");
        }
    }
}