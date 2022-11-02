// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cthulhu;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
//using System.Diagnostics;
//using System.Linq;

// ----------------------------------------------------------------------
// These are RimWorld-specific usings. Activate/Deactivate what you need:
// ----------------------------------------------------------------------
// Always needed
//using VerseBase;         // Material/Graphics handling functions are found here
// RimWorld universal objects are here (like 'Building')
// Needed when you do something with the AI
//using Verse.AI.Group;
//using Verse.Sound;       // Needed when you do something with Sound
//using Verse.Noise;       // Needed when you do something with Noises
// RimWorld specific functions are found here (like 'Building_Battery')

//using RimWorld.Planet;   // RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

namespace CultOfCthulhu
{
    public partial class Building_SacrificialAltar : Building, IBillGiver
    {
        public enum Function
        {
            Level1 = 0,
            Level2 = 1,
            Level3 = 2,
            Nightmare = 3
        }

        //Offering Related Variables
        public enum OfferingState
        {
            off = 0,
            started,
            offering,
            finished
        }

        //Sacrifice Related Variables
        public enum SacrificeState
        {
            off = 0,
            started,
            gathering,
            sacrificing,
            finishing,
            finished
        }

        //Universal Variables
        public enum State
        {
            notinuse = 0,
            sacrificing,
            worshipping,
            offering
        }

        //Worship Related Variables
        public enum WorshipState
        {
            off = 0,
            started,
            gathering,
            worshipping,
            finishing,
            finished
        }

        public static int morningStart = 0;
        public static int eveningStart = 12;

        private HashSet<Pawn> availableWorshippers;
        private RecipeDef billRecipe;

        public Function currentFunction = Function.Level1;
        public CosmicEntity currentOfferingDeity;

        public OfferingState currentOfferingState = OfferingState.off;

        public SacrificeState currentSacrificeState = SacrificeState.off;
        private State currentState = State.notinuse;
        public CosmicEntity currentWorshipDeity;

        public WorshipState currentWorshipState = WorshipState.off;
        public bool debugAlwaysSucceed;
        private bool destroyedFlag; // For safety
        public List<ThingCount> determinedOfferings = new List<ThingCount>();
        private bool didEveningRitual;
        private bool didMorningRitual;
        public int eveningHour = 18;

        //Misc Event Variables
        private Function lastFunction = Function.Level1;
        private string lastReport = "";
        public int morningHour = 9;
        private Pawn offerer;
        public float ticksToNextRepair;
        public int remainingDuration = 20000;

        public bool OptionEvening;

        public bool OptionMorning;

        public Pawn preacher;
        public string RoomName = "Unnamed Temple";

        private Bill_Sacrifice sacrificeData;
        public List<int> seasonSchedule = new List<int>(collection: new int[15]);
        public CosmicEntity tempCurrentOfferingDeity;
        public CosmicEntity tempCurrentSacrificeDeity;
        public IncidentDef tempCurrentSpell;
        public CosmicEntity tempCurrentWorshipDeity;
        private List<ThingCount> tempDeterminedOfferings = new List<ThingCount>();
        public Pawn tempExecutioner;
        public Pawn tempOfferer;
        public CultUtility.OfferingSize tempOfferingSize = CultUtility.OfferingSize.none;
        public CultUtility.SacrificeType tempOfferingType = CultUtility.SacrificeType.none;
        public Pawn tempPreacher;

        public Pawn tempSacrifice;
        public bool toBePrunedAndRepaired;

        public Building_SacrificialAltar()
        {
            BillStack = new BillStack(giver: this);
        }

        public IEnumerable<IntVec3> CellsAround => GenRadial.RadialCellsAround(center: Position, radius: 5, useCenter: true);
        public Bill_Sacrifice SacrificeData => sacrificeData;

        public string LastReport
        {
            get => lastReport;
            set => lastReport = value;
        }

        private bool DoMorningSermon =>
            OptionMorning && Utility.IsMorning(map: Map) && didMorningRitual == false;

        private bool DoEveningSermon =>
            OptionEvening && Utility.IsEvening(map: Map) && didEveningRitual == false;

        private bool DoSermonNow
        {
            get
            {
                var typeOfSermons = seasonSchedule[index: GenLocalDate.DayOfQuadrum(map: Map)];
                if (typeOfSermons == 0)
                {
                    return false;
                }

                var currentHour = GenLocalDate.HourInteger(map: Map);
                if (didMorningRitual && currentHour >= eveningStart)
                {
                    didMorningRitual = false;
                }

                if (didEveningRitual && currentHour < eveningStart)
                {
                    didEveningRitual = false;
                }

                if (!didMorningRitual && (typeOfSermons == 1 || typeOfSermons == 3) && currentHour == morningHour)
                {
                    didMorningRitual = true;
                    return true;
                }

                if (didEveningRitual || typeOfSermons != 2 && typeOfSermons != 3 || currentHour != eveningHour)
                {
                    return false;
                }

                didEveningRitual = true;
                return true;
            }
        }

        // RimWorld.Building_Bed
        public static int LyingSlotsCount { get; } = 1;

