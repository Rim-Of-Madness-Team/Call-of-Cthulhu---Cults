using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Verse.AI;

namespace CultOfCthulhu
{
    class MapComponent_LocalCultTracker : MapComponent
    {
        public MapComponent_LocalCultTracker(Map map) : base(map)
        {
            this.map = map;
        }

        public const int OneMinute = 3600;
        public const int OneDay = 60000;
        public const int ThreeDays = 180000;
        
        UtilityWorldObject_GlobalCultTracker globalCultTracker = Cthulhu.UtilityWorldObjectManager.GetUtilityWorldObject<UtilityWorldObject_GlobalCultTracker>();

        public CultSeedState CurrentSeedState { get { return globalCultTracker.currentSeedState; } set { globalCultTracker.currentSeedState = value; } }
        public List<Pawn> antiCultists { get { return globalCultTracker.antiCultists; } }
        public List<Pawn> CultMembers { get { return globalCultTracker.cultMembers; } }
        public List<Pawn> LocalCultMembers {
            get {
                List<Pawn> locals = new List<Pawn>();
                if (globalCultTracker.cultMembers != null)
                {
                    foreach (Pawn pawn in globalCultTracker.cultMembers)
                    {
                        if (pawn != null)
                        {
                            if (pawn.Map == this.map) locals.Add(pawn);
                        }
                    }
                }
                return locals;
            }
        }
        public string CultName { get { return globalCultTracker.cultName; } set { globalCultTracker.cultName = value; } }
        public bool DoesCultExist { get { return globalCultTracker.doesCultExist; } }

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

        public static MapComponent_LocalCultTracker Get(Map map)
        {
            MapComponent_LocalCultTracker MapComponent_CultInception = map.components.OfType<MapComponent_LocalCultTracker>().FirstOrDefault<MapComponent_LocalCultTracker>();
            if (MapComponent_CultInception == null)
            {
                MapComponent_CultInception = new MapComponent_LocalCultTracker(map);
                map.components.Add(MapComponent_CultInception);
            }
            return MapComponent_CultInception;
        }

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
                int ran = Rand.Range(1, 2);
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

            foreach (Pawn antiCultist in assailants)
            {
                if (antiCultist == null) continue;
                if (!Cthulhu.Utility.IsActorAvailable(antiCultist)) continue;
                antiCultist.needs.mood.thoughts.memories.TryGainMemoryThought(DefDatabase<ThoughtDef>.GetNamed("MidnightInquisition"));
                Job J = new Job(CultDefOfs.MidnightInquisition, antiCultist, preacher);
                //antiCultist.MentalState.ForceHostileTo(Faction.OfPlayer);
                antiCultist.QueueJob(J);
                antiCultist.jobs.EndCurrentJob(JobCondition.InterruptForced);
            }
        }

