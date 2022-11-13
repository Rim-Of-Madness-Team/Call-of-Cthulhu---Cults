// RimWorld.Planet.TransportPodsArrivalAction_AttackSettlement

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;
using Verse;

namespace CultOfCthulhu;

public class PawnFlyerArrivalAction_AttackSettlement : PawnFlyerArrivalAction
{
    private Settlement settlement;

    private PawnsArrivalModeDef arrivalMode;

    public PawnFlyerArrivalAction_AttackSettlement()
    {
    }

    public PawnFlyerArrivalAction_AttackSettlement(Settlement settlement, PawnsArrivalModeDef arrivalMode)
    {
        this.settlement = settlement;
        this.arrivalMode = arrivalMode;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref settlement, "settlement");
        Scribe_Defs.Look(ref arrivalMode, "arrivalMode");
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

        return CanAttack(pods, settlement);
    }

    public override bool ShouldUseLongEvent(List<ActiveDropPodInfo> pods, int tile)
    {
        return !settlement.HasMap;
    }

    public override void Arrived(List<ActiveDropPodInfo> pods, int tile)
    {
        Thing lookTarget = PawnFlyerArrivalActionUtility.GetLookTarget(pods);
        bool num = !settlement.HasMap;
        Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(settlement.Tile, null);
        TaggedString letterLabel = "LetterLabelCaravanEnteredEnemyBase".Translate();
        TaggedString letterText = "PawnFlyer_LetterLandedInEnemyBase".Translate(settlement.Label).CapitalizeFirst();
        SettlementUtility.AffectRelationsOnAttacked(settlement, ref letterText);
        if (num)
        {
            Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
            PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(orGenerateMap.mapPawns.AllPawns, ref letterLabel,
                ref letterText, "LetterRelatedPawnsInMapWherePlayerLanded".Translate(Faction.OfPlayer.def.pawnsPlural),
                informEvenIfSeenBefore: true);
        }

        Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NeutralEvent, lookTarget,
            settlement.Faction);

        TravelingPawnFlyersArrived(pods, orGenerateMap, arrivalMode);
    }

    public static void TravelingPawnFlyersArrived(List<ActiveDropPodInfo> dropPods, Map map, PawnsArrivalModeDef arrivalMode)
    {
        IntVec3 intVec = IntVec3.Zero;
        if (arrivalMode == PawnsArrivalModeDefOf.EdgeDrop)
            intVec = DropCellFinder.FindRaidDropCenterDistant(map, false);
        if (arrivalMode == PawnsArrivalModeDefOf.CenterDrop)
            DropCellFinder.TryFindRaidDropCenterClose(out intVec, map, false);
        if (intVec == IntVec3.Zero)
            intVec = DropCellFinder.FindRaidDropCenterDistant(map, false);
        DropTravelingPawnFlyers(dropPods, intVec, map);
    }

    public static void DropTravelingPawnFlyers(List<ActiveDropPodInfo> dropPods, IntVec3 near, Map map)
    {
        PawnFlyerArrivalActionUtility.RemovePawnsFromWorldPawns(dropPods);
        for (int i = 0; i < dropPods.Count; i++)
        {
            IntVec3 intVec;
            DropCellFinder.TryFindDropSpotNear(center: near, map: map, result: out intVec, allowFogged: false,
                canRoofPunch: true, allowIndoors: true, size: null, mustBeReachableFromCenter: true);
            PawnFlyer flyer = (PawnFlyer)dropPods[i].innerContainer.FirstOrDefault(x => x.def is PawnFlyerDef);
            if (flyer != null)
                PawnFlyerArrivalActionUtility.MakeIncomingPawnFlyerAt(flyer, intVec, map, dropPods[i], null);
            else
            {
                Log.Error("No pawn flyer found in drop pod info");
            }
        }
    }

    public static FloatMenuAcceptanceReport CanAttack(IEnumerable<IThingHolder> pods, Settlement settlement)
    {
        if (settlement == null || !settlement.Spawned || !settlement.Attackable)
        {
            return false;
        }

        if (!PawnFlyerArrivalActionUtility.AnyNonDownedColonist(pods))
        {
            return false;
        }

        if (settlement.EnterCooldownBlocksEntering())
        {
            return FloatMenuAcceptanceReport.WithFailReasonAndMessage("EnterCooldownBlocksEntering".Translate(),
                "MessageEnterCooldownBlocksEntering".Translate(settlement.EnterCooldownTicksLeft()
                    .ToStringTicksToPeriod()));
        }

        return true;
    }

    public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(CompLaunchablePawn representative,
        IEnumerable<IThingHolder> pods, Settlement settlement)
    {
        foreach (FloatMenuOption floatMenuOption in PawnFlyerArrivalActionUtility.GetFloatMenuOptions(
                     () => CanAttack(pods, settlement),
                     () => new PawnFlyerArrivalAction_AttackSettlement(settlement, PawnsArrivalModeDefOf.EdgeDrop),
                     "PawnFlyer_AttackAndLandAtEdge".Translate(settlement.Label), representative, settlement.Tile))
        {
            yield return floatMenuOption;
        }

        foreach (FloatMenuOption floatMenuOption2 in PawnFlyerArrivalActionUtility.GetFloatMenuOptions(
                     () => CanAttack(pods, settlement),
                     () => new PawnFlyerArrivalAction_AttackSettlement(settlement,
                         PawnsArrivalModeDefOf.CenterDrop), "PawnFlyer_AttackAndLandInCenter".Translate(settlement.Label),
                     representative, settlement.Tile))
        {
            yield return floatMenuOption2;
        }
    }
}