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

        private readonly Vector2 BottomButtonSize = new Vector2(x: 160f, y: 40f);

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
            this.transporters.AddRange(collection: transporters);
            //this.closeOnEscapeKey = true;
            closeOnAccept = false;
            closeOnCancel = false;
            forcePause = true;
            absorbInputAroundWindow = true;
        }

        public override Vector2 InitialSize => new Vector2(x: 1024f, y: UI.screenHeight);

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
                cachedForagedFoodPerDay = ForagedFoodPerDayCalculator.ForagedFoodPerDay(transferables: transferables,
                    biome: Biome, faction: Faction.OfPlayer, explanation: stringBuilder);
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
                cachedVisibility = CaravanVisibilityCalculator.Visibility(transferables: transferables, explanation: stringBuilder);
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
                        result = pawnFlyer.GetStatValue(stat: StatDefOf.CarryingCapacity);
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

        private string TransportersLabel => Find.ActiveLanguageWorker.Pluralize(str: transporters[index: 0].parent.Label);

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
                cachedMassUsage = CollectionsMassCalculator.MassUsageTransferables(transferables: transferables,
                    ignoreInventory: IgnorePawnsInventoryMode.DontIgnore, includePawnsMass: true);

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
                cachedTilesPerDay = TilesPerDayCalculator.ApproxTilesPerDay(transferables: transferables, massUsage: MassUsage,
                    massCapacity: MassCapacity, tile: map.Tile, nextTile: -1, explanation: stringBuilder);
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
                var first = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(transferables: transferables, tile: map.Tile,
                    ignoreInventory: IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload, faction: Faction.OfPlayer, path: null, nextTileCostLeft: 0f, caravanTicksPerMove: 3500);
                cachedDaysWorthOfFood = new Pair<float, float>(first: first,
                    second: DaysUntilRotCalculator.ApproxDaysUntilRot(transferables: transferables, tile: map.Tile,
                        ignoreInventory: IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload, path: null, nextTileCostLeft: 0f, caravanTicksPerMove: 3500));

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
            var rect = new Rect(x: 0f, y: 0f, width: inRect.width, height: 35f);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect: rect, label: "LoadTransporters".Translate(
                arg1: TransportersLabel
            ));
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            CaravanUIUtility.DrawCaravanInfo(
                info: new CaravanUIUtility.CaravanInfo(massUsage: MassUsage, massCapacity: MassCapacity, massCapacityExplanation: "", tilesPerDay: TilesPerDay,
                    tilesPerDayExplanation: cachedTilesPerDayExplanation, daysWorthOfFood: DaysWorthOfFood, foragedFoodPerDay: ForagedFoodPerDay,
                    foragedFoodPerDayExplanation: cachedForagedFoodPerDayExplanation, visibility: Visibility, visibilityExplanation: cachedVisibilityExplanation), info2: null,
                currentTile: map.Tile, ticksToArrive: null, lastMassFlashTime: lastMassFlashTime, rect: new Rect(x: 12f, y: 35f, width: inRect.width - 24f, height: 40f), lerpMassColor: false);
            tabsList.Clear();
            tabsList.Add(item: new TabRecord(label: "PawnsTab".Translate(),
                clickedAction: delegate { tab = Tab.Pawns; },
                selected: tab == Tab.Pawns));
            tabsList.Add(item: new TabRecord(label: "ItemsTab".Translate(),
                clickedAction: delegate { tab = Tab.Items; },
                selected: tab == Tab.Items));
            inRect.yMin += 119f;
            Widgets.DrawMenuSection(rect: inRect);
            TabDrawer.DrawTabs(baseRect: inRect, tabs: tabsList);
            inRect = inRect.ContractedBy(margin: 17f);
            GUI.BeginGroup(position: inRect);
            var rect2 = inRect.AtZero();
            DoBottomButtons(rect: rect2);
            var inRect2 = rect2;
            inRect2.yMax -= 59f;
            var flag = false;
            var tab1 = tab;
            if (tab1 != Tab.Pawns)
            {
                if (tab1 == Tab.Items)
                {
                    itemsTransfer.OnGUI(inRect: inRect2, anythingChanged: out flag);
                }
            }
            else
            {
                pawnsTransfer.OnGUI(inRect: inRect2, anythingChanged: out flag);
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
                TransferableUtility.TransferableMatching(thing: t, transferables: transferables,
                    mode: TransferAsOneMode.PodsOrCaravanPacking);
            if (transferableOneWay == null)
            {
                transferableOneWay = new TransferableOneWay();
                transferables.Add(item: transferableOneWay);
            }

            transferableOneWay.things.Add(item: t);
        }

        private void DoBottomButtons(Rect rect)
        {
            var rect2 = new Rect(x: (rect.width / 2f) - (BottomButtonSize.x / 2f), y: rect.height - 55f,
                width: BottomButtonSize.x, height: BottomButtonSize.y);
            if (Widgets.ButtonText(rect: rect2, label: "AcceptButton".Translate(), drawBackground: true, doMouseoverSound: false) && TryAccept())
            {
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                Close(doCloseSound: false);
            }

            var rect3 = new Rect(x: rect2.x - 10f - BottomButtonSize.x, y: rect2.y, width: BottomButtonSize.x,
                height: BottomButtonSize.y);
            if (Widgets.ButtonText(rect: rect3, label: "ResetButton".Translate(), drawBackground: true, doMouseoverSound: false))
            {
                SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                CalculateAndRecacheTransferables();
            }

            var rect4 = new Rect(x: rect2.xMax + 10f, y: rect2.y, width: BottomButtonSize.x, height: BottomButtonSize.y);
            if (Widgets.ButtonText(rect: rect4, label: "CancelButton".Translate(), drawBackground: true, doMouseoverSound: false))
            {
                Close();
            }

            if (!Prefs.DevMode)
            {
                return;
            }

            var num = 200f;
            var num2 = BottomButtonSize.y / 2f;
            var rect5 = new Rect(x: rect.width - num, y: rect.height - 55f, width: num, height: num2);
            if (Widgets.ButtonText(rect: rect5, label: "Dev: Load instantly", drawBackground: true, doMouseoverSound: false) && DebugTryLoadInstantly())
            {
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                Close(doCloseSound: false);
            }

            var rect6 = new Rect(x: rect.width - num, y: rect.height - 55f + num2, width: num, height: num2);
            if (!Widgets.ButtonText(rect: rect6, label: "Dev: Select everything", drawBackground: true, doMouseoverSound: false))
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
            pawnsTransfer = new TransferableOneWayWidget(transferables: null, sourceLabel: Faction.OfPlayer.Name, destinationLabel: TransportersLabelCap,
                sourceCountDesc: "FormCaravanColonyThingCountTip".Translate(), drawMass: true, ignorePawnInventoryMass: IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload,
                includePawnsMassInMassUsage: true, availableMassGetter: () => MassCapacity - MassUsage, extraHeaderSpace: 24f, ignoreSpawnedCorpseGearAndInventoryMass: false, tile: map.Tile, drawMarketValue: true);
            CaravanUIUtility.AddPawnsSections(widget: pawnsTransfer, transferables: transferables);
            itemsTransfer = new TransferableOneWayWidget(transferables: from x in transferables
                where x.ThingDef.category != ThingCategory.Pawn
                select x, sourceLabel: Faction.OfPlayer.Name, destinationLabel: TransportersLabelCap,
                sourceCountDesc: "FormCaravanColonyThingCountTip".Translate(), drawMass: true, ignorePawnInventoryMass: IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload,
                includePawnsMassInMassUsage: true, availableMassGetter: () => MassCapacity - MassUsage, extraHeaderSpace: 24f, ignoreSpawnedCorpseGearAndInventoryMass: false, tile: map.Tile, drawMarketValue: true);
            CountToTransferChanged();
        }

        private bool DebugTryLoadInstantly()
        {
            CreateAndAssignNewTransportersGroup();
            int i;
            for (i = 0; i < transferables.Count; i++)
            {
                var i1 = i;
                TransferableUtility.Transfer(things: transferables[index: i].things, count: transferables[index: i].CountToTransfer,
                    transferred: delegate(Thing splitPiece, IThingHolder _)
                    {
                        transporters[index: i1 % transporters.Count].GetDirectlyHeldThings().TryAdd(item: splitPiece);
                    });
            }

            return true;
        }

        private bool TryAccept()
        {
            var pawnsFromTransferables = TransferableUtility.GetPawnsFromTransferables(transferables: transferables);
            if (!CheckForErrors(pawns: pawnsFromTransferables))
            {
                Utility.DebugReport(x: "TryAccept Failed");
                return false;
            }

            Utility.DebugReport(x: "TryAccept Succeeded");
            var transportersGroup = CreateAndAssignNewTransportersGroup();
            AssignTransferablesToRandomTransporters();
            var enumerable = from x in pawnsFromTransferables
                where x.IsColonist && !x.Downed
                select x;
            if (enumerable.Any())
            {
                Utility.DebugReport(x: "Pawn List Succeeded");
                LordMaker.MakeNewLord(faction: Faction.OfPlayer, lordJob: new LordJob_LoadAndEnterTransportersPawn(transportersGroup: transportersGroup),
                    map: map, startingPawns: enumerable);
                foreach (var current in enumerable)
                {
                    if (current.Spawned)
                    {
                        current.jobs.EndCurrentJob(condition: JobCondition.InterruptForced);
                    }
                }
            }

            Messages.Message(text: "PawnFlyer_MessageLoadingProcessStarted".Translate(), lookTargets: transporters[index: 0].parent,
                def: MessageTypeDefOf.PositiveEvent);
            return true;
        }

        private void AssignTransferablesToRandomTransporters()
        {
            Utility.DebugReport(x: "AssignTransferablesToRandomTransporters Called");
            var transferableOneWay =
                transferables.MaxBy(selector: x => x.CountToTransfer);
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

                transporters[index: num % transporters.Count].AddToTheToLoadList(t: oneWay,
                    count: oneWay.CountToTransfer);
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
                        transporters[index: j].AddToTheToLoadList(t: transferableOneWay, count: num4);
                    }

                    num2 -= num4;
                }
            }
            else
            {
                transporters[index: num % transporters.Count]
                    .AddToTheToLoadList(t: transferableOneWay, count: transferableOneWay.CountToTransfer);
            }
        }

        private int CreateAndAssignNewTransportersGroup()
        {
            Utility.DebugReport(x: "CreateAndAssignNewTransportersGroup Called");
            var nextTransporterGroupID = Find.UniqueIDsManager.GetNextTransporterGroupID();
            foreach (var compTransporterPawn in transporters)
            {
                compTransporterPawn.groupID = nextTransporterGroupID;
            }

            return nextTransporterGroupID;
        }

        private bool CheckForErrors(List<Pawn> pawns)
        {
            if (!transferables.Any(predicate: x => x.CountToTransfer != 0))
            {
                Messages.Message(text: "CantSendEmptyTransportPods".Translate(), def: MessageTypeDefOf.RejectInput);
                return false;
            }

            if (MassUsage > MassCapacity)
            {
                FlashMass();
                Messages.Message(text: "PawnFlyer_TooBigMassUsage".Translate(), def: MessageTypeDefOf.RejectInput);
                return false;
            }

            if (pawns.Count > PawnCapacity)
            {
                Messages.Message(text: "OverPawnRiderLimit".Translate( //Translation: ok
                    arg1: PawnCapacity.ToString()
                ), def: MessageTypeDefOf.RejectInput);
                return false;
            }

            var pawn = pawns.Find(match: x => !x.MapHeld.reachability.CanReach(start: x.PositionHeld,
                dest: transporters[index: 0].parent, peMode: PathEndMode.Touch,
                traverseParams: TraverseParms.For(mode: TraverseMode.PassDoors)));
            if (pawn != null)
            {
                Messages.Message(text: "PawnFlyer_PawnCantReach".Translate(
                    arg1: pawn.LabelShort
                ).CapitalizeFirst(), def: MessageTypeDefOf.RejectInput);
                return false;
            }

            var parentMap = transporters[index: 0].parent.Map;
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
                    if (!parentMap.reachability.CanReach(start: thing.Position, dest: transporters[index: 0].parent,
                        peMode: PathEndMode.Touch, traverseParams: TraverseParms.For(mode: TraverseMode.PassDoors)))
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
                    Messages.Message(text: "PawnFlyer_ItemIsUnreachableSingle".Translate(
                        arg1: transferableOneWay.ThingDef.label
                    ), def: MessageTypeDefOf.RejectInput);
                }
                else
                {
                    Messages.Message(text: "PawnFlyer_ItemIsUnreachableMulti".Translate(
                        arg1: countToTransfer,
                        arg2: transferableOneWay.ThingDef.label
                    ), def: MessageTypeDefOf.RejectInput);
                }

                return false;
            }

            return true;
        }

        private void AddPawnsToTransferables()
        {
            var list = CaravanFormingUtility.AllSendablePawns(map: map);
            foreach (var pawn in list)
            {
                if (pawn.TryGetComp<CompLaunchablePawn>() == null)
                {
                    AddToTransferables(t: pawn);
                }
            }
        }

        private void AddItemsToTransferables()
        {
            var list = CaravanFormingUtility.AllReachableColonyItems(map: map);
            foreach (var thing in list)
            {
                AddToTransferables(t: thing);
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
                transferableOneWay.AdjustTo(destination: transferableOneWay.GetMaximumToTransfer()); // SetToTransferMaxToDest();
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