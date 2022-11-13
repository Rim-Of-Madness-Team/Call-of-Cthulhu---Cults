// RimWorld.Planet.TransportPodsArrivalAction_GiveToCaravan

using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace CultOfCthulhu;

public class PawnFlyerArrivalAction_GiveToCaravan : PawnFlyerArrivalAction
{
    private Caravan caravan;

    private static List<Thing> tmpContainedThings = new List<Thing>();

    public PawnFlyerArrivalAction_GiveToCaravan()
    {
    }

    public PawnFlyerArrivalAction_GiveToCaravan(Caravan caravan)
    {
        this.caravan = caravan;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref caravan, "caravan");
    }

    public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, int destinationTile)
    {
        FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(pods, destinationTile);
        if (!floatMenuAcceptanceReport)
        {
            return floatMenuAcceptanceReport;
        }

        if (caravan != null && !Find.WorldGrid.IsNeighborOrSame(caravan.Tile, destinationTile))
        {
            return false;
        }

        return CanGiveTo(pods, caravan);
    }

    public override void Arrived(List<ActiveDropPodInfo> pods, int tile)
    {
        for (int i = 0; i < pods.Count; i++)
        {
            tmpContainedThings.Clear();
            tmpContainedThings.AddRange(pods[i].innerContainer);
            for (int j = 0; j < tmpContainedThings.Count; j++)
            {
                pods[i].innerContainer.Remove(tmpContainedThings[j]);
                caravan.AddPawnOrItem(tmpContainedThings[j], addCarriedPawnToWorldPawnsIfAny: true);
            }
        }

        tmpContainedThings.Clear();
        Messages.Message("PawnFlyer_MessageArrivedAndAddedToCaravan".Translate(caravan.Name), caravan,
            MessageTypeDefOf.TaskCompletion);
    }

    public static FloatMenuAcceptanceReport CanGiveTo(IEnumerable<IThingHolder> pods, Caravan caravan)
    {
        return caravan != null && caravan.Spawned && caravan.IsPlayerControlled;
    }

    public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(CompLaunchablePawn representative,
        IEnumerable<IThingHolder> pods, Caravan caravan)
    {
        return PawnFlyerArrivalActionUtility.GetFloatMenuOptions(() => CanGiveTo(pods, caravan),
            () => new PawnFlyerArrivalAction_GiveToCaravan(caravan), "GiveToCaravan".Translate(caravan.Label),
            representative, caravan.Tile);
    }
}