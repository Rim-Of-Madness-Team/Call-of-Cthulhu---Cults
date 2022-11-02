using Verse;

namespace CultOfCthulhu
{
    public class WorkGiver_InvestigateTree : WorkGiver_Investigate
    {
        public override ThingRequest PotentialWorkThingRequest =>
            ThingRequest.ForDef(singleDef: CultsDefOf.Cults_PlantTreeNightmare);
    }
}