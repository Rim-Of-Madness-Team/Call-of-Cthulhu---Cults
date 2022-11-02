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
            Pawn.health.hediffSet.GetFirstHediffOfDef(def: CultsDefOf.Cults_MonstrousBody) as Hediff_Transmogrified;

        public bool IsTransmogrified
        {
            get => isTransmogrified;
            set
            {
                if (value && isTransmogrified == false)
                {
                    Find.LetterStack.ReceiveLetter(label: "Cults_TransmogrifiedLetter".Translate(),
                        text: "Cults_TransmogrifiedLetterDesc".Translate(arg1: parent.LabelShort), textLetterDef: LetterDefOf.PositiveEvent,
                        lookTargets: new GlobalTargetInfo(thing: parent));
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

            var hediff = HediffMaker.MakeHediff(def: CultsDefOf.Cults_MonstrousBody, pawn: Pawn);
            hediff.Severity = 1.0f;
            Pawn.health.AddHediff(hediff: hediff);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(value: ref isTransmogrified, label: "isTransmogrified");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                MakeHediff();
            }
        }
    }
}