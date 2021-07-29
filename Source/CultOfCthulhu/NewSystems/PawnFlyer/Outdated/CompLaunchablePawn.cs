using System.Collections.Generic;
using System.Diagnostics;
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
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(parent));
            Find.WorldSelector.ClearSelection();
            var tile = parent.Map.Tile;
            Find.WorldTargeter.BeginTargeting(ChoseWorldTarget, true, TargeterMouseAttachment, true,
                delegate { GenDraw.DrawWorldRadiusRing(tile, MaxLaunchDistance); }, delegate(GlobalTargetInfo target)
                {
                    if (!target.IsValid)
                    {
                        return null;
                    }

                    var num = Find.WorldGrid.TraversalDistanceBetween(tile, target.Tile);
                    if (num <= MaxLaunchDistance)
                    {
                        return null;
                    }

                    return num > maxTileDistance
                        ? (string) "TransportPodDestinationBeyondMaximumRange".Translate()
                        : (string) "TransportPodNotEnoughFuel".Translate();
                });
        }

        private bool ChoseWorldTarget(GlobalTargetInfo target)
        {
            Utility.DebugReport("ChooseWorldTarget Called");
            if (!LoadingInProgressOrReadyToLaunch)
            {
                return true;
            }

            if (!target.IsValid)
            {
                Messages.Message("MessageTransportPodsDestinationIsInvalid".Translate(), MessageTypeDefOf.RejectInput);
                return false;
            }

            var num = Find.WorldGrid.TraversalDistanceBetween(parent.Map.Tile, target.Tile);
            if (num > MaxLaunchDistance)
            {
                //Messages.Message("MessageTransportPodsDestinationIsTooFar".Translate(new object[]
                //{
                //    CompLaunchable.FuelNeededToLaunchAtDist((float)num).ToString("0.#")
                //}), MessageTypeDefOf.RejectInput);
                return false;
            }

            if (target.WorldObject is MapParent {HasMap: true} mapParent)
            {
                var myMap = parent.Map;
                var map = mapParent.Map;
                Current.Game.CurrentMap = map;
                var arg_139_0 = Find.Targeter;

                void ActionWhenFinished()
                {
                    if (Find.Maps.Contains(myMap))
                    {
                        Current.Game.CurrentMap = myMap;
                    }
                }

                arg_139_0.BeginTargeting(TargetingParameters.ForDropPodsDestination(), delegate(LocalTargetInfo x)
                {
                    if (!LoadingInProgressOrReadyToLaunch)
                    {
                        Utility.DebugReport("ChooseTarget Exited - LoadingInProgressOrReadyToLaunch");
                        return;
                    }

                    TryLaunch(x.ToGlobalTargetInfo(map), PawnsArrivalModeDefOf.EdgeDrop, false);
                }, null, ActionWhenFinished, TargeterMouseAttachment);
                return true;
            }

            if (target.WorldObject is Settlement && target.WorldObject.Faction != Faction.OfPlayer)
            {
                Find.WorldTargeter.closeWorldTabWhenFinished = false;
                var list = new List<FloatMenuOption>();
                if (!target.WorldObject.Faction.HostileTo(Faction.OfPlayer))
                {
                    list.Add(new FloatMenuOption("VisitFactionBase".Translate(
                        target.WorldObject.Label
                    ), delegate
                    {
                        if (!LoadingInProgressOrReadyToLaunch)
                        {
                            return;
                        }

                        TryLaunch(target, PawnsArrivalModeDefOf.EdgeDrop, false);
                        CameraJumper.TryHideWorld();
                    }));
                }

                list.Add(new FloatMenuOption("DropAtEdge".Translate(), delegate
                {
                    if (!LoadingInProgressOrReadyToLaunch)
                    {
                        return;
                    }

                    TryLaunch(target, PawnsArrivalModeDefOf.EdgeDrop, true);
                    CameraJumper.TryHideWorld();
                }));
                list.Add(new FloatMenuOption("DropInCenter".Translate(), delegate
                {
                    if (!LoadingInProgressOrReadyToLaunch)
                    {
                        return;
                    }

                    TryLaunch(target, PawnsArrivalModeDefOf.CenterDrop, true);
                    CameraJumper.TryHideWorld();
                }));
                Find.WindowStack.Add(new FloatMenu(list));
                return true;
            }

            Messages.Message("MessageTransportPodsDestinationIsInvalid".Translate(), MessageTypeDefOf.RejectInput);
            return false;
            //this.TryLaunch(target, PawnsArrivalModeDefOf.Undecided, false);
            //return true;
        }

        private void TryLaunch(GlobalTargetInfo target, PawnsArrivalModeDef arriveMode, bool attackOnArrival)
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
            var num = Find.WorldGrid.TraversalDistanceBetween(map.Tile, target.Tile);
            if (num > MaxLaunchDistance)
            {
                Utility.DebugReport("TryLaunch Failed #2");
                return;
            }

            Transporter.TryRemoveLord(map);
            var groupID = Transporter.groupID;
            foreach (var compTransporterPawn in transportersInGroup)
            {
                Utility.DebugReport("Transporter Outspawn Attempt");
                var compTransporter = compTransporterPawn;
                Utility.DebugReport("Transporter Outspawn " + compTransporter.parent.Label);
                var pawnFlyerLeaving = (PawnFlyersLeaving) ThingMaker.MakeThing(PawnFlyerDef.leavingDef);
                pawnFlyerLeaving.groupID = groupID;
                pawnFlyerLeaving.pawnFlyer = parent as PawnFlyer;
                pawnFlyerLeaving.destinationTile = target.Tile;
                pawnFlyerLeaving.destinationCell = target.Cell;
                pawnFlyerLeaving.arriveMode = arriveMode;
                pawnFlyerLeaving.attackOnArrival = attackOnArrival;
                var innerContainer = compTransporter.GetDirectlyHeldThings();
                pawnFlyerLeaving.Contents = new ActiveDropPodInfo();
                innerContainer.TryTransferAllToContainer(pawnFlyerLeaving.Contents.innerContainer);
                //pawnFlyerLeaving.Contents.innerContainer. //TryAddMany(innerContainer);
                innerContainer.Clear();
                compTransporter.CleanUpLoadingVars(map);
                compTransporter.parent.DeSpawn();
                pawnFlyerLeaving.Contents.innerContainer.TryAdd(compTransporter.parent);
                GenSpawn.Spawn(pawnFlyerLeaving, compTransporter.parent.Position, map);
            }
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