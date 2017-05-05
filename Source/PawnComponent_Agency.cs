using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace CthulhuFactions
{
    [StaticConstructorOnStartup]
    class PawnComponent_Agency : ThingComp
    {
        static bool loadedCults;

        static PawnComponent_Agency()
        {
            loadedCults = false;
            foreach (ModContentPack ResolvedMod in LoadedModManager.RunningMods)
                if (ResolvedMod.Name.Contains("Call of Cthulhu - Cults"))
                {
                    loadedCults = true;
                    break;
                }
        }

        public CompProperties_Agency Props
        {
            get
            {
                return (CompProperties_Agency)props;
            }
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            if (!parent.Faction.HostileTo(Faction.OfPlayer))
            {
                Predicate<Thing> predicate = delegate (Thing t)
                {
                    if (t == null) return false;
                    if (t == parent) return false;
                    if (!t.Spawned) return false;
                    if (t.def == null) return false;
                    if (t is Corpse) t = (t as Corpse).InnerPawn;
                    if (t is MinifiedThing) t = (t as MinifiedThing).InnerThing;
                    if (loadedCults)
                    {
                        if (t.def.defName.Contains("Cult")) return true; //covers most things
                        if (t.def.defName.EqualsIgnoreCase("ForbiddenKnowledgeCenter")) return true; //specifically called because Jecrell's name consistency is awful ^^
                        if (t.def.defName.EqualsIgnoreCase("MonolithNightmare")) return true;
                        if (t.def.defName.EqualsIgnoreCase("PlantTreeNightmare")) return true;
                        if (t.def.defName.Contains("Cthulhu")) return true;
                        if (t.def.defName.Contains("Dagon")) return true;
                        if (t.def.defName.Contains("Nyarlathotep")) return true;
                        if (t.def.defName.Contains("Shub")) return true;
                    }
                    return false;
                };

                if (!(parent as Pawn).Dead && parent.Map != null)
                {
                    List<Thing> things = new List<Thing>();
                    parent.Map.listerBuildings.allBuildingsColonist.ForEach((Building x) => things.Add(x));
                    things.AddRange(parent.Map.listerThings.AllThings);
                    Thing thing2 = GenClosest.ClosestThing_Global_Reachable(parent.Position, parent.Map, things, PathEndMode.OnCell,
                        TraverseParms.For(parent as Pawn, Danger.Deadly, TraverseMode.PassDoors, false), 20, predicate, null);

                        /*ClosestThingReachable(parent.Position, parent.Map, ThingRequest.ForGroup(ThingRequestGroup.Everything), PathEndMode.OnCell,
                        TraverseParms.For(parent as Pawn, Danger.Deadly, TraverseMode.PassDoors, false), 20, predicate, null, 50, true);*/
                    if (thing2 != null)
                    {
                        parent.Faction.SetHostileTo(Faction.OfPlayer, true);
                        Log.Message("Agency discovered: " + thing2.def.label);
                        Find.LetterStack.ReceiveLetter("Cult discovered", "Agency discovered : " + thing2.def.label, LetterType.BadUrgent);
                        Messages.Message("Agency discovered: " + thing2.def.label, MessageSound.Negative);
                    }

                }
            }
        }
    }

    class CompProperties_Agency : CompProperties
    {
        public CompProperties_Agency()
        {
            compClass = typeof(PawnComponent_Agency);
        }
    }
}
