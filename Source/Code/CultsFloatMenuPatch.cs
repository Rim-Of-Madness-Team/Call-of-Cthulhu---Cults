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

            var madnessCondition = new _Condition(condition: _ConditionType.IsType, data: typeof(Pawn));

            List<FloatMenuOption> madnessFunc(Vector3 clickPos, Pawn pawn, Thing curThing)
            {
                var target = curThing as Pawn;
                if (pawn != target)
                {
                    return null;
                }

                if (!Utility.HasSanityLoss(pawn: pawn))
                {
                    return null;
                }

                var opts = new List<FloatMenuOption>();

                void action()
                {
                    var newMentalState = Rand.Value > 0.05
                        ? DefDatabase<MentalStateDef>.AllDefs.InRandomOrder()
                            .FirstOrDefault(predicate: x => x.IsAggro == false)
                        : MentalStateDefOf.Berserk;
                    Utility.DebugReport(x: "Selected mental state: " + newMentalState?.label);
                    if (pawn != null && pawn.Drafted)
                    {
                        pawn.drafter.Drafted = false;
                    }

                    pawn?.ClearMind();
                    pawn?.pather.StopDead();
                    if (pawn != null && !pawn.mindState.mentalStateHandler.TryStartMentalState(stateDef: newMentalState))
                    {
                        Messages.Message(text: "ROM_TradedSanityLossForMadnessFailed".Translate(arg1: pawn.LabelShort),
                            lookTargets: pawn,
                            def: MessageTypeDefOf.RejectInput);
                        return;
                    }

                    Messages.Message(text: "ROM_TradedSanityLossForMadness".Translate(arg1: pawn?.LabelShort), lookTargets: pawn,
                        def: MessageTypeDefOf.ThreatSmall);
                    Utility.RemoveSanityLoss(pawn: pawn);
                }

                opts.Add(item: new FloatMenuOption(label: "ROM_TradeSanityForMadness".Translate(), action: action,
                    priority: MenuOptionPriority.High, mouseoverGuiAction: null, revalidateClickTarget: target));
                return opts;
            }

            var curSec =
                new KeyValuePair<_Condition, Func<Vector3, Pawn, Thing, List<FloatMenuOption>>>(key: madnessCondition,
                    value: madnessFunc);
            floatMenus.Add(item: curSec);
            return floatMenus;
        }
    }
}