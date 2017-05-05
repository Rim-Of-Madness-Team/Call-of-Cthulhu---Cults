using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace CultOfCthulhu
{
    class Building_ForbiddenReserachCenter : Building_ResearchBench
    {
        private const float MaxCultMindedBoost = 0.9f;
        private const float MaxSanityLoss = 0.9f;
        private const float SocialSkillBoost = 75f;
        private enum WarningLevel : int { None = 0, One = 1, Two = 2, Three = 3, Four = 4 }
        private bool StartedUse = false;
        private WarningLevel warningLevel = WarningLevel.None;
        private Pawn storedPawn = null;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.LookReference<Pawn>(ref this.storedPawn, "storedPawn", false);
        }

        public override void SpawnSetup(Map map)
        {
            base.SpawnSetup(map);
            Cthulhu.UtilityWorldObjectManager.GetUtilityWorldObject<UtilityWorldObject_CosmicDeities>().GenerateCosmicEntitiesIntoWorld();
        }
        
        public Pawn InteractingPawn
        {
            get
            {
                Pawn pawn = null;
                foreach (Pawn p in Map.mapPawns.FreeColonistsSpawned)
                {
                    if (p.Position == this.InteractionCell && p.CurJob.def == JobDefOf.Research)
                    {
                        pawn = p;
                        break;
                    }
                }
                if (pawn == null) StartedUse = false;
                return pawn;
            }
        }

        public override void TickRare()
        {
            base.TickRare();
            ForbiddenChecker();
            ResolveNonOccultProjects();
            GiveInteractionSanityLoss();
        }
        private void ForbiddenChecker()
        {
            ResearchProjectDef currentProject = Find.ResearchManager.currentProj;
            if (currentProject == null) return;
            this.SetForbidden(false);
            if (IsThisCultistResearch(currentProject)) return;
            this.SetForbidden(true);
        }

        public override string GetInspectString()
        {
            StringBuilder s = new StringBuilder();
            s.Append(base.GetInspectString());
            s.AppendLine();
            s.Append("Note: This will auto-forbid on normal research.");
            return s.ToString();

        }

        private void ResolveNonOccultProjects()
        {
            //First, declare our variables.
            ResearchProjectDef currentProject = Find.ResearchManager.currentProj;
            Pawn interactingPawn = InteractingPawn;
            if (currentProject == null) return;

            if (interactingPawn == null) return;
            if (Cthulhu.UtilityWorldObjectManager.GetUtilityWorldObject<UtilityWorldObject_GlobalCultTracker>().cultResearch == null) return;
            //Are we using this for the correct project type?
            this.SetForbidden(false);
            if (IsThisCultistResearch(currentProject)) return;
            this.SetForbidden(true);
            //Uh oh.
            //Let's try and find another research station to research this at.
            Building_ResearchBench bench = null;
            foreach (Building bld in Map.listerBuildings.allBuildingsColonist)
            {
                if (bld != this && bld.def != this.def)
                {
                    if (bld is Building_ResearchBench) bench = bld as Building_ResearchBench;
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
            if (!interactingPawn.CanReach(bench, Verse.AI.PathEndMode.ClosestTouch, Danger.Deadly))
            {
                CancelResearch("Cannot research this project at the forbidden center.");
            }
            if (Map.reservationManager.IsReserved(bench, Faction.OfPlayer))
            {
                this.SetForbidden(true);
                interactingPawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                return;
            }
            
            //Okay, assign a job over there instead of here.
            Job J = new Job(JobDefOf.Research, bench);
            Map.reservationManager.ReleaseAllClaimedBy(interactingPawn);
            interactingPawn.QueueJob(J);
            interactingPawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
        }

        private void CancelResearch(string reason)
        {
            if (reason != null) Messages.Message(reason, MessageSound.RejectInput);
            Find.ResearchManager.currentProj = null;
            Find.ResearchManager.ReapplyAllMods();
            Messages.Message("Research cancelled.", MessageSound.Silent);
        }

        private bool IsThisCultistResearch(ResearchProjectDef currentProject)
        {
            foreach (ResearchProjectDef def in Cthulhu.UtilityWorldObjectManager.GetUtilityWorldObject<UtilityWorldObject_GlobalCultTracker>().cultResearch)
            {
                if (currentProject == def) return true;
            }
            return false;
        }

        private void GiveInteractionSanityLoss()
        {
            Pawn temp = InteractingPawn;
            ResearchProjectDef currentProject = Find.ResearchManager.currentProj;
            float modifier = 0.0040f;

            if (temp == null) return;
            if (currentProject == null) return;
            if (!IsThisCultistResearch(currentProject)) return;
            UsageWarning(temp);
            if (Find.ResearchManager.currentProj == ResearchProjectDef.Named("Forbidden_Lore"))
            {
                modifier *= 1.2f;
                temp.skills.Learn(SkillDefOf.Social, SocialSkillBoost);
            }
            Cthulhu.Utility.ApplySanityLoss(temp, modifier, MaxSanityLoss);
            CultUtility.AffectCultMindedness(temp, modifier, MaxCultMindedBoost);
        }
        private void UsageWarning(Pawn temp)
        {
            float sanityLevel = Cthulhu.Utility.CurrentSanityLoss(temp);
            if (storedPawn != temp)
            {
                storedPawn = temp;
                warningLevel = WarningLevel.None;
            }
            SetWarningLevel(sanityLevel);
            if (!StartedUse)
            {
                StartedUse = true;
                string stringToTranslate = "OccultCenterWarning" + warningLevel.ToString();
                if (stringToTranslate == "OccultCenterWarningNone") return;
                Messages.Message(stringToTranslate.Translate(new object[]
                    {
                    InteractingPawn.Name.ToStringShort,
                    InteractingPawn.gender.GetPronoun(),
                    InteractingPawn.gender.GetObjective(),
                    InteractingPawn.gender.GetPossessive(),
                    }), MessageSound.Standard);
            }
        }
        private void SetWarningLevel(float sanityLevel)
        {
            if (((int)warningLevel < 1) && sanityLevel > Cthulhu.SanityLossSeverity.Initial)
            { StartedUse = false; warningLevel = WarningLevel.One; }
            if (((int)warningLevel < 2) && sanityLevel > Cthulhu.SanityLossSeverity.Minor)
            { StartedUse = false; warningLevel = WarningLevel.Two; }
            if (((int)warningLevel < 3) && sanityLevel > Cthulhu.SanityLossSeverity.Major)
            { StartedUse = false; warningLevel = WarningLevel.Three; }
            if (((int)warningLevel < 4) && sanityLevel > Cthulhu.SanityLossSeverity.Severe)
            { StartedUse = false; warningLevel = WarningLevel.Four; }
        }
    }
}
