﻿using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using RimWorld;

namespace CultOfCthulhu
{

    [StaticConstructorOnStartup]
    public class CompLaunchablePawn : ThingComp
    {
        private static int maxTileDistance = 120;

        private CompTransporterPawn cachedCompTransporter;

        private static readonly Texture2D TargeterMouseAttachment = ContentFinder<Texture2D>.Get("UI/Overlays/LaunchableMouseAttachment", true);

        private static readonly Texture2D LaunchCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/LaunchShip", true);

        public bool LoadingInProgressOrReadyToLaunch
        {
            get
            {
                return this.Transporter.LoadingInProgressOrReadyToLaunch;
            }
        }

        public bool AnythingLeftToLoad
        {
            get
            {
                return this.Transporter.AnythingLeftToLoad;
            }
        }

        public Thing FirstThingLeftToLoad
        {
            get
            {
                return this.Transporter.FirstThingLeftToLoad;
            }
        }

        public List<CompTransporterPawn> TransportersInGroup
        {
            get
            {
                return this.Transporter.TransportersInGroup(this.parent.Map);
            }
        }

        public bool AnyInGroupHasAnythingLeftToLoad
        {
            get
            {
                return this.Transporter.AnyInGroupHasAnythingLeftToLoad;
            }
        }

        public Thing FirstThingLeftToLoadInGroup
        {
            get
            {
                return this.Transporter.FirstThingLeftToLoadInGroup;
            }
        }

