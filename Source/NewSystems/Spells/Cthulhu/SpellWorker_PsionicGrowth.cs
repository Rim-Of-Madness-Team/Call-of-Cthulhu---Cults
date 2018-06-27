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
    public class SpellWorker_PsionicGrowth : SpellWorker
    {

        protected Pawn pawn(Map map)
        {
            Pawn pawn = null;
            if (altar(map) != null)
            {
                pawn = altar(map).tempExecutioner;
            }
            return pawn;
        }

        public BodyPartRecord GetHead(Pawn pawn)
        {
            foreach (BodyPartRecord current in pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined))
            {
                if (current.def == BodyPartDefOf.Head) return current;
            }
            return null;
        }



        public override bool CanSummonNow(Map map)
        {
            if (pawn(map) == null)
            {
                Messages.Message("Executioner is missing.", MessageTypeDefOf.RejectInput);
                return false;
            }

            //If they have no brain... don't do this.
            if (pawn(map).health.hediffSet.GetBrain() == null)
            {
                Messages.Message(pawn(map).LabelShort + " is missing a brain to enhance.", MessageTypeDefOf.RejectInput);
                return false;
            }

            //Check if their brain is already upgraded.
            foreach (Hediff current in pawn(map).health.hediffSet.hediffs)
            {
                if (current.def == CultsDefOf.Cults_PsionicBrain)
                {
                    Messages.Message(pawn(map).LabelShort + " already posesses a brain with psionic power.", MessageTypeDefOf.RejectInput);
                    return false;
                }
            }

            //Cthulhu.Utility.DebugReport("CanFire: " + this.def.defName);
            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = parms.target as Map;

            BodyPartRecord brainRecord = pawn(map).health.hediffSet.GetBrain();
            BodyPartRecord headRecord = GetHead(pawn(map));
            //Error catch: Missing head!
            //if (tempRecord == null)
            //{
                //Log.Error("Couldn't find head part of the pawn(map) to give random damage.");
                //return false;
            //}


            int rand = new System.Random().Next(1, 100);
            if (rand > 90)
            {
                // No effect
            }
            else if (rand > 50 && rand <= 90)
            {
                //A15 code...
                //HediffDef quiet = null;
                //BodyPartDamageInfo value = new BodyPartDamageInfo(tempRecord, false, quiet);
                //pawn(map).TakeDamage(new DamageInfo(DamageDefOf.Cut, Rand.Range(5, 8), null, new BodyPartDamageInfo?(value), null));
                if (headRecord != null) pawn(map).TakeDamage(new DamageInfo(DamageDefOf.Cut, Rand.Range(5, 8), 1f, -1f, null, headRecord, null));
            }
            else if (rand > 10 && rand <= 50)
            {
                //HediffDef quiet = null;
                //BodyPartDamageInfo value = new BodyPartDamageInfo(tempRecord, false, quiet);
                if (headRecord != null) pawn(map).TakeDamage(new DamageInfo(DamageDefOf.Blunt, Rand.Range(8, 10), 1f, -1f, null, headRecord, null));
            }
            else if (rand <= 10)
            {
                //HediffDef quiet = null;
                //BodyPartDamageInfo value = new BodyPartDamageInfo(tempRecord, false, quiet);
                if (headRecord != null)
                {
                    pawn(map).TakeDamage(new DamageInfo(DamageDefOf.Bite, Rand.Range(10, 12), -1f, 1f, null, headRecord, null));
                    pawn(map).health.AddHediff(HediffDefOf.WoundInfection, headRecord, null);
                }
            }

            pawn(map).health.AddHediff(CultsDefOf.Cults_PsionicBrain, pawn(map).health.hediffSet.GetBrain(), null);
            Messages.Message(pawn(map).LabelShort + "'s brain has been enhanced with great psionic power.", MessageTypeDefOf.PositiveEvent);

            map.GetComponent<MapComponent_SacrificeTracker>().lastLocation = pawn(map).Position;
            Cthulhu.Utility.ApplyTaleDef("Cults_SpellPsionicGrowth", pawn(map));

            return true;
        }
    }
}
