using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.QuestGen;
using Verse;
using RimWorld;
using RimWorld.Planet;

namespace CultOfCthulhu
{
    public static class PawnFlyerArrivalActionUtility
    {
        public static IEnumerable<FloatMenuOption> GetFloatMenuOptions<T>(
            Func<FloatMenuAcceptanceReport> acceptanceReportGetter, Func<T> arrivalActionGetter, string label,
            CompLaunchablePawn representative, int destinationTile, Action<Action> uiConfirmationCallback = null)
            where T : PawnFlyerArrivalAction
        {
            FloatMenuAcceptanceReport floatMenuAcceptanceReport = acceptanceReportGetter();
            if (!floatMenuAcceptanceReport.Accepted && floatMenuAcceptanceReport.FailReason.NullOrEmpty() &&
                floatMenuAcceptanceReport.FailMessage.NullOrEmpty())
            {
                yield break;
            }

            if (!floatMenuAcceptanceReport.FailReason.NullOrEmpty())
            {
                yield return new FloatMenuOption(label + " (" + floatMenuAcceptanceReport.FailReason + ")", null);
                yield break;
            }

            yield return new FloatMenuOption(label, delegate
            {
                FloatMenuAcceptanceReport floatMenuAcceptanceReport2 = acceptanceReportGetter();
                if (floatMenuAcceptanceReport2.Accepted)
                {
                    if (uiConfirmationCallback == null)
                    {
                        representative.TryLaunch(destinationTile, arrivalActionGetter());
                    }
                    else
                    {
                        uiConfirmationCallback(delegate
                        {
                            representative.TryLaunch(destinationTile, arrivalActionGetter());
                        });
                    }
                }
                else if (!floatMenuAcceptanceReport2.FailMessage.NullOrEmpty())
                {
                    Messages.Message(floatMenuAcceptanceReport2.FailMessage, new GlobalTargetInfo(destinationTile),
                        MessageTypeDefOf.RejectInput, historical: false);
                }
            });
        }

        public static IEnumerable<FloatMenuOption> GetFloatMenuOptions<T>(
            Func<FloatMenuAcceptanceReport> acceptanceReportGetter, Func<T> arrivalActionGetter, string label,
            Action<int, PawnFlyerArrivalAction> launchAction, int destinationTile)
            where T : PawnFlyerArrivalAction
        {
            FloatMenuAcceptanceReport floatMenuAcceptanceReport = acceptanceReportGetter();
            if (!floatMenuAcceptanceReport.Accepted && floatMenuAcceptanceReport.FailReason.NullOrEmpty() &&
                floatMenuAcceptanceReport.FailMessage.NullOrEmpty())
            {
                yield break;
            }

            if (!floatMenuAcceptanceReport.Accepted && !floatMenuAcceptanceReport.FailReason.NullOrEmpty())
            {
                label = label + " (" + floatMenuAcceptanceReport.FailReason + ")";
            }

            yield return new FloatMenuOption(label, delegate
            {
                FloatMenuAcceptanceReport floatMenuAcceptanceReport2 = acceptanceReportGetter();
                if (floatMenuAcceptanceReport2.Accepted)
                {
                    launchAction(destinationTile, arrivalActionGetter());
                }
                else if (!floatMenuAcceptanceReport2.FailMessage.NullOrEmpty())
                {
                    Messages.Message(floatMenuAcceptanceReport2.FailMessage, new GlobalTargetInfo(destinationTile),
                        MessageTypeDefOf.RejectInput, historical: false);
                }
            });
        }

