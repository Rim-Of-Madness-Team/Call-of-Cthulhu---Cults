// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;
using Verse.AI;

// ----------------------------------------------------------------------
// These are RimWorld-specific usings. Activate/Deactivate what you need:
// ----------------------------------------------------------------------
// Always needed
//using VerseBase;         // Material/Graphics handling functions are found here
// RimWorld universal objects are here (like 'Building')
// Needed when you do something with the AI
// Needed when you do something with Sound
// Needed when you do something with Noises
// RimWorld specific functions are found here (like 'Building_Battery')

// RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

namespace CultOfCthulhu
{
    internal class JobDriver_ReflectOnResult : JobDriver
    {
        protected Building_SacrificialAltar altar => (Building_SacrificialAltar) job.GetTarget(ind: TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            //Toil 0 -- Go here first to reflect.
            if (altar?.SacrificeData?.Executioner != null)
            {
                if (pawn == altar.SacrificeData.Executioner)
                {
                    yield return Toils_Goto.GotoThing(ind: TargetIndex.A, peMode: PathEndMode.InteractionCell);
                }
            }

            //Toil 9 Celebrate or recoil
            yield return new Toil
            {
                initAction = delegate
                {
                    if (Map.GetComponent<MapComponent_SacrificeTracker>().lastResult ==
                        CultUtility.SacrificeResult.success)
                    {
                        //Do something? Ia Ia!
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };

            //Toil 10 Reflect on result
            var reflectingTime = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = CultUtility.reflectDuration
            };
            //chantingTime.PlaySustainerOrSound(DefDatabase<SoundDef>.GetNamed("Estate_GramophoneWindup"));
            yield return reflectingTime;

            //Toil 11 Reset the altar and clear variables.
            yield return new Toil
            {
                initAction = delegate
                {
                    if (altar == null)
                    {
                        return;
                    }

                    if (altar.currentSacrificeState == Building_SacrificialAltar.SacrificeState.finished)
                    {
                        return;
                    }

                    altar.ChangeState(type: Building_SacrificialAltar.State.sacrificing,
                        sacrificeState: Building_SacrificialAltar.SacrificeState.finished);
                    Map.GetComponent<MapComponent_SacrificeTracker>().ClearSacrificeVariables();
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
    }
}