// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------
using System;
using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
using System.Text;

// ----------------------------------------------------------------------
// These are RimWorld-specific usings. Activate/Deactivate what you need:
// ----------------------------------------------------------------------
using UnityEngine;         // Always needed
//using VerseBase;         // Material/Graphics handling functions are found here
using Verse;               // RimWorld universal objects are here (like 'Building')
using Verse.AI;          // Needed when you do something with the AI
//using Verse.AI.Group;
//using Verse.Sound;       // Needed when you do something with Sound
//using Verse.Noise;       // Needed when you do something with Noises
using RimWorld;            // RimWorld specific functions are found here (like 'Building_Battery')
using RimWorld.Planet;
//using RimWorld.Planet;   // RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

namespace CultOfCthulhu
{
    public partial class Building_SacrificialAltar : Building, IBillGiver
    {
        #region IBillGiver
        public BillStack billStack;

        public bool CurrentlyUsable()
        {
            return true;
        }
        public IEnumerable<IntVec3> CellsAround
        {
            get
            {
                return GenRadial.RadialCellsAround(base.Position, 5, true);
            }
        }
        public BillStack BillStack
        {
            get
            {
                return this.billStack;
            }
        }
        public IEnumerable<IntVec3> IngredientStackCells
        {
            get
            {
                return GenAdj.CellsOccupiedBy(this);
            }
        }
        public Building_SacrificialAltar()
        {
            this.billStack = new BillStack(this);
        }
        #endregion IBillGiver

        //Universal Variables
        public enum State { notinuse = 0, sacrificing, worshipping, offering };
        public enum Function { Level1 = 0, Level2 = 1, Level3 = 2, Nightmare = 3 };
        public Function currentFunction = Function.Level1;
        public State currentState = State.notinuse;
        private bool destroyedFlag = false;              // For safety
        public string RoomName = "Unnamed Temple";


        //Offering Related Variables
        public enum OfferingState { off = 0, started, offering, finished };
        public OfferingState currentOfferingState = OfferingState.off;
        public Pawn tempOfferer;
        public Pawn offerer;
        public CosmicEntity currentOfferingDeity;
        public CosmicEntity tempCurrentOfferingDeity;
        public CultUtility.SacrificeType tempOfferingType = CultUtility.SacrificeType.none;
        public CultUtility.OfferingSize tempOfferingSize = CultUtility.OfferingSize.none;
        public List<ThingAmount> tempDeterminedOfferings = new List<ThingAmount>();
        public List<ThingAmount> determinedOfferings = new List<ThingAmount>();
        public RecipeDef billRecipe = null;

        //Worship Related Variables
        public enum WorshipState { off = 0, started, gathering, worshipping, finishing, finished };
        public WorshipState currentWorshipState = WorshipState.off;

        public bool OptionMorning = false;
        public bool didMorningRitual = false;
        public bool OptionEvening = false;
        public bool didEveningRitual = false;

        public Pawn preacher = null;
        public Pawn tempPreacher = null;
        public CosmicEntity currentWorshipDeity;
        public CosmicEntity tempCurrentWorshipDeity;

        //Sacrifice Related Variables
        public enum SacrificeState { off = 0, started, gathering, sacrificing, finishing, finished };
        public SacrificeState currentSacrificeState = SacrificeState.off;
        public Pawn sacrifice = null;                   // Current sacrifice
        public Pawn tempSacrifice = null;
        public Pawn executioner = null;                 // Current executioner
        public Pawn tempExecutioner = null;
        public CosmicEntity currentSacrificeDeity;      // Actual deity
        public CosmicEntity tempCurrentSacrificeDeity;
        public IncidentDef currentSpell = null;
        public IncidentDef tempCurrentSpell = null;

        //Misc Event Variables
        public Function lastFunction = Function.Level1;
        public bool toBePrunedAndRepaired = false;

        public bool debugAlwaysSucceed = false;


        #region Bools
        public bool IsCongregating()
        {
            if (IsOffering() || IsSacrificing() || IsWorshipping()) return true;
            return false;
        }
        public bool IsOffering()
        {
            if ((currentState != State.offering) && (currentOfferingState == OfferingState.finished || currentOfferingState == OfferingState.off))
            {
                return false;
            }
            return true;
        }
        public bool IsSacrificing()
        {
            if ((currentState != State.sacrificing) && (currentSacrificeState == SacrificeState.finished || currentSacrificeState == SacrificeState.off))
            {
                return false;
            }
            return true;
        }
        public bool IsWorshipping()
        {
            if ((currentState != State.worshipping) &&
                (currentWorshipState == WorshipState.finished || currentWorshipState == WorshipState.off))
            {
                return false;
            }
            return true;
        }
        public bool DoMorningSermon
        {
            get
            {
                if (OptionMorning && (Cthulhu.Utility.IsMorning(Map)) && (didMorningRitual == false))
                {
                    return true;
                }
                return false;
            }
        }
        public bool DoEveningSermon
        {
            get
            {
                if (OptionEvening && (Cthulhu.Utility.IsEvening(Map)) && (didEveningRitual == false))
                {
                    return true;
                }
                return false;
            }
        }
        public bool CanUpgrade()
        {
            switch (currentFunction)
            {
                case Function.Level1:
                    if (ResearchProjectDef.Named("Forbidden_Sacrifice").IsFinished) return true;
                    return false;
                case Function.Level2:
                    if (ResearchProjectDef.Named("Forbidden_Human").IsFinished) return true;
                    return false;
                case Function.Level3:
                    return false;
            }
            return false;
        }
        private bool RejectMessage(string s, Pawn pawn = null)
        {
            Messages.Message(s, TargetInfo.Invalid, MessageTypeDefOf.RejectInput);
            if (pawn != null) pawn = null;
            return false;
        }
        #endregion Bools

