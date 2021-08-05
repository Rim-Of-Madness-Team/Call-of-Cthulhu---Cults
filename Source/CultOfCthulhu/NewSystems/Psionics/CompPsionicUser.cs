using AbilityUser;
using Verse;

namespace CultOfCthulhu
{
    public class CompPsionicUser : CompAbilityUser
    {
        public bool firstTick;

        public bool IsPsionic
        {
            get
            {
                if (Pawn?.health?.hediffSet == null)
                {
                    return false;
                }

                if (Pawn.health.hediffSet.HasHediff(CultsDefOf.Cults_PsionicBrain))
                {
                    return true;
                }

                return false;
            }
        }

        public override bool TryTransformPawn()
        {
            return IsPsionic;
        }

        public void PostInitializeTick()
        {
            if (Pawn == null)
            {
                return;
            }

            if (!Pawn.Spawned)
            {
                return;
            }

            if (Pawn.story == null)
            {
                return;
            }

            firstTick = true;
            Initialize();
            AddPawnAbility(CultsDefOf.Cults_PsionicBlast);
            AddPawnAbility(CultsDefOf.Cults_PsionicShock);
            AddPawnAbility(CultsDefOf.Cults_PsionicBurn);
        }

        public override void CompTick()
        {
            if (Pawn == null)
            {
                return;
            }

            if (!Pawn.Spawned)
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
    }
}