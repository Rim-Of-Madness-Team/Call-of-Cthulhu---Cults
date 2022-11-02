using Verse;

namespace CultOfCthulhu
{
    internal class Building_BurstingTentacle : Building
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
            }
            else
            {
                ticksUntilFlicker = defaultTicksUntilFlicker;
                Thing newTentacle =
                    (Building_BurstingTentacle) ThingMaker.MakeThing(def: ThingDef.Named(defName: "BurstingTentacle"));
                GenPlace.TryPlaceThing(thing: newTentacle, center: Position, map: Map, mode: ThingPlaceMode.Direct);
            }
        }
    }
}