// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------
using System;
using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
using System.Text;

// ----------------------------------------------------------------------
// These are RimWorld-specific usings. Activate/Deactivate what you need:
// ----------------------------------------------------------------------
using UnityEngine;         // Always needed
//using VerseBase;         // Material/Graphics handling functions are found here
using Verse;               // RimWorld universal objects are here (like 'Building')
using Verse.AI;          // Needed when you do something with the AI
//using Verse.AI.Group;
//using Verse.Sound;       // Needed when you do something with Sound
//using Verse.Noise;       // Needed when you do something with Noises
using RimWorld;            // RimWorld specific functions are found here (like 'Building_Battery')
using RimWorld.Planet;
using System.Linq;
//using RimWorld.Planet;   // RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

namespace CultOfCthulhu
{
    public partial class Building_SacrificialAltar : Building, IBillGiver
    {

        #region Worship

        public enum ChangeWorshipType
        {
            MorningWorship,
            EveningWorship
        };

        public void TryChangeWorshipValues(ChangeWorshipType type, bool value)
        {
            Cthulhu.Utility.DebugReport("Attempting to change worship values: " + type.ToString() + " " + value.ToString());
            //Disabling auto-worship is not a hard thing.
            if (value == false)
            {
                if (type == ChangeWorshipType.EveningWorship) this.OptionEvening = false;
                if (type == ChangeWorshipType.MorningWorship) this.OptionMorning = false;
                return;
            }

            bool canChange = true;
            //Check if another altar exists.
            foreach (Building bld in Map.listerBuildings.allBuildingsColonist)
            {
                //Check all other altars
                if (bld is Building_SacrificialAltar)
                {
                    Building_SacrificialAltar altar2 = bld as Building_SacrificialAltar;
                    //You want to enable evening worship here?
                    if (type == ChangeWorshipType.EveningWorship)
                    {
                        if (altar2.OptionEvening)
                        {
                            canChange = false;
                        }
                    }
                    if (type == ChangeWorshipType.MorningWorship)
                    {
                        if (altar2.OptionMorning)
                        {
                            canChange = false;
                        }
                    }
                }
            }
            if (canChange)
            {
                if (type == ChangeWorshipType.MorningWorship)
                {
                    this.OptionMorning = true;
                }
                if (type == ChangeWorshipType.EveningWorship)
                {
                    this.OptionEvening = true;
                }
            }
        }



        private void CancelWorship()
        {
            Pawn pawn = null;
            List<Pawn> listeners = Map.mapPawns.AllPawnsSpawned.FindAll(x => x.RaceProps.intelligence == Intelligence.Humanlike);
            bool[] flag = new bool[listeners.Count];
            for (int i = 0; i < listeners.Count; i++)
            {
                pawn = listeners[i];
                if (pawn.Faction == Faction.OfPlayer)
                {
                    if (pawn.CurJob.def == CultsDefOf.Cults_HoldWorship ||
                        pawn.CurJob.def == CultsDefOf.Cults_AttendWorship ||
                        pawn.CurJob.def == CultsDefOf.Cults_ReflectOnWorship)
                    {
                        pawn.jobs.StopAll();
                    }
                }
            }
            ChangeState(State.notinuse);
            //this.currentState = State.off;
            Messages.Message("Cults_CancellingSermon".Translate(), MessageSound.Negative);
        }


