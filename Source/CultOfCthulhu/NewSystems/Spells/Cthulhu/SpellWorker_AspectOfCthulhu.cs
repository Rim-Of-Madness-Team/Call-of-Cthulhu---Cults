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
            Messages.Message("Cults_AspectOfCthulhu_TargetACharacter".Translate(), MessageTypeDefOf.NeutralEvent);

            Find.Targeter.BeginTargeting(parms, delegate(LocalTargetInfo t)
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
                        if (pawn.health.hediffSet.PartIsMissing(current))
                        {
                            isEye = true;
                            pawn.health.RestorePart(current);
                            tempRecord = current;
                            goto Leap;
                        }
                    }

                    if (current.def != BodyPartDefOf.Leg && current.def != BodyPartDefOf.Arm &&
                        current.def != BodyPartDefOf.Hand)
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
                    Log.Error("Couldn't find part of the pawn to replace.");
                    return;
                }

                pawn.health.AddHediff(isEye ? CultsDefOf.Cults_CthulhidEyestalk : CultsDefOf.Cults_CthulhidTentacle,
                    tempRecord);

                Messages.Message("Cults_AspectOfCthulhuDesc".Translate(
                    pawn.LabelShort, tempRecord.def.label), MessageTypeDefOf.PositiveEvent);
                pawn.Map.GetComponent<MapComponent_SacrificeTracker>().lastLocation = pawn.Position;
                foundPawn = true;
            }, null, delegate
            {
                if (!foundPawn)
                {
                    LongEventHandler.QueueLongEvent(delegate { ApplyAspect(p, count - 1); }, "aspectOfCthulhu", false,
                        null);
                }
            });
        }


        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var map = parms.target as Map;
            var pawn = altar(map).SacrificeData.Executioner;
            ApplyAspect(pawn);
            return true;
        }
    }
}