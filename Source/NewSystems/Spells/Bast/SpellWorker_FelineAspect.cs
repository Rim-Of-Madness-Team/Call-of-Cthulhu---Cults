using CultOfCthulhu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace BastCult
{
    /// <summary>
    /// This spell augments the executioner with the grace and deadliness of a cat.
    /// </summary>
    public class SpellWorker_FelineAspect : SpellWorker
    {
        public override bool CanSummonNow(Map map)
        {
            FelineAspectProperties felineProps = def.GetModExtension<FelineAspectProperties>();

            if(felineProps != null)
            {
                //Get executioner.
                Pawn executioner = altar(map).tempExecutioner;

                return ExecutionerIsValid(executioner, felineProps);
            }

            return false;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = parms.target as Map;

            FelineAspectProperties felineProps = def.GetModExtension<FelineAspectProperties>();

            if (felineProps != null)
            {
                //Get executioner.
                Pawn executioner = altar(map).tempExecutioner;

                if (ExecutionerIsValid(executioner, felineProps))
                {
                    //Apply Hediffs
                    //To body
                    executioner.health.AddHediff(felineProps.hediffToApplyToBody, null);

                    //To hands
                    foreach(BodyPartDef hand in felineProps.handDefs)
                    {
                        BodyPartRecord record = executioner.RaceProps.body.AllParts.First(part => part.def == hand);
                        if(record != null)
                            executioner.health.AddHediff(felineProps.hediffToApplyToHands, record);
                    }
                }
            }

            return true;
        }

        public bool ExecutionerIsValid(Pawn preacher, FelineAspectProperties felineProps)
        {
            if (!preacher.health.hediffSet.HasHediff(felineProps.hediffToApplyToBody, false))
                return true;

            return false;
        }
    }
}
