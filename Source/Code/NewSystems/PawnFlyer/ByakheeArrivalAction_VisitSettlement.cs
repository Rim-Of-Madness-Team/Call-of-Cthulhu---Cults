using System;
using System.Collections.Generic;
using Verse;
using RimWorld.Planet;
using RimWorld;

namespace CultOfCthulhu
{
	public class ByakheeArrivalAction_VisitSettlement : TransportPodsArrivalAction_FormCaravan
	{
		protected Settlement settlement;

		public ByakheeArrivalAction_VisitSettlement()
		{
		}

		public ByakheeArrivalAction_VisitSettlement(Settlement settlement)
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
			return ByakheeArrivalAction_VisitSettlement.CanVisit(pods: pods, settlement: this.settlement);
		}

		public static FloatMenuAcceptanceReport CanVisit(IEnumerable<IThingHolder> pods, Settlement settlement)
		{
			if (settlement == null || !settlement.Spawned || !settlement.Visitable)
			{
				if (settlement.Faction.IsPlayer) return true;
				return false;
			}
			if (!TransportPodsArrivalActionUtility.AnyPotentialCaravanOwner(pods: pods, faction: Faction.OfPlayer))
			{
				return false;
			}
			return true;
		}

		public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(CompLaunchablePawn representative, IEnumerable<IThingHolder> pods, Settlement settlement)
		{
			return ByakheeArrivalActionUtility.GetFloatMenuOptions<ByakheeArrivalAction_VisitSettlement>(acceptanceReportGetter: () => ByakheeArrivalAction_VisitSettlement.CanVisit(pods: pods, settlement: settlement), arrivalActionGetter: () => new ByakheeArrivalAction_VisitSettlement(settlement: settlement), label: "VisitSettlement".Translate(arg1: settlement.Label), representative: representative, destinationTile: settlement.Tile, uiConfirmationCallback: null);
		}
	}
}
