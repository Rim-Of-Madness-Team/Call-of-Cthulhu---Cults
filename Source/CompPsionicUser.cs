using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using AbilityUser;

namespace CultOfCthulhu
{
    public class CompPsionicUser : AbilityUser.CompAbilityUser
    {
        public bool firstTick = false;

        public override bool TryTransformPawn()
        {
            return IsPsionic;
        }

        public void PostInitializeTick()
        {
            if (this.AbilityUser != null)
            {
                if (this.AbilityUser.Spawned)
                {
                    if (this.AbilityUser.story != null)
                    {
                        firstTick = true;
                        this.Initialize();
                        this.AddPawnAbility(CultsDefOf.Cults_PsionicBlast);
                        this.AddPawnAbility(CultsDefOf.Cults_PsionicShock);
                        this.AddPawnAbility(CultsDefOf.Cults_PsionicBurn);
                    }
                }
            }
        }

        public override void CompTick()
        {
            if (AbilityUser != null)
            {
                if (AbilityUser.Spawned)
                {
                    if (Find.TickManager.TicksGame > 200)
                    {
                        if (IsPsionic)
                        {
                            if (!firstTick) PostInitializeTick();
                            base.CompTick();
                        }
                    }
                }
            }
        }

        public bool IsPsionic
        {
            get
            {
                if (this.AbilityUser != null)
                {
                    if (this.AbilityUser.health != null)
                    {
                        if (this.AbilityUser.health.hediffSet != null)
                        {
                            if (this.AbilityUser.health.hediffSet.HasHediff(CultsDefOf.Cults_PsionicBrain)) return true;
                        }
                    }
                }
                return false;
            }
        }

        public override void PostExposeData()
        {
            //Scribe_Values.Look<bool>(ref this.firstTick, "fistTick", false);
            base.PostExposeData();
        }
    }
}
