// RimWorld.PawnFlyerArrivalAction_TransportShip

using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace CultOfCthulhu;

public class PawnFlyerArrivalAction_TransportShip : PawnFlyerArrivalAction
{
    public MapParent mapParent;

    public TransportShip transportShip;

    public IntVec3 cell = IntVec3.Invalid;

    public PawnFlyerArrivalAction_TransportShip()
    {
    }

    public PawnFlyerArrivalAction_TransportShip(MapParent mapParent, TransportShip transportShip)
    {
        this.mapParent = mapParent;
        this.transportShip = transportShip;
    }

    public override bool ShouldUseLongEvent(List<ActiveDropPodInfo> pods, int tile)
    {
        return !mapParent.HasMap;
    }

    public override void Arrived(List<ActiveDropPodInfo> pods, int tile)
    {
        if (transportShip == null || transportShip.Disposed)
        {
            Log.Error("Trying to arrive in a null or disposed transport ship.");
            return;
        }

        bool flag = !mapParent.HasMap;
        Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(tile, null);
        if (!cell.IsValid)
        {
            cell = DropCellFinder.GetBestShuttleLandingSpot(orGenerateMap, Faction.OfPlayer);
        }

        LookTargets lookTargets = new LookTargets(cell, orGenerateMap);
        if (!cell.IsValid)
        {
            Log.Error("Could not find cell for transport ship arrival.");
            return;
        }

        if (orGenerateMap.Parent is Settlement settlement && settlement.Faction != Faction.OfPlayer)
        {
            TaggedString letterLabel = "LetterLabelCaravanEnteredEnemyBase".Translate(); //Translation: ok
            TaggedString letterText = "PawnFlyer_LetterLandedInEnemyBase".Translate(transportShip.def.label.CapitalizeFirst(), settlement.Label).CapitalizeFirst();
            SettlementUtility.AffectRelationsOnAttacked(settlement, ref letterText);
            if (flag)
            {
                Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
                PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(orGenerateMap.mapPawns.AllPawns, ref letterLabel,
                    ref letterText,
                    "LetterRelatedPawnsInMapWherePlayerLanded".Translate(Faction.OfPlayer.def.pawnsPlural), //Translation: ok
                    informEvenIfSeenBefore: true);
            }

            Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NeutralEvent, lookTargets,
                settlement.Faction);
        }

        for (int i = 0; i < pods.Count; i++)
        {
            transportShip.TransporterComp.innerContainer.TryAddRangeOrTransfer(pods[i].innerContainer,
                canMergeWithExistingStacks: true, destroyLeftover: true);
        }

        transportShip.ArriveAt(cell, mapParent);
        Messages.Message("MessageShuttleArrived".Translate(), lookTargets, MessageTypeDefOf.TaskCompletion);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref transportShip, "transportShip");
        Scribe_References.Look(ref mapParent, "mapParent");
        Scribe_Values.Look(ref cell, "cell");
    }
}