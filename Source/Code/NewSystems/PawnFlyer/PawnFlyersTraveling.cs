using System;
using System.Collections.Generic;
using System.Linq;
using Cthulhu;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace CultOfCthulhu
{
    public class PawnFlyersTraveling : WorldObject, IThingHolder
    {
        public int destinationTile = -1;

        public PawnFlyerArrivalAction arrivalAction;

        private List<ActiveDropPodInfo> pods = new List<ActiveDropPodInfo>();

        private bool arrived;

        private int initialTile = -1;

        private float traveledPct;

        private const float TravelSpeed = 0.00025f;

        public bool IsPlayerControlled => base.Faction == Faction.OfPlayer;

        private Vector3 Start => Find.WorldGrid.GetTileCenter(initialTile);

        private Vector3 End => Find.WorldGrid.GetTileCenter(destinationTile);

        public override Vector3 DrawPos => Vector3.Slerp(Start, End, traveledPct);

        public override bool ExpandingIconFlipHorizontal =>
            GenWorldUI.WorldToUIPosition(Start).x > GenWorldUI.WorldToUIPosition(End).x;

        public override float ExpandingIconRotation
        {
            get
            {
                if (!def.rotateGraphicWhenTraveling)
                {
                    return base.ExpandingIconRotation;
                }

                Vector2 vector = GenWorldUI.WorldToUIPosition(Start);
                Vector2 vector2 = GenWorldUI.WorldToUIPosition(End);
                float num = Mathf.Atan2(vector2.y - vector.y, vector2.x - vector.x) * 57.29578f;
                if (num > 180f)
                {
                    num -= 180f;
                }

                return num + 90f;
            }
        }

        private float TraveledPctStepPerTick
        {
            get
            {
                Vector3 start = Start;
                Vector3 end = End;
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

        private bool PodsHaveAnyPotentialCaravanOwner
        {
            get
            {
                for (int i = 0; i < pods.Count; i++)
                {
                    ThingOwner innerContainer = pods[i].innerContainer;
                    for (int j = 0; j < innerContainer.Count; j++)
                    {
                        if (innerContainer[j] is Pawn pawn && CaravanUtility.IsOwner(pawn, base.Faction))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public bool PodsHaveAnyFreeColonist
        {
            get
            {
                for (int i = 0; i < pods.Count; i++)
                {
                    ThingOwner innerContainer = pods[i].innerContainer;
                    for (int j = 0; j < innerContainer.Count; j++)
                    {
                        if (innerContainer[j] is Pawn pawn && pawn.IsColonist && pawn.HostFaction == null)
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
                for (int i = 0; i < pods.Count; i++)
                {
                    ThingOwner things = pods[i].innerContainer;
                    for (int j = 0; j < things.Count; j++)
                    {
                        if (things[j] is Pawn pawn)
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
            Scribe_Collections.Look(ref pods, "pods", LookMode.Deep);
            Scribe_Values.Look(ref destinationTile, "destinationTile", 0);
            Scribe_Deep.Look(ref arrivalAction, "arrivalAction");
            Scribe_Values.Look(ref arrived, "arrived", defaultValue: false);
            Scribe_Values.Look(ref initialTile, "initialTile", 0);
            Scribe_Values.Look(ref traveledPct, "traveledPct", 0f);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                for (int i = 0; i < pods.Count; i++)
                {
                    pods[i].parent = this;
                }
            }
        }

        public override void PostAdd()
        {
            base.PostAdd();
            initialTile = base.Tile;
        }

        public override void Tick()
        {
            base.Tick();
            traveledPct += TraveledPctStepPerTick;
            if (traveledPct >= 1f)
            {
                traveledPct = 1f;
                Arrived();
            }
        }

        public void AddPod(ActiveDropPodInfo contents, bool justLeftTheMap)
        {
            contents.parent = this;
            pods.Add(contents);
            ThingOwner innerContainer = contents.innerContainer;
            for (int i = 0; i < innerContainer.Count; i++)
            {
                if (innerContainer[i] is Pawn pawn && !pawn.IsWorldPawn())
                {
                    if (!base.Spawned)
                    {
                        Log.Warning(string.Concat("Passing pawn ", pawn,
                            " to world, but the TravelingTransportPod is not spawned. This means that WorldPawns can discard this pawn which can cause bugs."));
                    }

                    if (justLeftTheMap)
                    {
                        pawn.ExitMap(allowedToJoinOrCreateCaravan: false, Rot4.Invalid);
                    }
                    else
                    {
                        Find.WorldPawns.PassToWorld(pawn);
                    }
                }
            }

            contents.savePawnsWithReferenceMode = true;
        }

        public bool ContainsPawn(Pawn p)
        {
            for (int i = 0; i < pods.Count; i++)
            {
                if (pods[i].innerContainer.Contains(p))
                {
                    return true;
                }
            }

            return false;
        }

        private void Arrived()
        {
            if (arrived)
            {
                return;
            }

            arrived = true;
            if (arrivalAction == null || !arrivalAction.StillValid(pods.Cast<IThingHolder>(), destinationTile))
            {
                arrivalAction = null;
                List<Map> maps = Find.Maps;
                for (int i = 0; i < maps.Count; i++)
                {
                    if (maps[i].Tile == destinationTile)
                    {
                        arrivalAction = new PawnFlyerArrivalAction_LandInSpecificCell(maps[i].Parent,
                            DropCellFinder.RandomDropSpot(maps[i]));
                        break;
                    }
                }

                if (arrivalAction == null)
                {
                    if (TransportPodsArrivalAction_FormCaravan.CanFormCaravanAt(pods.Cast<IThingHolder>(),
                            destinationTile))
                    {
                        arrivalAction = new PawnFlyerArrivalAction_FormCaravan();
                    }
                    else
                    {
                        List<Caravan> caravans = Find.WorldObjects.Caravans;
                        for (int j = 0; j < caravans.Count; j++)
                        {
                            if (caravans[j].Tile == destinationTile &&
                                (bool)PawnFlyerArrivalAction_GiveToCaravan.CanGiveTo(pods.Cast<IThingHolder>(),
                                    caravans[j]))
                            {
                                arrivalAction = new PawnFlyerArrivalAction_GiveToCaravan(caravans[j]);
                                break;
                            }
                        }
                    }
                }
            }

            if (arrivalAction != null && arrivalAction.ShouldUseLongEvent(pods, destinationTile))
            {
                LongEventHandler.QueueLongEvent(delegate { DoArrivalAction(); }, "GeneratingMapForNewEncounter",
                    doAsynchronously: false, null);
            }
            else
            {
                DoArrivalAction();
            }
        }

        private void DoArrivalAction()
        {
            for (int i = 0; i < pods.Count; i++)
            {
                pods[i].savePawnsWithReferenceMode = false;
                pods[i].parent = null;
            }

            if (arrivalAction != null)
            {
                try
                {
                    arrivalAction.Arrived(pods, destinationTile);
                }
                catch (Exception ex)
                {
                    Log.Error("Exception in transport pods arrival action: " + ex);
                }

                arrivalAction = null;
            }
            else
            {
                for (int j = 0; j < pods.Count; j++)
                {
                    for (int k = 0; k < pods[j].innerContainer.Count; k++)
                    {
                        if (pods[j].innerContainer[k] is Pawn pawn &&
                            (pawn.Faction == Faction.OfPlayer || pawn.HostFaction == Faction.OfPlayer))
                        {
                            PawnBanishUtility.Banish(pawn, destinationTile);
                        }
                    }
                }

                bool flag = true;
                if (ModsConfig.BiotechActive)
                {
                    flag = false;
                    for (int l = 0; l < pods.Count; l++)
                    {
                        if (flag)
                        {
                            break;
                        }

                        for (int m = 0; m < pods[l].innerContainer.Count; m++)
                        {
                            if (pods[l].innerContainer[m].def != ThingDefOf.Wastepack)
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                }

                for (int n = 0; n < pods.Count; n++)
                {
                    for (int num = 0; num < pods[n].innerContainer.Count; num++)
                    {
                        pods[n].innerContainer[num].Notify_AbandonedAtTile(destinationTile);
                    }
                }

                for (int num2 = 0; num2 < pods.Count; num2++)
                {
                    pods[num2].innerContainer.ClearAndDestroyContentsOrPassToWorld();
                }

                if (flag)
                {
                    string key = "PawnFlyer_MessageArrivedAndLost";
                    if (def == WorldObjectDefOf.TravelingShuttle)
                    {
                        key = "PawnFlyer_MessageArrivedAndLost";
                    }

                    Messages.Message(key.Translate(), new GlobalTargetInfo(destinationTile),
                        MessageTypeDefOf.NegativeEvent);
                }
            }

            pods.Clear();
            Destroy();
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return null;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
            for (int i = 0; i < pods.Count; i++)
            {
                outChildren.Add(pods[i]);
            }
        }
    }
}