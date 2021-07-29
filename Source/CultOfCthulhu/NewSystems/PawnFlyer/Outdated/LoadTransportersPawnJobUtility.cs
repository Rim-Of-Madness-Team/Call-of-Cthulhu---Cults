using System.Collections.Generic;
using System.Linq;
using Cthulhu;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CultOfCthulhu
{
    public static class LoadTransportersPawnJobUtility
    {
        private static readonly HashSet<Thing> neededThings = new HashSet<Thing>();


        // RimWorld.TransporterUtility
        public static void GetTransportersInGroup(int transportersGroup, Map map,
            List<CompTransporterPawn> outTransporters)
        {
            outTransporters.Clear();
            if (transportersGroup < 0)
            {
                return;
            }

            var listSel = from Pawn pawns in map.mapPawns.AllPawnsSpawned
                where pawns is PawnFlyer
                select pawns;
            var list = new List<Pawn>(listSel);
            foreach (var pawn in list)
            {
                var compTransporter = pawn.TryGetComp<CompTransporterPawn>();
                if (compTransporter.groupID != transportersGroup)
                {
                    continue;
                }

                Utility.DebugReport("Outlist Added: " + pawn.Label);
                outTransporters.Add(compTransporter);
            }
        }


        public static Job JobOnTransporter(Pawn p, CompTransporterPawn transporter)
        {
            Utility.DebugReport("JobOnTransporter Called");
            var thing = FindThingToLoad(p, transporter);
            return new Job(JobDefOf.HaulToContainer, thing, transporter.parent)
            {
                count = Mathf.Min(
                    TransferableUtility.TransferableMatching(thing, transporter.leftToLoad,
                        TransferAsOneMode.PodsOrCaravanPacking).CountToTransfer, thing.stackCount),
                ignoreForbidden = true
            };
        }

        // RimWorld.LoadTransportersJobUtility
        public static bool HasJobOnTransporter(Pawn pawn, CompTransporterPawn transporter)
        {
            var result = !transporter.parent.IsForbidden(pawn) && transporter.AnythingLeftToLoad &&
                         pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) &&
                         pawn.CanReserveAndReach(transporter.parent, PathEndMode.Touch, pawn.NormalMaxDanger()) &&
                         FindThingToLoad(pawn, transporter) != null;
            Utility.DebugReport(pawn.Label + " HasJobOnTransporter: " + result);
            return result;
        }

        // RimWorld.LoadTransportersJobUtility
        private static Thing FindThingToLoad(Pawn p, CompTransporterPawn transporter)
        {
            neededThings.Clear();
            var leftToLoad = transporter.leftToLoad;
            if (leftToLoad != null)
            {
                foreach (var transferableOneWay in leftToLoad)
                {
                    if (transferableOneWay.CountToTransfer <= 0)
                    {
                        continue;
                    }

                    foreach (var item in transferableOneWay.things)
                    {
                        neededThings.Add(item);
                    }
                }
            }

            if (!neededThings.Any())
            {
                return null;
            }

            bool validator(Thing x)
            {
                return neededThings.Contains(x) && p.CanReserve(x);
            }

            var thing = GenClosest.ClosestThingReachable(p.Position, p.Map,
                ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), PathEndMode.Touch,
                TraverseParms.For(p), 9999f, validator);
            if (thing == null)
            {
                foreach (var current in neededThings)
                {
                    if (current is not Pawn pawn || pawn.IsColonist && !pawn.Downed ||
                        !p.CanReserveAndReach(pawn, PathEndMode.Touch, Danger.Deadly))
                    {
                        continue;
                    }

                    Utility.DebugReport("Pawn to load : " + pawn.Label);
                    return pawn;
                }
            }

            if (thing != null)
            {
                Utility.DebugReport("Thing to load : " + thing.Label);
            }

            neededThings.Clear();
            return thing;
        }
    }
}