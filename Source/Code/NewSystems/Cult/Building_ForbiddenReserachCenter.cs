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
            Scribe_References.Look(refee: ref storedPawn, label: "storedPawn");
        }

        public override void SpawnSetup(Map map, bool bla)
        {
            base.SpawnSetup(map: map, respawningAfterLoad: bla);
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

            this.SetForbidden(value: false);
            if (IsThisCultistResearch(currentProject: currentProject))
            {
                return;
            }

            this.SetForbidden(value: true);
        }

        public override string GetInspectString()
        {
            var s = new StringBuilder();
            s.Append(value: base.GetInspectString());
            //s.AppendLine();
            s.AppendLine(value: "Cults_AutoForbidWarning".Translate());
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
            this.SetForbidden(value: false);
            if (IsThisCultistResearch(currentProject: currentProject))
            {
                return;
            }

            this.SetForbidden(value: true);
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
                CancelResearch(reason: "Cannot use the grimoire to research standard research projects.");
                return;
            }

            //We found a research bench! Can we send the researcher there?
            if (!currentProject.CanBeResearchedAt(bench: bench, ignoreResearchBenchPowerStatus: false))
            {
                CancelResearch(reason: "Cannot research this project at the forbidden center.");
            }

            if (!interactingPawn.CanReach(dest: bench, peMode: PathEndMode.ClosestTouch, maxDanger: Danger.Deadly))
            {
                CancelResearch(reason: "Cannot research this project at the forbidden center.");
            }

            if (!interactingPawn.CanReserve(target: bench)) //Map.reservationManager.IsReserved(bench, Faction.OfPlayer))
            {
                this.SetForbidden(value: true);
                interactingPawn.jobs.EndCurrentJob(condition: JobCondition.InterruptForced);
                return;
            }

            //Okay, assign a job over there instead of here.
            var J = new Job(def: JobDefOf.Research, targetA: bench);
            Map.reservationManager.ReleaseAllClaimedBy(claimant: interactingPawn);
            interactingPawn.jobs.TryTakeOrderedJob(job: J);
            //interactingpawn.CurJob.EndCurrentJob(JobCondition.InterruptForced);
        }

        private void CancelResearch(string reason)
        {
            if (reason != null)
            {
                Messages.Message(text: reason, def: MessageTypeDefOf.RejectInput);
            }

            Find.ResearchManager.currentProj = null;
            Find.ResearchManager.ReapplyAllMods();
            Messages.Message(text: "Research cancelled.", def: MessageTypeDefOf.SilentInput);
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

            if (!IsThisCultistResearch(currentProject: currentProject))
            {
                return;
            }

            UsageWarning(temp: temp);
            if (Find.ResearchManager.currentProj == ResearchProjectDef.Named(defName: "Forbidden_Lore"))
            {
                modifier *= 1.2f;
                temp.skills.Learn(sDef: SkillDefOf.Social, xp: SocialSkillBoost);
            }

            Utility.ApplySanityLoss(pawn: temp, sanityLoss: modifier, sanityLossMax: MaxSanityLoss);
            CultUtility.AffectCultMindedness(pawn: temp, amount: modifier, max: MaxCultMindedBoost);
        }

        private void UsageWarning(Pawn temp)
        {
            var sanityLevel = Utility.CurrentSanityLoss(pawn: temp);
            if (storedPawn != temp)
            {
                storedPawn = temp;
                warningLevel = WarningLevel.None;
            }

            SetWarningLevel(sanityLevel: sanityLevel);
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

            Messages.Message(text: stringToTranslate.Translate(
                arg1: InteractingPawn.Name.ToStringShort,
                arg2: InteractingPawn.gender.GetPronoun(),
                arg3: InteractingPawn.gender.GetObjective(),
                arg4: InteractingPawn.gender.GetPossessive()
            ), def: MessageTypeDefOf.NeutralEvent);
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