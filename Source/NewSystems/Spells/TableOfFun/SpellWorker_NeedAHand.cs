// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

// ----------------------------------------------------------------------
// These are RimWorld-specific usings. Activate/Deactivate what you need:
// ----------------------------------------------------------------------
using UnityEngine;         // Always needed
//using VerseBase;         // Material/Graphics handling functions are found here
using Verse;               // RimWorld universal objects are here (like 'Building')
using Verse.AI;          // Needed when you do something with the AI
using Verse.AI.Group;
using Verse.Sound;       // Needed when you do something with Sound
using Verse.Noise;       // Needed when you do something with Noises
using RimWorld;            // RimWorld specific functions are found here (like 'Building_Battery')
using RimWorld.Planet;   // RimWorld specific functions for world creation
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
                IEnumerable<Pawn> list = PawnsToTransmogrify(map).InRandomOrder<Pawn>();
                Pawn pawn;
                if (!list.TryRandomElement<Pawn>(out pawn))
                {
                    return null;
                }
                return pawn;
        }


        public IEnumerable<Pawn> PawnsToTransmogrify(Map map)
        {
                //Get a pawn downed or bed-ridden incapable of moving
                IEnumerable<Pawn> one = from Pawn peeps in map.mapPawns.FreeColonistsSpawned
                                        where (peeps.RaceProps.Humanlike && peeps.Faction == Faction.OfPlayer && !peeps.Dead) && (peeps.Downed || peeps.InBed()) && !peeps.health.capacities.CapableOf(PawnCapacityDefOf.Moving)
                                        select peeps;
                    if (one.Count<Pawn>() > 0)
                    {
                        return one;
                    }
                one = from Pawn peeps in map.mapPawns.FreeColonistsSpawned
                                        where (peeps.RaceProps.Humanlike && peeps.Faction == Faction.OfPlayer && !peeps.Dead)
                                        select peeps;
                return one;
        }


        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Pawn pawn = TestPawn((Map)parms.target);
            BodyPartRecord tempRecord = null;
            foreach (BodyPartRecord current in pawn.RaceProps.body.AllParts.InRandomOrder<BodyPartRecord>())
                {
                    if (current.def == BodyPartDefOf.Leg ||
                        current.def == BodyPartDefOf.Arm ||
                        current.def == BodyPartDefOf.Hand ||
                        current.def == BodyPartDefOf.Eye ||
                        current.def == BodyPartDefOf.Jaw)
                    {
                        if (pawn.health.hediffSet.PartIsMissing(current))
                        {
                            pawn.health.RestorePart(current);
                            tempRecord = current;
                            goto Leap;
                        }
                    }
                }
                foreach (BodyPartRecord current in pawn.RaceProps.body.AllParts.InRandomOrder<BodyPartRecord>())
                {
                    if (current.def == BodyPartDefOf.Leg ||
                        current.def == BodyPartDefOf.Arm ||
                        current.def == BodyPartDefOf.Hand ||
                        current.def == BodyPartDefOf.Eye ||
                        current.def == BodyPartDefOf.Jaw)
                    {
                        tempRecord = current;
                        break;
                    }
            }
            Leap:

            //Error catch: Missing parts!
            if (tempRecord == null)
            {
                Log.Error("Couldn't find part of the pawn to replace.");
                return false;
            }

            pawn.health.AddHediff(CultsDefOf.Cults_TentacleArm, tempRecord, null);
            Messages.Message(pawn.LabelShort + "'s " + tempRecord.def.label + " has been replaced with an otherworldly tentacle appendage.", MessageTypeDefOf.PositiveEvent);

            return true;

        }
    }
}
