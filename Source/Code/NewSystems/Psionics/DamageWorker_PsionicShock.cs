using RimWorld;
using Verse;

namespace CultOfCthulhu
{
    internal class DamageWorker_PsionicShock : DamageWorker
    {
        public override DamageResult Apply(DamageInfo dinfo, Thing victim)
        {
            var result = new DamageResult();
            if (victim is not Pawn pawn)
            {
                return result;
            }

            if (!pawn.Spawned || pawn.Dead)
            {
                return result;
            }

            if (pawn.health == null)
            {
                return result;
            }

            var d20 = Rand.Range(min: 1, max: 20);

            if (d20 <= 1)
            {
                MoteMaker.ThrowText(loc: dinfo.Instigator.DrawPos, map: dinfo.Instigator.Map, text: "Critical Failure",
                    timeBeforeStartFadeout: 12.0f);
                if (dinfo.Instigator == null)
                {
                    return result;
                }

                if (dinfo.Instigator is Pawn pawn2)
                {
                    pawn2.TakeDamage(dinfo: new DamageInfo(def: DamageDefOf.Stun, amount: 60));
                }

                return result;
            }

            if (d20 <= 5)
            {
                MoteMaker.ThrowText(loc: dinfo.Instigator.DrawPos, map: dinfo.Instigator.Map, text: "Failure", timeBeforeStartFadeout: 12.0f);
                if (dinfo.Instigator == null)
                {
                    return result;
                }

                if (dinfo.Instigator is Pawn pawn2)
                {
                    pawn2.TakeDamage(dinfo: new DamageInfo(def: DamageDefOf.Stun, amount: 10));
                }

                return result;
            }

            if (d20 <= 10)
            {
                MoteMaker.ThrowText(loc: dinfo.Instigator.DrawPos, map: dinfo.Instigator.Map, text: "Success", timeBeforeStartFadeout: 12.0f);
                pawn.mindState.mentalStateHandler.TryStartMentalState(stateDef: MentalStateDefOf.Wander_Psychotic,
                    reason: "psionic shock");

                return result;
            }

            if (d20 <= 15)
            {
                MoteMaker.ThrowText(loc: dinfo.Instigator.DrawPos, map: dinfo.Instigator.Map, text: "Success", timeBeforeStartFadeout: 12.0f);
                pawn.mindState.mentalStateHandler.TryStartMentalState(stateDef: MentalStateDefOf.Berserk,
                    reason: "psionic shock");

                return result;
            }

            if (d20 < 18)
            {
                MoteMaker.ThrowText(loc: dinfo.Instigator.DrawPos, map: dinfo.Instigator.Map, text: "Success", timeBeforeStartFadeout: 12.0f);
                var part = pawn.health.hediffSet.GetBrain();
                if (part == null)
                {
                    Log.ErrorOnce(text: "Cults :: Missing Brain", key: 6781923);
                }

                pawn.TakeDamage(dinfo: new DamageInfo(def: CultsDefOf.Cults_Psionic, amount: Rand.Range(min: 5, max: 8), armorPenetration: 1f, angle: -1,
                    instigator: dinfo.Instigator, hitPart: part));

                return result;
            }
            else
            {
                MoteMaker.ThrowText(loc: dinfo.Instigator.DrawPos, map: dinfo.Instigator.Map, text: "Critical Success",
                    timeBeforeStartFadeout: 12.0f);
                var part = pawn.health.hediffSet.GetBrain();
                if (part == null)
                {
                    Log.ErrorOnce(text: "Cults :: Missing Brain", key: 6781923);
                }

                victim.TakeDamage(dinfo: new DamageInfo(def: CultsDefOf.Cults_Psionic, amount: 9999, armorPenetration: 1f, angle: -1, instigator: dinfo.Instigator,
                    hitPart: part));

                return result;
            }
        }
    }
}