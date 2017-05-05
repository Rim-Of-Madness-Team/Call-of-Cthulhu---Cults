﻿// ----------------------------------------------------------------------
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
    public class Thing_Transmogrified : Thing
    {
        public ThingDef originalDef;

        public Thing_Transmogrified(ThingDef newDef)
        {
            ThingDef_Transmogrified currentDef = newDef as ThingDef_Transmogrified;
            if (currentDef != null)
            {
                this.originalDef = newDef;
            }
            return;
        }

        public override void SpawnSetup(Map map)
        {
            base.SpawnSetup(map);
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<ThingDef>(ref this.originalDef, "originalDef", null, false);
        }

    }
}
