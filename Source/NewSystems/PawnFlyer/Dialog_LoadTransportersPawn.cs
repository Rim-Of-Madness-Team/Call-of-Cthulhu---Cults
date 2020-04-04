﻿using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;
using RimWorld;

namespace CultOfCthulhu
{
    public class Dialog_LoadTransportersPawn : Window
    {
        private enum Tab
        {
            Pawns,
            Items
        }

        private const float TitleRectHeight = 40f;

        private const float BottomAreaHeight = 55f;

        private Map map;

        private List<CompTransporterPawn> transporters;

        private List<TransferableOneWay> transferables;

        private TransferableOneWayWidget pawnsTransfer;

        private TransferableOneWayWidget itemsTransfer;

        private Dialog_LoadTransportersPawn.Tab tab;

        private float lastMassFlashTime = -9999f;

        private bool massUsageDirty = true;

        private float cachedMassUsage;

        private bool daysWorthOfFoodDirty = true;

        private Pair<float, float> cachedDaysWorthOfFood;

        private readonly Vector2 BottomButtonSize = new Vector2(160f, 40f);

        private static List<TabRecord> tabsList = new List<TabRecord>();

        public override Vector2 InitialSize
        {
            get { return new Vector2(1024f, (float) UI.screenHeight); }
        }

        protected override float Margin
        {
            get { return 0f; }
        }

        private BiomeDef Biome
        {
            get { return this.map.Biome; }
        }


        private Pair<ThingDef, float> ForagedFoodPerDay
        {
            get
            {
                if (this.foragedFoodPerDayDirty)
                {
                    this.foragedFoodPerDayDirty = false;
                    StringBuilder stringBuilder = new StringBuilder();
                    this.cachedForagedFoodPerDay = ForagedFoodPerDayCalculator.ForagedFoodPerDay(this.transferables,
                        this.Biome, Faction.OfPlayer, stringBuilder);
                    this.cachedForagedFoodPerDayExplanation = stringBuilder.ToString();
                }
                return this.cachedForagedFoodPerDay;
            }
        }

        private float Visibility
        {
            get
            {
                if (this.visibilityDirty)
                {
                    this.visibilityDirty = false;
                    StringBuilder stringBuilder = new StringBuilder();
                    this.cachedVisibility = CaravanVisibilityCalculator.Visibility(this.transferables, stringBuilder);
                    this.cachedVisibilityExplanation = stringBuilder.ToString();
                }
                return this.cachedVisibility;
            }
        }


        private int PawnCapacity
        {
            get
            {
                int num = 0;
                for (int i = 0; i < this.transporters.Count; i++)
                {
                    int result = 1; //In-case PawnFlyer doesn't work out
                    PawnFlyer pawnFlyer = this.transporters[i].parent as PawnFlyer;
                    if (pawnFlyer != null)
                    {
                        PawnFlyerDef pawnFlyerDef = pawnFlyer.def as PawnFlyerDef;
                        if (pawnFlyerDef != null)
                        {
                            result = pawnFlyerDef.flightPawnLimit;
                        }
                    }
                    num += result;
                }
                return num;
            }
        }

        /// <summary>
        /// Modified to use PawnFlyerDef
        /// </summary>
        private float MassCapacity
        {
            get
            {
                float num = 0f;
                for (int i = 0; i < this.transporters.Count; i++)
                {
                    float result = 150f; //In-case PawnFlyer doesn't work out
                    PawnFlyer pawnFlyer = this.transporters[i].parent as PawnFlyer;
                    if (pawnFlyer != null)
                    {
                        result = pawnFlyer.GetStatValue(StatDefOf.CarryingCapacity);
                        //PawnFlyerDef pawnFlyerDef = pawnFlyer.def as PawnFlyerDef;
                        //if (pawnFlyerDef != null)
                        //{
                        //    result = pawnFlyerDef.flightCarryCapacity;
                        //}
                    }
                    num += result;
                }
                return num;
            }
        }

        private string TransportersLabel
        {
            get { return Find.ActiveLanguageWorker.Pluralize(this.transporters[0].parent.Label); }
        }

        private string TransportersLabelCap
        {
            get { return this.TransportersLabel.CapitalizeFirst(); }
        }

        private float MassUsage
        {
            get
            {
                if (this.massUsageDirty)
                {
                    this.massUsageDirty = false;
                    this.cachedMassUsage = CollectionsMassCalculator.MassUsageTransferables(this.transferables,
                        IgnorePawnsInventoryMode.DontIgnore, true, false);
                }
                return this.cachedMassUsage;
            }
        }

