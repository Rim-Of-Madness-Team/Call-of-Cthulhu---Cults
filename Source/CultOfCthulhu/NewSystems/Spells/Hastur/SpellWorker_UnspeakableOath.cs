using System.Collections.Generic;
using System.Text;
using Cthulhu;
using RimWorld;
using Verse;

namespace CultOfCthulhu
{
    internal class SpellWorker_UnspeakableOath : SpellWorker
    {
        public override bool CanSummonNow(Map map)
        {
            if (TempExecutioner(map) != null)
            {
                var sacrificeTracker = map.GetComponent<MapComponent_SacrificeTracker>();
                if (sacrificeTracker != null)
                {
                    if (sacrificeTracker.unspeakableOathPawns == null)
                    {
                        sacrificeTracker.unspeakableOathPawns = new List<Pawn>();
                    }

                    if (!sacrificeTracker.unspeakableOathPawns.Contains(TempExecutioner(map)))
                    {
                        return true;
                    }

                    Messages.Message("Executioner has already taken an unspeakable oath.",
                        MessageTypeDefOf.RejectInput);
                    return false;
                }

                Messages.Message("Missing map component.", MessageTypeDefOf.RejectInput);
                return false;
            }

            Messages.Message("Executioner is unavailable.", MessageTypeDefOf.RejectInput);
            return false;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var map = (Map) parms.target;
            var sacrificeTracker = map.GetComponent<MapComponent_SacrificeTracker>();
            if (sacrificeTracker == null)
            {
                return Utility.ResultFalseWithReport(new StringBuilder("Missing map component."));
            }

            if (sacrificeTracker.unspeakableOathPawns == null)
            {
                sacrificeTracker.unspeakableOathPawns = new List<Pawn>();
            }

            if (!Utility.IsActorAvailable(executioner(map)))
            {
                Messages.Message("Executioner is unavailable.", MessageTypeDefOf.RejectInput);
                return false;
            }

            executioner(map).story.traits.GainTrait(new Trait(TraitDef.Named("Cults_OathtakerHastur")));
            sacrificeTracker.unspeakableOathPawns.Add(executioner(map));
            return true;
        }
    }
}