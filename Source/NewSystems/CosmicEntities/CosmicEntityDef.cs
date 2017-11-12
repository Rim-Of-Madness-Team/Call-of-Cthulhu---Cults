using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;
using RimWorld;
using System.Xml;

namespace CultOfCthulhu
{
    public class FavoredThing
    {
        public string thingDef;
        public float favor;
        public FavoredThing()
        {
        }
        
        public FavoredThing(string thingDef, float favor)
        {
            this.thingDef = thingDef;
            this.favor = favor;
        }

        public string Summary
        {
            get
            {
                return this.favor.ToStringPercent() + " favor " + ((this.thingDef == null) ? "null" : this.thingDef);
            }
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            //DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "thingDef", xmlRoot.Name);
            this.thingDef = (string)ParseHelper.FromString(xmlRoot.Name, typeof(string));
            this.favor = (float)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(float));
        }
        
        public override string ToString()
        {
            return string.Concat(new object[]
            {
            "(",
            (this.thingDef == null) ? "null" : this.thingDef,
            " (",
            this.favor.ToStringPercent(),
            "% Favor)",
            ")"
            });
        }
        
    }


    public class CosmicEntityDef : ThingDef
    {
        private string symbol;
        private string version = "0";

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
