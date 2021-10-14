// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Cthulhu;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

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
    public static class CultLevel
    {
        public const float PureAntiCultist = 0.1f;
        public const float AntiCultist = 0.3f;
        public const float Middling = 0.5f;
        public const float Cultist = 0.7f;
        public const float PureCultist = 0.9f;
    }

    public static class CultUtility
    {
        //TraitDef.Named("Masochist")
        //TraitDef.Named("PsychicSensitivity"),
        //TraitDef.Named("Nerves"),
        //TraitDefOf.DrugDesire
        public enum CultistType
        {
            None,
            Preacher,
            DarkEmmisary
        }

        public enum OfferingSize
        {
            none = 0,
            meagre = 5,
            decent = 10,
            sizable = 20,
            worthy = 50,
            impressive = 100
        }

        public enum SacrificeResult
        {
            none = 0,
            criticalfailure,
            failure,
            mixedsuccess,
            success
        }

        public enum SacrificeType
        {
            none,
            meat,
            plants,
            meals,
            animal,
            human
        }


        public static List<TraitDef> immoralistTraits = new List<TraitDef>
        {
            TraitDefOf.Psychopath,
            TraitDefOf.Bloodlust,
            TraitDefOf.Cannibal
        };

        public static readonly int ritualDuration = 740; // 15 seconds max

        public static readonly int reflectDuration = 600; // 10 seconds max

        // RimWorld.IncidentWorker_ShipChunkDrop
        public static bool TryFindDropCell(IntVec3 nearLoc, Map map, int maxDist, out IntVec3 pos,
            ThingDef defToCheck = null)
        {
            if (defToCheck == null)
            {
                defToCheck = ThingDefOf.ShipChunkIncoming;
            }

            return CellFinderLoose.TryFindSkyfallerCell(defToCheck, map, out pos, 10, nearLoc, maxDist, true, false,
                false, false, false);
        }

        public static List<Pawn> GetCultMindedAffectablePawns(Map map)
        {
            var spawnedColonyMembers = new List<Pawn>(map.mapPawns.FreeColonistsSpawned);
            var spawnedPrisonersAndSlaves = new List<Pawn>(map.mapPawns.SlavesAndPrisonersOfColonySpawned);
            spawnedColonyMembers.AddRange(spawnedPrisonersAndSlaves);
            return spawnedColonyMembers;
        }

        public static float GetBaseCultistModifier(Pawn pawn)
        {
            float result = 0;
            var bigMod = Rand.Range(0.2f, 0.25f);
            var smallMod = Rand.Range(0.05f, 0.1f);
            if (pawn?.story?.adulthood == null ||
                pawn.story?.childhood == null)
            {
                return result;
            }

            string adultStory = pawn.story.adulthood.FullDescriptionFor(pawn);
            string childStory = pawn.story.childhood.FullDescriptionFor(pawn);

            //Immoralist modifiers

            //Immoral Traits:
            //         I do eat human flesh.
            //         I like to kill.
            //         I don't care about others.
            result = (from trait in pawn.story.traits.allTraits
                from def in immoralistTraits
                where trait.def == def
                select bigMod).Sum();
            //Cult inclined.
            //          Midworlders are more open to superstition.
            //          Abandoned children, looking for 'family.'

            if (adultStory.Contains("midworld") || adultStory.Contains("Midworld"))
            {
                result += smallMod;
            }

            if (childStory.Contains("midworld") || childStory.Contains("Midworld"))
            {
                result += smallMod;
            }

            if (childStory.Contains("abandoned"))
            {
                result += smallMod;
            }

            //Moralist modifiers

            //Moral: I am not a violent person.
            if (pawn.story.adulthood.workDisables == WorkTags.Violent ||
                pawn.story.childhood.workDisables == WorkTags.Violent)
            {
                result -= bigMod;
            }

            //Cult disinclined.
            //          Glitterworlders. Morality is paramount.
            if (adultStory.Contains("glitterworld") || adultStory.Contains("Glitterworld"))
            {
                result -= smallMod;
            }

            if (childStory.Contains("glitterworld") || childStory.Contains("Glitterworld"))
            {
                result -= smallMod;
            }

            //Randomness
            //          Evangelists can be cultist or moralists.
            if (pawn.story.adulthood.title.Contains("Evangelist"))
            {
                if (Rand.Range(0, 100) > 50)
                {
                    result += bigMod;
                }

                result -= bigMod;
            }

            if (Rand.Range(0, 100) > 50)
            {
                result += smallMod;
            }
            else
            {
                result -= smallMod;
            }

            return Mathf.Clamp(result, -0.5f, 0.2f);
        }

        public static bool TrySpawnWalkInCultist(Map map, CultistType type = CultistType.None, bool showMessage = true)
        {
            try
            {
                if (map == null)
                {
                    map = Find.CurrentMap;
                }

                if (map == null)
                {
                    return false;
                }

                if (!CellFinder.TryFindRandomEdgeCellWith(c => map.reachability.CanReachColony(c), map,
                    CellFinder.EdgeRoadChance_Neutral, out var loc))
                {
                    return false;
                }

                if (!TryGenerateCultist(out var p, map, type))
                {
                    //Log.Messag("Unable to generate cultist");
                    return false;
                }

                if (p == null)
                {
                    //Log.Messag("Pawn is null");
                    return false;
                }

                GenSpawn.Spawn(p, loc, map);
                var text = "CultistJoin".Translate(p.kindDef.label, p.story.adulthood.title.ToLower());
                text = text.AdjustedFor(p);
                var label = "LetterLabelCultistJoin".Translate();
                if (showMessage)
                {
                    Find.LetterStack.ReceiveLetter(label, text, CultsDefOf.Cults_StandardMessage);
                }

                PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref label, p);
                return true;
            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
            {
                return false;
            }
        }

        public static bool TryGenerateCultist(out Pawn cultist, Map map, CultistType type = CultistType.None)
        {
            var pawnKindDef = new List<PawnKindDef>
            {
                PawnKindDefOf.Villager
            }.RandomElement();


            Pawn p = null;
            PawnGenerationRequest request;

            //Resolve the type of cultist.
            //If it's a preacher, we need a high speaking skill.
            if (type == CultistType.Preacher)
            {
                for (var i = 0; i < 999; i++)
                {
                    request = new PawnGenerationRequest(pawnKindDef, Faction.OfPlayer, PawnGenerationContext.NonPlayer,
                        map.Tile, false, false, false, false, true, true, 20f, false, true, true, false);
                    p = PawnGenerator.GeneratePawn(request);

                    if (p.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
                    {
                        continue;
                    }

                    if (p.skills.GetSkill(SkillDefOf.Social).Level < 5)
                    {
                        continue;
                    }

                    if (p.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(WorkTags.Social))
                    {
                        continue;
                    }

                    break;
                }
            }
            //If it's a dark emissary of Nyarlathotep, we need to add clothing.
            else if (type == CultistType.DarkEmmisary)
            {
                request = new PawnGenerationRequest(pawnKindDef, Faction.OfPlayer, PawnGenerationContext.NonPlayer,
                    map.Tile, false, false, false, false, true, true, 20f, false, true, true, false);
                p = PawnGenerator.GeneratePawn(request);
                var tHood = ThingMaker.MakeThing(ThingDef.Named("Apparel_NyarlathotepHood"),
                    ThingDef.Named("DevilstrandCloth"));
                var tRobe = ThingMaker.MakeThing(ThingDef.Named("Apparel_CultistRobes"),
                    ThingDef.Named("DevilstrandCloth"));
                var Hood = tHood as Apparel;
                var Robe = tRobe as Apparel;
                p.apparel.Wear(Hood, false);
                p.apparel.Wear(Robe, false);
            }
            else
            {
                request = new PawnGenerationRequest(pawnKindDef, Faction.OfPlayer, PawnGenerationContext.NonPlayer,
                    map.Tile, false, false, false, false, true, true, 20f, false, true, true, false);
                p = PawnGenerator.GeneratePawn(request);
            }

            //We need psychopathic cannibals
            //GenSpawn.Spawn(p, loc);
            if (p == null)
            {
                cultist = null;
                return false;
            }

            //Add cultist traits.
            var traitToAdd = TraitDefOf.Psychopath;
            if (!p.story.traits.HasTrait(TraitDefOf.Cannibal))
            {
                traitToAdd = TraitDefOf.Cannibal;
            }

            if (!p.story.traits.HasTrait(TraitDefOf.Psychopath))
            {
                traitToAdd = TraitDefOf.Psychopath;
            }

            if (p.story.traits.allTraits.Count < 3)
            {
                p.story.traits.GainTrait(new Trait(traitToAdd));
            }
            else
            {
                foreach (var t in p.story.traits.allTraits)
                {
                    if (t.def == TraitDefOf.Cannibal || t.def == TraitDefOf.Psychopath)
                    {
                        continue;
                    }

                    p.story.traits.allTraits.Remove(t);
                    break; //Remove 1 trait and get out
                }

                p.story.traits.GainTrait(new Trait(traitToAdd));
            }

            //Add cult-mindedness.
            AffectCultMindedness(p, 0.8f);

            cultist = p;
            return true;
        }

        public static SacrificeResult GetSacrificeResult(Map map)
        {
            //Temporary
            //return SacrificeResult.success;
            var s = new StringBuilder();
            s.AppendLine("Sacrifice Success Calculation");

            var Success = false;
            var TableOfFun = false;

            var diceRoll = Rand.Range(1, 100);
            var baseDifficulty = 40;
            var failDifficulty = 0;

            var altar = map.GetComponent<MapComponent_SacrificeTracker>().lastUsedAltar;
            if (altar?.SacrificeData?.Entity != null)
            {
                if (altar.SacrificeData.Spell != null)
                {
                    //Setup a Report
                    ///////////////////
                    altar.LastReport = "";
                    var reportHeader = new StringBuilder();
                    var reportFavorables = new StringBuilder();
                    var reportUnfavorables = new StringBuilder();
                    var reportResult = new StringBuilder();

                    //Start the Header
                    ///////////////////
                    var date = GenDate.DateFullStringAt(GenDate.TickGameToAbs(Find.TickManager.TicksGame),
                        Find.WorldGrid.LongLatOf(map.Tile));
                    reportHeader.AppendLine("Cults_LRHeading".Translate(date));
                    reportHeader.AppendLine("Cults_LRSacrifice".Translate(altar.SacrificeData.Sacrifice.def.label,
                        altar.SacrificeData.Entity.LabelCap, altar.SacrificeData.Spell.label));
                    reportHeader.AppendLine(
                        "Cults_LRAttendance".Translate(altar.SacrificeData?.Congregation?.Count ?? 0));
                    reportFavorables.AppendLine("Cults_LRFactorsFavorable".Translate());
                    reportUnfavorables.AppendLine("Cults_LRFactorsUnfavorable".Translate());
                    ///////////////////

                    if (altar.SacrificeData.Spell.defName != "Cults_SpellFavor")
                    {
                        s.AppendLine("Initial Failure Difficulty: " + baseDifficulty);

                        //Difficulty modifiers
                        failDifficulty +=
                            SpellCalc_TierDifficulty(altar, s, reportUnfavorables,
                                reportFavorables); //Tier 2 +10, Tier 3 + 20, Final +50
                        failDifficulty += SpellCalc_GameConditions(altar, s, reportUnfavorables, reportFavorables,
                            out var successModifier); //+50 stars are wrong / -20 stars are right
                        failDifficulty += SpellCalc_Characters(altar, s, reportUnfavorables, reportFavorables,
                            successModifier, out successModifier); //+50 stars are wrong / -20 stars are right
                        s.AppendLine("Adjusted Failure Difficulty: " + baseDifficulty + failDifficulty);

                        //Success modifiers
                        successModifier +=
                            SpellCalc_CongregationQuality(altar, s,
                                reportFavorables); //Some robes +10, Dagger equipped +5, All robed and hooded +15
                        successModifier +=
                            SpellCalc_StatuesNearby(altar, s,
                                reportFavorables); //Minimum one statue of normal quality +10, Statue of deity +10
                        successModifier +=
                            SpellCalc_TempleQuality(altar, s,
                                reportFavorables); //Some quality +10, Great quality +20, Outdoors when deity favors it +20
                        s.AppendLine("Success Modifier: " + successModifier);

                        //Difficulty check
                        s.AppendLine("Difficulty check: (Rolling d100. " + diceRoll +
                                     " result + Success Modifier: " + successModifier +
                                     ") vs (Difficulty: " + baseDifficulty + " + Modifier: " + failDifficulty +
                                     ")");
                        if (diceRoll + successModifier >= baseDifficulty + failDifficulty)
                        {
                            Success = true;
                        }

                        s.AppendLine("Success = " + Success.ToString().CapitalizeFirst());
                        reportResult.AppendLine("Cults_LRCheck".Translate(
                            diceRoll, successModifier, baseDifficulty, failDifficulty));
                        reportResult.AppendLine(Success
                            ? "Cults_LRResultSuccess".Translate()
                            : "Cults_LRResultFailure".Translate());

                        //Table of fun
                        var randFun = Rand.Range(1, 10);
                        if (randFun >= 6)
                        {
                            TableOfFun = true; //40% chance
                        }

                        s.AppendLine("Side Effect = " + TableOfFun);

                        altar.LastReport = reportHeader + "\n" +
                                           reportFavorables + "\n" +
                                           reportUnfavorables + "\n" +
                                           reportResult;

                        Utility.DebugReport(s.ToString());
                        if (Success && TableOfFun)
                        {
                            return SacrificeResult.mixedsuccess;
                        }

                        if (!Success && TableOfFun)
                        {
                            return SacrificeResult.failure;
                        }

                        if (Success)
                        {
                            return SacrificeResult.success;
                        }
                    }
                    else if (altar.SacrificeData.Spell.defName == "Cults_SpellFavor")
                    {
                        return SacrificeResult.success;
                    }
                }
            }

            Utility.DebugReport(s.ToString());
            return SacrificeResult.criticalfailure;
        }

        private static int SpellCalc_Characters(Building_SacrificialAltar altar, StringBuilder debugString,
            StringBuilder reportUnfavorables, StringBuilder reportFavorables, int successModifierIn,
            out int successModifierOut)
        {
            var modifier = 0;
            successModifierOut = successModifierIn;
            var executioner = altar.tempExecutioner;
            var worldComponentGlobalCultTracker = Find.World.GetComponent<WorldComponent_GlobalCultTracker>();
            var experience = worldComponentGlobalCultTracker.GetExperience(executioner, true);
            if (experience == 0)
            {
                modifier += 10;
                debugString.AppendLine("Executioner Difficulty: +10 First time");
                reportUnfavorables.AppendLine("+10 / 10: " + "Cults_LRExecFirstTime".Translate(executioner.LabelShort));
            }
            else if (experience < 3)
            {
                modifier += 5;
                debugString.AppendLine("Executioner Difficulty: +10 Inexperienced executioner");
                reportUnfavorables.AppendLine("+ 5 / 10: " +
                                              "Cults_LRExecInex".Translate(
                                                  executioner.LabelShort, experience));
            }
            else if (experience < 10)
            {
                debugString.AppendLine("Executioner Success: +0 Somewhat-lacking executioner");
                reportFavorables.AppendLine("+ 0 / 10: " +
                                            "Cults_LRExecEven".Translate(
                                                executioner.LabelShort, experience));
            }
            else if (experience < 24)
            {
                successModifierOut += 5;
                debugString.AppendLine("Executioner Success: +5 Experienced executioner");
                reportFavorables.AppendLine("+ 5 / 10: " +
                                            "Cults_LRExecExp".Translate(
                                                executioner.LabelShort, experience));
            }
            else
            {
                successModifierOut += 10;
                debugString.AppendLine("Executioner Success: +10 Expert executioner");
                reportFavorables.AppendLine("+10 / 10: " +
                                            "Cults_LRExecVeryExp".Translate(executioner.LabelShort, experience));
                var executionerBonus = new IntRange(0, 5).RandomInRange;
                successModifierOut += executionerBonus;
                debugString.AppendLine("Executioner Success: +" + executionerBonus + " Executioner finesse bonus");
                reportFavorables.AppendLine("+" + executionerBonus + " /   5: " +
                                            "Cults_LRExecBonus".Translate(executioner.LabelShort));
            }

            worldComponentGlobalCultTracker.GainExperience(executioner, true);
            return modifier;
        }

        private static int SpellCalc_GameConditions(Building_SacrificialAltar altar, StringBuilder s,
            StringBuilder reportUnfavorables, StringBuilder reportFavorables, out int successModifier)
        {
            var modifier = 0;
            successModifier = 0;
            var starsAreRight = altar.Map.GameConditionManager.GetActiveCondition<GameCondition_StarsAreRight>();
            var starsAreWrong = altar.Map.GameConditionManager.GetActiveCondition<GameCondition_StarsAreWrong>();
            var eclipseActive = altar.Map.GameConditionManager.GetActiveCondition(GameConditionDefOf.Eclipse);
            var auroraActive = altar.Map.GameConditionManager.GetActiveCondition(GameConditionDefOf.Aurora);

            //Astral events
            /////////////
            if (starsAreRight != null)
            {
                s.AppendLine("Map Condition Difficulty: +20 Stars Are Right");
                successModifier += 20;
                reportFavorables.AppendLine("+20 / 20: " +
                                            "Cults_LRStarsAreRight".Translate(altar.SacrificeData.Entity.LabelCap));
            }
            else if (starsAreWrong != null)
            {
                s.AppendLine("Map Condition Difficulty: +50 Stars Are Wrong");
                modifier += 50;
                reportUnfavorables.AppendLine("+50 / 50: " +
                                              "Cults_LRStarsAreWrong".Translate(altar.SacrificeData.Entity.LabelCap));
            }
            else
            {
                s.AppendLine("Map Condition Difficulty: +0 No Astral Event");
                reportUnfavorables.AppendLine("+  0 / 50: " + "Cults_LRNoAstralEvents".Translate());
            }

            //Eclipse
            ///////////
            if (eclipseActive != null)
            {
                s.AppendLine("Map Condition Difficulty: +5 Eclipse Active");
                successModifier += 5;
                reportFavorables.AppendLine("+ 5 /   5: " +
                                            "Cults_LREclipseActive".Translate());
            }
            else
            {
                s.AppendLine("Map Condition Difficulty: +0 No Eclipse Active");
                reportFavorables.AppendLine("+ 0 /   5: " +
                                            "Cults_LRNoEclipseActive".Translate());
            }

            //Aurora
            ///////////
            if (auroraActive != null)
            {
                s.AppendLine("Map Condition Difficulty: +5 Aurora Active");
                successModifier += 5;
                reportFavorables.AppendLine("+ 5 /   5: " +
                                            "Cults_LRAuroraActive".Translate());
            }
            else
            {
                s.AppendLine("Map Condition Difficulty: +0 No Aurora Active");
                reportFavorables.AppendLine("+ 0 /   5: " +
                                            "Cults_LRNoAuroraActive".Translate());
            }

            return modifier;
        }

        // RimWorld.BaseGen.SymbolResolver_Doors
        private static bool IsOutdoorsAt(Map map, IntVec3 c)
        {
            return c.GetRegion(map) != null && c.GetRegion(map).Room.PsychologicallyOutdoors;
        }

        private static int SpellCalc_TempleQuality(Building_SacrificialAltar altar, StringBuilder s,
            StringBuilder reportFavorables)
        {
            var modifier = 0;
            var deity = altar.SacrificeData.Entity;
            if (!IsOutdoorsAt(altar.Map, altar.Position))
            {
                var temple = altar.GetRoom();
                if (temple == null)
                {
                    return modifier;
                }

                var impressiveScore = temple.GetStat(RoomStatDefOf.Impressiveness);
                var wealthScore = temple.GetStat(RoomStatDefOf.Wealth);
                var spaceScore = temple.GetStat(RoomStatDefOf.Space);
                var beautyScore = temple.GetStat(RoomStatDefOf.Beauty);

                //Expected quality. 13x13 tiles. Pews. Altar. 2 objects of Lighting.

                //WEALTH
                //////////////
                if (wealthScore < 2000)
                {
                    s.AppendLine("Temple Wealth Bonus: +0 - Not good");
                    reportFavorables.AppendLine("+ 0 /   5: " + "Cults_LRTempWealthNo".Translate());
                }

                if (wealthScore < 4000)
                {
                    modifier += 3;
                    s.AppendLine("Temple Wealth Bonus: +3 - Good");
                    reportFavorables.AppendLine("+ 3 /   5: " + "Cults_LRTempWealthDecent".Translate());
                }
                else if (wealthScore >= 4000)
                {
                    modifier += 5;
                    s.AppendLine("Temple Wealth Bonus: +5 - Great");
                    reportFavorables.AppendLine("+ 5 /   5: " + "Cults_LRTempWealth".Translate());
                }

                //SPACE
                //////////////////
                if (spaceScore < 160)
                {
                    s.AppendLine("Temple Space Bonus: +0 - Not good");
                    reportFavorables.AppendLine("+ 0 /   5: " + "Cults_LRTempSpaceNo".Translate());
                }
                else if (spaceScore < 400)
                {
                    modifier += 3;
                    s.AppendLine("Temple Space Bonus: +3 - Good");
                    reportFavorables.AppendLine("+ 3 /   5: " + "Cults_LRTempSpaceDecent".Translate());
                }
                else if (spaceScore >= 400)
                {
                    modifier += 5;
                    s.AppendLine("Temple Space Bonus: +5 - Great");
                    reportFavorables.AppendLine("+ 5 /   5: " + "Cults_LRTempSpace".Translate());
                }

                //BEAUTY
                ////////////////

                if (beautyScore < 1.59)
                {
                    s.AppendLine("Temple Beauty Bonus: +0 - Not good");
                    reportFavorables.AppendLine("+ 0 /   5: " + "Cults_LRTempBeautyNo".Translate());
                }
                else if (beautyScore <= 2.0)
                {
                    modifier += 3;
                    s.AppendLine("Temple Beauty Bonus: +3 - Good");
                    reportFavorables.AppendLine("+ 3 /   5: " + "Cults_LRTempBeautyDecent".Translate());
                }
                else if (beautyScore >= 2.0)
                {
                    modifier += 5;
                    s.AppendLine("Temple Beauty Bonus: +5 - Great");
                    reportFavorables.AppendLine("+ 5 /   5: " + "Cults_LRTempBeauty".Translate());
                }

                //IMPRESSIVENESS
                ////////////////
                if (impressiveScore < 80)
                {
                    s.AppendLine("Temple Quality Bonus: +0 - Not good");
                    reportFavorables.AppendLine("+ 0 /   5: " + "Cults_LRTempImpressNo".Translate());
                }
                else if (impressiveScore < 150)
                {
                    modifier += 3;
                    s.AppendLine("Temple Quality Bonus: + 3 - Good");
                    reportFavorables.AppendLine("+ 3 /   5: " + "Cults_LRTempImpressDecent".Translate());
                }
                else if (impressiveScore > 150)
                {
                    modifier += 5;
                    s.AppendLine("Temple Quality Bonus: +5 - Great");
                    reportFavorables.AppendLine("+ 5 /   5: " + "Cults_LRTempImpress".Translate());
                }
            }
            else
            {
                if (deity.FavorsOutdoorWorship)
                {
                    modifier += 20;
                    s.AppendLine("Temple Quality Bonus: +20 Outside Deity Favor");
                    reportFavorables.AppendLine("+20 / 20: " + "Cults_LRTempOutdoorFavored".Translate(deity.LabelCap));
                }
                else
                {
                    s.AppendLine("Temple Quality Bonus: +0 - No Outside Deity Favor");
                    reportFavorables.AppendLine("+ 0 / 20: " +
                                                "Cults_LRTempNoOutdoorFavored".Translate(deity.LabelCap));
                }
            }

            return modifier;
        }

        private static int SpellCalc_StatuesNearby(Building_SacrificialAltar altar, StringBuilder s,
            StringBuilder reportFavorables)
        {
            var modifier = 0;
            var statueOfDeityExists = false;
            var qualityExists = false;
            var temple = altar.GetRoom();
            var deity = altar.SacrificeData.Entity;

            var sculptures = temple?.ContainedAndAdjacentThings.FindAll(x => x is ThingWithComps y &&
                                                                             y.TryGetComp<CompFavoredObject>() != null);
            if (sculptures != null && sculptures.Count > 0)
            {
                foreach (var sculpture in sculptures)
                {
                    var compFavoredObject = sculpture.TryGetComp<CompFavoredObject>();
                    if (compFavoredObject?.Deities.FirstOrDefault(y => y.deityDef == deity.def.defName) != null)
                    {
                        statueOfDeityExists = true;
                    }

                    if (!sculpture.TryGetQuality(out var qc))
                    {
                        continue;
                    }

                    if (qc >= QualityCategory.Normal)
                    {
                        qualityExists = true;
                    }
                }
            }

            if (statueOfDeityExists)
            {
                modifier += 10;
                s.AppendLine("Deity Statue Bonus: Sacrifice modifier + 10");
                reportFavorables.AppendLine("+10 / 10: " + "Cults_LRDeityStatue".Translate(deity.LabelCap));
            }
            else
            {
                s.AppendLine("No Deity Statue Bonus: Sacrifice modifier + 0");
                reportFavorables.AppendLine("+ 0 / 10: " + "Cults_LRNoDeityStatue".Translate(deity.LabelCap));
            }

            if (qualityExists)
            {
                modifier += 10;
                s.AppendLine("Quality Statue Bonus: Sacrifice modifier + 10");
                reportFavorables.AppendLine("+10 / 10: " + "Cults_LRQualityStatue".Translate());
            }
            else
            {
                s.AppendLine("No Quality Statue Bonus: Sacrifice modifier + 0");
                reportFavorables.AppendLine("+ 0 / 10: " + "Cults_LRNoQualityStatue".Translate());
            }

            return modifier;
        }

        public static int SpellCalc_CongregationQuality(Building_SacrificialAltar altar, StringBuilder debugLog,
            StringBuilder reportFavorables)
        {
            var modifier = 0;

            if (altar?.SacrificeData?.Congregation == null)
            {
                return modifier;
            }

            var deity = altar.SacrificeData.Entity;
            _ = altar.SacrificeData.Spell;
            var value = CongregationBonus(altar.SacrificeData.Congregation, deity, out var perfect,
                out var sacrificialDagger, debugLog);
            if (value > 0)
            {
                modifier += 10;
                reportFavorables.AppendLine("+10 / 10: " + "Cults_LRAttireBonus".Translate());
                debugLog.AppendLine("Attire Bonus: Sacrifice modifier + 10");
            }
            else
            {
                reportFavorables.AppendLine("+ 0 / 10: " + "Cults_LRNoAttireBonus".Translate());
                debugLog.AppendLine("No Attire Bonus: Sacrifice modifier + 0");
            }

            if (sacrificialDagger)
            {
                modifier += 5;
                reportFavorables.AppendLine("+ 5 /  5: " + "Cults_LRDaggerBonus".Translate());
                debugLog.AppendLine("Dagger Bonus: Sacrifice modifier + 5");
            }
            else
            {
                reportFavorables.AppendLine("+ 0 /   5: " + "Cults_LRNoDaggerBonus".Translate());
                debugLog.AppendLine("No Dagger Bonus: Sacrifice modifier + 0");
            }

            if (perfect)
            {
                modifier += 15;
                reportFavorables.AppendLine("+15 / 15: " + "Cults_LRPerfectBonus".Translate());
                debugLog.AppendLine("Perfect Attire Bonus: Sacrifice modifier + 15");
            }
            else
            {
                reportFavorables.AppendLine("+ 0 / 15: " + "Cults_LRNoPerfectBonus".Translate());
                debugLog.AppendLine("No Perfect Attire Bonus: Sacrifice modifier + 0");
            }

            return modifier;
        }

        public static int SpellCalc_TierDifficulty(Building_SacrificialAltar altar, StringBuilder debugLog,
            StringBuilder reportUnfavorables, StringBuilder reportFavorables)
        {
            var modifier = 0;

            if (altar?.SacrificeData?.Congregation != null
            ) //Map.GetComponent<MapComponent_SacrificeTracker>().lastSacrificeCongregation != null)
            {
                var deity = altar.SacrificeData.Entity; // currentSacrificeDeity;
                var spell = altar.SacrificeData.Spell; // currentSpell;

                //Is tier 1?
                foreach (var current in deity.tier1Spells)
                {
                    if (current != spell)
                    {
                        continue;
                    }

                    debugLog.AppendLine(current.defName + " is a tier 1 spell. No difficulty modifier added.");
                    reportFavorables.AppendLine("+ 0 / 50: " + "Cults_LRSpellDifficultyOne".Translate());
                    goto GoToTheEnd;
                }

                //Is tier 2? +10% difficulty
                foreach (var current in deity.tier2Spells)
                {
                    if (current != spell)
                    {
                        continue;
                    }

                    debugLog.AppendLine(current.defName + " is a tier 2 spell. +10 sacrifice failure rate.");
                    modifier = 10;
                    reportUnfavorables.AppendLine("+10 / 50: " + "Cults_LRSpellDifficultyTwo".Translate());
                    goto GoToTheEnd;
                }

                //Is tier 3? +20% difficulty
                foreach (var current in deity.tier3Spells)
                {
                    if (current != spell)
                    {
                        continue;
                    }

                    debugLog.AppendLine(current.defName + " is a tier 3 spell. +20% sacrifice failure rate.");
                    modifier = 20;
                    reportUnfavorables.AppendLine("+20 / 50: " + "Cults_LRSpellDifficultyThree".Translate());
                    goto GoToTheEnd;
                }

                //Is final spell? +50% difficulty
                if (spell == deity.finalSpell)
                {
                    debugLog.AppendLine(spell.defName + " is a final spell. +50% sacrifice failure rate.");
                    modifier = 50;
                    reportUnfavorables.AppendLine("+50 / 50: " + "Cults_LRSpellDifficultyFour".Translate());
                }

                //Nothing
            }

            GoToTheEnd:
            return modifier;
        }

        public static float CongregationBonus(List<Pawn> congregationIn, CosmicEntity entity, out bool perfect,
            out bool sacrificialDagger, StringBuilder s2 = null)
        {
            var s = new StringBuilder();
            if (s2 != null)
            {
                s = s2;
            }

            s.AppendLine("Congregation Bonus Report");
            s.AppendLine("=========================");
            s.AppendLine();
            var congregation = new List<Pawn>(congregationIn);
            perfect = false;
            sacrificialDagger = false;
            var result = 0f;
            var count = 0;

            if (congregation.Count == 0)
            {
                return result;
            }

            if (entity == null)
            {
                return result;
            }

            //Are they wearing the right outfits?
            foreach (var member in congregation)
            {
                var wearingHood = false;
                var wearingRobes = false;
                if (member == null)
                {
                    count++;
                    continue;
                }

                if (member.Dead)
                {
                    count++;
                    continue;
                }

                if (!member.IsColonist && !member.IsSlaveOfColony)
                {
                    count++;
                    continue;
                }

                if (member.apparel?.WornApparel == null)
                {
                    continue;
                }

                if (member.apparel.WornApparelCount == 0)
                {
                    continue;
                }

                if (member.equipment == null)
                {
                    continue;
                }

                foreach (var clothing in member.apparel.WornApparel)
                {
                    var favoredObject = clothing.GetComp<CompFavoredObject>();
                    if (favoredObject != null)
                    {
                        var deities = favoredObject.Deities;
                        if (deities == null || deities.Count <= 0)
                        {
                            continue;
                        }

                        var entry = deities.FirstOrDefault(x => x.deityDef == entity.def.defName);
                        if (entry == null)
                        {
                            continue;
                        }

                        result += entry.favorBonus;
                        if (entry.favorBonus != 0)
                        {
                            s.AppendLine(member.Label + " is wearing " + clothing.Label +
                                         " that gives a bonus of " + entry.favorBonus + " for " + entity.Label);
                        }

                        if (!wearingRobes && clothing.def.apparel.layers.Contains(ApparelLayerDefOf.Shell))
                        {
                            wearingRobes = true;
                        }

                        if (!wearingHood && clothing.def.apparel.layers.Contains(ApparelLayerDefOf.Overhead))
                        {
                            wearingHood = true;
                        }
                    }
                    else
                    {
                        if (clothing.def.defName == "Apparel_CultistRobes")
                        {
                            wearingRobes = true;
                            result += 0.005f;
                        }

                        if (clothing.def.defName == "Apparel_CultistHood" ||
                            clothing.def.defName == "Apparel_StandardHood" ||
                            clothing.def.defName == "Apparel_CthulhuMaskHood" ||
                            clothing.def.defName == "Apparel_NyarlathotepHood" ||
                            clothing.def.defName == "Apparel_DagonMitre" ||
                            clothing.def.defName == "Apparel_ShubMask")
                        {
                            wearingHood = true;
                            result += 0.005f;
                        }

                        if (entity.favoredApparel == null)
                        {
                            continue;
                        }

                        if (entity.favoredApparel.Count == 0)
                        {
                            continue;
                        }

                        foreach (var def in entity.favoredApparel)
                        {
                            if (def == null)
                            {
                                continue;
                            }

                            if (clothing.def == def)
                            {
                                result += 0.025f;
                            }
                        }
                    }
                }

                if (member.equipment.AllEquipmentListForReading != null &&
                    member.equipment.AllEquipmentListForReading.Count > 0)
                {
                    foreach (var eq in member.equipment.AllEquipmentListForReading)
                    {
                        if (eq.def.defName != "MeleeWeapon_CultKris")
                        {
                            continue;
                        }

                        sacrificialDagger = true;
                        result += 0.005f;
                        s.AppendLine(member.LabelShort + " is wielding a sacrificial dagger.");
                    }
                }

                if (wearingHood && wearingRobes)
                {
                    count++;
                    s.Append(member.LabelShort + " is perfectly attired for the congregation.");
                    s.AppendLine();
                }
                else
                {
                    s.Append(member.LabelShort + " is not perfectly attired for the congregation.");
                    s.AppendLine();
                }
            }

            if (result == 0)
            {
                RemindPlayerAboutCongregationBonuses();
            }

            if (count >= congregation.Count)
            {
                perfect = true;
                s.Append("Perfect Bonus: +0.05");
                s.AppendLine();
                result += 0.05f;
            }

            s.AppendLine("Congregation Bonus: " + result.ToString("F"));
            s.AppendLine("=========================");
            Utility.DebugReport(s.ToString());
            return result;
        }


        /// <summary>
        ///     When an execution completes, this method should trigger.
        /// </summary>
        /// <param name="altar"></param>
        public static void SacrificeExecutionComplete(Building_SacrificialAltar altar)
        {
            altar.ChangeState(Building_SacrificialAltar.State.sacrificing,
                Building_SacrificialAltar.SacrificeState.finishing);
            var starsAreRight =
                altar.Map.GameConditionManager.GetActiveCondition<GameCondition_StarsAreRight>();
            var starsAreWrong =
                altar.Map.GameConditionManager.GetActiveCondition<GameCondition_StarsAreWrong>();
            var bstarsAreRight = starsAreRight != null;
            var bstarsAreWrong = starsAreWrong != null;

            altar.SacrificeData.Entity.ReceiveSacrifice(altar.SacrificeData.Sacrifice, altar.Map, bstarsAreRight,
                bstarsAreWrong);

            var SuccessMod = Rand.Range(0.03f, 0.035f);
            var FailureMod = Rand.Range(-0.035f, 0.03f);

            var tracker = altar.Map.GetComponent<MapComponent_SacrificeTracker>();
            if (tracker != null)
            {
                tracker.lastUsedAltar = altar;
                if (tracker.lastSacrificeType == SacrificeType.human)
                {
                    tracker.lastSpell = altar.SacrificeData.Spell;
                    tracker.lastResult = altar.debugAlwaysSucceed
                        ? SacrificeResult.success
                        : GetSacrificeResult(altar.Map);

                    var funTable = new CultTableOfFun();

                    var result = tracker.lastResult;
                    switch (result)
                    {
                        case SacrificeResult.success:
                            Utility.DebugReport("Sacrifice: Success");
                            CastSpell(altar.SacrificeData.Spell, altar.Map, true);
                            AffectCultMindedness(altar.SacrificeData.Executioner, SuccessMod);
                            break;
                        case SacrificeResult.mixedsuccess:
                            Utility.DebugReport("Sacrifice: Mixed Success");
                            CastSpell(altar.SacrificeData.Spell, altar.Map, true);
                            AffectCultMindedness(altar.SacrificeData.Executioner, SuccessMod);
                            funTable.RollTableOfFun(altar.Map);
                            break;
                        case SacrificeResult.failure:
                            Utility.DebugReport("Sacrifice: Failure");
                            funTable.RollTableOfFun(altar.Map);
                            AffectCultMindedness(altar.SacrificeData.Executioner, FailureMod);
                            SacrificeSpellComplete(altar.SacrificeData.Executioner, altar);
                            break;
                        case SacrificeResult.criticalfailure:
                            Utility.DebugReport("Sacrifice: Critical failure");
                            AffectCultMindedness(altar.SacrificeData.Executioner, FailureMod);
                            SacrificeSpellComplete(altar.SacrificeData.Executioner, altar);
                            break;
                    }

                    //If it's a prisoner, oh lordy~
                    var prisoners = altar.Map.mapPawns.PrisonersOfColonySpawned;
                    if (prisoners != null)
                    {
                        foreach (var prisoner in prisoners)
                        {
                            if (prisoner == null)
                            {
                                continue;
                            }

                            if (GenSight.LineOfSight(prisoner.Position, altar.Position, altar.Map, true))
                            {
                                prisoner.needs?.mood.thoughts.memories.TryGainMemory(CultsDefOf
                                    .Cults_OtherPrisonerWasSacrificed);
                            }
                        }
                    }
                }

                //Tell the player!
                MakeSacrificeThoughts(altar.SacrificeData.Executioner, altar.SacrificeData.Sacrifice, true);
                if (!altar.SacrificeData.Congregation.NullOrEmpty()
                ) //tracker.lastSacrificeCongregation != null && tracker.lastSacrificeCongregation.Count > 0)
                {
                    foreach (var pawn in altar.SacrificeData.Congregation)
                    {
                        if (!pawn.Spawned || pawn.Dead || pawn == altar.SacrificeData.Sacrifice)
                        {
                            continue;
                        }

                        TryGainTempleRoomThought(pawn);
                        if (pawn != altar.SacrificeData.Executioner)
                        {
                            MakeSacrificeThoughts(pawn, altar.SacrificeData.Sacrifice);
                        }
                    }
                }

                tracker.GenerateSacrificeMessage();
                altar.ChangeState(Building_SacrificialAltar.State.sacrificing,
                    Building_SacrificialAltar.SacrificeState.finished);
            }

            //Increase the tally
            Find.World.GetComponent<WorldComponent_GlobalCultTracker>().numHumanSacrifices++;
        }

        public static void WorshipComplete(Pawn preacher, Building_SacrificialAltar altar, CosmicEntity deity)
        {
            altar.ChangeState(Building_SacrificialAltar.State.worshipping,
                Building_SacrificialAltar.WorshipState.finishing);

            deity.ReceiveWorship(preacher);

            altar.ChangeState(Building_SacrificialAltar.State.worshipping,
                Building_SacrificialAltar.WorshipState.finished);
            //altar.currentState = Building_SacrificialAltar.State.finished;

            var CultistMod = Rand.Range(0.01f, 0.02f);
            AffectCultMindedness(preacher, CultistMod);

            var factionBase = (Settlement) altar.Map.info.parent;

            Messages.Message("WorshipFinished".Translate(factionBase.Label), TargetInfo.Invalid,
                MessageTypeDefOf.PositiveEvent);
        }

        public static void OfferingComplete(Pawn offerer, Building_SacrificialAltar altar, CosmicEntity deity,
            List<Thing> offering)
        {
            //altar.ChangeState(Building_SacrificialAltar.State.worshipping, Building_SacrificialAltar.WorshipState.finishing);

            altar.ChangeState(Building_SacrificialAltar.State.offering,
                Building_SacrificialAltar.OfferingState.finished);
            deity.ReceiveOffering(offerer, altar, offering);


            var CultistMod = Rand.Range(0.01f, 0.02f);
            AffectCultMindedness(offerer, CultistMod);

            if (Utility.IsActorAvailable(offerer))
            {
                var job = new Job(CultsDefOf.Cults_ReflectOnOffering)
                {
                    targetA = altar
                };
                offerer.jobs.TryTakeOrderedJob(job);
                //offerer.jobs.EndCurrentJob(JobCondition.InterruptForced);
            }

            var factionBase = (Settlement) altar.Map.info.parent;

            Messages.Message("WorshipFinished".Translate(factionBase.Label), TargetInfo.Invalid,
                MessageTypeDefOf.PositiveEvent);
        }

        public static bool IsSomeoneInvestigating(Map map)
        {
            if (map.GetComponent<MapComponent_LocalCultTracker>() != null)
            {
                if (map.GetComponent<MapComponent_LocalCultTracker>().CurrentSeedState == CultSeedState.NeedWriting)
                {
                    return true;
                }
            }

            if (map.mapPawns.FreeColonists == null)
            {
                return false;
            }

            foreach (var colonist in map.mapPawns.FreeColonists)
            {
                if (colonist.CurJob == null)
                {
                    continue;
                }

                if (colonist.CurJob.def.defName == "Investigate" ||
                    colonist.CurJob.def.defName == "WriteTheBook")
                {
                    return true;
                }
            }

            return false;
        }

        public static bool AreCultObjectsAvailable(Map map)
        {
            Utility.DebugReport("Cult Objects Check");
            //Do we have a forbidden knowledge center?
            if (AreForbiddenKnowledgeCentersAvailable(map))
            {
                Utility.DebugReport("FKC Exists");
                return true;
            }

            //Do we have a book available?
            if (!AreOccultGrimoiresAvailable(map))
            {
                return false;
            }

            Utility.DebugReport("Grimoire Exists");
            return true;
        }

        public static bool AreOccultGrimoiresAvailable(Map map)
        {
            return map?.listerThings.AllThings != null &&
                   map.listerThings.ThingsOfDef(ThingDef.Named("Cults_Grimoire")).Count > 0;
        }

        public static bool AreForbiddenKnowledgeCentersAvailable(Map map)
        {
            return map?.listerBuildings.AllBuildingsColonistOfClass<Building_ForbiddenReserachCenter>() != null &&
                   map.listerBuildings.AllBuildingsColonistOfClass<Building_ForbiddenReserachCenter>().Any();
        }


        public static bool AreAltarsAvailable(Map map)
        {
            return map.listerBuildings.AllBuildingsColonistOfClass<Building_SacrificialAltar>() != null &&
                   map.listerBuildings.AllBuildingsColonistOfClass<Building_SacrificialAltar>().Any();
        }

        public static bool CheckValidCultName(string str)
        {
            if (str.Length > 40)
            {
                return false;
            }

            var str2 = new string(Path.GetInvalidFileNameChars());
            var regex = new Regex("[" + Regex.Escape(str2) + "]");
            return !regex.IsMatch(str);
        }

        public static bool IsPreacher(Pawn p)
        {
            if (p.Map == null) return false;

            var list = p.Map.listerThings.AllThings.FindAll(s => s.GetType() == typeof(Building_SacrificialAltar));
            foreach (var thing in list)
            {
                var b = (Building_SacrificialAltar) thing;
                if (b.preacher == p)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsExecutioner(Pawn p)
        {
            if (p.Map == null) return false;

            var list = p.Map.listerThings.AllThings.FindAll(s => s.GetType() == typeof(Building_SacrificialAltar));
            foreach (var thing in list)
            {
                var b = (Building_SacrificialAltar) thing;
                if (b.SacrificeData?.Executioner == p)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsSacrifice(Pawn p)
        {
            if (p.Map == null) return false;
            
            var list = p.Map.listerThings.AllThings.FindAll(s => s.GetType() == typeof(Building_SacrificialAltar));
            foreach (var thing in list)
            {
                var b = (Building_SacrificialAltar) thing;
                if (b.SacrificeData?.Sacrifice == p)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool ResultFalseWithReport(StringBuilder s)
        {
            s.Append("ActorAvailble: Result = Unavailable");
            Utility.DebugReport(s.ToString());
            return false;
        }

        public static bool IsCultistAvailable(Pawn pawn)
        {
            if (!Utility.IsActorAvailable(pawn))
            {
                return false;
            }

            return IsCultMinded(pawn);
        }

        public static bool IsCultMinded(Pawn pawn)
        {
            if (pawn == null)
            {
                Utility.DebugReport("IsCultMinded :: Pawn Null Exception");
                return false;
            }

            if (pawn.needs == null)
            {
                Utility.DebugReport("IsCultMinded :: Pawn Needs Null Exception");
                return false;
            }

            if (pawn.needs.TryGetNeed<Need_CultMindedness>() != null)
            {
                return pawn.needs.TryGetNeed<Need_CultMindedness>().CurLevel > Need_CultMindedness.ThreshHigh;
            }

            Utility.DebugReport("IsCultMinded :: Pawn has no cult mind");
            return false;
        }

        public static bool ShouldAttendSacrifice(Pawn p, Building_SacrificialAltar altar)
        {
            if (Utility.IsActorAvailable(altar.SacrificeData.Executioner))
            {
                return p != altar.SacrificeData.Executioner && p != altar.SacrificeData.Sacrifice;
            }

            AbortCongregation(altar);
            return false;

            //Everyone get over here!
        }

        public static bool ShouldAttendWorship(Pawn p, Building_SacrificialAltar altar)
        {
            if (Utility.IsActorAvailable(altar.preacher))
            {
                return p != altar.preacher;
            }

            AbortCongregation(altar);
            return false;

            //Everyone get over here!
        }

        public static void RemindPlayerAboutCongregationBonuses()
        {
            if (Rand.Range(0, 100) < 20)
            {
                Messages.Message("Tip: Wear cultist apparel for a worship bonus.", MessageTypeDefOf.SilentInput);
            }
        }

        public static void AffectCultMindedness(Pawn pawn, float amount = 0f, float max = 0.99f)
        {
            var trueMax = max;

            var cultMindedNeed = pawn?.needs.TryGetNeed<Need_CultMindedness>();
            if (cultMindedNeed == null)
            {
                return;
            }

            var result = pawn.needs.TryGetNeed<Need_CultMindedness>().CurLevel;
            if (result > trueMax)
            {
                trueMax = result;
            }

            result += amount;
            result = Mathf.Clamp(result, 0.01f, trueMax);
            pawn.needs.TryGetNeed<Need_CultMindedness>().CurLevel = result;
        }

        public static void InvestigatedCultSeed(Pawn pawn, Thing investigatee)
        {
            //It's a day to remember
            var taleToAdd = TaleDef.Named("ObservedNightmareMonolith");
            if (investigatee is Plant_TreeOfMadness)
            {
                taleToAdd = TaleDef.Named("ObservedNightmareTree");
            }

            if ((pawn.IsColonist || pawn.HostFaction == Faction.OfPlayer) && taleToAdd != null)
            {
                TaleRecorder.RecordTale(taleToAdd, pawn);
            }

            //Internal memory
            pawn.needs.mood.thoughts.memories.TryGainMemory(CultsDefOf.Cults_MadeInvestigation);

            Utility.ApplySanityLoss(pawn);
            AffectCultMindedness(pawn, 0.10f);
            pawn.Map.GetComponent<MapComponent_LocalCultTracker>().CurrentSeedState = CultSeedState.NeedWriting;
            pawn.Map.GetComponent<MapComponent_LocalCultTracker>().CurrentSeedPawn = pawn;
        }

        public static void FinishedTheBook(Pawn pawn)
        {
            pawn.needs.mood.thoughts.memories.TryGainMemory(CultsDefOf.Cults_BlackoutBook);
            Utility.ApplySanityLoss(pawn);
            pawn.Map.GetComponent<MapComponent_LocalCultTracker>().CurrentSeedState = CultSeedState.NeedTable;
            pawn.Map.GetComponent<MapComponent_LocalCultTracker>().CurrentSeedPawn = pawn;

            //Spawn in the book.
            var spawnLoc = pawn.Position + GenAdj.AdjacentCells[(int) Direction8Way.South];
            var unused = pawn.Map.GetComponent<MapComponent_LocalCultTracker>().CurrentSeedTarget;

            var thing = (ThingWithComps) ThingMaker.MakeThing(CultsDefOf.Cults_Grimoire);
            //thing.SetFaction(Faction.OfPlayer);
            GenPlace.TryPlaceThing(thing, spawnLoc, pawn.Map, ThingPlaceMode.Near);
            Find.WindowStack.Add(new Dialog_MessageBox("CultBookSummary".Translate(pawn.Name.ToStringShort),
                "CultBookLabel".Translate()));
        }

        /*
        public static void AttendSacrificeTickCheckEnd(Pawn pawn, Pawn pawn2, bool isExecutioner = false)
        {
            if (pawn == null) return;
            if (pawn.RaceProps.Animal) return;
        }
        */
        public static void AttendWorshipTickCheckEnd(Pawn preacher, Pawn pawn)
        {
            if (preacher == null)
            {
                return;
            }

            if (pawn == null)
            {
                return;
            }

            TryGainTempleRoomThought(pawn);
            var newThought = GetAttendWorshipThoughts(preacher, pawn);
            if (newThought != null)
            {
                pawn.needs.mood.thoughts.memories.TryGainMemory(newThought);
            }
        }

        public static void HoldWorshipTickCheckEnd(Pawn preacher)
        {
            if (preacher == null)
            {
                return;
            }

            TryGainTempleRoomThought(preacher);
            AffectCultMindedness(preacher, 0.1f);
            var newThought = CultsDefOf.Cults_HeldSermon; // DefDatabase<ThoughtDef>.GetNamed("HeldSermon");
            if (newThought != null)
            {
                preacher.needs.mood.thoughts.memories.TryGainMemory(newThought);
            }
        }

        public static void MakeSacrificeThoughts(Pawn attendee, Pawn other = null, bool isExcutioner = false)
        {
            if (attendee == null)
            {
                return;
            }

            var lastSacrifice =
                attendee.Map.GetComponent<MapComponent_SacrificeTracker>().lastSacrificeType;

            //Human sacrifices
            if (lastSacrifice == SacrificeType.human)
            {
                //Sacrifice Thought
                ThoughtDef resultThought = null;
                switch (attendee.Map.GetComponent<MapComponent_SacrificeTracker>().lastResult)
                {
                    case SacrificeResult.mixedsuccess:
                    case SacrificeResult.success:
                        resultThought = IsCultMinded(attendee)
                            ? CultsDefOf.Cults_AttendedSuccessfulSacrifice
                            : CultsDefOf.Cults_InnocentAttendedSuccessfulSacrifice;

                        break;
                    case SacrificeResult.failure:
                    case SacrificeResult.criticalfailure:
                        resultThought = IsCultMinded(attendee)
                            ? CultsDefOf.Cults_AttendedFailedSacrifice
                            : CultsDefOf.Cults_InnocentAttendedFailedSacrifice;

                        break;
                    case SacrificeResult.none:
                        break;
                }

                attendee.needs.mood.thoughts.memories.TryGainMemory(resultThought);

                //Relationship Thoughts
                if (other == null)
                {
                    return;
                }
                //Family

                ThoughtDef familyThought = null;
                if (attendee.relations.FamilyByBlood.Contains(other))
                {
                    familyThought = isExcutioner ? CultsDefOf.Cults_ExecutedFamily : CultsDefOf.Cults_SacrificedFamily;
                }

                if (familyThought != null)
                {
                    attendee.needs.mood.thoughts.memories.TryGainMemory(familyThought);
                }

                //Friends and Rivals
                ThoughtDef relationThought = null;
                var num = attendee.relations.OpinionOf(other);
                if (num >= 20)
                {
                    relationThought = isExcutioner ? ThoughtDefOf.KilledMyFriend : CultsDefOf.Cults_SacrificedFriend;
                }
                else if (num <= -20)
                {
                    relationThought = isExcutioner ? ThoughtDefOf.KilledMyRival : CultsDefOf.Cults_SacrificedRival;
                }

                if (relationThought != null)
                {
                    attendee.needs.mood.thoughts.memories.TryGainMemory(relationThought);
                }

                //Bloodlust
                if (!attendee.story.traits.HasTrait(TraitDefOf.Bloodlust))
                {
                    return;
                }

                attendee.needs.mood.thoughts.memories.TryGainMemory(
                    isExcutioner ? ThoughtDefOf.KilledHumanlikeBloodlust : ThoughtDefOf.WitnessedDeathBloodlust, other);
            }

            //Animal Sacrifices
            else if (lastSacrifice == SacrificeType.animal)
            {
                if (other?.RaceProps == null)
                {
                    return;
                }

                if (!other.RaceProps.Animal)
                {
                    return;
                }

                //Pet checker
                if (other.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Bond) != attendee)
                {
                    return;
                }

                attendee.needs.mood.thoughts.memories.TryGainMemory(
                    isExcutioner ? CultsDefOf.Cults_ExecutedPet : CultsDefOf.Cults_SacrificedPet,
                    other);
            }
        }

        public static ThoughtDef GetAttendWorshipThoughts(Pawn preacher, Pawn attendee)
        {
            //The grades of a sermon are categorized like this internally.
            const float S_Effect = 0.3f;
            const float A_Effect = 0.25f;
            const float B_Effect = 0.1f;
            const float C_Effect = 0.05f;
            const float F_Effect = 0.01f;

            var CultistMod = Rand.Range(0.01f, 0.02f);
            var InnocentMod = Rand.Range(-0.005f, 0.1f);

            if (attendee == null)
            {
                return null;
            }

            var num = preacher.skills.GetSkill(SkillDefOf.Social).Level;
            num += Rand.Range(-6, 6); //Randomness


            switch (num)
            {
                //S-Ranked Sermon: WOW!
                case > 20 when IsCultMinded(attendee):
                    AffectCultMindedness(attendee, S_Effect + CultistMod);
                    return CultsDefOf.Cults_AttendedIncredibleSermonAsCultist;
                case > 20:
                    AffectCultMindedness(attendee, S_Effect + InnocentMod);
                    return CultsDefOf.Cults_AttendedIncredibleSermonAsInnocent;
                //A-Ranked Sermon: Fantastic
                case <= 20 and > 15 when IsCultMinded(attendee):
                    AffectCultMindedness(attendee, A_Effect + CultistMod);
                    return CultsDefOf.Cults_AttendedGreatSermonAsCultist;
                case <= 20 and > 15:
                    AffectCultMindedness(attendee, A_Effect + InnocentMod);
                    return CultsDefOf.Cults_AttendedGreatSermonAsInnocent;
                //B-Ranked Sermon: Alright
                case <= 15 and > 10 when IsCultMinded(attendee):
                    AffectCultMindedness(attendee, B_Effect + CultistMod);
                    return CultsDefOf.Cults_AttendedGoodSermonAsCultist;
                case <= 15 and > 10:
                    AffectCultMindedness(attendee, B_Effect + InnocentMod);
                    return CultsDefOf.Cults_AttendedGoodSermonAsInnocent;
                //C-Ranked Sermon: Average
                case <= 10 and > 5 when IsCultMinded(attendee):
                    AffectCultMindedness(attendee, C_Effect + CultistMod);
                    return CultsDefOf.Cults_AttendedDecentSermonAsCultist;
                case <= 10 and > 5:
                    AffectCultMindedness(attendee, C_Effect + InnocentMod);
                    return CultsDefOf.Cults_AttendedDecentSermonAsInnocent;
            }

            //F-Ranked Sermon: Garbage
            if (IsCultMinded(attendee))
            {
                AffectCultMindedness(attendee, F_Effect + CultistMod);
                return CultsDefOf.Cults_AttendedAwfulSermonAsCultist;
            }

            AffectCultMindedness(attendee, F_Effect + InnocentMod);
            return CultsDefOf.Cults_AttendedAwfulSermonAsInnocent;
        }

        // RimWorld.JoyUtility
        public static void TryGainTempleRoomThought(Pawn pawn)
        {
            var room = pawn.GetRoom();
            var def = CultsDefOf.Cults_PrayedInImpressiveTemple;
            if (pawn == null)
            {
                return;
            }

            if (room?.Role == null)
            {
                return;
            }

            if (def == null)
            {
                return;
            }

            if (room.Role != CultsDefOf.Cults_Temple)
            {
                return;
            }

            var scoreStageIndex =
                RoomStatDefOf.Impressiveness.GetScoreStageIndex(room.GetStat(RoomStatDefOf.Impressiveness));
            if (def.stages[scoreStageIndex] == null)
            {
                return;
            }

            pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(def, scoreStageIndex));
        }

        public static void GiveAttendSacrificeJob(Building_SacrificialAltar altar, Pawn attendee)
        {
            if (IsExecutioner(attendee))
            {
                return;
            }

            if (IsSacrifice(attendee))
            {
                return;
            }

            if (!Utility.IsActorAvailable(attendee))
            {
                return;
            }

            if (attendee.jobs.curJob.def == CultsDefOf.Cults_ReflectOnResult)
            {
                return;
            }

            if (attendee.jobs.curJob.def == CultsDefOf.Cults_AttendSacrifice)
            {
                return;
            }

            if (attendee.Drafted)
            {
                return;
            }

            if (attendee.IsPrisoner)
            {
                return;
            }

            if (!WatchBuildingUtility.TryFindBestWatchCell(altar, attendee, true, out var result, out var chair))
            {
                if (!WatchBuildingUtility.TryFindBestWatchCell(altar, attendee, false, out result, out chair))
                {
                    return;
                }
            }

            var dir = altar.Rotation.Opposite.AsInt;

            if (chair != null)
            {
                var newPos = chair.Position + GenAdj.CardinalDirections[dir];

                var J = new Job(CultsDefOf.Cults_AttendSacrifice, altar, newPos, chair)
                {
                    playerForced = true,
                    ignoreJoyTimeAssignment = true,
                    expiryInterval = 9999,
                    ignoreDesignations = true,
                    ignoreForbidden = true
                };
                //Cthulhu.Utility.DebugReport("Cults :: Original Position " + chair.Position.ToString() + " :: Modded Position " + newPos.ToString());
                attendee.jobs.TryTakeOrderedJob(J);
                //attendee.jobs.EndCurrentJob(JobCondition.Incompletable);
            }
            else
            {
                var newPos = result + GenAdj.CardinalDirections[dir];

                var J = new Job(CultsDefOf.Cults_AttendSacrifice, altar, newPos, result)
                {
                    playerForced = true,
                    ignoreJoyTimeAssignment = true,
                    expiryInterval = 9999,
                    ignoreDesignations = true,
                    ignoreForbidden = true
                };
                //Cthulhu.Utility.DebugReport("Cults :: Original Position " + result.ToString() + " :: Modded Position " + newPos.ToString());
                attendee.jobs.TryTakeOrderedJob(J);
                //attendee.jobs.EndCurrentJob(JobCondition.Incompletable);
            }
        }

        public static Pawn DetermineBestResearcher(Map map)
        {
            Pawn result = null;
            foreach (var p in map.mapPawns.FreeColonistsSpawned)
            {
                if (result == null)
                {
                    result = p;
                }

                if (Utility.GetResearchSkill(result) < Utility.GetResearchSkill(p))
                {
                    result = p;
                }
            }

            return result;
        }

        public static Pawn DetermineBestPreacher(Map map)
        {
            Pawn result = null;
            foreach (var p in map.mapPawns.FreeColonistsSpawned)
            {
                if (result == null)
                {
                    result = p;
                }

                if (IsCultMinded(p) && Utility.GetSocialSkill(result) < Utility.GetSocialSkill(p))
                {
                    result = p;
                }
            }

            if (!IsCultMinded(result))
            {
                result = null;
            }

            return result;
        }


        public static bool ShouldAttendeeKeepAttendingWorship(Pawn p)
        {
            return !p.Downed && (p?.needs == null || p?.needs?.food?.Starving != true) && p?.health?.hediffSet?.BleedRateTotal <= 0f && (p?.needs?.rest == null || p?.needs?.rest?.CurCategory < RestCategory.Exhausted) && p?.health?.hediffSet?.HasTendableNonInjuryNonMissingPartHediff(false) != true && p?.Awake() == true && p?.InAggroMentalState != true && p?.IsPrisoner != true;
        }

        //Checkyoself
        public static void GiveAttendWorshipJob(Building_SacrificialAltar altar, Pawn attendee)
        {
            //Log.Message("1");
            if (IsPreacher(attendee))
            {
                return;
            }

            if (!ShouldAttendeeKeepAttendingWorship(attendee))
            {
                return;
            }

            if (attendee.Drafted)
            {
                return;
            }

            if (attendee.IsPrisoner)
            {
                return;
            }

            if (attendee.jobs.curJob.def.defName == "ReflectOnWorship")
            {
                return;
            }

            if (attendee.jobs.curJob.def.defName == "AttendWorship")
            {
                return;
            }

            if (!WatchBuildingUtility.TryFindBestWatchCell(altar, attendee, true, out var result, out var chair))
            {
                if (!WatchBuildingUtility.TryFindBestWatchCell(altar, attendee, false, out result, out chair))
                {
                    return;
                }
            }

            //Log.Message("2");
            var dir = altar.Rotation.Opposite.AsInt;
            Job attendJob;
            IntVec3 newPos;
            if (chair != null)
            {
                newPos = chair.Position + GenAdj.CardinalDirections[dir];

                //Log.Message("3a");

                attendJob = new Job(CultsDefOf.Cults_AttendWorship, altar, newPos, chair)
                {
                    playerForced = true,
                    ignoreJoyTimeAssignment = true,
                    expiryInterval = 9999,
                    ignoreDesignations = true,
                    ignoreForbidden = true
                };
            }
            else
            {
                //Log.Message("3b");

                newPos = result + GenAdj.CardinalDirections[dir];

                attendJob = new Job(CultsDefOf.Cults_AttendWorship, altar, newPos, result)
                {
                    playerForced = true,
                    ignoreJoyTimeAssignment = true,
                    expiryInterval = 9999,
                    ignoreDesignations = true,
                    ignoreForbidden = true
                };
            }

            if (!ModSettings_Data.makeWorshipsVoluntary)
            {
                attendee.jobs.EndCurrentJob(JobCondition.Incompletable);
                attendee.jobs.TryTakeOrderedJob(attendJob);
            }
            else
            {
                attendee.jobs.jobQueue.EnqueueLast(attendJob);
                if (attendee.CurJobDef?.defName.Contains("Haul") != true)
                {
                    attendee.jobs.EndCurrentJob(JobCondition.Incompletable);
                }
            }
        }

        public static void AbortCongregation(Building_SacrificialAltar altar)
        {
            altar?.ChangeState(Building_SacrificialAltar.State.notinuse);
        }

        public static void AbortCongregation(Building_SacrificialAltar altar, string reason)
        {
            altar?.ChangeState(Building_SacrificialAltar.State.notinuse);

            Messages.Message(reason + " Aborting congregation.", MessageTypeDefOf.NegativeEvent);
        }

        public static void CastSpell(IncidentDef spell, Map map, bool fromAltar = false)
        {
            if (spell != null)
            {
                var parms = StorytellerUtility.DefaultParmsNow(spell.category, map);
                spell.Worker.TryExecute(parms);
                Utility.DebugReport("Cults_Spell cast: " + spell);
            }

            if (!fromAltar)
            {
                return;
            }

            var lastAltar = map.GetComponent<MapComponent_SacrificeTracker>().lastUsedAltar;
            SacrificeSpellComplete(lastAltar.SacrificeData.Executioner, lastAltar);
        }

        public static void SacrificeSpellComplete(Pawn executioner, Building_SacrificialAltar altar)
        {
            if (altar == null)
            {
                Utility.DebugReport("Altar Null Exception");
                return;
            }

            if (executioner == null)
            {
                Utility.DebugReport("Executioner null exception");
            }

            if (Utility.IsActorAvailable(executioner))
            {
                var job = new Job(CultsDefOf.Cults_ReflectOnResult)
                {
                    targetA = altar
                };
                executioner?.jobs.TryTakeOrderedJob(job);
                //executioner.jobs.EndCurrentJob(JobCondition.InterruptForced);
            }

            altar.ChangeState(Building_SacrificialAltar.State.sacrificing,
                Building_SacrificialAltar.SacrificeState.finished);
            //altar.currentState = Building_SacrificialAltar.State.finished;
            //Map.GetComponent<MapComponent_SacrificeTracker>().lastUsedAltar.ChangeState(Building_SacrificialAltar.State.sacrificing, Building_SacrificialAltar.SacrificeState.finished);
        }
    }
}