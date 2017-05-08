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
            this.wanderDestValidator = ((Pawn pawn, IntVec3 loc) => WanderRoomUtility.IsValidWanderDest(pawn, loc, this.GetWanderRoot(pawn)));
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
