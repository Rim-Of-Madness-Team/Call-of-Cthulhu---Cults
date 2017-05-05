using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace CultOfCthulhu
{
    internal class ModdablePawn : Pawn
    {
        RaceProperties race;
        Graphic graphicInt;

        public void Init()
        {
            race = new RaceProperties();
            graphicInt = new Graphic();
            

            graphicInt = def.graphicData.GraphicColoredFor(this);
        }

        public new Graphic Graphic
        {
            set
            {
                this.graphicInt = value;
            }

            get
            {
                return this.graphicInt;
            }
        }

        public new RaceProperties RaceProps
        {
            set
            {
                this.race = value;
            }
            get
            {
                return this.race;
            }
        }

        public void ModdablePawn()
        {

        }
    }
}
