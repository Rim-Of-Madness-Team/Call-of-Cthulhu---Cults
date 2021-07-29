using System.Collections.Generic;
using System.Diagnostics;
using RimWorld;
using UnityEngine;
using Verse.AI;

namespace CultOfCthulhu
{
    public class JobDriver_PruneAndRepair : JobDriver
    {
        private const float WarmupTicks = 80f;

        //public static int remainingDuration = 20000; // 6 in-game hours


        protected Building_SacrificialAltar Altar => (Building_SacrificialAltar) job.GetTarget(TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Reserve.Reserve(TargetIndex.A);

            //Toil 1: Go to the pruning site.
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            //Toil 2: Begin pruning.
            var toil = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = Altar.remainingDuration
            };
            toil.WithProgressBarToilDelay(TargetIndex.A);
            toil.initAction = delegate { Altar.ticksToNextRepair = 80f; };
            toil.tickAction = delegate
            {
                var actor = pawn;
                actor.skills.Learn(SkillDefOf.Construction, 0.5f);
                actor.skills.Learn(SkillDefOf.Plants, 0.5f);
                var statValue = actor.GetStatValue(StatDefOf.ConstructionSpeed);
                Altar.ticksToNextRepair -= statValue;
                if (!(Altar.ticksToNextRepair <= 0f))
                {
                    return;
                }

                Altar.ticksToNextRepair += 16f;
                TargetThingA.HitPoints++;
                TargetThingA.HitPoints = Mathf.Min(TargetThingA.HitPoints, TargetThingA.MaxHitPoints);
                //if (this.TargetThingA.HitPoints == this.TargetThingA.MaxHitPoints)
                //{
                //    actor.records.Increment(RecordDefOf.ThingsRepaired);
                //    actor.jobs.EndCurrentJob(JobCondition.Succeeded, true);
                //}
            };
            toil.WithEffect(TargetThingA.def.repairEffect, TargetIndex.A);
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            yield return toil;


            //Toil 3 Unreserve
            yield return Toils_Reserve.Release(TargetIndex.A);

            //Toil 4: Transform the altar once again.
            yield return new Toil
            {
                initAction = PruneResult,
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }

        private void PruneResult()
        {
            Altar.NightmarePruned(pawn);
        }
    }
}