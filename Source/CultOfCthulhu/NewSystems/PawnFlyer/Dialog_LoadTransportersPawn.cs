using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cthulhu;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace CultOfCthulhu
{
    public class Dialog_LoadTransportersPawn : Window
    {
        private const float TitleRectHeight = 40f;

        private const float BottomAreaHeight = 55f;

        private static readonly List<TabRecord> tabsList = new List<TabRecord>();

        private readonly Vector2 BottomButtonSize = new Vector2(160f, 40f);

        private readonly Map map;

        private readonly List<CompTransporterPawn> transporters;

        private Pair<float, float> cachedDaysWorthOfFood;

        private Pair<ThingDef, float> cachedForagedFoodPerDay;

        private string cachedForagedFoodPerDayExplanation;

        private float cachedMassUsage;
        private float cachedTilesPerDay;

        private string cachedTilesPerDayExplanation;

        private float cachedVisibility;

        private string cachedVisibilityExplanation;

        private bool daysWorthOfFoodDirty = true;

        private bool foragedFoodPerDayDirty = true;

        private TransferableOneWayWidget itemsTransfer;

        private float lastMassFlashTime = -9999f;

        private bool massUsageDirty = true;

        private TransferableOneWayWidget pawnsTransfer;

        private Tab tab;

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

        private List<TransferableOneWay> transferables;

        private bool visibilityDirty = true;

        public Dialog_LoadTransportersPawn(Map map, List<CompTransporterPawn> transporters)
        {
            this.map = map;
            this.transporters = new List<CompTransporterPawn>();
            this.transporters.AddRange(transporters);
            //this.closeOnEscapeKey = true;
            closeOnAccept = false;
            closeOnCancel = false;
            forcePause = true;
            absorbInputAroundWindow = true;
        }

        public override Vector2 InitialSize => new Vector2(1024f, UI.screenHeight);

        protected override float Margin => 0f;

        private BiomeDef Biome => map.Biome;


        private Pair<ThingDef, float> ForagedFoodPerDay
        {
            get
            {
                if (!foragedFoodPerDayDirty)
                {
                    return cachedForagedFoodPerDay;
                }

                foragedFoodPerDayDirty = false;
                var stringBuilder = new StringBuilder();
                cachedForagedFoodPerDay = ForagedFoodPerDayCalculator.ForagedFoodPerDay(transferables,
                    Biome, Faction.OfPlayer, stringBuilder);
                cachedForagedFoodPerDayExplanation = stringBuilder.ToString();

                return cachedForagedFoodPerDay;
            }
        }

        private float Visibility
        {
            get
            {
                if (!visibilityDirty)
                {
                    return cachedVisibility;
                }

                visibilityDirty = false;
                var stringBuilder = new StringBuilder();
                cachedVisibility = CaravanVisibilityCalculator.Visibility(transferables, stringBuilder);
                cachedVisibilityExplanation = stringBuilder.ToString();

                return cachedVisibility;
            }
        }


        private int PawnCapacity
        {
            get
            {
                var num = 0;
                foreach (var compTransporterPawn in transporters)
                {
                    var result = 1; //In-case PawnFlyer doesn't work out
                    if (compTransporterPawn.parent is PawnFlyer pawnFlyer)
                    {
                        if (pawnFlyer.def is PawnFlyerDef pawnFlyerDef)
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
        ///     Modified to use PawnFlyerDef
        /// </summary>
        private float MassCapacity
        {
            get
            {
                var num = 0f;
                foreach (var compTransporterPawn in transporters)
                {
                    var result = 150f; //In-case PawnFlyer doesn't work out
                    if (compTransporterPawn.parent is PawnFlyer pawnFlyer)
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

        private string TransportersLabel => Find.ActiveLanguageWorker.Pluralize(transporters[0].parent.Label);

        private string TransportersLabelCap => TransportersLabel.CapitalizeFirst();

        private float MassUsage
        {
            get
            {
                if (!massUsageDirty)
                {
                    return cachedMassUsage;
                }

                massUsageDirty = false;
                cachedMassUsage = CollectionsMassCalculator.MassUsageTransferables(transferables,
                    IgnorePawnsInventoryMode.DontIgnore, true);

                return cachedMassUsage;
            }
        }

        private float TilesPerDay
        {
            get
            {
                if (!tilesPerDayDirty)
                {
                    return cachedTilesPerDay;
                }

                tilesPerDayDirty = false;
                var stringBuilder = new StringBuilder();
                cachedTilesPerDay = TilesPerDayCalculator.ApproxTilesPerDay(transferables, MassUsage,
                    MassCapacity, map.Tile, -1, stringBuilder);
                cachedTilesPerDayExplanation = stringBuilder.ToString();

                return cachedTilesPerDay;
            }
        }


        private Pair<float, float> DaysWorthOfFood
        {
            get
            {
                if (!daysWorthOfFoodDirty)
                {
                    return cachedDaysWorthOfFood;
                }

                daysWorthOfFoodDirty = false;
                var first = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(transferables, map.Tile,
                    IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload, Faction.OfPlayer, null, 0f, 3500);
                cachedDaysWorthOfFood = new Pair<float, float>(first,
                    DaysUntilRotCalculator.ApproxDaysUntilRot(transferables, map.Tile,
                        IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload, null, 0f, 3500));

                return cachedDaysWorthOfFood;
            }
        }

        public override void PostOpen()
        {
            base.PostOpen();
            CalculateAndRecacheTransferables();
        }

        public override void DoWindowContents(Rect inRect)
        {
            var rect = new Rect(0f, 0f, inRect.width, 35f);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, "LoadTransporters".Translate(
                TransportersLabel
            ));
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            CaravanUIUtility.DrawCaravanInfo(
                new CaravanUIUtility.CaravanInfo(MassUsage, MassCapacity, "", TilesPerDay,
                    cachedTilesPerDayExplanation, DaysWorthOfFood, ForagedFoodPerDay,
                    cachedForagedFoodPerDayExplanation, Visibility, cachedVisibilityExplanation), null,
                map.Tile, null, lastMassFlashTime, new Rect(12f, 35f, inRect.width - 24f, 40f), false);
            tabsList.Clear();
            tabsList.Add(new TabRecord("PawnsTab".Translate(),
                delegate { tab = Tab.Pawns; },
                tab == Tab.Pawns));
            tabsList.Add(new TabRecord("ItemsTab".Translate(),
                delegate { tab = Tab.Items; },
                tab == Tab.Items));
            inRect.yMin += 119f;
            Widgets.DrawMenuSection(inRect);
            TabDrawer.DrawTabs(inRect, tabsList);
            inRect = inRect.ContractedBy(17f);
            GUI.BeginGroup(inRect);
            var rect2 = inRect.AtZero();
            DoBottomButtons(rect2);
            var inRect2 = rect2;
            inRect2.yMax -= 59f;
            var flag = false;
            var tab1 = tab;
            if (tab1 != Tab.Pawns)
            {
                if (tab1 == Tab.Items)
                {
                    itemsTransfer.OnGUI(inRect2, out flag);
                }
            }
            else
            {
                pawnsTransfer.OnGUI(inRect2, out flag);
            }

            if (flag)
            {
                CountToTransferChanged();
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
            var transferableOneWay =
                TransferableUtility.TransferableMatching(t, transferables,
                    TransferAsOneMode.PodsOrCaravanPacking);
            if (transferableOneWay == null)
            {
                transferableOneWay = new TransferableOneWay();
                transferables.Add(transferableOneWay);
            }

            transferableOneWay.things.Add(t);
        }

        private void DoBottomButtons(Rect rect)
        {
            var rect2 = new Rect((rect.width / 2f) - (BottomButtonSize.x / 2f), rect.height - 55f,
                BottomButtonSize.x, BottomButtonSize.y);
            if (Widgets.ButtonText(rect2, "AcceptButton".Translate(), true, false) && TryAccept())
            {
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                Close(false);
            }

            var rect3 = new Rect(rect2.x - 10f - BottomButtonSize.x, rect2.y, BottomButtonSize.x,
                BottomButtonSize.y);
            if (Widgets.ButtonText(rect3, "ResetButton".Translate(), true, false))
            {
                SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                CalculateAndRecacheTransferables();
            }

            var rect4 = new Rect(rect2.xMax + 10f, rect2.y, BottomButtonSize.x, BottomButtonSize.y);
            if (Widgets.ButtonText(rect4, "CancelButton".Translate(), true, false))
            {
                Close();
            }

            if (!Prefs.DevMode)
            {
                return;
            }

            var num = 200f;
            var num2 = BottomButtonSize.y / 2f;
            var rect5 = new Rect(rect.width - num, rect.height - 55f, num, num2);
            if (Widgets.ButtonText(rect5, "Dev: Load instantly", true, false) && DebugTryLoadInstantly())
            {
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                Close(false);
            }

            var rect6 = new Rect(rect.width - num, rect.height - 55f + num2, num, num2);
            if (!Widgets.ButtonText(rect6, "Dev: Select everything", true, false))
            {
                return;
            }

            SoundDefOf.Tick_High.PlayOneShotOnCamera();
            SetToLoadEverything();
        }

        private void CalculateAndRecacheTransferables()
        {
            transferables = new List<TransferableOneWay>();
            AddPawnsToTransferables();
            AddItemsToTransferables();
            pawnsTransfer = new TransferableOneWayWidget(null, Faction.OfPlayer.Name, TransportersLabelCap,
                "FormCaravanColonyThingCountTip".Translate(), true, IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload,
                true, () => MassCapacity - MassUsage, 24f, false, map.Tile, true);
            CaravanUIUtility.AddPawnsSections(pawnsTransfer, transferables);
            itemsTransfer = new TransferableOneWayWidget(from x in transferables
                where x.ThingDef.category != ThingCategory.Pawn
                select x, Faction.OfPlayer.Name, TransportersLabelCap,
                "FormCaravanColonyThingCountTip".Translate(), true, IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload,
                true, () => MassCapacity - MassUsage, 24f, false, map.Tile, true);
            CountToTransferChanged();
        }

        private bool DebugTryLoadInstantly()
        {
            CreateAndAssignNewTransportersGroup();
            int i;
            for (i = 0; i < transferables.Count; i++)
            {
                var i1 = i;
                TransferableUtility.Transfer(transferables[i].things, transferables[i].CountToTransfer,
                    delegate(Thing splitPiece, IThingHolder _)
                    {
                        transporters[i1 % transporters.Count].GetDirectlyHeldThings().TryAdd(splitPiece);
                    });
            }

            return true;
        }

        private bool TryAccept()
        {
            var pawnsFromTransferables = TransferableUtility.GetPawnsFromTransferables(transferables);
            if (!CheckForErrors(pawnsFromTransferables))
            {
                Utility.DebugReport("TryAccept Failed");
                return false;
            }

            Utility.DebugReport("TryAccept Succeeded");
            var transportersGroup = CreateAndAssignNewTransportersGroup();
            AssignTransferablesToRandomTransporters();
            var enumerable = from x in pawnsFromTransferables
                where x.IsColonist && !x.Downed
                select x;
            if (enumerable.Any())
            {
                Utility.DebugReport("Pawn List Succeeded");
                LordMaker.MakeNewLord(Faction.OfPlayer, new LordJob_LoadAndEnterTransportersPawn(transportersGroup),
                    map, enumerable);
                foreach (var current in enumerable)
                {
                    if (current.Spawned)
                    {
                        current.jobs.EndCurrentJob(JobCondition.InterruptForced);
                    }
                }
            }

            Messages.Message("MessageTransportersLoadingProcessStarted".Translate(), transporters[0].parent,
                MessageTypeDefOf.PositiveEvent);
            return true;
        }

        private void AssignTransferablesToRandomTransporters()
        {
            Utility.DebugReport("AssignTransferablesToRandomTransporters Called");
            var transferableOneWay =
                transferables.MaxBy(x => x.CountToTransfer);
            var num = 0;
            foreach (var oneWay in transferables)
            {
                if (oneWay == transferableOneWay)
                {
                    continue;
                }

                if (oneWay.CountToTransfer <= 0)
                {
                    continue;
                }

                transporters[num % transporters.Count].AddToTheToLoadList(oneWay,
                    oneWay.CountToTransfer);
                num++;
            }

            if (num < transporters.Count)
            {
                var num2 = transferableOneWay.CountToTransfer;
                var num3 = num2 / (transporters.Count - num);
                for (var j = num; j < transporters.Count; j++)
                {
                    var num4 = j != transporters.Count - 1 ? num3 : num2;
                    if (num4 > 0)
                    {
                        transporters[j].AddToTheToLoadList(transferableOneWay, num4);
                    }

                    num2 -= num4;
                }
            }
            else
            {
                transporters[num % transporters.Count]
                    .AddToTheToLoadList(transferableOneWay, transferableOneWay.CountToTransfer);
            }
        }

        private int CreateAndAssignNewTransportersGroup()
        {
            Utility.DebugReport("CreateAndAssignNewTransportersGroup Called");
            var nextTransporterGroupID = Find.UniqueIDsManager.GetNextTransporterGroupID();
            foreach (var compTransporterPawn in transporters)
            {
                compTransporterPawn.groupID = nextTransporterGroupID;
            }

            return nextTransporterGroupID;
        }

        private bool CheckForErrors(List<Pawn> pawns)
        {
            if (!transferables.Any(x => x.CountToTransfer != 0))
            {
                Messages.Message("CantSendEmptyTransportPods".Translate(), MessageTypeDefOf.RejectInput);
                return false;
            }

            if (MassUsage > MassCapacity)
            {
                FlashMass();
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

            var pawn = pawns.Find(x => !x.MapHeld.reachability.CanReach(x.PositionHeld,
                transporters[0].parent, PathEndMode.Touch,
                TraverseParms.For(TraverseMode.PassDoors)));
            if (pawn != null)
            {
                Messages.Message("PawnCantReachTransporters".Translate(
                    pawn.LabelShort
                ).CapitalizeFirst(), MessageTypeDefOf.RejectInput);
                return false;
            }

            var parentMap = transporters[0].parent.Map;
            foreach (var transferableOneWay in transferables)
            {
                if (transferableOneWay.ThingDef.category != ThingCategory.Item)
                {
                    continue;
                }

                var countToTransfer = transferableOneWay.CountToTransfer;
                var num = 0;
                if (countToTransfer <= 0)
                {
                    continue;
                }

                foreach (var thing in transferableOneWay.things)
                {
                    if (!parentMap.reachability.CanReach(thing.Position, transporters[0].parent,
                        PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors)))
                    {
                        continue;
                    }

                    num += thing.stackCount;
                    if (num >= countToTransfer)
                    {
                        break;
                    }
                }

                if (num >= countToTransfer)
                {
                    continue;
                }

                if (countToTransfer == 1)
                {
                    Messages.Message("TransporterItemIsUnreachableSingle".Translate(
                        transferableOneWay.ThingDef.label
                    ), MessageTypeDefOf.RejectInput);
                }
                else
                {
                    Messages.Message("TransporterItemIsUnreachableMulti".Translate(
                        countToTransfer,
                        transferableOneWay.ThingDef.label
                    ), MessageTypeDefOf.RejectInput);
                }

                return false;
            }

            return true;
        }

        private void AddPawnsToTransferables()
        {
            var list = CaravanFormingUtility.AllSendablePawns(map);
            foreach (var pawn in list)
            {
                if (pawn.TryGetComp<CompLaunchablePawn>() == null)
                {
                    AddToTransferables(pawn);
                }
            }
        }

        private void AddItemsToTransferables()
        {
            var list = CaravanFormingUtility.AllReachableColonyItems(map);
            foreach (var thing in list)
            {
                AddToTransferables(thing);
            }
        }

        private void FlashMass()
        {
            lastMassFlashTime = Time.time;
        }

        private void SetToLoadEverything()
        {
            foreach (var transferableOneWay in transferables)
            {
                transferableOneWay.AdjustTo(transferableOneWay.GetMaximumToTransfer()); // SetToTransferMaxToDest();
                //TransferableUIUtility.ClearEditBuffer(this.transferables[i]);
            }

            CountToTransferChanged();
        }

        private void CountToTransferChanged()
        {
            massUsageDirty = true;
            daysWorthOfFoodDirty = true;
        }

        private enum Tab
        {
            Pawns,
            Items
        }
    }
}