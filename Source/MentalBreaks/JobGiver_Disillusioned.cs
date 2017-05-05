using System;
using Verse;
using Verse.AI;
using RimWorld;

namespace CultOfCthulhu
{
    public class JobGiver_Disillusioned : JobGiver_Wander
    {
        Building_Bed ownedBed;

        public JobGiver_Disillusioned()
        {
            this.wanderRadius = 7f;
            this.ticksBetweenWandersRange = new IntRange(300, 600);
            this.locomotionUrgency = LocomotionUrgency.Amble;
            this.wanderDestValidator = delegate (Pawn pawn, IntVec3 loc)
            {
                IntVec3 wanderRoot = this.GetWanderRoot(pawn);
                Room room = wanderRoot.GetRoom(pawn.Map);
                return room == null || room.IsDoor || WanderUtility.InSameRoom(wanderRoot, loc, pawn.Map);
            };
        }
        
        protected override IntVec3 GetWanderRoot(Pawn pawn)
        {
            if (pawn.ownership.OwnedBed != null)
            {
                return pawn.ownership.OwnedBed.Position;
            }
            return pawn.Position;
        }
    }
}
