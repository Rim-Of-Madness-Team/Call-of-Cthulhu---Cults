using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Verse.AI;

namespace CultOfCthulhu
{
    partial class MapComponent_LocalCultTracker : MapComponent
    {

        public void InquisitionCheck()
        {
            //Can we have an inquisition?

            //Are there any altars?
            if (!CultUtility.AreAltarsAvailable(this.map)) return;

            //Do we have enough colonists? 5 is a good number to allow for a purge
            if (map.mapPawns.FreeColonistsSpawnedCount < 5) return;

            //We need inquisitors. At least 2.
            if (antiCultists == null) return;
            if (antiCultists.Count < 2) return;

            //We need 2 violence-capable inquisitors.
            List<Pawn> assailants = new List<Pawn>();
            foreach (Pawn current in antiCultists)
            {
                if (Cthulhu.Utility.CapableOfViolence(current) && current.IsColonist) assailants.Add(current);
            }
            if (assailants == null) return;
            if (assailants.Count < 2) return;

            //We need night conditions.
            if (!Cthulhu.Utility.IsNight(map)) return;

            //We need a preacher
            Pawn preacher;
            if (!TryFindPreacher(out preacher))
            {
                Cthulhu.Utility.DebugReport("Inquisition: Unable to find preacher.");
                return;
            }

            //Check if the assailants equal the preacher...
            foreach (Pawn current in assailants)
            {
                if (current == preacher) return;
            }

            //Set up ticker. Give our plotters a day or two.
            if (ticksUntilInquisition == 0)
            {
                int ran = Rand.Range(1, 3);
                ticksUntilInquisition = Find.TickManager.TicksGame + (GenDate.TicksPerDay * ran);
                Cthulhu.Utility.DebugReport("Inquisition: Current Ticks: " + Find.TickManager.TicksGame.ToString() + " Ticker set to: " + ticksUntilInquisition.ToString());
            }
            if (ticksUntilInquisition < Find.TickManager.TicksGame)
            {
                TryInquisition(assailants, preacher);
            }
        }

        public void TryInquisition(List<Pawn> assailants, Pawn preacher)
        {
            //Don't try another inquisition for a long time.
            ticksUntilInquisition = Find.TickManager.TicksGame + (GenDate.TicksPerDay * Rand.Range(7, 28));

            if (assailants.Contains(preacher)) return;
            foreach (Pawn antiCultist in assailants)
            {
                if (antiCultist == null) continue;
                if (!Cthulhu.Utility.IsActorAvailable(antiCultist)) continue;
                antiCultist.needs.mood.thoughts.memories.TryGainMemory(CultsDefOf.Cults_MidnightInquisitionThought);
                Job J = new Job(CultsDefOf.Cults_MidnightInquisition, antiCultist, preacher);
                //antiCultist.MentalState.ForceHostileTo(Faction.OfPlayer);
                antiCultist.jobs.TryTakeOrderedJob(J);
                //antiCultist.jobs.EndCurrentJob(JobCondition.InterruptForced);
            }
        }
    }
}
