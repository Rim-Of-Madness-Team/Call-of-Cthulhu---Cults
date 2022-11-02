// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

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
    public class SpellWorker_AspectOfCthulhu : SpellWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            //Cthulhu.Utility.DebugReport("
            //: " + this.def.defName);
            return true;
        }

        //protected Pawn TestPawn(Map map)
        //{
        //        //IEnumerable<Pawn> list = PawnsToTransmogrify(map).InRandomOrder<Pawn>();
        //        //Pawn pawn;
        //        //if (!list.TryRandomElement<Pawn>(out pawn))
        //        //{
        //        //    if (altar(map) != null)
        //        //    {
        //        //        if (altar(map).executioner != null)
        //                    return 
        //        //    }
        //        //}
        //        //return pawn;
        //}


        //public IEnumerable<Pawn> PawnsToTransmogrify(Map map)
        //{
        //        //Get a colonist downed or bed-ridden incapable of moving
        //        IEnumerable<Pawn> one = from Pawn peeps in map.mapPawns.FreeColonists
        //                                where (peeps.RaceProps.Humanlike && peeps.Faction == Faction.OfPlayer && !peeps.Dead) && (peeps.Downed || peeps.InBed()) && !peeps.health.capacities.CapableOf(PawnCapacityDefOf.Moving)
        //                                select peeps;
        //        if (one.Count<Pawn>() > 0)
        //        {
        //            return one;
        //        }

        //        //Get a colonist.
        //        IEnumerable<Pawn> two = from Pawn peeps in map.mapPawns.FreeColonists
        //                                where (peeps.RaceProps.Humanlike && peeps.Faction == Faction.OfPlayer && !peeps.Dead)
        //                                select peeps;
        //        return two;
        //}

        public void ApplyAspect(Pawn p, int count = 3)
        {
            if (count <= 0)
            {
                return;
            }

            var parms = new TargetingParameters
            {
                canTargetPawns = true
            };
            var foundPawn = false;
            Messages.Message(text: "Cults_AspectOfCthulhu_TargetACharacter".Translate(), def: MessageTypeDefOf.NeutralEvent);

            Find.Targeter.BeginTargeting(targetParams: parms, action: delegate(LocalTargetInfo t)
            {
                if (t.Thing is not Pawn pawn)
                {
                    return;
                }

                BodyPartRecord tempRecord = null;
                var isEye = false;
                foreach (var current in pawn.RaceProps.body.AllParts.InRandomOrder())
                {
                    if (current.def == BodyPartDefOf.Eye)
                    {
                        if (pawn.health.hediffSet.PartIsMissing(part: current))
                        {
                            isEye = true;
                            pawn.health.RestorePart(part: current);
                            tempRecord = current;
                            goto Leap;
                        }
                    }

                    if (current.def != BodyPartDefOf.Leg && current.def != BodyPartDefOf.Arm &&
                        current.def != BodyPartDefOf.Hand)
                    {
                        continue;
                    }

                    if (!pawn.health.hediffSet.PartIsMissing(part: current))
                    {
                        continue;
                    }

                    pawn.health.RestorePart(part: current);
                    tempRecord = current;
                    goto Leap;
                }

                foreach (var current in pawn.RaceProps.body.AllParts.InRandomOrder())
                {
                    if (current.def == BodyPartDefOf.Eye)
                    {
                        isEye = true;
                        tempRecord = current;
                        break;
                    }

                    if (current.def != BodyPartDefOf.Leg && current.def != BodyPartDefOf.Arm &&
                        current.def != BodyPartDefOf.Hand)
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
                    Log.Error(text: "Couldn't find part of the pawn to replace.");
                    return;
                }

                pawn.health.AddHediff(def: isEye ? CultsDefOf.Cults_CthulhidEyestalk : CultsDefOf.Cults_CthulhidTentacle,
                    part: tempRecord);

                Messages.Message(text: "Cults_AspectOfCthulhuDesc".Translate(
                    arg1: pawn.LabelShort, arg2: tempRecord.def.label), def: MessageTypeDefOf.PositiveEvent);
                pawn.Map.GetComponent<MapComponent_SacrificeTracker>().lastLocation = pawn.Position;
                foundPawn = true;
            }, caster: null, actionWhenFinished: delegate
            {
                if (!foundPawn)
                {
                    LongEventHandler.QueueLongEvent(action: delegate { ApplyAspect(p: p, count: count - 1); }, textKey: "aspectOfCthulhu", doAsynchronously: false,
                        exceptionHandler: null);
                }
            });
        }


        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var map = parms.target as Map;
            var pawn = altar(map: map).SacrificeData.Executioner;
            ApplyAspect(p: pawn);
            return true;
        }
    }
}