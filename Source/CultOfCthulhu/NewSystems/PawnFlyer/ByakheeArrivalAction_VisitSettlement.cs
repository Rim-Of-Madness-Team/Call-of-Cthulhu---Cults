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
			Scribe_References.Look<Settlement>(ref this.settlement, "settlement", false);
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
			return ByakheeArrivalAction_VisitSettlement.CanVisit(pods, this.settlement);
		}

		public static FloatMenuAcceptanceReport CanVisit(IEnumerable<IThingHolder> pods, Settlement settlement)
		{
			if (settlement == null || !settlement.Spawned || !settlement.Visitable)
			{
				if (settlement.Faction.IsPlayer) return true;
				return false;
			}
			if (!TransportPodsArrivalActionUtility.AnyPotentialCaravanOwner(pods, Faction.OfPlayer))
			{
				return false;
			}
			return true;
		}

		public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(CompLaunchablePawn representative, IEnumerable<IThingHolder> pods, Settlement settlement)
		{
			return ByakheeArrivalActionUtility.GetFloatMenuOptions<ByakheeArrivalAction_VisitSettlement>(() => ByakheeArrivalAction_VisitSettlement.CanVisit(pods, settlement), () => new ByakheeArrivalAction_VisitSettlement(settlement), "VisitSettlement".Translate(settlement.Label), representative, settlement.Tile, null);
		}
	}
}
