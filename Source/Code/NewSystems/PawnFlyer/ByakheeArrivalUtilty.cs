using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.QuestGen;
using Verse;
using RimWorld;
using RimWorld.Planet;

namespace CultOfCthulhu
{
	public static class ByakheeArrivalActionUtility
	{
		public static IEnumerable<FloatMenuOption> GetFloatMenuOptions<T>(Func<FloatMenuAcceptanceReport> acceptanceReportGetter, Func<T> arrivalActionGetter, string label, CompLaunchablePawn representative, int destinationTile, Action<Action> uiConfirmationCallback = null) where T : TransportPodsArrivalAction
		{
			FloatMenuAcceptanceReport floatMenuAcceptanceReport = acceptanceReportGetter();
			if (floatMenuAcceptanceReport.Accepted || !floatMenuAcceptanceReport.FailReason.NullOrEmpty() || !floatMenuAcceptanceReport.FailMessage.NullOrEmpty())
			{
				if (!floatMenuAcceptanceReport.FailReason.NullOrEmpty())
				{
					yield return new FloatMenuOption(label: label + " (" + floatMenuAcceptanceReport.FailReason + ")", action: null, priority: MenuOptionPriority.Default, mouseoverGuiAction: null, revalidateClickTarget: null, extraPartWidth: 0f, extraPartOnGUI: null, revalidateWorldClickTarget: null, playSelectionSound: true, orderInPriority: 0);
				}
				else
				{
					//Action <> 9__1;
					yield return new FloatMenuOption(label: label, action: delegate ()
					{
						FloatMenuAcceptanceReport floatMenuAcceptanceReport2 = acceptanceReportGetter();
						if (!floatMenuAcceptanceReport2.Accepted)
						{
							if (!floatMenuAcceptanceReport2.FailMessage.NullOrEmpty())
							{
								Messages.Message(text: floatMenuAcceptanceReport2.FailMessage, lookTargets: new GlobalTargetInfo(tile: destinationTile), def: MessageTypeDefOf.RejectInput, historical: false);
							}
							return;
						}
						if (uiConfirmationCallback == null)
						{
							representative.TryLaunch(destinationTile: destinationTile, arrivalAction: arrivalActionGetter());
							return;
						}
						//Action<Action> uiConfirmationCallback2 = uiConfirmationCallback;
						//Action obj;
						//if ((obj = <> 9__1) == null)
						//{
						//	obj = (<> 9__1 = delegate ()
						//	{
								representative.TryLaunch(destinationTile: destinationTile, arrivalAction: arrivalActionGetter());
						//	});
						//}
						//uiConfirmationCallback2(obj);
					}, priority: MenuOptionPriority.Default, mouseoverGuiAction: null, revalidateClickTarget: null, extraPartWidth: 0f, extraPartOnGUI: null, revalidateWorldClickTarget: null, playSelectionSound: true, orderInPriority: 0);
				}
			}
			yield break;
		}

		public static IEnumerable<FloatMenuOption> GetFloatMenuOptions<T>(Func<FloatMenuAcceptanceReport> acceptanceReportGetter, Func<T> arrivalActionGetter, string label, Action<int, TransportPodsArrivalAction> launchAction, int destinationTile) where T : TransportPodsArrivalAction
		{
			FloatMenuAcceptanceReport floatMenuAcceptanceReport = acceptanceReportGetter();
			if (floatMenuAcceptanceReport.Accepted || !floatMenuAcceptanceReport.FailReason.NullOrEmpty() || !floatMenuAcceptanceReport.FailMessage.NullOrEmpty())
			{
				if (!floatMenuAcceptanceReport.Accepted && !floatMenuAcceptanceReport.FailReason.NullOrEmpty())
				{
					label = label + " (" + floatMenuAcceptanceReport.FailReason + ")";
				}
				yield return new FloatMenuOption(label: label, action: delegate ()
				{
					FloatMenuAcceptanceReport floatMenuAcceptanceReport2 = acceptanceReportGetter();
					if (floatMenuAcceptanceReport2.Accepted)
					{
						launchAction(arg1: destinationTile, arg2: arrivalActionGetter());
						return;
					}
					if (!floatMenuAcceptanceReport2.FailMessage.NullOrEmpty())
					{
						Messages.Message(text: floatMenuAcceptanceReport2.FailMessage, lookTargets: new GlobalTargetInfo(tile: destinationTile), def: MessageTypeDefOf.RejectInput, historical: false);
					}
				}, priority: MenuOptionPriority.Default, mouseoverGuiAction: null, revalidateClickTarget: null, extraPartWidth: 0f, extraPartOnGUI: null, revalidateWorldClickTarget: null, playSelectionSound: true, orderInPriority: 0);
			}
			yield break;
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
			ByakheeArrivalActionUtility.RemovePawnsFromWorldPawns(pods: dropPods);
			for (int i = 0; i < dropPods.Count; i++)
			{
				IntVec3 c;
				DropCellFinder.TryFindDropSpotNear(center: near, map: map, result: out c, allowFogged: false, canRoofPunch: true, allowIndoors: true, size: null, mustBeReachableFromCenter: true);
				DropPodUtility.MakeDropPodAt(c: c, map: map, info: dropPods[index: i]);
			}
		}

		public static Thing DropShuttle(List<ActiveDropPodInfo> pods, Map map, IntVec3 cell, Faction faction = null)
		{
			ByakheeArrivalActionUtility.RemovePawnsFromWorldPawns(pods: pods);
			Thing thing = QuestGen_Shuttle.GenerateShuttle(owningFaction: faction, requiredPawns: null, requiredItems: null, acceptColonists: false, onlyAcceptColonists: false, onlyAcceptHealthy: false, requireColonistCount: 0, dropEverythingIfUnsatisfied: false, leaveImmediatelyWhenSatisfied: false, dropEverythingOnArrival: true, stayAfterDroppedEverythingOnArrival: false, missionShuttleTarget: null, missionShuttleHome: null, maxColonistCount: -1, shuttleDef: null, permitShuttle: false, hideControls: true, allowSlaves: false, requireAllColonistsOnMap: false);
			TransportShip transportShip = TransportShipMaker.MakeTransportShip(def: TransportShipDefOf.Ship_Shuttle, contents: null, shipThing: thing);
			CompTransporter compTransporter = thing.TryGetComp<CompTransporter>();
			for (int i = 0; i < pods.Count; i++)
			{
				compTransporter.innerContainer.TryAddRangeOrTransfer(things: pods[index: i].innerContainer, canMergeWithExistingStacks: true, destroyLeftover: false);
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
	}
}
