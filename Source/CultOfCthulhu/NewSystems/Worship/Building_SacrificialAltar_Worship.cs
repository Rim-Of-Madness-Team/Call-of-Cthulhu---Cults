// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using System.Collections.Generic;
using Cthulhu;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

//using System.Diagnostics;
//using System.Linq;

// ----------------------------------------------------------------------
// These are RimWorld-specific usings. Activate/Deactivate what you need:
// ----------------------------------------------------------------------
// Always needed
//using VerseBase;         // Material/Graphics handling functions are found here
// RimWorld universal objects are here (like 'Building')
// Needed when you do something with the AI
//using Verse.AI.Group;
//using Verse.Sound;       // Needed when you do something with Sound
//using Verse.Noise;       // Needed when you do something with Noises
// RimWorld specific functions are found here (like 'Building_Battery')

//using RimWorld.Planet;   // RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

namespace CultOfCthulhu
{
    public partial class Building_SacrificialAltar : Building, IBillGiver
    {
        public enum ChangeWorshipType
        {
            MorningWorship,
            EveningWorship
        }

        public bool UsableForBillsAfterFueling()
        {
            return CurrentlyUsableForBills();
        }

        public void TryChangeWorshipValues(ChangeWorshipType type, bool value)
        {
            Utility.DebugReport("Attempting to change worship values: " + type + " " + value);
            //Disabling auto-worship is not a hard thing.
            if (value == false)
            {
                if (type == ChangeWorshipType.EveningWorship)
                {
                    OptionEvening = false;
                }

                if (type == ChangeWorshipType.MorningWorship)
                {
                    OptionMorning = false;
                }

                return;
            }

            var canChange = true;
            //Check if another altar exists.
            foreach (var bld in Map.listerBuildings.allBuildingsColonist)
            {
                //Check all other altars
                if (bld is not Building_SacrificialAltar)
                {
                    continue;
                }

                var altar2 = bld as Building_SacrificialAltar;
                //You want to enable evening worship here?
                if (type == ChangeWorshipType.EveningWorship)
                {
                    if (altar2.OptionEvening)
                    {
                        canChange = false;
                    }
                }

                if (type != ChangeWorshipType.MorningWorship)
                {
                    continue;
                }

                if (altar2.OptionMorning)
                {
                    canChange = false;
                }
            }

            if (!canChange)
            {
                return;
            }

            if (type == ChangeWorshipType.MorningWorship)
            {
                OptionMorning = true;
            }

            if (type == ChangeWorshipType.EveningWorship)
            {
                OptionEvening = true;
            }
        }


        private void CancelWorship()
        {
            var listeners =
                Map.mapPawns.AllPawnsSpawned.FindAll(x => x.RaceProps.intelligence == Intelligence.Humanlike);
            var flag = new bool[listeners.Count];
            foreach (var pawn in listeners)
            {
                if (pawn.Faction != Faction.OfPlayer)
                {
                    continue;
                }

                if (pawn.CurJob.def == CultsDefOf.Cults_HoldWorship ||
                    pawn.CurJob.def == CultsDefOf.Cults_AttendWorship ||
                    pawn.CurJob.def == CultsDefOf.Cults_ReflectOnWorship)
                {
                    pawn.jobs.StopAll();
                }
            }

            ChangeState(State.notinuse);
            //this.currentState = State.off;
            Messages.Message("Cults_CancellingSermon".Translate(), MessageTypeDefOf.NegativeEvent);
        }


        private void TryTimedWorship()
        {
            if (tempCurrentWorshipDeity == null)
            {
                Messages.Message("Cults_NoWorshipWithoutDeity".Translate(), MessageTypeDefOf.RejectInput);
                //CancelWorship();
                return;
            }

            if (tempPreacher == null)
            {
                tempPreacher = CultUtility.DetermineBestPreacher(Map);
            }

            if (Utility.IsMorning(Map))
            {
                didMorningRitual = true;
            }

            if (Utility.IsEvening(Map))
            {
                didEveningRitual = true;
            }

            TryWorship();
        }

        private void TryWorshipForced()
        {
            TryWorship(true);
        }

