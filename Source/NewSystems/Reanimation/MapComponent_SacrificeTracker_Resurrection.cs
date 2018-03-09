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
    public partial class MapComponent_SacrificeTracker : MapComponent
    {

        /// <summary>
        /// This keeps track of dead colonists who have taken the Unspeakable Oath.
        /// They will need to be resurrected. This begins the process.
        /// </summary>
        public void ResolveHasturOathtakers()
        {
            try
            {

                if (Find.TickManager.TicksGame % 100 != 0) return;
                if (unspeakableOathPawns.NullOrEmpty()) return;
                List<Pawn> tempOathList = new List<Pawn>(unspeakableOathPawns);
                foreach (Pawn oathtaker in tempOathList)
                {
                    if (oathtaker.Dead)
                    {
                        if (unspeakableOathPawns != null) unspeakableOathPawns.Remove(oathtaker);
                        if ((toBeResurrected?.Count ?? 0) > 0) toBeResurrected = new List<Pawn>();
                        toBeResurrected.Add(oathtaker);
                        Cthulhu.Utility.DebugReport("Started Resurrection Process");
                        ticksUntilResurrection = resurrectionTicks;
                    }
                }
            }
            catch (NullReferenceException)
            { }
        }

        /// <summary>
        /// When Oathtakers die, they need to be resurrected after a period of time.
        /// 
        /// </summary>
        public void ResolveHasturResurrections()
        {
            //Check ticks

            if (ticksUntilResurrection == -999) return;

            if (ticksUntilResurrection > 0)
            {
                ticksUntilResurrection--;
                return;
            }
            ticksUntilResurrection = -999;


            //Ticks passed. Commence resurrection!
            HasturResurrection();

            //Do we still have colonists that need resurrection? If so, let's proceed with another round of resurrection.
            //Reset the timer, and let's get to work.
            if ((toBeResurrected?.Count ?? 0) > 0)
            {
                ticksUntilResurrection = resurrectionTicks;
            }
        }

        public void HasturResurrection()
        {
            Pawn sourceCorpse = toBeResurrected.RandomElement();
            toBeResurrected.Remove(sourceCorpse);
            IntVec3 spawnLoc = IntVec3.Invalid;
            Map map = null;

            if (sourceCorpse.Corpse != null)
            {
                //Use B18's Resurrect Feature
                ResurrectionUtility.Resurrect(sourceCorpse);

                //Remove everything that conflicts with Psychopathic behavior
                sourceCorpse.story.traits.allTraits.RemoveAll(
                    x => (x.def.conflictingTraits is List<TraitDef> conflicts && !conflicts.NullOrEmpty() &&
                          conflicts.Contains(TraitDefOf.Psychopath)) ||
                         x.def.defName == "Cults_OathtakerHastur");
                
                //Remove a random trait and add Psychopath
                if (sourceCorpse.story.traits.allTraits is List<Trait> allTraits && allTraits.Count > 1 &&
                    allTraits.FirstOrDefault(x => x.def == TraitDefOf.Psychopath) == null)
                {
                    sourceCorpse.story.traits.allTraits.RemoveLast();
                    sourceCorpse.story.traits.GainTrait(new Trait(TraitDefOf.Psychopath, 0, true));   
                }
                
                //Adds the "Reanimated" trait
                sourceCorpse.story.traits.GainTrait(new Trait(TraitDef.Named("Cults_OathtakerHastur2"), 0, true));

                //Message to the player
                Messages.Message("ReanimatedOath".Translate(new object[] {
                    sourceCorpse.Name
                }), MessageTypeDefOf.PositiveEvent);
            }
        }

    }
}
