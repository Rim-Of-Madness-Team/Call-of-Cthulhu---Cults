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
            ContentFinder<Texture2D>.Get("UI/Overlays/LaunchableMouseAttachment");

        private static readonly Texture2D LaunchCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/LaunchShip");

        private CompTransporterPawn cachedCompTransporter;

        public bool LoadingInProgressOrReadyToLaunch => Transporter.LoadingInProgressOrReadyToLaunch;

        public bool AnythingLeftToLoad => Transporter.AnythingLeftToLoad;

        public Thing FirstThingLeftToLoad => Transporter.FirstThingLeftToLoad;

        public List<CompTransporterPawn> TransportersInGroup => Transporter.TransportersInGroup(parent.Map);

        public bool AnyInGroupHasAnythingLeftToLoad => Transporter.AnyInGroupHasAnythingLeftToLoad;

        public Thing FirstThingLeftToLoadInGroup => Transporter.FirstThingLeftToLoadInGroup;

        public bool AnyInGroupIsUnderRoof
        {
            get
            {
                var transportersInGroup = TransportersInGroup;
                foreach (var compTransporterPawn in transportersInGroup)
                {
                    if (compTransporterPawn.parent.Position.Roofed(parent.Map))
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
                    Log.Error("PawnFlyerDef is null");
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
                    defaultLabel = "CommandLaunchGroup".Translate(),
                    defaultDesc = "CommandLaunchGroupDesc".Translate(),
                    icon = LaunchCommandTex,
                    action = delegate
                    {
                        if (AnyInGroupHasAnythingLeftToLoad)
                        {
                            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                                "ConfirmSendNotCompletelyLoadedPods".Translate(
                                    FirstThingLeftToLoadInGroup.LabelCap
                                ), StartChoosingDestination));
                        }
                        else
                        {
                            StartChoosingDestination();
                        }
                    }
                };
                if (AnyInGroupIsUnderRoof)
                {
                    command_Action.Disable("CommandLaunchGroupFailUnderRoof".Translate());
                }

                yield return command_Action;
            }
            else
            {
                var command_Action = new Command_Action
                {
                    defaultLabel = "DEBUG",
                    defaultDesc = "CommandLaunchGroupDesc".Translate(),
                    icon = LaunchCommandTex,
                    action = delegate
                    {
                        if (AnyInGroupHasAnythingLeftToLoad)
                        {
                            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                                "ConfirmSendNotCompletelyLoadedPods".Translate(
                                    FirstThingLeftToLoadInGroup.LabelCap
                                ), StartChoosingDestination));
                        }
                        else
                        {
                            StartChoosingDestination();
                        }
                    }
                };
                if (AnyInGroupIsUnderRoof)
                {
                    command_Action.Disable("CommandLaunchGroupFailUnderRoof".Translate());
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
                ? (string) ("NotReadyForLaunch".Translate() + ": " +
                            "TransportPodInGroupHasSomethingLeftToLoad".Translate() + ".")
                : (string) "ReadyForLaunch".Translate();
        }

        public void StartChoosingDestination()
        {
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(this.parent));
            Find.WorldSelector.ClearSelection();
            int tile = this.parent.Map.Tile;
            Find.WorldTargeter.BeginTargeting(new Func<GlobalTargetInfo, bool>(this.ChoseWorldTarget), true, CompLaunchable.TargeterMouseAttachment, true, delegate
            {
                GenDraw.DrawWorldRadiusRing(tile, this.MaxLaunchDistance);
            }, (GlobalTargetInfo target) => CompLaunchablePawn.TargetingLabelGetter(target, tile, this.MaxLaunchDistance, this.TransportersInGroup.Cast<IThingHolder>(), new Action<int, TransportPodsArrivalAction>(this.TryLaunch), this), null);
        }
        public static string TargetingLabelGetter(GlobalTargetInfo target, int tile, int maxLaunchDistance, IEnumerable<IThingHolder> pods, Action<int, TransportPodsArrivalAction> launchAction, CompLaunchablePawn launchable)
        {
            if (!target.IsValid)
            {
                return null;
            }
            int num = Find.WorldGrid.TraversalDistanceBetween(tile, target.Tile, true, int.MaxValue);
            if (maxLaunchDistance > 0 && num > maxLaunchDistance)
            {
                GUI.color = ColorLibrary.RedReadable;
                return "TransportPodDestinationBeyondMaximumRange".Translate();
            }
            IEnumerable<FloatMenuOption> source = (launchable != null) ? launchable.GetTransportPodsFloatMenuOptionsAt(target.Tile) : CompLaunchable.GetOptionsForTile(target.Tile, pods, launchAction);
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
                return "ClickToSeeAvailableOrders_WorldObject".Translate(mapParent.LabelCap);
            }
            return "ClickToSeeAvailableOrders_Empty".Translate();
        }

        //RimWorld 1.0
        //public void StartChoosingDestination()
        //{
        //    CameraJumper.TryJump(CameraJumper.GetWorldTarget(parent));
        //    Find.WorldSelector.ClearSelection();
        //    var tile = parent.Map.Tile;
        //    Find.WorldTargeter.BeginTargeting(ChoseWorldTarget, true, TargeterMouseAttachment, true,
        //        delegate { GenDraw.DrawWorldRadiusRing(tile, MaxLaunchDistance); }, delegate(GlobalTargetInfo target)
        //        {
        //            if (!target.IsValid)
        //            {
        //                return null;
        //            }

        //            var num = Find.WorldGrid.TraversalDistanceBetween(tile, target.Tile);
        //            if (num <= MaxLaunchDistance)
        //            {
        //                return null;
        //            }

        //            return num > maxTileDistance
        //                ? (string) "TransportPodDestinationBeyondMaximumRange".Translate()
        //                : (string) "TransportPodNotEnoughFuel".Translate();
        //        });
        //}

        private IEnumerable<FloatMenuOption> GetTransportPodsFloatMenuOptionsAt(int tile)
        {
            bool anything = false;
            if (TransportPodsArrivalAction_FormCaravan.CanFormCaravanAt(this.TransportersInGroup.Cast<IThingHolder>(), tile) && !Find.WorldObjects.AnySettlementBaseAt(tile) && !Find.WorldObjects.AnySiteAt(tile))
            {
                anything = true;
                yield return new FloatMenuOption("FormCaravanHere".Translate(), delegate ()
                {
                    this.TryLaunch(tile, new TransportPodsArrivalAction_FormCaravan());
                }, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
            }
            List<WorldObject> worldObjects = Find.WorldObjects.AllWorldObjects;
            int num;
            for (int i = 0; i < worldObjects.Count; i = num + 1)
            {
                if (worldObjects[i].Tile == tile)
                {
                    if (worldObjects[i] is Caravan caravan)
                    {
                        foreach (FloatMenuOption floatMenuOption in ByakheeArrivalAction_GiveToCaravan.GetFloatMenuOptions(this, this.TransportersInGroup.Cast<IThingHolder>(), caravan))
                        {
                            yield return floatMenuOption;
                        }
                    }
                    if (worldObjects[i] is Site site)
                    {
                        yield return new FloatMenuOption("DropAtEdge".Translate(site.Label), delegate
                        {
                            this.TryLaunch(tile, new ByakheeArrivalAction_VisitSite(site, PawnsArrivalModeDefOf.EdgeDrop));
                        }, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                        yield return new FloatMenuOption("DropInCenter".Translate(site.Label), delegate
                        {
                            this.TryLaunch(tile, new ByakheeArrivalAction_VisitSite(site, PawnsArrivalModeDefOf.CenterDrop));
                        }, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                    }
                    if (worldObjects[i] is Settlement settlement)
                    {
                        if (!settlement.Faction.IsPlayer)
                        {
                            yield return new FloatMenuOption("AttackAndDropAtEdge".Translate(settlement.Label), delegate
                            {
                                this.TryLaunch(tile, new ByakheeArrivalAction_AttackSettlement(settlement, PawnsArrivalModeDefOf.EdgeDrop));
                            }, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                            yield return new FloatMenuOption("AttackAndDropInCenter".Translate(settlement.Label), delegate
                            {
                                this.TryLaunch(tile, new ByakheeArrivalAction_AttackSettlement(settlement, PawnsArrivalModeDefOf.CenterDrop));
                            }, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                        }

                        foreach (FloatMenuOption floatMenuOption in ByakheeArrivalAction_VisitSettlement.GetFloatMenuOptions(this, this.TransportersInGroup.Cast<IThingHolder>(), settlement))
                        {
                            yield return floatMenuOption;
                        }

                        //foreach (FloatMenuOption floatMenuOption in ByakheeArrivalAction_AttackSettlement.GetFloatMenuOptions(this, this.TransportersInGroup.Cast<IThingHolder>(), settlement))
                        //{
                        //    yield return floatMenuOption;
                        //}



                        foreach (FloatMenuOption floatMenuOption in ByakheeArrivalAction_GiveGift.GetFloatMenuOptions(this, this.TransportersInGroup.Cast<IThingHolder>(), settlement))
                        {
                            yield return floatMenuOption;
                        }
                    }
                }
                num = i;
            }
            if (!anything && !Find.World.Impassable(tile))
            {
                yield return new FloatMenuOption("TransportPodsContentsWillBeLost".Translate(), delegate ()
                {
                    this.TryLaunch(tile, null);
                }, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
            }
            yield break;
        }



        private bool ChoseWorldTarget(GlobalTargetInfo target)
        {
            return !this.LoadingInProgressOrReadyToLaunch || CompLaunchablePawn.ChoseWorldTarget(target, this.parent.Map.Tile, this.TransportersInGroup.Cast<IThingHolder>(), this.MaxLaunchDistance, new Action<int, TransportPodsArrivalAction>(this.TryLaunch), this);
        }

        public static bool ChoseWorldTarget(GlobalTargetInfo target, int tile, IEnumerable<IThingHolder> pods, int maxLaunchDistance, Action<int, TransportPodsArrivalAction> launchAction, CompLaunchablePawn launchable)
        {
            if (!target.IsValid)
            {
                Messages.Message("MessageTransportPodsDestinationIsInvalid".Translate(), MessageTypeDefOf.RejectInput, false);
                return false;
            }
            int num = Find.WorldGrid.TraversalDistanceBetween(tile, target.Tile, true, int.MaxValue);
            if (maxLaunchDistance > 0 && num > maxLaunchDistance)
            {
                Messages.Message("TransportPodDestinationBeyondMaximumRange".Translate(), MessageTypeDefOf.RejectInput, false);
                return false;
            }
            IEnumerable<FloatMenuOption> source = (launchable != null) ? launchable.GetTransportPodsFloatMenuOptionsAt(target.Tile) : CompLaunchable.GetOptionsForTile(target.Tile, pods, launchAction);
            if (!source.Any<FloatMenuOption>())
            {
                if (Find.World.Impassable(target.Tile))
                {
                    Messages.Message("MessageTransportPodsDestinationIsInvalid".Translate(), MessageTypeDefOf.RejectInput, false);
                    return false;
                }
                launchAction(target.Tile, null);
                return true;
            }
            else
            {
                if (source.Count<FloatMenuOption>() != 1)
                {
                    Find.WindowStack.Add(new FloatMenu(source.ToList<FloatMenuOption>()));
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


        //private bool ChoseWorldTarget(GlobalTargetInfo target)
        //{
        //    Utility.DebugReport("ChooseWorldTarget Called");
        //    if (!LoadingInProgressOrReadyToLaunch)
        //    {
        //        return true;
        //    }

        //    if (!target.IsValid)
        //    {
        //        Messages.Message("MessageTransportPodsDestinationIsInvalid".Translate(), MessageTypeDefOf.RejectInput);
        //        return false;
        //    }

        //    var num = Find.WorldGrid.TraversalDistanceBetween(parent.Map.Tile, target.Tile);
        //    if (num > MaxLaunchDistance)
        //    {
        //        //Messages.Message("MessageTransportPodsDestinationIsTooFar".Translate(new object[]
        //        //{
        //        //    CompLaunchable.FuelNeededToLaunchAtDist((float)num).ToString("0.#")
        //        //}), MessageTypeDefOf.RejectInput);
        //        return false;
        //    }

        //    if (target.WorldObject is MapParent {HasMap: true} mapParent)
        //    {
        //        var myMap = parent.Map;
        //        var map = mapParent.Map;
        //        Current.Game.CurrentMap = map;
        //        var arg_139_0 = Find.Targeter;

        //        void ActionWhenFinished()
        //        {
        //            if (Find.Maps.Contains(myMap))
        //            {
        //                Current.Game.CurrentMap = myMap;
        //            }
        //        }

        //        arg_139_0.BeginTargeting(TargetingParameters.ForDropPodsDestination(), delegate(LocalTargetInfo x)
        //        {
        //            if (!LoadingInProgressOrReadyToLaunch)
        //            {
        //                Utility.DebugReport("ChooseTarget Exited - LoadingInProgressOrReadyToLaunch");
        //                return;
        //            }

        //            TryLaunch(x.ToGlobalTargetInfo(map), PawnsArrivalModeDefOf.EdgeDrop, false);
        //        }, null, ActionWhenFinished, TargeterMouseAttachment);
        //        return true;
        //    }

        //    if (target.WorldObject is Settlement && target.WorldObject.Faction != Faction.OfPlayer)
        //    {
        //        Find.WorldTargeter.closeWorldTabWhenFinished = false;
        //        var list = new List<FloatMenuOption>();
        //        if (!target.WorldObject.Faction.HostileTo(Faction.OfPlayer))
        //        {
        //            list.Add(new FloatMenuOption("VisitFactionBase".Translate(
        //                target.WorldObject.Label
        //            ), delegate
        //            {
        //                if (!LoadingInProgressOrReadyToLaunch)
        //                {
        //                    return;
        //                }

        //                TryLaunch(target, PawnsArrivalModeDefOf.EdgeDrop, false);
        //                CameraJumper.TryHideWorld();
        //            }));
        //        }

        //        list.Add(new FloatMenuOption("DropAtEdge".Translate(), delegate
        //        {
        //            if (!LoadingInProgressOrReadyToLaunch)
        //            {
        //                return;
        //            }

        //            TryLaunch(target, PawnsArrivalModeDefOf.EdgeDrop, true);
        //            CameraJumper.TryHideWorld();
        //        }));
        //        list.Add(new FloatMenuOption("DropInCenter".Translate(), delegate
        //        {
        //            if (!LoadingInProgressOrReadyToLaunch)
        //            {
        //                return;
        //            }

        //            TryLaunch(target, PawnsArrivalModeDefOf.CenterDrop, true);
        //            CameraJumper.TryHideWorld();
        //        }));
        //        Find.WindowStack.Add(new FloatMenu(list));
        //        return true;
        //    }
        //    Messages.Message("MessageTransportPodsDestinationIsInvalid".Translate(), MessageTypeDefOf.RejectInput);
        //    return false;
        //    //this.TryLaunch(target, PawnsArrivalModeDefOf.Undecided, false);
        //    //return true;
        //}

        public void TryLaunch(int destinationTile, TransportPodsArrivalAction arrivalAction)
        {
            Utility.DebugReport("TryLaunch Called");
            if (!parent.Spawned)
            {
                Log.Error("Tried to launch " + parent + ", but it's unspawned.");
                return;
            }

            var transportersInGroup = TransportersInGroup;
            if (transportersInGroup == null)
            {
                Log.Error("Tried to launch " + parent + ", but it's not in any group.");
                return;
            }

            if (!LoadingInProgressOrReadyToLaunch)
            {
                Utility.DebugReport("TryLaunch Failed");
                return;
            }

            var map = parent.Map;
            var num = Find.WorldGrid.TraversalDistanceBetween(map.Tile, destinationTile);
            if (num > MaxLaunchDistance)
            {
                Utility.DebugReport("TryLaunch Failed #2");
                return;
            }

            Transporter.TryRemoveLord(map);
            var groupID = Transporter.groupID;
            foreach (var compTransporterPawn in transportersInGroup)
            {
                var compTransporter = compTransporterPawn;
                var originalPawn = (Pawn)compTransporter.parent;
                var oldPosition = compTransporter.parent.Position;


                ActiveDropPod activeDropPod = (ActiveDropPod)ThingMaker.MakeThing(ThingDefOf.ActiveDropPod, null);
                activeDropPod.Contents = new ActiveDropPodInfo();
                activeDropPod.Contents.innerContainer.TryAddRangeOrTransfer(compTransporter.GetDirectlyHeldThings(), true, true);
                FlyShipLeaving flyShipLeaving = (FlyShipLeaving)SkyfallerMaker.MakeSkyfaller(PawnFlyerDef.leavingDef ?? ThingDefOf.DropPodLeaving, activeDropPod);
                flyShipLeaving.groupID = groupID;
                flyShipLeaving.destinationTile = destinationTile;
                flyShipLeaving.arrivalAction = arrivalAction;
                flyShipLeaving.worldObjectDef = PawnFlyerDef.travelingDef;
                //flyShipLeaving.worldObjectDef = WorldObjectDefOf.TravelingTransportPods;
                compTransporter.CleanUpLoadingVars(map);
                //compTransporter.parent.Destroy(DestroyMode.Vanish);
                compTransporterPawn.parent.DeSpawn();
                flyShipLeaving.Contents.innerContainer.TryAddOrTransfer(originalPawn);
                GenSpawn.Spawn(flyShipLeaving, oldPosition, map, WipeMode.Vanish);

                //Utility.DebugReport("Transporter Outspawn Attempt");
                //var compTransporter = compTransporterPawn;
                //Utility.DebugReport("Transporter Outspawn " + compTransporter.parent.Label);
                //var pawnFlyerLeaving = (Skyfaller) ThingMaker.MakeThing(PawnFlyerDef.leavingDef);
                //pawnFlyerLeaving.groupID = groupID;
                //pawnFlyerLeaving.pawnFlyer = parent as PawnFlyer;
                //pawnFlyerLeaving.destinationTile = target.Tile;
                //pawnFlyerLeaving.destinationCell = target.Cell;
                //pawnFlyerLeaving.arriveMode = arriveMode;
                //pawnFlyerLeaving.attackOnArrival = attackOnArrival;
                //var innerContainer = compTransporter.GetDirectlyHeldThings();
                //pawnFlyerLeaving.Contents = new ActiveDropPodInfo();
                //innerContainer.TryTransferAllToContainer(pawnFlyerLeaving.Contents.innerContainer);
                ////pawnFlyerLeaving.Contents.innerContainer. //TryAddMany(innerContainer);
                //innerContainer.Clear();
                //compTransporter.CleanUpLoadingVars(map);
                //compTransporter.parent.DeSpawn();
                //pawnFlyerLeaving.Contents.innerContainer.TryAdd(compTransporter.parent);
                //GenSpawn.Spawn(pawnFlyerLeaving, compTransporter.parent.Position, map);
            }
            CameraJumper.TryHideWorld();
        }

        public void Notify_FuelingPortSourceDeSpawned()
        {
            if (Transporter.CancelLoad())
            {
                Messages.Message("MessageTransportersLoadCanceled_FuelingPortGiverDeSpawned".Translate(), parent,
                    MessageTypeDefOf.NegativeEvent);
            }
        }
    }
}