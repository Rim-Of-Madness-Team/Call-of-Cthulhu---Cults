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
    public class SpellWorker_StarVampireVisit : SpellWorker
    {
        public override bool CanSummonNow(Map map)
        {
            if (!Utility.IsCosmicHorrorsLoaded())
            {
                Messages.Message(text: "Note: Cosmic Horrors mod isn't loaded. Megaspiders will be summoned instead.",
                    def: MessageTypeDefOf.NeutralEvent);
            }

            //Cthulhu.Utility.DebugReport("CanFire: " + this.def.defName);
            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var map = parms.target as Map;
            //Spawn a Dark Young
            if (Utility.IsCosmicHorrorsLoaded())
            {
                Utility.SpawnPawnsOfCountAt(kindDef: DefDatabase<PawnKindDef>.GetNamed(defName: "ROM_StarVampire"),
                    at: altar(map: map).Position, map: map, count: 1,
                    fac: Find.World.factionManager.FirstFactionOfDef(facDef: FactionDefOf.AncientsHostile));
            }
            else
            {
                Utility.SpawnPawnsOfCountAt(kindDef: PawnKindDefOf.Megaspider, at: altar(map: map).Position, map: map,
                    count: Rand.Range(min: 1, max: 2), fac: Find.FactionManager.FirstFactionOfDef(facDef: FactionDefOf.AncientsHostile), berserk: true);
            }

            Messages.Message(text: "A star vampire is unleashed.", def: MessageTypeDefOf.ThreatBig);

            Utility.ApplyTaleDef(defName: "Cults_SpellStarVampireVisit", map: map);

            return true;
        }
    }
}