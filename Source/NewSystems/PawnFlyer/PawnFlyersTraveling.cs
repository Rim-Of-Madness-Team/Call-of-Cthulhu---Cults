using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using RimWorld.Planet;

namespace CultOfCthulhu
{
    public class PawnFlyersTraveling : WorldObject
    {
        public PawnFlyer pawnFlyer;
        
        private float TravelSpeed
        {
            get
            {
                return PawnFlyerDef.flightSpeed;
            }
        }

        private PawnFlyerDef PawnFlyerDef
        {
            get
            {
                return pawnFlyer.def as PawnFlyerDef;
            }
        }


        public int destinationTile = -1;

        public IntVec3 destinationCell = IntVec3.Invalid;

        public PawnsArriveMode arriveMode;

        public bool attackOnArrival;

        private List<ActiveDropPodInfo> pods = new List<ActiveDropPodInfo>();

        private bool arrived;

        private int initialTile = -1;

        private float traveledPct;

        private static List<Pawn> tmpPawns = new List<Pawn>();

        private Vector3 Start
        {
            get
            {
                return Find.WorldGrid.GetTileCenter(this.initialTile);
            }
        }

        private Vector3 End
        {
            get
            {
                return Find.WorldGrid.GetTileCenter(this.destinationTile);
            }
        }

        public override Vector3 DrawPos
        {
            get
            {
                return Vector3.Slerp(this.Start, this.End, this.traveledPct);
            }
        }

        private float TraveledPctStepPerTick
        {
            get
            {
                Vector3 start = this.Start;
                Vector3 end = this.End;
                if (start == end)
                {
                    return 1f;
                }
                float num = GenMath.SphericalDistance(start.normalized, end.normalized);
                if (num == 0f)
                {
                    return 1f;
                }
                return 0.00025f / num;
            }
        }

        //There is always the byakhee
        private bool PodsHaveAnyPotentialCaravanOwner
        {
            get
            {
                return true;
            }
        }