        //private float DaysWorthOfFood
        //{
        //    get
        //    {
        //        if (this.daysWorthOfFoodDirty)
        //        {
        //            this.daysWorthOfFoodDirty = false;
        //            this.cachedDaysWorthOfFood = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(this.transferables);
        //        }
        //        return this.cachedDaysWorthOfFood;
        //    }
        //}

        private bool tilesPerDayDirty = true;
        private float cachedTilesPerDay;

        private string cachedTilesPerDayExplanation;

        private float TilesPerDay
        {
            get
            {
                if (this.tilesPerDayDirty)
                {
                    this.tilesPerDayDirty = false;
                    StringBuilder stringBuilder = new StringBuilder();
                    this.cachedTilesPerDay = TilesPerDayCalculator.ApproxTilesPerDay(this.transferables, this.MassUsage,
                        this.MassCapacity, this.map.Tile, -1, stringBuilder);
                    this.cachedTilesPerDayExplanation = stringBuilder.ToString();
                }
                return this.cachedTilesPerDay;
            }
        }

        public Dialog_LoadTransportersPawn(Map map, List<CompTransporterPawn> transporters)
        {
            this.map = map;
            this.transporters = new List<CompTransporterPawn>();
            this.transporters.AddRange(transporters);
            //this.closeOnEscapeKey = true;
            this.closeOnAccept = false;
            this.closeOnCancel = false;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
        }

        public override void PostOpen()
        {
            base.PostOpen();
            this.CalculateAndRecacheTransferables();
        }

        private bool foragedFoodPerDayDirty = true;

        private Pair<ThingDef, float> cachedForagedFoodPerDay;

        private string cachedForagedFoodPerDayExplanation;

        private bool visibilityDirty = true;

        private float cachedVisibility;

        private string cachedVisibilityExplanation;