        #region State
        public void ChangeState(State type)
        {
            if (type == State.notinuse)
            {
                this.currentState = type;
                this.currentWorshipState = WorshipState.off;
                this.currentSacrificeState = SacrificeState.off;
                this.currentOfferingState = OfferingState.off;
                this.availableWorshippers = null;
            }
            else Log.Error("Changed default state of Sacrificial Altar this should never happen.");
            ReportState();
        }
        public void ChangeState(State type, WorshipState worshipState)
        {
            this.currentState = type;
            this.currentWorshipState = worshipState;
            ReportState();
        }
        public void ChangeState(State type, SacrificeState sacrificeState)
        {
            this.currentState = type;
            this.currentSacrificeState = sacrificeState;
            ReportState();
        }
        public void ChangeState(State type, OfferingState offeringState)
        {
            this.currentState = type;
            this.currentOfferingState = offeringState;
            ReportState();
        }
        public void ReportState()
        {
            StringBuilder s = new StringBuilder();
            s.Append("===================");
            s.AppendLine();
            s.Append("Sacrifical Altar States Changed");
            s.AppendLine();
            s.Append("===================");
            s.AppendLine();
            s.Append("State: " + currentState.ToString());
            s.AppendLine();
            s.Append("Worship: " + currentWorshipState.ToString());
            s.AppendLine();
            s.Append("Offering: " + currentOfferingState.ToString());
            s.AppendLine();
            s.Append("Sacrifice: " + currentSacrificeState.ToString());
            s.AppendLine();
            s.Append("===================");
            Cthulhu.Utility.DebugReport(s.ToString());
        }
        #endregion State

        #region Bed-Like
        // RimWorld.Building_Bed
        public int LyingSlotsCount
        {
            get
            {
                return 1;
            }
        }

