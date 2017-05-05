using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;

namespace CultOfCthulhu
{
    public class LordToil_DefendAndExpandWomb : LordToil
    {
        public float distToHiveToAttack = 10f;

        private LordToilData_DefendAndExpandWomb Data
        {
            get
            {
                return (LordToilData_DefendAndExpandWomb)this.data;
            }
        }

        public LordToil_DefendAndExpandWomb()
        {
            this.data = new LordToilData_DefendAndExpandWomb();
        }

        public override void UpdateAllDuties()
        {
            this.FilterOutUnspawnedWombs();
            for (int i = 0; i < this.lord.ownedPawns.Count; i++)
            {
                WombBetweenWorlds hiveFor = this.GetHiveFor(this.lord.ownedPawns[i]);
                PawnDuty duty = new PawnDuty(CultDefOfs.DefendAndExpandWomb, hiveFor, this.distToHiveToAttack);
                this.lord.ownedPawns[i].mindState.duty = duty;
            }
        }

        private void FilterOutUnspawnedWombs()
        {
            this.Data.assignedHives.RemoveAll((KeyValuePair<Pawn, WombBetweenWorlds> x) => x.Value == null || !x.Value.Spawned);
        }

        private WombBetweenWorlds GetHiveFor(Pawn pawn)
        {
            WombBetweenWorlds hive;
            if (this.Data.assignedHives.TryGetValue(pawn, out hive))
            {
                return hive;
            }
            hive = this.FindClosestHive(pawn);
            if (hive != null)
            {
                this.Data.assignedHives.Add(pawn, hive);
            }
            return hive;
        }

        private WombBetweenWorlds FindClosestHive(Pawn pawn)
        {
            return (WombBetweenWorlds)GenClosest.ClosestThingReachable(pawn.Position, ThingRequest.ForDef(ThingDefOf.Hive), PathEndMode.Touch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 30f, (Thing x) => x.Faction == pawn.Faction, null, 30, false);
        }
    }
}