        private Pair<float, float> DaysWorthOfFood
        {
            get
            {
                if (this.daysWorthOfFoodDirty)
                {
                    this.daysWorthOfFoodDirty = false;
                    float first = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(this.transferables, this.map.Tile,
                        IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload, Faction.OfPlayer, null, 0f, 3500);
                    this.cachedDaysWorthOfFood = new Pair<float, float>(first,
                        DaysUntilRotCalculator.ApproxDaysUntilRot(this.transferables, this.map.Tile,
                            IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload, null, 0f, 3500));
                }
                return this.cachedDaysWorthOfFood;
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect rect = new Rect(0f, 0f, inRect.width, 35f);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, "LoadTransporters".Translate(
                this.TransportersLabel
            ));
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            CaravanUIUtility.DrawCaravanInfo(
                new CaravanUIUtility.CaravanInfo(this.MassUsage, this.MassCapacity, "", this.TilesPerDay,
                    this.cachedTilesPerDayExplanation, this.DaysWorthOfFood, this.ForagedFoodPerDay,
                    this.cachedForagedFoodPerDayExplanation, this.Visibility, this.cachedVisibilityExplanation), null,
                this.map.Tile, null, this.lastMassFlashTime, new Rect(12f, 35f, inRect.width - 24f, 40f), false, null,
                false);
            Dialog_LoadTransportersPawn.tabsList.Clear();
            Dialog_LoadTransportersPawn.tabsList.Add(new TabRecord("PawnsTab".Translate(),
                delegate { this.tab = Dialog_LoadTransportersPawn.Tab.Pawns; },
                this.tab == Dialog_LoadTransportersPawn.Tab.Pawns));
            Dialog_LoadTransportersPawn.tabsList.Add(new TabRecord("ItemsTab".Translate(),
                delegate { this.tab = Dialog_LoadTransportersPawn.Tab.Items; },
                this.tab == Dialog_LoadTransportersPawn.Tab.Items));
            inRect.yMin += 119f;
            Widgets.DrawMenuSection(inRect);
            TabDrawer.DrawTabs(inRect, Dialog_LoadTransportersPawn.tabsList, 200f);
            inRect = inRect.ContractedBy(17f);
            GUI.BeginGroup(inRect);
            Rect rect2 = inRect.AtZero();
            this.DoBottomButtons(rect2);
            Rect inRect2 = rect2;
            inRect2.yMax -= 59f;
            bool flag = false;
            Dialog_LoadTransportersPawn.Tab tab = this.tab;
            if (tab != Dialog_LoadTransportersPawn.Tab.Pawns)
            {
                if (tab == Dialog_LoadTransportersPawn.Tab.Items)
                {
                    this.itemsTransfer.OnGUI(inRect2, out flag);
                }
            }
            else
            {
                this.pawnsTransfer.OnGUI(inRect2, out flag);
            }
            if (flag)
            {
                this.CountToTransferChanged();
            }
            GUI.EndGroup();
//            Rect rect = new Rect(0f, 0f, inRect.width, 40f);
//            Text.Font = GameFont.Medium;
//            Text.Anchor = TextAnchor.MiddleCenter;
//            Widgets.Label(rect, "LoadTransporters".Translate(new object[]
//            {
//                this.TransportersLabel
//            }));
//            Text.Font = GameFont.Small;
//            Text.Anchor = TextAnchor.UpperLeft;
//            Dialog_LoadTransportersPawn.tabsList.Clear();
//            Dialog_LoadTransportersPawn.tabsList.Add(new TabRecord("PawnsTab".Translate(), delegate
//            {
//                this.tab = Dialog_LoadTransportersPawn.Tab.Pawns;
//            }, this.tab == Dialog_LoadTransportersPawn.Tab.Pawns));
//            //Dialog_LoadTransportersPawn.tabsList.Add(new TabRecord("ItemsTab".Translate(), delegate
//            //{
//            //    this.tab = Dialog_LoadTransportersPawn.Tab.Items;
//            //}, this.tab == Dialog_LoadTransportersPawn.Tab.Items));
//            inRect.yMin += 72f;
//            Widgets.DrawMenuSection(inRect);
//            TabDrawer.DrawTabs(inRect, Dialog_LoadTransportersPawn.tabsList);
//            inRect = inRect.ContractedBy(17f);
//            GUI.BeginGroup(inRect);
//            Rect rect2 = inRect.AtZero();
//            Rect rect3 = rect2;
//            rect3.xMin += rect2.width - this.pawnsTransfer.TotalNumbersColumnsWidths;
//            rect3.y += 32f;
//            TransferableUIUtility.DrawMassInfo(rect3, this.MassUsage, this.MassCapacity, "TransportersMassUsageTooltip".Translate(), this.lastMassFlashTime, true);
//            //CaravanUIUtility.DrawDaysWorthOfFoodInfo(new Rect(rect3.x, rect3.y + 22f, rect3.width, rect3.height), this.DaysWorthOfFood, true);
//            this.DoBottomButtons(rect2);
//            Rect inRect2 = rect2;
//            inRect2.yMax -= 59f;
//            bool flag = false;
//            Dialog_LoadTransportersPawn.Tab tab = this.tab;
//            if (tab != Dialog_LoadTransportersPawn.Tab.Pawns)
//            {
//                if (tab == Dialog_LoadTransportersPawn.Tab.Items)
//                {
//                    this.itemsTransfer.OnGUI(inRect2, out flag);
//                }
//            }
//            else
//            {
//                this.pawnsTransfer.OnGUI(inRect2, out flag);
//            }
//            if (flag)
//            {
//                this.CountToTransferChanged();
//            }
//            GUI.EndGroup();
        }

        public override bool CausesMessageBackground()
        {
            return true;
        }

        private void AddToTransferables(Thing t)
        {
            TransferableOneWay transferableOneWay =
                TransferableUtility.TransferableMatching<TransferableOneWay>(t, this.transferables,
                    TransferAsOneMode.PodsOrCaravanPacking);
            if (transferableOneWay == null)
            {
                transferableOneWay = new TransferableOneWay();
                this.transferables.Add(transferableOneWay);
            }
            transferableOneWay.things.Add(t);
        }

        private void DoBottomButtons(Rect rect)
        {
            Rect rect2 = new Rect(rect.width / 2f - this.BottomButtonSize.x / 2f, rect.height - 55f,
                this.BottomButtonSize.x, this.BottomButtonSize.y);
            if (Widgets.ButtonText(rect2, "AcceptButton".Translate(), true, false, true) && this.TryAccept())
            {
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                this.Close(false);
            }
            Rect rect3 = new Rect(rect2.x - 10f - this.BottomButtonSize.x, rect2.y, this.BottomButtonSize.x,
                this.BottomButtonSize.y);
            if (Widgets.ButtonText(rect3, "ResetButton".Translate(), true, false, true))
            {
                SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                this.CalculateAndRecacheTransferables();
            }
            Rect rect4 = new Rect(rect2.xMax + 10f, rect2.y, this.BottomButtonSize.x, this.BottomButtonSize.y);
            if (Widgets.ButtonText(rect4, "CancelButton".Translate(), true, false, true))
            {
                this.Close(true);
            }
            if (Prefs.DevMode)
            {
                float num = 200f;
                float num2 = this.BottomButtonSize.y / 2f;
                Rect rect5 = new Rect(rect.width - num, rect.height - 55f, num, num2);
                if (Widgets.ButtonText(rect5, "Dev: Load instantly", true, false, true) && this.DebugTryLoadInstantly())
                {
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                    this.Close(false);
                }
                Rect rect6 = new Rect(rect.width - num, rect.height - 55f + num2, num, num2);
                if (Widgets.ButtonText(rect6, "Dev: Select everything", true, false, true))
                {
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                    this.SetToLoadEverything();
                }
            }
        }

