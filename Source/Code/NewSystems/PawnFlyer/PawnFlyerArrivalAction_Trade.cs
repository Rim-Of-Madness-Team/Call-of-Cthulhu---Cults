// RimWorld.Planet.CaravanArrivalAction_Trade

using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;


namespace CultOfCthulhu;

public class PawnFlyerArrivalAction_Trade : PawnFlyerArrivalAction_VisitSettlement
{
    public PawnFlyerArrivalAction_Trade()
    {
    }

    public PawnFlyerArrivalAction_Trade(Settlement settlement)
        : base(settlement)
    {
    }

    public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, int destinationTile)
    {
        FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(pods, destinationTile);
        if (!floatMenuAcceptanceReport)
        {
            return floatMenuAcceptanceReport;
        }

        return CanTradeWith(pods, settlement);
    }

    public override void Arrived(List<ActiveDropPodInfo> pods, int tile)
    {
        Pawn pawn = null;
        for (int i = 0; i < pods.Count; i++)
        {
            if (pawn != null)
            {
                break;
            }

            foreach (Thing item in (IEnumerable<Thing>)pods[i].GetDirectlyHeldThings())
            {
                if (item is Pawn pawn2)
                {
                    pawn = pawn2;
                    break;
                }
            }
        }

        base.Arrived(pods, tile);
        if (pawn != null)
        {
            Caravan caravan = pawn.GetCaravan();
            if (caravan != null && CaravanArrivalAction_Trade.HasNegotiator(caravan, settlement))
            {
                CameraJumper.TryJumpAndSelect(caravan);
                Pawn playerNegotiator =
                    BestCaravanPawnUtility.FindBestNegotiator(caravan, settlement.Faction, settlement.TraderKind);
                Find.WindowStack.Add(new Dialog_Trade(playerNegotiator, settlement));
            }
        }
    }

    public static FloatMenuAcceptanceReport CanTradeWith(IEnumerable<IThingHolder> pods, Settlement settlement)
    {
        if (!TransportPodsArrivalAction_VisitSettlement.CanVisit(pods, settlement))
        {
            return false;
        }

        if (settlement.Faction == null || settlement.Faction == Faction.OfPlayer)
        {
            return false;
        }

        bool flag = false;
        foreach (IThingHolder pod in pods)
        {
            foreach (Thing item in (IEnumerable<Thing>)pod.GetDirectlyHeldThings())
            {
                if (item is Pawn pawn && pawn.RaceProps.Humanlike &&
                    pawn.CanTradeWith(settlement.Faction, settlement.TraderKind).Accepted)
                {
                    flag = true;
                    break;
                }
            }

            if (flag)
            {
                break;
            }
        }

        return flag && !settlement.HasMap && !settlement.Faction.def.permanentEnemy &&
               !settlement.Faction.HostileTo(Faction.OfPlayer) && settlement.CanTradeNow;
    }
}