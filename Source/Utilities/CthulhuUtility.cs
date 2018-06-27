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
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

/// <summary>
/// Utility File for use between Cthulhu mods.
/// Last Update: 6/8/2017
/// </summary>
namespace Cthulhu
{
    public static class ModProps
    {
        public static string main = "Cthulhu";
        public static string mod = "Cults";
        public static string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }

    public static class SanityLossSeverity
    {
        public const float Initial = 0.1f;
        public const float Minor = 0.25f;
        public const float Major = 0.5f;
        public const float Severe = 0.7f;
        public const float Extreme = 0.95f;
    }

    static public class Utility
    {
        public enum SanLossSev { None = 0, Hidden, Initial, Minor, Major, Extreme };
        public const string SanityLossDef = "ROM_SanityLoss";
        public const string AltSanityLossDef = "Cults_SanityLoss";

        public static bool modCheck = false;
        public static bool loadedCosmicHorrors = false;
        public static bool loadedIndustrialAge = false;
        public static bool loadedCults = false;
        public static bool loadedFactions = false;


        public static bool IsMorning(Map map) =>
            GenLocalDate.HourInteger(map) > 6 && GenLocalDate.HourInteger(map) < 10; public static bool IsEvening(Map map) => GenLocalDate.HourInteger(map) > 18 && GenLocalDate.HourInteger(map) < 22; public static bool IsNight(Map map) => GenLocalDate.HourInteger(map) > 22;
        public static T GetMod<T>(string s) where T : Mod
        {
            //Call of Cthulhu - Cosmic Horrors
            T result = default(T);
            foreach (Mod ResolvedMod in LoadedModManager.ModHandles)
            {
                if (ResolvedMod.Content.Name == s) result = ResolvedMod as T;
            }
            return result;
        }

        public static bool IsCosmicHorror(Pawn thing)
        {
            if (!IsCosmicHorrorsLoaded()) return false;

            Type type = Type.GetType("CosmicHorror.CosmicHorrorPawn");
            if (type != null)
            {
                if (thing.GetType() == type)
                {
                    return true;
                }
            }
            return false;
        }

        //public static float GetSanityLossRate(PawnKindDef kindDef)
        //{
        //    float sanityLossRate = 0f;
        //    if (kindDef.ToString() == "ROM_StarVampire")
        //        sanityLossRate = 0.04f;
        //    if (kindDef.ToString() == "StarSpawnOfCthulhu")
        //        sanityLossRate = 0.02f;
        //    if (kindDef.ToString() == "DarkYoung")
        //        sanityLossRate = 0.004f;
        //    if (kindDef.ToString() == "DeepOne")
        //        sanityLossRate = 0.008f;
        //    if (kindDef.ToString() == "DeepOneGreat")
        //        sanityLossRate = 0.012f;
        //    if (kindDef.ToString() == "MiGo")
        //        sanityLossRate = 0.008f;
        //    if (kindDef.ToString() == "Shoggoth")
        //        sanityLossRate = 0.012f;
        //    return sanityLossRate;
        //}

        public static bool CapableOfViolence(Pawn pawn, bool allowDowned = false)
        {
            if (pawn == null) return false;
            if (pawn.Dead) return false;
            if (pawn.Downed && !allowDowned) return false;
            if (pawn.story.WorkTagIsDisabled(WorkTags.Violent)) return false;
            return true;
        }

        public static bool IsActorAvailable(Pawn preacher, bool downedAllowed = false)
        {
            StringBuilder s = new StringBuilder();
            s.Append("ActorAvailble Checks Initiated");
            s.AppendLine();
            if (preacher == null)
                return ResultFalseWithReport(s);
            s.Append("ActorAvailble: Passed null Check");
            s.AppendLine();
            //if (!preacher.Spawned)
            //    return ResultFalseWithReport(s);
            //s.Append("ActorAvailble: Passed not-spawned check");
            //s.AppendLine();
            if (preacher.Dead)
                return ResultFalseWithReport(s);
            s.Append("ActorAvailble: Passed not-dead");
            s.AppendLine();
            if (preacher.Downed && !downedAllowed)
                return ResultFalseWithReport(s);
            s.Append("ActorAvailble: Passed downed check & downedAllowed = " + downedAllowed.ToString());
            s.AppendLine();
            if (preacher.Drafted)
                return ResultFalseWithReport(s);
            s.Append("ActorAvailble: Passed drafted check");
            s.AppendLine();
            if (preacher.InAggroMentalState)
                return ResultFalseWithReport(s);
            s.Append("ActorAvailble: Passed drafted check");
            s.AppendLine();
            if (preacher.InMentalState)
                return ResultFalseWithReport(s);
            s.Append("ActorAvailble: Passed InMentalState check");
            s.AppendLine();
            s.Append("ActorAvailble Checks Passed");
            Cthulhu.Utility.DebugReport(s.ToString());
            return true;
        }

