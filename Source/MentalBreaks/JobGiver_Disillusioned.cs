using System;
using Verse;
using Verse.AI;
using RimWorld;

namespace CultOfCthulhu
{
    public class JobGiver_Disillusioned : JobGiver_Wander
    {
        public JobGiver_Disillusioned()
        {
            this.wanderRadius = 7f;
            this.ticksBetweenWandersRange = new IntRange(300, 600);
            this.locomotionUrgency = LocomotionUrgency.Amble;
            this.wanderDestValidator = (WanderRoomUtility.IsValidWanderDest);
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
