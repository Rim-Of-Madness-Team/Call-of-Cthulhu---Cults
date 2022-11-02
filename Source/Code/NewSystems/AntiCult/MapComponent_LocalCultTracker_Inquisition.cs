using System.Collections.Generic;
using Cthulhu;
using RimWorld;
using Verse;
using Verse.AI;

namespace CultOfCthulhu
{
    internal partial class MapComponent_LocalCultTracker
    {
        private void InquisitionCheck()
        {
            //Can we have an inquisition?

            //Are there any altars?
            if (!CultUtility.AreAltarsAvailable(map: map))
            {
                return;
            }

            //Do we have enough colonists? 5 is a good number to allow for a purge
            if (map.mapPawns.FreeColonistsSpawnedCount < 5)
            {
                return;
            }

            //We need inquisitors. At least 2.
            if (antiCultists == null)
            {
                return;
            }

            if (antiCultists.Count < 2)
            {
                return;
            }

            //We need 2 violence-capable inquisitors.
            var assailants = new List<Pawn>();
            foreach (var current in antiCultists)
            {
                if (Utility.CapableOfViolence(pawn: current) && current.IsColonist)
                {
                    assailants.Add(item: current);
                }
            }

            if (assailants.Count < 2)
            {
                return;
            }

            //We need night conditions.
            if (!Utility.IsNight(map: map))
            {
                return;
            }

            //We need a preacher
            if (!TryFindPreacher(preacher: out var preacher))
            {
                Utility.DebugReport(x: "Inquisition: Unable to find preacher.");
                return;
            }

            //Check if the assailants equal the preacher...
            foreach (var current in assailants)
            {
                if (current == preacher)
                {
                    return;
                }
            }

            //Set up ticker. Give our plotters a day or two.
            if (ticksUntilInquisition == 0)
            {
                var ran = Rand.Range(min: 1, max: 3);
                ticksUntilInquisition = Find.TickManager.TicksGame + (GenDate.TicksPerDay * ran);
                Utility.DebugReport(x: "Inquisition: Current Ticks: " + Find.TickManager.TicksGame + " Ticker set to: " +
                                       ticksUntilInquisition);
            }

            if (ticksUntilInquisition < Find.TickManager.TicksGame)
            {
                TryInquisition(assailants: assailants, preacher: preacher);
            }
        }

        private void TryInquisition(List<Pawn> assailants, Pawn preacher)
        {
            //Don't try another inquisition for a long time.
            ticksUntilInquisition = Find.TickManager.TicksGame + (GenDate.TicksPerDay * Rand.Range(min: 7, max: 28));

            if (assailants.Contains(item: preacher))
            {
                return;
            }

            foreach (var antiCultist in assailants)
            {
                if (antiCultist == null)
                {
                    continue;
                }

                if (!Utility.IsActorAvailable(preacher: antiCultist))
                {
                    continue;
                }

                antiCultist.needs.mood.thoughts.memories.TryGainMemory(def: CultsDefOf.Cults_MidnightInquisitionThought);
                var J = new Job(def: CultsDefOf.Cults_MidnightInquisition, targetA: antiCultist, targetB: preacher);
                //antiCultist.MentalState.ForceHostileTo(Faction.OfPlayer);
                antiCultist.jobs.TryTakeOrderedJob(job: J);
                //antiCultist.jobs.EndCurrentJob(JobCondition.InterruptForced);
            }
        }
    }
}