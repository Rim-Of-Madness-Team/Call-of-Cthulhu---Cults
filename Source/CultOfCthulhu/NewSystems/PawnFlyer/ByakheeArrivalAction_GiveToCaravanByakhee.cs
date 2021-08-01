using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

namespace CultOfCthulhu
{
	public class ByakheeArrivalAction_GiveToCaravan : TransportPodsArrivalAction
	{
		private Caravan caravan;

		private static List<Thing> tmpContainedThings = new List<Thing>();

		public ByakheeArrivalAction_GiveToCaravan()
		{
		}

		public ByakheeArrivalAction_GiveToCaravan(Caravan caravan)
		{
			this.caravan = caravan;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look<Caravan>(ref this.caravan, "caravan", false);
		}

		public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, int destinationTile)
		{
			FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(pods, destinationTile);
			if (!floatMenuAcceptanceReport)
			{
				return floatMenuAcceptanceReport;
			}
			if (this.caravan != null && !Find.WorldGrid.IsNeighborOrSame(this.caravan.Tile, destinationTile))
			{
				return false;
			}
			return ByakheeArrivalAction_GiveToCaravan.CanGiveTo(pods, this.caravan);
		}

		public override void Arrived(List<ActiveDropPodInfo> pods, int tile)
		{
			for (int i = 0; i < pods.Count; i++)
			{
				ByakheeArrivalAction_GiveToCaravan.tmpContainedThings.Clear();
				ByakheeArrivalAction_GiveToCaravan.tmpContainedThings.AddRange(pods[i].innerContainer);
				for (int j = 0; j < ByakheeArrivalAction_GiveToCaravan.tmpContainedThings.Count; j++)
				{
					pods[i].innerContainer.Remove(ByakheeArrivalAction_GiveToCaravan.tmpContainedThings[j]);
					this.caravan.AddPawnOrItem(ByakheeArrivalAction_GiveToCaravan.tmpContainedThings[j], true);
				}
			}
			ByakheeArrivalAction_GiveToCaravan.tmpContainedThings.Clear();
			Messages.Message("MessageTransportPodsArrivedAndAddedToCaravan".Translate(this.caravan.Name), this.caravan, MessageTypeDefOf.TaskCompletion, true);
		}

		public static FloatMenuAcceptanceReport CanGiveTo(IEnumerable<IThingHolder> pods, Caravan caravan)
		{
			return caravan != null && caravan.Spawned && caravan.IsPlayerControlled;
		}

		public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(CompLaunchablePawn representative, IEnumerable<IThingHolder> pods, Caravan caravan)
		{
			return ByakheeArrivalActionUtility.GetFloatMenuOptions<ByakheeArrivalAction_GiveToCaravan>(() => ByakheeArrivalAction_GiveToCaravan.CanGiveTo(pods, caravan), () => new ByakheeArrivalAction_GiveToCaravan(caravan), "GiveToCaravan".Translate(caravan.Label), representative, caravan.Tile, null);
		}
	}
}
