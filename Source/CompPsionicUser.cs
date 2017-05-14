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
        AbilityDef Cults_PsionicBlast;
        AbilityDef Cults_PsionicShock;

        public bool firstTick = false;

        public void PostInitializeTick()
        {
            firstTick = true;
            this.AddPawnAbility(PsionicBlast);
            this.AddPawnAbility(PsionicShock);
        }

        public override void CompTick()
        {
            if (abilityUser != null)
            {
                if (abilityUser.Spawned)
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
                if (this.abilityUser != null)
                {
                    if (this.abilityUser.health != null)
                    {
                        if (this.abilityUser.health.hediffSet != null)
                        {
                            if (this.abilityUser.health.hediffSet.HasHediff(CultsDefOfs.Cults_PsionicBrain)) return true;
                        }
                    }
                }
                return false;
            }
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look<bool>(ref this.firstTick, "fistTick", false);
            base.PostExposeData();
        }
    }
}