        public static bool ResultFalseWithReport(StringBuilder s)
        {
            s.Append("ActorAvailble: Result = Unavailable");
            Cthulhu.Utility.DebugReport(s.ToString());
            return false;
        }

        static public Pawn GenerateNewPawnFromSource(ThingDef newDef, Pawn sourcePawn)
        {
            Pawn pawn = (Pawn)ThingMaker.MakeThing(newDef);
            //Cthulhu.Utility.DebugReport("Declare a new thing");
            pawn.Name = sourcePawn.Name;
            //Cthulhu.Utility.DebugReport("The name!");
            pawn.SetFactionDirect(Faction.OfPlayer);
            pawn.kindDef = sourcePawn.kindDef;
            //Cthulhu.Utility.DebugReport("The def!");
            pawn.pather = new Pawn_PathFollower(pawn);
            //Cthulhu.Utility.DebugReport("The pather!");
            pawn.ageTracker = new Pawn_AgeTracker(pawn);
            pawn.health = new Pawn_HealthTracker(pawn);
            pawn.jobs = new Pawn_JobTracker(pawn);
            pawn.mindState = new Pawn_MindState(pawn);
            pawn.filth = new Pawn_FilthTracker(pawn);
            pawn.needs = new Pawn_NeedsTracker(pawn);
            pawn.stances = new Pawn_StanceTracker(pawn);
            pawn.natives = new Pawn_NativeVerbs(pawn);
            pawn.relations = sourcePawn.relations;
            PawnComponentsUtility.CreateInitialComponents(pawn);

            if (pawn.RaceProps.ToolUser)
            {
                pawn.equipment = new Pawn_EquipmentTracker(pawn);
                pawn.carryTracker = new Pawn_CarryTracker(pawn);
                pawn.apparel = new Pawn_ApparelTracker(pawn);
                pawn.inventory = new Pawn_InventoryTracker(pawn);
            }
            if (pawn.RaceProps.intelligence <= Intelligence.ToolUser)
            {
                pawn.caller = new Pawn_CallTracker(pawn);
            }
            pawn.gender = sourcePawn.gender;
            pawn.needs.SetInitialLevels();
            GenerateRandomAge(pawn, sourcePawn.Map);
            CopyPawnRecords(sourcePawn, pawn);
            //Cthulhu.Utility.DebugReport("We got so far.");
            return pawn;
        }

