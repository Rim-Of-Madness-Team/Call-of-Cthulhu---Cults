using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CultOfCthulhu
{
    public class JobDriver_MakeOffering : JobDriver
    {
        public Building_SacrificialAltar DropAltar
        {
            get
            {
                Building_SacrificialAltar result = this.pawn.jobs.curJob.GetTarget(TargetIndex.A).Thing as Building_SacrificialAltar;
                if (result == null)
                {
                    throw new InvalidOperationException("Altar is missing.");
                }
                return result;
            }
        }

        private float BaseWorkAmount = 2300;

        private float workLeft = -1000f;

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.SetCompleteMode(ToilCompleteMode.Never);
            yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            Toil toil = new Toil();
            Texture2D deitySymbol = ((CosmicEntityDef)DropAltar.currentOfferingDeity.def).Symbol;
            toil.PlaySustainerOrSound(CultDefOfs.RitualChanting);
            toil.initAction = delegate
            {
                this.workLeft = 2300f;
                if (deitySymbol != null)
                    MoteMaker.MakeInteractionMote(this.pawn, null, ThingDefOf.Mote_Speech, deitySymbol);
            };
            toil.tickAction = delegate
            {
                this.workLeft -= 1;
                if (this.workLeft <= 0f)
                {
                    //Thing thing = ThingMaker.MakeThing(ThingDefOf.Snowman, null);
                    //thing.SetFaction(this.<> f__this.pawn.Faction, null);
                    //GenSpawn.Spawn(thing, this.<> f__this.TargetLocA);
                    this.ReadyForNextToil();
                    return;
                }
                JoyUtility.JoyTickCheckEnd(this.pawn, JoyTickFullJoyAction.EndJob, 1f);
            };
            toil.WithProgressBar(TargetIndex.A, () => this.workLeft / this.BaseWorkAmount, true, -0.5f);
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            //toil.FailOn(() => !JoyUtility.EnjoyableOutsideNow(this.<> f__this.pawn, null));
            yield return toil;
            yield break;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<float>(ref this.workLeft, "workLeft", 0f, false);
        }


    }
}
