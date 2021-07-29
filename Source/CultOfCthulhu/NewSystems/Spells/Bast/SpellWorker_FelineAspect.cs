using CultOfCthulhu;
using RimWorld;
using Verse;

namespace BastCult
{
    /// <summary>
    ///     This spell augments the executioner with the grace and deadliness of a cat.
    /// </summary>
    public class SpellWorker_FelineAspect : SpellWorker
    {
        public override bool CanSummonNow(Map map)
        {
            var felineProps = def.GetModExtension<FelineAspectProperties>();

            if (felineProps == null)
            {
                return false;
            }

            //Get executioner.
            var executioner = altar(map).tempExecutioner;

            return ExecutionerIsValid(executioner, felineProps);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var map = parms.target as Map;

            var felineProps = def.GetModExtension<FelineAspectProperties>();

            if (felineProps == null)
            {
                return true;
            }

            //Get executioner.
            var executioner = altar(map).tempExecutioner;

            if (!ExecutionerIsValid(executioner, felineProps))
            {
                return true;
            }

            //Apply Hediffs
            //To body
            executioner.health.AddHediff(felineProps.hediffToApplyToBody);

            //To hands
            foreach (var hand in felineProps.handDefs)
            {
                var records = executioner.RaceProps.body.AllParts.FindAll(part => part.def == hand);
                if (!(records.Count > 0))
                {
                    continue;
                }

                foreach (var record in records)
                {
                    executioner.health.AddHediff(felineProps.hediffToApplyToHands, record);
                }
            }

            return true;
        }

        public bool ExecutionerIsValid(Pawn preacher, FelineAspectProperties felineProps)
        {
            return !preacher.health.hediffSet.HasHediff(felineProps.hediffToApplyToBody);
        }
    }
}