// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

// ----------------------------------------------------------------------
// These are RimWorld-specific usings. Activate/Deactivate what you need:
// ----------------------------------------------------------------------
using UnityEngine;         // Always needed
//using VerseBase;         // Material/Graphics handling functions are found here
using Verse;               // RimWorld universal objects are here (like 'Building')
using Verse.AI;          // Needed when you do something with the AI
using Verse.AI.Group;
using Verse.Sound;       // Needed when you do something with Sound
using Verse.Noise;       // Needed when you do something with Noises
using RimWorld;            // RimWorld specific functions are found here (like 'Building_Battery')
using RimWorld.Planet;   // RimWorld specific functions for world creation
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
                    Messages.Message("Null executioner.", MessageSound.RejectInput);
                    return false;
                }
                if (TempExecutioner(map).story.traits.HasTrait(TraitDefOf.Psychopath) &&
                    TempExecutioner(map).story.traits.HasTrait(TraitDefOf.Cannibal))
                {
                    Messages.Message("The executioner already has both psychopath and cannibal traits.", MessageSound.RejectInput);
                    return false;
                }
            }
            catch (Exception e)
            {
                Cthulhu.Utility.DebugReport(e.ToString());
            }
            return true;
        }

        public override bool TryExecute(IncidentParms parms)
        {
            Map map = parms.target as Map;
            Pawn p = map.GetComponent<MapComponent_SacrificeTracker>().lastUsedAltar.executioner;
            TraitDef traitToAdd = null;
            if (!p.story.traits.HasTrait(TraitDefOf.Cannibal)) traitToAdd = TraitDefOf.Cannibal;
            if (!p.story.traits.HasTrait(TraitDefOf.Psychopath)) traitToAdd = TraitDefOf.Psychopath;
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
            Cthulhu.Utility.ApplyTaleDef("Cults_SpellStarryWisdom", p);

            return true;
        }
    }
}
