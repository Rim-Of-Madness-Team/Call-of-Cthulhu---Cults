using System;
using Verse;
using Verse.AI;
using RimWorld;

namespace CultOfCthulhu
{
    public class JobGiver_DeepSleepCarcosa : ThinkNode_JobGiver
    {
        Building_Bed ownedBed;

        protected IntVec3 GetBedRoot(Pawn pawn)
        {
            ownedBed = pawn.ownership.OwnedBed;
            if (ownedBed != null)
            {
                return RestUtility.GetBedSleepingSlotPosFor(pawn, ownedBed);
            }
            return pawn.Position;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            IntVec3 forcedGotoPosition = GetBedRoot(pawn);
            if (!forcedGotoPosition.IsValid)
            {
                return null;
            }
            if (!pawn.CanReach(forcedGotoPosition, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn))
            {
                pawn.mindState.forcedGotoPosition = IntVec3.Invalid;
                return null;
            }
            return new Job(JobDefOf.LayDown, forcedGotoPosition)
            {
                locomotionUrgency = LocomotionUrgency.Walk
            };
        }
    }
}
