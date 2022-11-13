using RimWorld;
using Verse;

namespace CultOfCthulhu
{
    public class PawnFlyerDef : ThingDef
    {
        public SoundDef dismountSound;

        public int flightPawnLimit;

        public float flightSpeed;

        public int flyableDistance;

        public ThingDef incomingDef;

        public TaleDef landedTale;

        public SoundDef landingSound;

        public ThingDef leavingDef;

        public SoundDef takeOffSound;

        public WorldObjectDef travelingDef;
    }
}