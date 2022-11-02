using Verse;
using Verse.AI;

namespace CultOfCthulhu
{
    public class JobGiver_Disillusioned : JobGiver_Wander
    {
        public JobGiver_Disillusioned()
        {
            wanderRadius = 7f;
            ticksBetweenWandersRange = new IntRange(min: 300, max: 600);
            locomotionUrgency = LocomotionUrgency.Amble;
            wanderDestValidator = WanderRoomUtility.IsValidWanderDest;
        }

        protected override IntVec3 GetWanderRoot(Pawn pawn)
        {
            return pawn.ownership.OwnedBed?.Position ?? pawn.Position;
        }
    }
}