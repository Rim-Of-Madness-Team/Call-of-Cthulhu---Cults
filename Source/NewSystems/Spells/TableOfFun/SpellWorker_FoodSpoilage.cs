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
    public class SpellWorker_FoodSpoilage : SpellWorker
    {

        protected IEnumerable<ThingWithComps> Food(Map map)
        {
            return from ThingWithComps food in map.listerThings.ThingsInGroup(ThingRequestGroup.FoodSourceNotPlantOrTree)
                       where food.IsInAnyStorage()
                       select food;
        }



        protected override bool CanFireNowSub(IIncidentTarget target)
        {

            //Cthulhu.Utility.DebugReport("CanFire: " + this.def.defName);
            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            for (int i = 0; i < Rand.Range(3,6); i++)
            {
                if (Food((Map)parms.target).Count<ThingWithComps>() != 0)
                {
                    ThingWithComps item;
                    if (Food((Map)parms.target).TryRandomElement<ThingWithComps>(out item))
                    {
                        //Cthulhu.Utility.DebugReport("Destroyed: " + item.ToString());
                        item.Destroy();
                    }
                }
                else
                {
                    Cthulhu.Utility.DebugReport("No food to spoil.");
                }
            }
            return true;
        }

    }
}