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
    public class SpellWorker_Reanimator : SpellWorker
    {
        protected Pawn innerSacrifice(Map map)
        {
            var c = map.thingGrid.ThingAt<Corpse>(c: altar(map: map).Position);
            return c.InnerPawn;
        }

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            //Cthulhu.Utility.DebugReport("CanFire: " + this.def.defName);
            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!(parms.target is Map map))
            {
                return false;
            }

            //Generate the zombie
            var pawn = ReanimatedPawnUtility.DoGenerateZombiePawnFromSource(sourcePawn: innerSacrifice(map: map));
            var intVec = innerSacrifice(map: map).Position.RandomAdjacentCell8Way();
            GenSpawn.Spawn(newThing: pawn, loc: intVec, map: map);
            innerSacrifice(map: map).Corpse.Destroy();
            //Destroy the corpse
            //Replace the innerSacrifice with the new pawn just in-case
            //altar.innerSacrifice = thing;
            map.GetComponent<MapComponent_SacrificeTracker>().lastLocation = intVec;
            Messages.Message(text: "The innerSacrifice reanimates and attacks.", def: MessageTypeDefOf.ThreatBig);
            Utility.ApplyTaleDef(defName: "Cults_SpellReanimator", pawn: pawn);
            return true;
        }
    }
}