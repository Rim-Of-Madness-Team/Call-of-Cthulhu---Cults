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
    public class SpellWorker_FoodSpoilage : SpellWorker
    {
        protected IEnumerable<ThingWithComps> Food(Map map)
        {
            return from ThingWithComps food in map.listerThings.ThingsInGroup(
                    @group: ThingRequestGroup.FoodSourceNotPlantOrTree)
                where food.IsInAnyStorage()
                select food;
        }


        protected override bool CanFireNowSub(IncidentParms parms)
        {
            //Cthulhu.Utility.DebugReport("CanFire: " + this.def.defName);
            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            for (var i = 0; i < Rand.Range(min: 3, max: 6); i++)
            {
                if (Food(map: (Map) parms.target).Count() != 0)
                {
                    if (Food(map: (Map) parms.target).TryRandomElement(result: out var item))
                    {
                        //Cthulhu.Utility.DebugReport("Destroyed: " + item.ToString());
                        item.Destroy();
                    }
                }
                else
                {
                    Utility.DebugReport(x: "No food to spoil.");
                }
            }

            return true;
        }
    }
}