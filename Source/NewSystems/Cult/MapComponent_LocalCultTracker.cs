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
        public MapComponent_LocalCultTracker(Map map) : base(map)
        {
            this.map = map;
        }

        public const int OneMinute = 3600;
        public const int OneDay = 60000;
        public const int ThreeDays = 180000;
        
        //WorldComponent_GlobalCultTracker globalCultTracker = Find.World.GetComponent<WorldComponent_GlobalCultTracker>();

        public CultSeedState CurrentSeedState { get { return CultTracker.Get.currentSeedState; } set { CultTracker.Get.currentSeedState = value; } }
        public List<Pawn> antiCultists { get { return CultTracker.Get.antiCultists; } }

        public bool needPreacher = false;
        public bool doingInquisition = false;
        public int ticksToSpawnHelpfulPreacher = OneMinute + Rand.Range(OneMinute, OneDay);
        public int ticksToCheckCultists = 0;
        public int ticksUntilInquisition = 0;

        //Cult seed stuff
        public Pawn CurrentSeedPawn = null;
        public Thing CurrentSeedTarget = null;
        public int ticksToTryJobAgain = OneMinute; //1 minute
        public int ticksToSpawnCultSeed = ThreeDays + Rand.Range(-OneDay, OneDay); //Between 2-4 days. 1 day = 60000
        //public int ticksToSpawnCultSeed = (OneMinute + 1000) + Rand.Range(-OneMinute, OneMinute); //Between 2-4 days. 1 day = 60000

        public List<IncidentDef> seedIncidents = new List<IncidentDef>()
        {
            IncidentDef.Named("CultSeedIncident_TreeOfNightmares"),
            IncidentDef.Named("CultSeedIncident_NightmareMonolith")
        };

        public void ResolveTerribleCultFounder(Pawn founder)
        {
            if (founder == null) return;
            if (founder.skills.GetSkill(SkillDefOf.Social).Level > 5) return; //A preacher with at least 5 preaching skill can be a good preacher for a cultist colony.

            //We need a preacher!
            needPreacher = true;
            ticksToSpawnHelpfulPreacher = OneMinute + Rand.Range(OneMinute, OneDay);
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            this.CultSeedCheck();
            this.NewCultistCheck();
            this.ResetResearchCheck();
            this.PreacherCheck();
            this.InquisitionCheck();
        }


        public bool TryFindPreacher(out Pawn preacher)
        {
            preacher = null;
            if (CultTracker.Get.PlayerCult != null)
            {
                List<Pawn> tempList = new List<Pawn>(CultTracker.Get.PlayerCult.members);
                foreach (Pawn current in tempList.InRandomOrder<Pawn>())
                {
                    if (current == null) continue;
                    if (current.Dead) { CultTracker.Get.PlayerCult.RemoveMember(current); continue; }
                    if (preacher == null) preacher = current;
                    if (current.skills.GetSkill(SkillDefOf.Social).Level > preacher.skills.GetSkill(SkillDefOf.Social).Level) preacher = current;
                }
                if (preacher != null) return true;
            }
            return false;
        }

        public void PreacherCheck()
        {
            if (!needPreacher) return;
            if (ticksToSpawnHelpfulPreacher > 0)
            {
                ticksToSpawnHelpfulPreacher--;
                return;
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


        public void ResetResearchCheck()
        {
            try
            {
                ResearchProjectDef repeatableResearch = ResearchProjectDef.Named("Forbidden_Lore");

                if (repeatableResearch != null)
                {
                    if (ModSettings_Data.cultsStudySuccessfulCultsIsRepeatable == true)
                    {
                        if (repeatableResearch.IsFinished)
                        {
                            Cthulhu.Utility.ChangeResearchProgress(repeatableResearch, 0f, true);
                            Messages.Message("RepeatableResearch".Translate(new object[] {
                    repeatableResearch.LabelCap
                }), MessageSound.Benefit);
                        }
                    }
                }
            }
            catch (NullReferenceException) { }
        }

        public void NewCultistCheck()
        {
            if (CurrentSeedState < CultSeedState.FinishedWriting) return;
            if (ticksToCheckCultists == 0) ticksToCheckCultists = Find.TickManager.TicksGame + 500;
            if (ticksToCheckCultists < Find.TickManager.TicksGame)
            {
                ticksToCheckCultists = Find.TickManager.TicksGame + 500;
                IEnumerable<Pawn> spawnedColonyMembers = map.mapPawns.FreeColonistsAndPrisonersSpawned;
                Cult playerCult = CultTracker.Get.PlayerCult;
                if (spawnedColonyMembers != null)
                {
                    if (spawnedColonyMembers.Count<Pawn>() > 0)
                    {
                        foreach (Pawn colonist in map.mapPawns.FreeColonistsAndPrisonersSpawned)
                        {
                            
                            Need_CultMindedness cultMind = colonist.needs.TryGetNeed<Need_CultMindedness>();
                            if (cultMind != null)
                            {
                                if (cultMind.CurLevelPercentage > 0.7)
                                {
                                    if (playerCult == null)
                                    {
                                        playerCult = new Cult(colonist);
                                    }
                                    playerCult.SetMember(colonist);
                                }
                                else if (cultMind.CurInstantLevelPercentage > 0.3 &&
                                    cultMind.CurInstantLevelPercentage < 0.7)
                                {
                                    if (playerCult != null)
                                    {
                                        playerCult.RemoveMember(colonist);
                                        CultTracker.Get.RemoveInquisitor(colonist);
                                    }
                                }
                                else if (cultMind.CurInstantLevelPercentage < 0.3)
                                {
                                    CultTracker.Get.SetInquisitor(colonist);
                                }
                            }
                            if (colonist.Dead)
                            {
                                playerCult.RemoveMember(colonist);
                                //Log.Messag("9b");

                                CultTracker.Get.RemoveInquisitor(colonist);
                                continue;
                            }
                            //Log.Messag("10");

                        }
                    }
                }
            }
        }

        public bool CanDoJob(JobDef job, Pawn pawn, Thing target = null, bool targetRequired = false)
        {
            if (pawn == null) return false;
            if (target == null && targetRequired) return false;
            
            if (ModSettings_Data.cultsForcedInvestigation == false && job != CultsDefOf.Cults_WriteTheBook) return false;

            //Toxic Fallout? Let's not force the colonist to do this job.
            if (this.map.GameConditionManager.GetActiveCondition(GameConditionDefOf.ToxicFallout) != null) return false;

            if (ticksToSpawnCultSeed > 0)
            {
                ticksToTryJobAgain -= 1;
            }
            if (CurrentSeedPawn.CurJob.def != job &&
                ticksToTryJobAgain <= 0)
            {
                Job J = new Job(job, pawn);
                if (CurrentSeedTarget != null) J.SetTarget(TargetIndex.B, target);
                pawn.jobs.TryTakeOrderedJob(J);
                //pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                ticksToTryJobAgain = OneMinute;
                return true;
            }
            return false;
        }

        public override void ExposeData()
        {
            //Cult Variables
            Scribe_Values.Look<bool>(ref this.needPreacher, "needPreacher", false, false);
            Scribe_Values.Look<bool>(ref this.doingInquisition, "doingInquisition", false, false);
            Scribe_Values.Look<int>(ref this.ticksToSpawnHelpfulPreacher, "ticksToSpawnHelpfulPreacher", 0, false);
            Scribe_Values.Look<int>(ref this.ticksToCheckCultists, "ticksToCheckCultists", 0, false);
            Scribe_Values.Look<int>(ref this.ticksUntilInquisition, "ticksUntilInquisition", 0, false);

            //Cult Seed Variables
            Scribe_References.Look<Pawn>(ref this.CurrentSeedPawn, "CurrentSeedPawn", false);
            Scribe_References.Look<Thing>(ref this.CurrentSeedTarget, "CurrentSeedTarget", false);
            Scribe_Values.Look<int>(ref this.ticksToSpawnCultSeed, "ticksToSpawnCultSeed", 0, false);
            base.ExposeData();
        }
    }
}
