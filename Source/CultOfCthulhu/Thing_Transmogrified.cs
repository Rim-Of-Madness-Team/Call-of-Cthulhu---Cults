// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

// ----------------------------------------------------------------------
// These are RimWorld-specific usings. Activate/Deactivate what you need:
// ----------------------------------------------------------------------
// Always needed
//using VerseBase;         // Material/Graphics handling functions are found here

using Verse;

// RimWorld universal objects are here (like 'Building')
// Needed when you do something with the AI
// Needed when you do something with Sound
// Needed when you do something with Noises
// RimWorld specific functions are found here (like 'Building_Battery')

// RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

namespace CultOfCthulhu
{
    public class Thing_Transmogrified : Thing
    {
        public ThingDef originalDef;

        public Thing_Transmogrified(ThingDef newDef)
        {
            if (newDef is ThingDef_Transmogrified)
            {
                originalDef = newDef;
            }
        }

        public override void SpawnSetup(Map map, bool bla)
        {
            base.SpawnSetup(map, bla);
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref originalDef, "originalDef");
        }
    }
}