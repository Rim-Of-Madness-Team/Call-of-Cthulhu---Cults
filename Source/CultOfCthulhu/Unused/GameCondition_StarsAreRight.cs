using RimWorld;
using Verse;

namespace CultOfCthulhu
{
    public class GameCondition_StarsAreRight : GameCondition
    {
        public override float PlantDensityFactor(Map map)
        {
            return 2f;
        }

        public override float AnimalDensityFactor(Map map)
        {
            return 2f;
        }
    }
}