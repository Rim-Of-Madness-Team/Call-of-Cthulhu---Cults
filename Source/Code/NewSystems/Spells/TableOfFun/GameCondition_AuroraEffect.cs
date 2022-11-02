// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using RimWorld;
using UnityEngine;
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
    public class GameCondition_AuroraEffect : GameCondition
    {
        private static ColorInt colorInt = new ColorInt(r: 0, g: 141, b: 153); //Green
        private static ColorInt colorInt2 = new ColorInt(r: 141, g: 0, b: 153); //Purple
        private static ColorInt transition = new ColorInt(r: 0, g: 141, b: 153); //Green
        private readonly int LerpTicks = 200;
        private readonly int switchTicks = 5000;

        private SkyColorSet AuroraSkyColors =
            new SkyColorSet(sky: transition.ToColor, shadow: Color.white, overlay: new Color(r: 0.6f, g: 0.6f, b: 0.6f), saturation: 0.8f);

        private bool firstTick = true;
        private float Green;

        private float Red = 141f;
        private int switchCount = 5000;

        private bool switchTime;

        public override string Label
        {
            get
            {
                string temp;
                if (Find.WorldGrid.LongLatOf(tileID: Find.CurrentMap.Tile).y >= 74)
                {
                    temp = " " + "Borealis".Translate();
                }
                else
                {
                    temp = " " + "Australis".Translate();
                }

                return def.label + temp;
            }
        }

        public override float SkyTargetLerpFactor(Map map)
        {
            return GameConditionUtility.LerpInOutValue(timePassed: TicksPassed, timeLeft: TicksLeft, lerpTime: LerpTicks);
        }

        public override void GameConditionTick()
        {
            base.GameConditionTick();
            var affectedMaps = AffectedMaps;
            if (firstTick)
            {
                foreach (var map in affectedMaps)
                {
                    foreach (var pawn in map.mapPawns.FreeColonistsAndPrisoners)
                    {
                        if (!pawn.Position.Roofed(map: map) && pawn.def.race.IsFlesh)
                        {
                            pawn.needs.mood.thoughts.memories.TryGainMemory(def: CultsDefOf.Cults_SawAurora);
                        }
                    }
                }

                firstTick = false;
            }

            foreach (var map in affectedMaps)
            {
                foreach (var unused in map.mapPawns.FreeColonistsAndPrisoners)
                {
                    if (!switchTime)
                    {
                        Red -= 0.03f;
                        Green += 0.03f;
                        transition.r = (int) Red;
                        transition.g = (int) Green;
                        AuroraSkyColors = new SkyColorSet(sky: transition.ToColor, shadow: Color.white, overlay: new Color(r: 0.6f, g: 0.6f, b: 0.6f),
                            saturation: 0.8f);
                        SkyTarget(map: map);
                    }

                    if (switchTime)
                    {
                        Red += 0.03f;
                        Green -= 0.03f;
                        transition.r = (int) Red;
                        transition.g = (int) Green;
                        AuroraSkyColors = new SkyColorSet(sky: transition.ToColor, shadow: Color.white, overlay: new Color(r: 0.6f, g: 0.6f, b: 0.6f),
                            saturation: 0.8f);
                        SkyTarget(map: map);
                    }

                    if (switchCount >= 0)
                    {
                        switchCount -= 1;
                    }
                    else
                    {
                        switchCount = switchTicks;
                        switchTime = !switchTime;
                    }
                }
            }
        }

        public override SkyTarget? SkyTarget(Map map)
        {
            return new SkyTarget(glow: 0.85f, colorSet: AuroraSkyColors, lightsourceShineSize: 1f, lightsourceShineIntensity: 1f);
        }
    }
}