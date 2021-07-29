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

            var d20 = Rand.Range(1, 20);

            if (d20 <= 1)
            {
                MoteMaker.ThrowText(dinfo.Instigator.DrawPos, dinfo.Instigator.Map, "Critical Failure",
                    12.0f);
                if (dinfo.Instigator == null)
                {
                    return result;
                }

                if (dinfo.Instigator is Pawn pawn2)
                {
                    pawn2.TakeDamage(new DamageInfo(DamageDefOf.Stun, 60));
                }

                return result;
            }

            if (d20 <= 5)
            {
                MoteMaker.ThrowText(dinfo.Instigator.DrawPos, dinfo.Instigator.Map, "Failure", 12.0f);
                if (dinfo.Instigator == null)
                {
                    return result;
                }

                if (dinfo.Instigator is Pawn pawn2)
                {
                    pawn2.TakeDamage(new DamageInfo(DamageDefOf.Stun, 10));
                }

                return result;
            }

            if (d20 <= 10)
            {
                MoteMaker.ThrowText(dinfo.Instigator.DrawPos, dinfo.Instigator.Map, "Success", 12.0f);
                pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Wander_Psychotic,
                    "psionic shock");

                return result;
            }

            if (d20 <= 15)
            {
                MoteMaker.ThrowText(dinfo.Instigator.DrawPos, dinfo.Instigator.Map, "Success", 12.0f);
                pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk,
                    "psionic shock");

                return result;
            }

            if (d20 < 18)
            {
                MoteMaker.ThrowText(dinfo.Instigator.DrawPos, dinfo.Instigator.Map, "Success", 12.0f);
                var part = pawn.health.hediffSet.GetBrain();
                if (part == null)
                {
                    Log.ErrorOnce("Cults :: Missing Brain", 6781923);
                }

                pawn.TakeDamage(new DamageInfo(CultsDefOf.Cults_Psionic, Rand.Range(5, 8), 1f, -1,
                    dinfo.Instigator, part));

                return result;
            }
            else
            {
                MoteMaker.ThrowText(dinfo.Instigator.DrawPos, dinfo.Instigator.Map, "Critical Success",
                    12.0f);
                var part = pawn.health.hediffSet.GetBrain();
                if (part == null)
                {
                    Log.ErrorOnce("Cults :: Missing Brain", 6781923);
                }

                victim.TakeDamage(new DamageInfo(CultsDefOf.Cults_Psionic, 9999, 1f, -1, dinfo.Instigator,
                    part));

                return result;
            }
        }
    }
}