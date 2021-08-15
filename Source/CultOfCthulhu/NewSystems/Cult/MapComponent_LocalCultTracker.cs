using System.Collections.Generic;
using Cthulhu;
using RimWorld;
using Verse;
using Verse.AI;

namespace CultOfCthulhu
{
    internal partial class MapComponent_LocalCultTracker : MapComponent
    {
        public const int OneMinute = 3600;
        public const int OneDay = 60000;

        public const int ThreeDays = 180000;
        //public int ticksToSpawnCultSeed = (OneMinute + 1000) + Rand.Range(-OneMinute, OneMinute); //Between 2-4 days. 1 day = 60000

        public readonly List<IncidentDef> seedIncidents = new List<IncidentDef>
        {
            IncidentDef.Named("CultSeedIncident_TreeOfNightmares"),
            IncidentDef.Named("CultSeedIncident_NightmareMonolith")
        };

        //Cult seed stuff
        public Pawn CurrentSeedPawn;
        public Thing CurrentSeedTarget;
        public bool doingInquisition;

        public bool needPreacher;

        public int ticksToCheckCultists;
        public int ticksToSpawnCultSeed = ThreeDays + Rand.Range(-OneDay, OneDay); //Between 2-4 days. 1 day = 60000
        public int ticksToSpawnHelpfulPreacher = OneMinute + Rand.Range(OneMinute, OneDay);
        public int ticksToTryJobAgain = OneMinute; //1 minute
        public int ticksUntilInquisition;

        public MapComponent_LocalCultTracker(Map map) : base(map)
        {
            this.map = map;
        }

        //WorldComponent_GlobalCultTracker globalCultTracker = Find.World.GetComponent<WorldComponent_GlobalCultTracker>();

        public CultSeedState CurrentSeedState
        {
            get => CultTracker.Get.currentSeedState;
            set => CultTracker.Get.currentSeedState = value;
        }

        public List<Pawn> antiCultists => CultTracker.Get.antiCultists;

        public void ResolveTerribleCultFounder(Pawn founder)
        {
            if (founder == null)
            {
                return;
            }

            if (founder.skills.GetSkill(SkillDefOf.Social).Level > 5)
            {
                return; //A preacher with at least 5 preaching skill can be a good preacher for a cultist colony.
            }

            //We need a preacher!
            needPreacher = true;
            ticksToSpawnHelpfulPreacher = OneMinute + Rand.Range(OneMinute, OneDay);
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            CultSeedCheck();
            NewCultistCheck();
            ResetResearchCheck();
            PreacherCheck();
            InquisitionCheck();
        }


        private bool TryFindPreacher(out Pawn preacher)
        {
            preacher = null;
            if (CultTracker.Get.PlayerCult == null)
            {
                return false;
            }

            var tempList = new List<Pawn>(CultTracker.Get.PlayerCult.members);
            foreach (var current in tempList.InRandomOrder())
            {
                if (current == null)
                {
                    continue;
                }

                if (current.Dead)
                {
                    CultTracker.Get.PlayerCult.RemoveMember(current);
                    continue;
                }

                if (preacher == null)
                {
                    preacher = current;
                }

                if (current.skills.GetSkill(SkillDefOf.Social).Level >
                    preacher.skills.GetSkill(SkillDefOf.Social).Level)
                {
                    preacher = current;
                }
            }

            if (preacher != null)
            {
                return true;
            }

            return false;
        }

        private void PreacherCheck()
        {
            if (!needPreacher)
            {
                return;
            }

            if (ticksToSpawnHelpfulPreacher > 0)
            {
                ticksToSpawnHelpfulPreacher--;
            }
            else
            {
                if (!CultUtility.TrySpawnWalkInCultist(map, CultUtility.CultistType.Preacher))
                {
                    //Log.Messag("Failed to spawn walk-in cultist");
                }

                needPreacher = false;
            }
        }


        private void ResetResearchCheck()
        {
            try
            {
                var repeatableResearch = ResearchProjectDef.Named("Forbidden_Lore");

                if (repeatableResearch == null)
                {
                    return;
                }

                if (!ModSettings_Data.cultsStudySuccessfulCultsIsRepeatable)
                {
                    return;
                }

                if (!repeatableResearch.IsFinished)
                {
                    return;
                }

                Utility.ChangeResearchProgress(repeatableResearch, 0f, true);
                Messages.Message("RepeatableResearch".Translate(
                    repeatableResearch.LabelCap
                ), MessageTypeDefOf.PositiveEvent);
            }
            catch
            {
                // ignored
            }
        }

