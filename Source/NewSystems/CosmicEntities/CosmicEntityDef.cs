using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;
using RimWorld;

namespace CultOfCthulhu
{
    public class CosmicEntityDef : ThingDef
    {
        private string symbol;
        private string version = "0";

        public List<IncidentDef> tier1SpellDefs = new List<IncidentDef>();
        public List<IncidentDef> tier2SpellDefs = new List<IncidentDef>();
        public List<IncidentDef> tier3SpellDefs = new List<IncidentDef>();
        public IncidentDef finalSpellDef;
        public List<ThingDef> favoredApparel = new List<ThingDef>();
        public bool favorsOutdoorWorship = false;

        [Unsaved]
        private Texture2D symbolTex;

        public Texture2D Symbol
        {
            get
            {
                if (this.symbolTex == null)
                {
                    this.symbolTex = ContentFinder<Texture2D>.Get(this.symbol, true);
                }
                return this.symbolTex;
            }
        }

        public int Version
        {
            get
            {
                int x = 0;
                if(Int32.TryParse(version, out x))
                {
                    return x;
                }
                return 0;
            }
        }
    }
}
