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
                Messages.Message("Note: Cosmic Horrors mod isn't loaded. Megaspiders will be summoned instead.",
                    MessageTypeDefOf.NeutralEvent);
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
                Utility.SpawnPawnsOfCountAt(DefDatabase<PawnKindDef>.GetNamed("ROM_StarVampire"),
                    altar(map).Position, map, 1,
                    Find.World.factionManager.FirstFactionOfDef(FactionDefOf.AncientsHostile));
            }
            else
            {
                Utility.SpawnPawnsOfCountAt(PawnKindDefOf.Megaspider, altar(map).Position, map,
                    Rand.Range(1, 2), Find.FactionManager.FirstFactionOfDef(FactionDefOf.AncientsHostile), true);
            }

            Messages.Message("A star vampire is unleashed.", MessageTypeDefOf.ThreatBig);

            Utility.ApplyTaleDef("Cults_SpellStarVampireVisit", map);

            return true;
        }
    }
}