        private void CalculateAndRecacheTransferables()
        {
            this.transferables = new List<TransferableOneWay>();
            this.AddPawnsToTransferables();
            this.AddItemsToTransferables();
            this.pawnsTransfer = new TransferableOneWayWidget(null, Faction.OfPlayer.Name, this.TransportersLabelCap,
                "FormCaravanColonyThingCountTip".Translate(), true, IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload,
                true, () => this.MassCapacity - this.MassUsage, 24f, false, this.map.Tile, true);
            CaravanUIUtility.AddPawnsSections(this.pawnsTransfer, this.transferables);
            this.itemsTransfer = new TransferableOneWayWidget(from x in this.transferables
                where x.ThingDef.category != ThingCategory.Pawn
                select x, Faction.OfPlayer.Name, this.TransportersLabelCap,
                "FormCaravanColonyThingCountTip".Translate(), true, IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload,
                true, () => this.MassCapacity - this.MassUsage, 24f, false, this.map.Tile, true);
            this.CountToTransferChanged();
        }

        private bool DebugTryLoadInstantly()
        {
            this.CreateAndAssignNewTransportersGroup();
            int i;
            for (i = 0; i < this.transferables.Count; i++)
            {
                TransferableUtility.Transfer(this.transferables[i].things, this.transferables[i].CountToTransfer,
                    delegate(Thing splitPiece, IThingHolder originalThing)
                    {
                        this.transporters[i % this.transporters.Count].GetDirectlyHeldThings().TryAdd(splitPiece, true);
                    });
            }
            return true;
        }

        private bool TryAccept()
        {
            List<Pawn> pawnsFromTransferables = TransferableUtility.GetPawnsFromTransferables(this.transferables);
            if (!this.CheckForErrors(pawnsFromTransferables))
            {
                Cthulhu.Utility.DebugReport("TryAccept Failed");
                return false;
            }
            Cthulhu.Utility.DebugReport("TryAccept Succeeded");
            int transportersGroup = this.CreateAndAssignNewTransportersGroup();
            this.AssignTransferablesToRandomTransporters();
            IEnumerable<Pawn> enumerable = from x in pawnsFromTransferables
                where x.IsColonist && !x.Downed
                select x;
            if (enumerable.Any<Pawn>())
            {
                Cthulhu.Utility.DebugReport("Pawn List Succeeded");
                LordMaker.MakeNewLord(Faction.OfPlayer, new LordJob_LoadAndEnterTransportersPawn(transportersGroup),
                    this.map, enumerable);
                foreach (Pawn current in enumerable)
                {
                    if (current.Spawned)
                    {
                        current.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
                    }
                }
            }
            Messages.Message("MessageTransportersLoadingProcessStarted".Translate(), this.transporters[0].parent,
                MessageTypeDefOf.PositiveEvent);
            return true;
        }

        private void AssignTransferablesToRandomTransporters()
        {
            Cthulhu.Utility.DebugReport("AssignTransferablesToRandomTransporters Called");
            TransferableOneWay transferableOneWay =
                this.transferables.MaxBy((TransferableOneWay x) => x.CountToTransfer);
            int num = 0;
            for (int i = 0; i < this.transferables.Count; i++)
            {
                if (this.transferables[i] != transferableOneWay)
                {
                    if (this.transferables[i].CountToTransfer > 0)
                    {
                        this.transporters[num % this.transporters.Count].AddToTheToLoadList(this.transferables[i],
                            this.transferables[i].CountToTransfer);
                        num++;
                    }
                }
            }
            if (num < this.transporters.Count)
            {
                int num2 = transferableOneWay.CountToTransfer;
                int num3 = num2 / (this.transporters.Count - num);
                for (int j = num; j < this.transporters.Count; j++)
                {
                    int num4 = (j != this.transporters.Count - 1) ? num3 : num2;
                    if (num4 > 0)
                    {
                        this.transporters[j].AddToTheToLoadList(transferableOneWay, num4);
                    }
                    num2 -= num4;
                }
            }
            else
            {
                this.transporters[num % this.transporters.Count]
                    .AddToTheToLoadList(transferableOneWay, transferableOneWay.CountToTransfer);
            }
        }

