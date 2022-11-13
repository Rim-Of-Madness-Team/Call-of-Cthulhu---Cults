using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cthulhu;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace CultOfCthulhu
{
    [StaticConstructorOnStartup]
    public class CompLaunchablePawn : ThingComp
    {
        private static readonly int maxTileDistance = 120;

        private static readonly Texture2D TargeterMouseAttachment =
            ContentFinder<Texture2D>.Get(itemPath: "UI/Overlays/LaunchableMouseAttachment");

        private static readonly Texture2D LaunchCommandTex =
            ContentFinder<Texture2D>.Get(itemPath: "UI/Commands/LaunchShip");

        private CompTransporterPawn cachedCompTransporter;

        public bool LoadingInProgressOrReadyToLaunch => Transporter.LoadingInProgressOrReadyToLaunch;

        public bool AnythingLeftToLoad => Transporter.AnythingLeftToLoad;

        public Thing FirstThingLeftToLoad => Transporter.FirstThingLeftToLoad;

        public List<CompTransporterPawn> TransportersInGroup => Transporter.TransportersInGroup(map: parent.Map);

        public bool AnyInGroupHasAnythingLeftToLoad => Transporter.AnyInGroupHasAnythingLeftToLoad;

        public Thing FirstThingLeftToLoadInGroup => Transporter.FirstThingLeftToLoadInGroup;

        public bool AnyInGroupIsUnderRoof
        {
            get
            {
                var transportersInGroup = TransportersInGroup;
                if (transportersInGroup?.FirstOrDefault() == null) return false;
                foreach (var compTransporterPawn in transportersInGroup)
                {
                    if (compTransporterPawn?.parent?.Position.Roofed(map: parent.Map) == true)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public CompTransporterPawn Transporter
        {
            get
            {
                if (cachedCompTransporter == null)
                {
                    cachedCompTransporter = parent.GetComp<CompTransporterPawn>();
                }

                return cachedCompTransporter;
            }
        }

        public PawnFlyerDef PawnFlyerDef
        {
            get
            {
                var result = parent.def as PawnFlyerDef;
                if (result == null)
                {
                    Log.Error(text: "PawnFlyerDef is null");
                }

                return result;
            }
        }

        public int MaxLaunchDistance => !LoadingInProgressOrReadyToLaunch ? 0 : PawnFlyerDef.flyableDistance;

        [DebuggerHidden]
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            using var enumerator = base.CompGetGizmosExtra().GetEnumerator();
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                yield return current;
            }

            if (LoadingInProgressOrReadyToLaunch)
            {
                var command_Action = new Command_Action
                {
                    defaultLabel = "PawnFlyer_CommandTakeOffGroup".Translate(),
                    defaultDesc = "CommandLaunchGroupDesc".Translate(), //Translaton: ok
                    icon = LaunchCommandTex,
                    action = delegate
                    {
                        if (AnyInGroupHasAnythingLeftToLoad)
                        {
                            Find.WindowStack.Add(window: Dialog_MessageBox.CreateConfirmation(
                                text: "PawnFlyer_ConfirmSendNotCompletelyLoaded".Translate(
                                    arg1: FirstThingLeftToLoadInGroup.LabelCap
                                ), confirmedAct: StartChoosingDestination));
                        }
                        else
                        {
                            StartChoosingDestination();
                        }
                    }
                };
                if (AnyInGroupIsUnderRoof)
                {
                    command_Action.Disable(reason: "PawnFlyer_CommandLaunchGroupFailUnderRoof".Translate());
                }

                yield return command_Action;
            }
        }

        public override string CompInspectStringExtra()
        {
            if (!LoadingInProgressOrReadyToLaunch)
            {
                return null;
            }

            return AnyInGroupHasAnythingLeftToLoad
                ? (string)("PawnFlyer_NotReadyForTakeOff".Translate() + ": " +
                           "TransportPodInGroupHasSomethingLeftToLoad".Translate() + ".") //Translation: ok
                : (string)"PawnFlyer_ReadyForTakeOff".Translate();
        }

        public void StartChoosingDestination()
        {
            CameraJumper.TryJump(target: CameraJumper.GetWorldTarget(target: this.parent));
            Find.WorldSelector.ClearSelection();
            int tile = this.parent.Map.Tile;
            Find.WorldTargeter.BeginTargeting(action: new Func<GlobalTargetInfo, bool>(this.ChoseWorldTarget),
                canTargetTiles: true, mouseAttachment: CompLaunchable.TargeterMouseAttachment,
                closeWorldTabWhenFinished: true,
                onUpdate: delegate { GenDraw.DrawWorldRadiusRing(center: tile, radius: this.MaxLaunchDistance); },
                extraLabelGetter: (GlobalTargetInfo target) => CompLaunchablePawn.TargetingLabelGetter(target: target,
                    tile: tile, maxLaunchDistance: this.MaxLaunchDistance,
                    pods: this.TransportersInGroup.Cast<IThingHolder>(),
                    launchAction: new Action<int, PawnFlyerArrivalAction>(this.TryLaunch), launchable: this),
                canSelectTarget: null);
        }

        public static IEnumerable<FloatMenuOption> GetOptionsForTile(int tile, IEnumerable<IThingHolder> pods,
            Action<int, PawnFlyerArrivalAction> launchAction)
        {
            bool anything = false;
            if (PawnFlyerArrivalAction_FormCaravan.CanFormCaravanAt(pods, tile) &&
                !Find.WorldObjects.AnySettlementBaseAt(tile) && !Find.WorldObjects.AnySiteAt(tile))
            {
                anything = true;
                yield return new FloatMenuOption("FormCaravanHere".Translate(), //Translation: ok
                    delegate { launchAction(tile, new PawnFlyerArrivalAction_FormCaravan("MessageShuttleArrived")); }); //Translation: ok
            }

            List<WorldObject> worldObjects = Find.WorldObjects.AllWorldObjects;
            for (int i = 0; i < worldObjects.Count; i++)
            {
                if (worldObjects[i].Tile != tile)
                {
                    continue;
                }
                
                switch (worldObjects[i])
                {
                    case Settlement s:
                    {
                        foreach (FloatMenuOption pawnFlyerFloatMenuOption in PawnFlyerArrivalActionUtility.GetSettlementFloatMenuOptions(pods, launchAction, s))
                        {
                            anything = true;
                            yield return pawnFlyerFloatMenuOption;
                        }

                        break;
                    }
                    case Site s:
                    {
                        foreach (FloatMenuOption pawnFlyerFloatMenuOption in PawnFlyerArrivalActionUtility.GetSiteFloatMenuOptions(pods, launchAction, s))
                        {
                            anything = true;
                            yield return pawnFlyerFloatMenuOption;
                        }

                        break;
                    }
                    case MapParent s:
                    {
                        foreach (FloatMenuOption pawnFlyerFloatMenuOption in PawnFlyerArrivalActionUtility.GetMapParentFloatMenuOptions(pods, launchAction, s))
                        {
                            anything = true;
                            yield return pawnFlyerFloatMenuOption;
                        }

                        break;
                    }
                }
            }

            if (!anything && !Find.World.Impassable(tile))
            {
                yield return new FloatMenuOption("TransportPodsContentsWillBeLost".Translate(), //Translation: ok
                    delegate { launchAction(tile, null); });
            }
        }


        public static string TargetingLabelGetter(GlobalTargetInfo target, int tile, int maxLaunchDistance,
            IEnumerable<IThingHolder> pods, Action<int, PawnFlyerArrivalAction> launchAction,
            CompLaunchablePawn launchable)
        {
            if (!target.IsValid)
            {
                return null;
            }

            int num = Find.WorldGrid.TraversalDistanceBetween(start: tile, end: target.Tile, passImpassable: true,
                maxDist: int.MaxValue);
            if (maxLaunchDistance > 0 && num > maxLaunchDistance)
            {
                GUI.color = ColorLibrary.RedReadable;
                return "TransportPodDestinationBeyondMaximumRange".Translate(); //Translation: ok
            }

            IEnumerable<FloatMenuOption> source = (launchable != null)
                ? launchable.GetPawnFlyerFloatMenuOptionsAt(tile: target.Tile)
                : CompLaunchablePawn.GetOptionsForTile(tile: target.Tile, pods: pods, launchAction: launchAction);
            if (!source.Any<FloatMenuOption>())
            {
                return string.Empty;
            }

            if (source.Count<FloatMenuOption>() == 1)
            {
                if (source.First<FloatMenuOption>().Disabled)
                {
                    GUI.color = ColorLibrary.RedReadable;
                }

                return source.First<FloatMenuOption>().Label;
            }

            MapParent mapParent;
            if ((mapParent = (target.WorldObject as MapParent)) != null)
            {
                return "ClickToSeeAvailableOrders_WorldObject".Translate(arg1: mapParent.LabelCap); //Translation: ok
            }

            return "ClickToSeeAvailableOrders_Empty".Translate(); //Translation: ok
        }

        private IEnumerable<FloatMenuOption> GetPawnFlyerFloatMenuOptionsAt(int tile)
        {
            bool anything = false;
            if (TransportPodsArrivalAction_FormCaravan.CanFormCaravanAt(
                    pods: this.TransportersInGroup.Cast<IThingHolder>(), tile: tile) &&
                !Find.WorldObjects.AnySettlementBaseAt(tile: tile) && !Find.WorldObjects.AnySiteAt(tile: tile))
            {
                anything = true;
                yield return new FloatMenuOption(label: "FormCaravanHere".Translate(), //Translation: ok
                    action: delegate()
                    {
                        this.TryLaunch(destinationTile: tile,
                            arrivalAction: new PawnFlyerArrivalAction_FormCaravan());
                    }, priority: MenuOptionPriority.Default, mouseoverGuiAction: null, revalidateClickTarget: null,
                    extraPartWidth: 0f, extraPartOnGUI: null, revalidateWorldClickTarget: null,
                    playSelectionSound: true, orderInPriority: 0);
            }

            List<WorldObject> worldObjects = Find.WorldObjects.AllWorldObjects;
            int num;
            for (int i = 0; i < worldObjects.Count; i = num + 1)
            {
                if (worldObjects[index: i].Tile == tile)
                {
                    if (worldObjects[index: i] is Caravan caravan)
                    {
                        foreach (FloatMenuOption floatMenuOption in PawnFlyerArrivalAction_GiveToCaravan
                                     .GetFloatMenuOptions(representative: this,
                                         pods: this.TransportersInGroup.Cast<IThingHolder>(), caravan: caravan))
                        {
                            yield return floatMenuOption;
                        }
                    }

                    if (worldObjects[index: i] is Site site)
                    {
                        yield return new FloatMenuOption(label: "PawnFlyer_LandAtEdge".Translate(arg1: site.Label),
                            action: delegate
                            {
                                this.TryLaunch(destinationTile: tile,
                                    arrivalAction: new PawnFlyerArrivalAction_VisitSite(site: site,
                                        arrivalMode: PawnsArrivalModeDefOf.EdgeDrop));
                            }, priority: MenuOptionPriority.Default, mouseoverGuiAction: null,
                            revalidateClickTarget: null, extraPartWidth: 0f, extraPartOnGUI: null,
                            revalidateWorldClickTarget: null, playSelectionSound: true, orderInPriority: 0);
                        yield return new FloatMenuOption(label: "PawnFlyer_LandInCenter".Translate(arg1: site.Label),
                            action: delegate
                            {
                                this.TryLaunch(destinationTile: tile,
                                    arrivalAction: new PawnFlyerArrivalAction_VisitSite(site: site,
                                        arrivalMode: PawnsArrivalModeDefOf.CenterDrop));
                            }, priority: MenuOptionPriority.Default, mouseoverGuiAction: null,
                            revalidateClickTarget: null, extraPartWidth: 0f, extraPartOnGUI: null,
                            revalidateWorldClickTarget: null, playSelectionSound: true, orderInPriority: 0);
                    }

                    if (worldObjects[index: i] is Settlement settlement)
                    {
                        if (!settlement.Faction.IsPlayer)
                        {
                            yield return new FloatMenuOption(
                                label: "PawnFlyer_AttackAndLandAtEdge".Translate(arg1: settlement.Label),
                                action: delegate
                                {
                                    this.TryLaunch(destinationTile: tile,
                                        arrivalAction: new PawnFlyerArrivalAction_AttackSettlement(settlement: settlement,
                                            arrivalMode: PawnsArrivalModeDefOf.EdgeDrop));
                                }, priority: MenuOptionPriority.Default, mouseoverGuiAction: null,
                                revalidateClickTarget: null, extraPartWidth: 0f, extraPartOnGUI: null,
                                revalidateWorldClickTarget: null, playSelectionSound: true, orderInPriority: 0);
                            yield return new FloatMenuOption(
                                label: "PawnFlyer_AttackAndLandInCenter".Translate(arg1: settlement.Label),
                                action: delegate
                                {
                                    this.TryLaunch(destinationTile: tile,
                                        arrivalAction: new PawnFlyerArrivalAction_AttackSettlement(settlement: settlement,
                                            arrivalMode: PawnsArrivalModeDefOf.CenterDrop));
                                }, priority: MenuOptionPriority.Default, mouseoverGuiAction: null,
                                revalidateClickTarget: null, extraPartWidth: 0f, extraPartOnGUI: null,
                                revalidateWorldClickTarget: null, playSelectionSound: true, orderInPriority: 0);
                        }

                        foreach (FloatMenuOption floatMenuOption in PawnFlyerArrivalAction_VisitSettlement
                                     .GetFloatMenuOptions(representative: this,
                                         pods: this.TransportersInGroup.Cast<IThingHolder>(), settlement: settlement))
                        {
                            yield return floatMenuOption;
                        }

                        foreach (FloatMenuOption floatMenuOption in PawnFlyerArrivalAction_GiveGift.GetFloatMenuOptions(
                                     representative: this, pods: this.TransportersInGroup.Cast<IThingHolder>(),
                                     settlement: settlement))
                        {
                            yield return floatMenuOption;
                        }
                    }
                }

                num = i;
            }

            if (!anything && !Find.World.Impassable(tileID: tile))
            {
                yield return new FloatMenuOption(label: "TransportPodsContentsWillBeLost".Translate(),
                    action: delegate() { this.TryLaunch(destinationTile: tile, arrivalAction: null); },
                    priority: MenuOptionPriority.Default, mouseoverGuiAction: null, revalidateClickTarget: null,
                    extraPartWidth: 0f, extraPartOnGUI: null, revalidateWorldClickTarget: null,
                    playSelectionSound: true, orderInPriority: 0);
            }

            yield break;
        }


        private bool ChoseWorldTarget(GlobalTargetInfo target)
        {
            return !this.LoadingInProgressOrReadyToLaunch || CompLaunchablePawn.ChoseWorldTarget(target: target,
                tile: this.parent.Map.Tile, pods: this.TransportersInGroup.Cast<IThingHolder>(),
                maxLaunchDistance: this.MaxLaunchDistance,
                launchAction: new Action<int, PawnFlyerArrivalAction>(this.TryLaunch), launchable: this);
        }

        public static bool ChoseWorldTarget(GlobalTargetInfo target, int tile, IEnumerable<IThingHolder> pods,
            int maxLaunchDistance, Action<int, PawnFlyerArrivalAction> launchAction, CompLaunchablePawn launchable)
        {
            if (!target.IsValid)
            {
                //Translation is ok
                Messages.Message(text: "MessageTransportPodsDestinationIsInvalid".Translate(),
                    def: MessageTypeDefOf.RejectInput, historical: false);
                return false;
            }

            int num = Find.WorldGrid.TraversalDistanceBetween(start: tile, end: target.Tile, passImpassable: true,
                maxDist: int.MaxValue);
            if (maxLaunchDistance > 0 && num > maxLaunchDistance)
            {
                //Translation is ok
                Messages.Message(text: "TransportPodDestinationBeyondMaximumRange".Translate(),
                    def: MessageTypeDefOf.RejectInput, historical: false);
                return false;
            }

            IEnumerable<FloatMenuOption> source = (launchable != null)
                ? launchable.GetPawnFlyerFloatMenuOptionsAt(tile: target.Tile)
                : null;//: CompLaunchable.GetOptionsForTile(tile: target.Tile, pods: pods, launchAction: launchAction);
            if (!source.Any<FloatMenuOption>())
            {
                if (Find.World.Impassable(tileID: target.Tile))
                {
                    Messages.Message(text: "MessageTransportPodsDestinationIsInvalid".Translate(),
                        def: MessageTypeDefOf.RejectInput, historical: false);
                    return false;
                }

                launchAction(arg1: target.Tile, arg2: null);
                return true;
            }
            else
            {
                if (source.Count<FloatMenuOption>() != 1)
                {
                    Find.WindowStack.Add(window: new FloatMenu(options: source.ToList<FloatMenuOption>()));
                    return false;
                }

                if (!source.First<FloatMenuOption>().Disabled)
                {
                    source.First<FloatMenuOption>().action();
                    return true;
                }

                return false;
            }
        }

        public void TryLaunch(int destinationTile, PawnFlyerArrivalAction arrivalAction)
        {
            Utility.DebugReport(x: "TryLaunch Called");
            if (!parent.Spawned)
            {
                Log.Error(text: "Tried to launch " + parent + ", but it's unspawned.");
                return;
            }

            var transportersInGroup = TransportersInGroup;
            if (transportersInGroup == null)
            {
                Log.Error(text: "Tried to launch " + parent + ", but it's not in any group.");
                return;
            }

            if (!LoadingInProgressOrReadyToLaunch)
            {
                Utility.DebugReport(x: "TryLaunch Failed");
                return;
            }

            var map = parent.Map;
            var num = Find.WorldGrid.TraversalDistanceBetween(start: map.Tile, end: destinationTile);
            if (num > MaxLaunchDistance)
            {
                Utility.DebugReport(x: "TryLaunch Failed #2");
                return;
            }

            Transporter.TryRemoveLord(map: map);
            var groupID = Transporter.groupID;
            foreach (var compTransporterPawn in transportersInGroup)
            {
                var compTransporter = compTransporterPawn;
                var originalPawn = (Pawn)compTransporter.parent;
                var oldPosition = compTransporter.parent.Position;
                PawnFlyersCargo activePawnFlyerCargo =
                    (PawnFlyersCargo)ThingMaker.MakeThing(def: ThingDef.Named("ByakheeDrop"), stuff: null);
                activePawnFlyerCargo.Contents = new ActiveDropPodInfo();
                activePawnFlyerCargo.Contents.innerContainer.TryAddRangeOrTransfer(
                    things: compTransporter.GetDirectlyHeldThings(), canMergeWithExistingStacks: true,
                    destroyLeftover: true);
                activePawnFlyerCargo.pawnFlyer = originalPawn;
                PawnFlyersLeaving flyShipLeaving = (PawnFlyersLeaving)SkyfallerMaker.MakeSkyfaller(
                    skyfaller: PawnFlyerDef.leavingDef ?? ThingDefOf.DropPodLeaving, innerThing: activePawnFlyerCargo);
                flyShipLeaving.groupID = groupID;
                flyShipLeaving.destinationTile = destinationTile;
                flyShipLeaving.arrivalAction = arrivalAction;
                flyShipLeaving.worldObjectDef = PawnFlyerDef.travelingDef;
                compTransporter.CleanUpLoadingVars(map: map);
                Utility.DebugReport(originalPawn.ToString());
                originalPawn.ClearAllReservations();
                originalPawn.ClearMind();
                originalPawn.mindState.Active = false;
                originalPawn.DeSpawn();
                flyShipLeaving.Contents.innerContainer.TryAdd(originalPawn);
                GenSpawn.Spawn(newThing: flyShipLeaving, loc: oldPosition, map: map, wipeMode: WipeMode.Vanish);
            }

            CameraJumper.TryHideWorld();
        }
    }
}