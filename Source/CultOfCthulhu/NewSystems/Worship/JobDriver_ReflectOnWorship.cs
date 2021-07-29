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
    internal class JobDriver_ReflectOnWorship : JobDriver
    {
        protected Building_SacrificialAltar altar => (Building_SacrificialAltar) job.GetTarget(TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            //Toil 0 -- Go here first to reflect.
            if (altar != null)
            {
                if (pawn == altar.preacher)
                {
                    yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
                }
            }

            //Toil 1 Celebrate or recoil
            yield return new Toil
            {
                initAction = delegate
                {
                    //Do something? Ia ia!
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };

            //Toil 2 Reflect on worship
            var reflectingTime = new Toil
            {
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = CultUtility.reflectDuration
            };
            //chantingTime.PlaySustainerOrSound(DefDatabase<SoundDef>.GetNamed("Estate_GramophoneWindup"));
            yield return reflectingTime;

            //Toil 3 Reset the altar and clear variables.
            yield return new Toil
            {
                initAction = delegate
                {
                    if (altar == null)
                    {
                        return;
                    }

                    if (altar.currentWorshipState != Building_SacrificialAltar.WorshipState.finished)
                    {
                        altar.ChangeState(Building_SacrificialAltar.State.worshipping,
                            Building_SacrificialAltar.WorshipState.finished);
                        //Map.GetComponent<MapComponent_SacrificeTracker>().ClearVariables();
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
    }
}