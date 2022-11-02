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
            if (altar(map: map) != null)
            {
                pawn = altar(map: map).tempExecutioner;
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
            if (pawn(map: map) == null)
            {
                Messages.Message(text: "Executioner is missing.", def: MessageTypeDefOf.RejectInput);
                return false;
            }

            //If they have no brain... don't do this.
            if (pawn(map: map).health.hediffSet.GetBrain() == null)
            {
                Messages.Message(text: pawn(map: map).LabelShort + " is missing a brain to enhance.",
                    def: MessageTypeDefOf.RejectInput);
                return false;
            }

            //Check if their brain is already upgraded.
            foreach (var current in pawn(map: map).health.hediffSet.hediffs)
            {
                if (current.def != CultsDefOf.Cults_PsionicBrain)
                {
                    continue;
                }

                Messages.Message(text: pawn(map: map).LabelShort + " already posesses a brain with psionic power.",
                    def: MessageTypeDefOf.RejectInput);
                return false;
            }

            //Cthulhu.Utility.DebugReport("CanFire: " + this.def.defName);
            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var map = parms.target as Map;
            _ = pawn(map: map).health.hediffSet.GetBrain();
            var headRecord = GetHead(pawn: pawn(map: map));
            //Error catch: Missing head!
            //if (tempRecord == null)
            //{
            //Log.Error("Couldn't find head part of the pawn(map) to give random damage.");
            //return false;
            //}


            var rand = new Random().Next(minValue: 1, maxValue: 100);
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
                        pawn(map: map).TakeDamage(dinfo: new DamageInfo(def: DamageDefOf.Cut, amount: Rand.Range(min: 5, max: 8), armorPenetration: 1f, angle: -1f, instigator: null,
                            hitPart: headRecord));
                    }

                    break;
                }
                case > 10 and <= 50:
                {
                    //HediffDef quiet = null;
                    //BodyPartDamageInfo value = new BodyPartDamageInfo(tempRecord, false, quiet);
                    if (headRecord != null)
                    {
                        pawn(map: map).TakeDamage(
                            dinfo: new DamageInfo(def: DamageDefOf.Blunt, amount: Rand.Range(min: 8, max: 10), armorPenetration: 1f, angle: -1f, instigator: null, hitPart: headRecord));
                    }

                    break;
                }
                case <= 10:
                {
                    //HediffDef quiet = null;
                    //BodyPartDamageInfo value = new BodyPartDamageInfo(tempRecord, false, quiet);
                    if (headRecord != null)
                    {
                        pawn(map: map).TakeDamage(
                            dinfo: new DamageInfo(def: DamageDefOf.Bite, amount: Rand.Range(min: 10, max: 12), armorPenetration: -1f, angle: 1f, instigator: null, hitPart: headRecord));
                        pawn(map: map).health.AddHediff(def: HediffDefOf.WoundInfection, part: headRecord);
                    }

                    break;
                }
            }

            pawn(map: map).health.AddHediff(def: CultsDefOf.Cults_PsionicBrain, part: pawn(map: map).health.hediffSet.GetBrain());
            Messages.Message(text: pawn(map: map).LabelShort + "'s brain has been enhanced with great psionic power.",
                def: MessageTypeDefOf.PositiveEvent);

            if (map == null)
            {
                return true;
            }

            map.GetComponent<MapComponent_SacrificeTracker>().lastLocation = pawn(map: map).Position;
            Utility.ApplyTaleDef(defName: "Cults_SpellPsionicGrowth", pawn: pawn(map: map));

            return true;
        }
    }
}