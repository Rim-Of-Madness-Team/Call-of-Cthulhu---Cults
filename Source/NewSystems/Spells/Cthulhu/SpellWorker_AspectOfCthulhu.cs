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
            if (count <= 0) return;
            TargetingParameters parms = new TargetingParameters();
            parms.canTargetPawns = true;
            bool foundPawn = false;
            Messages.Message("Cults_AspectOfCthulhu_TargetACharacter".Translate(), MessageTypeDefOf.NeutralEvent);

            Find.Targeter.BeginTargeting(parms, delegate (LocalTargetInfo t)
            {
                if (t.Thing is Pawn pawn)
                {
                    BodyPartRecord tempRecord = null;
                    bool isEye = false;
                    foreach (BodyPartRecord current in pawn.RaceProps.body.AllParts.InRandomOrder<BodyPartRecord>())
                    {
                        if (current.def == BodyPartDefOf.LeftEye ||
                            current.def == BodyPartDefOf.RightEye)
                        {
                            if (pawn.health.hediffSet.PartIsMissing(current))
                            {
                                isEye = true;
                                pawn.health.RestorePart(current);
                                tempRecord = current;
                                goto Leap;
                            }
                        }

                        if (current.def == BodyPartDefOf.LeftLeg ||
                            current.def == BodyPartDefOf.RightLeg ||
                            current.def == BodyPartDefOf.LeftArm ||
                            current.def == BodyPartDefOf.RightArm ||
                            current.def == BodyPartDefOf.LeftHand ||
                            current.def == BodyPartDefOf.RightHand)
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
                        if (current.def == BodyPartDefOf.LeftEye ||
                            current.def == BodyPartDefOf.RightEye)
                        {
                            isEye = true;
                            tempRecord = current;
                            break;
                        }

                        if (current.def == BodyPartDefOf.LeftLeg ||
                            current.def == BodyPartDefOf.RightLeg ||
                            current.def == BodyPartDefOf.LeftArm ||
                            current.def == BodyPartDefOf.RightArm ||
                            current.def == BodyPartDefOf.LeftHand ||
                            current.def == BodyPartDefOf.RightHand)
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
                        return;
                    }

                    if (isEye) pawn.health.AddHediff(CultsDefOf.Cults_CthulhidEyestalk, tempRecord, null);
                    else pawn.health.AddHediff(CultsDefOf.Cults_CthulhidTentacle, tempRecord, null);
                    Messages.Message("Cults_AspectOfCthulhuDesc".Translate(new object[] { pawn.LabelShort, tempRecord.def.label }), MessageTypeDefOf.PositiveEvent);
                    pawn.Map.GetComponent<MapComponent_SacrificeTracker>().lastLocation = pawn.Position;
                    foundPawn = true;
                }

            }, null, delegate
            {
                if (!foundPawn)
                {
                    LongEventHandler.QueueLongEvent(delegate
                    {
                        this.ApplyAspect(p, count - 1);
                    }, "aspectOfCthulhu", false, null);
                }
            });

        }


        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = parms.target as Map;
            Pawn pawn = altar(map).SacrificeData.Executioner;
            ApplyAspect(pawn);
            return true;

        }
    }
}
