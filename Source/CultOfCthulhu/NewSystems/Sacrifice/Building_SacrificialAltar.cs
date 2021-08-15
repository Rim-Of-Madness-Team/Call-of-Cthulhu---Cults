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
        public List<int> seasonSchedule = new List<int>(new int[15]);
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
            BillStack = new BillStack(this);
        }

        public IEnumerable<IntVec3> CellsAround => GenRadial.RadialCellsAround(Position, 5, true);
        public Bill_Sacrifice SacrificeData => sacrificeData;

        public string LastReport
        {
            get => lastReport;
            set => lastReport = value;
        }

        private bool DoMorningSermon =>
            OptionMorning && Utility.IsMorning(Map) && didMorningRitual == false;

        private bool DoEveningSermon =>
            OptionEvening && Utility.IsEvening(Map) && didEveningRitual == false;

        private bool DoSermonNow
        {
            get
            {
                var typeOfSermons = seasonSchedule[GenLocalDate.DayOfQuadrum(Map)];
                if (typeOfSermons == 0)
                {
                    return false;
                }

                var currentHour = GenLocalDate.HourInteger(Map);
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
                        new HashSet<Pawn>(Map.mapPawns.AllPawnsSpawned.FindAll(y => y is Pawn x &&
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

        public BillStack BillStack { get; }
        public IEnumerable<IntVec3> IngredientStackCells => GenAdj.CellsOccupiedBy(this);

        public override string GetInspectString()
        {
            var stringBuilder = new StringBuilder();

            // Add the inspections string from the base
            stringBuilder.Append(base.GetInspectString());

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
                    if (ResearchProjectDef.Named("Forbidden_Sacrifice").IsFinished)
                    {
                        return true;
                    }

                    return false;
                case Function.Level2:
                    if (ResearchProjectDef.Named("Forbidden_Human").IsFinished)
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
            Messages.Message(s, TargetInfo.Invalid, MessageTypeDefOf.RejectInput);
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
                Log.Error("Changed default state of Sacrificial Altar this should never happen.");
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
            s.Append("===================");
            s.AppendLine("Sacrifical Altar States Changed");
            s.AppendLine("===================");
            s.AppendLine("State: " + currentState);
            s.AppendLine("Worship: " + currentWorshipState);
            s.AppendLine("Offering: " + currentOfferingState);
            s.AppendLine("Sacrifice: " + currentSacrificeState);
            s.AppendLine("===================");
            Utility.DebugReport(s.ToString());
        }

        private Pawn GetCurOccupant()
        {
            var sleepingSlotPos = GetLyingSlotPos();
            var list = Map.thingGrid.ThingsListAt(sleepingSlotPos);
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
                return new IntVec3(cellRect.minX + index, Position.y, cellRect.minZ);
            }

            return Rotation == Rot4.East
                ? new IntVec3(cellRect.minX, Position.y, cellRect.maxZ - index)
                : Rotation == Rot4.South
                    ? new IntVec3(cellRect.minX + index, Position.y, cellRect.maxZ)
                    : new IntVec3(cellRect.maxX, Position.y, cellRect.maxZ - index);
        }

        public override void SpawnSetup(Map map, bool bla)
        {
            base.SpawnSetup(map, bla);
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

                seasonSchedule = new List<int>(new[]
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

            Scribe_Values.Look(ref RoomName, "RoomName");
            Scribe_Deep.Look(ref sacrificeData, "sacrificeData");
            Scribe_References.Look(ref tempCurrentSacrificeDeity, "tempCurrentSacrificeDeity");
            Scribe_References.Look(ref tempSacrifice, "tempSacrifice");
            Scribe_References.Look(ref tempExecutioner, "tempExecutioner");
            Scribe_Defs.Look(ref tempCurrentSpell, "tempCurrentSpell");
            Scribe_Values.Look(ref currentState, "currentState");
            Scribe_Values.Look(ref currentSacrificeState, "currentSacrificeState");
            Scribe_Values.Look(ref lastFunction, "lastFunction");
            Scribe_Values.Look(ref currentFunction, "currentFunction");
            // Worship values
            Scribe_References.Look(ref currentWorshipDeity, "currentWorshipDeity");
            Scribe_References.Look(ref tempCurrentWorshipDeity, "tempCurrentWorshipDeity");
            Scribe_References.Look(ref preacher, "preacher");
            Scribe_References.Look(ref tempPreacher, "tempPreacher");
            Scribe_Values.Look(ref currentWorshipState, "currentWorshipState");
            Scribe_Values.Look(ref OptionMorning, "OptionMorning");
            Scribe_Values.Look(ref OptionEvening, "OptionEvening");
            Scribe_Values.Look(ref didMorningRitual, "didMorningRitual");
            Scribe_Values.Look(ref didEveningRitual, "didEveningRitual");
            Scribe_Values.Look(ref morningHour, "morningHour", 9);
            Scribe_Values.Look(ref eveningHour, "eveningHour", 18);
            Scribe_Collections.Look(ref seasonSchedule, "seasonSchedule", LookMode.Value, false);
            //Misc
            Scribe_Values.Look(ref toBePrunedAndRepaired, "tobePrunedAndRepaired");
            Scribe_Values.Look(ref lastReport, "lastReport");
            Scribe_Values.Look(ref ticksToNextRepair, "ticksToNextRepair");
            Scribe_Values.Look(ref remainingDuration, "remainingDuration");
    }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            // block further ticker work
            destroyedFlag = true;
            ITab_AltarSacrificesCardUtility.Tab = ITab_AltarSacrificesCardUtility.SacrificeCardTab.Offering;
            base.Destroy(mode);
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
                    if (!Utility.IsActorAvailable(SacrificeData.Executioner))
                    {
                        CultUtility.AbortCongregation(this, "Executioner".Translate() + "IsUnavailable".Translate());
                        return;
                    }
                    else if (!Utility.IsActorAvailable(SacrificeData.Sacrifice, true))
                    {
                        CultUtility.AbortCongregation(this, "Sacrifice".Translate() + "IsUnavailable".Translate());
                        return;
                    }
                    else
                    {
                        if (SacrificeData.Executioner?.CurJob != null &&
                            SacrificeData.Executioner.CurJob.def != CultsDefOf.Cults_HoldSacrifice)
                        {
                            CultUtility.AbortCongregation(this,
                                "Executioner".Translate() + "IsUnavailable".Translate());
                            return;
                        }
                    }

                    if (availableWorshippers == null)
                    {
                        GetSacrificeGroup();
                    }

                    return;

                case SacrificeState.finishing:
                    if (!Utility.IsActorAvailable(SacrificeData.Executioner))
                    {
                        CultUtility.AbortCongregation(this, "Executioner".Translate() + "IsUnavailable".Translate());
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
                    ChangeState(State.notinuse);
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
                    if (!Utility.IsActorAvailable(offerer))
                    {
                        CultUtility.AbortCongregation(this, "Offerer".Translate() + "IsUnavailable".Translate());
                        return;
                    }
                    else if (offerer.CurJob.def != CultsDefOf.Cults_GiveOffering)
                    {
                        Utility.DebugReport(offerer.CurJob.def.defName);
                        CultUtility.AbortCongregation(this, "Offerer is not performing the task at hand.");
                        return;
                    }

                    return;
                case OfferingState.offering:
                    if (!Utility.IsActorAvailable(offerer))
                    {
                        CultUtility.AbortCongregation(this, "Offerer".Translate() + "IsUnavailable".Translate());
                        return;
                    }
                    else if (offerer.CurJob.def != CultsDefOf.Cults_GiveOffering)
                    {
                        Utility.DebugReport(offerer.CurJob.def.defName);
                        CultUtility.AbortCongregation(this, "Offerer is not performing the task at hand.");
                        return;
                    }

                    return;
                case OfferingState.finished:
                case OfferingState.off:
                    ChangeState(State.notinuse);
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
                    if (!Utility.IsActorAvailable(preacher))
                    {
                        CultUtility.AbortCongregation(this, "Preacher".Translate() + "IsUnavailable".Translate());
                        return;
                    }

                    if (preacher.CurJob.def != CultsDefOf.Cults_HoldWorship)
                    {
                        CultUtility.AbortCongregation(this, "Preacher".Translate() + "IsUnavailable".Translate());
                        return;
                    }

                    if (availableWorshippers != null)
                    {
                        return;
                    }

                    GetWorshipGroup(this,
                        GenRadial.RadialCellsAround(Position, GenRadial.MaxRadialPatternRadius - 1, true));
                    Utility.DebugReport("Gathering yay");
                    return;
                case WorshipState.finishing:
                    if (!Utility.IsActorAvailable(preacher))
                    {
                        CultUtility.AbortCongregation(this, "Preacher".Translate() + "IsUnavailable".Translate());
                        return;
                    }

                    if (preacher.CurJob.def != CultsDefOf.Cults_ReflectOnWorship)
                    {
                        return;
                    }

                    GetWorshipGroup(this,
                        GenRadial.RadialCellsAround(Position, GenRadial.MaxRadialPatternRadius - 1, true));
                    Utility.DebugReport("Finishing yay");
                    return;
                case WorshipState.finished:
                case WorshipState.off:
                    ChangeState(State.notinuse);
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
                    if (Utility.IsActorAvailable(SacrificeData.Executioner))
                    {
                        if (Utility.IsActorAvailable(SacrificeData.Sacrifice, true))
                        {
                            if (SacrificeData.Executioner.CurJob.def != CultsDefOf.Cults_HoldSacrifice)
                            {
                                CultUtility.AbortCongregation(this,
                                    "Executioner".Translate() + "IsUnavailable".Translate());
                            }

                            return;
                        }

                        CultUtility.AbortCongregation(this, "Sacrifice".Translate() + "IsUnavailable".Translate());
                        return;
                    }

                    CultUtility.AbortCongregation(this, "Executioner".Translate() + "IsUnavailable".Translate());
                    return;

                case SacrificeState.finishing:
                    if (!Utility.IsActorAvailable(SacrificeData.Executioner))
                    {
                        CultUtility.AbortCongregation(this, "Executioner".Translate() + "IsUnavailable".Translate());
                    }

                    if (MapComponent_SacrificeTracker.Get(Map).lastSacrificeType == CultUtility.SacrificeType.animal)
                    {
                        ChangeState(State.sacrificing, SacrificeState.finished);
                    }

                    return;

                case SacrificeState.finished:
                case SacrificeState.off:
                    ChangeState(State.notinuse);
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
                    if (Utility.IsActorAvailable(preacher))
                    {
                        if (preacher.CurJob.def != CultsDefOf.Cults_HoldWorship)
                        {
                            CultUtility.AbortCongregation(this, "Preacher".Translate() + "IsUnavailable".Translate());
                            return;
                        }
                    }

                    Utility.DebugReport("Gathering yay");
                    return;
                case WorshipState.finishing:
                    if (!Utility.IsActorAvailable(preacher))
                    {
                        CultUtility.AbortCongregation(this, "Preacher".Translate() + "IsUnavailable".Translate());
                    }

                    Utility.DebugReport("Finishing yay");
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
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/Worship")
                };
                if (CanUpgrade())
                {
                    if (currentFunction == Function.Level1)
                    {
                        command_Upgrade.icon = ContentFinder<Texture2D>.Get("UI/Commands/Upgrade2");
                    }
                    else if (currentFunction == Function.Level2)
                    {
                        command_Upgrade.icon = ContentFinder<Texture2D>.Get("UI/Commands/Upgrade3");
                    }
                }
                else
                {
                    if (currentFunction == Function.Level1)
                    {
                        command_Upgrade.icon = ContentFinder<Texture2D>.Get("UI/Commands/Upgrade2Disabled");
                    }
                    else if (currentFunction == Function.Level2)
                    {
                        command_Upgrade.icon = ContentFinder<Texture2D>.Get("UI/Commands/Upgrade3Disabled");
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
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/Sacrifice")
                };
                if (currentFunction < Function.Level2)
                {
                    command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/SacrificeDisabled");
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
                    icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel")
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
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/Worship")
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
                    icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel")
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
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/MakeOffering")
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
                    icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel")
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
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/CultReport")
                };
            }

            if (currentFunction == Function.Nightmare)
            {
                yield return new Command_Toggle
                {
                    hotKey = KeyBindingDefOf.Command_TogglePower,
                    icon = ContentFinder<Texture2D>.Get("UI/Icons/Commands/PruneAndRepair"),
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
                        CultUtility.AffectCultMindedness(p, 0.99f);
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
                        entity.AffectFavor(9999999);
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
                        let currentDef = IncidentDef.Named(spell.defName)
                        select new FloatMenuOption(currentDef.LabelCap, delegate
                        {
                            var temp = DefDatabase<IncidentDef>.GetNamed(spell.defName);
                            if (temp != null)
                            {
                                CultUtility.CastSpell(temp, Map);
                            }
                        })).ToList();
                    Find.WindowStack.Add(new FloatMenu(list));
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
            Find.WindowStack.Add(new Dialog_MessageBox(LastReport));
        }

        public void PruneAndRepairToggle()
        {
            toBePrunedAndRepaired = !toBePrunedAndRepaired;
        }

        private void TryUpgrade()
        {
            if (IsCongregating())
            {
                Messages.Message("UpgradeCongregationWarning".Translate(), MessageTypeDefOf.RejectInput);
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
                    Log.Error("Tried to upgrade fully functional altar. This should never happen.");
                    return;
            }

            if (newDefName == "")
            {
                return;
            }

            ReplaceAltarWith(newDefName);
        }

        public void NightmareEvent()
        {
            lastFunction = currentFunction;
            currentFunction = Function.Nightmare;
            ReplaceAltarWith("Cult_NightmareSacrificeAltar");
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

            var newAltar = ReplaceAltarWith(oldDefName);
            Messages.Message("PruningSuccessful".Translate(
                pruner.LabelShort
            ), MessageTypeDefOf.PositiveEvent);
            newAltar.Map.reservationManager.ReleaseAllForTarget(newAltar);
        }

        private Building_SacrificialAltar ReplaceAltarWith(string newDefName)
        {
            if (newDefName == "")
            {
                Utility.ErrorReport("ReplaceAltarWith :: Null exception.");
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
            var thing = (Building_SacrificialAltar) ThingMaker.MakeThing(ThingDef.Named(newDefName), currentStuff);
            var result = thing;
            thing.SetFaction(Faction.OfPlayer);
            thing.Rotation = currentRotation;
            GenPlace.TryPlaceThing(thing, currentLocation, currentMap, ThingPlaceMode.Direct);
            thing.Rotation = currentRotation;
            thing.TryGetComp<CompQuality>().SetQuality(qualityCat, ArtGenerationContext.Colony);
            thing.lastFunction = currentLastFunction;
            if (currentFunction != Function.Nightmare)
            {
                Messages.Message("UpgradeSuccessful".Translate(), new TargetInfo(currentLocation, Map),
                    MessageTypeDefOf.PositiveEvent);
            }
            else
            {
                Messages.Message("CorruptedAltarWarning".Translate(), MessageTypeDefOf.NegativeEvent);
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
                Map.mapPawns.AllPawnsSpawned.FindAll(x => x.RaceProps.intelligence == Intelligence.Humanlike);
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

            ChangeState(State.notinuse);
            //this.currentState = State.off;
            Messages.Message("Cancelling offering.", MessageTypeDefOf.NegativeEvent);
        }

        private void TryOffering()
        {
            if (IsCongregating())
            {
                Messages.Message("A congregation is already gathering.", MessageTypeDefOf.RejectInput);
                return;
            }

            if (!CanGatherOfferingNow())
            {
                return;
            }

            if (!TryDetermineOffering(tempOfferingType, tempOfferingSize, tempOfferer, this,
                out tempDeterminedOfferings, out billRecipe))
            {
                Utility.DebugReport("Failed to determine offering");
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
                    Messages.Message("An offering is already happening.", TargetInfo.Invalid,
                        MessageTypeDefOf.RejectInput);
                    return;
            }
        }

        private bool CanGatherOfferingNow()
        {
            if (tempOfferingType == CultUtility.SacrificeType.none)
            {
                return RejectMessage("No offering type selected");
            }

            if (tempOfferingSize == CultUtility.OfferingSize.none)
            {
                return RejectMessage("No offering amount selected");
            }

            if (tempOfferer == null)
            {
                return RejectMessage("No offerer selected");
            }

            if (tempOfferer.Drafted)
            {
                return RejectMessage("Offerer is drafted.");
            }

            if (tempOfferer.Dead || tempOfferer.Downed)
            {
                return RejectMessage("Select an able-bodied offerer.", tempOfferer);
            }

            if (tempCurrentOfferingDeity == null)
            {
                return RejectMessage(
                    "No cosmic entity selected. Entities can be discovered at the forbidden knowledge center.");
            }

            return !tempOfferer.CanReserve(this)
                ? RejectMessage("The altar is reserved by something else.")
                : !Position.GetThingList(Map).OfType<Corpse>().Any() ||
                  RejectMessage("The altar needs to be cleared first.");
        }

        public void StartOffering()
        {
            determinedOfferings = tempDeterminedOfferings;
            offerer = tempOfferer;
            currentOfferingDeity = tempCurrentOfferingDeity;

            if (Destroyed || !Spawned)
            {
                CultUtility.AbortCongregation(null, "Altar".Translate() + "IsUnavailable".Translate());
                return;
            }

            if (!Utility.IsActorAvailable(offerer))
            {
                CultUtility.AbortCongregation(this,
                    "Offerer".Translate() + " " + offerer.LabelShort + "IsUnavailable".Translate());
                offerer = null;
                return;
            }

            Messages.Message("An offering is being gathered.", TargetInfo.Invalid, MessageTypeDefOf.NeutralEvent);
            ChangeState(State.offering, OfferingState.started);
            Utility.DebugReport("Make offering called.");

            var job2 = new Job(CultsDefOf.Cults_GiveOffering)
            {
                playerForced = true,
                targetA = this,
                targetQueueB = new List<LocalTargetInfo>(determinedOfferings.Count),
                targetC = PositionHeld,
                countQueue = new List<int>(determinedOfferings.Count)
            };
            foreach (var thingCount in determinedOfferings)
            {
                job2.targetQueueB.Add(thingCount.Thing);
                job2.countQueue.Add(thingCount.Count);
            }

            job2.haulMode = HaulMode.ToCellNonStorage;
            job2.locomotionUrgency = LocomotionUrgency.Sprint;
            job2.bill = new Bill_Production(billRecipe);
            offerer.jobs.TryTakeOrderedJob(job2);
        }

        private void CancelSacrifice()
        {
            var listeners =
                Map.mapPawns.AllPawnsSpawned.FindAll(x => x.RaceProps.intelligence == Intelligence.Humanlike);
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

            ChangeState(State.notinuse);
            Messages.Message("Cancelling sacrifice.", MessageTypeDefOf.NegativeEvent);
        }

        private void TrySacrifice()
        {
            if (IsSacrificing())
            {
                Messages.Message("A sacrifice is already gathering.", MessageTypeDefOf.RejectInput);
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
                    Messages.Message("A sacrifice is already gathering.", TargetInfo.Invalid,
                        MessageTypeDefOf.RejectInput);
                    return;
            }
        }

        private bool CanGatherSacrificeNow()
        {
            if (tempSacrifice == null)
            {
                return RejectMessage("No prisoner to sacrifice selected.");
            }

            if (tempExecutioner == null)
            {
                return RejectMessage("No executioner selected");
            }

            if (tempExecutioner.Drafted)
            {
                return RejectMessage("The executioner is drafted.");
            }

            if (tempCurrentSacrificeDeity == null)
            {
                return RejectMessage("No cosmic entity selected");
            }

            if (MapComponent_SacrificeTracker.Get(Map).lastSacrificeType != CultUtility.SacrificeType.animal &&
                tempCurrentSpell == null)
            {
                return RejectMessage("No spell selected. Tip: Earn favor to unlock spells.");
            }

            if (tempSacrifice.Dead)
            {
                return RejectMessage("The sacrifice is already dead", tempSacrifice);
            }

            if (tempExecutioner.Dead || tempExecutioner.Downed)
            {
                return RejectMessage("Select an able-bodied executioner");
            }

            if (!tempExecutioner.CanReserve(tempSacrifice))
            {
                return RejectMessage("The executioner can't reserve the sacrifice.");
            }

            if (!tempExecutioner.CanReserve(this))
            {
                return RejectMessage("The altar is reserved by something else");
            }

            if (Position.GetThingList(Map).OfType<Corpse>().Any())
            {
                return RejectMessage("The altar needs to be cleared first.");
            }

            return MapComponent_SacrificeTracker.Get(Map).lastSacrificeType != CultUtility.SacrificeType.human
                   || tempCurrentSpell.Worker is SpellWorker worker && worker.CanSummonNow(Map);
        }

        private void StartSacrifice()
        {
            //Check for missing actors
            if (!PreStartSacrificeReady())
            {
                return;
            }

            //Reset results
            MapComponent_SacrificeTracker.Get(Map).lastResult = CultUtility.SacrificeResult.none;

            //Send a message about the gathering
            if (Map?.info?.parent is Settlement factionBase)
            {
                Messages.Message("SacrificeGathering".Translate(factionBase.Label), TargetInfo.Invalid,
                    MessageTypeDefOf.NeutralEvent);
            }

            //Change the state
            ChangeState(State.sacrificing, SacrificeState.started);

            //Create a new "bill" with all the variables from the sacrifice form
            sacrificeData = new Bill_Sacrifice(tempSacrifice, tempExecutioner, tempCurrentSacrificeDeity,
                tempCurrentSpell);
            Utility.DebugReport("Force Sacrifice called");

            //Give the sacrifice job
            var job = new Job(CultsDefOf.Cults_HoldSacrifice, tempSacrifice, this) {count = 1};
            tempExecutioner.jobs.TryTakeOrderedJob(job);

            //Set the congregation
            sacrificeData.Congregation = GetSacrificeGroup();
            Utility.DebugReport("Sacrifice state set to gathering");
        }

        private bool PreStartSacrificeReady()
        {
            if (Destroyed || !Spawned)
            {
                CultUtility.AbortCongregation(null, "The altar is unavailable.");
                return false;
            }

            if (!Utility.IsActorAvailable(tempExecutioner))
            {
                CultUtility.AbortCongregation(this,
                    "The executioner, " + tempExecutioner.LabelShort + " is unavaialable.");
                tempExecutioner = null;
                return false;
            }

            if (Utility.IsActorAvailable(tempSacrifice, true))
            {
                return true;
            }

            CultUtility.AbortCongregation(this, "The sacrifice, " + tempSacrifice.LabelShort + " is unavaialable.");
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
                CultUtility.GiveAttendSacrificeJob(this, p);
                pawns.Add(p);
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
            var validRegionAt = Map.regionGrid.GetValidRegionAt(billGiverRootCell);
            if (validRegionAt == null)
            {
                Utility.DebugReport("Invalid Region");

                //resultThings = relevantThings;
                return false;
            }

            ingredientsOrdered.Clear();
            if (recipe.productHasIngredientStuff)
            {
                Utility.DebugReport(recipe.ingredients[0].ToString());
                ingredientsOrdered.Add(recipe.ingredients[0]);
            }

            for (var i = 0; i < recipe.ingredients.Count; i++)
            {
                if (recipe.productHasIngredientStuff && i == 0)
                {
                    continue;
                }

                var ingredientCount = recipe.ingredients[i];
                if (ingredientCount.filter.AllowedDefCount != 1)
                {
                    continue;
                }

                Utility.DebugReport(ingredientCount.ToString());
                ingredientsOrdered.Add(ingredientCount);
            }

            foreach (var item in recipe.ingredients)
            {
                if (ingredientsOrdered.Contains(item))
                {
                    continue;
                }

                Utility.DebugReport(item.ToString());
                ingredientsOrdered.Add(item);
            }

            relevantThings.Clear();
            var foundAll = false;

            bool BaseValidator(Thing t)
            {
                return t.Spawned && !t.IsForbidden(pawn) &&
                       (t.Position - billGiver.Position).LengthHorizontalSquared < 999 * 999 &&
                       recipe.fixedIngredientFilter.Allows(t) && recipe.defaultIngredientFilter.Allows(t) &&
                       recipe.ingredients.Any(ingNeed => ingNeed.filter.Allows(t)) &&
                       pawn.CanReserve(t);
            }

            bool RegionProcessor(Region r)
            {
                newRelevantThings.Clear();
                var list = r.ListerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.HaulableEver));
                foreach (var thing in list)
                {
                    if (!BaseValidator(thing))
                    {
                        continue;
                    }

                    Utility.DebugReport(thing.ToString());
                    newRelevantThings.Add(thing);
                }

                if (newRelevantThings.Count <= 0)
                {
                    return false;
                }

                int comparison(Thing t1, Thing t2)
                {
                    float lengthHorizontalSquared = (t1.Position - pawn.Position).LengthHorizontalSquared;
                    float lengthHorizontalSquared2 = (t2.Position - pawn.Position).LengthHorizontalSquared;
                    return lengthHorizontalSquared.CompareTo(lengthHorizontalSquared2);
                }

                newRelevantThings.Sort(comparison);
                relevantThings.AddRange(newRelevantThings);
                newRelevantThings.Clear();
                var flag = true;
                foreach (var ingredientCount in recipe.ingredients)
                {
                    var num = ingredientCount.GetBaseCount();
                    foreach (var thing in relevantThings)
                    {
                        if (!ingredientCount.filter.Allows(thing))
                        {
                            continue;
                        }

                        Utility.DebugReport(thing.ToString());
                        var num2 = recipe.IngredientValueGetter.ValuePerUnitOf(thing.def);
                        var num3 = Mathf.Min(Mathf.CeilToInt(num / num2), thing.stackCount);
                        ThingCountUtility.AddToList(chosen, thing, num3);
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

            var traverseParams = TraverseParms.For(pawn);

            bool entryCondition(Region from, Region r)
            {
                return r.Allows(traverseParams, false);
            }

            RegionTraverser.BreadthFirstTraverse(validRegionAt, entryCondition, RegionProcessor, 99999);
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
                    MapComponent_SacrificeTracker.Get(Map).lastSacrificeType = CultUtility.SacrificeType.plants;
                    break;
                case CultUtility.SacrificeType.meat:
                    recipeDefName += "Meat";

                    MapComponent_SacrificeTracker.Get(Map).lastSacrificeType = CultUtility.SacrificeType.meat;
                    break;
                case CultUtility.SacrificeType.meals:
                    recipeDefName += "Meals";

                    MapComponent_SacrificeTracker.Get(Map).lastSacrificeType = CultUtility.SacrificeType.meals;
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

            var recipe = DefDatabase<RecipeDef>.GetNamed(recipeDefName);
            resultRecipe = recipe;
            if (!TryFindBestOfferingIngredients(recipe, pawn, altar, list))
            {
                Messages.Message("Failed to find offering ingredients", MessageTypeDefOf.RejectInput);
                result = null;
                return false;
            }

            result = list;
            return true;
        }
    }
}