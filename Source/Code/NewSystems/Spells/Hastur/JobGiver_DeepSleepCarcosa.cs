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
            return ownedBed != null ? RestUtility.GetBedSleepingSlotPosFor(pawn: pawn, bed: ownedBed) : pawn.Position;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            var forcedGotoPosition = GetBedRoot(pawn: pawn);
            if (!forcedGotoPosition.IsValid)
            {
                return null;
            }

            if (pawn.CanReach(dest: forcedGotoPosition, peMode: PathEndMode.ClosestTouch, maxDanger: Danger.Deadly))
            {
                return new Job(def: JobDefOf.LayDown, targetA: forcedGotoPosition)
                {
                    locomotionUrgency = LocomotionUrgency.Walk
                };
            }

            pawn.mindState.forcedGotoPosition = IntVec3.Invalid;
            return null;
        }
    }
}