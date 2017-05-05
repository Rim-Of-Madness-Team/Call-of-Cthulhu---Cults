using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CultOfCthulhu
{
    public static class LoadTransportersPawnJobUtility
    {

        private static HashSet<Thing> neededThings = new HashSet<Thing>();


        // RimWorld.TransporterUtility
        public static void GetTransportersInGroup(int transportersGroup, Map map, List<CompTransporterPawn> outTransporters)
        {
            outTransporters.Clear();
            if (transportersGroup < 0)
            {
                return;
            }
            IEnumerable<Pawn> listSel = from Pawn pawns in map.mapPawns.AllPawnsSpawned
                                        where pawns is PawnFlyer
                                        select pawns;
            List<Pawn> list = new List<Pawn>(listSel);
            for (int i = 0; i < list.Count; i++)
            {
                CompTransporterPawn compTransporter = list[i].TryGetComp<CompTransporterPawn>();
                if (compTransporter.groupID == transportersGroup)
                {
                    Cthulhu.Utility.DebugReport("Outlist Added: " + list[i].Label);
                    outTransporters.Add(compTransporter);
                }
            }
        }



        public static Job JobOnTransporter(Pawn p, CompTransporterPawn transporter)
        {
            Cthulhu.Utility.DebugReport("JobOnTransporter Called");
            Thing thing = LoadTransportersPawnJobUtility.FindThingToLoad(p, transporter);
            return new Job(JobDefOf.HaulToContainer, thing, transporter.parent)
            {
                count = Mathf.Min(TransferableUtility.TransferableMatching<TransferableOneWay>(thing, transporter.leftToLoad).countToTransfer, thing.stackCount),
                ignoreForbidden = true
            };
        }
        // RimWorld.LoadTransportersJobUtility
        public static bool HasJobOnTransporter(Pawn pawn, CompTransporterPawn transporter)
        {
            bool result = !transporter.parent.IsForbidden(pawn) && transporter.AnythingLeftToLoad && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) && pawn.CanReserveAndReach(transporter.parent, PathEndMode.Touch, pawn.NormalMaxDanger(), 1) && LoadTransportersPawnJobUtility.FindThingToLoad(pawn, transporter) != null;
            Cthulhu.Utility.DebugReport(pawn.Label + " HasJobOnTransporter: " + result.ToString());
            return result;
        }

        // RimWorld.LoadTransportersJobUtility
        private static Thing FindThingToLoad(Pawn p, CompTransporterPawn transporter)
        {
            LoadTransportersPawnJobUtility.neededThings.Clear();
            List<TransferableOneWay> leftToLoad = transporter.leftToLoad;
            if (leftToLoad != null)
            {
                for (int i = 0; i < leftToLoad.Count; i++)
                {
                    TransferableOneWay transferableOneWay = leftToLoad[i];
                    if (transferableOneWay.countToTransfer > 0)
                    {
                        for (int j = 0; j < transferableOneWay.things.Count; j++)
                        {
                            LoadTransportersPawnJobUtility.neededThings.Add(transferableOneWay.things[j]);
                        }
                    }
                }
            }
            if (!LoadTransportersPawnJobUtility.neededThings.Any<Thing>())
            {
                return null;
            }
            Predicate<Thing> validator = (Thing x) => LoadTransportersPawnJobUtility.neededThings.Contains(x) && p.CanReserve(x, 1);
            Thing thing = GenClosest.ClosestThingReachable(p.Position, p.Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), PathEndMode.Touch, TraverseParms.For(p, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, -1, false);
            if (thing == null)
            {
                foreach (Thing current in LoadTransportersPawnJobUtility.neededThings)
                {
                    Pawn pawn = current as Pawn;
                    if (pawn != null && (!pawn.IsColonist || pawn.Downed) && p.CanReserveAndReach(pawn, PathEndMode.Touch, Danger.Deadly, 1))
                    {
                        Cthulhu.Utility.DebugReport("Pawn to load : " + pawn.Label);
                        return pawn;
                    }
                }
            }
            if (thing != null) Cthulhu.Utility.DebugReport("Thing to load : " + thing.Label);
            LoadTransportersPawnJobUtility.neededThings.Clear();
            return thing;
        }
    }
}
