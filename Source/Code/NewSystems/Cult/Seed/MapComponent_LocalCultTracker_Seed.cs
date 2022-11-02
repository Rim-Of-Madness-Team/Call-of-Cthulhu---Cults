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
                if (CultUtility.AreOccultGrimoiresAvailable(map: map))
                {
                    CurrentSeedState = CultSeedState.FinishedWriting;
                }
            }

            if (CurrentSeedState < CultSeedState.NeedTable)
            {
                if (map.listerBuildings.allBuildingsColonist.FirstOrDefault(predicate: bld =>
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
                    CanDoJob(job: CultsDefOf.Cults_Investigate, pawn: CurrentSeedPawn, target: CurrentSeedTarget, targetRequired: true);
                    return;

                case CultSeedState.NeedWriting:
                    CanDoJob(job: CultsDefOf.Cults_WriteTheBook, pawn: CurrentSeedPawn);
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
                if (GenLocalDate.HourInteger(map: map) > 21 || GenLocalDate.HourInteger(map: map) < 6)
                {
                    ticksToSpawnCultSeed = OneDay + Rand.Range(min: -20000, max: +20000);
                    var seed = seedIncidents.RandomElement();
                    var parms = StorytellerUtility.DefaultParmsNow(incCat: seed.category, target: map);
                    seed.Worker.TryExecute(parms: parms);
                    return;
                }

                ticksToSpawnCultSeed += GenDate.TicksPerHour;
            }
        }
    }
}