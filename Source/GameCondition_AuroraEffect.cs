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
    public class GameCondition_AuroraEffect : GameCondition
    {
        private int LerpTicks = 200;
        private static ColorInt colorInt = new ColorInt(0, 141, 153); //Green
        private static ColorInt colorInt2 = new ColorInt(141, 0, 153); //Purple
        private static ColorInt transition = new ColorInt(0, 141, 153); //Green

        private float Red = 141f;
        private float Green = 0f;

        private bool switchTime = false;
        private int switchTicks = 5000;
        private int switchCount = 5000;

        private bool firstTick = true;

        private SkyColorSet AuroraSkyColors = new SkyColorSet(transition.ToColor, Color.white, new Color(0.6f, 0.6f, 0.6f), 0.8f);

        public override float SkyTargetLerpFactor()
        {
            return GameConditionUtility.LerpInOutValue((float)base.TicksPassed, (float)base.TicksLeft, (float)this.LerpTicks, 1f);
        }

        public override void GameConditionTick()
        {
            base.GameConditionTick();

            if (firstTick)
            {
                foreach (Pawn pawn in Map.mapPawns.FreeColonistsAndPrisoners)
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(CultsDefOf.Cults_SawAurora);
                }
                firstTick = false;
            }

            if (!switchTime)
            {
                Red -= 0.03f;
                Green += 0.03f;
                transition.r = (int)Red;
                transition.g = (int)Green;
                AuroraSkyColors = new SkyColorSet(transition.ToColor, Color.white, new Color(0.6f, 0.6f, 0.6f), 0.8f);
                SkyTarget();
            }
            if (switchTime)
            {
                Red += 0.03f;
                Green -= 0.03f;
                transition.r = (int)Red;
                transition.g = (int)Green;
                AuroraSkyColors = new SkyColorSet(transition.ToColor, Color.white, new Color(0.6f, 0.6f, 0.6f), 0.8f);
                SkyTarget();
            }

            if (switchCount >= 0)
            {
                switchCount -= 1;
            }
            else
            {
                switchCount = switchTicks;
                if (switchTime)
                {
                    //Cthulhu.Utility.DebugReport("Switch");
                    switchTime = false;
                }

                else
                {
                    //Cthulhu.Utility.DebugReport("Switch");
                    switchTime = true;
                }
            }
        }

        public override string Label
        {
            get
            {
                string temp = "";
                if (Find.WorldGrid.LongLatOf(Map.Tile).y >= 74) temp = " " + "Borealis".Translate();
                else temp = " " + "Australis".Translate();
                return this.def.label + temp;
            }
        }

        public override SkyTarget? SkyTarget()
        {
            return new SkyTarget?(new SkyTarget(0.85f, this.AuroraSkyColors, 1f, 1f));
        }
    }
}