        // RimWorld.Building_Bed
        public bool AnyUnoccupiedLyingSlot
        {
            get
            {
                for (int i = 0; i < this.LyingSlotsCount; i++)
                {
                    if (this.GetCurOccupant() == null)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        // RimWorld.Building_Bed
        public Pawn GetCurOccupant()
        {
            IntVec3 sleepingSlotPos = this.GetLyingSlotPos();
            List<Thing> list = Map.thingGrid.ThingsListAt(sleepingSlotPos);
            for (int i = 0; i < list.Count; i++)
            {
                Pawn pawn = list[i] as Pawn;
                if (pawn != null)
                {
                    if (pawn.CurJob != null)
                    {
                        if (pawn.jobs.curDriver.layingDown == LayingDownState.LayingInBed ||
                            pawn.jobs.curDriver.layingDown == LayingDownState.LayingSurface)
                        {
                            return pawn;
                        }
                    }
                }
            }
            return null;
        }

        // RimWorld.Building_Bed
        public IntVec3 GetLyingSlotPos()
        {
            int index = 1;
            CellRect cellRect = this.OccupiedRect();
            if (base.Rotation == Rot4.North)
            {
                return new IntVec3(cellRect.minX + index, base.Position.y, cellRect.minZ);
            }
            if (base.Rotation == Rot4.East)
            {
                return new IntVec3(cellRect.minX, base.Position.y, cellRect.maxZ - index);
            }
            if (base.Rotation == Rot4.South)
            {
                return new IntVec3(cellRect.minX + index, base.Position.y, cellRect.maxZ);
            }
            return new IntVec3(cellRect.maxX, base.Position.y, cellRect.maxZ - index);
        }

        #endregion Bed-Like

        #region Spawn
        public override void SpawnSetup(Map map, bool bla)
        {
            base.SpawnSetup(map, bla);
            CultTracker.Get.ExposedToCults = true;
            if (RoomName == null) RoomName = "Unnamed Temple";
            DeityTracker.Get.orGenerate();
            if (this.def.defName == "Cult_AnimalSacrificeAltar") currentFunction = Function.Level2;
            if (this.def.defName == "Cult_HumanSacrificeAltar") currentFunction = Function.Level3;
            if (this.def.defName == "Cult_NightmareSacrificeAltar") currentFunction = Function.Nightmare;
            //UpdateGraphics();
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<string>(ref this.RoomName, "RoomName", null);
            Scribe_References.Look<CosmicEntity>(ref this.currentSacrificeDeity, "currentSacrificeDeity");
            Scribe_References.Look<Pawn>(ref this.sacrifice, "sacrifice");
            Scribe_References.Look<Pawn>(ref this.executioner, "executioner");
            Scribe_Defs.Look<IncidentDef>(ref this.currentSpell, "currentSpell");
            Scribe_Values.Look<State>(ref this.currentState, "currentState", State.notinuse);
            Scribe_Values.Look<SacrificeState>(ref this.currentSacrificeState, "currentSacrificeState", SacrificeState.off);
            Scribe_Values.Look<Function>(ref this.lastFunction, "lastFunction", Function.Level1);
            Scribe_Values.Look<Function>(ref this.currentFunction, "currentFunction", Function.Level1);
            ///Worship values
            Scribe_References.Look<CosmicEntity>(ref this.currentWorshipDeity, "currentWorshipDeity");
            Scribe_References.Look<CosmicEntity>(ref this.tempCurrentWorshipDeity, "tempCurrentWorshipDeity");
            Scribe_References.Look<Pawn>(ref this.preacher, "preacher");
            Scribe_References.Look<Pawn>(ref this.tempPreacher, "tempPreacher");
            Scribe_Values.Look<WorshipState>(ref this.currentWorshipState, "currentWorshipState", WorshipState.off);
            Scribe_Values.Look<bool>(ref this.OptionMorning, "OptionMorning", false);
            Scribe_Values.Look<bool>(ref this.OptionEvening, "OptionEvening", false);
            Scribe_Values.Look<bool>(ref this.didMorningRitual, "didMorningRitual", false);
            Scribe_Values.Look<bool>(ref this.didEveningRitual, "didEveningRitual", false);
            //Misc
            Scribe_Values.Look<bool>(ref this.toBePrunedAndRepaired, "tobePrunedAndRepaired", false);
        }
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            // block further ticker work
            destroyedFlag = true;

            base.Destroy(mode);
        }
        #endregion Spawn

        #region Ticker
        public override void TickLong()
        {
            base.TickLong();

        }

        public override void TickRare()
        {
            if (destroyedFlag) // Do nothing further, when destroyed (just a safety)
                return;


            if (!this.Spawned) return;

            // Don't forget the base work
            base.TickRare();
            AutoWorshipRareTick();
            WorshipRareTick();
            OfferingRareTick();
            if (currentFunction > Function.Level1) SacrificeRareTick();

        }

        public override void Tick()
        {
            if (destroyedFlag) // Do nothing further, when destroyed (just a safety)
                return;

            if (!this.Spawned) return;

            // Don't forget the base work
            base.Tick();
            WorshipTick();
            if (currentFunction > Function.Level1) SacrificeTick();
        }

        public void AutoWorshipRareTick()
        {
            //In the morning, let's gather to worship
            if (DoMorningSermon && (!IsWorshipping() && !IsSacrificing()))
            {
                didMorningRitual = true;
                TryTimedWorship();
            }

            //In the evening, let's gather to worship
            if (DoEveningSermon && (!IsWorshipping() && !IsSacrificing()))
            {
                didEveningRitual = true;
                TryTimedWorship();
            }

            //Reset values
            if (!Cthulhu.Utility.IsEvening(Map) && !Cthulhu.Utility.IsMorning(Map))
            {
                didEveningRitual = false;
                didMorningRitual = false;
            }
        }
        public void SacrificeRareTick()
        {
            if (!this.Spawned) return;
            if (executioner == null) return;
            if (currentState == State.sacrificing)
            {
                switch (currentSacrificeState)
                {
                    case SacrificeState.started:
                    case SacrificeState.gathering:
                    case SacrificeState.sacrificing:
                        if (!Cthulhu.Utility.IsActorAvailable(this.executioner))
                        {
                            CultUtility.AbortCongregation(this, "Executioner".Translate() + "IsUnavailable".Translate());
                            return;
                        }
                        else if (!Cthulhu.Utility.IsActorAvailable(this.sacrifice, true))
                        {
                            CultUtility.AbortCongregation(this, "Sacrifice".Translate() + "IsUnavailable".Translate());
                            return;
                        }
                        else if (this.executioner != null)
                        {
                            if (this.executioner.CurJob != null)
                            {
                                if (this.executioner.CurJob.def != CultsDefOf.Cults_HoldSacrifice)
                                {
                                    CultUtility.AbortCongregation(this, "Executioner".Translate() + "IsUnavailable".Translate());
                                    return;
                                }
                            }
                        }
                        if (availableWorshippers == null)
                        {
                            GetSacrificeGroup();
                        }
                        return;

                    case SacrificeState.finishing:
                        if (!Cthulhu.Utility.IsActorAvailable(this.executioner))
                        {
                            CultUtility.AbortCongregation(this, "Executioner".Translate() + "IsUnavailable".Translate());
                            return;
                        }
                        if (this.executioner != null)
                        {
                            if (this.executioner.CurJob != null)
                            {
                                if (this.executioner.CurJob.def != CultsDefOf.Cults_ReflectOnResult) return;
                            }
                        }
                        GetSacrificeGroup();
                        return;

                    case SacrificeState.finished:
                    case SacrificeState.off:
                        ChangeState(State.notinuse);
                        return;
                }
            }
        }
        public void OfferingRareTick()
        {
            if (currentState == State.offering)
            {
                switch (currentOfferingState)
                {
                    case OfferingState.started:
                        if (!Cthulhu.Utility.IsActorAvailable(this.offerer))
                        {
                            CultUtility.AbortCongregation(this, "Offerer".Translate() + "IsUnavailable".Translate());
                            return;
                        }
                        else if (this.offerer.CurJob.def != CultsDefOf.Cults_GiveOffering)
                        {
                            CultUtility.AbortCongregation(this, "Offerer is not performing the task at hand.");
                            return;
                        }
                        return;
                    case OfferingState.offering:
                        if (!Cthulhu.Utility.IsActorAvailable(this.offerer))
                        {
                            CultUtility.AbortCongregation(this, "Offerer".Translate() + "IsUnavailable".Translate());
                            return;
                        }
                        else if (this.offerer.CurJob.def != CultsDefOf.Cults_GiveOffering)
                        {
                            Cthulhu.Utility.DebugReport(this.offerer.CurJob.def.defName);
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
        }
        public void WorshipRareTick()
        {
            if (currentState == State.worshipping)
            {
                switch (currentWorshipState)
                {
                    case WorshipState.started:
                    case WorshipState.gathering:
                    case WorshipState.worshipping:
                        if (!Cthulhu.Utility.IsActorAvailable(this.preacher))
                        {
                            CultUtility.AbortCongregation(this, "Preacher".Translate() + "IsUnavailable".Translate());
                            return;
                        }
                        if (this.preacher.CurJob.def != CultsDefOf.Cults_HoldWorship)
                        {
                            CultUtility.AbortCongregation(this, "Preacher".Translate() + "IsUnavailable".Translate());
                            return;
                        }
                        if (availableWorshippers == null)
                        {
                            GetWorshipGroup(this, GenRadial.RadialCellsAround(base.Position, GenRadial.MaxRadialPatternRadius - 1, true));
                            Cthulhu.Utility.DebugReport("Gathering yay");
                        }
                        return;
                    case WorshipState.finishing:
                        if (!Cthulhu.Utility.IsActorAvailable(this.preacher))
                        {
                            CultUtility.AbortCongregation(this, "Preacher".Translate() + "IsUnavailable".Translate());
                            return;
                        }
                        if (this.preacher.CurJob.def != CultsDefOf.Cults_ReflectOnWorship)
                            return;
                        GetWorshipGroup(this, GenRadial.RadialCellsAround(base.Position, GenRadial.MaxRadialPatternRadius - 1, true));
                        Cthulhu.Utility.DebugReport("Finishing yay");
                        return;
                    case WorshipState.finished:
                    case WorshipState.off:
                        ChangeState(State.notinuse);
                        return;
                }
            }
        }
        public void SacrificeTick()
        {
            if (currentState == State.sacrificing)
            {
                switch (currentSacrificeState)
                {
                    case SacrificeState.started:
                    case SacrificeState.gathering:
                    case SacrificeState.sacrificing:
                        if (Cthulhu.Utility.IsActorAvailable(this.executioner))
                        {
                            if (Cthulhu.Utility.IsActorAvailable(this.sacrifice, true))
                            {
                                if (this.executioner.CurJob.def != CultsDefOf.Cults_HoldSacrifice)
                                    CultUtility.AbortCongregation(this, "Executioner".Translate() + "IsUnavailable".Translate());
                                return;
                            }
                            CultUtility.AbortCongregation(this, "Sacrifice".Translate() + "IsUnavailable".Translate());
                            return;
                        }
                        CultUtility.AbortCongregation(this, "Executioner".Translate() + "IsUnavailable".Translate());
                        return;

                    case SacrificeState.finishing:
                        if (!Cthulhu.Utility.IsActorAvailable(this.executioner))
                        {
                            CultUtility.AbortCongregation(this, "Executioner".Translate() + "IsUnavailable".Translate());
                        }
                        if (Map.GetComponent<MapComponent_SacrificeTracker>().lastSacrificeType == CultUtility.SacrificeType.animal)
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
        }
        public void WorshipTick()
        {
            if (currentState == State.worshipping)
            {
                switch (currentWorshipState)
                {
                    case WorshipState.started:
                    case WorshipState.gathering:
                        if (Cthulhu.Utility.IsActorAvailable(this.preacher))
                        {
                            if (this.preacher.CurJob.def != CultsDefOf.Cults_HoldWorship)
                            {
                                CultUtility.AbortCongregation(this, "Preacher".Translate() + "IsUnavailable".Translate());
                                return;
                            }
                        }
                        Cthulhu.Utility.DebugReport("Gathering yay");
                        return;
                    case WorshipState.finishing:
                        if (!Cthulhu.Utility.IsActorAvailable(this.preacher))
                        {
                            CultUtility.AbortCongregation(this, "Preacher".Translate() + "IsUnavailable".Translate());
                        }
                        Cthulhu.Utility.DebugReport("Finishing yay");
                        return;
                    case WorshipState.finished:
                    case WorshipState.off:
                        currentState = State.notinuse;
                        return;
                }
            }
        }


        #endregion Ticker

        #region Inspect
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            // Add the inspections string from the base
            stringBuilder.Append(base.GetInspectString());

            // return the complete string
            return stringBuilder.ToString();
        }
        #endregion Inspect

        #region Gizmos
        public override IEnumerable<Gizmo> GetGizmos()
        {
            IEnumerator<Gizmo> enumerator = base.GetGizmos().GetEnumerator();
            while (enumerator.MoveNext())
            {
                Gizmo current = enumerator.Current;
                yield return current;
            }

            if (currentFunction < Function.Level3)
            {
                Command_Action command_Upgrade = new Command_Action();
                command_Upgrade.action = new Action(this.TryUpgrade);
                command_Upgrade.defaultLabel = "CommandUpgrade".Translate();
                command_Upgrade.defaultDesc = "CommandUpgrade".Translate();
                command_Upgrade.disabled = (!CanUpgrade() || currentFunction == Function.Nightmare);
                command_Upgrade.disabledReason = "CommandCultDisabled".Translate();
                command_Upgrade.hotKey = KeyBindingDefOf.Misc1;
                command_Upgrade.icon = ContentFinder<Texture2D>.Get("UI/Commands/Worship", true);
                if (CanUpgrade())
                {
                    if (currentFunction == Function.Level1)
                        command_Upgrade.icon = ContentFinder<Texture2D>.Get("UI/Commands/Upgrade2", true);
                    if (currentFunction == Function.Level2)
                        command_Upgrade.icon = ContentFinder<Texture2D>.Get("UI/Commands/Upgrade3", true);
                }
                else
                {
                    if (currentFunction == Function.Level1)
                        command_Upgrade.icon = ContentFinder<Texture2D>.Get("UI/Commands/Upgrade2Disabled", true);
                    if (currentFunction == Function.Level2)
                        command_Upgrade.icon = ContentFinder<Texture2D>.Get("UI/Commands/Upgrade3Disabled", true);
                }
                yield return command_Upgrade;
            }



            if (!IsSacrificing())
            {
                Command_Action command_Action = new Command_Action();
                command_Action.action = new Action(this.TrySacrifice);
                command_Action.defaultLabel = "CommandCultSacrifice".Translate();
                command_Action.defaultDesc = "CommandCultSacrificeDesc".Translate();
                command_Action.disabled = (currentFunction < Function.Level2 || currentFunction == Function.Nightmare);
                command_Action.disabledReason = "CommandCultDisabled".Translate();
                command_Action.hotKey = KeyBindingDefOf.Misc1;
                command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/Sacrifice", true);
                if ((currentFunction < Function.Level2)) command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/SacrificeDisabled", true);
                yield return command_Action;
            }
            else
            {
                Command_Action command_Cancel = new Command_Action();
                command_Cancel.action = new Action(this.CancelSacrifice);
                command_Cancel.defaultLabel = "CommandCancelConstructionLabel".Translate();
                command_Cancel.defaultDesc = "CommandCancelSacrifice".Translate();
                command_Cancel.disabled = (currentFunction < Function.Level2);
                command_Cancel.hotKey = KeyBindingDefOf.DesignatorCancel;
                command_Cancel.icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true);
                yield return command_Cancel;
            }

            if (!IsWorshipping())
            {
                Command_Action command_Action = new Command_Action();
                command_Action.action = new Action(this.TryWorshipForced);
                command_Action.defaultLabel = "CommandForceWorship".Translate();
                command_Action.defaultDesc = "CommandForceWorshipDesc".Translate();
                command_Action.disabled = currentFunction == Function.Nightmare;
                command_Action.hotKey = KeyBindingDefOf.Misc1;
                command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/Worship", true);
                yield return command_Action;
            }
            else
            {
                Command_Action command_Cancel = new Command_Action();
                command_Cancel.action = new Action(this.CancelWorship);
                command_Cancel.defaultLabel = "CommandCancelConstructionLabel".Translate();
                command_Cancel.defaultDesc = "CommandCancelWorship".Translate();
                command_Cancel.hotKey = KeyBindingDefOf.DesignatorCancel;
                command_Cancel.icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true);
                yield return command_Cancel;
            }

            if (!IsOffering())
            {//
                Command_Action command_Action = new Command_Action();
                command_Action.action = new Action(this.TryOffering);
                command_Action.defaultLabel = "CommandOffering".Translate();
                command_Action.defaultDesc = "CommandOfferingDesc".Translate();
                command_Action.disabled = currentFunction == Function.Nightmare;
                command_Action.hotKey = KeyBindingDefOf.Misc1;
                command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/MakeOffering", true);
                yield return command_Action;
            }
            else
            {
                Command_Action command_Cancel = new Command_Action();
                command_Cancel.action = new Action(this.CancelOffering);
                command_Cancel.defaultLabel = "CommandCancelConstructionLabel".Translate();
                command_Cancel.defaultDesc = "CommandCancelOffering".Translate();
                command_Cancel.hotKey = KeyBindingDefOf.DesignatorCancel;
                command_Cancel.icon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true);
                yield return command_Cancel;
            }

            if (currentFunction == Function.Nightmare)
            {
                Command_Toggle toggleDef = new Command_Toggle();
                toggleDef.hotKey = KeyBindingDefOf.CommandTogglePower;
                toggleDef.icon = ContentFinder<Texture2D>.Get("UI/Icons/Commands/PruneAndRepair", true);
                toggleDef.defaultLabel = "PruneAndRepair".Translate();
                toggleDef.defaultDesc = "PruneAndRepairDesc".Translate();
                toggleDef.isActive = (() => this.toBePrunedAndRepaired);
                toggleDef.toggleAction = delegate
                {
                    PruneAndRepairToggle();
                };
                yield return toggleDef;
            }



            if (DebugSettings.godMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Debug: Discover All Deities",
                    action = delegate
                    {
                        foreach (CosmicEntity entity in DeityTracker.Get.DeityCache.Keys)
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
                        foreach (CosmicEntity entity in DeityTracker.Get.DeityCache.Keys)
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
                        foreach (Pawn p in Map.mapPawns.FreeColonistsSpawned)
                        {
                            CultUtility.AffectCultMindedness(p, 0.99f);
                        }
                    }
                };

                yield return new Command_Action
                {
                    defaultLabel = "Debug: Upgrade Max Level Altar",
                    action = delegate
                    {
                        currentFunction = Function.Level3;
                    }
                };

                yield return new Command_Action
                {
                    defaultLabel = "Debug: Unlock All Spells",
                    action = delegate
                    {
                        foreach (CosmicEntity entity in DeityTracker.Get.DeityCache.Keys)
                        {
                            entity.AffectFavor(9999999);
                        }
                    }
                };

                yield return new Command_Toggle
                {
                    defaultLabel = "Debug: Always Succeed",
                    isActive = (() => debugAlwaysSucceed),
                    toggleAction = delegate
                    {
                        debugAlwaysSucceed = !debugAlwaysSucceed;
                    }
                };

                yield return new Command_Action
                {
                    defaultLabel = "Debug: Force Side Effect",
                    action = delegate
                    {
                        List<FloatMenuOption> list = new List<FloatMenuOption>();
                        CultTableOfFun table = new CultTableOfFun();
                        foreach (FunSpell spell in table.TableOfFun)
                        {
                            IncidentDef currentDef = IncidentDef.Named(spell.defName);
                            list.Add(new FloatMenuOption(currentDef.LabelCap, delegate
                            {
                                IncidentDef temp = DefDatabase<IncidentDef>.GetNamed(spell.defName);
                                if (temp != null)
                                {
                                    CultUtility.CastSpell(temp, this.Map);
                                }
                            }));
                        }
                        Find.WindowStack.Add(new FloatMenu(list));
                    }
                };

                if (currentFunction != Function.Nightmare)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Debug: Force Nightmare Tree",
                        action = delegate
                        {
                            NightmareEvent();
                        }
                    };
                }
            }
            yield break;
        }
        public void PruneAndRepairToggle()
        {
            this.toBePrunedAndRepaired = !this.toBePrunedAndRepaired;
        }

