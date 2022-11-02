using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace CultOfCthulhu
{
    public class CosmicEntityDef : ThingDef
    {
        public readonly string descriptionLong = string.Empty;

        public readonly string domains = string.Empty;
        public readonly List<ThingDef> favoredApparel = new List<ThingDef>();

        public readonly bool favorsOutdoorWorship = false;

        public readonly string portrait = string.Empty;
        public readonly string symbol = string.Empty;

        public readonly List<IncidentDef> tier1SpellDefs = new List<IncidentDef>();
        public readonly List<IncidentDef> tier2SpellDefs = new List<IncidentDef>();
        public readonly List<IncidentDef> tier3SpellDefs = new List<IncidentDef>();

        public readonly string titles = string.Empty;
        private readonly string version = "0";
        public List<FavoredThing> displeasingOfferings = new List<FavoredThing>();
        public List<FavoredThing> favoredWorshipperRaces = new List<FavoredThing>();
        public IncidentDef finalSpellDef;
        public List<FavoredThing> hereticWorshipperRaces = new List<FavoredThing>();
        public List<FavoredThing> pleasingOfferings = new List<FavoredThing>();

        [Unsaved] private Texture2D symbolTex;

        public Texture2D Symbol
        {
            get
            {
                if (symbolTex == null)
                {
                    symbolTex = ContentFinder<Texture2D>.Get(itemPath: symbol);
                }

                return symbolTex;
            }
        }

        public int Version => int.TryParse(s: version, result: out var x) ? x : 0;
    }
}