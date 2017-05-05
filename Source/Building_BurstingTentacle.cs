using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace CultOfCthulhu
{
    class Building_BurstingTentacle : Building
    {
        public const int defaultTicksUntilFlicker = 500;
        public int ticksUntilFlicker = 500;

        public override void Tick()
        {
            flickerCheck();
            base.Tick();
        }

        public void flickerCheck()
        {
            if (ticksUntilFlicker > 0)
            {
                ticksUntilFlicker -= 1;
                return;
            }
            else
            {
                ticksUntilFlicker = defaultTicksUntilFlicker;
                Thing newTentacle = (Building_BurstingTentacle)ThingMaker.MakeThing(ThingDef.Named("BurstingTentacle"), null);
                GenPlace.TryPlaceThing(newTentacle, this.Position, this.Map, ThingPlaceMode.Direct);
            }
        }
    }
}
