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
    public class SpellWorker_BlackIchor : SpellWorker
    {
        public override bool CanSummonNow(Map map)
        {
            return true;
        }

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            //Cthulhu.Utility.DebugReport("CanFire: " + this.def.defName);
            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var map = parms.target as Map;

            Utility.SpawnThingDefOfCountAt(of: CultsDefOf.Cults_BlackIchorMeal, count: Rand.Range(min: 18, max: 22),
                target: new TargetInfo(cell: altar(map: map).RandomAdjacentCell8Way(), map: map));

            Messages.Message(text: "Cults_BlackIchor_Spawns".Translate(), def: MessageTypeDefOf.PositiveEvent);

            Utility.ApplyTaleDef(defName: "Cults_SpellBlackIchor", map: map);
            return true;
        }
    }
}