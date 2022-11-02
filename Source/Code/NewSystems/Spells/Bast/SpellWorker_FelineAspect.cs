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
            var executioner = altar(map: map).tempExecutioner;

            return ExecutionerIsValid(preacher: executioner, felineProps: felineProps);
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
            var executioner = altar(map: map).tempExecutioner;

            if (!ExecutionerIsValid(preacher: executioner, felineProps: felineProps))
            {
                return true;
            }

            //Apply Hediffs
            //To body
            executioner.health.AddHediff(def: felineProps.hediffToApplyToBody);

            //To hands
            foreach (var hand in felineProps.handDefs)
            {
                var records = executioner.RaceProps.body.AllParts.FindAll(match: part => part.def == hand);
                if (!(records.Count > 0))
                {
                    continue;
                }

                foreach (var record in records)
                {
                    executioner.health.AddHediff(def: felineProps.hediffToApplyToHands, part: record);
                }
            }

            return true;
        }

        public bool ExecutionerIsValid(Pawn preacher, FelineAspectProperties felineProps)
        {
            return !preacher.health.hediffSet.HasHediff(def: felineProps.hediffToApplyToBody);
        }
    }
}