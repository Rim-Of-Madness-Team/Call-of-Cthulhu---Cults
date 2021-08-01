// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
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
    public partial class MapComponent_SacrificeTracker : MapComponent
    {
        /// <summary>
        ///     This keeps track of dead colonists who have taken the Unspeakable Oath.
        ///     They will need to be resurrected. This begins the process.
        /// </summary>
        public void ResolveHasturOathtakers()
        {
            try
            {
                if (Find.TickManager.TicksGame % 100 != 0)
                {
                    return;
                }

                if (unspeakableOathPawns.NullOrEmpty())
                {
                    return;
                }

                var tempOathList = new List<Pawn>(unspeakableOathPawns);
                foreach (var oathtaker in tempOathList)
                {
                    if (!oathtaker.Dead)
                    {
                        continue;
                    }

                    unspeakableOathPawns?.Remove(oathtaker);

                    if ((toBeResurrected?.Count ?? 0) > 0)
                    {
                        toBeResurrected = new List<Pawn>();
                    }

                    toBeResurrected?.Add(oathtaker);
                    Utility.DebugReport("Started Resurrection Process");
                    ticksUntilResurrection = resurrectionTicks;
                }
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        ///     When Oathtakers die, they need to be resurrected after a period of time.
        /// </summary>
        public void ResolveHasturResurrections()
        {
            //Check ticks

            if (ticksUntilResurrection == -999)
            {
                return;
            }

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
            if (toBeResurrected?.FirstOrDefault() == null)
            {
                ticksUntilResurrection = -999;
                return;
            }
            var sourceCorpse = toBeResurrected.RandomElement();
            toBeResurrected.Remove(sourceCorpse);
            var unused = IntVec3.Invalid;

            if (sourceCorpse.Corpse == null)
            {
                return;
            }

            try
            {
                if (sourceCorpse?.Corpse?.holdingOwner is ThingOwner owner)
                {
                    Thing lastThing = null;
                    owner.TryDrop(sourceCorpse.Corpse, ThingPlaceMode.Near, out lastThing);
                }
            }
            catch
            {

            }

            //Use B18's Resurrect Feature
            ResurrectionUtility.Resurrect(sourceCorpse);

            //Remove everything that conflicts with Psychopathic behavior
            sourceCorpse.story.traits.allTraits.RemoveAll(
                x => x.def.conflictingTraits is List<TraitDef> conflicts && !conflicts.NullOrEmpty() &&
                     conflicts.Contains(TraitDefOf.Psychopath) ||
                     x.def.defName == "Cults_OathtakerHastur");

            //Remove a random trait and add Psychopath
            if (sourceCorpse.story.traits.allTraits is List<Trait> {Count: > 1} allTraits &&
                allTraits.FirstOrDefault(x => x.def == TraitDefOf.Psychopath) == null)
            {
                sourceCorpse.story.traits.allTraits.RemoveLast();
                sourceCorpse.story.traits.GainTrait(new Trait(TraitDefOf.Psychopath, 0, true));
            }

            //Adds the "Reanimated" trait
            sourceCorpse.story.traits.GainTrait(new Trait(TraitDef.Named("Cults_OathtakerHastur2"), 0, true));

            //Message to the player
#pragma warning disable CS0618 // Type or member is obsolete
            Messages.Message("ReanimatedOath".Translate(sourceCorpse.Name), MessageTypeDefOf.PositiveEvent);
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}