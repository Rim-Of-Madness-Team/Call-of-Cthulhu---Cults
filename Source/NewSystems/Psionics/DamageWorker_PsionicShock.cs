using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace CultOfCthulhu
{
    class DamageWorker_PsionicShock : DamageWorker
    {
        public override float Apply(DamageInfo dinfo, Thing victim)
        {
            Pawn pawn = victim as Pawn;
            if (pawn != null)
            {
                if (pawn.Spawned && !pawn.Dead)
                {

                    if (pawn.health != null)
                    {

                        int d20 = Rand.Range(1, 20);

                        if (d20 <= 1)
                        {
                            MoteMaker.ThrowText(dinfo.Instigator.DrawPos, dinfo.Instigator.Map, "Critical Failure", 12.0f);
                            if (dinfo.Instigator != null)
                            {
                                Pawn pawn2 = dinfo.Instigator as Pawn;
                                if (pawn2 != null)
                                {
                                    pawn2.TakeDamage(new DamageInfo(DamageDefOf.Stun, 60));
                                }
                            }
                            return 0f;
                        }
                        else if (d20 <= 5)
                        {
                            MoteMaker.ThrowText(dinfo.Instigator.DrawPos, dinfo.Instigator.Map, "Failure", 12.0f);
                            if (dinfo.Instigator != null)
                            {
                                Pawn pawn2 = dinfo.Instigator as Pawn;
                                if (pawn2 != null)
                                {
                                    pawn2.TakeDamage(new DamageInfo(DamageDefOf.Stun, 10));
                                }
                            }
                            return 0f;
                        }
                        else if (d20 <= 10)
                        {
                            MoteMaker.ThrowText(dinfo.Instigator.DrawPos, dinfo.Instigator.Map, "Success", 12.0f);
                            pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.WanderPsychotic, "psionic shock");
                            return 0f;
                        }
                        else if (d20 <= 15)
                        {
                            MoteMaker.ThrowText(dinfo.Instigator.DrawPos, dinfo.Instigator.Map, "Success", 12.0f);
                            pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, "psionic shock");
                            return 0f;
                        }
                        else if (d20 < 18)
                        {
                            MoteMaker.ThrowText(dinfo.Instigator.DrawPos, dinfo.Instigator.Map, "Success", 12.0f);
                            BodyPartRecord part = pawn.health.hediffSet.GetBrain();
                            if (part == null) Log.ErrorOnce("Cults :: Missing Brain", 6781923);
                            pawn.TakeDamage(new DamageInfo(CultsDefOf.Cults_Psionic, Rand.Range(5, 8), -1, dinfo.Instigator, part));
                            return 0f;
                        }
                        else
                        {
                            MoteMaker.ThrowText(dinfo.Instigator.DrawPos, dinfo.Instigator.Map, "Critical Success", 12.0f);
                            BodyPartRecord part = pawn.health.hediffSet.GetBrain();
                            if (part == null) Log.ErrorOnce("Cults :: Missing Brain", 6781923);
                            victim.TakeDamage(new DamageInfo(CultsDefOf.Cults_Psionic, 9999, -1, dinfo.Instigator, part));
                            return 0f;
                        }
                    }

                }
            }

            return 0f;
        }
    }
}
