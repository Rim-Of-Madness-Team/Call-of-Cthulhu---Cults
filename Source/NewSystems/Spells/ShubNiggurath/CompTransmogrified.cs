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
        private bool isTransmogrified = false;
        public bool IsTransmogrified
        {
            get => isTransmogrified;
            set
            {
                if (value == true && isTransmogrified == false)
                {
                    Find.LetterStack.ReceiveLetter("Cults_TransmogrifiedLetter".Translate(), "Cults_TransmogrifiedLetterDesc".Translate(this.parent.LabelShort), LetterDefOf.Good, new RimWorld.Planet.GlobalTargetInfo(this.parent), null);
                }
                isTransmogrified = value;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref this.isTransmogrified, "isTransmogrified", false);
        }
    }
}
