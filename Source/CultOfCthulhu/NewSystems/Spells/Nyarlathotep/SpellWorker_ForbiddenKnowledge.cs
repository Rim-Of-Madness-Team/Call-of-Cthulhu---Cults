// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using RimWorld;
using Verse;

// ----------------------------------------------------------------------
// These are RimWorld-specific usings. Activate/Deactivate what you need:
// ----------------------------------------------------------------------
// Always needed
//using VerseBase;         // Material/Graphics handling functions are found here
// RimWorld universal objects are here (like 'Building')
// Needed when you do something with the AI
// Needed when you do something with Sound
// Needed when you do something with Noises
// RimWorld specific functions are found here (like 'Building_Battery')

// RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

namespace CultOfCthulhu
{
    public class SpellWorker_ForbiddenKnowledge : SpellWorker
    {
        private Reason failReason = Reason.Null;


        protected Building_ResearchBench ResearchStation(Map map)
        {
            var benches = map.listerBuildings.AllBuildingsColonistOfClass<Building_ResearchBench>();
            if (benches == null)
            {
                return null;
            }

            if (benches.TryRandomElement(out var bench))
            {
                return bench;
            }

            return null;
        }

        protected ResearchProjectDef ResearchProject()
        {
            return Find.ResearchManager.currentProj;
        }

        public override bool CanSummonNow(Map map)
        {
            var flag = ResearchStation(map) != null && ResearchProject() != null;

            if (ResearchStation(map) == null)
            {
                failReason = Reason.NoBenches;
                flag = false;
            }

            if (ResearchProject() == null)
            {
                failReason = Reason.NoResearchProject;
                flag = false;
            }

            if (flag)
            {
                //Cthulhu.Utility.DebugReport("CanFire: " + this.def.defName);
                return true;
            }

            if (failReason == Reason.NoBenches)
            {
                Messages.Message("There are no research benches to be found.", MessageTypeDefOf.RejectInput);
                failReason = Reason.Null;
                return false;
            }

            if (failReason == Reason.NoResearchProject)
            {
                Messages.Message("There are no research projects currently being researched.",
                    MessageTypeDefOf.RejectInput);
                failReason = Reason.Null;
                return false;
            }

            //Cthulhu.Utility.DebugReport(this.ToString() + " Unknown error");
            failReason = Reason.Null;
            return false;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!(parms.target is Map map))
            {
                return false;
            }

            //Set up variables
            var researchFinishedValue = ResearchProject().baseCost;
            _ = Find.ResearchManager.GetProgress(ResearchProject());
            var researchAddedProgress = 0f;

            researchAddedProgress += (researchFinishedValue + 1) / 2 * 99;

            //Cthulhu.Utility.DebugReport("Research Added: " + researchAddedProgress.ToString());

            //Perform some research
            Find.ResearchManager.ResearchPerformed(researchAddedProgress, executioner(map));


            map.GetComponent<MapComponent_SacrificeTracker>().lastLocation = executioner(map).Position;
            Messages.Message("Nyarlathotep grants your colony forbidden knowledge.", MessageTypeDefOf.PositiveEvent);

            return true;
        }

        private enum Reason
        {
            Null = 0,
            NoBenches,
            NoResearchProject
        }
    }
}