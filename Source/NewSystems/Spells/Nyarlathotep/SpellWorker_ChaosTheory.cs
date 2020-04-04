// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

// ----------------------------------------------------------------------
// These are RimWorld-specific usings. Activate/Deactivate what you need:
// ----------------------------------------------------------------------
using UnityEngine;         // Always needed
//using VerseBase;         // Material/Graphics handling functions are found here
using Verse;               // RimWorld universal objects are here (like 'Building')
using Verse.AI;          // Needed when you do something with the AI
using Verse.AI.Group;
using Verse.Sound;       // Needed when you do something with Sound
using Verse.Noise;       // Needed when you do something with Noises
using RimWorld;            // RimWorld specific functions are found here (like 'Building_Battery')
using RimWorld.Planet;   // RimWorld specific functions for world creation
using System.Reflection;
using HarmonyLib;
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 


namespace CultOfCthulhu
{
    public class SpellWorker_ChaosTheory : SpellWorker
    {
        
        public bool HasIncapableWorkTags(Pawn pawn)
        {
            HarmonyPatches.DebugMessage("HasIncapableWorkTags called");
            if (pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag((WorkTags.Animals))) return true;
            if (pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag((WorkTags.Artistic))) return true;
            if (pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag((WorkTags.Caring))) return true;
            if (pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag((WorkTags.Cleaning))) return true;
            if (pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag((WorkTags.Cooking))) return true;
            if (pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag((WorkTags.Crafting))) return true;
            if (pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag((WorkTags.Firefighting))) return true;
            if (pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag((WorkTags.Hauling))) return true;
            if (pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag((WorkTags.Intellectual))) return true;
            if (pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag((WorkTags.ManualDumb))) return true;
            if (pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag((WorkTags.ManualSkilled))) return true;
            if (pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag((WorkTags.Mining))) return true;
            if (pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag((WorkTags.PlantWork))) return true;
            if (pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag((WorkTags.Social))) return true;
            if (pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag((WorkTags.Violent))) return true;
            return false;
        }

        public bool HasIncapableSkills(Pawn pawn)
        {
            HarmonyPatches.DebugMessage($"HasIncapableSkills called");
            Map map = pawn.Map;
            //Check if we have level 0 skills
            List<SkillDef> allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
            HarmonyPatches.DebugMessage($"AllDefsForReading");
            for (int i = 0; i < allDefsListForReading.Count; i++)
            {
                SkillDef skillDef = allDefsListForReading[i];
                SkillRecord skill = TempExecutioner(map).skills.GetSkill(skillDef);
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
                
                if (!HasIncapableSkills(TempExecutioner(map)) && !HasIncapableWorkTags(TempExecutioner(map)))
                {
                    Messages.Message("Executioner is already fully capable.", MessageTypeDefOf.RejectInput);
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                Cthulhu.Utility.DebugReport(e.ToString());
            }
            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            HarmonyPatches.DebugMessage("Chaos Theory attempted");
            Map map = parms.target as Map;
            Pawn pawn = map.GetComponent<MapComponent_SacrificeTracker>().lastUsedAltar.SacrificeData.Executioner;
            HarmonyPatches.DebugMessage("Executioner selected");
            if (HasIncapableWorkTags(pawn))
            {
                HarmonyPatches.DebugMessage($"{pawn.Label} has incapable worktags and must be remade.");
                HarmonyPatches.DebugMessage("Childhood redo");
                bool fixedChildhood = false;
                IEnumerable<WorkTypeDef> childWorkList = new List<WorkTypeDef>(pawn.story.childhood.DisabledWorkTypes);
                HarmonyPatches.DebugMessage($"childwork list defined");
                while (fixedChildhood == false)
                {
                    //200 tries to set to 0 disabled work types
                    for (int i = 0; i < 200; i++)
                    {
                        childWorkList = pawn.story.childhood.DisabledWorkTypes;
                        if (childWorkList.Count<WorkTypeDef>() == 0) { fixedChildhood = true; goto FirstLeap; }
                        pawn.story.childhood = BackstoryDatabase.RandomBackstory(BackstorySlot.Childhood);
                    }

                    //200 tries to set to 1 disabled work type
                    for (int i = 0; i < 200; i++)
                    {
                        childWorkList = pawn.story.childhood.DisabledWorkTypes;
                        if (childWorkList.Count<WorkTypeDef>() <= 1) { fixedChildhood = true; goto FirstLeap; }
                        pawn.story.childhood = BackstoryDatabase.RandomBackstory(BackstorySlot.Childhood);
                    }
                    //Give up
                    fixedChildhood = true;
                }
                FirstLeap:

                HarmonyPatches.DebugMessage($"First leap");
                //Your adulthood is out
                bool fixedAdulthood = false;
                IEnumerable<WorkTypeDef> adultWorkList = pawn.story.adulthood.DisabledWorkTypes;
                while (fixedAdulthood == false)
                {
                    //Try 200 times to get to 0 disabled work types
                    for (int i = 0; i < 200; i++)
                    {
                        adultWorkList = pawn.story.adulthood.DisabledWorkTypes;
                        if (adultWorkList?.Count<WorkTypeDef>() == 0) { fixedAdulthood = true; goto SecondLeap; }
                        pawn.story.adulthood = BackstoryDatabase.RandomBackstory(BackstorySlot.Adulthood);
                    }
                    //Try 200 times to get to 1 disabled work types
                    for (int i = 0; i < 200; i++)
                    {
                        adultWorkList = pawn.story.adulthood.DisabledWorkTypes;
                        if (adultWorkList?.Count<WorkTypeDef>() <= 1) { fixedAdulthood = true; goto SecondLeap; }
                        pawn.story.adulthood = BackstoryDatabase.RandomBackstory(BackstorySlot.Adulthood);
                    }
                    //Give up
                    fixedAdulthood = true;
                }
                SecondLeap:
                    HarmonyPatches.DebugMessage($"Second leap");
            }
            if (HasIncapableSkills(pawn))
            {
                HarmonyPatches.DebugMessage($"{pawn.Label} has incapable skills");
                //pawn.story.GenerateSkillsFromBackstory();
                List<SkillDef> allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;

                for (int i = 0; i < allDefsListForReading.Count; i++)
                {
                    SkillDef skillDef = allDefsListForReading[i];
                    SkillRecord skill = pawn.skills.GetSkill(skillDef);
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
                HarmonyPatches.DebugMessage($"Skills assigned");
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