        #endregion Gizmos

        #region LevelChangers

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
            string newdefName = "";
            switch (currentFunction)
            {
                case Function.Level1:
                    newdefName = "Cult_AnimalSacrificeAltar";
                    break;
                case Function.Level2:
                    newdefName = "Cult_HumanSacrificeAltar";
                    break;
                case Function.Level3:
                    Log.Error("Tried to upgrade fully functional altar. This should never happen.");
                    return;
            }
            if (newdefName == "") return;
            ReplaceAltarWith(newdefName);
        }

        public void NightmareEvent()
        {
            lastFunction = this.currentFunction;
            currentFunction = Function.Nightmare;
            ReplaceAltarWith("Cult_NightmareSacrificeAltar");
        }

        public void NightmarePruned(Pawn pruner)
        {
            string olddefName = "";
            switch (lastFunction)
            {
                case Function.Level1:
                    olddefName = "Cult_SacrificialAltar";
                    break;
                case Function.Level2:
                    olddefName = "Cult_AnimalSacrificeAltar";
                    break;
                case Function.Level3:
                    olddefName = "Cult_HumanSacrificeAltar";
                    break;
            }
            if (olddefName == "") return;
            Building_SacrificialAltar newAltar = ReplaceAltarWith(olddefName);
            Messages.Message("PruningSuccessful".Translate(new object[]
            {
                pruner.LabelShort
            }), MessageTypeDefOf.PositiveEvent);
            newAltar.Map.reservationManager.ReleaseAllForTarget(newAltar);
        }

