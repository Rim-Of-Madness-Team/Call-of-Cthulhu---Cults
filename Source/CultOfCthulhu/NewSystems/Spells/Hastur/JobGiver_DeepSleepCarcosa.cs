using RimWorld;
using Verse;
using Verse.AI;

namespace CultOfCthulhu
{
    public class JobGiver_DeepSleepCarcosa : ThinkNode_JobGiver
    {
        private Building_Bed ownedBed;

        protected IntVec3 GetBedRoot(Pawn pawn)
        {
            ownedBed = pawn.ownership.OwnedBed;
            return ownedBed != null ? RestUtility.GetBedSleepingSlotPosFor(pawn, ownedBed) : pawn.Position;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            var forcedGotoPosition = GetBedRoot(pawn);
            if (!forcedGotoPosition.IsValid)
            {
                return null;
            }

            if (pawn.CanReach(forcedGotoPosition, PathEndMode.ClosestTouch, Danger.Deadly))
            {
                return new Job(JobDefOf.LayDown, forcedGotoPosition)
                {
                    locomotionUrgency = LocomotionUrgency.Walk
                };
            }

            pawn.mindState.forcedGotoPosition = IntVec3.Invalid;
            return null;
        }
    }
}