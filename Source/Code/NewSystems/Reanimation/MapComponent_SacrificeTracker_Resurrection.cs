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

                var tempOathList = new List<Pawn>(collection: unspeakableOathPawns);
                foreach (var oathtaker in tempOathList)
                {
                    if (!oathtaker.Dead)
                    {
                        continue;
                    }

                    unspeakableOathPawns?.Remove(item: oathtaker);

                    if ((toBeResurrected?.Count ?? 0) > 0)
                    {
                        toBeResurrected = new List<Pawn>();
                    }

                    toBeResurrected?.Add(item: oathtaker);
                    Utility.DebugReport(x: "Started Resurrection Process");
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
            toBeResurrected.Remove(item: sourceCorpse);
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
                    owner.TryDrop(thing: sourceCorpse.Corpse, mode: ThingPlaceMode.Near, lastResultingThing: out lastThing);
                }
            }
            catch
            {

            }

            //Use B18's Resurrect Feature
            ResurrectionUtility.Resurrect(pawn: sourceCorpse);

            //Remove everything that conflicts with Psychopathic behavior
            sourceCorpse.story.traits.allTraits.RemoveAll(
                match: x => x.def.conflictingTraits is List<TraitDef> conflicts && !conflicts.NullOrEmpty() &&
                            conflicts.Contains(item: TraitDefOf.Psychopath) ||
                            x.def.defName == "Cults_OathtakerHastur");

            //Remove a random trait and add Psychopath
            if (sourceCorpse.story.traits.allTraits is List<Trait> {Count: > 1} allTraits &&
                allTraits.FirstOrDefault(predicate: x => x.def == TraitDefOf.Psychopath) == null)
            {
                sourceCorpse.story.traits.allTraits.RemoveLast();
                sourceCorpse.story.traits.GainTrait(trait: new Trait(def: TraitDefOf.Psychopath, degree: 0, forced: true));
            }

            //Adds the "Reanimated" trait
            sourceCorpse.story.traits.GainTrait(trait: new Trait(def: TraitDef.Named(defName: "Cults_OathtakerHastur2"), degree: 0, forced: true));

            //Message to the player
#pragma warning disable CS0618 // Type or member is obsolete
            Messages.Message(text: "ReanimatedOath".Translate(sourceCorpse.Name), def: MessageTypeDefOf.PositiveEvent);
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}