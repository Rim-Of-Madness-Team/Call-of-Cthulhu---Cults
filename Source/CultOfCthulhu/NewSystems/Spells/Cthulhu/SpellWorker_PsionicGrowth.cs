// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using System;
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
            foreach (var current in pawn.health.hediffSet.GetNotMissingParts())
            {
                if (current.def == BodyPartDefOf.Head)
                {
                    return current;
                }
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
                Messages.Message(pawn(map).LabelShort + " is missing a brain to enhance.",
                    MessageTypeDefOf.RejectInput);
                return false;
            }

            //Check if their brain is already upgraded.
            foreach (var current in pawn(map).health.hediffSet.hediffs)
            {
                if (current.def != CultsDefOf.Cults_PsionicBrain)
                {
                    continue;
                }

                Messages.Message(pawn(map).LabelShort + " already posesses a brain with psionic power.",
                    MessageTypeDefOf.RejectInput);
                return false;
            }

            //Cthulhu.Utility.DebugReport("CanFire: " + this.def.defName);
            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var map = parms.target as Map;
            _ = pawn(map).health.hediffSet.GetBrain();
            var headRecord = GetHead(pawn(map));
            //Error catch: Missing head!
            //if (tempRecord == null)
            //{
            //Log.Error("Couldn't find head part of the pawn(map) to give random damage.");
            //return false;
            //}


            var rand = new Random().Next(1, 100);
            switch (rand)
            {
                case > 90:
                    // No effect
                    break;
                case > 50 and <= 90:
                {
                    //A15 code...
                    //HediffDef quiet = null;
                    //BodyPartDamageInfo value = new BodyPartDamageInfo(tempRecord, false, quiet);
                    //pawn(map).TakeDamage(new DamageInfo(DamageDefOf.Cut, Rand.Range(5, 8), null, new BodyPartDamageInfo?(value), null));
                    if (headRecord != null)
                    {
                        pawn(map).TakeDamage(new DamageInfo(DamageDefOf.Cut, Rand.Range(5, 8), 1f, -1f, null,
                            headRecord));
                    }

                    break;
                }
                case > 10 and <= 50:
                {
                    //HediffDef quiet = null;
                    //BodyPartDamageInfo value = new BodyPartDamageInfo(tempRecord, false, quiet);
                    if (headRecord != null)
                    {
                        pawn(map).TakeDamage(
                            new DamageInfo(DamageDefOf.Blunt, Rand.Range(8, 10), 1f, -1f, null, headRecord));
                    }

                    break;
                }
                case <= 10:
                {
                    //HediffDef quiet = null;
                    //BodyPartDamageInfo value = new BodyPartDamageInfo(tempRecord, false, quiet);
                    if (headRecord != null)
                    {
                        pawn(map).TakeDamage(
                            new DamageInfo(DamageDefOf.Bite, Rand.Range(10, 12), -1f, 1f, null, headRecord));
                        pawn(map).health.AddHediff(HediffDefOf.WoundInfection, headRecord);
                    }

                    break;
                }
            }

            pawn(map).health.AddHediff(CultsDefOf.Cults_PsionicBrain, pawn(map).health.hediffSet.GetBrain());
            Messages.Message(pawn(map).LabelShort + "'s brain has been enhanced with great psionic power.",
                MessageTypeDefOf.PositiveEvent);

            if (map == null)
            {
                return true;
            }

            map.GetComponent<MapComponent_SacrificeTracker>().lastLocation = pawn(map).Position;
            Utility.ApplyTaleDef("Cults_SpellPsionicGrowth", pawn(map));

            return true;
        }
    }
}