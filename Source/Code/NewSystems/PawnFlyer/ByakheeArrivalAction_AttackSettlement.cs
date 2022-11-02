using System;
using System.Collections.Generic;
using Verse;
using RimWorld.Planet;
using RimWorld;

namespace CultOfCthulhu
{
    public class ByakheeArrivalAction_AttackSettlement : TransportPodsArrivalAction
    {
        private Settlement settlement;

        private PawnsArrivalModeDef arrivalMode;

        public ByakheeArrivalAction_AttackSettlement()
        {
        }

        public ByakheeArrivalAction_AttackSettlement(Settlement settlement, PawnsArrivalModeDef arrivalMode)
        {
            this.settlement = settlement;
            this.arrivalMode = arrivalMode;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Settlement>(refee: ref this.settlement, label: "settlement", saveDestroyedThings: false);
            Scribe_Defs.Look<PawnsArrivalModeDef>(value: ref this.arrivalMode, label: "arrivalMode");
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
            return ByakheeArrivalAction_AttackSettlement.CanAttack(pods: pods, settlement: this.settlement);
        }

        public override bool ShouldUseLongEvent(List<ActiveDropPodInfo> pods, int tile)
        {
            return !this.settlement.HasMap;
        }

        public override void Arrived(List<ActiveDropPodInfo> pods, int tile)
        {
            Thing lookTarget = ByakheeArrivalActionUtility.GetLookTarget(pods: pods);
            bool flag = !this.settlement.HasMap;
            Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(tile: this.settlement.Tile, suggestedMapParentDef: null);
            TaggedString label = "LetterLabelCaravanEnteredEnemyBase".Translate();
            TaggedString text = "LetterTransportPodsLandedInEnemyBase".Translate(arg1: this.settlement.Label).CapitalizeFirst();
            SettlementUtility.AffectRelationsOnAttacked(mapParent: this.settlement, letterText: ref text);
            if (flag)
            {
                Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
                PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(seenPawns: orGenerateMap.mapPawns.AllPawns, letterLabel: ref label, letterText: ref text, relationsInfoHeader: "LetterRelatedPawnsInMapWherePlayerLanded".Translate(arg1: Faction.OfPlayer.def.pawnsPlural), informEvenIfSeenBefore: true, writeSeenPawnsNames: true);
            }
            Find.LetterStack.ReceiveLetter(label: label, text: text, textLetterDef: LetterDefOf.NeutralEvent, lookTargets: lookTarget, relatedFaction: this.settlement.Faction, quest: null, hyperlinkThingDefs: null, debugInfo: null);
            this.arrivalMode.Worker.TravelingTransportPodsArrived(dropPods: pods, map: orGenerateMap);
        }

        public static FloatMenuAcceptanceReport CanAttack(IEnumerable<IThingHolder> pods, Settlement settlement)
        {
            if (settlement == null || !settlement.Spawned || !settlement.Attackable)
            {
                return false;
            }
            if (!ByakheeArrivalActionUtility.AnyNonDownedColonist(pods: pods))
            {
                return false;
            }
            if (settlement.EnterCooldownBlocksEntering())
            {
                return FloatMenuAcceptanceReport.WithFailReasonAndMessage(failReason: "EnterCooldownBlocksEntering".Translate(), failMessage: "MessageEnterCooldownBlocksEntering".Translate(arg1: settlement.EnterCooldownTicksLeft().ToStringTicksToPeriod(allowSeconds: true, shortForm: false, canUseDecimals: true, allowYears: true)));
            }
            return true;
        }


        private static ByakheeArrivalAction_AttackSettlement arrivalActionEdgeDrop(Settlement settlement)
        {
            return new ByakheeArrivalAction_AttackSettlement(settlement: settlement, arrivalMode: PawnsArrivalModeDefOf.EdgeDrop);
        }

        private static ByakheeArrivalAction_AttackSettlement arrivalActionCenterDrop(Settlement settlement)
        {
            return new ByakheeArrivalAction_AttackSettlement(settlement: settlement, arrivalMode: PawnsArrivalModeDefOf.CenterDrop);
        }

        public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(CompLaunchablePawn representative, IEnumerable<IThingHolder> pods, Settlement settlement)
        {
            if (ByakheeArrivalAction_AttackSettlement.CanAttack(pods: pods, settlement: settlement))
            {
                yield break;
            }

            Func<FloatMenuAcceptanceReport> acceptanceReportGetter = new Func<FloatMenuAcceptanceReport>(() => CanAttack(pods: pods, settlement: settlement));
            Func<ByakheeArrivalAction_AttackSettlement> dropAtEdge = new Func<ByakheeArrivalAction_AttackSettlement>(() => arrivalActionEdgeDrop(settlement: settlement));

            Func<FloatMenuAcceptanceReport> idunno = null;
            foreach (FloatMenuOption floatMenuOption in ByakheeArrivalActionUtility.GetFloatMenuOptions<ByakheeArrivalAction_AttackSettlement>(acceptanceReportGetter: acceptanceReportGetter, arrivalActionGetter: dropAtEdge, label: "AttackAndDropAtEdge".Translate(arg1: settlement.Label), representative: representative, destinationTile: settlement.Tile, uiConfirmationCallback: null))
            {
                yield return floatMenuOption;
            }


            Func<ByakheeArrivalAction_AttackSettlement> dropAtCenter = new Func<ByakheeArrivalAction_AttackSettlement>(() => arrivalActionCenterDrop(settlement: settlement));

            foreach (FloatMenuOption floatMenuOption2 in ByakheeArrivalActionUtility.GetFloatMenuOptions<ByakheeArrivalAction_AttackSettlement>(acceptanceReportGetter: acceptanceReportGetter, arrivalActionGetter: dropAtCenter, label: "AttackAndDropInCenter".Translate(arg1: settlement.Label), representative: representative, destinationTile: settlement.Tile, uiConfirmationCallback: null))
            {
                yield return floatMenuOption2;
            }
            yield break;
        }

    }
}