        static public void CopyPawnRecords(Pawn pawn, Pawn newPawn)
        {
            //Who has a relationship with this pet?
            Pawn pawnMaster = null;
            Map map = pawn.Map;
            foreach (Pawn current in map.mapPawns.AllPawns)
            {
                if (current.relations.DirectRelationExists(PawnRelationDefOf.Bond, pawn))
                {
                    pawnMaster = current;
                }
            }

            //Fix the relations
            if (pawnMaster != null)
            {
                pawnMaster.relations.TryRemoveDirectRelation(PawnRelationDefOf.Bond, pawn);
                pawnMaster.relations.AddDirectRelation(PawnRelationDefOf.Bond, newPawn);
                //Train that stuff!

                DefMap<TrainableDef, int> oldMap = (DefMap<TrainableDef, int>)typeof(Pawn_TrainingTracker).GetField("steps", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(pawn.training);
                DefMap<TrainableDef, int> newMap = (DefMap<TrainableDef, int>)typeof(Pawn_TrainingTracker).GetField("steps", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(newPawn.training);

                foreach (TrainableDef def in DefDatabase<TrainableDef>.AllDefs)
                {
                    newMap[def] = oldMap[def];
                }
            }


            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                newPawn.health.AddHediff(hediff);
            }

        }

        static public void GenerateRandomAge(Pawn pawn, Map map)
        {
            int num = 0;
            int num2;
            do
            {
                if (pawn.RaceProps.ageGenerationCurve != null)
                {
                    num2 = Mathf.RoundToInt(Rand.ByCurve(pawn.RaceProps.ageGenerationCurve));
                }
                else if (pawn.RaceProps.IsMechanoid)
                {
                    num2 = Rand.Range(0, 2500);
                }
                else
                {
                    if (!pawn.RaceProps.Animal)
                    {
                        goto IL_84;
                    }
                    num2 = Rand.Range(1, 10);
                }
                num++;
                if (num > 100)
                {
                    goto IL_95;
                }
            }
            while (num2 > pawn.kindDef.maxGenerationAge || num2 < pawn.kindDef.minGenerationAge);
            goto IL_A5;
            IL_84:
            Log.Warning("Didn't get age for " + pawn);
            return;
            IL_95:
            Log.Error("Tried 100 times to generate age for " + pawn);
            IL_A5:
            pawn.ageTracker.AgeBiologicalTicks = ((long)(num2 * 3600000f) + Rand.Range(0, 3600000));
            int num3;
            if (Rand.Value < pawn.kindDef.backstoryCryptosleepCommonality)
            {
                float value = Rand.Value;
                if (value < 0.7f)
                {
                    num3 = Rand.Range(0, 100);
                }
                else if (value < 0.95f)
                {
                    num3 = Rand.Range(100, 1000);
                }
                else
                {
                    int num4 = GenLocalDate.Year(map) - 2026 - pawn.ageTracker.AgeBiologicalYears;
                    num3 = Rand.Range(1000, num4);
                }
            }
            else
            {
                num3 = 0;
            }
            long num5 = GenTicks.TicksAbs - pawn.ageTracker.AgeBiologicalTicks;
            num5 -= num3 * 3600000L;
            pawn.ageTracker.BirthAbsTicks = num5;
            if (pawn.ageTracker.AgeBiologicalTicks > pawn.ageTracker.AgeChronologicalTicks)
            {
                pawn.ageTracker.AgeChronologicalTicks = (pawn.ageTracker.AgeBiologicalTicks);
            }
        }


        /// <summary>
        /// A very complicated method for finding a proper place for objects to spawn in Cthulhu Utility.
        /// </summary>
        /// <param name="def"></param>
        /// <param name="nearLoc"></param>
        /// <param name="map"></param>
        /// <param name="maxDist"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static bool TryFindSpawnCell(ThingDef def, IntVec3 nearLoc, Map map, int maxDist, out IntVec3 pos) =>
            CellFinder.TryFindRandomCellNear(nearLoc, map, maxDist, delegate (IntVec3 x)
            {
                ///Check if the entire area is safe based on the size of the object definition.
                foreach (IntVec3 current in GenAdj.OccupiedRect(x, Rot4.North, new IntVec2(def.size.x + 2, def.size.z + 2)))
                {
                    if (!current.InBounds(map) || current.Fogged(map) || !current.Standable(map) || (current.Roofed(map) && current.GetRoof(map).isThickRoof))
                    {
                        return false;
                    }
                    if (!current.SupportsStructureType(map, def.terrainAffordanceNeeded))
                    {
                        return false;
                    }

                    ///
                    //  If it has an interaction cell, check to see if it can be reached by colonists.
                    //
                    bool intCanBeReached = true;
                    if (def.interactionCellOffset != IntVec3.Zero)
                    {
                        foreach (Pawn colonist in map.mapPawns.FreeColonistsSpawned)
                        {
                            if (!colonist.CanReach(current + def.interactionCellOffset, PathEndMode.ClosestTouch, Danger.Deadly))
                                intCanBeReached = false;
                        }
                    }
                    if (!intCanBeReached)
                        return false;
                    //

                    //Don't wipe existing objets...
                    List<Thing> thingList = current.GetThingList(map);
                    for (int i = 0; i < thingList.Count; i++)
                    {
                        Thing thing = thingList[i];
                        if (thing.def.category != ThingCategory.Plant && GenSpawn.SpawningWipes(def, thing.def))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }, out pos);

        public static BodyPartRecord GetMouth(HediffSet set)
        {
            foreach (BodyPartRecord current in set.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined))
            {
                for (int i = 0; i < current.def.tags.Count; i++)
                {
                    if (current.def.defName == "TalkingSource")
                    {
                        return current;
                    }
                }
            }
            return null;
        }
        public static BodyPartRecord GetEar(HediffSet set)
        {
            foreach (BodyPartRecord current in set.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined))
            {
                for (int i = 0; i < current.def.tags.Count; i++)
                {
                    if (current.def.defName == "HearingSource")
                    {
                        return current;
                    }
                }
            }
            return null;
        }

        public static BodyPartRecord GetNose(HediffSet set)
        {
            foreach (BodyPartRecord current in set.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined))
            {
                for (int i = 0; i < current.def.tags.Count; i++)
                {
                    if (current.def.defName == "Nose")
                    {
                        return current;
                    }
                }
            }
            return null;
        }

