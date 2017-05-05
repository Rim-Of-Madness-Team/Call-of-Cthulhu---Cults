using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace CultOfCthulhu
{
    public class PawnFlyerDef : ThingDef
    {

        public float flightSpeed;

        public int flightPawnLimit;

        public int flyableDistance;

        public SoundDef takeOffSound;

        public SoundDef landingSound;

        public SoundDef dismountSound;

        public TaleDef landedTale;

        public ThingDef leavingDef;

        public WorldObjectDef travelingDef;

        public ThingDef incomingDef;

        public ThingDef landedDef;
    }
}