        // RimWorld.Building_Bed
        public bool AnyUnoccupiedLyingSlot
        {
            get
            {
                for (var i = 0; i < LyingSlotsCount; i++)
                {
                    if (GetCurOccupant() == null)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public HashSet<Pawn> AvailableWorshippers
        {
            get
            {
                if (availableWorshippers == null || availableWorshippers.Count == 0)
                {
                    
                    availableWorshippers =
                        new HashSet<Pawn>(collection: Map.mapPawns.AllPawnsSpawned.FindAll(match: y => y is Pawn x &&
                                                                                                       x.RaceProps.Humanlike &&
                                                                                                       x.Faction == Faction.OfPlayerSilentFail &&
                                                                                                       x.RaceProps.intelligence == Intelligence.Humanlike &&
                                                                                                       !x.IsPrisoner &&
                                                                                                       !x.Downed && !x.Dead &&
                                                                                                       !x.InMentalState && !x.InAggroMentalState &&
                                                                                                       x.CurJob.def != CultsDefOf.Cults_MidnightInquisition &&
                                                                                                       x.CurJob.def != CultsDefOf.Cults_AttendSacrifice &&
                                                                                                       x.CurJob.def != CultsDefOf.Cults_ReflectOnWorship &&
                                                                                                       x.CurJob.def != CultsDefOf.Cults_AttendWorship &&
                                                                                                       x.CurJob.def != JobDefOf.Capture &&
                                                                                                       x.CurJob.def != JobDefOf.ExtinguishSelf && //Oh god help
                                                                                                       x.CurJob.def != JobDefOf.Rescue && //Saving lives is more important
                                                                                                       x.CurJob.def != JobDefOf.TendPatient && //Saving lives is more important
                                                                                                       x.CurJob.def != JobDefOf.BeatFire && //Fire?! This is more important
                                                                                                       x.CurJob.def != JobDefOf.Lovin && //Not ready~~
                                                                                                       x.CurJob.def != JobDefOf.LayDown && //They're resting
                                                                                                       x.CurJob.def != JobDefOf.FleeAndCower //They're not cowering
                        ).ChangeType<List<Pawn>>());
                }

                return availableWorshippers;
            }
        }

        public bool CurrentlyUsableForBills()
        {
            return true;
        }

        //Required by RW1.4, unused
        public void Notify_BillDeleted(Bill bill)
        {
        }

        public BillStack BillStack { get; }
        public IEnumerable<IntVec3> IngredientStackCells => GenAdj.CellsOccupiedBy(t: this);

        public override string GetInspectString()
        {
            var stringBuilder = new StringBuilder();

            // Add the inspections string from the base
            stringBuilder.Append(value: base.GetInspectString());

            // return the complete string
            return stringBuilder.ToString();
        }

        private bool IsCongregating()
        {
            return IsOffering() || IsSacrificing() || IsWorshipping();
        }

        private bool IsOffering()
        {
            return currentState == State.offering || currentOfferingState != OfferingState.finished &&
                currentOfferingState != OfferingState.off;
        }

        private bool IsSacrificing()
        {
            return currentState == State.sacrificing || currentSacrificeState != SacrificeState.finished &&
                currentSacrificeState != SacrificeState.off;
        }

        private bool IsWorshipping()
        {
            return currentState == State.worshipping || currentWorshipState != WorshipState.finished &&
                currentWorshipState != WorshipState.off;
        }

        private bool CanUpgrade()
        {
            switch (currentFunction)
            {
                case Function.Level1:
                    if (ResearchProjectDef.Named(defName: "Forbidden_Sacrifice").IsFinished)
                    {
                        return true;
                    }

                    return false;
                case Function.Level2:
                    if (ResearchProjectDef.Named(defName: "Forbidden_Human").IsFinished)
                    {
                        return true;
                    }

                    return false;
                case Function.Level3:
                    return false;
                case Function.Nightmare:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
        }

        private static bool RejectMessage(string s, Pawn pawn = null)
        {
            Messages.Message(text: s, lookTargets: TargetInfo.Invalid, def: MessageTypeDefOf.RejectInput);
            pawn = null;
            return false;
        }

        public void ChangeState(State type)
        {
            if (type == State.notinuse)
            {
                currentState = type;
                currentWorshipState = WorshipState.off;
                currentSacrificeState = SacrificeState.off;
                currentOfferingState = OfferingState.off;
                availableWorshippers = null;
            }
            else
            {
                Log.Error(text: "Changed default state of Sacrificial Altar this should never happen.");
            }

            ReportState();
        }

        public void ChangeState(State type, WorshipState worshipState)
        {
            currentState = type;
            currentWorshipState = worshipState;
            ReportState();
        }

        public void ChangeState(State type, SacrificeState sacrificeState)
        {
            currentState = type;
            currentSacrificeState = sacrificeState;
            ReportState();
        }

        public void ChangeState(State type, OfferingState offeringState)
        {
            currentState = type;
            currentOfferingState = offeringState;
            ReportState();
        }

        private void ReportState()
        {
            var s = new StringBuilder();
            s.Append(value: "===================");
            s.AppendLine(value: "Sacrifical Altar States Changed");
            s.AppendLine(value: "===================");
            s.AppendLine(value: "State: " + currentState);
            s.AppendLine(value: "Worship: " + currentWorshipState);
            s.AppendLine(value: "Offering: " + currentOfferingState);
            s.AppendLine(value: "Sacrifice: " + currentSacrificeState);
            s.AppendLine(value: "===================");
            Utility.DebugReport(x: s.ToString());
        }

        private Pawn GetCurOccupant()
        {
            var sleepingSlotPos = GetLyingSlotPos();
            var list = Map.thingGrid.ThingsListAt(c: sleepingSlotPos);
            if (list.NullOrEmpty())
            {
                return null;
            }

            foreach (var t in list)
            {
                if (!(t is Pawn pawn))
                {
                    continue;
                }

                if (pawn.CurJob == null)
                {
                    continue;
                }

                if (pawn.jobs.posture != PawnPosture.Standing)
                {
                    return pawn;
                }
            }

            return null;
        }

        // RimWorld.Building_Bed
        public IntVec3 GetLyingSlotPos()
        {
            var index = 1;
            var cellRect = this.OccupiedRect();
            if (Rotation == Rot4.North)
            {
                return new IntVec3(newX: cellRect.minX + index, newY: Position.y, newZ: cellRect.minZ);
            }

            return Rotation == Rot4.East
                ? new IntVec3(newX: cellRect.minX, newY: Position.y, newZ: cellRect.maxZ - index)
                : Rotation == Rot4.South
                    ? new IntVec3(newX: cellRect.minX + index, newY: Position.y, newZ: cellRect.maxZ)
                    : new IntVec3(newX: cellRect.maxX, newY: Position.y, newZ: cellRect.maxZ - index);
        }

        public override void SpawnSetup(Map map, bool bla)
        {
            base.SpawnSetup(map: map, respawningAfterLoad: bla);
            CultTracker.Get.ExposedToCults = true;
            if (RoomName == null)
            {
                RoomName = "Unnamed Temple";
            }

            if (seasonSchedule == null)
            {
                var settingToMigrate = 0;
                if (OptionMorning && OptionEvening)
                {
                    settingToMigrate = 3;
                }
                else
                {
                    if (OptionMorning)
                    {
                        settingToMigrate = 1;
                    }

                    if (OptionEvening)
                    {
                        settingToMigrate = 2;
                    }
                }

                seasonSchedule = new List<int>(collection: new[]
                {
                    settingToMigrate, settingToMigrate, settingToMigrate, settingToMigrate, settingToMigrate,
                    settingToMigrate, settingToMigrate, settingToMigrate, settingToMigrate, settingToMigrate,
                    settingToMigrate, settingToMigrate, settingToMigrate, settingToMigrate, settingToMigrate
                });
                morningHour = 9;
                eveningHour = 18;
            }

            if (eveningHour == 0)
            {
                eveningHour = 18;
            }

            DeityTracker.Get.orGenerate();
            switch (def.defName)
            {
                case "Cult_AnimalSacrificeAltar":
                    currentFunction = Function.Level2;
                    break;
                case "Cult_HumanSacrificeAltar":
                    currentFunction = Function.Level3;
                    break;
                case "Cult_NightmareSacrificeAltar":
                    currentFunction = Function.Nightmare;
                    break;
            }

            //UpdateGraphics();
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(value: ref RoomName, label: "RoomName");
            Scribe_Deep.Look(target: ref sacrificeData, label: "sacrificeData");
            Scribe_References.Look(refee: ref tempCurrentSacrificeDeity, label: "tempCurrentSacrificeDeity");
            Scribe_References.Look(refee: ref tempSacrifice, label: "tempSacrifice");
            Scribe_References.Look(refee: ref tempExecutioner, label: "tempExecutioner");
            Scribe_Defs.Look(value: ref tempCurrentSpell, label: "tempCurrentSpell");
            Scribe_Values.Look(value: ref currentState, label: "currentState");
            Scribe_Values.Look(value: ref currentSacrificeState, label: "currentSacrificeState");
            Scribe_Values.Look(value: ref lastFunction, label: "lastFunction");
            Scribe_Values.Look(value: ref currentFunction, label: "currentFunction");
            // Worship values
            Scribe_References.Look(refee: ref currentWorshipDeity, label: "currentWorshipDeity");
            Scribe_References.Look(refee: ref tempCurrentWorshipDeity, label: "tempCurrentWorshipDeity");
            Scribe_References.Look(refee: ref preacher, label: "preacher");
            Scribe_References.Look(refee: ref tempPreacher, label: "tempPreacher");
            Scribe_Values.Look(value: ref currentWorshipState, label: "currentWorshipState");
            Scribe_Values.Look(value: ref OptionMorning, label: "OptionMorning");
            Scribe_Values.Look(value: ref OptionEvening, label: "OptionEvening");
            Scribe_Values.Look(value: ref didMorningRitual, label: "didMorningRitual");
            Scribe_Values.Look(value: ref didEveningRitual, label: "didEveningRitual");
            Scribe_Values.Look(value: ref morningHour, label: "morningHour", defaultValue: 9);
            Scribe_Values.Look(value: ref eveningHour, label: "eveningHour", defaultValue: 18);
            Scribe_Collections.Look(list: ref seasonSchedule, label: "seasonSchedule", lookMode: LookMode.Value, false);
            //Misc
            Scribe_Values.Look(value: ref toBePrunedAndRepaired, label: "tobePrunedAndRepaired");
            Scribe_Values.Look(value: ref lastReport, label: "lastReport");
            Scribe_Values.Look(value: ref ticksToNextRepair, label: "ticksToNextRepair");
            Scribe_Values.Look(value: ref remainingDuration, label: "remainingDuration");
    }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            // block further ticker work
            destroyedFlag = true;
            ITab_AltarSacrificesCardUtility.Tab = ITab_AltarSacrificesCardUtility.SacrificeCardTab.Offering;
            base.Destroy(mode: mode);
        }

        public override void TickRare()
        {
            if (destroyedFlag) // Do nothing further, when destroyed (just a safety)
            {
                return;
            }

            if (!Spawned)
            {
                return;
            }

            // Don't forget the base work
            base.TickRare();
            AutoWorshipRareTick();
            WorshipRareTick();
            OfferingRareTick();
            if (currentFunction > Function.Level1)
            {
                SacrificeRareTick();
            }
        }

        public override void Tick()
        {
            if (destroyedFlag) // Do nothing further, when destroyed (just a safety)
            {
                return;
            }

            if (!Spawned)
            {
                return;
            }

            // Don't forget the base work
            base.Tick();
            WorshipTick();
            if (currentFunction > Function.Level1)
            {
                SacrificeTick();
            }
        }

        public void AutoWorshipRareTick()
        {
            if (DoSermonNow)
            {
                TryTimedWorship();
            }

            // Old code, for only morning/evening options

            ////In the morning, let's gather to worship
            //if (DoMorningSermon && !IsWorshipping() && !IsSacrificing())
            //{
            //    didMorningRitual = true;
            //    TryTimedWorship();
            //}

            ////In the evening, let's gather to worship
            //if (DoEveningSermon && !IsWorshipping() && !IsSacrificing())
            //{
            //    didEveningRitual = true;
            //    TryTimedWorship();
            //}

            //Reset values
            //if (Cthulhu.Utility.IsEvening(Map) || Cthulhu.Utility.IsMorning(Map))
            //{
            //    return;
            //}

            //didEveningRitual = false;
            //didMorningRitual = false;
        }

        public void SacrificeRareTick()
        {
            if (!Spawned)
            {
                return;
            }

            if (SacrificeData?.Executioner == null)
            {
                return;
            }

            if (currentState != State.sacrificing)
            {
                return;
            }

            switch (currentSacrificeState)
            {
                case SacrificeState.started:
                case SacrificeState.gathering:
                case SacrificeState.sacrificing:
                    if (!Utility.IsActorAvailable(preacher: SacrificeData.Executioner))
                    {
                        CultUtility.AbortCongregation(altar: this, reason: "Executioner".Translate() + "IsUnavailable".Translate());
                        return;
                    }
                    else if (!Utility.IsActorAvailable(preacher: SacrificeData.Sacrifice, downedAllowed: true))
                    {
                        CultUtility.AbortCongregation(altar: this, reason: "Sacrifice".Translate() + "IsUnavailable".Translate());
                        return;
                    }
                    else
                    {
                        if (SacrificeData.Executioner?.CurJob != null &&
                            SacrificeData.Executioner.CurJob.def != CultsDefOf.Cults_HoldSacrifice)
                        {
                            CultUtility.AbortCongregation(altar: this,
                                reason: "Executioner".Translate() + "IsUnavailable".Translate());
                            return;
                        }
                    }

                    if (availableWorshippers == null)
                    {
                        GetSacrificeGroup();
                    }

                    return;

                case SacrificeState.finishing:
                    if (!Utility.IsActorAvailable(preacher: SacrificeData.Executioner))
                    {
                        CultUtility.AbortCongregation(altar: this, reason: "Executioner".Translate() + "IsUnavailable".Translate());
                        return;
                    }

                    if (SacrificeData.Executioner?.CurJob != null &&
                        SacrificeData.Executioner.CurJob.def != CultsDefOf.Cults_ReflectOnResult)
                    {
                        return;
                    }

                    GetSacrificeGroup();
                    return;

                case SacrificeState.finished:
                case SacrificeState.off:
                    ChangeState(type: State.notinuse);
                    return;
            }
        }

        public void OfferingRareTick()
        {
            if (currentState != State.offering)
            {
                return;
            }

            switch (currentOfferingState)
            {
                case OfferingState.started:
                    if (!Utility.IsActorAvailable(preacher: offerer))
                    {
                        CultUtility.AbortCongregation(altar: this, reason: "Offerer".Translate() + "IsUnavailable".Translate());
                        return;
                    }
                    else if (offerer.CurJob.def != CultsDefOf.Cults_GiveOffering)
                    {
                        Utility.DebugReport(x: offerer.CurJob.def.defName);
                        CultUtility.AbortCongregation(altar: this, reason: "Offerer is not performing the task at hand.");
                        return;
                    }

                    return;
                case OfferingState.offering:
                    if (!Utility.IsActorAvailable(preacher: offerer))
                    {
                        CultUtility.AbortCongregation(altar: this, reason: "Offerer".Translate() + "IsUnavailable".Translate());
                        return;
                    }
                    else if (offerer.CurJob.def != CultsDefOf.Cults_GiveOffering)
                    {
                        Utility.DebugReport(x: offerer.CurJob.def.defName);
                        CultUtility.AbortCongregation(altar: this, reason: "Offerer is not performing the task at hand.");
                        return;
                    }

                    return;
                case OfferingState.finished:
                case OfferingState.off:
                    ChangeState(type: State.notinuse);
                    return;
            }
        }

        public void WorshipRareTick()
        {
            if (currentState != State.worshipping)
            {
                return;
            }

            switch (currentWorshipState)
            {
                case WorshipState.started:
                case WorshipState.gathering:
                case WorshipState.worshipping:
                    if (!Utility.IsActorAvailable(preacher: preacher))
                    {
                        CultUtility.AbortCongregation(altar: this, reason: "Preacher".Translate() + "IsUnavailable".Translate());
                        return;
                    }

                    if (preacher.CurJob.def != CultsDefOf.Cults_HoldWorship)
                    {
                        CultUtility.AbortCongregation(altar: this, reason: "Preacher".Translate() + "IsUnavailable".Translate());
                        return;
                    }

                    if (availableWorshippers != null)
                    {
                        return;
                    }

                    GetWorshipGroup(altar: this,
                        inRangeCells: GenRadial.RadialCellsAround(center: Position, radius: GenRadial.MaxRadialPatternRadius - 1, useCenter: true));
                    Utility.DebugReport(x: "Gathering yay");
                    return;
                case WorshipState.finishing:
                    if (!Utility.IsActorAvailable(preacher: preacher))
                    {
                        CultUtility.AbortCongregation(altar: this, reason: "Preacher".Translate() + "IsUnavailable".Translate());
                        return;
                    }

                    if (preacher.CurJob.def != CultsDefOf.Cults_ReflectOnWorship)
                    {
                        return;
                    }

                    GetWorshipGroup(altar: this,
                        inRangeCells: GenRadial.RadialCellsAround(center: Position, radius: GenRadial.MaxRadialPatternRadius - 1, useCenter: true));
                    Utility.DebugReport(x: "Finishing yay");
                    return;
                case WorshipState.finished:
                case WorshipState.off:
                    ChangeState(type: State.notinuse);
                    return;
            }
        }

        public void SacrificeTick()
        {
            if (currentState != State.sacrificing)
            {
                return;
            }

            switch (currentSacrificeState)
            {
                case SacrificeState.started:
                case SacrificeState.gathering:
                case SacrificeState.sacrificing:
                    if (Utility.IsActorAvailable(preacher: SacrificeData.Executioner))
                    {
                        if (Utility.IsActorAvailable(preacher: SacrificeData.Sacrifice, downedAllowed: true))
                        {
                            if (SacrificeData.Executioner.CurJob.def != CultsDefOf.Cults_HoldSacrifice)
                            {
                                CultUtility.AbortCongregation(altar: this,
                                    reason: "Executioner".Translate() + "IsUnavailable".Translate());
                            }

                            return;
                        }

                        CultUtility.AbortCongregation(altar: this, reason: "Sacrifice".Translate() + "IsUnavailable".Translate());
                        return;
                    }

                    CultUtility.AbortCongregation(altar: this, reason: "Executioner".Translate() + "IsUnavailable".Translate());
                    return;

                case SacrificeState.finishing:
                    if (!Utility.IsActorAvailable(preacher: SacrificeData.Executioner))
                    {
                        CultUtility.AbortCongregation(altar: this, reason: "Executioner".Translate() + "IsUnavailable".Translate());
                    }

                    if (MapComponent_SacrificeTracker.Get(map: Map).lastSacrificeType == CultUtility.SacrificeType.animal)
                    {
                        ChangeState(type: State.sacrificing, sacrificeState: SacrificeState.finished);
                    }

                    return;

                case SacrificeState.finished:
                case SacrificeState.off:
                    ChangeState(type: State.notinuse);
                    return;
            }
        }

        public void WorshipTick()
        {
            if (currentState != State.worshipping)
            {
                return;
            }

            switch (currentWorshipState)
            {
                case WorshipState.started:
                case WorshipState.gathering:
                    if (Utility.IsActorAvailable(preacher: preacher))
                    {
                        if (preacher.CurJob.def != CultsDefOf.Cults_HoldWorship)
                        {
                            CultUtility.AbortCongregation(altar: this, reason: "Preacher".Translate() + "IsUnavailable".Translate());
                            return;
                        }
                    }

                    Utility.DebugReport(x: "Gathering yay");
                    return;
                case WorshipState.finishing:
                    if (!Utility.IsActorAvailable(preacher: preacher))
                    {
                        CultUtility.AbortCongregation(altar: this, reason: "Preacher".Translate() + "IsUnavailable".Translate());
                    }

                    Utility.DebugReport(x: "Finishing yay");
                    return;
                case WorshipState.finished:
                case WorshipState.off:
                    currentState = State.notinuse;
                    return;
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }

            if (currentFunction < Function.Level3)
            {
                var command_Upgrade = new Command_Action
                {
                    action = TryUpgrade,
                    defaultLabel = "CommandUpgrade".Translate(),
                    defaultDesc = "CommandUpgrade".Translate(),
                    disabled = !CanUpgrade() || currentFunction == Function.Nightmare,
                    disabledReason = "CommandCultDisabled".Translate(),
                    hotKey = KeyBindingDefOf.Misc1,
                    icon = ContentFinder<Texture2D>.Get(itemPath: "UI/Commands/Worship")
                };
                if (CanUpgrade())
                {
                    if (currentFunction == Function.Level1)
                    {
                        command_Upgrade.icon = ContentFinder<Texture2D>.Get(itemPath: "UI/Commands/Upgrade2");
                    }
                    else if (currentFunction == Function.Level2)
                    {
                        command_Upgrade.icon = ContentFinder<Texture2D>.Get(itemPath: "UI/Commands/Upgrade3");
                    }
                }
                else
                {
                    if (currentFunction == Function.Level1)
                    {
                        command_Upgrade.icon = ContentFinder<Texture2D>.Get(itemPath: "UI/Commands/Upgrade2Disabled");
                    }
                    else if (currentFunction == Function.Level2)
                    {
                        command_Upgrade.icon = ContentFinder<Texture2D>.Get(itemPath: "UI/Commands/Upgrade3Disabled");
                    }
                }

                yield return command_Upgrade;
            }


            if (!IsSacrificing())
            {
                var command_Action = new Command_Action
                {
                    action = TrySacrifice,
                    defaultLabel = "CommandCultSacrifice".Translate(),
                    defaultDesc = "CommandCultSacrificeDesc".Translate(),
                    disabled = currentFunction < Function.Level2 || currentFunction == Function.Nightmare,
                    disabledReason = "CommandCultDisabled".Translate(),
                    hotKey = KeyBindingDefOf.Misc1,
                    icon = ContentFinder<Texture2D>.Get(itemPath: "UI/Commands/Sacrifice")
                };
                if (currentFunction < Function.Level2)
                {
                    command_Action.icon = ContentFinder<Texture2D>.Get(itemPath: "UI/Commands/SacrificeDisabled");
                }

                yield return command_Action;
            }
            else
            {
                var command_Cancel = new Command_Action
                {
                    action = CancelSacrifice,
                    defaultLabel = "CommandCancelConstructionLabel".Translate(),
                    defaultDesc = "CommandCancelSacrifice".Translate(),
                    disabled = currentFunction < Function.Level2,
                    hotKey = KeyBindingDefOf.Designator_Cancel,
                    icon = ContentFinder<Texture2D>.Get(itemPath: "UI/Designators/Cancel")
                };
                yield return command_Cancel;
            }

            if (!IsWorshipping())
            {
                var command_Action = new Command_Action
                {
                    action = TryWorshipForced,
                    defaultLabel = "CommandForceWorship".Translate(),
                    defaultDesc = "CommandForceWorshipDesc".Translate(),
                    disabled = currentFunction == Function.Nightmare,
                    hotKey = KeyBindingDefOf.Misc2,
                    icon = ContentFinder<Texture2D>.Get(itemPath: "UI/Commands/Worship")
                };
                yield return command_Action;
            }
            else
            {
                var command_Cancel = new Command_Action
                {
                    action = CancelWorship,
                    defaultLabel = "CommandCancelConstructionLabel".Translate(),
                    defaultDesc = "CommandCancelWorship".Translate(),
                    hotKey = KeyBindingDefOf.Designator_Cancel,
                    icon = ContentFinder<Texture2D>.Get(itemPath: "UI/Designators/Cancel")
                };
                yield return command_Cancel;
            }

            if (!IsOffering())
            {
                var command_Action = new Command_Action
                {
                    action = TryOffering,
                    defaultLabel = "CommandOffering".Translate(),
                    defaultDesc = "CommandOfferingDesc".Translate(),
                    disabled = currentFunction == Function.Nightmare,
                    hotKey = KeyBindingDefOf.Misc3,
                    icon = ContentFinder<Texture2D>.Get(itemPath: "UI/Commands/MakeOffering")
                };
                yield return command_Action;
            }
            else
            {
                var command_Cancel = new Command_Action
                {
                    action = CancelOffering,
                    defaultLabel = "CommandCancelConstructionLabel".Translate(),
                    defaultDesc = "CommandCancelOffering".Translate(),
                    hotKey = KeyBindingDefOf.Designator_Cancel,
                    icon = ContentFinder<Texture2D>.Get(itemPath: "UI/Designators/Cancel")
                };
                yield return command_Cancel;
            }

            if (CultsDefOf.Forbidden_Reports.IsFinished)
            {
                yield return new Command_Action
                {
                    action = GiveReport,
                    defaultLabel = "CommandCultReport".Translate(),
                    defaultDesc = "CommandCultReportDesc".Translate(),
                    disabled = LastReport == "",
                    disabledReason = "CommandCultReportDisabled".Translate(),
                    hotKey = KeyBindingDefOf.Misc4,
                    icon = ContentFinder<Texture2D>.Get(itemPath: "UI/Commands/CultReport")
                };
            }

            if (currentFunction == Function.Nightmare)
            {
                yield return new Command_Toggle
                {
                    hotKey = KeyBindingDefOf.Command_TogglePower,
                    icon = ContentFinder<Texture2D>.Get(itemPath: "UI/Icons/Commands/PruneAndRepair"),
                    defaultLabel = "PruneAndRepair".Translate(),
                    defaultDesc = "PruneAndRepairDesc".Translate(),
                    isActive = () => toBePrunedAndRepaired,
                    toggleAction = PruneAndRepairToggle
                };
            }


            if (!DebugSettings.godMode)
            {
                yield break;
            }

            yield return new Command_Action
            {
                defaultLabel = "Debug: Discover All Deities",
                action = delegate
                {
                    foreach (var entity in DeityTracker.Get.DeityCache.Keys)
                    {
                        entity.discovered = true;
                    }
                }
            };


            yield return new Command_Action
            {
                defaultLabel = "Debug: All Favor to 0",
                action = delegate
                {
                    foreach (var entity in DeityTracker.Get.DeityCache.Keys)
                    {
                        entity.ResetFavor();
                    }
                }
            };


            yield return new Command_Action
            {
                defaultLabel = "Debug: Make All Colonists Cult-Minded",
                action = delegate
                {
                    foreach (var p in Map.mapPawns.FreeColonistsSpawned)
                    {
                        CultUtility.AffectCultMindedness(pawn: p, amount: 0.99f);
                    }
                }
            };

            yield return new Command_Action
            {
                defaultLabel = "Debug: Upgrade Max Level Altar",
                action = delegate { currentFunction = Function.Level3; }
            };

            yield return new Command_Action
            {
                defaultLabel = "Debug: Unlock All Spells",
                action = delegate
                {
                    foreach (var entity in DeityTracker.Get.DeityCache.Keys)
                    {
                        entity.AffectFavor(favorChange: 9999999);
                    }
                }
            };

            yield return new Command_Toggle
            {
                defaultLabel = "Debug: Always Succeed",
                isActive = () => debugAlwaysSucceed,
                toggleAction = delegate { debugAlwaysSucceed = !debugAlwaysSucceed; }
            };

            yield return new Command_Action
            {
                defaultLabel = "Debug: Force Side Effect",
                action = delegate
                {
                    var table = new CultTableOfFun();
                    var list = (from spell in table.TableOfFun
                        let currentDef = IncidentDef.Named(defName: spell.defName)
                        select new FloatMenuOption(label: currentDef.LabelCap, action: delegate
                        {
                            var temp = DefDatabase<IncidentDef>.GetNamed(defName: spell.defName);
                            if (temp != null)
                            {
                                CultUtility.CastSpell(spell: temp, map: Map);
                            }
                        })).ToList();
                    Find.WindowStack.Add(window: new FloatMenu(options: list));
                }
            };

            if (currentFunction != Function.Nightmare)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Debug: Force Nightmare Tree",
                    action = NightmareEvent
                };
            }
        }

        private void GiveReport()
        {
            Find.WindowStack.Add(window: new Dialog_MessageBox(text: LastReport));
        }

        public void PruneAndRepairToggle()
        {
            toBePrunedAndRepaired = !toBePrunedAndRepaired;
        }

        private void TryUpgrade()
        {
            if (IsCongregating())
            {
                Messages.Message(text: "UpgradeCongregationWarning".Translate(), def: MessageTypeDefOf.RejectInput);
                return;
            }

            Upgrade();
        }

        public void Upgrade()
        {
            var newDefName = "";
            switch (currentFunction)
            {
                case Function.Level1:
                    newDefName = "Cult_AnimalSacrificeAltar";
                    break;
                case Function.Level2:
                    newDefName = "Cult_HumanSacrificeAltar";
                    break;
                case Function.Level3:
                    Log.Error(text: "Tried to upgrade fully functional altar. This should never happen.");
                    return;
            }

            if (newDefName == "")
            {
                return;
            }

            ReplaceAltarWith(newDefName: newDefName);
        }

        public void NightmareEvent()
        {
            lastFunction = currentFunction;
            currentFunction = Function.Nightmare;
            ReplaceAltarWith(newDefName: "Cult_NightmareSacrificeAltar");
        }

        public void NightmarePruned(Pawn pruner)
        {
            var oldDefName = "";
            switch (lastFunction)
            {
                case Function.Level1:
                    oldDefName = "Cult_SacrificialAltar";
                    break;
                case Function.Level2:
                    oldDefName = "Cult_AnimalSacrificeAltar";
                    break;
                case Function.Level3:
                    oldDefName = "Cult_HumanSacrificeAltar";
                    break;
            }

            if (oldDefName == "")
            {
                return;
            }

            var newAltar = ReplaceAltarWith(newDefName: oldDefName);
            Messages.Message(text: "PruningSuccessful".Translate(
                arg1: pruner.LabelShort
            ), def: MessageTypeDefOf.PositiveEvent);
            newAltar.Map.reservationManager.ReleaseAllForTarget(t: newAltar);
        }

        private Building_SacrificialAltar ReplaceAltarWith(string newDefName)
        {
            if (newDefName == "")
            {
                Utility.ErrorReport(x: "ReplaceAltarWith :: Null exception.");
                return null;
            }

            //Copy the important values.
            var currentLocation = Position;
            var currentRotation = Rotation;
            var currentStuff = Stuff;
            var compQuality = this.TryGetComp<CompQuality>();
            var currentLastFunction = lastFunction;
            var currentMap = Map;
            var qualityCat = QualityCategory.Normal;
            if (compQuality != null)
            {
                qualityCat = compQuality.Quality;
            }

            //Worship values
            var s1 = RoomName;
            var p1 = tempPreacher;
            var c1 = tempCurrentWorshipDeity;
            var b1 = OptionMorning;
            var b2 = OptionEvening;

            Destroy();
            //Spawn the new altar over the other
            var thing = (Building_SacrificialAltar) ThingMaker.MakeThing(def: ThingDef.Named(defName: newDefName), stuff: currentStuff);
            var result = thing;
            thing.SetFaction(newFaction: Faction.OfPlayer);
            thing.Rotation = currentRotation;
            GenPlace.TryPlaceThing(thing: thing, center: currentLocation, map: currentMap, mode: ThingPlaceMode.Direct);
            thing.Rotation = currentRotation;
            thing.TryGetComp<CompQuality>().SetQuality(q: qualityCat, source: ArtGenerationContext.Colony);
            thing.lastFunction = currentLastFunction;
            if (currentFunction != Function.Nightmare)
            {
                Messages.Message(text: "UpgradeSuccessful".Translate(), lookTargets: new TargetInfo(cell: currentLocation, map: Map),
                    def: MessageTypeDefOf.PositiveEvent);
            }
            else
            {
                Messages.Message(text: "CorruptedAltarWarning".Translate(), def: MessageTypeDefOf.NegativeEvent);
            }

            //Pass worship values
            thing.RoomName = s1;
            thing.tempPreacher = p1;
            thing.tempCurrentWorshipDeity = c1;
            thing.OptionMorning = b1;
            thing.OptionEvening = b2;
            thing.remainingDuration = 2000;

            return result;
        }

        private void CancelOffering()
        {
            var listeners =
                Map.mapPawns.AllPawnsSpawned.FindAll(match: x => x.RaceProps.intelligence == Intelligence.Humanlike);
            var unused = new bool[listeners.Count];
            foreach (var pawn in listeners)
            {
                if (pawn.Faction != Faction.OfPlayer)
                {
                    continue;
                }

                //Hold Offering
                if (pawn.CurJob.def == CultsDefOf.Cults_GiveOffering)
                {
                    pawn.jobs.StopAll();
                }
            }

            ChangeState(type: State.notinuse);
            //this.currentState = State.off;
            Messages.Message(text: "Cancelling offering.", def: MessageTypeDefOf.NegativeEvent);
        }

        private void TryOffering()
        {
            if (IsCongregating())
            {
                Messages.Message(text: "A congregation is already gathering.", def: MessageTypeDefOf.RejectInput);
                return;
            }

            if (!CanGatherOfferingNow())
            {
                return;
            }

            if (!TryDetermineOffering(type: tempOfferingType, size: tempOfferingSize, pawn: tempOfferer, altar: this,
                result: out tempDeterminedOfferings, resultRecipe: out billRecipe))
            {
                Utility.DebugReport(x: "Failed to determine offering");
                return;
            }

            switch (currentOfferingState)
            {
                case OfferingState.finished:
                case OfferingState.off:
                    if (IsWorshipping())
                    {
                        CancelWorship();
                    }

                    if (IsSacrificing())
                    {
                        CancelSacrifice();
                    }

                    StartOffering();
                    return;

                case OfferingState.started:
                case OfferingState.offering:
                    Messages.Message(text: "An offering is already happening.", lookTargets: TargetInfo.Invalid,
                        def: MessageTypeDefOf.RejectInput);
                    return;
            }
        }

        private bool CanGatherOfferingNow()
        {
            if (tempOfferingType == CultUtility.SacrificeType.none)
            {
                return RejectMessage(s: "No offering type selected");
            }

            if (tempOfferingSize == CultUtility.OfferingSize.none)
            {
                return RejectMessage(s: "No offering amount selected");
            }

            if (tempOfferer == null)
            {
                return RejectMessage(s: "No offerer selected");
            }

            if (tempOfferer.Drafted)
            {
                return RejectMessage(s: "Offerer is drafted.");
            }

            if (tempOfferer.Dead || tempOfferer.Downed)
            {
                return RejectMessage(s: "Select an able-bodied offerer.", pawn: tempOfferer);
            }

            if (tempCurrentOfferingDeity == null)
            {
                return RejectMessage(
                    s: "No cosmic entity selected. Entities can be discovered at the forbidden knowledge center.");
            }

            return !tempOfferer.CanReserve(target: this)
                ? RejectMessage(s: "The altar is reserved by something else.")
                : !Position.GetThingList(map: Map).OfType<Corpse>().Any() ||
                  RejectMessage(s: "The altar needs to be cleared first.");
        }

        public void StartOffering()
        {
            determinedOfferings = tempDeterminedOfferings;
            offerer = tempOfferer;
            currentOfferingDeity = tempCurrentOfferingDeity;

            if (Destroyed || !Spawned)
            {
                CultUtility.AbortCongregation(altar: null, reason: "Altar".Translate() + "IsUnavailable".Translate());
                return;
            }

            if (!Utility.IsActorAvailable(preacher: offerer))
            {
                CultUtility.AbortCongregation(altar: this,
                    reason: "Offerer".Translate() + " " + offerer.LabelShort + "IsUnavailable".Translate());
                offerer = null;
                return;
            }

            Messages.Message(text: "An offering is being gathered.", lookTargets: TargetInfo.Invalid, def: MessageTypeDefOf.NeutralEvent);
            ChangeState(type: State.offering, offeringState: OfferingState.started);
            Utility.DebugReport(x: "Make offering called.");

            var job2 = new Job(def: CultsDefOf.Cults_GiveOffering)
            {
                playerForced = true,
                targetA = this,
                targetQueueB = new List<LocalTargetInfo>(capacity: determinedOfferings.Count),
                targetC = PositionHeld,
                countQueue = new List<int>(capacity: determinedOfferings.Count)
            };
            foreach (var thingCount in determinedOfferings)
            {
                job2.targetQueueB.Add(item: thingCount.Thing);
                job2.countQueue.Add(item: thingCount.Count);
            }

            job2.haulMode = HaulMode.ToCellNonStorage;
            job2.locomotionUrgency = LocomotionUrgency.Sprint;
            job2.bill = new Bill_Production(recipe: billRecipe);
            offerer.jobs.TryTakeOrderedJob(job: job2);
        }

        private void CancelSacrifice()
        {
            var listeners =
                Map.mapPawns.AllPawnsSpawned.FindAll(match: x => x.RaceProps.intelligence == Intelligence.Humanlike);
            if (!listeners.NullOrEmpty())
            {
                foreach (var t in listeners)
                {
                    var pawn = t;
                    if (pawn.Faction != Faction.OfPlayer)
                    {
                        continue;
                    }

                    if (pawn.CurJob.def == CultsDefOf.Cults_HoldSacrifice ||
                        pawn.CurJob.def == CultsDefOf.Cults_AttendSacrifice ||
                        pawn.CurJob.def == CultsDefOf.Cults_ReflectOnResult ||
                        pawn.CurJob.def == CultsDefOf.Cults_GiveOffering)
                    {
                        pawn.jobs.StopAll();
                    }
                }
            }

            ChangeState(type: State.notinuse);
            Messages.Message(text: "Cancelling sacrifice.", def: MessageTypeDefOf.NegativeEvent);
        }

        private void TrySacrifice()
        {
            if (IsSacrificing())
            {
                Messages.Message(text: "A sacrifice is already gathering.", def: MessageTypeDefOf.RejectInput);
                return;
            }

            if (!CanGatherSacrificeNow())
            {
                return;
            }

            switch (currentSacrificeState)
            {
                case SacrificeState.finished:
                case SacrificeState.off:
                    if (IsWorshipping())
                    {
                        CancelWorship();
                    }

                    StartSacrifice();
                    return;

                case SacrificeState.started:
                case SacrificeState.gathering:
                case SacrificeState.sacrificing:
                case SacrificeState.finishing:
                    Messages.Message(text: "A sacrifice is already gathering.", lookTargets: TargetInfo.Invalid,
                        def: MessageTypeDefOf.RejectInput);
                    return;
            }
        }

        private bool CanGatherSacrificeNow()
        {
            if (tempSacrifice == null)
            {
                return RejectMessage(s: "No prisoner to sacrifice selected.");
            }

            if (tempExecutioner == null)
            {
                return RejectMessage(s: "No executioner selected");
            }

            if (tempExecutioner.Drafted)
            {
                return RejectMessage(s: "The executioner is drafted.");
            }

            if (tempCurrentSacrificeDeity == null)
            {
                return RejectMessage(s: "No cosmic entity selected");
            }

            if (MapComponent_SacrificeTracker.Get(map: Map).lastSacrificeType != CultUtility.SacrificeType.animal &&
                tempCurrentSpell == null)
            {
                return RejectMessage(s: "No spell selected. Tip: Earn favor to unlock spells.");
            }

            if (tempSacrifice.Dead)
            {
                return RejectMessage(s: "The sacrifice is already dead", pawn: tempSacrifice);
            }

            if (tempExecutioner.Dead || tempExecutioner.Downed)
            {
                return RejectMessage(s: "Select an able-bodied executioner");
            }

            if (!tempExecutioner.CanReserve(target: tempSacrifice))
            {
                return RejectMessage(s: "The executioner can't reserve the sacrifice.");
            }

            if (!tempExecutioner.CanReserve(target: this))
            {
                return RejectMessage(s: "The altar is reserved by something else");
            }

            if (Position.GetThingList(map: Map).OfType<Corpse>().Any())
            {
                return RejectMessage(s: "The altar needs to be cleared first.");
            }

            return MapComponent_SacrificeTracker.Get(map: Map).lastSacrificeType != CultUtility.SacrificeType.human
                   || tempCurrentSpell.Worker is SpellWorker worker && worker.CanSummonNow(map: Map);
        }

        private void StartSacrifice()
        {
            //Check for missing actors
            if (!PreStartSacrificeReady())
            {
                return;
            }

            //Reset results
            MapComponent_SacrificeTracker.Get(map: Map).lastResult = CultUtility.SacrificeResult.none;

            //Send a message about the gathering
            if (Map?.info?.parent is Settlement factionBase)
            {
                Messages.Message(text: "SacrificeGathering".Translate(arg1: factionBase.Label), lookTargets: TargetInfo.Invalid,
                    def: MessageTypeDefOf.NeutralEvent);
            }

            //Change the state
            ChangeState(type: State.sacrificing, sacrificeState: SacrificeState.started);

            //Create a new "bill" with all the variables from the sacrifice form
            sacrificeData = new Bill_Sacrifice(newSacrifice: tempSacrifice, newExecutioner: tempExecutioner, newEntity: tempCurrentSacrificeDeity,
                newSpell: tempCurrentSpell);
            Utility.DebugReport(x: "Force Sacrifice called");

            //Give the sacrifice job
            var job = new Job(def: CultsDefOf.Cults_HoldSacrifice, targetA: tempSacrifice, targetB: this) {count = 1};
            tempExecutioner.jobs.TryTakeOrderedJob(job: job);

            //Set the congregation
            sacrificeData.Congregation = GetSacrificeGroup();
            Utility.DebugReport(x: "Sacrifice state set to gathering");
        }

        private bool PreStartSacrificeReady()
        {
            if (Destroyed || !Spawned)
            {
                CultUtility.AbortCongregation(altar: null, reason: "The altar is unavailable.");
                return false;
            }

            if (!Utility.IsActorAvailable(preacher: tempExecutioner))
            {
                CultUtility.AbortCongregation(altar: this,
                    reason: "The executioner, " + tempExecutioner.LabelShort + " is unavaialable.");
                tempExecutioner = null;
                return false;
            }

            if (Utility.IsActorAvailable(preacher: tempSacrifice, downedAllowed: true))
            {
                return true;
            }

            CultUtility.AbortCongregation(altar: this, reason: "The sacrifice, " + tempSacrifice.LabelShort + " is unavaialable.");
            tempSacrifice = null;
            return false;
        }

        public static List<Pawn> GetSacrificeGroup(Building_SacrificialAltar altar)
        {
            return altar.GetSacrificeGroup();
        }

        public List<Pawn> GetSacrificeGroup()
        {
            var room = this.GetRoom();

            var pawns = new List<Pawn>();
            var availableWorshippers = AvailableWorshippers;
            if (room.Role == RoomRoleDefOf.PrisonBarracks || room.Role == RoomRoleDefOf.PrisonCell)
            {
                return pawns;
            }

            if (availableWorshippers == null || availableWorshippers.Count <= 0)
            {
                return pawns;
            }

            foreach (var p in availableWorshippers)
            {
                CultUtility.GiveAttendSacrificeJob(altar: this, attendee: p);
                pawns.Add(item: p);
                //this.Map.GetComponent<MapComponent_SacrificeTracker>().lastSacrificeCongregation.Add(p);
            }

            return pawns;
        }

        // RimWorld.WorkGiver_DoBill
        // Vanilla Code
        private bool TryFindBestOfferingIngredients(RecipeDef recipe, Pawn pawn, Building_SacrificialAltar billGiver,
            List<ThingCount> chosen)
        {
            chosen.Clear();
            var relevantThings = new List<Thing>();
            var ingredientsOrdered = new List<IngredientCount>();
            var newRelevantThings = new List<Thing>();
            if (recipe.ingredients.Count == 0)
            {
                //resultThings = relevantThings;
                return true;
            }

            var billGiverRootCell = billGiver.InteractionCell;
            var validRegionAt = Map.regionGrid.GetValidRegionAt(c: billGiverRootCell);
            if (validRegionAt == null)
            {
                Utility.DebugReport(x: "Invalid Region");

                //resultThings = relevantThings;
                return false;
            }

            ingredientsOrdered.Clear();
            if (recipe.productHasIngredientStuff)
            {
                Utility.DebugReport(x: recipe.ingredients[index: 0].ToString());
                ingredientsOrdered.Add(item: recipe.ingredients[index: 0]);
            }

            for (var i = 0; i < recipe.ingredients.Count; i++)
            {
                if (recipe.productHasIngredientStuff && i == 0)
                {
                    continue;
                }

                var ingredientCount = recipe.ingredients[index: i];
                if (ingredientCount.filter.AllowedDefCount != 1)
                {
                    continue;
                }

                Utility.DebugReport(x: ingredientCount.ToString());
                ingredientsOrdered.Add(item: ingredientCount);
            }

            foreach (var item in recipe.ingredients)
            {
                if (ingredientsOrdered.Contains(item: item))
                {
                    continue;
                }

                Utility.DebugReport(x: item.ToString());
                ingredientsOrdered.Add(item: item);
            }

            relevantThings.Clear();
            var foundAll = false;

            bool BaseValidator(Thing t)
            {
                return t.Spawned && !t.IsForbidden(pawn: pawn) &&
                       (t.Position - billGiver.Position).LengthHorizontalSquared < 999 * 999 &&
                       recipe.fixedIngredientFilter.Allows(t: t) && recipe.defaultIngredientFilter.Allows(t: t) &&
                       recipe.ingredients.Any(predicate: ingNeed => ingNeed.filter.Allows(t: t)) &&
                       pawn.CanReserve(target: t);
            }

            bool RegionProcessor(Region r)
            {
                newRelevantThings.Clear();
                var list = r.ListerThings.ThingsMatching(req: ThingRequest.ForGroup(@group: ThingRequestGroup.HaulableEver));
                foreach (var thing in list)
                {
                    if (!BaseValidator(t: thing))
                    {
                        continue;
                    }

                    Utility.DebugReport(x: thing.ToString());
                    newRelevantThings.Add(item: thing);
                }

                if (newRelevantThings.Count <= 0)
                {
                    return false;
                }

                int comparison(Thing t1, Thing t2)
                {
                    float lengthHorizontalSquared = (t1.Position - pawn.Position).LengthHorizontalSquared;
                    float lengthHorizontalSquared2 = (t2.Position - pawn.Position).LengthHorizontalSquared;
                    return lengthHorizontalSquared.CompareTo(value: lengthHorizontalSquared2);
                }

                newRelevantThings.Sort(comparison: comparison);
                relevantThings.AddRange(collection: newRelevantThings);
                newRelevantThings.Clear();
                var flag = true;
                foreach (var ingredientCount in recipe.ingredients)
                {
                    var num = ingredientCount.GetBaseCount();
                    foreach (var thing in relevantThings)
                    {
                        if (!ingredientCount.filter.Allows(t: thing))
                        {
                            continue;
                        }

                        Utility.DebugReport(x: thing.ToString());
                        var num2 = recipe.IngredientValueGetter.ValuePerUnitOf(t: thing.def);
                        var num3 = Mathf.Min(a: Mathf.CeilToInt(f: num / num2), b: thing.stackCount);
                        ThingCountUtility.AddToList(list: chosen, thing: thing, countToAdd: num3);
                        num -= num3 * num2;
                        if (num <= 0.0001f)
                        {
                            break;
                        }
                    }

                    if (num > 0.0001f)
                    {
                        flag = false;
                    }
                }

                if (!flag)
                {
                    return false;
                }

                foundAll = true;
                return true;
            }

            var traverseParams = TraverseParms.For(pawn: pawn);

            bool entryCondition(Region from, Region r)
            {
                return r.Allows(tp: traverseParams, isDestination: false);
            }

            RegionTraverser.BreadthFirstTraverse(root: validRegionAt, entryCondition: entryCondition, regionProcessor: RegionProcessor, maxRegions: 99999);
            return foundAll;
        }

        public bool TryDetermineOffering(CultUtility.SacrificeType type, CultUtility.OfferingSize size, Pawn pawn,
            Building_SacrificialAltar altar, out List<ThingCount> result, out RecipeDef resultRecipe)

        {
            var list = new List<ThingCount>();
            var recipeDefName = "OfferingOf";
            switch (type)
            {
                case CultUtility.SacrificeType.plants:
                    recipeDefName += "Plants";
                    MapComponent_SacrificeTracker.Get(map: Map).lastSacrificeType = CultUtility.SacrificeType.plants;
                    break;
                case CultUtility.SacrificeType.meat:
                    recipeDefName += "Meat";

                    MapComponent_SacrificeTracker.Get(map: Map).lastSacrificeType = CultUtility.SacrificeType.meat;
                    break;
                case CultUtility.SacrificeType.meals:
                    recipeDefName += "Meals";

                    MapComponent_SacrificeTracker.Get(map: Map).lastSacrificeType = CultUtility.SacrificeType.meals;
                    break;
            }

            switch (size)
            {
                case CultUtility.OfferingSize.meagre:
                    recipeDefName += "_Meagre";
                    break;
                case CultUtility.OfferingSize.decent:
                    recipeDefName += "_Decent";
                    break;
                case CultUtility.OfferingSize.sizable:
                    recipeDefName += "_Sizable";
                    break;
                case CultUtility.OfferingSize.worthy:
                    recipeDefName += "_Worthy";
                    break;
                case CultUtility.OfferingSize.impressive:
                    recipeDefName += "_Impressive";
                    break;
            }

            var recipe = DefDatabase<RecipeDef>.GetNamed(defName: recipeDefName);
            resultRecipe = recipe;
            if (!TryFindBestOfferingIngredients(recipe: recipe, pawn: pawn, billGiver: altar, chosen: list))
            {
                Messages.Message(text: "Failed to find offering ingredients", def: MessageTypeDefOf.RejectInput);
                result = null;
                return false;
            }

            result = list;
            return true;
        }
    }
}