        private void TryTimedWorship()
        {
            if (tempCurrentWorshipDeity == null)
            {
                Messages.Message("Cults_NoWorshipWithoutDeity".Translate(), MessageSound.RejectInput);
                //CancelWorship();
                return;
            }
            if (tempPreacher == null)
            {
                tempPreacher = CultUtility.DetermineBestPreacher(Map);
            }
            if (Cthulhu.Utility.IsMorning(Map))
            {
                didMorningRitual = true;
            }
            if (Cthulhu.Utility.IsEvening(Map))
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

            if (CanGatherToWorshipNow())
            {
                switch (currentWorshipState)
                {
                    case WorshipState.finished:
                    case WorshipState.off:
                        if (IsSacrificing())
                        {
                            string timeOfDay = "Cults_Morning".Translate();
                            if (Cthulhu.Utility.IsEvening(Map)) timeOfDay = "Cults_Evening".Translate();
                            Messages.Message("Cults_MorningEveningSermonInterrupted".Translate(timeOfDay), MessageSound.RejectInput);
                        }
                        StartToWorship(forced);
                        return;

                    case WorshipState.started:
                    case WorshipState.gathering:
                    case WorshipState.finishing:
                        Messages.Message("Cults_AlreadyGatheringForASermon".Translate(), TargetInfo.Invalid, MessageSound.RejectInput);
                        return;
                }
            }
        }

        private bool CanGatherToWorshipNow()
        {
            if (tempPreacher == null) return RejectMessage("Cults_NoPreacher".Translate());
            if (tempCurrentWorshipDeity == null) return RejectMessage("Cults_NoCosmicEntity".Translate());
            if (tempPreacher.Drafted) return RejectMessage("Cults_NoPreacherDrafted".Translate());
            if (tempPreacher.Dead || this.tempPreacher.Downed) return RejectMessage("Cults_SelectAblebodiedPreacher".Translate(), this.tempPreacher);
            if (!tempPreacher.CanReserve(this)) return RejectMessage("Cults_AltarIsReserved".Translate());
            foreach (var thing in Position.GetThingList(Map))
            {
                if (thing is Corpse) return RejectMessage("Cults_AltarNeedsToBeCleared".Translate());
            }
            return true;
        }


        public void StartToWorship(bool forced = false)
        {
            preacher = tempPreacher;
            currentWorshipDeity = tempCurrentWorshipDeity;

            if (this.Destroyed || !this.Spawned)
            {
                CultUtility.AbortCongregation(null, "The altar is unavailable.");
                return;
            }
            if (!Cthulhu.Utility.IsActorAvailable(this.preacher))
            {
                CultUtility.AbortCongregation(this, "The preacher, " + this.preacher.LabelShort + ", is unavaialable.");
                this.preacher = null;
                return;
            }

            FactionBase factionBase = (FactionBase)this.Map.info.parent;

            Messages.Message("WorshipGathering".Translate(new object[] {
                factionBase.Label
        }), TargetInfo.Invalid, MessageSound.Standard);
            ChangeState(State.worshipping, WorshipState.started);
            //this.currentState = State.started;
            //Map.GetComponent<MapComponent_SacrificeTracker>().lastResult = CultUtility.SacrificeResult.none;

            Cthulhu.Utility.DebugReport("Force worship called");
            Job job = new Job(CultsDefOf.Cults_HoldWorship, this);
            job.playerForced = forced;
            preacher.jobs.TryTakeOrderedJob(job);
            //preacher.jobs.EndCurrentJob(JobCondition.InterruptForced);
            //GetWorshipGroup(this, Map, forced);

        }

        
        public static void GetWorshipGroup(Building_SacrificialAltar altar, IEnumerable<IntVec3> inRangeCells, bool forced = false)
        {
            altar.GetWorshipGroup(inRangeCells, false);
        }

        public void GetWorshipGroup(IEnumerable<IntVec3> inRangeCells, bool forced = false)
        {
            var cultFaction = this.Faction;
            var room = this.GetRoom();
            
            if (AvailableWorshippers != null && AvailableWorshippers.Count > 0)
                foreach (Pawn p in AvailableWorshippers)
                    if (CultUtility.ShouldAttendWorship(p, this))
                        CultUtility.GiveAttendWorshipJob(this, p);
        }

        public static bool ShouldAttendWorship(Pawn p, Pawn preacher)
        {
            int num = 100; //Forced for testing purposes

            if (p.CurJob.def == CultsDefOf.Cults_AttendWorship)
            {
                num = 0;
            }

            if ((Rand.RangeInclusive(0, 15) + num) >= 20)
            {
                return true;
            }

            return false;
        }


        #endregion Worship

    }
}