        private void TryWorship(bool forced = false)
        {
            if (!CanGatherToWorshipNow())
            {
                return;
            }

            switch (currentWorshipState)
            {
                case WorshipState.finished:
                case WorshipState.off:
                    if (IsSacrificing())
                    {
                        string timeOfDay = "Cults_Morning".Translate();
                        if (Utility.IsEvening(Map))
                        {
                            timeOfDay = "Cults_Evening".Translate();
                        }

                        Messages.Message("Cults_MorningEveningSermonInterrupted".Translate(timeOfDay),
                            MessageTypeDefOf.RejectInput);
                    }

                    StartToWorship(forced);
                    return;

                case WorshipState.started:
                case WorshipState.gathering:
                case WorshipState.finishing:
                    Messages.Message("Cults_AlreadyGatheringForASermon".Translate(), TargetInfo.Invalid,
                        MessageTypeDefOf.RejectInput);
                    return;
            }
        }

        private bool CanGatherToWorshipNow()
        {
            if (tempPreacher == null)
            {
                return RejectMessage("Cults_NoPreacher".Translate());
            }

            if (tempCurrentWorshipDeity == null)
            {
                return RejectMessage("Cults_NoCosmicEntity".Translate());
            }

            if (tempPreacher.Drafted)
            {
                return RejectMessage("Cults_NoPreacherDrafted".Translate());
            }

            if (tempPreacher.Dead || tempPreacher.Downed)
            {
                return RejectMessage("Cults_SelectAblebodiedPreacher".Translate(), tempPreacher);
            }

            if (!tempPreacher.CanReserve(this))
            {
                return RejectMessage("Cults_AltarIsReserved".Translate());
            }

            foreach (var thing in Position.GetThingList(Map))
            {
                if (thing is Corpse)
                {
                    return RejectMessage("Cults_AltarNeedsToBeCleared".Translate());
                }
            }

            return true;
        }


        public void StartToWorship(bool forced = false)
        {
            preacher = tempPreacher;
            currentWorshipDeity = tempCurrentWorshipDeity;

            if (Destroyed || !Spawned)
            {
                CultUtility.AbortCongregation(null, "The altar is unavailable.");
                return;
            }

            if (!Utility.IsActorAvailable(preacher))
            {
                CultUtility.AbortCongregation(this, "The preacher, " + preacher.LabelShort + ", is unavaialable.");
                preacher = null;
                return;
            }

            var factionBase = (Settlement) Map.info.parent;

            Messages.Message("WorshipGathering".Translate(factionBase.Label), TargetInfo.Invalid,
                MessageTypeDefOf.NeutralEvent);
            ChangeState(State.worshipping, WorshipState.started);
            //this.currentState = State.started;
            //Map.GetComponent<MapComponent_SacrificeTracker>().lastResult = CultUtility.SacrificeResult.none;

            Utility.DebugReport("Force worship called");
            var job = new Job(CultsDefOf.Cults_HoldWorship, this)
            {
                playerForced = forced
            };
            preacher.jobs.TryTakeOrderedJob(job);
            //preacher.jobs.EndCurrentJob(JobCondition.InterruptForced);
            //GetWorshipGroup(this, Map, forced);
        }


        public static void GetWorshipGroup(Building_SacrificialAltar altar, IEnumerable<IntVec3> inRangeCells,
            bool forced = false)
        {
            altar.GetWorshipGroup(inRangeCells);
        }

        public void GetWorshipGroup(IEnumerable<IntVec3> inRangeCells, bool forced = false)
        {
            _ = Faction;
            _ = this.GetRoom();

            if (AvailableWorshippers == null || AvailableWorshippers.Count <= 0)
            {
                return;
            }

            foreach (var p in AvailableWorshippers)
            {
                if (CultUtility.ShouldAttendWorship(p, this))
                {
                    CultUtility.GiveAttendWorshipJob(this, p);
                }
            }
        }

        public static bool ShouldAttendWorship(Pawn p, Pawn preacher)
        {
            var num = 100; //Forced for testing purposes

            if (p.CurJob.def == CultsDefOf.Cults_AttendWorship)
            {
                num = 0;
            }

            return Rand.RangeInclusive(0, 15) + num >= 20;
        }
    }
}