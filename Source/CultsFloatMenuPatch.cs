using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JecsTools;
using Verse;
using UnityEngine;
using Verse.AI;
using RimWorld;

namespace CultOfCthulhu
{
    public class CultsFloatMenuPatch : FloatMenuPatch
    {
        public override IEnumerable<KeyValuePair<_Condition, Func<Vector3, Pawn, Thing, List<FloatMenuOption>>>>
            GetFloatMenus()
        {
            List<KeyValuePair<_Condition, Func<Vector3, Pawn, Thing, List<FloatMenuOption>>>> floatMenus =
                new List<KeyValuePair<_Condition, Func<Vector3, Pawn, Thing, List<FloatMenuOption>>>>();

            _Condition madnessCondition = new _Condition(_ConditionType.IsType, typeof(Pawn));
            Func<Vector3, Pawn, Thing, List<FloatMenuOption>> madnessFunc =
                delegate(Vector3 clickPos, Pawn pawn, Thing curThing)
                {
                    List<FloatMenuOption> opts = null;
                    Pawn target = curThing as Pawn;
                    if (pawn == target)
                    {
                        if (Cthulhu.Utility.HasSanityLoss(pawn))
                        {
                            opts = new List<FloatMenuOption>();
                            Action action = delegate
                            {
                                var newMentalState = (Rand.Value > 0.05)
                                    ? DefDatabase<MentalStateDef>.AllDefs.InRandomOrder()
                                        .FirstOrDefault(x => x.IsAggro == false) : MentalStateDefOf.Berserk;
                                Cthulhu.Utility.DebugReport("Selected mental state: " + newMentalState.label);
                                if (pawn.Drafted) pawn.drafter.Drafted = false;
                                pawn.ClearMind();
                                pawn.pather.StopDead();
                                if (!pawn.mindState.mentalStateHandler.TryStartMentalState(newMentalState))
                                {
                                    Messages.Message("ROM_TradedSanityLossForMadnessFailed".Translate(pawn.LabelShort), pawn,
                                        MessageTypeDefOf.RejectInput);
                                    return;
                                }
                                Messages.Message("ROM_TradedSanityLossForMadness".Translate(pawn.LabelShort), pawn,
                                    MessageTypeDefOf.ThreatSmall);
                                Cthulhu.Utility.RemoveSanityLoss(pawn);
                            };
                            opts.Add(new FloatMenuOption("ROM_TradeSanityForMadness".Translate(), action,
                                MenuOptionPriority.High, null, target, 0f, null, null));
                            return opts;
                        }
                    }

                    return null;
                };
            KeyValuePair<_Condition, Func<Vector3, Pawn, Thing, List<FloatMenuOption>>> curSec =
                new KeyValuePair<_Condition, Func<Vector3, Pawn, Thing, List<FloatMenuOption>>>(madnessCondition,
                    madnessFunc);
            floatMenus.Add(curSec);
            return floatMenus;
        }
    }
}