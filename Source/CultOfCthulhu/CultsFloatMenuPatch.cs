using System;
using System.Collections.Generic;
using System.Linq;
using Cthulhu;
using JecsTools;
using RimWorld;
using UnityEngine;
using Verse;

namespace CultOfCthulhu
{
    public class CultsFloatMenuPatch : FloatMenuPatch
    {
        public override IEnumerable<KeyValuePair<_Condition, Func<Vector3, Pawn, Thing, List<FloatMenuOption>>>>
            GetFloatMenus()
        {
            var floatMenus =
                new List<KeyValuePair<_Condition, Func<Vector3, Pawn, Thing, List<FloatMenuOption>>>>();

            var madnessCondition = new _Condition(_ConditionType.IsType, typeof(Pawn));

            List<FloatMenuOption> madnessFunc(Vector3 clickPos, Pawn pawn, Thing curThing)
            {
                var target = curThing as Pawn;
                if (pawn != target)
                {
                    return null;
                }

                if (!Utility.HasSanityLoss(pawn))
                {
                    return null;
                }

                var opts = new List<FloatMenuOption>();

                void action()
                {
                    var newMentalState = Rand.Value > 0.05
                        ? DefDatabase<MentalStateDef>.AllDefs.InRandomOrder()
                            .FirstOrDefault(x => x.IsAggro == false)
                        : MentalStateDefOf.Berserk;
                    Utility.DebugReport("Selected mental state: " + newMentalState?.label);
                    if (pawn != null && pawn.Drafted)
                    {
                        pawn.drafter.Drafted = false;
                    }

                    pawn?.ClearMind();
                    pawn?.pather.StopDead();
                    if (pawn != null && !pawn.mindState.mentalStateHandler.TryStartMentalState(newMentalState))
                    {
                        Messages.Message("ROM_TradedSanityLossForMadnessFailed".Translate(pawn.LabelShort),
                            pawn,
                            MessageTypeDefOf.RejectInput);
                        return;
                    }

                    Messages.Message("ROM_TradedSanityLossForMadness".Translate(pawn?.LabelShort), pawn,
                        MessageTypeDefOf.ThreatSmall);
                    Utility.RemoveSanityLoss(pawn);
                }

                opts.Add(new FloatMenuOption("ROM_TradeSanityForMadness".Translate(), action,
                    MenuOptionPriority.High, null, target));
                return opts;
            }

            var curSec =
                new KeyValuePair<_Condition, Func<Vector3, Pawn, Thing, List<FloatMenuOption>>>(madnessCondition,
                    madnessFunc);
            floatMenus.Add(curSec);
            return floatMenus;
        }
    }
}