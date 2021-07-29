// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

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
    public class SpellWorker_Reincarnation : SpellWorker
    {
        private static Map map;
        private static IntVec3 pos;
        private static Pawn exSacrifice;
        private static Corpse deadBody;

        protected Corpse corpse(Map map)
        {
            var c = map.thingGrid.ThingAt<Corpse>(altar(map).Position);
            return c;
        }

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            //Cthulhu.Utility.DebugReport("
            //: " + this.def.defName);
            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            map = parms.target as Map;
            pos = altar(map).Position;
            exSacrifice = corpse(map).InnerPawn;
            deadBody = corpse(map);

            LongEventHandler.QueueLongEvent(delegate
            {
                //Throw some smoke
                FleckMaker.ThrowDustPuff(pos, map, 2f);

                //Make the body strip and despawn around the altar
                deadBody.Strip();
                deadBody.DeSpawn();

                //Trigger the nightmare event on the altar
                altar(map).NightmareEvent();

                Utility.ApplyTaleDef("Cults_SpellReincarnation", deadBody.InnerPawn);
            }, "Cults_SpellReincarnation", false, null);
            return true;
        }
    }
}