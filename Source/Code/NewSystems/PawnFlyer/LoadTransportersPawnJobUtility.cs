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
            var list = new List<Pawn>(collection: listSel);
            foreach (var pawn in list)
            {
                var compTransporter = pawn.TryGetComp<CompTransporterPawn>();
                if (compTransporter.groupID != transportersGroup)
                {
                    continue;
                }

                Utility.DebugReport(x: "Outlist Added: " + pawn.Label);
                outTransporters.Add(item: compTransporter);
            }
        }


        public static Job JobOnTransporter(Pawn p, CompTransporterPawn transporter)
        {
            Utility.DebugReport(x: "JobOnTransporter Called");
            var thing = FindThingToLoad(p: p, transporter: transporter);
            return new Job(def: JobDefOf.HaulToContainer, targetA: thing, targetB: transporter.parent)
            {
                count = Mathf.Min(
                    a: TransferableUtility.TransferableMatching(thing: thing, transferables: transporter.leftToLoad,
                        mode: TransferAsOneMode.PodsOrCaravanPacking).CountToTransfer, b: thing.stackCount),
                ignoreForbidden = true
            };
        }

        // RimWorld.LoadTransportersJobUtility
        public static bool HasJobOnTransporter(Pawn pawn, CompTransporterPawn transporter)
        {
            var result = !transporter.parent.IsForbidden(pawn: pawn) && transporter.AnythingLeftToLoad &&
                         pawn.health.capacities.CapableOf(capacity: PawnCapacityDefOf.Manipulation) &&
                         pawn.CanReserveAndReach(target: transporter.parent, peMode: PathEndMode.Touch, maxDanger: pawn.NormalMaxDanger()) &&
                         FindThingToLoad(p: pawn, transporter: transporter) != null;
            Utility.DebugReport(x: pawn.Label + " HasJobOnTransporter: " + result);
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
                        neededThings.Add(item: item);
                    }
                }
            }

            if (!neededThings.Any())
            {
                return null;
            }

            bool validator(Thing x)
            {
                return neededThings.Contains(item: x) && p.CanReserve(target: x);
            }

            var thing = GenClosest.ClosestThingReachable(root: p.Position, map: p.Map,
                thingReq: ThingRequest.ForGroup(@group: ThingRequestGroup.HaulableEver), peMode: PathEndMode.Touch,
                traverseParams: TraverseParms.For(pawn: p), maxDistance: 9999f, validator: validator);
            if (thing == null)
            {
                foreach (var current in neededThings)
                {
                    if (current is not Pawn pawn || pawn.IsColonist && !pawn.Downed ||
                        !p.CanReserveAndReach(target: pawn, peMode: PathEndMode.Touch, maxDanger: Danger.Deadly))
                    {
                        continue;
                    }

                    Utility.DebugReport(x: "Pawn to load : " + pawn.Label);
                    return pawn;
                }
            }

            if (thing != null)
            {
                Utility.DebugReport(x: "Thing to load : " + thing.Label);
            }

            neededThings.Clear();
            return thing;
        }
    }
}