        public bool AnyInGroupIsUnderRoof
        {
            get
            {
                List<CompTransporterPawn> transportersInGroup = this.TransportersInGroup;
                for (int i = 0; i < transportersInGroup.Count; i++)
                {
                    if (transportersInGroup[i].parent.Position.Roofed(this.parent.Map))
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
                if (this.cachedCompTransporter == null)
                {
                    this.cachedCompTransporter = this.parent.GetComp<CompTransporterPawn>();
                }
                return this.cachedCompTransporter;
            }
        }

        public PawnFlyerDef PawnFlyerDef
        {
            get
            {
                PawnFlyerDef result = this.parent.def as PawnFlyerDef;
                if (result == null)
                {
                    Log.Error("PawnFlyerDef is null");
                }
                return result;
            }
        }
        
        public int MaxLaunchDistance
        {
            get
            {
                if (!this.LoadingInProgressOrReadyToLaunch)
                {
                    return 0;
                }
                return PawnFlyerDef.flyableDistance;
            }
        }

        [DebuggerHidden]
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            IEnumerator<Gizmo> enumerator = base.CompGetGizmosExtra().GetEnumerator();
            while (enumerator.MoveNext())
            {
                Gizmo current = enumerator.Current;
                yield return current;
            }
            if (this.LoadingInProgressOrReadyToLaunch)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = "CommandLaunchGroup".Translate();
                command_Action.defaultDesc = "CommandLaunchGroupDesc".Translate();
                command_Action.icon = CompLaunchablePawn.LaunchCommandTex;
                command_Action.action = delegate
                {
                    if (this.AnyInGroupHasAnythingLeftToLoad)
                    {
                        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmSendNotCompletelyLoadedPods".Translate(new object[]
                        {
                            this.FirstThingLeftToLoadInGroup.LabelCap
                        }), new Action(this.StartChoosingDestination), false, null));
                    }
                    else
                    {
                        this.StartChoosingDestination();
                    }
                };
                if (this.AnyInGroupIsUnderRoof)
                {
                    command_Action.Disable("CommandLaunchGroupFailUnderRoof".Translate());
                }
                yield return command_Action;
            }
            else
            {

                    Command_Action command_Action = new Command_Action();
                    command_Action.defaultLabel = "DEBUG";
                    command_Action.defaultDesc = "CommandLaunchGroupDesc".Translate();
                    command_Action.icon = CompLaunchablePawn.LaunchCommandTex;
                    command_Action.action = delegate
                    {
                        if (this.AnyInGroupHasAnythingLeftToLoad)
                        {
                            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmSendNotCompletelyLoadedPods".Translate(new object[]
                            {
                            this.FirstThingLeftToLoadInGroup.LabelCap
                            }), new Action(this.StartChoosingDestination), false, null));
                        }
                        else
                        {
                            this.StartChoosingDestination();
                        }
                    };
                    if (this.AnyInGroupIsUnderRoof)
                    {
                        command_Action.Disable("CommandLaunchGroupFailUnderRoof".Translate());
                    }
                    yield return command_Action;
                
            }
            yield break;
        }

        public override string CompInspectStringExtra()
        {
            if (!this.LoadingInProgressOrReadyToLaunch)
            {
                return null;
            }
            if (this.AnyInGroupHasAnythingLeftToLoad)
            {
                return "NotReadyForLaunch".Translate() + ": " + "TransportPodInGroupHasSomethingLeftToLoad".Translate() + ".";
            }
            return "ReadyForLaunch".Translate();
        }

        public void StartChoosingDestination()
        {
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(this.parent));
            Find.WorldSelector.ClearSelection();
            int tile = this.parent.Map.Tile;
            Find.WorldTargeter.BeginTargeting(new Func<GlobalTargetInfo, bool>(this.ChoseWorldTarget), true, CompLaunchablePawn.TargeterMouseAttachment, true, delegate
            {
                GenDraw.DrawWorldRadiusRing(tile, this.MaxLaunchDistance);
            }, delegate (GlobalTargetInfo target)
            {
                if (!target.IsValid)
                {
                    return null;
                }
                int num = Find.WorldGrid.TraversalDistanceBetween(tile, target.Tile);
                if (num <= this.MaxLaunchDistance)
                {
                    return null;
                }
                if (num > CompLaunchablePawn.maxTileDistance)
                {
                    return "TransportPodDestinationBeyondMaximumRange".Translate();
                }
                return "TransportPodNotEnoughFuel".Translate();
            });
        }

        private bool ChoseWorldTarget(GlobalTargetInfo target)
        {
            Cthulhu.Utility.DebugReport("ChooseWorldTarget Called");
            if (!this.LoadingInProgressOrReadyToLaunch)
            {
                return true;
            }
            if (!target.IsValid)
            {
                Messages.Message("MessageTransportPodsDestinationIsInvalid".Translate(), MessageTypeDefOf.RejectInput);
                return false;
            }
            int num = Find.WorldGrid.TraversalDistanceBetween(this.parent.Map.Tile, target.Tile);
            if (num > this.MaxLaunchDistance)
            {
                //Messages.Message("MessageTransportPodsDestinationIsTooFar".Translate(new object[]
                //{
                //    CompLaunchable.FuelNeededToLaunchAtDist((float)num).ToString("0.#")
                //}), MessageTypeDefOf.RejectInput);
                return false;
            }
            MapParent mapParent = target.WorldObject as MapParent;
            if (mapParent != null && mapParent.HasMap)
            {
                Map myMap = this.parent.Map;
                Map map = mapParent.Map;
                Current.Game.CurrentMap = map;
                Targeter arg_139_0 = Find.Targeter;

                void ActionWhenFinished()
                {
                    if (Find.Maps.Contains(myMap))
                    {
                        Current.Game.CurrentMap = myMap;
                    }
                }

                arg_139_0.BeginTargeting(TargetingParameters.ForDropPodsDestination(), delegate (LocalTargetInfo x)
                {
                    if (!this.LoadingInProgressOrReadyToLaunch)
                    {
                        Cthulhu.Utility.DebugReport("ChooseTarget Exited - LoadingInProgressOrReadyToLaunch");
                        return;
                    }
                    this.TryLaunch(x.ToGlobalTargetInfo(map), PawnsArrivalModeDefOf.EdgeDrop, false);
                }, null, ActionWhenFinished, CompLaunchablePawn.TargeterMouseAttachment);
                return true;
            }
            if (target.WorldObject is FactionBase && target.WorldObject.Faction != Faction.OfPlayer)
            {
                Find.WorldTargeter.closeWorldTabWhenFinished = false;
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                if (!target.WorldObject.Faction.HostileTo(Faction.OfPlayer))
                {
                    list.Add(new FloatMenuOption("VisitFactionBase".Translate(new object[]
                    {
                        target.WorldObject.Label
                    }), delegate
                    {
                        if (!this.LoadingInProgressOrReadyToLaunch)
                        {
                            return;
                        }
                        this.TryLaunch(target, PawnsArrivalModeDefOf.EdgeDrop, false);
                        CameraJumper.TryHideWorld();
                    }, MenuOptionPriority.Default, null, null, 0f, null, null));
                }
                list.Add(new FloatMenuOption("DropAtEdge".Translate(), delegate
                {
                    if (!this.LoadingInProgressOrReadyToLaunch)
                    {
                        return;
                    }
                    this.TryLaunch(target, PawnsArrivalModeDefOf.EdgeDrop, true);
                    CameraJumper.TryHideWorld();
                }, MenuOptionPriority.Default, null, null, 0f, null, null));
                list.Add(new FloatMenuOption("DropInCenter".Translate(), delegate
                {
                    if (!this.LoadingInProgressOrReadyToLaunch)
                    {
                        return;
                    }
                    this.TryLaunch(target, PawnsArrivalModeDefOf.CenterDrop, true);
                    CameraJumper.TryHideWorld();
                }, MenuOptionPriority.Default, null, null, 0f, null, null));
                Find.WindowStack.Add(new FloatMenu(list));
                return true;
            }
            else // if (Find.World.Impassable(target.Tile))
            {
                Messages.Message("MessageTransportPodsDestinationIsInvalid".Translate(), MessageTypeDefOf.RejectInput);
                return false;
            }
            //this.TryLaunch(target, PawnsArrivalModeDefOf.Undecided, false);
            //return true;
        }

        private void TryLaunch(GlobalTargetInfo target, PawnsArrivalModeDef arriveMode, bool attackOnArrival)
        {

            Cthulhu.Utility.DebugReport("TryLaunch Called");
            if (!this.parent.Spawned)
            {
                Log.Error("Tried to launch " + this.parent + ", but it's unspawned.");
                return;
            }
            List<CompTransporterPawn> transportersInGroup = this.TransportersInGroup;
            if (transportersInGroup == null)
            {
                Log.Error("Tried to launch " + this.parent + ", but it's not in any group.");
                return;
            }
            if (!this.LoadingInProgressOrReadyToLaunch)
            {
                Cthulhu.Utility.DebugReport("TryLaunch Failed");
                return;
            }
            Map map = this.parent.Map;
            int num = Find.WorldGrid.TraversalDistanceBetween(map.Tile, target.Tile);
            if (num > this.MaxLaunchDistance)
            {
                Cthulhu.Utility.DebugReport("TryLaunch Failed #2");
                return;
            }
            this.Transporter.TryRemoveLord(map);
            int groupID = this.Transporter.groupID;
            for (int i = 0; i < transportersInGroup.Count; i++)
            {
                Cthulhu.Utility.DebugReport("Transporter Outspawn Attempt");
                CompTransporterPawn compTransporter = transportersInGroup[i];
                Cthulhu.Utility.DebugReport("Transporter Outspawn " + compTransporter.parent.Label);
                PawnFlyersLeaving pawnFlyerLeaving = (PawnFlyersLeaving)ThingMaker.MakeThing(PawnFlyerDef.leavingDef, null);
                pawnFlyerLeaving.groupID = groupID;
                pawnFlyerLeaving.pawnFlyer = this.parent as PawnFlyer;
                pawnFlyerLeaving.destinationTile = target.Tile;
                pawnFlyerLeaving.destinationCell = target.Cell;
                pawnFlyerLeaving.arriveMode = arriveMode;
                pawnFlyerLeaving.attackOnArrival = attackOnArrival;
                ThingOwner innerContainer = compTransporter.GetDirectlyHeldThings();
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
            if (this.Transporter.CancelLoad())
            {
                Messages.Message("MessageTransportersLoadCanceled_FuelingPortGiverDeSpawned".Translate(), this.parent, MessageTypeDefOf.NegativeEvent);
            }
        }
    }
}
