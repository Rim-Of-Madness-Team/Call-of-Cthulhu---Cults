using System;
using System.Collections.Generic;
using Verse;
using RimWorld.Planet;
using RimWorld;

namespace CultOfCthulhu
{
	public class ByakheeArrivalAction_AttackSettlement : TransportPodsArrivalAction
	{
		private Settlement settlement;

		private PawnsArrivalModeDef arrivalMode;

		public ByakheeArrivalAction_AttackSettlement()
		{
		}

		public ByakheeArrivalAction_AttackSettlement(Settlement settlement, PawnsArrivalModeDef arrivalMode)
		{
			this.settlement = settlement;
			this.arrivalMode = arrivalMode;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look<Settlement>(ref this.settlement, "settlement", false);
			Scribe_Defs.Look<PawnsArrivalModeDef>(ref this.arrivalMode, "arrivalMode");
		}

		public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, int destinationTile)
		{
			FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(pods, destinationTile);
			if (!floatMenuAcceptanceReport)
			{
				return floatMenuAcceptanceReport;
			}
			if (this.settlement != null && this.settlement.Tile != destinationTile)
			{
				return false;
			}
			return ByakheeArrivalAction_AttackSettlement.CanAttack(pods, this.settlement);
		}

		public override bool ShouldUseLongEvent(List<ActiveDropPodInfo> pods, int tile)
		{
			return !this.settlement.HasMap;
		}

		public override void Arrived(List<ActiveDropPodInfo> pods, int tile)
		{
			Thing lookTarget = ByakheeArrivalActionUtility.GetLookTarget(pods);
			bool flag = !this.settlement.HasMap;
			Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(this.settlement.Tile, null);
			TaggedString label = "LetterLabelCaravanEnteredEnemyBase".Translate();
			TaggedString text = "LetterTransportPodsLandedInEnemyBase".Translate(this.settlement.Label).CapitalizeFirst();
			SettlementUtility.AffectRelationsOnAttacked(this.settlement, ref text);
			if (flag)
			{
				Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
				PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(orGenerateMap.mapPawns.AllPawns, ref label, ref text, "LetterRelatedPawnsInMapWherePlayerLanded".Translate(Faction.OfPlayer.def.pawnsPlural), true, true);
			}
			Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, lookTarget, this.settlement.Faction, null, null, null);
			this.arrivalMode.Worker.TravelingTransportPodsArrived(pods, orGenerateMap);
		}

		public static FloatMenuAcceptanceReport CanAttack(IEnumerable<IThingHolder> pods, Settlement settlement)
		{
			if (settlement == null || !settlement.Spawned || !settlement.Attackable)
			{
				return false;
			}
			if (!ByakheeArrivalActionUtility.AnyNonDownedColonist(pods))
			{
				return false;
			}
			if (settlement.EnterCooldownBlocksEntering())
			{
				return FloatMenuAcceptanceReport.WithFailReasonAndMessage("EnterCooldownBlocksEntering".Translate(), "MessageEnterCooldownBlocksEntering".Translate(settlement.EnterCooldownTicksLeft().ToStringTicksToPeriod(true, false, true, true)));
			}
			return true;
		}


		private static ByakheeArrivalAction_AttackSettlement arrivalActionEdgeDrop(Settlement settlement)
		{
			return new ByakheeArrivalAction_AttackSettlement(settlement, PawnsArrivalModeDefOf.EdgeDrop);
		}

		private static ByakheeArrivalAction_AttackSettlement arrivalActionCenterDrop(Settlement settlement)
		{
			return new ByakheeArrivalAction_AttackSettlement(settlement, PawnsArrivalModeDefOf.CenterDrop);
		}

		public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(CompLaunchablePawn representative, IEnumerable<IThingHolder> pods, Settlement settlement)
		{
			if (ByakheeArrivalAction_AttackSettlement.CanAttack(pods, settlement))
			{
				yield break;
            }

			Func<FloatMenuAcceptanceReport> acceptanceReportGetter = new Func<FloatMenuAcceptanceReport>(() => CanAttack(pods, settlement));
			Func<ByakheeArrivalAction_AttackSettlement> dropAtEdge = new Func<ByakheeArrivalAction_AttackSettlement>(() => arrivalActionEdgeDrop(settlement));

			Func<FloatMenuAcceptanceReport> idunno = null;
            foreach (FloatMenuOption floatMenuOption in ByakheeArrivalActionUtility.GetFloatMenuOptions<ByakheeArrivalAction_AttackSettlement>(acceptanceReportGetter, dropAtEdge, "AttackAndDropAtEdge".Translate(settlement.Label), representative, settlement.Tile, null))
			{
				yield return floatMenuOption;
			}


			Func<ByakheeArrivalAction_AttackSettlement> dropAtCenter = new Func<ByakheeArrivalAction_AttackSettlement>(() => arrivalActionCenterDrop(settlement));

			foreach (FloatMenuOption floatMenuOption2 in ByakheeArrivalActionUtility.GetFloatMenuOptions<ByakheeArrivalAction_AttackSettlement>(acceptanceReportGetter, dropAtCenter, "AttackAndDropInCenter".Translate(settlement.Label), representative, settlement.Tile, null))
			{
				yield return floatMenuOption2;
			}
			yield break;
		}

    }
}