        public bool PodsHaveAnyFreeColonist
        {
            get
            {
                for (int i = 0; i < this.pods.Count; i++)
                {
                    ThingOwner innerContainer = this.pods[i].innerContainer;
                    for (int j = 0; j < innerContainer.Count; j++)
                    {
                        Pawn pawn = innerContainer[j] as Pawn;
                        if (pawn != null && pawn.IsColonist && pawn.HostFaction == null)
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
                for (int i = 0; i < this.pods.Count; i++)
                {
                    ThingOwner innerContainer = this.pods[i].innerContainer;
                    for (int j = 0; j < innerContainer.Count; j++)
                    {
                        Pawn pawn = innerContainer[j] as Pawn;
                        PawnFlyer pawnFlyer = innerContainer[j] as PawnFlyer;
                        if (pawn != null)
                        {
                            yield return pawn;
                        }
                        else if (pawnFlyer != null)
                        {
                            yield return (Pawn)pawnFlyer;
                        }
                    }
                }
                yield break;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            //Pawn
            Scribe_References.Look<PawnFlyer>(ref this.pawnFlyer, "pawnFlyer");

            //Vanilla
            Scribe_Collections.Look<ActiveDropPodInfo>(ref this.pods, "pods", LookMode.Deep, new object[0]);
            Scribe_Values.Look<int>(ref this.destinationTile, "destinationTile", 0, false);
            Scribe_Values.Look<IntVec3>(ref this.destinationCell, "destinationCell", default(IntVec3), false);
            Scribe_Values.Look<PawnsArriveMode>(ref this.arriveMode, "arriveMode", PawnsArriveMode.Undecided, false);
            Scribe_Values.Look<bool>(ref this.attackOnArrival, "attackOnArrival", false, false);
            Scribe_Values.Look<bool>(ref this.arrived, "arrived", false, false);
            Scribe_Values.Look<int>(ref this.initialTile, "initialTile", 0, false);
            Scribe_Values.Look<float>(ref this.traveledPct, "traveledPct", 0f, false);
        }

        public override void PostAdd()
        {
            base.PostAdd();
            this.initialTile = base.Tile;
        }

        public override void Tick()
        {
            base.Tick();
            this.traveledPct += this.TraveledPctStepPerTick;
            if (this.traveledPct >= 1f)
            {
                this.traveledPct = 1f;
                this.Arrived();
            }
        }

        public void AddPod(ActiveDropPodInfo contents, bool justLeftTheMap)
        {
            contents.parent = null;
            this.pods.Add(contents);
            ThingOwner innerContainer = contents.innerContainer;
            for (int i = 0; i < innerContainer.Count; i++)
            {
                Pawn pawn = innerContainer[i] as Pawn;
                PawnFlyer pawnFlyer = innerContainer[i] as PawnFlyer;
                if (pawn != null && !pawn.IsWorldPawn())
                {
                    if (!base.Spawned)
                    {
                        Log.Warning("Passing pawn " + pawn + " to world, but the TravelingTransportPod is not spawned. This means that WorldPawns can discard this pawn which can cause bugs.");
                    }
                    if (justLeftTheMap)
                    {
                        pawn.ExitMap(false);
                    }
                    else
                    {
                        Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
                    }
                }
                if (pawnFlyer != null && !pawnFlyer.IsWorldPawn())
                {
                    if (!base.Spawned)
                    {
                        Log.Warning("Passing pawn " + pawnFlyer + " to world, but the TravelingTransportPod is not spawned. This means that WorldPawns can discard this pawn which can cause bugs.");
                    }
                    if (justLeftTheMap)
                    {
                        pawnFlyer.ExitMap(false);
                    }
                    else
                    {
                        Find.WorldPawns.PassToWorld(pawnFlyer, PawnDiscardDecideMode.Decide);
                    }
                }

            }
            contents.savePawnsWithReferenceMode = true;
        }

        public bool ContainsPawn(Pawn p)
        {
            for (int i = 0; i < this.pods.Count; i++)
            {
                if (this.pods[i].innerContainer.Contains(p))
                {
                    return true;
                }
            }
            return false;
        }

        public bool ContainsPawnFlyer(PawnFlyer p)
        {
            for (int i = 0; i < this.pods.Count; i++)
            {
                if (this.pods[i].innerContainer.Contains(p))
                {
                    return true;
                }
            }
            return false;
        }

        private void Arrived()
        {
            Cthulhu.Utility.DebugReport("Arrived");
            if (this.arrived)
            {
                return;
            }
            this.arrived = true;
            Map map = Current.Game.FindMap(this.destinationTile);
            if (map != null)
            {

                this.SpawnDropPodsInMap(map, null);
            }
            else if (!this.PodsHaveAnyPotentialCaravanOwner)
            {
                Caravan caravan = Find.WorldObjects.PlayerControlledCaravanAt(this.destinationTile);
                if (caravan != null)
                {
                    this.GivePodContentsToCaravan(caravan);
                }
                else
                {
                    for (int i = 0; i < this.pods.Count; i++)
                    {
                        this.pods[i].innerContainer.ClearAndDestroyContentsOrPassToWorld(DestroyMode.Vanish);
                    }
                    this.RemoveAllPods();
                    Find.WorldObjects.Remove(this);
                    Messages.Message("MessageTransportPodsArrivedAndLost".Translate(), new GlobalTargetInfo(this.destinationTile), MessageTypeDefOf.NegativeEvent);
                }
            }
            else
            {
                MapParent mapParent = Find.WorldObjects.MapParentAt(this.destinationTile);
                if (mapParent != null && mapParent.TransportPodsCanLandAndGenerateMap && this.attackOnArrival)
                {
                    LongEventHandler.QueueLongEvent(delegate
                    {
                        Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(mapParent.Tile, null);
                        string extraMessagePart = null;
                        if (!mapParent.Faction.HostileTo(Faction.OfPlayer))
                        {
                            mapParent.Faction.SetHostileTo(Faction.OfPlayer, true);
                            extraMessagePart = "MessageTransportPodsArrived_BecameHostile".Translate(new object[]
                            {
                                mapParent.Faction.Name
                            }).CapitalizeFirst();
                        }
                        Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
                        this.SpawnDropPodsInMap(mapParent.Map, extraMessagePart);
                    }, "GeneratingMapForNewEncounter", false, null);
                }
                else
                {
                    this.SpawnCaravanAtDestinationTile();
                }
            }
        }

        private void SpawnDropPodsInMap(Map map, string extraMessagePart = null)
        {
            Cthulhu.Utility.DebugReport("SpawnDropPodsInMap Called");
            this.RemoveAllPawnsFromWorldPawns();
            IntVec3 intVec;
            if (this.destinationCell.IsValid && this.destinationCell.InBounds(map))
            {
                intVec = this.destinationCell;
            }
            else if (this.arriveMode == PawnsArriveMode.CenterDrop)
            {
                if (!DropCellFinder.TryFindRaidDropCenterClose(out intVec, map))
                {
                    intVec = DropCellFinder.FindRaidDropCenterDistant(map);
                }
            }
            else
            {
                if (this.arriveMode != PawnsArriveMode.EdgeDrop && this.arriveMode != PawnsArriveMode.Undecided)
                {
                    Log.Warning("Unsupported arrive mode " + this.arriveMode);
                }
                intVec = DropCellFinder.FindRaidDropCenterDistant(map);
            }
            for (int i = 0; i < this.pods.Count; i++)
            {
                Cthulhu.Utility.DebugReport("PawnFlyerIncoming Generation Started");
                IntVec3 c;
                DropCellFinder.TryFindDropSpotNear(intVec, map, out c, false, true);
                PawnFlyersIncoming pawnFlyerIncoming = (PawnFlyersIncoming)ThingMaker.MakeThing(PawnFlyerDef.incomingDef, null);
                pawnFlyerIncoming.pawnFlyer = this.pawnFlyer;
                pawnFlyerIncoming.Contents = this.pods[i];
                GenSpawn.Spawn(pawnFlyerIncoming, c, map);
            }
            this.RemoveAllPods();
            Find.WorldObjects.Remove(this);
            string text = "MessageTransportPodsArrived".Translate();
            if (extraMessagePart != null)
            {
                text = text + " " + extraMessagePart;
            }
            Messages.Message(text, new TargetInfo(intVec, map, false), MessageTypeDefOf.PositiveEvent);
        }

        private void GivePodContentsToCaravan(Caravan caravan)
        {
            for (int i = 0; i < this.pods.Count; i++)
            {
                List<Thing> tmpContainedThings = new List<Thing>();
                //PawnFlyersTraveling.tmpContainedThing.Clear();

                tmpContainedThings.AddRange(this.pods[i].innerContainer);
                //this.pods[i].innerContainer.
                for (int j = 0; j < tmpContainedThings.Count; j++)
                {
                    this.pods[i].innerContainer.Remove(tmpContainedThings[j]);
                    tmpContainedThings[j].holdingOwner = null;
                    Pawn pawn = tmpContainedThings[j] as Pawn;
                    PawnFlyer pawnFlyer = tmpContainedThings[j] as PawnFlyer;
                    if (pawn != null)
                    {
                        caravan.AddPawn(pawn, true);
                    }
                    else if (pawnFlyer != null)
                    {
                        caravan.AddPawn(pawnFlyer, true);
                    }
                    else
                    {
                        Pawn pawn2 = CaravanInventoryUtility.FindPawnToMoveInventoryTo(tmpContainedThings[j], caravan.PawnsListForReading, null, null);
                        bool flag = false;
                        if (pawn2 != null)
                        {
                            flag = pawn2.inventory.innerContainer.TryAdd(tmpContainedThings[j], true);
                        }
                        if (!flag)
                        {
                            tmpContainedThings[j].Destroy(DestroyMode.Vanish);
                        }
                    }
                }
            }
            this.RemoveAllPods();
            Find.WorldObjects.Remove(this);
            Messages.Message("MessageTransportPodsArrivedAndAddedToCaravan".Translate(), caravan, MessageTypeDefOf.PositiveEvent);
        }


        private void SpawnCaravanAtDestinationTile()
        {
            PawnFlyersTraveling.tmpPawns.Clear();
            for (int i = 0; i < this.pods.Count; i++)
            {
                ThingOwner innerContainer = this.pods[i].innerContainer;
                for (int j = 0; j < innerContainer.Count; j++)
                {
                    Pawn pawn = innerContainer[j] as Pawn;
                    PawnFlyer pawnFlyer = innerContainer[j] as PawnFlyer;
                    if (pawn != null)
                    {
                        PawnFlyersTraveling.tmpPawns.Add(pawn);
                    }
                    else if (pawnFlyer != null)
                    {
                        PawnFlyersTraveling.tmpPawns.Add((Pawn)pawnFlyer);
                    }
                }
            }
            int startingTile;
            if (!GenWorldClosest.TryFindClosestPassableTile(this.destinationTile, out startingTile))
            {
                startingTile = this.destinationTile;
            }
            Caravan o = CaravanMaker.MakeCaravan(PawnFlyersTraveling.tmpPawns, Faction.OfPlayer, startingTile, true);
            o.AddPawn((Pawn)pawnFlyer, false);
            for (int k = 0; k < this.pods.Count; k++)
            {
                ThingOwner innerContainer2 = this.pods[k].innerContainer;
                for (int l = 0; l < innerContainer2.Count; l++)
                {
                    if (!(innerContainer2[l] is Pawn))
                    {
                        Pawn pawn2 = CaravanInventoryUtility.FindPawnToMoveInventoryTo(innerContainer2[l], PawnFlyersTraveling.tmpPawns, null, null);
                        pawn2.inventory.innerContainer.TryAdd(innerContainer2[l], true);
                    }
                    else
                    {
                        Pawn pawn3 = innerContainer2[l] as Pawn;
                        if (!pawn3.IsPrisoner)
                        {
                            if (pawn3.Faction != pawnFlyer.Faction)
                                pawn3.SetFaction(pawnFlyer.Faction);
                        }
                    }
                }
            }
            this.RemoveAllPods();
            Find.WorldObjects.Remove(this);
            Messages.Message("MessageTransportPodsArrived".Translate(), o, MessageTypeDefOf.PositiveEvent);
        }

        private void RemoveAllPawnsFromWorldPawns()
        {
            for (int i = 0; i < this.pods.Count; i++)
            {
                ThingOwner innerContainer = this.pods[i].innerContainer;
                for (int j = 0; j < innerContainer.Count; j++)
                {
                    Pawn pawn = innerContainer[j] as Pawn;
                    Pawn pawnFlyer = innerContainer[j] as PawnFlyer;
                    if (pawn != null && pawn.IsWorldPawn())
                    {
                        Find.WorldPawns.RemovePawn(pawn);
                    }
                    else if (pawnFlyer != null && pawn.IsWorldPawn())
                    {
                        Find.WorldPawns.RemovePawn(pawnFlyer);
                    }

                }
            }
        }

        private void RemoveAllPods()
        {
            for (int i = 0; i < this.pods.Count; i++)
            {
                this.pods[i].savePawnsWithReferenceMode = false;
            }
            this.pods.Clear();
        }
    }
}
