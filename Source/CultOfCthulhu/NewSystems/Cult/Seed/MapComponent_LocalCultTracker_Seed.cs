using System.Linq;
using RimWorld;
using Verse;

namespace CultOfCthulhu
{
    internal partial class MapComponent_LocalCultTracker : MapComponent
    {
        private void CultSeedCheck()
        {
            //Check for god-mode spawned things.
            if (CurrentSeedState < CultSeedState.FinishedWriting)
            {
                if (CultUtility.AreOccultGrimoiresAvailable(map))
                {
                    CurrentSeedState = CultSeedState.FinishedWriting;
                }
            }

            if (CurrentSeedState < CultSeedState.NeedTable)
            {
                if (map.listerBuildings.allBuildingsColonist.FirstOrDefault(bld =>
                    bld is Building_SacrificialAltar || bld is Building_ForbiddenReserachCenter) != null)
                {
                    CurrentSeedState = CultSeedState.NeedTable;
                }
            }

            switch (CurrentSeedState)
            {
                case CultSeedState.NeedSeed:
                    NeedSeedCountDown();
                    return;
                case CultSeedState.FinishedSeeing:
                    return;
                case CultSeedState.NeedSeeing:
                    CanDoJob(CultsDefOf.Cults_Investigate, CurrentSeedPawn, CurrentSeedTarget, true);
                    return;

                case CultSeedState.NeedWriting:
                    CanDoJob(CultsDefOf.Cults_WriteTheBook, CurrentSeedPawn);
                    return;
                case CultSeedState.FinishedWriting:
                case CultSeedState.NeedTable:
                    return;
            }
        }

        private void NeedSeedCountDown()
        {
            if (ticksToSpawnCultSeed > 0)
            {
                ticksToSpawnCultSeed -= 1;
            }
            else
            {
                if (GenLocalDate.HourInteger(map) > 21 || GenLocalDate.HourInteger(map) < 6)
                {
                    ticksToSpawnCultSeed = OneDay + Rand.Range(-20000, +20000);
                    var seed = seedIncidents.RandomElement();
                    var parms = StorytellerUtility.DefaultParmsNow(seed.category, map);
                    seed.Worker.TryExecute(parms);
                    return;
                }

                ticksToSpawnCultSeed += GenDate.TicksPerHour;
            }
        }
    }
}