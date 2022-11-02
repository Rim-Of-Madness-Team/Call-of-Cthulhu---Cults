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
            var c = map.thingGrid.ThingAt<Corpse>(c: altar(map: map).Position);
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
            pos = altar(map: map).Position;
            exSacrifice = corpse(map: map).InnerPawn;
            deadBody = corpse(map: map);

            LongEventHandler.QueueLongEvent(action: delegate
            {
                //Throw some smoke
                FleckMaker.ThrowDustPuff(cell: pos, map: map, scale: 2f);

                //Make the body strip and despawn around the altar
                deadBody.Strip();
                deadBody.DeSpawn();

                //Trigger the nightmare event on the altar
                altar(map: map).NightmareEvent();

                Utility.ApplyTaleDef(defName: "Cults_SpellReincarnation", pawn: deadBody.InnerPawn);
            }, textKey: "Cults_SpellReincarnation", doAsynchronously: false, exceptionHandler: null);
            return true;
        }
    }
}