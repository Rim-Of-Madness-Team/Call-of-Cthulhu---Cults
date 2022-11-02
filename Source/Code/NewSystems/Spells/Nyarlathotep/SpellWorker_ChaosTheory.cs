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
            HarmonyPatches.DebugMessage(s: "HasIncapableWorkTags called");
            return pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(flag: WorkTags.Animals)
                   || pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(flag: WorkTags.Artistic)
                   || pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(flag: WorkTags.Caring)
                   || pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(flag: WorkTags.Cleaning)
                   || pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(flag: WorkTags.Cooking)
                   || pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(flag: WorkTags.Crafting)
                   || pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(flag: WorkTags.Firefighting)
                   || pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(flag: WorkTags.Hauling)
                   || pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(flag: WorkTags.Intellectual)
                   || pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(flag: WorkTags.ManualDumb)
                   || pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(flag: WorkTags.ManualSkilled)
                   || pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(flag: WorkTags.Mining)
                   || pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(flag: WorkTags.PlantWork)
                   || pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(flag: WorkTags.Social)
                   || pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(flag: WorkTags.Violent);
        }

        public void RemoveObstacleTraits(Pawn pawn)
        {
            var removeList = new List<TraitDef>();
            foreach (var trait in pawn.story.traits.allTraits)
            {
                if (trait.GetDisabledWorkTypes()?.FirstOrDefault() != null)
                {
                    removeList.Add(item: trait.def);
                }
            }
            foreach (var traitDef in removeList)
            {
                HarmonyPatches.DebugMessage(s: "Removed " + traitDef.label);
                pawn.story.traits.RemoveTrait(trait: pawn.story.traits.GetTrait(tDef: traitDef));
            }
        }

        public bool HasIncapableSkills(Pawn pawn)
        {
            HarmonyPatches.DebugMessage(s: "HasIncapableSkills called");
            var map = pawn.Map;
            //Check if we have level 0 skills
            var allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
            HarmonyPatches.DebugMessage(s: "AllDefsForReading");
            foreach (var skillDef in allDefsListForReading)
            {
                var skill = TempExecutioner(map: map).skills.GetSkill(skillDef: skillDef);
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
                if (TempExecutioner(map: map) == null)
                {
                    Messages.Message(text: "Executioner does not exist.", def: MessageTypeDefOf.RejectInput);
                    return false;
                }

                if (HasIncapableSkills(pawn: TempExecutioner(map: map)) || HasIncapableWorkTags(pawn: TempExecutioner(map: map)))
                {
                    return true;
                }

                Messages.Message(text: "Executioner is already fully capable.", def: MessageTypeDefOf.RejectInput);
                return false;
            }
            catch (Exception e)
            {
                Utility.DebugReport(x: e.ToString());
            }

            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            HarmonyPatches.DebugMessage(s: "Chaos Theory attempted");
            if (!(parms.target is Map map))
            {
                return false;
            }

            var pawn = map.GetComponent<MapComponent_SacrificeTracker>().lastUsedAltar.SacrificeData.Executioner;
            HarmonyPatches.DebugMessage(s: "Executioner selected");

            HarmonyPatches.DebugMessage(s: "Obstacle traits being removed:: ");
            RemoveObstacleTraits(pawn: pawn);

            if (HasIncapableWorkTags(pawn: pawn))
            {
                HarmonyPatches.DebugMessage(s: $"{pawn.Label} has incapable worktags and must be remade.");
                HarmonyPatches.DebugMessage(s: "Childhood redo");
                var fixedChildhood = false;
                _ = new List<WorkTypeDef>(collection: pawn.story.Childhood.DisabledWorkTypes);
                HarmonyPatches.DebugMessage(s: "childwork list defined");
                while (fixedChildhood == false)
                {
                    IEnumerable<WorkTypeDef> childWorkList;
                    //1000 tries to set to 0 disabled work types
                    for (var i = 0; i < 1000; i++)
                    {
                        childWorkList = pawn.story.Childhood.DisabledWorkTypes;
                        if (!childWorkList.Any())
                        {
                            goto FirstLeap;
                        }

                        pawn.story.Childhood = DefDatabase<BackstoryDef>.AllDefs.Where(x => x.slot == BackstorySlot.Childhood).RandomElement();
                    }

                    //1000 tries to set to 1 disabled work type
                    for (var i = 0; i < 1000; i++)
                    {
                        childWorkList = pawn.story.Childhood.DisabledWorkTypes;
                        if (childWorkList.Count() <= 1)
                        {
                            goto FirstLeap;
                        }

                        pawn.story.Childhood = DefDatabase<BackstoryDef>.AllDefs.Where(x => x.slot == BackstorySlot.Childhood).RandomElement();
                    }

                    //Give up
                    fixedChildhood = true;
                }

                FirstLeap:

                HarmonyPatches.DebugMessage(s: "First leap");
                //Your adulthood is out
                var fixedAdulthood = false;
                _ = pawn.story.Adulthood.DisabledWorkTypes;
                while (fixedAdulthood == false)
                {
                    IEnumerable<WorkTypeDef> adultWorkList;
                    //Try 1000 times to get to 0 disabled work types
                    for (var i = 0; i < 1000; i++)
                    {
                        adultWorkList = pawn.story.Adulthood.DisabledWorkTypes;
                        if (adultWorkList?.Count() == 0)
                        {
                            goto SecondLeap;
                        }

                        pawn.story.Adulthood = DefDatabase<BackstoryDef>.AllDefs.Where(x => x.slot == BackstorySlot.Adulthood).RandomElement();
                    }

                    //Try 1000 times to get to 1 disabled work types
                    for (var i = 0; i < 1000; i++)
                    {
                        adultWorkList = pawn.story.Adulthood.DisabledWorkTypes;
                        if (adultWorkList?.Count() <= 1)
                        {
                            goto SecondLeap;
                        }

                        pawn.story.Adulthood = DefDatabase<BackstoryDef>.AllDefs.Where(x => x.slot == BackstorySlot.Adulthood).RandomElement();
                    }

                    //Give up
                    fixedAdulthood = true;
                }

                SecondLeap:
                HarmonyPatches.DebugMessage(s: "Second leap");
            }

            if (HasIncapableSkills(pawn: pawn))
            {
                HarmonyPatches.DebugMessage(s: $"{pawn.Label} has incapable skills");
                //pawn.story.GenerateSkillsFromBackstory();
                var allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;

                foreach (var skillDef in allDefsListForReading)
                {
                    var skill = pawn.skills.GetSkill(skillDef: skillDef);
                    if (skill.Level <= 3)
                    {
                        skill.Level = 3;
                    }

                    if (skill.TotallyDisabled)
                    {
                        HarmonyPatches.DebugMessage(s: $"{pawn.Label}'s {skill.def.LabelCap} is now 3");
                        skill.Level = 3;
                    }

                    skill.Notify_SkillDisablesChanged();
                }

                HarmonyPatches.DebugMessage(s: "Skills assigned");
            }

            HarmonyPatches.DebugMessage(s: "Disabled Work Types Attempted");
            Traverse.Create(root: pawn).Field(name: "cachedDisabledWorkTypes").SetValue(value: null);
            HarmonyPatches.DebugMessage(s: "Disabled Work Types Succeeded");
            //typeof(Pawn).GetField("cachedDisabledWorkTypes", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(pawn, null);
            map.GetComponent<MapComponent_SacrificeTracker>().lastLocation = pawn.Position;
            Messages.Message(text: pawn.Label + " has lived their entire life over again.", def: MessageTypeDefOf.PositiveEvent);
            return true;
        }
    }
}