        public static BodyPartRecord GetEye(HediffSet set)
        {
            foreach (BodyPartRecord current in set.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined))
            {
                for (int i = 0; i < current.def.tags.Count; i++)
                {
                    if (current.def.tags[i].defName == "BloodPumpingSource")
                    {
                        return current;
                    }
                }
            }
            return null;
        }

        public static BodyPartRecord GetHeart(HediffSet set)
        {
            foreach (BodyPartRecord current in set.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined))
            {
                for (int i = 0; i < current.def.tags.Count; i++)
                {
                    if (current.def.tags[i].defName == "BloodPumpingSource")
                    {
                        return current;
                    }
                }
            }
            return null;
        }


        public static void SpawnThingDefOfCountAt(ThingDef of, int count, TargetInfo target)
        {
            while (count > 0)
            {
                Thing thing = ThingMaker.MakeThing(of, null);

                thing.stackCount = Math.Min(count, of.stackLimit);
                GenPlace.TryPlaceThing(thing, target.Cell, target.Map, ThingPlaceMode.Near);
                count -= thing.stackCount;
            }
        }

        public static void SpawnPawnsOfCountAt(PawnKindDef kindDef, IntVec3 at, Map map, int count, out Pawn returnable, Faction fac = null, bool berserk = false, bool target = false)
        {
            Pawn result = null;
            for (int i = 1; i <= count; i++)
            {
                if ((from cell in GenAdj.CellsAdjacent8Way(new TargetInfo(at, map))
                     where at.Walkable(map)
                     select cell).TryRandomElement(out at))
                {
                    Pawn pawn = PawnGenerator.GeneratePawn(kindDef, fac);
                    if (result == null) result = pawn;
                    if (GenPlace.TryPlaceThing(pawn, at, map, ThingPlaceMode.Near, null))
                    {
                        //if (target) Map.GetComponent<MapComponent_SacrificeTracker>().lastLocation = at;
                        //continue;
                    }
                    //Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
                    if (berserk) pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk);
                }
            }
            returnable = result;
        }

        public static void SpawnPawnsOfCountAt(PawnKindDef kindDef, IntVec3 at, Map map, int count, Faction fac = null, bool berserk = false, bool target = false)
        {
            for (int i = 1; i <= count; i++)
            {
                if ((from cell in GenAdj.CellsAdjacent8Way(new TargetInfo(at, map))
                     where at.Walkable(map)
                     select cell).TryRandomElement(out at))
                {

                    Pawn pawn = PawnGenerator.GeneratePawn(kindDef, fac);
                    if (GenPlace.TryPlaceThing(pawn, at, map, ThingPlaceMode.Near, null))
                    {
                        //if (target) Map.GetComponent<MapComponent_SacrificeTracker>().lastLocation = at;
                        //continue;
                    }
                    //Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
                    if (berserk) pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk);
                }
            }
        }

        public static bool TryGetUnreservedPewSpot(Thing pew, Pawn claimer, out IntVec3 loc)
        {
            loc = IntVec3.Invalid;

            Map map = pew.Map;
            Rot4 currentDirection = pew.Rotation;

            IntVec3 CellNorth = pew.Position + GenAdj.CardinalDirections[Rot4.North.AsInt];
            IntVec3 CellSouth = pew.Position + GenAdj.CardinalDirections[Rot4.South.AsInt];
            IntVec3 CellEast = pew.Position + GenAdj.CardinalDirections[Rot4.East.AsInt];
            IntVec3 CellWest = pew.Position + GenAdj.CardinalDirections[Rot4.West.AsInt];

            if (claimer.CanReserve(pew.Position)) //!map.reservationManager.IsReserved(pew.Position, Faction.OfPlayer))
                { loc = pew.Position; return true; }

            if (currentDirection == Rot4.North ||
                currentDirection == Rot4.South)
            {
                if (claimer.CanReserve(CellWest))// !map.reservationManager.IsReserved(CellWest, Faction.OfPlayer)) 
                { loc = CellWest; return true; }
                if (claimer.CanReserve(CellEast))//!map.reservationManager.IsReserved(CellEast, Faction.OfPlayer)) 
                { loc = CellEast; return true; }
            }
            if (currentDirection == Rot4.East ||
                currentDirection == Rot4.West)
            {
                if (claimer.CanReserve(CellNorth))//!map.reservationManager.IsReserved(CellNorth, Faction.OfPlayer)) 
                { loc = CellNorth; return true; }
                if (claimer.CanReserve(CellSouth))//!map.reservationManager.IsReserved(CellSouth, Faction.OfPlayer)) 
                { loc = CellSouth; return true; }
            }
            //map.reservationManager.Reserve(claimer, pew);
            return false;
        }


        public static void ChangeResearchProgress(ResearchProjectDef projectDef, float progressValue, bool deselectCurrentResearch = false)
        {
            FieldInfo researchProgressInfo = typeof(ResearchManager).GetField("progress", BindingFlags.Instance | BindingFlags.NonPublic);
            object researchProgress = researchProgressInfo.GetValue(Find.ResearchManager);
            PropertyInfo itemPropertyInfo = researchProgress.GetType().GetProperty("Item");
            itemPropertyInfo.SetValue(researchProgress, progressValue, new[] { projectDef });
            if (deselectCurrentResearch) Find.ResearchManager.currentProj = null;
            Find.ResearchManager.ReapplyAllMods();
        }

        public static float CurrentSanityLoss(Pawn pawn)
        {
            string sanityLossDef;
            sanityLossDef = AltSanityLossDef;
            if (IsCosmicHorrorsLoaded()) sanityLossDef = SanityLossDef;

            Hediff pawnSanityHediff = pawn.health.hediffSet.GetFirstHediffOfDef(DefDatabase<HediffDef>.GetNamed(sanityLossDef));
            if (pawnSanityHediff != null)
            {
                return pawnSanityHediff.Severity;
            }
            return 0f;
        }


        public static void ApplyTaleDef(string defName, Map map)
        {
            Pawn randomPawn = map.mapPawns.FreeColonists.RandomElement<Pawn>();
            TaleDef taleToAdd = TaleDef.Named(defName);
            TaleRecorder.RecordTale(taleToAdd, new object[]
                    {
                        randomPawn,
                    });
        }

        public static void ApplyTaleDef(string defName, Pawn pawn)
        {
            TaleDef taleToAdd = TaleDef.Named(defName);
            if ((pawn.IsColonist || pawn.HostFaction == Faction.OfPlayer) && taleToAdd != null)
            {
                TaleRecorder.RecordTale(taleToAdd, new object[]
                {
                    pawn,
                });
            }
        }


        public static bool HasSanityLoss(Pawn pawn)
        {
            string sanityLossDef = (!IsCosmicHorrorsLoaded()) ? AltSanityLossDef : SanityLossDef;
            var pawnSanityHediff = pawn.health.hediffSet.GetFirstHediffOfDef(DefDatabase<HediffDef>.GetNamed(sanityLossDef));

            return pawnSanityHediff != null;
        }

        /// <summary>
        /// This method handles the application of Sanity Loss in multiple mods.
        /// It returns true and false depending on if it applies successfully.
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="sanityLoss"></param>
        /// <param name="sanityLossMax"></param>
        public static bool ApplySanityLoss(Pawn pawn, float sanityLoss = 0.3f, float sanityLossMax = 1.0f)
        {
            bool appliedSuccessfully = false;
            if (pawn != null)
            {
                string sanityLossDef = (!IsCosmicHorrorsLoaded()) ? AltSanityLossDef : SanityLossDef;

                var pawnSanityHediff = pawn.health.hediffSet.GetFirstHediffOfDef(DefDatabase<HediffDef>.GetNamedSilentFail(sanityLossDef));
                if (pawnSanityHediff != null)
                {
                    if (pawnSanityHediff.Severity > sanityLossMax) sanityLossMax = pawnSanityHediff.Severity;
                    float result = pawnSanityHediff.Severity;
                    result += sanityLoss;
                    result = Mathf.Clamp(result, 0.0f, sanityLossMax);
                    pawnSanityHediff.Severity = result;
                    appliedSuccessfully = true;
                }
                else if (sanityLoss > 0)
                {
                    var sanityLossHediff = HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamedSilentFail(sanityLossDef), pawn, null);
                    if (sanityLossHediff != null)
                    {
                        sanityLossHediff.Severity = sanityLoss;
                        pawn.health.AddHediff(sanityLossHediff, null, null);
                        appliedSuccessfully = true;
                    }
                }
            }
            return appliedSuccessfully;
        }


        public static int GetSocialSkill(Pawn p) => p.skills.GetSkill(SkillDefOf.Social).Level;

        public static int GetResearchSkill(Pawn p) => p.skills.GetSkill(SkillDefOf.Intellectual).Level;

        public static bool IsCosmicHorrorsLoaded()
        {

            if (!modCheck) ModCheck();
            return loadedCosmicHorrors;
        }


        public static bool IsIndustrialAgeLoaded()
        {
            if (!modCheck) ModCheck();
            return loadedIndustrialAge;
        }



        public static bool IsCultsLoaded()
        {
            if (!modCheck) ModCheck();
            return loadedCults;
        }

        public static bool IsRandomWalkable8WayAdjacentOf(IntVec3 cell, Map map, out IntVec3 resultCell)
        {
            if (cell != IntVec3.Invalid)
            {
                IntVec3 temp = cell.RandomAdjacentCell8Way();
                if (map != null)
                {
                    for (int i = 0; i < 100; i++)
                    {
                        temp = cell.RandomAdjacentCell8Way();
                        if (temp.Walkable(map))
                        {
                            resultCell = temp;
                            return true;
                        }
                    }
                }
            }
            resultCell = IntVec3.Invalid;
            return false;
        }

        public static void TemporaryGoodwill(Faction faction, bool reset = false)
        {
            Faction playerFaction = Faction.OfPlayer;
            if (!reset)
            {
                if (faction.GoodwillWith(playerFaction) == 0f)
                {
                    faction.RelationWith(playerFaction, false).goodwill = faction.PlayerGoodwill;
                }

                faction.RelationWith(playerFaction, false).goodwill = 100;
                faction.TrySetRelationKind(playerFaction, FactionRelationKind.Neutral);
            }
            else
            {
                faction.RelationWith(playerFaction, false).goodwill = 0;
                faction.TrySetRelationKind(playerFaction, FactionRelationKind.Hostile);
                //faction.RelationWith(playerFaction, false).hostile = true;
            }
        }


        public static void ModCheck()
        {
            loadedCosmicHorrors = false;
            loadedIndustrialAge = false;
            foreach (ModContentPack ResolvedMod in LoadedModManager.RunningMods)
            {
                if (loadedCosmicHorrors && loadedIndustrialAge && loadedCults) break; //Save some loading
                if (ResolvedMod.Name.Contains("Call of Cthulhu - Cosmic Horrors"))
                {
                    DebugReport("Loaded - Call of Cthulhu - Cosmic Horrors");
                    loadedCosmicHorrors = true;
                }
                if (ResolvedMod.Name.Contains("Call of Cthulhu - Industrial Age"))
                {
                    DebugReport("Loaded - Call of Cthulhu - Industrial Age");
                    loadedIndustrialAge = true;
                }
                if (ResolvedMod.Name.Contains("Call of Cthulhu - Cults"))
                {
                    DebugReport("Loaded - Call of Cthulhu - Cults");
                    loadedCults = true;
                }
                if (ResolvedMod.Name.Contains("Call of Cthulhu - Factions"))
                {
                    DebugReport("Loaded - Call of Cthulhu - Factions");
                    loadedFactions = true;
                }
            }
            modCheck = true;
            return;
        }

        public static string Prefix => ModProps.main + " :: " + ModProps.mod + " " + ModProps.Version + " :: ";

        public static void DebugReport(string x)
        {
            if (Prefs.DevMode && DebugSettings.godMode)
            {
                Log.Message(Prefix + x);
            }
        }

        public static void ErrorReport(string x) => Log.Error(Prefix + x);


    }
}
