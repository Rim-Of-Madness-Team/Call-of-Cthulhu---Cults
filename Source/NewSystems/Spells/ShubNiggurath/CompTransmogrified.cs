using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace CultOfCthulhu
{
    public class CompTransmogrified : ThingComp
    {
        public Pawn Pawn => this.parent as Pawn;
        public Hediff_Transmogrified Hediff => Pawn.health.hediffSet.GetFirstHediffOfDef(CultsDefOf.Cults_MonstrousBody, false) as Hediff_Transmogrified;

        //public BodyPartRecord CorePart
        //{
        //    get
        //    {
        //        return Pawn?.health?.hediffSet.GetNotMissingParts().FirstOrDefault(x => x.def == Pawn?.RaceProps?.body?.corePart?.def) ?? null;
        //    }
        //}

        private bool isTransmogrified = false;
        public bool IsTransmogrified
        {
            get => isTransmogrified;
            set
            {
                if (value == true && isTransmogrified == false)
                {
                    Find.LetterStack.ReceiveLetter("Cults_TransmogrifiedLetter".Translate(), "Cults_TransmogrifiedLetterDesc".Translate(this.parent.LabelShort), LetterDefOf.PositiveEvent, new RimWorld.Planet.GlobalTargetInfo(this.parent), null);
                }
                //HealthUtility.AdjustSeverity(this.parent as Pawn, CultsDefOf.Cults_MonstrousBody, 1.0f);
                isTransmogrified = value;
                MakeHediff();

            }
        }

        public void MakeHediff()
        {
            if (isTransmogrified && Hediff == null)
            {
                Hediff hediff = HediffMaker.MakeHediff(CultsDefOf.Cults_MonstrousBody, Pawn, null);
                hediff.Severity = 1.0f;
                Pawn.health.AddHediff(hediff, null, null);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref this.isTransmogrified, "isTransmogrified", false);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                MakeHediff();
            }
        }
    }
}