        private Building_SacrificialAltar ReplaceAltarWith(string newdefName)
        {
            Building_SacrificialAltar result = null;
            if (newdefName == "")
            {
                Cthulhu.Utility.ErrorReport("ReplaceAltarWith :: Null exception.");
                return result;
            }
            //Copy the important values.
            IntVec3 currentLocation = this.Position;
            Rot4 currentRotation = this.Rotation;
            ThingDef currentStuff = this.Stuff;
            CompQuality compQuality = this.TryGetComp<CompQuality>();
            Function currentLastFunction = this.lastFunction;
            Map currentMap = this.Map;
            QualityCategory qualityCat = QualityCategory.Normal;
            if (compQuality != null)
            {
                qualityCat = compQuality.Quality;
            }

            //Worship values
            string s1 = RoomName;
            Pawn p1 = tempPreacher;
            CosmicEntity c1 = tempCurrentWorshipDeity;
            bool b1 = OptionMorning;
            bool b2 = OptionEvening;

            this.Destroy(0);
            //Spawn the new altar over the other
            Building_SacrificialAltar thing = (Building_SacrificialAltar)ThingMaker.MakeThing(ThingDef.Named(newdefName), currentStuff);
            result = thing;
            thing.SetFaction(Faction.OfPlayer);
            thing.Rotation = currentRotation;
            GenPlace.TryPlaceThing(thing, currentLocation, currentMap, ThingPlaceMode.Direct);
            thing.Rotation = currentRotation;
            thing.TryGetComp<CompQuality>().SetQuality(qualityCat, ArtGenerationContext.Colony);
            thing.lastFunction = currentLastFunction;
            if (currentFunction != Function.Nightmare) Messages.Message("UpgradeSuccessful".Translate(), new TargetInfo(currentLocation, Map), MessageTypeDefOf.PositiveEvent);
            else Messages.Message("CorruptedAltarWarning".Translate(), MessageTypeDefOf.NegativeEvent);

            //Pass worship values
            thing.RoomName = s1;
            thing.tempPreacher = p1;
            thing.tempCurrentWorshipDeity = c1;
            thing.OptionMorning = b1;
            thing.OptionEvening = b2;

            return result;
        }