        private int CreateAndAssignNewTransportersGroup()
        {
            Cthulhu.Utility.DebugReport("CreateAndAssignNewTransportersGroup Called");
            int nextTransporterGroupID = Find.UniqueIDsManager.GetNextTransporterGroupID();
            for (int i = 0; i < this.transporters.Count; i++)
            {
                this.transporters[i].groupID = nextTransporterGroupID;
            }
            return nextTransporterGroupID;
        }

        private bool CheckForErrors(List<Pawn> pawns)
        {
            if (!this.transferables.Any((TransferableOneWay x) => x.CountToTransfer != 0))
            {
                Messages.Message("CantSendEmptyTransportPods".Translate(), MessageTypeDefOf.RejectInput);
                return false;
            }
            if (this.MassUsage > this.MassCapacity)
            {
                this.FlashMass();
                Messages.Message("TooBigTransportersMassUsage".Translate(), MessageTypeDefOf.RejectInput);
                return false;
            }
            if (pawns.Count > PawnCapacity)
            {
                Messages.Message("OverPawnRiderLimit".Translate(
                    PawnCapacity.ToString()
                ), MessageTypeDefOf.RejectInput);
                return false;
            }

            Pawn pawn = pawns.Find((Pawn x) => !x.MapHeld.reachability.CanReach(x.PositionHeld,
                this.transporters[0].parent, PathEndMode.Touch,
                TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false)));
            if (pawn != null)
            {
                Messages.Message("PawnCantReachTransporters".Translate(
                    pawn.LabelShort
                ).CapitalizeFirst(), MessageTypeDefOf.RejectInput);
                return false;
            }
            Map map = this.transporters[0].parent.Map;
            for (int i = 0; i < this.transferables.Count; i++)
            {
                if (this.transferables[i].ThingDef.category == ThingCategory.Item)
                {
                    int countToTransfer = this.transferables[i].CountToTransfer;
                    int num = 0;
                    if (countToTransfer > 0)
                    {
                        for (int j = 0; j < this.transferables[i].things.Count; j++)
                        {
                            Thing thing = this.transferables[i].things[j];
                            if (map.reachability.CanReach(thing.Position, this.transporters[0].parent,
                                PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false)))
                            {
                                num += thing.stackCount;
                                if (num >= countToTransfer)
                                {
                                    break;
                                }
                            }
                        }
                        if (num < countToTransfer)
                        {
                            if (countToTransfer == 1)
                            {
                                Messages.Message("TransporterItemIsUnreachableSingle".Translate(
                                    this.transferables[i].ThingDef.label
                                ), MessageTypeDefOf.RejectInput);
                            }
                            else
                            {
                                Messages.Message("TransporterItemIsUnreachableMulti".Translate(
                                    countToTransfer,
                                    this.transferables[i].ThingDef.label
                                ), MessageTypeDefOf.RejectInput);
                            }
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private void AddPawnsToTransferables()
        {
            List<Pawn> list = CaravanFormingUtility.AllSendablePawns(this.map, false, false);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].TryGetComp<CompLaunchablePawn>() == null)
                    this.AddToTransferables(list[i]);
            }
        }

        private void AddItemsToTransferables()
        {
            List<Thing> list = CaravanFormingUtility.AllReachableColonyItems(this.map, false, false);
            for (int i = 0; i < list.Count; i++)
            {
                this.AddToTransferables(list[i]);
            }
        }

        private void FlashMass()
        {
            this.lastMassFlashTime = Time.time;
        }

        private void SetToLoadEverything()
        {
            for (int i = 0; i < this.transferables.Count; i++)
            {
                this.transferables[i].AdjustTo(this.transferables[i].GetMaximumToTransfer()); // SetToTransferMaxToDest();
                //TransferableUIUtility.ClearEditBuffer(this.transferables[i]);
            }
            this.CountToTransferChanged();
        }

        private void CountToTransferChanged()
        {
            this.massUsageDirty = true;
            this.daysWorthOfFoodDirty = true;
        }
    }
}