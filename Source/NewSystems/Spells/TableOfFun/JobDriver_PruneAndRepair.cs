using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

namespace CultOfCthulhu
{
    public class JobDriver_PruneAndRepair : JobDriver
    {
        public static int remainingDuration = 20000; // 6 in-game hours

        private const float WarmupTicks = 80f;

        private const float TicksBetweenRepairs = 16f;

        protected float ticksToNextRepair;


        protected Building_SacrificialAltar Altar
        {
            get
            {
                return (Building_SacrificialAltar)base.CurJob.GetTarget(TargetIndex.A).Thing;
            }
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Reserve.Reserve(TargetIndex.A, 1);

            //Toil 1: Go to the pruning site.
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            //Toil 2: Begin pruning.
            Toil toil = new Toil();
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = JobDriver_PruneAndRepair.remainingDuration;
            toil.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            toil.initAction = delegate
            {
                this.ticksToNextRepair = 80f;
            };
            toil.tickAction = delegate
            {
                Pawn actor = this.pawn;
                actor.skills.Learn(SkillDefOf.Construction, 0.5f, false);
                actor.skills.Learn(SkillDefOf.Growing, 0.5f, false);
                float statValue = actor.GetStatValue(StatDefOf.ConstructionSpeed, true);
                this.ticksToNextRepair -= statValue;
                if (this.ticksToNextRepair <= 0f)
                {
                    this.ticksToNextRepair += 16f;
                    this.TargetThingA.HitPoints++;
                    this.TargetThingA.HitPoints = Mathf.Min(this.TargetThingA.HitPoints, this.TargetThingA.MaxHitPoints);
                    //if (this.TargetThingA.HitPoints == this.TargetThingA.MaxHitPoints)
                    //{
                    //    actor.records.Increment(RecordDefOf.ThingsRepaired);
                    //    actor.jobs.EndCurrentJob(JobCondition.Succeeded, true);
                    //}
                }
            };
            toil.WithEffect(base.TargetThingA.def.repairEffect, TargetIndex.A);
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            yield return toil;


            //Toil 3 Unreserve
            yield return Toils_Reserve.Release(TargetIndex.A);

            //Toil 4: Transform the altar once again.
            yield return new Toil
            {
                initAction = delegate
                {
                    PruneResult(); 
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };


            yield break;
        }

        public void PruneResult()
        {
            Altar.NightmarePruned(this.pawn);
        }
    }
}
