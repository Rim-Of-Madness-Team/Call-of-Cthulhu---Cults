using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using RimWorld.Planet;

namespace CultOfCthulhu
{
	public class ByakheeArrivalAction_VisitSite : TransportPodsArrivalAction
	{
		private Site site;

		private PawnsArrivalModeDef arrivalMode;

		public ByakheeArrivalAction_VisitSite()
		{
		}

		public ByakheeArrivalAction_VisitSite(Site site, PawnsArrivalModeDef arrivalMode)
		{
			this.site = site;
			this.arrivalMode = arrivalMode;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look<Site>(refee: ref this.site, label: "site", saveDestroyedThings: false);
			Scribe_Defs.Look<PawnsArrivalModeDef>(value: ref this.arrivalMode, label: "arrivalMode");
		}


		public override bool ShouldUseLongEvent(List<ActiveDropPodInfo> pods, int tile)
		{
			return !this.site.HasMap;
		}

		public override void Arrived(List<ActiveDropPodInfo> pods, int tile)
		{
			Thing lookTarget = TransportPodsArrivalActionUtility.GetLookTarget(pods: pods);
			bool flag = !this.site.HasMap;
			Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(tile: this.site.Tile, size: this.site.PreferredMapSize, suggestedMapParentDef: null);
			if (flag)
			{
				Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
				PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(seenPawns: orGenerateMap.mapPawns.AllPawns, relationsInfoHeader: "LetterRelatedPawnsInMapWherePlayerLanded".Translate(arg1: Faction.OfPlayer.def.pawnsPlural), letterDef: LetterDefOf.NeutralEvent, informEvenIfSeenBefore: true, writeSeenPawnsNames: true);
			}
			if (this.site.Faction != null && this.site.Faction != Faction.OfPlayer)
			{
				Faction.OfPlayer.TryAffectGoodwillWith(other: this.site.Faction, goodwillChange: Faction.OfPlayer.GoodwillToMakeHostile(other: this.site.Faction), canSendMessage: true, canSendHostilityLetter: true, reason: HistoryEventDefOf.AttackedSettlement, lookTarget: null);
			}
			Messages.Message(text: "MessageTransportPodsArrived".Translate(), lookTargets: lookTarget, def: MessageTypeDefOf.TaskCompletion, historical: true);
			this.arrivalMode.Worker.TravelingTransportPodsArrived(dropPods: pods, map: orGenerateMap);
		}

	}
}
