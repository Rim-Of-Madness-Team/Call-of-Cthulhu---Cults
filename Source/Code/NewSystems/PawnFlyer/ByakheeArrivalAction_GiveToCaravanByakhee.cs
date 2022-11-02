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
			Scribe_References.Look<Caravan>(refee: ref this.caravan, label: "caravan", saveDestroyedThings: false);
		}

		public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, int destinationTile)
		{
			FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(pods: pods, destinationTile: destinationTile);
			if (!floatMenuAcceptanceReport)
			{
				return floatMenuAcceptanceReport;
			}
			if (this.caravan != null && !Find.WorldGrid.IsNeighborOrSame(tile1: this.caravan.Tile, tile2: destinationTile))
			{
				return false;
			}
			return ByakheeArrivalAction_GiveToCaravan.CanGiveTo(pods: pods, caravan: this.caravan);
		}

		public override void Arrived(List<ActiveDropPodInfo> pods, int tile)
		{
			for (int i = 0; i < pods.Count; i++)
			{
				ByakheeArrivalAction_GiveToCaravan.tmpContainedThings.Clear();
				ByakheeArrivalAction_GiveToCaravan.tmpContainedThings.AddRange(collection: pods[index: i].innerContainer);
				for (int j = 0; j < ByakheeArrivalAction_GiveToCaravan.tmpContainedThings.Count; j++)
				{
					pods[index: i].innerContainer.Remove(item: ByakheeArrivalAction_GiveToCaravan.tmpContainedThings[index: j]);
					this.caravan.AddPawnOrItem(thing: ByakheeArrivalAction_GiveToCaravan.tmpContainedThings[index: j], addCarriedPawnToWorldPawnsIfAny: true);
				}
			}
			ByakheeArrivalAction_GiveToCaravan.tmpContainedThings.Clear();
			Messages.Message(text: "MessageTransportPodsArrivedAndAddedToCaravan".Translate(arg1: this.caravan.Name), lookTargets: this.caravan, def: MessageTypeDefOf.TaskCompletion, historical: true);
		}

		public static FloatMenuAcceptanceReport CanGiveTo(IEnumerable<IThingHolder> pods, Caravan caravan)
		{
			return caravan != null && caravan.Spawned && caravan.IsPlayerControlled;
		}

		public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(CompLaunchablePawn representative, IEnumerable<IThingHolder> pods, Caravan caravan)
		{
			return ByakheeArrivalActionUtility.GetFloatMenuOptions<ByakheeArrivalAction_GiveToCaravan>(acceptanceReportGetter: () => ByakheeArrivalAction_GiveToCaravan.CanGiveTo(pods: pods, caravan: caravan), arrivalActionGetter: () => new ByakheeArrivalAction_GiveToCaravan(caravan: caravan), label: "GiveToCaravan".Translate(arg1: caravan.Label), representative: representative, destinationTile: caravan.Tile, uiConfirmationCallback: null);
		}
	}
}