        public bool TryFindPreacher(out Pawn preacher)
        {
            preacher = null;
            List<Pawn> tempList = new List<Pawn>(globalCultTracker.cultMembers);
            foreach (Pawn current in tempList.InRandomOrder<Pawn>())
            {
                if (current == null) continue;
                if (current.Dead) { globalCultTracker.RemoveMember(current); continue; }
                if (preacher == null) preacher = current;
                if (current.skills.GetSkill(SkillDefOf.Social).Level > preacher.skills.GetSkill(SkillDefOf.Social).Level) preacher = current;
            }
            if (preacher != null) return true;
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
                    if (HugsModOptionalCode.cultsStudySuccessfulCultsIsRepeatable() == true)
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
            //Log.Messag("1");
            if (CurrentSeedState < CultSeedState.FinishedWriting) return;
            //Log.Messag("2");
            if (ticksToCheckCultists == 0) ticksToCheckCultists = Find.TickManager.TicksGame + 500;
            //Log.Messag("3");

            if (ticksToCheckCultists < Find.TickManager.TicksGame)
            {
                //Log.Messag("4");

                ticksToCheckCultists = Find.TickManager.TicksGame + 500;
            //Log.Messag("5");
                //Cthulhu.Utility.DebugReport("Cultist Check: Tick");
                IEnumerable<Pawn> spawnedColonyMembers = map.mapPawns.FreeColonistsAndPrisonersSpawned;
                //Log.Messag("6");

                if (spawnedColonyMembers != null)
                {
                    //Log.Messag("7");

                    if (spawnedColonyMembers.Count<Pawn>() > 0)
                    {
                        //Log.Messag("8");

                        foreach (Pawn colonist in map.mapPawns.FreeColonistsAndPrisonersSpawned)
                        {
                            //Log.Messag("9");

                            if (colonist.Dead)
                            {
                                //Log.Messag("9a");

                                globalCultTracker.RemoveMember(colonist);
                                //Log.Messag("9b");

                                globalCultTracker.RemoveInquisitor(colonist);
                                continue;
                            }
                            //Log.Messag("10");

                            Need_CultMindedness cultMind = colonist.needs.TryGetNeed<Need_CultMindedness>();
                            if (cultMind != null)
                            {
                                //Log.Messag("11");

                                if (cultMind.CurLevelPercentage > 0.7)
                                {
                                    //Log.Messag("11a");

                                    globalCultTracker.SetMember(colonist);
                                }
                                else if (cultMind.CurInstantLevelPercentage > 0.3 &&
                                    cultMind.CurInstantLevelPercentage < 0.7)
                                {
                                    //Log.Messag("11b");

                                    globalCultTracker.RemoveMember(colonist);
                                    globalCultTracker.RemoveInquisitor(colonist);
                                }
                                else if (cultMind.CurInstantLevelPercentage < 0.3)
                                {
                                    //Log.Messag("11c");

                                    globalCultTracker.SetInquisitor(colonist);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void CultSeedCheck()
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
                if (map.listerBuildings.allBuildingsColonist.FirstOrDefault((Building bld) => bld is Building_SacrificialAltar || bld is Building_ForbiddenReserachCenter) != null)
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
                    CanDoJob(CultDefOfs.Investigate, CurrentSeedPawn, CurrentSeedTarget);
                    return;

                case CultSeedState.NeedWriting:
                    CanDoJob(CultDefOfs.WriteTheBook, CurrentSeedPawn);
                    return;
                case CultSeedState.FinishedWriting:
                case CultSeedState.NeedTable:
                    return;
            }
        }

        public void NeedSeedCountDown()
        {
            if (ticksToSpawnCultSeed > 0) ticksToSpawnCultSeed -= 1;
            else
            {
                ticksToSpawnCultSeed = OneDay + Rand.Range(-20000, +20000);
                IncidentDef seed = seedIncidents.RandomElement<IncidentDef>();
                IncidentParms parms = StorytellerUtility.DefaultParmsNow(Find.Storyteller.def, seed.category, map);
                seed.Worker.TryExecute(parms);
            }
        }

        public bool CanDoJob(JobDef job, Pawn pawn, Thing target = null)
        {
            if (pawn == null) return false;
            
            if (HugsModOptionalCode.cultsForcedInvestigation() == false && job != CultDefOfs.WriteTheBook) return false;

            //Toxic Fallout? Let's not force the colonist to do this job.
            if (this.map.mapConditionManager.GetActiveCondition(MapConditionDefOf.ToxicFallout) != null) return false;

            if (ticksToSpawnCultSeed > 0)
            {
                ticksToTryJobAgain -= 1;
            }
            if (CurrentSeedPawn.CurJob.def != job &&
                ticksToTryJobAgain <= 0)
            {
                Job J = new Job(job, pawn);
                if (CurrentSeedTarget != null) J.SetTarget(TargetIndex.B, target);
                pawn.QueueJob(J);
                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                ticksToTryJobAgain = OneMinute;
                return true;
            }
            return false;
        }

        public override void ExposeData()
        {
            //Cult Variables
            Scribe_Values.LookValue<bool>(ref this.needPreacher, "needPreacher", false, false);
            Scribe_Values.LookValue<bool>(ref this.doingInquisition, "doingInquisition", false, false);
            Scribe_Values.LookValue<int>(ref this.ticksToSpawnHelpfulPreacher, "ticksToSpawnHelpfulPreacher", 0, false);
            Scribe_Values.LookValue<int>(ref this.ticksToCheckCultists, "ticksToCheckCultists", 0, false);
            Scribe_Values.LookValue<int>(ref this.ticksUntilInquisition, "ticksUntilInquisition", 0, false);

            //Cult Seed Variables
            Scribe_References.LookReference<Pawn>(ref this.CurrentSeedPawn, "CurrentSeedPawn", false);
            Scribe_References.LookReference<Thing>(ref this.CurrentSeedTarget, "CurrentSeedTarget", false);
            Scribe_Values.LookValue<int>(ref this.ticksToSpawnCultSeed, "ticksToSpawnCultSeed", 0, false);
            base.ExposeData();
        }
    }
}
