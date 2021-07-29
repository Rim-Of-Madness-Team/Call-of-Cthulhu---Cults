// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
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
    public class SpellWorker_NeedAHand : SpellWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            //Cthulhu.Utility.DebugReport("
            //: " + this.def.defName);
            return true;
        }

        protected Pawn TestPawn(Map map)
        {
            var list = PawnsToTransmogrify(map).InRandomOrder();
            return !list.TryRandomElement(out var pawn) ? null : pawn;
        }


        public IEnumerable<Pawn> PawnsToTransmogrify(Map map)
        {
            //Get a pawn downed or bed-ridden incapable of moving
            var one = from Pawn peeps in map.mapPawns.FreeColonistsSpawned
                where peeps.RaceProps.Humanlike && peeps.Faction == Faction.OfPlayer && !peeps.Dead &&
                      (peeps.Downed || peeps.InBed()) && !peeps.health.capacities.CapableOf(PawnCapacityDefOf.Moving)
                select peeps;
            if (one.Any())
            {
                return one;
            }

            one = from Pawn peeps in map.mapPawns.FreeColonistsSpawned
                where peeps.RaceProps.Humanlike && peeps.Faction == Faction.OfPlayer && !peeps.Dead
                select peeps;
            return one;
        }


        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var pawn = TestPawn((Map) parms.target);
            BodyPartRecord tempRecord = null;
            foreach (var current in pawn.RaceProps.body.AllParts.InRandomOrder())
            {
                if (current.def != BodyPartDefOf.Leg && current.def != BodyPartDefOf.Arm &&
                    current.def != BodyPartDefOf.Hand && current.def != BodyPartDefOf.Eye &&
                    current.def != BodyPartDefOf.Jaw)
                {
                    continue;
                }

                if (!pawn.health.hediffSet.PartIsMissing(current))
                {
                    continue;
                }

                pawn.health.RestorePart(current);
                tempRecord = current;
                goto Leap;
            }

            foreach (var current in pawn.RaceProps.body.AllParts.InRandomOrder())
            {
                if (current.def != BodyPartDefOf.Leg && current.def != BodyPartDefOf.Arm &&
                    current.def != BodyPartDefOf.Hand && current.def != BodyPartDefOf.Eye &&
                    current.def != BodyPartDefOf.Jaw)
                {
                    continue;
                }

                tempRecord = current;
                break;
            }

            Leap:

            //Error catch: Missing parts!
            if (tempRecord == null)
            {
                Log.Error("Couldn't find part of the pawn to replace.");
                return false;
            }

            pawn.health.AddHediff(CultsDefOf.Cults_TentacleArm, tempRecord);
            Messages.Message(
                pawn.LabelShort + "'s " + tempRecord.def.label +
                " has been replaced with an otherworldly tentacle appendage.", MessageTypeDefOf.PositiveEvent);

            return true;
        }
    }
}