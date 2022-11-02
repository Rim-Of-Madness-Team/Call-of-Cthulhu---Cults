using AbilityUser;
using Verse;

namespace CultOfCthulhu
{
    public class CompPsionicUserExample : CompAbilityUser
    {
        private bool firstTick;

        //A simple check boolean to make sure we don't give abilities twice.
        //Starts false because we haven't given abilities yet.
        private bool gaveAbilities;

        /// <summary>
        ///     To be psionic, the character must have a psionic brain.
        /// </summary>
        private bool IsPsionic
        {
            get
            {
                if (Pawn?.health?.hediffSet == null)
                {
                    return false;
                }

                return Pawn.health.hediffSet.HasHediff(def: CultsDefOf.Cults_PsionicBrain);
            }
        }

        /// <summary>
        ///     Gives this component class to the character if they are psionic.
        /// </summary>
        public override bool TryTransformPawn()
        {
            return IsPsionic;
        }

        /// <summary>
        ///     After getting the component class, checks 200 ticks
        ///     after the game starts.
        ///     If the character is psionic, give them the abilities in
        ///     the function PostInitalizeTick()
        /// </summary>
        public override void CompTick()
        {
            if (Pawn?.Spawned != true)
            {
                return;
            }

            if (Find.TickManager.TicksGame <= 200)
            {
                return;
            }

            if (!IsPsionic)
            {
                return;
            }

            if (!firstTick)
            {
                PostInitializeTick();
            }

            base.CompTick();
        }

        /// <summary>
        ///     Adds the ability "Psionic Blast" to the character.
        ///     Sets gaveAbilities to true, because we gave the abilties.
        /// </summary>
        private void PostInitializeTick()
        {
            if (Pawn?.Spawned != true)
            {
                return;
            }

            if (Pawn?.story == null)
            {
                return;
            }

            firstTick = true;
            if (gaveAbilities)
            {
                return;
            }

            gaveAbilities = true;
            Initialize();
            AddPawnAbility(abilityDef: CultsDefOf.Cults_PsionicBlast);
        }

        //Use this area to store any extra data you want to load
        //with your component.
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(value: ref gaveAbilities, label: "gaveAbilities");
        }
    }
}