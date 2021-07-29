// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Cthulhu;
using HarmonyLib;
using RimWorld;
using Verse;

// ----------------------------------------------------------------------
// These are RimWorld-specific usings. Activate/Deactivate what you need:
// ----------------------------------------------------------------------
// Always needed
//using VerseBase;         // Material/Graphics handling functions are found here
// RimWorld universal objects are here (like 'Building')
// Needed when you do something with the AI
// Needed when you do something with Sound
// Needed when you do something with Noises
// RimWorld specific functions are found here (like 'Building_Battery')
// RimWorld specific functions for world creation

//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 


namespace CultOfCthulhu
{
    public class SpellWorker_ChaosTheory : SpellWorker
    {
        public bool HasIncapableWorkTags(Pawn pawn)
        {
            HarmonyPatches.DebugMessage("HasIncapableWorkTags called");
            return pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(WorkTags.Animals)
                   || pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(WorkTags.Artistic)
                   || pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(WorkTags.Caring)
                   || pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(WorkTags.Cleaning)
                   || pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(WorkTags.Cooking)
                   || pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(WorkTags.Crafting)
                   || pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(WorkTags.Firefighting)
                   || pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(WorkTags.Hauling)
                   || pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(WorkTags.Intellectual)
                   || pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(WorkTags.ManualDumb)
                   || pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(WorkTags.ManualSkilled)
                   || pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(WorkTags.Mining)
                   || pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(WorkTags.PlantWork)
                   || pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(WorkTags.Social)
                   || pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(WorkTags.Violent);
        }

        public bool HasIncapableSkills(Pawn pawn)
        {
            HarmonyPatches.DebugMessage("HasIncapableSkills called");
            var map = pawn.Map;
            //Check if we have level 0 skills
            var allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
            HarmonyPatches.DebugMessage("AllDefsForReading");
            foreach (var skillDef in allDefsListForReading)
            {
                var skill = TempExecutioner(map).skills.GetSkill(skillDef);
                if (skill.Level == 0)
                {
                    return true;
                }

                if (skill.TotallyDisabled)
                {
                    return true;
                }
            }

            return false;
        }

        public override bool CanSummonNow(Map map)
        {
            try
            {
                if (TempExecutioner(map) == null)
                {
                    Messages.Message("Executioner does not exist.", MessageTypeDefOf.RejectInput);
                    return false;
                }

                if (HasIncapableSkills(TempExecutioner(map)) || HasIncapableWorkTags(TempExecutioner(map)))
                {
                    return true;
                }

                Messages.Message("Executioner is already fully capable.", MessageTypeDefOf.RejectInput);
                return false;
            }
            catch (Exception e)
            {
                Utility.DebugReport(e.ToString());
            }

            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            HarmonyPatches.DebugMessage("Chaos Theory attempted");
            if (!(parms.target is Map map))
            {
                return false;
            }

            var pawn = map.GetComponent<MapComponent_SacrificeTracker>().lastUsedAltar.SacrificeData.Executioner;
            HarmonyPatches.DebugMessage("Executioner selected");
            if (HasIncapableWorkTags(pawn))
            {
                HarmonyPatches.DebugMessage($"{pawn.Label} has incapable worktags and must be remade.");
                HarmonyPatches.DebugMessage("Childhood redo");
                var fixedChildhood = false;
                _ = new List<WorkTypeDef>(pawn.story.childhood.DisabledWorkTypes);
                HarmonyPatches.DebugMessage("childwork list defined");
                while (fixedChildhood == false)
                {
                    IEnumerable<WorkTypeDef> childWorkList;
                    //200 tries to set to 0 disabled work types
                    for (var i = 0; i < 200; i++)
                    {
                        childWorkList = pawn.story.childhood.DisabledWorkTypes;
                        if (!childWorkList.Any())
                        {
                            goto FirstLeap;
                        }

                        pawn.story.childhood = BackstoryDatabase.RandomBackstory(BackstorySlot.Childhood);
                    }

                    //200 tries to set to 1 disabled work type
                    for (var i = 0; i < 200; i++)
                    {
                        childWorkList = pawn.story.childhood.DisabledWorkTypes;
                        if (childWorkList.Count() <= 1)
                        {
                            goto FirstLeap;
                        }

                        pawn.story.childhood = BackstoryDatabase.RandomBackstory(BackstorySlot.Childhood);
                    }

                    //Give up
                    fixedChildhood = true;
                }

                FirstLeap:

                HarmonyPatches.DebugMessage("First leap");
                //Your adulthood is out
                var fixedAdulthood = false;
                _ = pawn.story.adulthood.DisabledWorkTypes;
                while (fixedAdulthood == false)
                {
                    IEnumerable<WorkTypeDef> adultWorkList;
                    //Try 200 times to get to 0 disabled work types
                    for (var i = 0; i < 200; i++)
                    {
                        adultWorkList = pawn.story.adulthood.DisabledWorkTypes;
                        if (adultWorkList?.Count() == 0)
                        {
                            goto SecondLeap;
                        }

                        pawn.story.adulthood = BackstoryDatabase.RandomBackstory(BackstorySlot.Adulthood);
                    }

                    //Try 200 times to get to 1 disabled work types
                    for (var i = 0; i < 200; i++)
                    {
                        adultWorkList = pawn.story.adulthood.DisabledWorkTypes;
                        if (adultWorkList?.Count() <= 1)
                        {
                            goto SecondLeap;
                        }

                        pawn.story.adulthood = BackstoryDatabase.RandomBackstory(BackstorySlot.Adulthood);
                    }

                    //Give up
                    fixedAdulthood = true;
                }

                SecondLeap:
                HarmonyPatches.DebugMessage("Second leap");
            }

            if (HasIncapableSkills(pawn))
            {
                HarmonyPatches.DebugMessage($"{pawn.Label} has incapable skills");
                //pawn.story.GenerateSkillsFromBackstory();
                var allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;

                foreach (var skillDef in allDefsListForReading)
                {
                    var skill = pawn.skills.GetSkill(skillDef);
                    if (skill.Level <= 3)
                    {
                        skill.Level = 3;
                    }

                    if (skill.TotallyDisabled)
                    {
                        HarmonyPatches.DebugMessage($"{pawn.Label}'s {skill.def.LabelCap} is now 3");
                        skill.Level = 3;
                    }

                    skill.Notify_SkillDisablesChanged();
                }

                HarmonyPatches.DebugMessage("Skills assigned");
            }

            HarmonyPatches.DebugMessage("Disabled Work Types Attempted");
            Traverse.Create(pawn).Field("cachedDisabledWorkTypes").SetValue(null);
            HarmonyPatches.DebugMessage("Disabled Work Types Succeeded");
            //typeof(Pawn).GetField("cachedDisabledWorkTypes", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(pawn, null);
            map.GetComponent<MapComponent_SacrificeTracker>().lastLocation = pawn.Position;
            Messages.Message(pawn.Label + " has lived their entire life over again.", MessageTypeDefOf.PositiveEvent);
            return true;
        }
    }
}