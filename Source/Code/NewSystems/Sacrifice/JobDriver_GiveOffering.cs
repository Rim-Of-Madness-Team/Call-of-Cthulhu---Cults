using System;
using System.Collections.Generic;
using System.Diagnostics;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CultOfCthulhu
{
    public class JobDriver_GiveOffering : JobDriver
    {
        public const TargetIndex BillGiverInd = TargetIndex.A;

        public const TargetIndex IngredientInd = TargetIndex.B;

        public const TargetIndex IngredientPlaceCellInd = TargetIndex.C;

        public int billStartTick;

        public List<Thing> offerings;

        public int ticksSpentDoingRecipeWork;

        public float workLeft;

        public IBillGiver BillGiver => !(pawn.jobs.curJob.GetTarget(ind: TargetIndex.A).Thing is IBillGiver billGiver)
            ? throw new InvalidOperationException(message: "DoBill on non-Billgiver.")
            : billGiver;

        public Building_SacrificialAltar DropAltar =>
            !(pawn.jobs.curJob.GetTarget(ind: TargetIndex.A).Thing is Building_SacrificialAltar result)
                ? throw new InvalidOperationException(message: "Altar is missing.")
                : result;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        public override string GetReport()
        {
            return pawn.jobs.curJob.RecipeDef != null
                ? ReportStringProcessed(str: pawn.jobs.curJob.RecipeDef.jobString)
                : base.GetReport();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(value: ref workLeft, label: "workLeft");
            Scribe_Collections.Look(list: ref offerings, label: "offerings", lookMode: LookMode.Reference);
            Scribe_Values.Look(value: ref billStartTick, label: "billStartTick");
            Scribe_Values.Look(value: ref ticksSpentDoingRecipeWork, label: "ticksSpentDoingRecipeWork");
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            AddEndCondition(newEndCondition: delegate
            {
                var thing = GetActor().jobs.curJob.GetTarget(ind: TargetIndex.A).Thing;
                return thing is Building && !thing.Spawned ? JobCondition.Incompletable : JobCondition.Ongoing;
            });
            this.FailOnBurningImmobile(ind: TargetIndex.A);
            //this.FailOn(delegate
            //{
            //    IBillGiver billGiver = this.pawn.jobs.curJob.GetTarget(TargetIndex.A).Thing as IBillGiver;
            //    if (billGiver != null)
            //    {
            //        if (this.pawn.jobs.curJob.bill.DeletedOrDereferenced)
            //        {
            //            return true;
            //        }
            //        if (!billGiver.CurrentlyUsable())
            //        {
            //            return true;
            //        }
            //    }
            //    return false;
            //});
            //yield return ToilLogMessage("Pass 0 - Start");

            var toil = Toils_Goto.GotoThing(ind: TargetIndex.A, peMode: PathEndMode.InteractionCell);
            yield return new Toil {initAction = delegate { offerings = new List<Thing>(); }};
            yield return Toils_Reserve.Reserve(ind: TargetIndex.A);
            //yield return new Toil {initAction = delegate {Log.Message("Pass 2");}};
            yield return Toils_Reserve.ReserveQueue(ind: TargetIndex.B);
            //yield return new Toil {initAction = delegate {Log.Message("Pass 3");}};
            yield return new Toil
            {
                initAction = delegate
                {
                    if (job.targetQueueB == null || job.targetQueueB.Count != 1)
                    {
                        return;
                    }

                    if (job.targetQueueB[index: 0].Thing is UnfinishedThing unfinishedThing)
                    {
                        unfinishedThing.BoundBill = (Bill_ProductionWithUft) job.bill;
                    }
                }
            };
            //yield return new Toil {initAction = delegate {Log.Message("Pass 4");}};
            yield return Toils_Jump.JumpIf(jumpTarget: toil, condition: () => job.GetTargetQueue(ind: TargetIndex.B).NullOrEmpty());
            //yield return new Toil {initAction = delegate {Log.Message("Pass 5");}};
            var toil2 = Toils_JobTransforms.ExtractNextTargetFromQueue(ind: TargetIndex.B, failIfCountFromQueueTooBig: false);
            yield return toil2;
            //yield return new Toil {initAction = delegate {Log.Message("Pass 6");}};            
            var toil3 = Toils_Goto.GotoThing(ind: TargetIndex.B, peMode: PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden(ind: TargetIndex.B).FailOnSomeonePhysicallyInteracting(ind: TargetIndex.B);
            yield return toil3;
            //yield return new Toil {initAction = delegate {Log.Message("Pass 7");}};
            yield return Toils_Haul.StartCarryThing(haulableInd: TargetIndex.B, putRemainderInQueue: true);
            //yield return new Toil {initAction = delegate {Log.Message("Pass 8");}};
            yield return JumpToCollectNextIntoHandsForBill(gotoGetTargetToil: toil3, ind: TargetIndex.B);
            //yield return new Toil {initAction = delegate {Log.Message("Pass 9");}};
            yield return Toils_Goto.GotoThing(ind: TargetIndex.A, peMode: PathEndMode.ClosestTouch)
                .FailOnDestroyedOrNull(ind: TargetIndex.B);
            //yield return new Toil {initAction = delegate {Log.Message("Pass 10");}};
            var toil4 = Toils_JobTransforms.SetTargetToIngredientPlaceCell(facilityInd: TargetIndex.A, carryItemInd: TargetIndex.B, cellTargetInd: TargetIndex.C);
            yield return toil4;
            //yield return new Toil {initAction = delegate {Log.Message("Pass 11");}};
            yield return Toils_Haul.PlaceHauledThingInCell(cellInd: TargetIndex.C, nextToilOnPlaceFailOrIncomplete: toil4, storageMode: false);
            //yield return new Toil {initAction = delegate {Log.Message("Pass 12");}};
            yield return new Toil
            {
                initAction = delegate
                {
                    if (offerings.Count > 0)
                    {
                        offerings.RemoveAll(match: x => x.DestroyedOrNull());
                    }

                    offerings.Add(item: TargetB.Thing);
                }
            };
            yield return Toils_Jump.JumpIfHaveTargetInQueue(ind: TargetIndex.B, jumpToil: toil2);
            yield return toil;
            //yield return ToilLogMessage("Pass 13");
            var chantingTime = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = CultUtility.ritualDuration
            };
            chantingTime.WithProgressBarToilDelay(ind: TargetIndex.A);
            chantingTime.PlaySustainerOrSound(soundDef: CultsDefOf.RitualChanting);
            var deitySymbol = ((CosmicEntityDef) DropAltar.currentOfferingDeity.def).Symbol;
            chantingTime.initAction = delegate
            {
                if (deitySymbol != null)
                {
                    MoteMaker.MakeInteractionBubble(initiator: pawn, recipient: null, interactionMote: ThingDefOf.Mote_Speech, symbol: deitySymbol);
                }
            };
            yield return chantingTime;
            //yield return ToilLogMessage("Pass 14");
            //Toil 8: Execution of Prisoner
            yield return new Toil
            {
                initAction = delegate
                {
                    CultUtility.OfferingComplete(offerer: pawn, altar: DropAltar, deity: DropAltar.currentOfferingDeity, offering: offerings);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            //yield return ToilLogMessage("Pass 15 - Final");

            //this.AddEndCondition(delegate
            //{
            //    Thing thing = this.GetActor().jobs.curJob.GetTarget(TargetIndex.A).Thing;
            //    if (thing is Building && !thing.Spawned)
            //    {
            //        return JobCondition.Incompletable;
            //    }
            //    return JobCondition.Ongoing;
            //});
            //this.FailOnBurningImmobile(TargetIndex.A);
            //Toil toil = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            //yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
            //yield return Toils_Reserve.ReserveQueue(TargetIndex.B, 1);
            //yield return new Toil
            //{
            //    initAction = delegate
            //    {
            //        if (this.job.targetQueueB != null && this.job.targetQueueB.Count == 1)
            //        {
            //            UnfinishedThing unfinishedThing = this.job.targetQueueB[0].Thing as UnfinishedThing;
            //            if (unfinishedThing != null)
            //            {
            //                unfinishedThing.BoundBill = (Bill_ProductionWithUft)this.job.bill;
            //            }
            //        }
            //    }
            //};
            //yield return Toils_Jump.JumpIf(toil, () => this.job.GetTargetQueue(TargetIndex.B).NullOrEmpty<LocalTargetInfo>());
            //Toil toil2 = Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.B);
            //yield return toil2;
            //Toil toil3 = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
            //yield return toil3;
            //yield return Toils_Haul.StartCarryThing(TargetIndex.B);
            //yield return JumpToCollectNextIntoHandsForBill(toil3, TargetIndex.B);
            //yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnDestroyedOrNull(TargetIndex.B);
            //Toil toil4 = Toils_JobTransforms.SetTargetToIngredientPlaceCell(TargetIndex.A, TargetIndex.B, TargetIndex.C);
            //yield return toil4;
            //yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, toil4, false);
            //yield return Toils_Jump.JumpIfHaveTargetInQueue(TargetIndex.B, toil2);
            //yield return toil;
            //Toil chantingTime = new Toil();
            //chantingTime.defaultCompleteMode = ToilCompleteMode.Delay;
            //CultUtility.remainingDuration = CultUtility.ritualDuration;
            //chantingTime.defaultDuration = CultUtility.remainingDuration - 360;
            //chantingTime.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            //chantingTime.PlaySustainerOrSound(CultDefOfs.RitualChanting);
            //Texture2D deitySymbol = ((CosmicEntityDef)DropAltar.currentOfferingDeity.def).Symbol;
            //chantingTime.initAction = delegate
            //{
            //    if (deitySymbol != null)
            //        MoteMaker.MakeInteractionBubble(this.pawn, null, ThingDefOf.Mote_Speech, deitySymbol);
            //};
            //yield return chantingTime;
            ////Toil 8: Execution of Prisoner
            //yield return new Toil
            //{
            //    initAction = delegate
            //    {
            //        CultUtility.OfferingComplete(this.pawn, DropAltar, DropAltar.currentOfferingDeity);
            //    },
            //    defaultCompleteMode = ToilCompleteMode.Instant
            //};


            ////this.AddFinishAction(() =>
            ////{
            ////    if (this.pawn.CurJob.targetQueueB.Count == 0 &&
            ////        DropAltar.currentOfferingState == Building_SacrificialAltar.OfferingState.started)
            ////    //When the ritual is finished -- then let's give the thoughts
            ////    CultUtility.OfferingReady(this.pawn, DropAltar);

            ////});
        }

        private static Toil ToilLogMessage(string message)
        {
            return new Toil {initAction = delegate { Log.Message(text: message); }};
        }

        private static Toil JumpToCollectNextIntoHandsForBill(Toil gotoGetTargetToil, TargetIndex ind)
        {
            var toil = new Toil();
            toil.initAction = delegate
            {
                var actor = toil.actor;
                if (actor.carryTracker.CarriedThing == null)
                {
                    Log.Error(text: "JumpToAlsoCollectTargetInQueue run on " + actor + " who is not carrying something.");
                    return;
                }

                if (actor.carryTracker.Full)
                {
                    return;
                }

                var curJob = actor.jobs.curJob;
                var targetQueue = curJob.GetTargetQueue(ind: ind);
                if (targetQueue.NullOrEmpty())
                {
                    return;
                }

                for (var i = 0; i < targetQueue.Count; i++)
                {
                    if (!GenAI.CanUseItemForWork(p: actor, item: targetQueue[index: i].Thing))
                    {
                        continue;
                    }

                    if (!targetQueue[index: i].Thing.CanStackWith(other: actor.carryTracker.CarriedThing))
                    {
                        continue;
                    }

                    if (!((actor.Position - targetQueue[index: i].Thing.Position).LengthHorizontalSquared <= 64f))
                    {
                        continue;
                    }

                    var num = actor.carryTracker.CarriedThing?.stackCount ?? 0;
                    var num2 = curJob.countQueue[index: i];
                    num2 = Mathf.Min(a: num2, b: targetQueue[index: i].Thing.def.stackLimit - num);
                    num2 = Mathf.Min(a: num2,
                        b: actor.carryTracker.AvailableStackSpace(td: targetQueue[index: i].Thing.def));
                    if (num2 <= 0)
                    {
                        continue;
                    }

                    curJob.count = num2;
                    curJob.SetTarget(ind: ind, pack: targetQueue[index: i].Thing);
                    List<int> countQueue;
                    var expr_1B2 = countQueue = curJob.countQueue;
                    int num3;
                    var expr_1B6 = num3 = i;
                    num3 = countQueue[index: num3];
                    expr_1B2[index: expr_1B6] = num3 - num2;
                    if (curJob.countQueue[index: i] == 0)
                    {
                        curJob.countQueue.RemoveAt(index: i);
                        targetQueue.RemoveAt(index: i);
                    }

                    actor.jobs.curDriver.JumpToToil(to: gotoGetTargetToil);
                    return;
                }
            };
            return toil;
        }
    }
}