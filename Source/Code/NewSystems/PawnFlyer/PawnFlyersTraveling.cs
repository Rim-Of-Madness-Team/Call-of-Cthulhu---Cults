using System.Collections.Generic;
using Cthulhu;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace CultOfCthulhu
{
    public class PawnFlyersTraveling : WorldObject
    {
        private static readonly List<Pawn> tmpPawns = new List<Pawn>();

        private bool arrived;

        public PawnsArrivalModeDef arriveMode;

        public bool attackOnArrival;

        public IntVec3 destinationCell = IntVec3.Invalid;


        public int destinationTile = -1;

        private int initialTile = -1;
        public PawnFlyer pawnFlyer;

        private List<ActiveDropPodInfo> pods = new List<ActiveDropPodInfo>();

        private float traveledPct;

        private float TravelSpeed => PawnFlyerDef.flightSpeed;

        private PawnFlyerDef PawnFlyerDef => pawnFlyer.def as PawnFlyerDef;

        private Vector3 Start => Find.WorldGrid.GetTileCenter(tileID: initialTile);

        private Vector3 End => Find.WorldGrid.GetTileCenter(tileID: destinationTile);

        public override Vector3 DrawPos => Vector3.Slerp(a: Start, b: End, t: traveledPct);

        private float TraveledPctStepPerTick
        {
            get
            {
                var start = Start;
                var end = End;
                if (start == end)
                {
                    return 1f;
                }

                var num = GenMath.SphericalDistance(normalizedA: start.normalized, normalizedB: end.normalized);
                return num == 0f ? 1f : 0.00025f / num;
            }
        }

        //There is always the byakhee
        private bool PodsHaveAnyPotentialCaravanOwner => true;

        public bool PodsHaveAnyFreeColonist
        {
            get
            {
                foreach (var activeDropPodInfo in pods)
                {
                    var innerContainer = activeDropPodInfo.innerContainer;
                    foreach (var thing in innerContainer)
                    {
                        if (thing is Pawn {IsColonist: true, HostFaction: null})
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public IEnumerable<Pawn> Pawns
        {
            get
            {
                foreach (var activeDropPodInfo in pods)
                {
                    var innerContainer = activeDropPodInfo.innerContainer;
                    foreach (var thing in innerContainer)
                    {
                        if (thing is Pawn pawn)
                        {
                            yield return pawn;
                        }
                    }
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            //Pawn
            Scribe_References.Look(refee: ref pawnFlyer, label: "pawnFlyer");

            //Vanilla
            Scribe_Collections.Look(list: ref pods, label: "pods", lookMode: LookMode.Deep);
            Scribe_Values.Look(value: ref destinationTile, label: "destinationTile");
            Scribe_Values.Look(value: ref destinationCell, label: "destinationCell");
            Scribe_Values.Look(value: ref arriveMode, label: "arriveMode", defaultValue: PawnsArrivalModeDefOf.EdgeDrop);
            Scribe_Values.Look(value: ref attackOnArrival, label: "attackOnArrival");
            Scribe_Values.Look(value: ref arrived, label: "arrived");
            Scribe_Values.Look(value: ref initialTile, label: "initialTile");
            Scribe_Values.Look(value: ref traveledPct, label: "traveledPct");
        }

        public override void PostAdd()
        {
            base.PostAdd();
            initialTile = Tile;
        }

        public override void Tick()
        {
            base.Tick();
            traveledPct += TraveledPctStepPerTick;
            if (!(traveledPct >= 1f))
            {
                return;
            }

            traveledPct = 1f;
            Arrived();
        }

        public void AddPod(ActiveDropPodInfo contents, bool justLeftTheMap)
        {
            contents.parent = null;
            pods.Add(item: contents);
            var innerContainer = contents.innerContainer;
            foreach (var thing in innerContainer)
            {
                if (thing is Pawn pawn && !pawn.IsWorldPawn())
                {
                    if (!Spawned)
                    {
                        Log.Warning(text: "Passing pawn " + pawn +
                                          " to world, but the TravelingTransportPod is not spawned. This means that WorldPawns can discard this pawn which can cause bugs.");
                    }

                    if (justLeftTheMap)
                    {
                        pawn.ExitMap(allowedToJoinOrCreateCaravan: false, exitDir: Rot4.Random);
                    }
                    else
                    {
                        Find.WorldPawns.PassToWorld(pawn: pawn);
                    }
                }

                if (thing is not PawnFlyer flyer || flyer.IsWorldPawn())
                {
                    continue;
                }

                if (!Spawned)
                {
                    Log.Warning(text: "Passing pawn " + flyer +
                                      " to world, but the TravelingTransportPod is not spawned. This means that WorldPawns can discard this pawn which can cause bugs.");
                }

                if (justLeftTheMap)
                {
                    flyer.ExitMap(allowedToJoinOrCreateCaravan: false, exitDir: Rot4.Random);
                }
                else
                {
                    Find.WorldPawns.PassToWorld(pawn: flyer);
                }
            }

            contents.savePawnsWithReferenceMode = true;
        }

        public bool ContainsPawn(Pawn p)
        {
            foreach (var activeDropPodInfo in pods)
            {
                if (activeDropPodInfo.innerContainer.Contains(item: p))
                {
                    return true;
                }
            }

            return false;
        }

        public bool ContainsPawnFlyer(PawnFlyer p)
        {
            foreach (var activeDropPodInfo in pods)
            {
                if (activeDropPodInfo.innerContainer.Contains(item: p))
                {
                    return true;
                }
            }

            return false;
        }

        private void Arrived()
        {
            Utility.DebugReport(x: "Arrived");
            if (arrived)
            {
                return;
            }

            arrived = true;
            var map = Current.Game.FindMap(tile: destinationTile);
            if (map != null)
            {
                SpawnDropPodsInMap(map: map);
            }
            else if (!PodsHaveAnyPotentialCaravanOwner)
            {
                var caravan = Find.WorldObjects.PlayerControlledCaravanAt(tile: destinationTile);
                if (caravan != null)
                {
                    GivePodContentsToCaravan(caravan: caravan);
                }
                else
                {
                    foreach (var activeDropPodInfo in pods)
                    {
                        activeDropPodInfo.innerContainer.ClearAndDestroyContentsOrPassToWorld();
                    }

                    RemoveAllPods();
                    Find.WorldObjects.Remove(o: this);
                    Messages.Message(text: "MessageTransportPodsArrivedAndLost".Translate(),
                        lookTargets: new GlobalTargetInfo(tile: destinationTile), def: MessageTypeDefOf.NegativeEvent);
                }
            }
            else
            {
                var mapParent = Find.WorldObjects.MapParentAt(tile: destinationTile);
                if (mapParent != null && attackOnArrival)
                {
                    LongEventHandler.QueueLongEvent(action: delegate
                    {
                        var unused = GetOrGenerateMapUtility.GetOrGenerateMap(tile: mapParent.Tile, suggestedMapParentDef: null);
                        string extraMessagePart = null;
                        if (!mapParent.Faction.HostileTo(other: Faction.OfPlayer))
                        {
                            mapParent.Faction.SetRelationDirect(other: Faction.OfPlayer, kind: FactionRelationKind.Hostile, canSendHostilityLetter: false);
                            //mapParent.Faction.SetHostileTo(Faction.OfPlayer, true);
                            extraMessagePart = "MessageTransportPodsArrived_BecameHostile".Translate(
                                arg1: mapParent.Faction.Name
                            ).CapitalizeFirst();
                        }

                        Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
                        SpawnDropPodsInMap(map: mapParent.Map, extraMessagePart: extraMessagePart);
                    }, textKey: "GeneratingMapForNewEncounter", doAsynchronously: false, exceptionHandler: null);
                }
                else
                {
                    SpawnCaravanAtDestinationTile();
                }
            }
        }

        private void SpawnDropPodsInMap(Map map, string extraMessagePart = null)
        {
            Utility.DebugReport(x: "SpawnDropPodsInMap Called");
            RemoveAllPawnsFromWorldPawns();
            IntVec3 intVec;
            if (destinationCell.IsValid && destinationCell.InBounds(map: map))
            {
                intVec = destinationCell;
            }
            else if (arriveMode == PawnsArrivalModeDefOf.CenterDrop)
            {
                if (!DropCellFinder.TryFindRaidDropCenterClose(spot: out intVec, map: map))
                {
                    intVec = DropCellFinder.FindRaidDropCenterDistant(map: map);
                }
            }
            else
            {
                if (arriveMode != PawnsArrivalModeDefOf.EdgeDrop && arriveMode != PawnsArrivalModeDefOf.EdgeDrop)
                {
                    Log.Warning(text: "Unsupported arrive mode " + arriveMode);
                }

                intVec = DropCellFinder.FindRaidDropCenterDistant(map: map);
            }

            foreach (var activeDropPodInfo in pods)
            {
                Utility.DebugReport(x: "PawnFlyerIncoming Generation Started");
                DropCellFinder.TryFindDropSpotNear(center: intVec, map: map, result: out var c, allowFogged: false, canRoofPunch: true);
                var pawnFlyerIncoming =
                    (PawnFlyersIncoming) ThingMaker.MakeThing(def: PawnFlyerDef.incomingDef);
                pawnFlyerIncoming.pawnFlyer = pawnFlyer;
                pawnFlyerIncoming.Contents = activeDropPodInfo;
                GenSpawn.Spawn(newThing: pawnFlyerIncoming, loc: c, map: map);
            }

            RemoveAllPods();
            Find.WorldObjects.Remove(o: this);
            string text = "MessageTransportPodsArrived".Translate();
            if (extraMessagePart != null)
            {
                text = text + " " + extraMessagePart;
            }

            Messages.Message(text: text, lookTargets: new TargetInfo(cell: intVec, map: map), def: MessageTypeDefOf.PositiveEvent);
        }

        private void GivePodContentsToCaravan(Caravan caravan)
        {
            foreach (var activeDropPodInfo in pods)
            {
                var tmpContainedThings = new List<Thing>();
                //PawnFlyersTraveling.tmpContainedThing.Clear();

                tmpContainedThings.AddRange(collection: activeDropPodInfo.innerContainer);
                //this.pods[i].innerContainer.
                foreach (var thing in tmpContainedThings)
                {
                    activeDropPodInfo.innerContainer.Remove(item: thing);
                    thing.holdingOwner = null;
                    if (thing is Pawn pawn)
                    {
                        caravan.AddPawn(p: pawn, addCarriedPawnToWorldPawnsIfAny: true);
                    }
                    else
                    {
                        var pawn2 = CaravanInventoryUtility.FindPawnToMoveInventoryTo(item: thing,
                            candidates: caravan.PawnsListForReading, ignoreCandidates: null);
                        var flag = false;
                        if (pawn2 != null)
                        {
                            flag = pawn2.inventory.innerContainer.TryAdd(item: thing);
                        }

                        if (!flag)
                        {
                            thing.Destroy();
                        }
                    }
                }
            }

            RemoveAllPods();
            Find.WorldObjects.Remove(o: this);
            Messages.Message(text: "MessageTransportPodsArrivedAndAddedToCaravan".Translate(), lookTargets: caravan,
                def: MessageTypeDefOf.PositiveEvent);
        }


        private void SpawnCaravanAtDestinationTile()
        {
            tmpPawns.Clear();
            foreach (var activeDropPodInfo in pods)
            {
                var innerContainer = activeDropPodInfo.innerContainer;
                foreach (var thing in innerContainer)
                {
                    if (thing is Pawn pawn)
                    {
                        tmpPawns.Add(item: pawn);
                    }
                }
            }

            if (!GenWorldClosest.TryFindClosestPassableTile(rootTile: destinationTile, foundTile: out var startingTile))
            {
                startingTile = destinationTile;
            }

            var o = CaravanMaker.MakeCaravan(pawns: tmpPawns, faction: Faction.OfPlayer, startingTile: startingTile, addToWorldPawnsIfNotAlready: true);
            o.AddPawn(p: pawnFlyer, addCarriedPawnToWorldPawnsIfAny: false);
            foreach (var activeDropPodInfo in pods)
            {
                var innerContainer2 = activeDropPodInfo.innerContainer;
                foreach (var thing in innerContainer2)
                {
                    if (!(thing is Pawn))
                    {
                        var pawn2 = CaravanInventoryUtility.FindPawnToMoveInventoryTo(item: thing,
                            candidates: tmpPawns, ignoreCandidates: null);
                        pawn2.inventory.innerContainer.TryAdd(item: thing);
                    }
                    else
                    {
                        var pawn3 = thing as Pawn;
                        if (pawn3.IsPrisoner)
                        {
                            continue;
                        }

                        if (pawn3.Faction != pawnFlyer.Faction)
                        {
                            pawn3.SetFaction(newFaction: pawnFlyer.Faction);
                        }
                    }
                }
            }

            RemoveAllPods();
            Find.WorldObjects.Remove(o: this);
            Messages.Message(text: "MessageTransportPodsArrived".Translate(), lookTargets: o, def: MessageTypeDefOf.PositiveEvent);
        }

        private void RemoveAllPawnsFromWorldPawns()
        {
            foreach (var activeDropPodInfo in pods)
            {
                var innerContainer = activeDropPodInfo.innerContainer;
                foreach (var thing in innerContainer)
                {
                    var pawn = thing as Pawn;
                    Pawn flyer = thing as PawnFlyer;
                    if (pawn != null && pawn.IsWorldPawn())
                    {
                        Find.WorldPawns.RemovePawn(p: pawn);
                    }
                    else if (flyer != null && pawn.IsWorldPawn())
                    {
                        Find.WorldPawns.RemovePawn(p: flyer);
                    }
                }
            }
        }

        private void RemoveAllPods()
        {
            foreach (var activeDropPodInfo in pods)
            {
                activeDropPodInfo.savePawnsWithReferenceMode = false;
            }

            pods.Clear();
        }
    }
}