using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld.Planet;
using RimWorld;

namespace CultOfCthulhu
{
	public class ByakheeArrivalAction_GiveGift : TransportPodsArrivalAction
	{
		private Settlement settlement;

		public ByakheeArrivalAction_GiveGift()
		{
		}

		public ByakheeArrivalAction_GiveGift(Settlement settlement)
		{
			this.settlement = settlement;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look<Settlement>(refee: ref this.settlement, label: "settlement", saveDestroyedThings: false);
		}

		public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, int destinationTile)
		{
			FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(pods: pods, destinationTile: destinationTile);
			if (!floatMenuAcceptanceReport)
			{
				return floatMenuAcceptanceReport;
			}
			if (this.settlement != null && this.settlement.Tile != destinationTile)
			{
				return false;
			}
			return ByakheeArrivalAction_GiveGift.CanGiveGiftTo(pods: pods, settlement: this.settlement);
		}

		public override void Arrived(List<ActiveDropPodInfo> pods, int tile)
		{
			for (int i = 0; i < pods.Count; i++)
			{
				for (int j = 0; j < pods[index: i].innerContainer.Count; j++)
				{
					Pawn pawn = pods[index: i].innerContainer[index: j] as Pawn;
					if (pawn != null)
					{
						if (pawn.RaceProps.Humanlike)
						{
							Pawn arg;
							if (pawn.HomeFaction == this.settlement.Faction)
							{
								GenGuest.AddHealthyPrisonerReleasedThoughts(prisoner: pawn);
							}
							else if (PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists.TryRandomElement(result: out arg))
							{
								Find.HistoryEventsManager.RecordEvent(historyEvent: new HistoryEvent(def: HistoryEventDefOf.SoldSlave, arg1: arg.Named(label: HistoryEventArgsNames.Doer)), canApplySelfTookThoughts: true);
							}
						}
						else if (pawn.RaceProps.Animal && pawn.relations != null)
						{
							Pawn firstDirectRelationPawn = pawn.relations.GetFirstDirectRelationPawn(def: PawnRelationDefOf.Bond, predicate: null);
							if (firstDirectRelationPawn != null && firstDirectRelationPawn.needs.mood != null)
							{
								pawn.relations.RemoveDirectRelation(def: PawnRelationDefOf.Bond, otherPawn: firstDirectRelationPawn);
								firstDirectRelationPawn.needs.mood.thoughts.memories.TryGainMemory(def: ThoughtDefOf.SoldMyBondedAnimalMood, otherPawn: null, sourcePrecept: null);
							}
						}
					}
				}
			}
			FactionGiftUtility.GiveGift(pods: pods, giveTo: this.settlement);
		}

		public static FloatMenuAcceptanceReport CanGiveGiftTo(IEnumerable<IThingHolder> pods, Settlement settlement)
		{
			foreach (IThingHolder thingHolder in pods)
			{
				ThingOwner directlyHeldThings = thingHolder.GetDirectlyHeldThings();
				for (int i = 0; i < directlyHeldThings.Count; i++)
				{
					Pawn p;
					if ((p = (directlyHeldThings[index: i] as Pawn)) != null && p.IsQuestLodger())
					{
						return false;
					}
				}
			}
			return settlement != null && settlement.Spawned && settlement.Faction != null && settlement.Faction != Faction.OfPlayer && !settlement.Faction.def.permanentEnemy && !settlement.HasMap;
		}

		public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(CompLaunchablePawn representative, IEnumerable<IThingHolder> pods, Settlement settlement)
		{
			if (settlement.Faction == Faction.OfPlayer)
			{
				return Enumerable.Empty<FloatMenuOption>();
			}
			return ByakheeArrivalActionUtility.GetFloatMenuOptions<ByakheeArrivalAction_GiveGift>(acceptanceReportGetter: () => ByakheeArrivalAction_GiveGift.CanGiveGiftTo(pods: pods, settlement: settlement), arrivalActionGetter: () => new ByakheeArrivalAction_GiveGift(settlement: settlement), label: "GiveGiftViaTransportPods".Translate(arg1: settlement.Faction.Name, arg2: FactionGiftUtility.GetGoodwillChange(pods: pods, giveTo: settlement).ToStringWithSign()), representative: representative, destinationTile: settlement.Tile, uiConfirmationCallback: delegate (Action action)
			{
				TradeRequestComp tradeReqComp = settlement.GetComponent<TradeRequestComp>();
				if (tradeReqComp.ActiveRequest && pods.Any(predicate: (IThingHolder p) => p.GetDirectlyHeldThings().Contains(def: tradeReqComp.requestThingDef)))
				{
					Find.WindowStack.Add(window: new Dialog_MessageBox(text: "GiveGiftViaTransportPodsTradeRequestWarning".Translate(), buttonAText: "Yes".Translate(), buttonAAction: delegate ()
					{
						action();
					}, buttonBText: "No".Translate(), buttonBAction: null, title: null, buttonADestructive: false, acceptAction: null, cancelAction: null, layer: WindowLayer.Dialog));
					return;
				}
				action();
			});
		}
	}
}