        private void NewCultistCheck()
        {
            if (CurrentSeedState < CultSeedState.FinishedWriting)
            {
                return;
            }

            //Cult Tick (500 ticks)
            if (ticksToCheckCultists == 0)
            {
                ticksToCheckCultists = Find.TickManager.TicksGame + 500;
            }

            if (ticksToCheckCultists >= Find.TickManager.TicksGame)
            {
                return;
            }

            ticksToCheckCultists = Find.TickManager.TicksGame + 500;

            var spawnedColonyMembers = CultUtility.GetCultMindedAffectablePawns(map);
            var playerCult = CultTracker.Get.PlayerCult;
            if (spawnedColonyMembers.Count == 0)
            {
                return;
            }

            foreach (var colonist in spawnedColonyMembers)
            {
                if (!colonist.RaceProps.Humanlike ||
                    (colonist.IsPrisonerOfColony &&
                    !colonist.IsSlaveOfColony) ||
                    colonist.RaceProps.intelligence != Intelligence.Humanlike ||
                    colonist.Dead)
                {
                    playerCult?.RemoveMember(colonist);

                    CultTracker.Get.RemoveInquisitor(colonist);
                    continue;
                }

                if (colonist.needs.TryGetNeed<Need_CultMindedness>() is not Need_CultMindedness cultMind)
                {
                    continue;
                }

                //Cult-Mindedness Above 70%? You will join the cult.
                if (cultMind.CurLevelPercentage > CultLevel.Cultist)
                {
                    if (playerCult == null)
                    {
                        playerCult = new Cult(colonist);
                    }

                    playerCult.SetMember(colonist);
                }
                //Otherwise, you will be removed from the cult.
                else if (cultMind.CurInstantLevelPercentage > CultLevel.AntiCultist &&
                         cultMind.CurInstantLevelPercentage < CultLevel.Cultist)
                {
                    if (playerCult == null)
                    {
                        continue;
                    }

                    playerCult.RemoveMember(colonist);
                    CultTracker.Get.RemoveInquisitor(colonist);
                }
                //Those with cult mindedness below 30% will be inquisitors.
                else if (cultMind.CurInstantLevelPercentage < CultLevel.AntiCultist)
                {
                    CultTracker.Get.SetInquisitor(colonist);
                }
            }
        }

        private void CanDoJob(JobDef job, Pawn pawn, Thing target = null, bool targetRequired = false)
        {
            if (pawn == null)
            {
                return;
            }

            if (target == null && targetRequired)
            {
                return;
            }

            if (ModSettings_Data.cultsForcedInvestigation == false && job != CultsDefOf.Cults_WriteTheBook)
            {
                return;
            }

            //Toxic Fallout? Let's not force the colonist to do this job.
            if (map.GameConditionManager.GetActiveCondition(GameConditionDefOf.ToxicFallout) != null)
            {
                return;
            }

            if (ticksToSpawnCultSeed > 0)
            {
                ticksToTryJobAgain -= 1;
            }

            if (CurrentSeedPawn.CurJob.def == job || ticksToTryJobAgain > 0)
            {
                return;
            }

            var J = new Job(job, pawn);
            if (CurrentSeedTarget != null)
            {
                J.SetTarget(TargetIndex.B, target);
            }

            pawn.jobs.TryTakeOrderedJob(J);
            //pawn.CurJob.EndCurrentJob(JobCondition.InterruptForced);
            ticksToTryJobAgain = OneMinute;
        }

        public override void ExposeData()
        {
            //Cult Variables
            Scribe_Values.Look(ref needPreacher, "needPreacher");
            Scribe_Values.Look(ref doingInquisition, "doingInquisition");
            Scribe_Values.Look(ref ticksToSpawnHelpfulPreacher, "ticksToSpawnHelpfulPreacher");
            Scribe_Values.Look(ref ticksToCheckCultists, "ticksToCheckCultists");
            Scribe_Values.Look(ref ticksUntilInquisition, "ticksUntilInquisition");

            //Cult Seed Variables
            Scribe_References.Look(ref CurrentSeedPawn, "CurrentSeedPawn");
            Scribe_References.Look(ref CurrentSeedTarget, "CurrentSeedTarget");
            Scribe_Values.Look(ref ticksToSpawnCultSeed, "ticksToSpawnCultSeed");
            base.ExposeData();
        }
    }
}