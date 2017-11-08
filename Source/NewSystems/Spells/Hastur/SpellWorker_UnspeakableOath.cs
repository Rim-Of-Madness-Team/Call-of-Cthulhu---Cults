using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace CultOfCthulhu
{
    class SpellWorker_UnspeakableOath : SpellWorker
    {
        public override bool CanSummonNow(Map map)
        {
            if (TempExecutioner(map) != null)
            {
                MapComponent_SacrificeTracker sacrificeTracker = map.GetComponent<MapComponent_SacrificeTracker>();
                if (sacrificeTracker != null)
                {
                    if (sacrificeTracker.unspeakableOathPawns == null) sacrificeTracker.unspeakableOathPawns = new List<Pawn>();
                    if (sacrificeTracker.unspeakableOathPawns.Contains(TempExecutioner(map)))
                    {
                        Messages.Message("Executioner has already taken an unspeakable oath.", MessageTypeDefOf.RejectInput);
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    Messages.Message("Missing map component.", MessageTypeDefOf.RejectInput);
                    return false;
                }
            }
            else
            {
                Messages.Message("Executioner is unavailable.", MessageTypeDefOf.RejectInput);
                return false;
            }
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            MapComponent_SacrificeTracker sacrificeTracker = map.GetComponent<MapComponent_SacrificeTracker>();
            if (sacrificeTracker == null) return Cthulhu.Utility.ResultFalseWithReport(new StringBuilder("Missing map component."));
            if (sacrificeTracker.unspeakableOathPawns == null) sacrificeTracker.unspeakableOathPawns = new List<Pawn>();
            if (!Cthulhu.Utility.IsActorAvailable(executioner(map)))
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
