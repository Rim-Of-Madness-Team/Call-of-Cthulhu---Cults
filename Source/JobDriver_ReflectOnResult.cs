// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

// ----------------------------------------------------------------------
// These are RimWorld-specific usings. Activate/Deactivate what you need:
// ----------------------------------------------------------------------
using UnityEngine;         // Always needed
//using VerseBase;         // Material/Graphics handling functions are found here
using Verse;               // RimWorld universal objects are here (like 'Building')
using Verse.AI;          // Needed when you do something with the AI
using Verse.AI.Group;
using Verse.Sound;       // Needed when you do something with Sound
using Verse.Noise;       // Needed when you do something with Noises
using RimWorld;            // RimWorld specific functions are found here (like 'Building_Battery')
using RimWorld.Planet;   // RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

namespace CultOfCthulhu
{
    class JobDriver_ReflectOnResult : JobDriver
    {

        protected Building_SacrificialAltar altar
        {
            get
            {
                return (Building_SacrificialAltar)base.CurJob.GetTarget(TargetIndex.A).Thing;
            }
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            //Toil 0 -- Go here first to reflect.
            if (altar != null)
            {
                if (altar.executioner != null)
                {
                    if (this.pawn == altar.executioner)
                    {
                        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
                    }
                }
            }

            //Toil 9 Celebrate or recoil
            yield return new Toil
            {
                initAction = delegate
                {
                    if (Map.GetComponent<MapComponent_SacrificeTracker>().lastResult == CultUtility.SacrificeResult.success)
                    {
                        //Do something? Ia Ia!
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };

            //Toil 10 Reflect on result
            Toil reflectingTime = new Toil();
            reflectingTime.defaultCompleteMode = ToilCompleteMode.Delay;
            reflectingTime.defaultDuration = CultUtility.reflectDuration;
            //chantingTime.PlaySustainerOrSound(DefDatabase<SoundDef>.GetNamed("Estate_GramophoneWindup"));
            yield return reflectingTime;

            //Toil 11 Reset the altar and clear variables.
            yield return new Toil
            {
                initAction = delegate
                {
                    if (altar != null)
                    {
                        if (altar.currentSacrificeState != Building_SacrificialAltar.SacrificeState.finished)
                        {
                            altar.ChangeState(Building_SacrificialAltar.State.sacrificing, Building_SacrificialAltar.SacrificeState.finished);
                            Map.GetComponent<MapComponent_SacrificeTracker>().ClearSacrificeVariables();
                        }
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }

    }
}
