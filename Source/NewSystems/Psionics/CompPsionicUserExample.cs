using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using AbilityUser;

namespace CultOfCthulhu
{
    public class CompPsionicUserExample : AbilityUser.CompAbilityUser
    {
        //A simple check boolean to make sure we don't give abilities twice.
        //Starts false because we haven't given abilities yet.
        private bool gaveAbilities = false;

        private bool firstTick = false;
        
        /// <summary>
        /// To be psionic, the character must have a psionic brain.
        /// </summary>
        private bool IsPsionic 
        {
            get
            {
                if (Pawn?.health?.hediffSet == null) return false;
                if (Pawn.health.hediffSet.HasHediff(CultsDefOf.Cults_PsionicBrain)) return true;
                return false;
            }
        }
        
        /// <summary>
        /// Gives this component class to the character if they are psionic.
        /// </summary>
        public override bool TryTransformPawn() => IsPsionic;

        /// <summary>
        /// After getting the component class, checks 200 ticks
        /// after the game starts.
        /// If the character is psionic, give them the abilities in
        /// the function PostInitalizeTick()
        /// </summary>
        public override void CompTick()
        {
            if (Pawn?.Spawned != true) return;
            if (Find.TickManager.TicksGame > 200)
            {
                if (IsPsionic)
                {
                    if (!firstTick)
                    {
                        PostInitializeTick();
                    }
                    base.CompTick();
                }
            }
        }
        
        /// <summary>
        /// Adds the ability "Psionic Blast" to the character.
        /// Sets gaveAbilities to true, because we gave the abilties.
        /// </summary>
        private void PostInitializeTick()
        {
            if (Pawn?.Spawned != true) return;
            if (Pawn?.story == null) return;
            firstTick = true;
            if (!gaveAbilities)
            {
                gaveAbilities = true;
                this.Initialize();
                this.AddPawnAbility(CultsDefOf.Cults_PsionicBlast);   
            }
        }

        //Use this area to store any extra data you want to load
        //with your component.
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref this.gaveAbilities, "gaveAbilities", false);
        }
    }
}