        public static bool AnyNonDownedColonist(IEnumerable<IThingHolder> pods)
        {
            foreach (IThingHolder thingHolder in pods)
            {
                ThingOwner directlyHeldThings = thingHolder.GetDirectlyHeldThings();
                for (int i = 0; i < directlyHeldThings.Count; i++)
                {
                    Pawn pawn = directlyHeldThings[index: i] as Pawn;
                    if (pawn != null && pawn.IsColonist && !pawn.Downed)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool AnyPotentialCaravanOwner(IEnumerable<IThingHolder> pods, Faction faction)
        {
            foreach (IThingHolder thingHolder in pods)
            {
                ThingOwner directlyHeldThings = thingHolder.GetDirectlyHeldThings();
                for (int i = 0; i < directlyHeldThings.Count; i++)
                {
                    Pawn pawn = directlyHeldThings[index: i] as Pawn;
                    if (pawn != null && CaravanUtility.IsOwner(pawn: pawn, caravanFaction: faction))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static Thing GetLookTarget(List<ActiveDropPodInfo> pods)
        {
            for (int i = 0; i < pods.Count; i++)
            {
                ThingOwner directlyHeldThings = pods[index: i].GetDirectlyHeldThings();
                for (int j = 0; j < directlyHeldThings.Count; j++)
                {
                    Pawn pawn = directlyHeldThings[index: j] as Pawn;
                    if (pawn != null && pawn.IsColonist)
                    {
                        return pawn;
                    }
                }
            }

            for (int k = 0; k < pods.Count; k++)
            {
                Thing thing = pods[index: k].GetDirectlyHeldThings().FirstOrDefault<Thing>();
                if (thing != null)
                {
                    return thing;
                }
            }

            return null;
        }

        public static void DropTravelingTransportPods(List<ActiveDropPodInfo> dropPods, IntVec3 near, Map map)
        {
            PawnFlyerArrivalActionUtility.RemovePawnsFromWorldPawns(pods: dropPods);
            for (int i = 0; i < dropPods.Count; i++)
            {
                IntVec3 c;
                DropCellFinder.TryFindDropSpotNear(center: near, map: map, result: out c, allowFogged: false,
                    canRoofPunch: true, allowIndoors: true, size: null, mustBeReachableFromCenter: true);
                DropPodUtility.MakeDropPodAt(c: c, map: map, info: dropPods[index: i]);
            }
        }

        public static Thing DropShuttle(List<ActiveDropPodInfo> pods, Map map, IntVec3 cell, Faction faction = null)
        {
            PawnFlyerArrivalActionUtility.RemovePawnsFromWorldPawns(pods: pods);
            Thing thing = QuestGen_Shuttle.GenerateShuttle(owningFaction: faction, requiredPawns: null,
                requiredItems: null, acceptColonists: false, onlyAcceptColonists: false, onlyAcceptHealthy: false,
                requireColonistCount: 0, dropEverythingIfUnsatisfied: false, leaveImmediatelyWhenSatisfied: false,
                dropEverythingOnArrival: true, stayAfterDroppedEverythingOnArrival: false, missionShuttleTarget: null,
                missionShuttleHome: null, maxColonistCount: -1, shuttleDef: null, permitShuttle: false,
                hideControls: true, allowSlaves: false, requireAllColonistsOnMap: false);
            TransportShip transportShip = TransportShipMaker.MakeTransportShip(def: TransportShipDefOf.Ship_Shuttle,
                contents: null, shipThing: thing);
            CompTransporter compTransporter = thing.TryGetComp<CompTransporter>();
            for (int i = 0; i < pods.Count; i++)
            {
                compTransporter.innerContainer.TryAddRangeOrTransfer(things: pods[index: i].innerContainer,
                    canMergeWithExistingStacks: true, destroyLeftover: false);
            }

            if (!cell.IsValid)
            {
                cell = DropCellFinder.GetBestShuttleLandingSpot(map: map, factionForFindingSpot: Faction.OfPlayer);
            }

            transportShip.ArriveAt(cell: cell, mapParent: map.Parent);
            transportShip.AddJobs(defs: new ShipJobDef[]
            {
                ShipJobDefOf.Unload,
                ShipJobDefOf.FlyAway
            });
            return thing;
        }

        public static void RemovePawnsFromWorldPawns(List<ActiveDropPodInfo> pods)
        {
            for (int i = 0; i < pods.Count; i++)
            {
                ThingOwner innerContainer = pods[index: i].innerContainer;
                for (int j = 0; j < innerContainer.Count; j++)
                {
                    Pawn pawn = innerContainer[index: j] as Pawn;
                    if (pawn != null && pawn.IsWorldPawn())
                    {
                        Find.WorldPawns.RemovePawn(p: pawn);
                    }
                }
            }
        }

        public static IEnumerable<FloatMenuOption> GetSettlementFloatMenuOptions(IEnumerable<IThingHolder> pods,
            Action<int, PawnFlyerArrivalAction> launchAction, Settlement settlement)
        {
            if ((bool)PawnFlyerArrivalAction_Trade.CanTradeWith(pods, settlement))
            {
                yield return new FloatMenuOption("TradeWith".Translate(settlement.Label),
                    delegate { launchAction(settlement.Tile, new PawnFlyerArrivalAction_Trade(settlement)); });
            }

            if ((bool)PawnFlyerArrivalAction_GiveGift.CanGiveGiftTo(pods, settlement))
            {
                yield return new FloatMenuOption(
                    "GiveGiftViaTransportPods".Translate(settlement.Faction.Name,
                        FactionGiftUtility.GetGoodwillChange(pods, settlement).ToStringWithSign()), delegate
                    {
                        TradeRequestComp tradeReqComp = settlement.GetComponent<TradeRequestComp>();
                        if (tradeReqComp.ActiveRequest && pods.Any((IThingHolder p) =>
                                p.GetDirectlyHeldThings().Contains(tradeReqComp.requestThingDef)))
                        {
                            Find.WindowStack.Add(new Dialog_MessageBox(
                                "GiveGiftViaTransportPodsTradeRequestWarning".Translate(), "Yes".Translate(),
                                delegate
                                {
                                    launchAction(settlement.Tile, new PawnFlyerArrivalAction_GiveGift(settlement));
                                },
                                "No".Translate()));
                        }
                        else
                        {
                            launchAction(settlement.Tile, new PawnFlyerArrivalAction_GiveGift(settlement));
                        }
                    });
            }

            CompTransporter firstPod;
            if (settlement.HasMap || (firstPod = pods.FirstOrDefault() as CompTransporter) == null ||
                firstPod.Shuttle.shipParent == null)
            {
                yield break;
            }

            foreach (FloatMenuOption floatMenuOption in PawnFlyerArrivalActionUtility.GetFloatMenuOptions(
                         () => PawnFlyerArrivalAction_AttackSettlement.CanAttack(pods, settlement),
                         () => new PawnFlyerArrivalAction_TransportShip(settlement, firstPod.Shuttle.shipParent),
                         "AttackShuttle".Translate(settlement.Label), launchAction, settlement.Tile))
            {
                yield return floatMenuOption;
            }
        }


        public static IEnumerable<FloatMenuOption> GetSiteFloatMenuOptions(IEnumerable<IThingHolder> pods,
            Action<int, PawnFlyerArrivalAction> launchAction, Site site)
        {
            CompTransporterPawn firstPod;
            if ((firstPod = pods.FirstOrDefault() as CompTransporterPawn) == null)
            {
                yield break;
            }

            foreach (FloatMenuOption floatMenuOption in PawnFlyerArrivalActionUtility.GetFloatMenuOptions(
                         () => true,
                         () => new PawnFlyerArrivalAction_TransportShip(site, firstPod.Shuttle.shipParent),
                         "EnterMap".Translate(site.Label), launchAction, site.Tile))
            {
                yield return floatMenuOption;
            }
        }

        public static void MakeIncomingPawnFlyerAt(PawnFlyer flyer, IntVec3 c, Map map, ActiveDropPodInfo info,
            Faction faction = null)
        {
            ActiveDropPod activeDropPod = (ActiveDropPod)ThingMaker.MakeThing(
                ThingDef.Named("ByakheeDrop") ?? ThingDefOf.ActiveDropPod, null);
            activeDropPod.Contents = info;
            if (flyer.def is not PawnFlyerDef flyerDef) return;
            SkyfallerMaker.SpawnSkyfaller(
                flyerDef.incomingDef ?? ThingDefOf.DropPodIncoming, activeDropPod,
                c, map);

            using (IEnumerator<Thing> enumerator =
                   ((IEnumerable<Thing>)activeDropPod.Contents.innerContainer).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Pawn pawn;
                    if ((pawn = enumerator.Current as Pawn) != null && pawn.IsWorldPawn())
                    {
                        Find.WorldPawns.RemovePawn(pawn);
                        Pawn_PsychicEntropyTracker psychicEntropy = pawn.psychicEntropy;
                        if (psychicEntropy != null)
                        {
                            psychicEntropy.SetInitialPsyfocusLevel();
                        }
                    }
                }
            }
        }

        public static IEnumerable<FloatMenuOption> GetMapParentFloatMenuOptions(IEnumerable<IThingHolder> pods,
            Action<int, PawnFlyerArrivalAction> launchAction, MapParent mapParent)
        {
            if (!TransportPodsArrivalAction_LandInSpecificCell.CanLandInSpecificCell(pods, mapParent))
            {
                yield break;
            }

            yield return new FloatMenuOption("LandInExistingMap".Translate(mapParent.Label), delegate
            {
                Map map = mapParent.Map;
                Current.Game.CurrentMap = map;
                CameraJumper.TryHideWorld();
                Find.Targeter.BeginTargeting(TargetingParameters.ForDropPodsDestination(),
                    delegate(LocalTargetInfo x)
                    {
                        launchAction(mapParent.Tile,
                            new PawnFlyerArrivalAction_LandInSpecificCell(mapParent, x.Cell, landInShuttle: true));
                    }, delegate(LocalTargetInfo x) { RoyalTitlePermitWorker_CallShuttle.DrawShuttleGhost(x, map); },
                    delegate(LocalTargetInfo x)
                    {
                        AcceptanceReport acceptanceReport =
                            RoyalTitlePermitWorker_CallShuttle.ShuttleCanLandHere(x, map);
                        if (!acceptanceReport.Accepted)
                        {
                            Messages.Message(acceptanceReport.Reason, new LookTargets(mapParent),
                                MessageTypeDefOf.RejectInput, historical: false);
                        }

                        return acceptanceReport.Accepted;
                    }, null, null, CompLaunchable.TargeterMouseAttachment);
            });
        }
    }
}