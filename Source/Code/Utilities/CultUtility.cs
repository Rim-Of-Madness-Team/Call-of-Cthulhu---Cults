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

            return CellFinderLoose.TryFindSkyfallerCell(skyfaller: defToCheck, map: map, cell: out pos, minDistToEdge: 10, nearLoc: nearLoc, nearLocMaxDist: maxDist, allowRoofedCells: true, allowCellsWithItems: false,
                allowCellsWithBuildings: false, colonyReachable: false, avoidColonistsIfExplosive: false);
        }

        public static List<Pawn> GetCultMindedAffectablePawns(Map map)
        {
            var spawnedColonyMembers = new List<Pawn>(collection: map.mapPawns.FreeColonistsSpawned);
            var spawnedPrisonersAndSlaves = new List<Pawn>(collection: map.mapPawns.SlavesAndPrisonersOfColonySpawned);
            spawnedColonyMembers.AddRange(collection: spawnedPrisonersAndSlaves);
            return spawnedColonyMembers;
        }

        public static float GetBaseCultistModifier(Pawn pawn)
        {
            float result = 0;
            var bigMod = Rand.Range(min: 0.2f, max: 0.25f);
            var smallMod = Rand.Range(min: 0.05f, max: 0.1f);
            if (pawn?.story?.Adulthood == null ||
                pawn.story?.Childhood == null)
            {
                return result;
            }

            string adultStory = pawn.story.Adulthood.FullDescriptionFor(p: pawn);
            string childStory = pawn.story.Childhood.FullDescriptionFor(p: pawn);

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

            if (adultStory.Contains(value: "midworld") || adultStory.Contains(value: "Midworld"))
            {
                result += smallMod;
            }

            if (childStory.Contains(value: "midworld") || childStory.Contains(value: "Midworld"))
            {
                result += smallMod;
            }

            if (childStory.Contains(value: "abandoned"))
            {
                result += smallMod;
            }

            //Moralist modifiers

            //Moral: I am not a violent person.
            if (pawn.story.Adulthood.workDisables == WorkTags.Violent ||
                pawn.story.Childhood.workDisables == WorkTags.Violent)
            {
                result -= bigMod;
            }

            //Cult disinclined.
            //          Glitterworlders. Morality is paramount.
            if (adultStory.Contains(value: "glitterworld") || adultStory.Contains(value: "Glitterworld"))
            {
                result -= smallMod;
            }

            if (childStory.Contains(value: "glitterworld") || childStory.Contains(value: "Glitterworld"))
            {
                result -= smallMod;
            }

            //Randomness
            //          Evangelists can be cultist or moralists.
            if (pawn.story.Adulthood.title.Contains(value: "Evangelist"))
            {
                if (Rand.Range(min: 0, max: 100) > 50)
                {
                    result += bigMod;
                }

                result -= bigMod;
            }

            if (Rand.Range(min: 0, max: 100) > 50)
            {
                result += smallMod;
            }
            else
            {
                result -= smallMod;
            }

            return Mathf.Clamp(value: result, min: -0.5f, max: 0.2f);
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

                if (!CellFinder.TryFindRandomEdgeCellWith(validator: c => map.reachability.CanReachColony(c: c), map: map,
                    roadChance: CellFinder.EdgeRoadChance_Neutral, result: out var loc))
                {
                    return false;
                }

                if (!TryGenerateCultist(cultist: out var p, map: map, type: type))
                {
                    //Log.Messag("Unable to generate cultist");
                    return false;
                }

                if (p == null)
                {
                    //Log.Messag("Pawn is null");
                    return false;
                }

                GenSpawn.Spawn(newThing: p, loc: loc, map: map);
                var text = "CultistJoin".Translate(arg1: p.kindDef.label, arg2: p.story.Adulthood.title.ToLower());
                text = text.AdjustedFor(p: p);
                var label = "LetterLabelCultistJoin".Translate();
                if (showMessage)
                {
                    Find.LetterStack.ReceiveLetter(label: label, text: text, textLetterDef: CultsDefOf.Cults_StandardMessage);
                }

                PawnRelationUtility.TryAppendRelationsWithColonistsInfo(text: ref text, title: ref label, pawn: p);
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
                    request = new PawnGenerationRequest(kind: pawnKindDef, faction: Faction.OfPlayer, context: PawnGenerationContext.NonPlayer,
                        tile: map.Tile, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: false, 
                        mustBeCapableOfViolence: true, colonistRelationChanceFactor: 0f, forceAddFreeWarmLayerIfNeeded: true, allowGay: false, allowPregnant: true, 
                        allowFood: true, allowAddictions: false);
                    p = PawnGenerator.GeneratePawn(request: request);

                    if (p.skills.GetSkill(skillDef: SkillDefOf.Social).TotallyDisabled)
                    {
                        continue;
                    }

                    if (p.skills.GetSkill(skillDef: SkillDefOf.Social).Level < 5)
                    {
                        continue;
                    }

                    if (p.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(flag: WorkTags.Social))
                    {
                        continue;
                    }

                    break;
                }
            }
            //If it's a dark emissary of Nyarlathotep, we need to add clothing.
            else if (type == CultistType.DarkEmmisary)
            {
                request = new PawnGenerationRequest(kind: pawnKindDef, faction: Faction.OfPlayer, context: PawnGenerationContext.NonPlayer,
                    tile: map.Tile, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: false,
                    mustBeCapableOfViolence: true, colonistRelationChanceFactor: 0f, forceAddFreeWarmLayerIfNeeded: true, allowGay: false,
                    allowPregnant: true, allowFood: true, allowAddictions: false);
                p = PawnGenerator.GeneratePawn(request: request);
                var tHood = ThingMaker.MakeThing(def: ThingDef.Named(defName: "Apparel_NyarlathotepHood"),
                    stuff: ThingDef.Named(defName: "DevilstrandCloth"));
                var tRobe = ThingMaker.MakeThing(def: ThingDef.Named(defName: "Apparel_CultistRobes"),
                    stuff: ThingDef.Named(defName: "DevilstrandCloth"));
                var Hood = tHood as Apparel;
                var Robe = tRobe as Apparel;
                p.apparel.Wear(newApparel: Hood, dropReplacedApparel: false);
                p.apparel.Wear(newApparel: Robe, dropReplacedApparel: false);
            }
            else
            {
                request = new PawnGenerationRequest(kind: pawnKindDef, faction: Faction.OfPlayer, context: PawnGenerationContext.NonPlayer,
                    tile: map.Tile, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: false, 
                    mustBeCapableOfViolence: true, colonistRelationChanceFactor: 0f, forceAddFreeWarmLayerIfNeeded: true,
                    allowGay: false, allowPregnant: true, allowFood: true, allowAddictions: false);
                p = PawnGenerator.GeneratePawn(request: request);
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
            if (!p.story.traits.HasTrait(tDef: TraitDefOf.Cannibal))
            {
                traitToAdd = TraitDefOf.Cannibal;
            }

            if (!p.story.traits.HasTrait(tDef: TraitDefOf.Psychopath))
            {
                traitToAdd = TraitDefOf.Psychopath;
            }

            if (p.story.traits.allTraits.Count < 3)
            {
                p.story.traits.GainTrait(trait: new Trait(def: traitToAdd));
            }
            else
            {
                foreach (var t in p.story.traits.allTraits)
                {
                    if (t.def == TraitDefOf.Cannibal || t.def == TraitDefOf.Psychopath)
                    {
                        continue;
                    }

                    p.story.traits.allTraits.Remove(item: t);
                    break; //Remove 1 trait and get out
                }

                p.story.traits.GainTrait(trait: new Trait(def: traitToAdd));
            }

            //Add cult-mindedness.
            AffectCultMindedness(pawn: p, amount: 0.8f);

            cultist = p;
            return true;
        }

        public static SacrificeResult GetSacrificeResult(Map map)
        {
            //Temporary
            //return SacrificeResult.success;
            var s = new StringBuilder();
            s.AppendLine(value: "Sacrifice Success Calculation");

            var Success = false;
            var TableOfFun = false;

            var diceRoll = Rand.Range(min: 1, max: 100);
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
                    var date = GenDate.DateFullStringAt(absTicks: GenDate.TickGameToAbs(gameTick: Find.TickManager.TicksGame),
                        location: Find.WorldGrid.LongLatOf(tileID: map.Tile));
                    reportHeader.AppendLine(value: "Cults_LRHeading".Translate(arg1: date));
                    reportHeader.AppendLine(value: "Cults_LRSacrifice".Translate(arg1: altar.SacrificeData.Sacrifice.def.label,
                        arg2: altar.SacrificeData.Entity.LabelCap, arg3: altar.SacrificeData.Spell.label));
                    reportHeader.AppendLine(
                        value: "Cults_LRAttendance".Translate(arg1: altar.SacrificeData?.Congregation?.Count ?? 0));
                    reportFavorables.AppendLine(value: "Cults_LRFactorsFavorable".Translate());
                    reportUnfavorables.AppendLine(value: "Cults_LRFactorsUnfavorable".Translate());
                    ///////////////////

                    if (altar.SacrificeData.Spell.defName != "Cults_SpellFavor")
                    {
                        s.AppendLine(value: "Initial Failure Difficulty: " + baseDifficulty);

                        //Difficulty modifiers
                        failDifficulty +=
                            SpellCalc_TierDifficulty(altar: altar, debugLog: s, reportUnfavorables: reportUnfavorables,
                                reportFavorables: reportFavorables); //Tier 2 +10, Tier 3 + 20, Final +50
                        failDifficulty += SpellCalc_GameConditions(altar: altar, s: s, reportUnfavorables: reportUnfavorables, reportFavorables: reportFavorables,
                            successModifier: out var successModifier); //+50 stars are wrong / -20 stars are right
                        failDifficulty += SpellCalc_Characters(altar: altar, debugString: s, reportUnfavorables: reportUnfavorables, reportFavorables: reportFavorables,
                            successModifierIn: successModifier, successModifierOut: out successModifier); //+50 stars are wrong / -20 stars are right
                        s.AppendLine(value: "Adjusted Failure Difficulty: " + baseDifficulty + failDifficulty);

                        //Success modifiers
                        successModifier +=
                            SpellCalc_CongregationQuality(altar: altar, debugLog: s,
                                reportFavorables: reportFavorables); //Some robes +10, Dagger equipped +5, All robed and hooded +15
                        successModifier +=
                            SpellCalc_StatuesNearby(altar: altar, s: s,
                                reportFavorables: reportFavorables); //Minimum one statue of normal quality +10, Statue of deity +10
                        successModifier +=
                            SpellCalc_TempleQuality(altar: altar, s: s,
                                reportFavorables: reportFavorables); //Some quality +10, Great quality +20, Outdoors when deity favors it +20
                        s.AppendLine(value: "Success Modifier: " + successModifier);

                        //Difficulty check
                        s.AppendLine(value: "Difficulty check: (Rolling d100. " + diceRoll +
                                            " result + Success Modifier: " + successModifier +
                                            ") vs (Difficulty: " + baseDifficulty + " + Modifier: " + failDifficulty +
                                            ")");
                        if (diceRoll + successModifier >= baseDifficulty + failDifficulty)
                        {
                            Success = true;
                        }

                        s.AppendLine(value: "Success = " + Success.ToString().CapitalizeFirst());
                        reportResult.AppendLine(value: "Cults_LRCheck".Translate(
                            arg1: diceRoll, arg2: successModifier, arg3: baseDifficulty, arg4: failDifficulty));
                        reportResult.AppendLine(value: Success
                            ? "Cults_LRResultSuccess".Translate()
                            : "Cults_LRResultFailure".Translate());

                        //Table of fun
                        var randFun = Rand.Range(min: 1, max: 10);
                        if (randFun >= 6)
                        {
                            TableOfFun = true; //40% chance
                        }

                        s.AppendLine(value: "Side Effect = " + TableOfFun);

                        altar.LastReport = reportHeader + "\n" +
                                           reportFavorables + "\n" +
                                           reportUnfavorables + "\n" +
                                           reportResult;

                        Utility.DebugReport(x: s.ToString());
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

            Utility.DebugReport(x: s.ToString());
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
            var experience = worldComponentGlobalCultTracker.GetExperience(p: executioner, sacrifice: true);
            if (experience == 0)
            {
                modifier += 10;
                debugString.AppendLine(value: "Executioner Difficulty: +10 First time");
                reportUnfavorables.AppendLine(value: "+10 / 10: " + "Cults_LRExecFirstTime".Translate(arg1: executioner.LabelShort));
            }
            else if (experience < 3)
            {
                modifier += 5;
                debugString.AppendLine(value: "Executioner Difficulty: +10 Inexperienced executioner");
                reportUnfavorables.AppendLine(value: "+ 5 / 10: " +
                                                     "Cults_LRExecInex".Translate(
                                                         arg1: executioner.LabelShort, arg2: experience));
            }
            else if (experience < 10)
            {
                debugString.AppendLine(value: "Executioner Success: +0 Somewhat-lacking executioner");
                reportFavorables.AppendLine(value: "+ 0 / 10: " +
                                                   "Cults_LRExecEven".Translate(
                                                       arg1: executioner.LabelShort, arg2: experience));
            }
            else if (experience < 24)
            {
                successModifierOut += 5;
                debugString.AppendLine(value: "Executioner Success: +5 Experienced executioner");
                reportFavorables.AppendLine(value: "+ 5 / 10: " +
                                                   "Cults_LRExecExp".Translate(
                                                       arg1: executioner.LabelShort, arg2: experience));
            }
            else
            {
                successModifierOut += 10;
                debugString.AppendLine(value: "Executioner Success: +10 Expert executioner");
                reportFavorables.AppendLine(value: "+10 / 10: " +
                                                   "Cults_LRExecVeryExp".Translate(arg1: executioner.LabelShort, arg2: experience));
                var executionerBonus = new IntRange(min: 0, max: 5).RandomInRange;
                successModifierOut += executionerBonus;
                debugString.AppendLine(value: "Executioner Success: +" + executionerBonus + " Executioner finesse bonus");
                reportFavorables.AppendLine(value: "+" + executionerBonus + " /   5: " +
                                                   "Cults_LRExecBonus".Translate(arg1: executioner.LabelShort));
            }

            worldComponentGlobalCultTracker.GainExperience(p: executioner, carriedOutSacrifice: true);
            return modifier;
        }

        private static int SpellCalc_GameConditions(Building_SacrificialAltar altar, StringBuilder s,
            StringBuilder reportUnfavorables, StringBuilder reportFavorables, out int successModifier)
        {
            var modifier = 0;
            successModifier = 0;
            var starsAreRight = altar.Map.GameConditionManager.GetActiveCondition<GameCondition_StarsAreRight>();
            var starsAreWrong = altar.Map.GameConditionManager.GetActiveCondition<GameCondition_StarsAreWrong>();
            var eclipseActive = altar.Map.GameConditionManager.GetActiveCondition(def: GameConditionDefOf.Eclipse);
            var auroraActive = altar.Map.GameConditionManager.GetActiveCondition(def: GameConditionDefOf.Aurora);

            //Astral events
            /////////////
            if (starsAreRight != null)
            {
                s.AppendLine(value: "Map Condition Difficulty: +20 Stars Are Right");
                successModifier += 20;
                reportFavorables.AppendLine(value: "+20 / 20: " +
                                                   "Cults_LRStarsAreRight".Translate(arg1: altar.SacrificeData.Entity.LabelCap));
            }
            else if (starsAreWrong != null)
            {
                s.AppendLine(value: "Map Condition Difficulty: +50 Stars Are Wrong");
                modifier += 50;
                reportUnfavorables.AppendLine(value: "+50 / 50: " +
                                                     "Cults_LRStarsAreWrong".Translate(arg1: altar.SacrificeData.Entity.LabelCap));
            }
            else
            {
                s.AppendLine(value: "Map Condition Difficulty: +0 No Astral Event");
                reportUnfavorables.AppendLine(value: "+  0 / 50: " + "Cults_LRNoAstralEvents".Translate());
            }

            //Eclipse
            ///////////
            if (eclipseActive != null)
            {
                s.AppendLine(value: "Map Condition Difficulty: +5 Eclipse Active");
                successModifier += 5;
                reportFavorables.AppendLine(value: "+ 5 /   5: " +
                                                   "Cults_LREclipseActive".Translate());
            }
            else
            {
                s.AppendLine(value: "Map Condition Difficulty: +0 No Eclipse Active");
                reportFavorables.AppendLine(value: "+ 0 /   5: " +
                                                   "Cults_LRNoEclipseActive".Translate());
            }

            //Aurora
            ///////////
            if (auroraActive != null)
            {
                s.AppendLine(value: "Map Condition Difficulty: +5 Aurora Active");
                successModifier += 5;
                reportFavorables.AppendLine(value: "+ 5 /   5: " +
                                                   "Cults_LRAuroraActive".Translate());
            }
            else
            {
                s.AppendLine(value: "Map Condition Difficulty: +0 No Aurora Active");
                reportFavorables.AppendLine(value: "+ 0 /   5: " +
                                                   "Cults_LRNoAuroraActive".Translate());
            }

            return modifier;
        }

        // RimWorld.BaseGen.SymbolResolver_Doors
        private static bool IsOutdoorsAt(Map map, IntVec3 c)
        {
            return c.GetRegion(map: map) != null && c.GetRegion(map: map).Room.PsychologicallyOutdoors;
        }

        private static int SpellCalc_TempleQuality(Building_SacrificialAltar altar, StringBuilder s,
            StringBuilder reportFavorables)
        {
            var modifier = 0;
            var deity = altar.SacrificeData.Entity;
            if (!IsOutdoorsAt(map: altar.Map, c: altar.Position))
            {
                var temple = altar.GetRoom();
                if (temple == null)
                {
                    return modifier;
                }

                var impressiveScore = temple.GetStat(roomStat: RoomStatDefOf.Impressiveness);
                var wealthScore = temple.GetStat(roomStat: RoomStatDefOf.Wealth);
                var spaceScore = temple.GetStat(roomStat: RoomStatDefOf.Space);
                var beautyScore = temple.GetStat(roomStat: RoomStatDefOf.Beauty);

                //Expected quality. 13x13 tiles. Pews. Altar. 2 objects of Lighting.

                //WEALTH
                //////////////
                if (wealthScore < 2000)
                {
                    s.AppendLine(value: "Temple Wealth Bonus: +0 - Not good");
                    reportFavorables.AppendLine(value: "+ 0 /   5: " + "Cults_LRTempWealthNo".Translate());
                }

                if (wealthScore < 4000)
                {
                    modifier += 3;
                    s.AppendLine(value: "Temple Wealth Bonus: +3 - Good");
                    reportFavorables.AppendLine(value: "+ 3 /   5: " + "Cults_LRTempWealthDecent".Translate());
                }
                else if (wealthScore >= 4000)
                {
                    modifier += 5;
                    s.AppendLine(value: "Temple Wealth Bonus: +5 - Great");
                    reportFavorables.AppendLine(value: "+ 5 /   5: " + "Cults_LRTempWealth".Translate());
                }

                //SPACE
                //////////////////
                if (spaceScore < 160)
                {
                    s.AppendLine(value: "Temple Space Bonus: +0 - Not good");
                    reportFavorables.AppendLine(value: "+ 0 /   5: " + "Cults_LRTempSpaceNo".Translate());
                }
                else if (spaceScore < 400)
                {
                    modifier += 3;
                    s.AppendLine(value: "Temple Space Bonus: +3 - Good");
                    reportFavorables.AppendLine(value: "+ 3 /   5: " + "Cults_LRTempSpaceDecent".Translate());
                }
                else if (spaceScore >= 400)
                {
                    modifier += 5;
                    s.AppendLine(value: "Temple Space Bonus: +5 - Great");
                    reportFavorables.AppendLine(value: "+ 5 /   5: " + "Cults_LRTempSpace".Translate());
                }

                //BEAUTY
                ////////////////

                if (beautyScore < 1.59)
                {
                    s.AppendLine(value: "Temple Beauty Bonus: +0 - Not good");
                    reportFavorables.AppendLine(value: "+ 0 /   5: " + "Cults_LRTempBeautyNo".Translate());
                }
                else if (beautyScore <= 2.0)
                {
                    modifier += 3;
                    s.AppendLine(value: "Temple Beauty Bonus: +3 - Good");
                    reportFavorables.AppendLine(value: "+ 3 /   5: " + "Cults_LRTempBeautyDecent".Translate());
                }
                else if (beautyScore >= 2.0)
                {
                    modifier += 5;
                    s.AppendLine(value: "Temple Beauty Bonus: +5 - Great");
                    reportFavorables.AppendLine(value: "+ 5 /   5: " + "Cults_LRTempBeauty".Translate());
                }

                //IMPRESSIVENESS
                ////////////////
                if (impressiveScore < 80)
                {
                    s.AppendLine(value: "Temple Quality Bonus: +0 - Not good");
                    reportFavorables.AppendLine(value: "+ 0 /   5: " + "Cults_LRTempImpressNo".Translate());
                }
                else if (impressiveScore < 150)
                {
                    modifier += 3;
                    s.AppendLine(value: "Temple Quality Bonus: + 3 - Good");
                    reportFavorables.AppendLine(value: "+ 3 /   5: " + "Cults_LRTempImpressDecent".Translate());
                }
                else if (impressiveScore > 150)
                {
                    modifier += 5;
                    s.AppendLine(value: "Temple Quality Bonus: +5 - Great");
                    reportFavorables.AppendLine(value: "+ 5 /   5: " + "Cults_LRTempImpress".Translate());
                }
            }
            else
            {
                if (deity.FavorsOutdoorWorship)
                {
                    modifier += 20;
                    s.AppendLine(value: "Temple Quality Bonus: +20 Outside Deity Favor");
                    reportFavorables.AppendLine(value: "+20 / 20: " + "Cults_LRTempOutdoorFavored".Translate(arg1: deity.LabelCap));
                }
                else
                {
                    s.AppendLine(value: "Temple Quality Bonus: +0 - No Outside Deity Favor");
                    reportFavorables.AppendLine(value: "+ 0 / 20: " +
                                                       "Cults_LRTempNoOutdoorFavored".Translate(arg1: deity.LabelCap));
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

            var sculptures = temple?.ContainedAndAdjacentThings.FindAll(match: x => x is ThingWithComps y &&
                                                                                    y.TryGetComp<CompFavoredObject>() != null);
            if (sculptures != null && sculptures.Count > 0)
            {
                foreach (var sculpture in sculptures)
                {
                    var compFavoredObject = sculpture.TryGetComp<CompFavoredObject>();
                    if (compFavoredObject?.Deities.FirstOrDefault(predicate: y => y.deityDef == deity.def.defName) != null)
                    {
                        statueOfDeityExists = true;
                    }

                    if (!sculpture.TryGetQuality(qc: out var qc))
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
                s.AppendLine(value: "Deity Statue Bonus: Sacrifice modifier + 10");
                reportFavorables.AppendLine(value: "+10 / 10: " + "Cults_LRDeityStatue".Translate(arg1: deity.LabelCap));
            }
            else
            {
                s.AppendLine(value: "No Deity Statue Bonus: Sacrifice modifier + 0");
                reportFavorables.AppendLine(value: "+ 0 / 10: " + "Cults_LRNoDeityStatue".Translate(arg1: deity.LabelCap));
            }

            if (qualityExists)
            {
                modifier += 10;
                s.AppendLine(value: "Quality Statue Bonus: Sacrifice modifier + 10");
                reportFavorables.AppendLine(value: "+10 / 10: " + "Cults_LRQualityStatue".Translate());
            }
            else
            {
                s.AppendLine(value: "No Quality Statue Bonus: Sacrifice modifier + 0");
                reportFavorables.AppendLine(value: "+ 0 / 10: " + "Cults_LRNoQualityStatue".Translate());
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
            var value = CongregationBonus(congregationIn: altar.SacrificeData.Congregation, entity: deity, perfect: out var perfect,
                sacrificialDagger: out var sacrificialDagger, s2: debugLog);
            if (value > 0)
            {
                modifier += 10;
                reportFavorables.AppendLine(value: "+10 / 10: " + "Cults_LRAttireBonus".Translate());
                debugLog.AppendLine(value: "Attire Bonus: Sacrifice modifier + 10");
            }
            else
            {
                reportFavorables.AppendLine(value: "+ 0 / 10: " + "Cults_LRNoAttireBonus".Translate());
                debugLog.AppendLine(value: "No Attire Bonus: Sacrifice modifier + 0");
            }

            if (sacrificialDagger)
            {
                modifier += 5;
                reportFavorables.AppendLine(value: "+ 5 /  5: " + "Cults_LRDaggerBonus".Translate());
                debugLog.AppendLine(value: "Dagger Bonus: Sacrifice modifier + 5");
            }
            else
            {
                reportFavorables.AppendLine(value: "+ 0 /   5: " + "Cults_LRNoDaggerBonus".Translate());
                debugLog.AppendLine(value: "No Dagger Bonus: Sacrifice modifier + 0");
            }

            if (perfect)
            {
                modifier += 15;
                reportFavorables.AppendLine(value: "+15 / 15: " + "Cults_LRPerfectBonus".Translate());
                debugLog.AppendLine(value: "Perfect Attire Bonus: Sacrifice modifier + 15");
            }
            else
            {
                reportFavorables.AppendLine(value: "+ 0 / 15: " + "Cults_LRNoPerfectBonus".Translate());
                debugLog.AppendLine(value: "No Perfect Attire Bonus: Sacrifice modifier + 0");
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

                    debugLog.AppendLine(value: current.defName + " is a tier 1 spell. No difficulty modifier added.");
                    reportFavorables.AppendLine(value: "+ 0 / 50: " + "Cults_LRSpellDifficultyOne".Translate());
                    goto GoToTheEnd;
                }

                //Is tier 2? +10% difficulty
                foreach (var current in deity.tier2Spells)
                {
                    if (current != spell)
                    {
                        continue;
                    }

                    debugLog.AppendLine(value: current.defName + " is a tier 2 spell. +10 sacrifice failure rate.");
                    modifier = 10;
                    reportUnfavorables.AppendLine(value: "+10 / 50: " + "Cults_LRSpellDifficultyTwo".Translate());
                    goto GoToTheEnd;
                }

                //Is tier 3? +20% difficulty
                foreach (var current in deity.tier3Spells)
                {
                    if (current != spell)
                    {
                        continue;
                    }

                    debugLog.AppendLine(value: current.defName + " is a tier 3 spell. +20% sacrifice failure rate.");
                    modifier = 20;
                    reportUnfavorables.AppendLine(value: "+20 / 50: " + "Cults_LRSpellDifficultyThree".Translate());
                    goto GoToTheEnd;
                }

                //Is final spell? +50% difficulty
                if (spell == deity.finalSpell)
                {
                    debugLog.AppendLine(value: spell.defName + " is a final spell. +50% sacrifice failure rate.");
                    modifier = 50;
                    reportUnfavorables.AppendLine(value: "+50 / 50: " + "Cults_LRSpellDifficultyFour".Translate());
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

            s.AppendLine(value: "Congregation Bonus Report");
            s.AppendLine(value: "=========================");
            s.AppendLine();
            var congregation = new List<Pawn>(collection: congregationIn);
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

                        var entry = deities.FirstOrDefault(predicate: x => x.deityDef == entity.def.defName);
                        if (entry == null)
                        {
                            continue;
                        }

                        result += entry.favorBonus;
                        if (entry.favorBonus != 0)
                        {
                            s.AppendLine(value: member.Label + " is wearing " + clothing.Label +
                                                " that gives a bonus of " + entry.favorBonus + " for " + entity.Label);
                        }

                        if (!wearingRobes && clothing.def.apparel.layers.Contains(item: ApparelLayerDefOf.Shell))
                        {
                            wearingRobes = true;
                        }

                        if (!wearingHood && clothing.def.apparel.layers.Contains(item: ApparelLayerDefOf.Overhead))
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
                        s.AppendLine(value: member.LabelShort + " is wielding a sacrificial dagger.");
                    }
                }

                if (wearingHood && wearingRobes)
                {
                    count++;
                    s.Append(value: member.LabelShort + " is perfectly attired for the congregation.");
                    s.AppendLine();
                }
                else
                {
                    s.Append(value: member.LabelShort + " is not perfectly attired for the congregation.");
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
                s.Append(value: "Perfect Bonus: +0.05");
                s.AppendLine();
                result += 0.05f;
            }

            s.AppendLine(value: "Congregation Bonus: " + result.ToString(format: "F"));
            s.AppendLine(value: "=========================");
            Utility.DebugReport(x: s.ToString());
            return result;
        }


        /// <summary>
        ///     When an execution completes, this method should trigger.
        /// </summary>
        /// <param name="altar"></param>
        public static void SacrificeExecutionComplete(Building_SacrificialAltar altar)
        {
            altar.ChangeState(type: Building_SacrificialAltar.State.sacrificing,
                sacrificeState: Building_SacrificialAltar.SacrificeState.finishing);
            var starsAreRight =
                altar.Map.GameConditionManager.GetActiveCondition<GameCondition_StarsAreRight>();
            var starsAreWrong =
                altar.Map.GameConditionManager.GetActiveCondition<GameCondition_StarsAreWrong>();
            var bstarsAreRight = starsAreRight != null;
            var bstarsAreWrong = starsAreWrong != null;

            altar.SacrificeData.Entity.ReceiveSacrifice(sacrifice: altar.SacrificeData.Sacrifice, map: altar.Map, favorSpell: bstarsAreRight,
                starsAreRight: bstarsAreWrong);

            var SuccessMod = Rand.Range(min: 0.03f, max: 0.035f);
            var FailureMod = Rand.Range(min: -0.035f, max: 0.03f);

            var tracker = altar.Map.GetComponent<MapComponent_SacrificeTracker>();
            if (tracker != null)
            {
                tracker.lastUsedAltar = altar;
                if (tracker.lastSacrificeType == SacrificeType.human)
                {
                    tracker.lastSpell = altar.SacrificeData.Spell;
                    tracker.lastResult = altar.debugAlwaysSucceed
                        ? SacrificeResult.success
                        : GetSacrificeResult(map: altar.Map);

                    var funTable = new CultTableOfFun();

                    var result = tracker.lastResult;
                    switch (result)
                    {
                        case SacrificeResult.success:
                            Utility.DebugReport(x: "Sacrifice: Success");
                            CastSpell(spell: altar.SacrificeData.Spell, map: altar.Map, fromAltar: true);
                            AffectCultMindedness(pawn: altar.SacrificeData.Executioner, amount: SuccessMod);
                            break;
                        case SacrificeResult.mixedsuccess:
                            Utility.DebugReport(x: "Sacrifice: Mixed Success");
                            CastSpell(spell: altar.SacrificeData.Spell, map: altar.Map, fromAltar: true);
                            AffectCultMindedness(pawn: altar.SacrificeData.Executioner, amount: SuccessMod);
                            funTable.RollTableOfFun(map: altar.Map);
                            break;
                        case SacrificeResult.failure:
                            Utility.DebugReport(x: "Sacrifice: Failure");
                            funTable.RollTableOfFun(map: altar.Map);
                            AffectCultMindedness(pawn: altar.SacrificeData.Executioner, amount: FailureMod);
                            SacrificeSpellComplete(executioner: altar.SacrificeData.Executioner, altar: altar);
                            break;
                        case SacrificeResult.criticalfailure:
                            Utility.DebugReport(x: "Sacrifice: Critical failure");
                            AffectCultMindedness(pawn: altar.SacrificeData.Executioner, amount: FailureMod);
                            SacrificeSpellComplete(executioner: altar.SacrificeData.Executioner, altar: altar);
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

                            if (GenSight.LineOfSight(start: prisoner.Position, end: altar.Position, map: altar.Map, skipFirstCell: true))
                            {
                                prisoner.needs?.mood.thoughts.memories.TryGainMemory(def: CultsDefOf
                                    .Cults_OtherPrisonerWasSacrificed);
                            }
                        }
                    }
                }

                //Tell the player!
                MakeSacrificeThoughts(attendee: altar.SacrificeData.Executioner, other: altar.SacrificeData.Sacrifice, isExcutioner: true);
                if (!altar.SacrificeData.Congregation.NullOrEmpty()
                ) //tracker.lastSacrificeCongregation != null && tracker.lastSacrificeCongregation.Count > 0)
                {
                    foreach (var pawn in altar.SacrificeData.Congregation)
                    {
                        if (!pawn.Spawned || pawn.Dead || pawn == altar.SacrificeData.Sacrifice)
                        {
                            continue;
                        }

                        TryGainTempleRoomThought(pawn: pawn);
                        if (pawn != altar.SacrificeData.Executioner)
                        {
                            MakeSacrificeThoughts(attendee: pawn, other: altar.SacrificeData.Sacrifice);
                        }
                    }
                }

                tracker.GenerateSacrificeMessage();
                altar.ChangeState(type: Building_SacrificialAltar.State.sacrificing,
                    sacrificeState: Building_SacrificialAltar.SacrificeState.finished);
            }

            //Increase the tally
            Find.World.GetComponent<WorldComponent_GlobalCultTracker>().numHumanSacrifices++;
        }

        public static void WorshipComplete(Pawn preacher, Building_SacrificialAltar altar, CosmicEntity deity)
        {
            altar.ChangeState(type: Building_SacrificialAltar.State.worshipping,
                worshipState: Building_SacrificialAltar.WorshipState.finishing);

            deity.ReceiveWorship(preacher: preacher);

            altar.ChangeState(type: Building_SacrificialAltar.State.worshipping,
                worshipState: Building_SacrificialAltar.WorshipState.finished);
            //altar.currentState = Building_SacrificialAltar.State.finished;

            var CultistMod = Rand.Range(min: 0.01f, max: 0.02f);
            AffectCultMindedness(pawn: preacher, amount: CultistMod);

            var factionBase = (Settlement) altar.Map.info.parent;

            Messages.Message(text: "WorshipFinished".Translate(arg1: factionBase.Label), lookTargets: TargetInfo.Invalid,
                def: MessageTypeDefOf.PositiveEvent);
        }

        public static void OfferingComplete(Pawn offerer, Building_SacrificialAltar altar, CosmicEntity deity,
            List<Thing> offering)
        {
            //altar.ChangeState(Building_SacrificialAltar.State.worshipping, Building_SacrificialAltar.WorshipState.finishing);

            altar.ChangeState(type: Building_SacrificialAltar.State.offering,
                offeringState: Building_SacrificialAltar.OfferingState.finished);
            deity.ReceiveOffering(offerer: offerer, altar: altar, offerings: offering);


            var CultistMod = Rand.Range(min: 0.01f, max: 0.02f);
            AffectCultMindedness(pawn: offerer, amount: CultistMod);

            if (Utility.IsActorAvailable(preacher: offerer))
            {
                var job = new Job(def: CultsDefOf.Cults_ReflectOnOffering)
                {
                    targetA = altar
                };
                offerer.jobs.TryTakeOrderedJob(job: job);
                //offerer.jobs.EndCurrentJob(JobCondition.InterruptForced);
            }

            var factionBase = (Settlement) altar.Map.info.parent;

            Messages.Message(text: "WorshipFinished".Translate(arg1: factionBase.Label), lookTargets: TargetInfo.Invalid,
                def: MessageTypeDefOf.PositiveEvent);
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
            Utility.DebugReport(x: "Cult Objects Check");
            //Do we have a forbidden knowledge center?
            if (AreForbiddenKnowledgeCentersAvailable(map: map))
            {
                Utility.DebugReport(x: "FKC Exists");
                return true;
            }

            //Do we have a book available?
            if (!AreOccultGrimoiresAvailable(map: map))
            {
                return false;
            }

            Utility.DebugReport(x: "Grimoire Exists");
            return true;
        }

        public static bool AreOccultGrimoiresAvailable(Map map)
        {
            return map?.listerThings.AllThings != null &&
                   map.listerThings.ThingsOfDef(def: ThingDef.Named(defName: "Cults_Grimoire")).Count > 0;
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

            var str2 = new string(value: Path.GetInvalidFileNameChars());
            var regex = new Regex(pattern: "[" + Regex.Escape(str: str2) + "]");
            return !regex.IsMatch(input: str);
        }

        public static bool IsPreacher(Pawn p)
        {
            if (p.Map == null) return false;

            var list = p.Map.listerThings.AllThings.FindAll(match: s => s.GetType() == typeof(Building_SacrificialAltar));
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

            var list = p.Map.listerThings.AllThings.FindAll(match: s => s.GetType() == typeof(Building_SacrificialAltar));
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
            
            var list = p.Map.listerThings.AllThings.FindAll(match: s => s.GetType() == typeof(Building_SacrificialAltar));
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
            s.Append(value: "ActorAvailble: Result = Unavailable");
            Utility.DebugReport(x: s.ToString());
            return false;
        }

        public static bool IsCultistAvailable(Pawn pawn)
        {
            if (!Utility.IsActorAvailable(preacher: pawn))
            {
                return false;
            }

            return IsCultMinded(pawn: pawn);
        }

        public static bool IsCultMinded(Pawn pawn)
        {
            if (pawn == null)
            {
                Utility.DebugReport(x: "IsCultMinded :: Pawn Null Exception");
                return false;
            }

            if (pawn.needs == null)
            {
                Utility.DebugReport(x: "IsCultMinded :: Pawn Needs Null Exception");
                return false;
            }

            if (pawn.needs.TryGetNeed<Need_CultMindedness>() != null)
            {
                return pawn.needs.TryGetNeed<Need_CultMindedness>().CurLevel > Need_CultMindedness.ThreshHigh;
            }

            Utility.DebugReport(x: "IsCultMinded :: Pawn has no cult mind");
            return false;
        }

        public static bool ShouldAttendSacrifice(Pawn p, Building_SacrificialAltar altar)
        {
            if (Utility.IsActorAvailable(preacher: altar.SacrificeData.Executioner))
            {
                return p != altar.SacrificeData.Executioner && p != altar.SacrificeData.Sacrifice;
            }

            AbortCongregation(altar: altar);
            return false;

            //Everyone get over here!
        }

        public static bool ShouldAttendWorship(Pawn p, Building_SacrificialAltar altar)
        {
            if (Utility.IsActorAvailable(preacher: altar.preacher))
            {
                return p != altar.preacher;
            }

            AbortCongregation(altar: altar);
            return false;

            //Everyone get over here!
        }

        public static void RemindPlayerAboutCongregationBonuses()
        {
            if (Rand.Range(min: 0, max: 100) < 20)
            {
                Messages.Message(text: "Tip: Wear cultist apparel for a worship bonus.", def: MessageTypeDefOf.SilentInput);
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
            result = Mathf.Clamp(value: result, min: 0.01f, max: trueMax);
            pawn.needs.TryGetNeed<Need_CultMindedness>().CurLevel = result;
        }

        public static void InvestigatedCultSeed(Pawn pawn, Thing investigatee)
        {
            //It's a day to remember
            var taleToAdd = TaleDef.Named(str: "ObservedNightmareMonolith");
            if (investigatee is Plant_TreeOfMadness)
            {
                taleToAdd = TaleDef.Named(str: "ObservedNightmareTree");
            }

            if ((pawn.IsColonist || pawn.HostFaction == Faction.OfPlayer) && taleToAdd != null)
            {
                TaleRecorder.RecordTale(def: taleToAdd, pawn);
            }

            //Internal memory
            pawn.needs.mood.thoughts.memories.TryGainMemory(def: CultsDefOf.Cults_MadeInvestigation);

            Utility.ApplySanityLoss(pawn: pawn);
            AffectCultMindedness(pawn: pawn, amount: 0.10f);
            pawn.Map.GetComponent<MapComponent_LocalCultTracker>().CurrentSeedState = CultSeedState.NeedWriting;
            pawn.Map.GetComponent<MapComponent_LocalCultTracker>().CurrentSeedPawn = pawn;
        }

        public static void FinishedTheBook(Pawn pawn)
        {
            pawn.needs.mood.thoughts.memories.TryGainMemory(def: CultsDefOf.Cults_BlackoutBook);
            Utility.ApplySanityLoss(pawn: pawn);
            pawn.Map.GetComponent<MapComponent_LocalCultTracker>().CurrentSeedState = CultSeedState.NeedTable;
            pawn.Map.GetComponent<MapComponent_LocalCultTracker>().CurrentSeedPawn = pawn;

            //Spawn in the book.
            var spawnLoc = pawn.Position + GenAdj.AdjacentCells[(int) Direction8Way.South];
            var unused = pawn.Map.GetComponent<MapComponent_LocalCultTracker>().CurrentSeedTarget;

            var thing = (ThingWithComps) ThingMaker.MakeThing(def: CultsDefOf.Cults_Grimoire);
            //thing.SetFaction(Faction.OfPlayer);
            GenPlace.TryPlaceThing(thing: thing, center: spawnLoc, map: pawn.Map, mode: ThingPlaceMode.Near);
            Find.WindowStack.Add(window: new Dialog_MessageBox(text: "CultBookSummary".Translate(arg1: pawn.Name.ToStringShort),
                buttonAText: "CultBookLabel".Translate()));
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

            TryGainTempleRoomThought(pawn: pawn);
            var newThought = GetAttendWorshipThoughts(preacher: preacher, attendee: pawn);
            if (newThought != null)
            {
                pawn.needs.mood.thoughts.memories.TryGainMemory(def: newThought);
            }
        }

        public static void HoldWorshipTickCheckEnd(Pawn preacher)
        {
            if (preacher == null)
            {
                return;
            }

            TryGainTempleRoomThought(pawn: preacher);
            AffectCultMindedness(pawn: preacher, amount: 0.1f);
            var newThought = CultsDefOf.Cults_HeldSermon; // DefDatabase<ThoughtDef>.GetNamed("HeldSermon");
            if (newThought != null)
            {
                preacher.needs.mood.thoughts.memories.TryGainMemory(def: newThought);
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
                        resultThought = IsCultMinded(pawn: attendee)
                            ? CultsDefOf.Cults_AttendedSuccessfulSacrifice
                            : CultsDefOf.Cults_InnocentAttendedSuccessfulSacrifice;

                        break;
                    case SacrificeResult.failure:
                    case SacrificeResult.criticalfailure:
                        resultThought = IsCultMinded(pawn: attendee)
                            ? CultsDefOf.Cults_AttendedFailedSacrifice
                            : CultsDefOf.Cults_InnocentAttendedFailedSacrifice;

                        break;
                    case SacrificeResult.none:
                        break;
                }

                attendee.needs.mood.thoughts.memories.TryGainMemory(def: resultThought);

                //Relationship Thoughts
                if (other == null)
                {
                    return;
                }
                //Family

                ThoughtDef familyThought = null;
                if (attendee.relations.FamilyByBlood.Contains(value: other))
                {
                    familyThought = isExcutioner ? CultsDefOf.Cults_ExecutedFamily : CultsDefOf.Cults_SacrificedFamily;
                }

                if (familyThought != null)
                {
                    attendee.needs.mood.thoughts.memories.TryGainMemory(def: familyThought);
                }

                //Friends and Rivals
                ThoughtDef relationThought = null;
                var num = attendee.relations.OpinionOf(other: other);
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
                    attendee.needs.mood.thoughts.memories.TryGainMemory(def: relationThought);
                }

                //Bloodlust
                if (!attendee.story.traits.HasTrait(tDef: TraitDefOf.Bloodlust))
                {
                    return;
                }

                attendee.needs.mood.thoughts.memories.TryGainMemory(
                    def: isExcutioner ? ThoughtDefOf.KilledHumanlikeBloodlust : ThoughtDefOf.WitnessedDeathBloodlust, otherPawn: other);
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
                if (other.relations.GetFirstDirectRelationPawn(def: PawnRelationDefOf.Bond) != attendee)
                {
                    return;
                }

                attendee.needs.mood.thoughts.memories.TryGainMemory(
                    def: isExcutioner ? CultsDefOf.Cults_ExecutedPet : CultsDefOf.Cults_SacrificedPet,
                    otherPawn: other);
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

            var CultistMod = Rand.Range(min: 0.01f, max: 0.02f);
            var InnocentMod = Rand.Range(min: -0.005f, max: 0.1f);

            if (attendee == null)
            {
                return null;
            }

            var num = preacher.skills.GetSkill(skillDef: SkillDefOf.Social).Level;
            num += Rand.Range(min: -6, max: 6); //Randomness


            switch (num)
            {
                //S-Ranked Sermon: WOW!
                case > 20 when IsCultMinded(pawn: attendee):
                    AffectCultMindedness(pawn: attendee, amount: S_Effect + CultistMod);
                    return CultsDefOf.Cults_AttendedIncredibleSermonAsCultist;
                case > 20:
                    AffectCultMindedness(pawn: attendee, amount: S_Effect + InnocentMod);
                    return CultsDefOf.Cults_AttendedIncredibleSermonAsInnocent;
                //A-Ranked Sermon: Fantastic
                case <= 20 and > 15 when IsCultMinded(pawn: attendee):
                    AffectCultMindedness(pawn: attendee, amount: A_Effect + CultistMod);
                    return CultsDefOf.Cults_AttendedGreatSermonAsCultist;
                case <= 20 and > 15:
                    AffectCultMindedness(pawn: attendee, amount: A_Effect + InnocentMod);
                    return CultsDefOf.Cults_AttendedGreatSermonAsInnocent;
                //B-Ranked Sermon: Alright
                case <= 15 and > 10 when IsCultMinded(pawn: attendee):
                    AffectCultMindedness(pawn: attendee, amount: B_Effect + CultistMod);
                    return CultsDefOf.Cults_AttendedGoodSermonAsCultist;
                case <= 15 and > 10:
                    AffectCultMindedness(pawn: attendee, amount: B_Effect + InnocentMod);
                    return CultsDefOf.Cults_AttendedGoodSermonAsInnocent;
                //C-Ranked Sermon: Average
                case <= 10 and > 5 when IsCultMinded(pawn: attendee):
                    AffectCultMindedness(pawn: attendee, amount: C_Effect + CultistMod);
                    return CultsDefOf.Cults_AttendedDecentSermonAsCultist;
                case <= 10 and > 5:
                    AffectCultMindedness(pawn: attendee, amount: C_Effect + InnocentMod);
                    return CultsDefOf.Cults_AttendedDecentSermonAsInnocent;
            }

            //F-Ranked Sermon: Garbage
            if (IsCultMinded(pawn: attendee))
            {
                AffectCultMindedness(pawn: attendee, amount: F_Effect + CultistMod);
                return CultsDefOf.Cults_AttendedAwfulSermonAsCultist;
            }

            AffectCultMindedness(pawn: attendee, amount: F_Effect + InnocentMod);
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
                RoomStatDefOf.Impressiveness.GetScoreStageIndex(score: room.GetStat(roomStat: RoomStatDefOf.Impressiveness));
            if (def.stages[index: scoreStageIndex] == null)
            {
                return;
            }

            pawn.needs.mood.thoughts.memories.TryGainMemory(newThought: ThoughtMaker.MakeThought(def: def, forcedStage: scoreStageIndex));
        }

        public static void GiveAttendSacrificeJob(Building_SacrificialAltar altar, Pawn attendee)
        {
            if (IsExecutioner(p: attendee))
            {
                return;
            }

            if (IsSacrifice(p: attendee))
            {
                return;
            }

            if (!Utility.IsActorAvailable(preacher: attendee))
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

            if (!WatchBuildingUtility.TryFindBestWatchCell(toWatch: altar, pawn: attendee, desireSit: true, result: out var result, chair: out var chair))
            {
                if (!WatchBuildingUtility.TryFindBestWatchCell(toWatch: altar, pawn: attendee, desireSit: false, result: out result, chair: out chair))
                {
                    return;
                }
            }

            var dir = altar.Rotation.Opposite.AsInt;

            if (chair != null)
            {
                var newPos = chair.Position + GenAdj.CardinalDirections[dir];

                var J = new Job(def: CultsDefOf.Cults_AttendSacrifice, targetA: altar, targetB: newPos, targetC: chair)
                {
                    playerForced = true,
                    ignoreJoyTimeAssignment = true,
                    expiryInterval = 9999,
                    ignoreDesignations = true,
                    ignoreForbidden = true
                };
                //Cthulhu.Utility.DebugReport("Cults :: Original Position " + chair.Position.ToString() + " :: Modded Position " + newPos.ToString());
                attendee.jobs.TryTakeOrderedJob(job: J);
                //attendee.jobs.EndCurrentJob(JobCondition.Incompletable);
            }
            else
            {
                var newPos = result + GenAdj.CardinalDirections[dir];

                var J = new Job(def: CultsDefOf.Cults_AttendSacrifice, targetA: altar, targetB: newPos, targetC: result)
                {
                    playerForced = true,
                    ignoreJoyTimeAssignment = true,
                    expiryInterval = 9999,
                    ignoreDesignations = true,
                    ignoreForbidden = true
                };
                //Cthulhu.Utility.DebugReport("Cults :: Original Position " + result.ToString() + " :: Modded Position " + newPos.ToString());
                attendee.jobs.TryTakeOrderedJob(job: J);
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

                if (Utility.GetResearchSkill(p: result) < Utility.GetResearchSkill(p: p))
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

                if (IsCultMinded(pawn: p) && Utility.GetSocialSkill(p: result) < Utility.GetSocialSkill(p: p))
                {
                    result = p;
                }
            }

            if (!IsCultMinded(pawn: result))
            {
                result = null;
            }

            return result;
        }


        public static bool ShouldAttendeeKeepAttendingWorship(Pawn p)
        {
            return !p.Downed && (p?.needs == null || p?.needs?.food?.Starving != true) && p?.health?.hediffSet?.BleedRateTotal <= 0f && (p?.needs?.rest == null || p?.needs?.rest?.CurCategory < RestCategory.Exhausted) && p?.health?.hediffSet?.HasTendableNonInjuryNonMissingPartHediff(forAlert: false) != true && p?.Awake() == true && p?.InAggroMentalState != true && p?.IsPrisoner != true;
        }

        //Checkyoself
        public static void GiveAttendWorshipJob(Building_SacrificialAltar altar, Pawn attendee)
        {
            //Log.Message("1");
            if (IsPreacher(p: attendee))
            {
                return;
            }

            if (!ShouldAttendeeKeepAttendingWorship(p: attendee))
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

            if (!WatchBuildingUtility.TryFindBestWatchCell(toWatch: altar, pawn: attendee, desireSit: true, result: out var result, chair: out var chair))
            {
                if (!WatchBuildingUtility.TryFindBestWatchCell(toWatch: altar, pawn: attendee, desireSit: false, result: out result, chair: out chair))
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

                attendJob = new Job(def: CultsDefOf.Cults_AttendWorship, targetA: altar, targetB: newPos, targetC: chair)
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

                attendJob = new Job(def: CultsDefOf.Cults_AttendWorship, targetA: altar, targetB: newPos, targetC: result)
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
                attendee.jobs.EndCurrentJob(condition: JobCondition.Incompletable);
                attendee.jobs.TryTakeOrderedJob(job: attendJob);
            }
            else
            {
                attendee.jobs.jobQueue.EnqueueLast(j: attendJob);
                if (attendee.CurJobDef?.defName.Contains(value: "Haul") != true)
                {
                    attendee.jobs.EndCurrentJob(condition: JobCondition.Incompletable);
                }
            }
        }

        public static void AbortCongregation(Building_SacrificialAltar altar)
        {
            altar?.ChangeState(type: Building_SacrificialAltar.State.notinuse);
        }

        public static void AbortCongregation(Building_SacrificialAltar altar, string reason)
        {
            altar?.ChangeState(type: Building_SacrificialAltar.State.notinuse);

            Messages.Message(text: reason + " Aborting congregation.", def: MessageTypeDefOf.NegativeEvent);
        }

        public static void CastSpell(IncidentDef spell, Map map, bool fromAltar = false)
        {
            if (spell != null)
            {
                var parms = StorytellerUtility.DefaultParmsNow(incCat: spell.category, target: map);
                spell.Worker.TryExecute(parms: parms);
                Utility.DebugReport(x: "Cults_Spell cast: " + spell);
            }

            if (!fromAltar)
            {
                return;
            }

            var lastAltar = map.GetComponent<MapComponent_SacrificeTracker>().lastUsedAltar;
            SacrificeSpellComplete(executioner: lastAltar.SacrificeData.Executioner, altar: lastAltar);
        }

        public static void SacrificeSpellComplete(Pawn executioner, Building_SacrificialAltar altar)
        {
            if (altar == null)
            {
                Utility.DebugReport(x: "Altar Null Exception");
                return;
            }

            if (executioner == null)
            {
                Utility.DebugReport(x: "Executioner null exception");
            }

            if (Utility.IsActorAvailable(preacher: executioner))
            {
                var job = new Job(def: CultsDefOf.Cults_ReflectOnResult)
                {
                    targetA = altar
                };
                executioner?.jobs.TryTakeOrderedJob(job: job);
                //executioner.jobs.EndCurrentJob(JobCondition.InterruptForced);
            }

            altar.ChangeState(type: Building_SacrificialAltar.State.sacrificing,
                sacrificeState: Building_SacrificialAltar.SacrificeState.finished);
            //altar.currentState = Building_SacrificialAltar.State.finished;
            //Map.GetComponent<MapComponent_SacrificeTracker>().lastUsedAltar.ChangeState(Building_SacrificialAltar.State.sacrificing, Building_SacrificialAltar.SacrificeState.finished);
        }
    }
}