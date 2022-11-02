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
            if (TempExecutioner(map: map) != null)
            {
                var sacrificeTracker = map.GetComponent<MapComponent_SacrificeTracker>();
                if (sacrificeTracker != null)
                {
                    if (sacrificeTracker.unspeakableOathPawns == null)
                    {
                        sacrificeTracker.unspeakableOathPawns = new List<Pawn>();
                    }

                    if (!sacrificeTracker.unspeakableOathPawns.Contains(item: TempExecutioner(map: map)))
                    {
                        return true;
                    }

                    Messages.Message(text: "Executioner has already taken an unspeakable oath.",
                        def: MessageTypeDefOf.RejectInput);
                    return false;
                }

                Messages.Message(text: "Missing map component.", def: MessageTypeDefOf.RejectInput);
                return false;
            }

            Messages.Message(text: "Executioner is unavailable.", def: MessageTypeDefOf.RejectInput);
            return false;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var map = (Map) parms.target;
            var sacrificeTracker = map.GetComponent<MapComponent_SacrificeTracker>();
            if (sacrificeTracker == null)
            {
                return Utility.ResultFalseWithReport(s: new StringBuilder(value: "Missing map component."));
            }

            if (sacrificeTracker.unspeakableOathPawns == null)
            {
                sacrificeTracker.unspeakableOathPawns = new List<Pawn>();
            }

            if (!Utility.IsActorAvailable(preacher: executioner(map: map)))
            {
                Messages.Message(text: "Executioner is unavailable.", def: MessageTypeDefOf.RejectInput);
                return false;
            }

            executioner(map: map).story.traits.GainTrait(trait: new Trait(def: TraitDef.Named(defName: "Cults_OathtakerHastur")));
            sacrificeTracker.unspeakableOathPawns.Add(item: executioner(map: map));
            return true;
        }
    }
}