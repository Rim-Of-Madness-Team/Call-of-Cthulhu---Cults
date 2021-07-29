using RimWorld;
using RimWorld.Planet;
using Verse;

namespace CultOfCthulhu
{
    public class CompTransmogrified : ThingComp
    {
        //public BodyPartRecord CorePart
        //{
        //    get
        //    {
        //        return Pawn?.health?.hediffSet.GetNotMissingParts().FirstOrDefault(x => x.def == Pawn?.RaceProps?.body?.corePart?.def) ?? null;
        //    }
        //}

        private bool isTransmogrified;
        public Pawn Pawn => parent as Pawn;

        public Hediff_Transmogrified Hediff =>
            Pawn.health.hediffSet.GetFirstHediffOfDef(CultsDefOf.Cults_MonstrousBody) as Hediff_Transmogrified;

        public bool IsTransmogrified
        {
            get => isTransmogrified;
            set
            {
                if (value && isTransmogrified == false)
                {
                    Find.LetterStack.ReceiveLetter("Cults_TransmogrifiedLetter".Translate(),
                        "Cults_TransmogrifiedLetterDesc".Translate(parent.LabelShort), LetterDefOf.PositiveEvent,
                        new GlobalTargetInfo(parent));
                }

                //HealthUtility.AdjustSeverity(this.parent as Pawn, CultsDefOf.Cults_MonstrousBody, 1.0f);
                isTransmogrified = value;
                MakeHediff();
            }
        }

        public void MakeHediff()
        {
            if (!isTransmogrified || Hediff != null)
            {
                return;
            }

            var hediff = HediffMaker.MakeHediff(CultsDefOf.Cults_MonstrousBody, Pawn);
            hediff.Severity = 1.0f;
            Pawn.health.AddHediff(hediff);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref isTransmogrified, "isTransmogrified");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                MakeHediff();
            }
        }
    }
}