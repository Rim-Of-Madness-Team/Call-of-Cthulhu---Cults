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


        protected Building_SacrificialAltar Altar => (Building_SacrificialAltar) job.GetTarget(ind: TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(ind: TargetIndex.A);
            yield return Toils_Reserve.Reserve(ind: TargetIndex.A);

            //Toil 1: Go to the pruning site.
            yield return Toils_Goto.GotoThing(ind: TargetIndex.A, peMode: PathEndMode.Touch);

            //Toil 2: Begin pruning.
            var toil = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = Altar.remainingDuration
            };
            toil.WithProgressBarToilDelay(ind: TargetIndex.A);
            toil.initAction = delegate { Altar.ticksToNextRepair = 80f; };
            toil.tickAction = delegate
            {
                var actor = pawn;
                actor.skills.Learn(sDef: SkillDefOf.Construction, xp: 0.5f);
                actor.skills.Learn(sDef: SkillDefOf.Plants, xp: 0.5f);
                var statValue = actor.GetStatValue(stat: StatDefOf.ConstructionSpeed);
                Altar.ticksToNextRepair -= statValue;
                if (!(Altar.ticksToNextRepair <= 0f))
                {
                    return;
                }

                Altar.ticksToNextRepair += 16f;
                TargetThingA.HitPoints++;
                TargetThingA.HitPoints = Mathf.Min(a: TargetThingA.HitPoints, b: TargetThingA.MaxHitPoints);
                //if (this.TargetThingA.HitPoints == this.TargetThingA.MaxHitPoints)
                //{
                //    actor.records.Increment(RecordDefOf.ThingsRepaired);
                //    actor.jobs.EndCurrentJob(JobCondition.Succeeded, true);
                //}
            };
            toil.WithEffect(effectDef: TargetThingA.def.repairEffect, ind: TargetIndex.A);
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            yield return toil;


            //Toil 3 Unreserve
            yield return Toils_Reserve.Release(ind: TargetIndex.A);

            //Toil 4: Transform the altar once again.
            yield return new Toil
            {
                initAction = PruneResult,
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }

        private void PruneResult()
        {
            Altar.NightmarePruned(pruner: pawn);
        }
    }
}