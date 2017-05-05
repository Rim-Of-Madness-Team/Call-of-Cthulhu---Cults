using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CultOfCthulhu
{
    public class MapCondition_StarsAreRight : MapCondition
    {
        public override float PlantDensityFactor()
        {
            return 2f;
        }

        public override float AnimalDensityFactor()
        {
            return 2f;
        }
        
    }
}
