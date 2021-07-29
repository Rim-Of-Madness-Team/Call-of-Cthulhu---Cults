// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using System;
using Cthulhu;
using RimWorld;
using Verse;

// ----------------------------------------------------------------------
// These are RimWorld-specific usings. Activate/Deactivate what you need:
// ----------------------------------------------------------------------
// Always needed
//using VerseBase;         // Material/Graphics handling functions are found here
// RimWorld universal objects are here (like 'Building')
// Needed when you do something with the AI
// Needed when you do something with Sound
// Needed when you do something with Noises
// RimWorld specific functions are found here (like 'Building_Battery')

// RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 


namespace CultOfCthulhu
{
    public class SpellWorker_StarryWisdom : SpellWorker
    {
        public override bool CanSummonNow(Map map)
        {
            try
            {
                if (TempExecutioner(map) == null)
                {
                    Messages.Message("Null executioner.", MessageTypeDefOf.RejectInput);
                    return false;
                }

                if (TempExecutioner(map).story.traits.HasTrait(TraitDefOf.Psychopath) &&
                    TempExecutioner(map).story.traits.HasTrait(TraitDefOf.Cannibal))
                {
                    Messages.Message("The executioner already has both psychopath and cannibal traits.",
                        MessageTypeDefOf.RejectInput);
                    return false;
                }
            }
            catch (Exception e)
            {
                Utility.DebugReport(e.ToString());
            }

            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!(parms.target is Map map))
            {
                return false;
            }

            var p = map.GetComponent<MapComponent_SacrificeTracker>().lastUsedAltar.SacrificeData.Executioner;
            TraitDef traitToAdd = null;
            if (!p.story.traits.HasTrait(TraitDefOf.Cannibal))
            {
                traitToAdd = TraitDefOf.Cannibal;
            }

            if (!p.story.traits.HasTrait(TraitDefOf.Psychopath))
            {
                traitToAdd = TraitDefOf.Psychopath;
            }

            p.story.traits.GainTrait(new Trait(traitToAdd));
            //if (p.story.traits.allTraits.Count < 3) p.story.traits.GainTrait(new Trait(traitToAdd));
            //else
            //{
            //    foreach (Trait t in p.story.traits.allTraits)
            //    {
            //        if(t.def != TraitDefOf.Cannibal && t.def != TraitDefOf.Psychopath)
            //        {
            //            p.story.traits.allTraits.Remove(t);
            //            break; //Remove 1 trait and get out
            //        }
            //    }
            //    p.story.traits.GainTrait(new Trait(traitToAdd));
            //}
            map.GetComponent<MapComponent_SacrificeTracker>().lastLocation = p.Position;
            Utility.ApplyTaleDef("Cults_SpellStarryWisdom", p);

            return true;
        }
    }
}