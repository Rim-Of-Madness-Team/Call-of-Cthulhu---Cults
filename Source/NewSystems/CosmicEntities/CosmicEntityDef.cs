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
#pragma warning disable IDE0044 // Add readonly modifier
        private readonly string symbol;
#pragma warning restore IDE0044 // Add readonly modifier
        private string version = "0";
        private string portrait = "";
        private string titles = "";
        private string domains = "";
        private string descriptionLong = "";

        public List<IncidentDef> tier1SpellDefs = new List<IncidentDef>();
        public List<IncidentDef> tier2SpellDefs = new List<IncidentDef>();
        public List<IncidentDef> tier3SpellDefs = new List<IncidentDef>();
        public IncidentDef finalSpellDef;
        public List<ThingDef> favoredApparel = new List<ThingDef>();
        public List<FavoredThing> pleasingOfferings = new List<FavoredThing>();
        public List<FavoredThing> displeasingOfferings = new List<FavoredThing>();
        public List<FavoredThing> favoredWorshipperRaces = new List<FavoredThing>();
        public List<FavoredThing> hereticWorshipperRaces = new List<FavoredThing>();

        public bool favorsOutdoorWorship = false;

        [Unsaved]
        private Texture2D symbolTex;

        public string Portrait => portrait;
        public string Domains => domains;
        public string DescriptionLong => descriptionLong;
        public string Titles => titles;

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
