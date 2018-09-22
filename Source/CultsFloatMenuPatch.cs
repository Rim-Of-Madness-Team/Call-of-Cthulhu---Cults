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
                                Cthulhu.Utility.RemoveSanityLoss(pawn);
                                var newMentalState = (Rand.Value > 0.05)
                                    ? DefDatabase<MentalStateDef>.AllDefs.InRandomOrder()
                                        .FirstOrDefault(x => x.IsAggro == false) : MentalStateDefOf.Berserk;
                                Messages.Message("Cults_TradedSanityLossForMadness".Translate(pawn.LabelShort), pawn,
                                    MessageTypeDefOf.ThreatSmall);
                                Cthulhu.Utility.DebugReport("Selected mental state: " + newMentalState.label);
                                pawn.mindState.mentalStateHandler.TryStartMentalState(newMentalState);
                            };
                            opts.Add(new FloatMenuOption("Cults_TradeSanityForMadness".Translate(), action,
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