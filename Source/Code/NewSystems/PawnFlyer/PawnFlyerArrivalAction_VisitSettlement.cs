using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace CultOfCthulhu;

public class PawnFlyerArrivalAction_VisitSettlement : PawnFlyerArrivalAction_FormCaravan
{
    protected Settlement settlement;

    public PawnFlyerArrivalAction_VisitSettlement()
    {
    }

    public PawnFlyerArrivalAction_VisitSettlement(Settlement settlement)
    {
        this.settlement = settlement;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref settlement, "settlement");
    }

    public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, int destinationTile)
    {
        FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(pods, destinationTile);
        if (!floatMenuAcceptanceReport)
        {
            return floatMenuAcceptanceReport;
        }

        if (settlement != null && settlement.Tile != destinationTile)
        {
            return false;
        }

        return CanVisit(pods, settlement);
    }

    public static FloatMenuAcceptanceReport CanVisit(IEnumerable<IThingHolder> pods, Settlement settlement)
    {
        if (settlement == null || !settlement.Spawned || !settlement.Visitable)
        {
            return false;
        }

        if (!PawnFlyerArrivalActionUtility.AnyPotentialCaravanOwner(pods, Faction.OfPlayer))
        {
            return false;
        }

        return true;
    }

    public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(CompLaunchablePawn representative,
        IEnumerable<IThingHolder> pods, Settlement settlement)
    {
        return PawnFlyerArrivalActionUtility.GetFloatMenuOptions(() => CanVisit(pods, settlement),
            () => new PawnFlyerArrivalAction_VisitSettlement(settlement),
            "VisitSettlement".Translate(settlement.Label), representative, settlement.Tile);
    }
}