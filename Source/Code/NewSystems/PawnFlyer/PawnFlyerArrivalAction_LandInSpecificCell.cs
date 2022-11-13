using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace CultOfCthulhu;

public class PawnFlyerArrivalAction_LandInSpecificCell : PawnFlyerArrivalAction
{
    private MapParent mapParent;

    private IntVec3 cell;

    private bool landInShuttle;

    public PawnFlyerArrivalAction_LandInSpecificCell()
    {
    }

    public PawnFlyerArrivalAction_LandInSpecificCell(MapParent mapParent, IntVec3 cell)
    {
        this.mapParent = mapParent;
        this.cell = cell;
    }

    public PawnFlyerArrivalAction_LandInSpecificCell(MapParent mapParent, IntVec3 cell, bool landInShuttle)
    {
        this.mapParent = mapParent;
        this.cell = cell;
        this.landInShuttle = landInShuttle;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref mapParent, "mapParent");
        Scribe_Values.Look(ref cell, "cell");
        Scribe_Values.Look(ref landInShuttle, "landInShuttle", defaultValue: false);
    }

    public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, int destinationTile)
    {
        FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(pods, destinationTile);
        if (!floatMenuAcceptanceReport)
        {
            return floatMenuAcceptanceReport;
        }

        if (mapParent != null && mapParent.Tile != destinationTile)
        {
            return false;
        }

        return CanLandInSpecificCell(pods, mapParent);
    }

    public override void Arrived(List<ActiveDropPodInfo> pods, int tile)
    {
        Thing lookTarget = PawnFlyerArrivalActionUtility.GetLookTarget(pods);
        if (landInShuttle)
        {
            PawnFlyerArrivalActionUtility.DropShuttle(pods, mapParent.Map, cell);
            Messages.Message("MessageShuttleArrived".Translate(), lookTarget, MessageTypeDefOf.TaskCompletion);
        }
        else
        {
            PawnFlyerArrivalActionUtility.DropTravelingTransportPods(pods, cell, mapParent.Map);
            Messages.Message("MessageTransportPodsArrived".Translate(), lookTarget, MessageTypeDefOf.TaskCompletion);
        }
    }

    public static bool CanLandInSpecificCell(IEnumerable<IThingHolder> pods, MapParent mapParent)
    {
        if (mapParent == null || !mapParent.Spawned || !mapParent.HasMap)
        {
            return false;
        }

        if (mapParent.EnterCooldownBlocksEntering())
        {
            return FloatMenuAcceptanceReport.WithFailMessage(
                "MessageEnterCooldownBlocksEntering".Translate(mapParent.EnterCooldownTicksLeft()
                    .ToStringTicksToPeriod()));
        }

        return true;
    }
}