        #endregion LevelChangers

        #region Offering
        private void CancelOffering()
        {
            Pawn pawn = null;
            List<Pawn> listeners = Map.mapPawns.AllPawnsSpawned.FindAll(x => x.RaceProps.intelligence == Intelligence.Humanlike);
            bool[] flag = new bool[listeners.Count];
            for (int i = 0; i < listeners.Count; i++)
            {
                pawn = listeners[i];
                if (pawn.Faction == Faction.OfPlayer)
                {
                    //Hold Offering
                    if (pawn.CurJob.def == CultsDefOf.Cults_GiveOffering)
                    {
                        pawn.jobs.StopAll();
                    }
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

            if (CanGatherOfferingNow())
            {
                if (!TryDetermineOffering(tempOfferingType, tempOfferingSize, tempOfferer, this, out tempDeterminedOfferings, out billRecipe))
                {
                    Cthulhu.Utility.DebugReport("Failed to determine offering");
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
                        Messages.Message("An offering is already happening.", TargetInfo.Invalid, MessageTypeDefOf.RejectInput);
                        return;
                }
            }
        }
        private bool CanGatherOfferingNow()
        {

            if (this.tempOfferingType == CultUtility.SacrificeType.none) return RejectMessage("No offering type selected");
            if (this.tempOfferingSize == CultUtility.OfferingSize.none) return RejectMessage("No offering amount selected");
            if (this.tempOfferer == null) return RejectMessage("No offerer selected");
            if (this.tempOfferer.Drafted) return RejectMessage("Offerer is drafted.");
            if (this.tempOfferer.Dead || this.tempOfferer.Downed) return RejectMessage("Select an able-bodied offerer.", tempOfferer);
            if (this.tempCurrentOfferingDeity == null) return RejectMessage("No cosmic entity selected. Entities can be discovered at the forbidden knowledge center.");
            if (!this.tempOfferer.CanReserve(this)) return RejectMessage("The altar is reserved by something else.");
            foreach (var thing in Position.GetThingList(Map))
            {
                if (thing is Corpse) return RejectMessage("The altar needs to be cleared first.");
            }
            return true;

        }
        public void StartOffering()
        {
            determinedOfferings = tempDeterminedOfferings;
            offerer = tempOfferer;
            currentOfferingDeity = tempCurrentOfferingDeity;

            if (this.Destroyed || !this.Spawned)
            {
                CultUtility.AbortCongregation(null, "Altar".Translate() + "IsUnavailable".Translate());
                return;
            }
            if (!Cthulhu.Utility.IsActorAvailable(this.offerer))
            {
                CultUtility.AbortCongregation(this, "Offerer".Translate() + " " + this.offerer.LabelShort + "IsUnavailable".Translate());
                this.offerer = null;
                return;
            }

            Messages.Message("An offering is being gathered.", TargetInfo.Invalid, MessageTypeDefOf.NeutralEvent);
            ChangeState(State.offering, OfferingState.started);
            Cthulhu.Utility.DebugReport("Make offering called.");

            Job job2 = new Job(CultsDefOf.Cults_GiveOffering);
            job2.playerForced = true;
            job2.targetA = this;
            job2.targetQueueB = new List<LocalTargetInfo>(this.determinedOfferings.Count);
            job2.targetC = this.Position;
            job2.countQueue = new List<int>(this.determinedOfferings.Count);
            for (int i = 0; i < this.determinedOfferings.Count; i++)
            {
                job2.targetQueueB.Add(this.determinedOfferings[i].thing);
                job2.countQueue.Add(this.determinedOfferings[i].count);
            }
            job2.haulMode = HaulMode.ToCellNonStorage;
            job2.locomotionUrgency = LocomotionUrgency.Sprint;
            job2.bill = new Bill_Production(billRecipe);
            //return job2;
            offerer.jobs.TryTakeOrderedJob(job2);
            //offerer.jobs.EndCurrentJob(JobCondition.InterruptForced);
            //GetSacrificeGroup(this);
            //Cthulhu.Utility.DebugReport("Sacrifice state set to gathering");
        }

        #endregion Offering

        #region Sacrifice


        private void CancelSacrifice()
        {
            Pawn pawn = null;
            List<Pawn> listeners = Map.mapPawns.AllPawnsSpawned.FindAll(x => x.RaceProps.intelligence == Intelligence.Humanlike);
            bool[] flag = new bool[listeners.Count];
            for (int i = 0; i < listeners.Count; i++)
            {
                pawn = listeners[i];
                if (pawn.Faction == Faction.OfPlayer)
                {
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
            //this.currentState = State.off;
            Messages.Message("Cancelling sacrifice.", MessageTypeDefOf.NegativeEvent);
        }
        private void TrySacrifice()
        {
            if (IsSacrificing())
            {
                Messages.Message("A sacrifice is already gathering.", MessageTypeDefOf.RejectInput);
                return;
            }

            if (CanGatherSacrificeNow())
            {
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
                        Messages.Message("A sacrifice is already gathering.", TargetInfo.Invalid, MessageTypeDefOf.RejectInput);
                        return;
                }
            }
        }
        private bool CanGatherSacrificeNow()
        {

            if (this.tempSacrifice == null) return RejectMessage("No prisoner to sacrifice selected.");
            if (this.tempExecutioner == null) return RejectMessage("No executioner selected");
            if (this.tempExecutioner.Drafted) return RejectMessage("The executioner is drafted.");
            if (this.tempCurrentSacrificeDeity == null) return RejectMessage("No cosmic entity selected");
            if (Map.GetComponent<MapComponent_SacrificeTracker>().lastSacrificeType != CultUtility.SacrificeType.animal && this.tempCurrentSpell == null) return RejectMessage("No spell selected. Tip: Earn favor to unlock spells.");
            if (this.tempSacrifice.Dead) return RejectMessage("The sacrifice is already dead", tempSacrifice);
            if (this.tempExecutioner.Dead || this.tempExecutioner.Downed) return RejectMessage("Select an able-bodied executioner");
            if (!this.tempExecutioner.CanReserve(this.tempSacrifice)) return RejectMessage("The executioner can't reserve the sacrifice.");
            if (!this.tempExecutioner.CanReserve(this)) return RejectMessage("The altar is reserved by something else");
            foreach (var thing in Position.GetThingList(Map))
            {
                if (thing is Corpse) return RejectMessage("The altar needs to be cleared first.");
            }
            if (Map.GetComponent<MapComponent_SacrificeTracker>().lastSacrificeType == CultUtility.SacrificeType.human)
            {
                SpellWorker worker = this.tempCurrentSpell.Worker as SpellWorker;
                if (!worker.CanSummonNow(Map))
                {
                    return false;
                }
            }
            return true;
        }
        public void StartSacrifice()
        {
            sacrifice = tempSacrifice;
            executioner = tempExecutioner;
            currentSacrificeDeity = tempCurrentSacrificeDeity;
            currentSpell = tempCurrentSpell;

            if (this.Destroyed || !this.Spawned)
            {
                CultUtility.AbortCongregation(null, "The altar is unavailable.");
                return;
            }
            if (!Cthulhu.Utility.IsActorAvailable(this.executioner))
            {
                CultUtility.AbortCongregation(this, "The executioner, " + this.executioner.LabelShort + " is unavaialable.");
                this.executioner = null;
                this.tempExecutioner = null;
                return;
            }
            if (!Cthulhu.Utility.IsActorAvailable(this.sacrifice, true))
            {
                CultUtility.AbortCongregation(this, "The sacrifice, " + this.sacrifice.LabelShort + " is unavaialable.");
                this.sacrifice = null;
                this.tempSacrifice = null;
                return;
            }

            FactionBase factionBase = (FactionBase)this.Map.info.parent;

            Messages.Message("SacrificeGathering".Translate(new object[] {
                factionBase.Label
        }), TargetInfo.Invalid, MessageTypeDefOf.NeutralEvent);

            ChangeState(State.sacrificing, SacrificeState.started);
            //this.currentState = State.started;
            Map.GetComponent<MapComponent_SacrificeTracker>().lastResult = CultUtility.SacrificeResult.none;

            Map.GetComponent<MapComponent_SacrificeTracker>().lastSacrificeCongregation = new List<Pawn>();

            Cthulhu.Utility.DebugReport("Force Sacrifice called");
            Job job = new Job(CultsDefOf.Cults_HoldSacrifice, sacrifice, this);
            job.count = 1;
            executioner.jobs.TryTakeOrderedJob(job);
            //executioner.jobs.EndCurrentJob(JobCondition.InterruptForced);
            Map.GetComponent<MapComponent_SacrificeTracker>().lastSacrificeCongregation.Add(executioner);
            GetSacrificeGroup();

            Cthulhu.Utility.DebugReport("Sacrifice state set to gathering");
        }

        HashSet<Pawn> availableWorshippers;
        public HashSet<Pawn> AvailableWorshippers
        {
            get
            {
                if (availableWorshippers == null || availableWorshippers.Count == 0)
                {
                    availableWorshippers = new HashSet<Pawn>(this.Map.mapPawns.AllPawnsSpawned.FindAll(y => y is Pawn x &&
                                                                                  x.RaceProps.Humanlike &&
                                                                                  !x.IsPrisoner &&
                                                                                  x.Faction == this.Faction &&
                                                                                  x.RaceProps.intelligence == Intelligence.Humanlike &&
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

        public static void GetSacrificeGroup(Building_SacrificialAltar altar)
        {
            altar.GetSacrificeGroup();
        }

        public void GetSacrificeGroup()
        {
            Room room = this.GetRoom();

            if (room.Role != RoomRoleDefOf.PrisonBarracks && room.Role != RoomRoleDefOf.PrisonCell)
            {
                if (AvailableWorshippers != null && AvailableWorshippers.Count > 0)
                    foreach (Pawn p in AvailableWorshippers)
                    {
                        CultUtility.GiveAttendSacrificeJob(this, p);
                        this.Map.GetComponent<MapComponent_SacrificeTracker>().lastSacrificeCongregation.Add(p);
                    }
            }
        }

        public static bool ShouldAttendSacrifice(Pawn p, Pawn executioner)
        {
            int num = 100; //Forced for testing purposes

            if (p.CurJob.def == CultsDefOf.Cults_AttendSacrifice)
            {
                num = 0;
            }

            if ((Rand.RangeInclusive(0, 15) + num) >= 20)
            {
                return true;
            }

            return false;
        }

        #endregion Sacrifice

        #region Misc

        // RimWorld.WorkGiver_DoBill
        private bool TryFindBestOfferingIngredients(RecipeDef recipe, Pawn pawn, Building_SacrificialAltar billGiver, List<ThingAmount> chosen)
        {
            chosen.Clear();
            List<Thing> relevantThings = new List<Thing>();
            List<IngredientCount> ingredientsOrdered = new List<IngredientCount>();
            List<Thing> newRelevantThings = new List<Thing>();
            if (recipe.ingredients.Count == 0)
            {
                //resultThings = relevantThings;
                return true;
            }
            IntVec3 billGiverRootCell = billGiver.InteractionCell;
            Region validRegionAt = Map.regionGrid.GetValidRegionAt(billGiverRootCell);
            if (validRegionAt == null)
            {
                Cthulhu.Utility.DebugReport("Invalid Region");

                //resultThings = relevantThings;
                return false;
            }
            ingredientsOrdered.Clear();
            if (recipe.productHasIngredientStuff)
            {
                Cthulhu.Utility.DebugReport(recipe.ingredients[0].ToString());
                ingredientsOrdered.Add(recipe.ingredients[0]);
            }
            for (int i = 0; i < recipe.ingredients.Count; i++)
            {
                if (!recipe.productHasIngredientStuff || i != 0)
                {
                    IngredientCount ingredientCount = recipe.ingredients[i];
                    if (ingredientCount.filter.AllowedDefCount == 1)
                    {
                        Cthulhu.Utility.DebugReport(ingredientCount.ToString());
                        ingredientsOrdered.Add(ingredientCount);
                    }
                }
            }
            for (int j = 0; j < recipe.ingredients.Count; j++)
            {
                IngredientCount item = recipe.ingredients[j];
                if (!ingredientsOrdered.Contains(item))
                {
                    Cthulhu.Utility.DebugReport(item.ToString());
                    ingredientsOrdered.Add(item);
                }
            }
            relevantThings.Clear();
            bool foundAll = false;
            Predicate<Thing> baseValidator = (Thing t) => t.Spawned && !t.IsForbidden(pawn) &&
            (t.Position - billGiver.Position).LengthHorizontalSquared < 999 * 999 && recipe.fixedIngredientFilter.Allows(t) &&
            recipe.defaultIngredientFilter.Allows(t) && recipe.ingredients.Any((IngredientCount ingNeed) => ingNeed.filter.Allows(t)) &&
            pawn.CanReserve(t, 1);

            RegionProcessor regionProcessor = delegate (Region r)
            {
                newRelevantThings.Clear();
                List<Thing> list = r.ListerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.HaulableEver));
                for (int i = 0; i < list.Count; i++)
                {
                    Thing thing = list[i];
                    if (baseValidator(thing))
                    {
                        Cthulhu.Utility.DebugReport(thing.ToString());
                        newRelevantThings.Add(thing);
                    }
                }
                if (newRelevantThings.Count > 0)
                {
                    Comparison<Thing> comparison = delegate (Thing t1, Thing t2)
                    {
                        float lengthHorizontalSquared = (t1.Position - pawn.Position).LengthHorizontalSquared;
                        float lengthHorizontalSquared2 = (t2.Position - pawn.Position).LengthHorizontalSquared;
                        return lengthHorizontalSquared.CompareTo(lengthHorizontalSquared2);
                    };
                    newRelevantThings.Sort(comparison);
                    relevantThings.AddRange(newRelevantThings);
                    newRelevantThings.Clear();
                    bool flag = true;
                    for (int i = 0; i < recipe.ingredients.Count; i++)
                    {
                        IngredientCount ingredientCount = recipe.ingredients[i];
                        float num = ingredientCount.GetBaseCount();
                        for (int j = 0; j < relevantThings.Count; j++) //was AvailableThings
                        {
                            Thing thing = relevantThings[j]; //was AvailableThings
                            if (ingredientCount.filter.Allows(thing))
                            {
                                Cthulhu.Utility.DebugReport(thing.ToString());
                                float num2 = recipe.IngredientValueGetter.ValuePerUnitOf(thing.def);
                                int num3 = Mathf.Min(Mathf.CeilToInt(num / num2), thing.stackCount);
                                ThingAmount.AddToList(chosen, thing, num3);
                                num -= (float)num3 * num2;
                                if (num <= 0.0001f)
                                {
                                    break;
                                }
                            }
                        }
                        if (num > 0.0001f)
                        {
                            flag = false;
                        }
                    }
                    if (flag)
                    {
                        foundAll = true;
                        return true;
                    }
                }
                return false;
            };
            TraverseParms traverseParams = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false);
            RegionEntryPredicate entryCondition = (Region from, Region r) => r.Allows(traverseParams, false);
            RegionTraverser.BreadthFirstTraverse(validRegionAt, entryCondition, regionProcessor, 99999);
            //resultThings = relevantThings;
            return foundAll;
        }
        public bool TryDetermineOffering(CultUtility.SacrificeType type, CultUtility.OfferingSize size, Pawn pawn, Building_SacrificialAltar altar, out List<ThingAmount> result, out RecipeDef resultRecipe)
        {
            result = null;
            resultRecipe = null;
            List<ThingAmount> list = new List<ThingAmount>();
            RecipeDef recipe = null;

            string recipedefName = "OfferingOf";
            switch (type)
            {
                case CultUtility.SacrificeType.plants:
                    recipedefName = recipedefName + "Plants";
                    Map.GetComponent<MapComponent_SacrificeTracker>().lastSacrificeType = CultUtility.SacrificeType.plants;
                    break;
                case CultUtility.SacrificeType.meat:
                    recipedefName = recipedefName + "Meat";

                    Map.GetComponent<MapComponent_SacrificeTracker>().lastSacrificeType = CultUtility.SacrificeType.meat;
                    break;
                case CultUtility.SacrificeType.meals:
                    recipedefName = recipedefName + "Meals";

                    Map.GetComponent<MapComponent_SacrificeTracker>().lastSacrificeType = CultUtility.SacrificeType.meals;
                    break;
            }
            switch (size)
            {
                case CultUtility.OfferingSize.meagre:
                    recipedefName = recipedefName + "_Meagre";
                    break;
                case CultUtility.OfferingSize.decent:
                    recipedefName = recipedefName + "_Decent";
                    break;
                case CultUtility.OfferingSize.sizable:
                    recipedefName = recipedefName + "_Sizable";
                    break;
                case CultUtility.OfferingSize.worthy:
                    recipedefName = recipedefName + "_Worthy";
                    break;
                case CultUtility.OfferingSize.impressive:
                    recipedefName = recipedefName + "_Impressive";
                    break;

            }
            recipe = DefDatabase<RecipeDef>.GetNamed(recipedefName);
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

        #endregion Misc

        public bool CurrentlyUsableForBills()
        {
            return true;
        }

    }


}