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
    public class SpellWorker_TheKingInYellow : SpellWorker
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
            //Spawn some goats
            //Cthulhu.Utility.SpawnPawnsOfCountAt(CultDefOfs.BlackIbex, altar.Position, Rand.Range(2, 5), Faction.OfPlayer);

            //Spawn a fertility idol.
            Utility.SpawnThingDefOfCountAt(of: CultsDefOf.Cults_TheKingInYellow, count: 1,
                target: new TargetInfo(cell: altar(map: map).RandomAdjacentCell8Way(), map: map));

            //Spawn a 
            Messages.Message(text: "The sacred play appears before the sacrificers.", def: MessageTypeDefOf.PositiveEvent);

            //Cthulhu.Utility.ApplyTaleDef("Cults_SpellFertilityRitual", map);
            return true;
        }
    }
}