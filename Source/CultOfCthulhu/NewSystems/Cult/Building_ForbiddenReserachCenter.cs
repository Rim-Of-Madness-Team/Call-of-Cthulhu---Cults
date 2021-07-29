using System.Text;
using Cthulhu;
using RimWorld;
using Verse;
using Verse.AI;

namespace CultOfCthulhu
{
    internal class Building_ForbiddenReserachCenter : Building_ResearchBench
    {
        private const float MaxCultMindedBoost = 0.9f;
        private const float MaxSanityLoss = 0.9f;
        private const float SocialSkillBoost = 75f;
        private bool initialTick;
        private bool StartedUse;
        private Pawn storedPawn;
        private WarningLevel warningLevel = WarningLevel.None;

        private Pawn InteractingPawn
        {
            get
            {
                Pawn pawn = null;
                foreach (var p in Map.mapPawns.FreeColonistsSpawned)
                {
                    if (p.Position != InteractionCell || p.CurJob.def != JobDefOf.Research)
                    {
                        continue;
                    }

                    pawn = p;
                    break;
                }

                if (pawn == null)
                {
                    StartedUse = false;
                }

                return pawn;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref storedPawn, "storedPawn");
        }

        public override void SpawnSetup(Map map, bool bla)
        {
            base.SpawnSetup(map, bla);
            DeityTracker.Get.orGenerate();
        }

        public override void TickRare()
        {
            base.TickRare();
            InitialTickHandler();
            ForbiddenChecker();
            ResolveNonOccultProjects();
            GiveInteractionSanityLoss();
        }

        private void InitialTickHandler()
        {
            if (initialTick)
            {
                return;
            }

            initialTick = true;
            CultTracker.Get.ExposedToCults = true;
        }

        private void ForbiddenChecker()
        {
            var currentProject = Find.ResearchManager.currentProj;
            if (currentProject == null)
            {
                return;
            }

            this.SetForbidden(false);
            if (IsThisCultistResearch(currentProject))
            {
                return;
            }

            this.SetForbidden(true);
        }

        public override string GetInspectString()
        {
            var s = new StringBuilder();
            s.Append(base.GetInspectString());
            //s.AppendLine();
            s.AppendLine("Cults_AutoForbidWarning".Translate());
            var result = s.ToString();
            result = result.TrimEndNewlines();
            return result;
        }

        private void ResolveNonOccultProjects()
        {
            //First, declare our variables.
            var currentProject = Find.ResearchManager.currentProj;
            var interactingPawn = InteractingPawn;
            if (currentProject == null)
            {
                return;
            }

            if (interactingPawn == null)
            {
                return;
            }

            if (Find.World.GetComponent<WorldComponent_GlobalCultTracker>().cultResearch == null)
            {
                return;
            }

            //Are we using this for the correct project type?
            this.SetForbidden(false);
            if (IsThisCultistResearch(currentProject))
            {
                return;
            }

            this.SetForbidden(true);
            //Uh oh.
            //Let's try and find another research station to research this at.
            Building_ResearchBench bench = null;
            foreach (var bld in Map.listerBuildings.allBuildingsColonist)
            {
                if (bld == this || bld.def == def)
                {
                    continue;
                }

                if (bld is Building_ResearchBench researchBench)
                {
                    bench = researchBench;
                }
            }

            //No building found? Cancel the research projects.
            if (bench == null)
            {
                CancelResearch("Cannot use the grimoire to research standard research projects.");
                return;
            }

            //We found a research bench! Can we send the researcher there?
            if (!currentProject.CanBeResearchedAt(bench, false))
            {
                CancelResearch("Cannot research this project at the forbidden center.");
            }

            if (!interactingPawn.CanReach(bench, PathEndMode.ClosestTouch, Danger.Deadly))
            {
                CancelResearch("Cannot research this project at the forbidden center.");
            }

            if (!interactingPawn.CanReserve(bench)) //Map.reservationManager.IsReserved(bench, Faction.OfPlayer))
            {
                this.SetForbidden(true);
                interactingPawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                return;
            }

            //Okay, assign a job over there instead of here.
            var J = new Job(JobDefOf.Research, bench);
            Map.reservationManager.ReleaseAllClaimedBy(interactingPawn);
            interactingPawn.jobs.TryTakeOrderedJob(J);
            //interactingpawn.CurJob.EndCurrentJob(JobCondition.InterruptForced);
        }

        private void CancelResearch(string reason)
        {
            if (reason != null)
            {
                Messages.Message(reason, MessageTypeDefOf.RejectInput);
            }

            Find.ResearchManager.currentProj = null;
            Find.ResearchManager.ReapplyAllMods();
            Messages.Message("Research cancelled.", MessageTypeDefOf.SilentInput);
        }

        private bool IsThisCultistResearch(ResearchProjectDef currentProject)
        {
            foreach (var researchProjectDef in Find.World.GetComponent<WorldComponent_GlobalCultTracker>().cultResearch)
            {
                if (currentProject == researchProjectDef)
                {
                    return true;
                }
            }

            return false;
        }

        private void GiveInteractionSanityLoss()
        {
            var temp = InteractingPawn;
            var currentProject = Find.ResearchManager.currentProj;
            var modifier = 0.0040f;

            if (temp == null)
            {
                return;
            }

            if (currentProject == null)
            {
                return;
            }

            if (!IsThisCultistResearch(currentProject))
            {
                return;
            }

            UsageWarning(temp);
            if (Find.ResearchManager.currentProj == ResearchProjectDef.Named("Forbidden_Lore"))
            {
                modifier *= 1.2f;
                temp.skills.Learn(SkillDefOf.Social, SocialSkillBoost);
            }

            Utility.ApplySanityLoss(temp, modifier, MaxSanityLoss);
            CultUtility.AffectCultMindedness(temp, modifier, MaxCultMindedBoost);
        }

        private void UsageWarning(Pawn temp)
        {
            var sanityLevel = Utility.CurrentSanityLoss(temp);
            if (storedPawn != temp)
            {
                storedPawn = temp;
                warningLevel = WarningLevel.None;
            }

            SetWarningLevel(sanityLevel);
            if (StartedUse)
            {
                return;
            }

            StartedUse = true;
            var stringToTranslate = "OccultCenterWarning" + warningLevel;
            if (stringToTranslate == "OccultCenterWarningNone")
            {
                return;
            }

            Messages.Message(stringToTranslate.Translate(
                InteractingPawn.Name.ToStringShort,
                InteractingPawn.gender.GetPronoun(),
                InteractingPawn.gender.GetObjective(),
                InteractingPawn.gender.GetPossessive()
            ), MessageTypeDefOf.NeutralEvent);
        }

        private void SetWarningLevel(float sanityLevel)
        {
            if ((int) warningLevel < 1 && sanityLevel > SanityLossSeverity.Initial)
            {
                StartedUse = false;
                warningLevel = WarningLevel.One;
            }

            if ((int) warningLevel < 2 && sanityLevel > SanityLossSeverity.Minor)
            {
                StartedUse = false;
                warningLevel = WarningLevel.Two;
            }

            if ((int) warningLevel < 3 && sanityLevel > SanityLossSeverity.Major)
            {
                StartedUse = false;
                warningLevel = WarningLevel.Three;
            }

            if ((int) warningLevel >= 4 || !(sanityLevel > SanityLossSeverity.Severe))
            {
                return;
            }

            StartedUse = false;
            warningLevel = WarningLevel.Four;
        }

        private enum WarningLevel
        {
            None = 0,
            One = 1,
            Two = 2,
            Three = 3,
            Four = 4
        }
    }
}