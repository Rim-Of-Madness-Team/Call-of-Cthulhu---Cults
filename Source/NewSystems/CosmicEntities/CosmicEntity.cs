// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

// ----------------------------------------------------------------------
// These are RimWorld-specific usings. Activate/Deactivate what you need:
// ----------------------------------------------------------------------
using UnityEngine;         // Always needed
//using VerseBase;         // Material/Graphics handling functions are found here
using Verse;               // RimWorld universal objects are here (like 'Building')
using Verse.AI;          // Needed when you do something with the AI
using Verse.AI.Group;
using Verse.Sound;       // Needed when you do something with Sound
using Verse.Noise;       // Needed when you do something with Noises
using RimWorld;            // RimWorld specific functions are found here (like 'Building_Battery')
using RimWorld.Planet;   // RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

namespace CultOfCthulhu
{

    public class CosmicEntity : Thing
    {
        private const float MODIFIER_HUMAN  = 0.5f;
        private const float MODIFIER_ANIMAL = 0.2f;
        private const float DIVIDER_HUMAN = 300f;
        private const float DIVIDER_FOOD = 700f;

        public List<IncidentDef> tier1Spells;
        //public IncidentDef tier2Spell;
        public List<IncidentDef> tier2Spells;
        //public IncidentDef tier3Spell;
        public List<IncidentDef> tier3Spells;
        public IncidentDef finalSpell;
        public List<ThingDef> favoredApparel = new List<ThingDef>();
        public bool discovered = false;

        private bool hostileToPlayer = false;
        private float favor = 0f;
        private float lastFavor = 0f;
        public float LastFavor => lastFavor;

        public float favorMax = 50f;
        public float favorTier0Max { get { return (favorMax * 0.05f); } }
        public float favorTier1Max { get { return (favorMax * 0.10f); } }
        public float favorTier2Max { get { return (favorMax * 0.5f); } }
        public float favorTier3Max { get { return (favorMax * 0.85f); } }


        public enum Tier : int
        {
            Zero = 0,
            One = 1,
            Two = 2,
            Three = 3,
            Final = 4
        };

        public string TierString
        {
            get
            {
                switch (PlayerTier)
                {
                    case Tier.Zero:
                        return "TierStringZero".Translate();
                    case Tier.One:
                        return "TierStringOne".Translate();
                    case Tier.Two:
                        return "TierStringTwo".Translate();
                    case Tier.Three:
                        return "TierStringThree".Translate();
                    case Tier.Final:
                        return "TierStringFinal".Translate();

                }
                return "";
            }

        }

        public Tier PlayerTier
        {
            get
            {
                if (favor <= favorTier0Max) return Tier.Zero;
                else if (favor > favorTier0Max && favor <= favorTier1Max) return Tier.One;
                else if (favor > favorTier1Max && favor <= favorTier2Max) return Tier.Two;
                else if (favor > favorTier2Max && favor < favorTier3Max) return Tier.Three;
                else if (favor >= favorTier3Max) return Tier.Final;
                else return Tier.Zero;
            }
        }

        public float currentTierMax
        {
            get
            {
                switch (PlayerTier)
                {
                    case Tier.Zero:
                        return favorTier0Max;
                    case Tier.One:
                        return favorTier1Max;
                    case Tier.Two:
                        return favorTier2Max;
                    case Tier.Three:
                        return favorTier3Max;
                    case Tier.Final:
                        return favorMax;

                }
                return favorMax;
            }
        }

        public float prevTierMax
        {
            get
            {
                switch (PlayerTier)
                {
                    case Tier.Zero:
                        return 0;
                    case Tier.One:
                        return favorTier0Max;
                    case Tier.Two:
                        return favorTier1Max;
                    case Tier.Three:
                        return favorTier2Max;
                    case Tier.Final:
                        return favorTier2Max;

                }
                return favorMax;
            }
        }

        public int Version
        {
            get
            {
                CosmicEntityDef currentDef = this.def as CosmicEntityDef;
                int i = 0;
                i = currentDef.Version;
                Cthulhu.Utility.DebugReport(this.Label + " version retrieved " + i);
                return i;
            }
        }
        
        public float PlayerFavor
        {
            get
            {
                return this.favor;
            }
        }

        public bool FavorsOutdoorWorship
        {
            get
            {
                return ((CosmicEntityDef)this.def).favorsOutdoorWorship;
            }
        }

        public CosmicEntity()
        {
        }

        public CosmicEntity(ThingDef newDef)
        {
            CosmicEntityDef currentDef = newDef as CosmicEntityDef;
            if(currentDef != null)
            {
                this.def = newDef;
                this.tier1Spells = currentDef.tier1SpellDefs;
                this.tier2Spells = currentDef.tier2SpellDefs;
                this.tier3Spells = currentDef.tier3SpellDefs;
                this.finalSpell = currentDef.finalSpellDef;
                this.favoredApparel = currentDef.favoredApparel;
            }
            return;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<IncidentDef>(ref this.tier1Spells, "tier1spells");
            Scribe_Collections.Look<IncidentDef>(ref this.tier2Spells, "tier2spells");
            Scribe_Collections.Look<IncidentDef>(ref this.tier3Spells, "tier3spells");
            Scribe_Defs.Look<IncidentDef>(ref this.finalSpell, "finalSpell");
            Scribe_Values.Look<bool>(ref this.hostileToPlayer, "hostileToPlayer", false, false);
            Scribe_Values.Look<float>(ref this.favor, "favor", 0f, false);
            Scribe_Values.Look<float>(ref this.lastFavor, "lastFavor", 0f, false);
            Scribe_Values.Look<bool>(ref this.discovered, "discovered", false, false);
            //Scribe_Deep.Look<KidnappedPawnsTracker>(ref this.kidnapped, "kidnapped", new object[]
            //{
            //    this
            //});
        }

        public override void Tick()
        {
            base.Tick();
            //this.kidnapped.KidnappedPawnsTrackerTick();
        }

        public bool IsOffering(Thing thingToCheck, Building_SacrificialAltar altarDictionary)
        {
            foreach (ThingAmount dictionary in altarDictionary.determinedOfferings)
            {
                if (thingToCheck.def == dictionary.thing.def) return true;
            }
            return false;
        }

        public void ConsumeOfferings(Thing offering)
        {
            float tierModifier = (float)(((int)PlayerTier) + 1);
            float foodModifier = DIVIDER_FOOD;
            float math = (offering.MarketValue * offering.stackCount) / (foodModifier * tierModifier);
            offering.Destroy(0);
            AffectFavor(math);
        }

        public void ReceiveOffering(Pawn offerer, Building_SacrificialAltar altar)
        {
            StringBuilder s = new StringBuilder();
            s.Append("Offering Report");
            s.AppendLine();
            s.Append("===============");

            foreach (IntVec3 arg_37_0 in altar.CellsAround)
            {
                Thing offering = null;
                Thing discoveredAltar = null;
                List<Thing> thingList = arg_37_0.GetThingList(offerer.Map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    Thing current = thingList[i];
                    if (current.def.defName.Contains("Altar"))
                    {
                        discoveredAltar = current;
                    }
                    if (IsOffering(current, altar))
                    {
                        offering = current;
                    }
                }
                if (offering != null)
                {
                    s.AppendLine();
                    s.Append(offering.stackCount + " " + offering.ToString() + ": $" + offering.MarketValue.ToString() + " each. Total: $" + (offering.MarketValue * (float)offering.stackCount).ToString());
                    ConsumeOfferings(offering);
                }
            }

            Cthulhu.Utility.DebugReport(s.ToString());
        }

        public float SacrificeBonus(Pawn sacrifice, Map map, bool favorSpell = false, bool starsAreRight = false, bool starsAreWrong = false)
        {
            StringBuilder s = new StringBuilder();
            s.Append("Sacrifice Bonus Calculation:");
            float result = 0f;
            if (sacrifice == null) return result;
            if (sacrifice.RaceProps == null) return result;
            MapComponent_SacrificeTracker tracker = map.GetComponent<MapComponent_SacrificeTracker>();
            if (tracker == null) return result;
            tracker.lastSacrificeName = sacrifice.LabelShort;

            //Divide by 0 exception
            if (sacrifice.MarketValue == 0f) return result;

            //Animal Sacrifice Bonuses
            if (tracker.lastSacrificeType == CultUtility.SacrificeType.animal)
            {
                //Default Animal Sacrifice Bonus
                result += (sacrifice.MarketValue * MODIFIER_ANIMAL); // 20% bonus for animal sacrifice

                //Pet Bonus handling code
                if (sacrifice.RaceProps.petness > 0f)
                {
                    result += sacrifice.MarketValue * 3; //Triple the sacrifice value of pets sacrificed
                    s.AppendLine();
                    s.Append("Subtotal: " + result.ToString() + " Applied Pet-like Modifier");
                    tracker.ASMwasPet = true;
                    if (sacrifice.playerSettings.master != null)
                    {
                        result += sacrifice.MarketValue * 2; //Add even more sacrifice value to pets with masters
                        s.AppendLine();
                        s.Append("Subtotal: " + result.ToString() + " Applied Bonded Modifier");
                        tracker.ASMwasBonded = true;
                    }
                    if (tracker.lastUsedAltar.SacrificeData.Executioner.relations.DirectRelationExists(PawnRelationDefOf.Bond, sacrifice))
                    {
                        result += sacrifice.MarketValue * 2; //Even more sacrifice value for sacrificing one's own pet
                        s.AppendLine();
                        s.Append("Subtotal: " + result.ToString() + " Applied Master As Executioner Modifier");
                        tracker.ASMwasExcMaster = true;
                    }
                }
            }
            //Human sacrifice bonuses
            if (tracker.lastSacrificeType == CultUtility.SacrificeType.human)
            {
                //Default human sacrifice bonus
                result += (sacrifice.MarketValue * MODIFIER_HUMAN); // 50% bonus for human sacrifice

                //Family bonus handling code
                Pawn ex = tracker.lastUsedAltar.SacrificeData.Executioner;
                if (ex.relations.FamilyByBlood.Contains(sacrifice))
                {
                    result += sacrifice.MarketValue * 3; //Three times the value for family members
                    ex.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.WitnessedDeathFamily, null);
                    s.AppendLine();

                    PawnRelationDef def = ex.GetMostImportantRelation(sacrifice);
                    s.Append("Subtotal: " + result.ToString() + " Applied Family Member, " + def.label + " as Executioner Modifier");
                    tracker.HSMwasFamily = true;
                    tracker.lastRelation = def;
                }

                //Favor Only Bonus
                if (favorSpell)
                {
                    result += (result * 0.2f); // 20% bonus
                }
            }
            //Stars are right
            if (starsAreRight)
            {
                result += (result * 0.5f); //50% bonus
            }
            //Stars are wrong
            if (starsAreWrong)
            {
                result = result * 0.5f; //Lose 50% of value
            }

            Cthulhu.Utility.DebugReport(s.ToString());
            return result;
        }
        
        public void ReceiveSacrifice(Pawn sacrifice, Map map, bool favorSpell = false, bool starsAreRight = false, bool starsAreWrong = false)
        {
            MapComponent_SacrificeTracker tracker = map.GetComponent<MapComponent_SacrificeTracker>();
            float value = (sacrifice.MarketValue + SacrificeBonus(sacrifice, map, favorSpell, starsAreRight, starsAreWrong)) / DIVIDER_HUMAN;
            bool perfect = false;
            bool sacrificialDagger = false;
            value += CultUtility.CongregationBonus(tracker.lastUsedAltar.SacrificeData.Congregation, this, out perfect, out sacrificialDagger);
            Cthulhu.Utility.DebugReport("Sacrifice Value: " + value.ToString());
            AffectFavor(value);
        }
        public void ReceiveWorship(Pawn preacher)
        {
            int preacherSoc = preacher.skills.GetSkill(SkillDefOf.Social).Level;
            List<Pawn> congregation = CultTracker.Get.PlayerCult.members; //preacher.Map.GetComponent<MapComponent_LocalCultTracker>().LocalCultMembers;
            if (preacher == null) Log.Error("Preacher is null.");
            if (congregation == null) Log.Error("Congregation is null");
            float value = (float)Math.Max(1, (preacherSoc));
            if (value != 0) value /= (20 * ((float)((int)PlayerTier + 1)));
            bool perfect = false;
            bool sacrificialDagger = false;
            value += CultUtility.CongregationBonus(congregation, this, out perfect, out sacrificialDagger);
            Cthulhu.Utility.DebugReport("Worship Value: " + value.ToString());
            AffectFavor(value);
        }

        public void ResetFavor()
        {
            this.favor = 0f;
        }

        public void AffectFavor(float favorChange)
        {
            float newFavor = (float)Math.Round(favorChange, 2);
            float value = this.favor + newFavor;
            Cthulhu.Utility.DebugReport(this.Label + "'s favor affected: " + newFavor.ToString() + " Total: " + value.ToString());
            favor = value;
        }

        public string GetInfoText()
        {
            string text = this.def.LabelCap;
            string text2 = text;
            text = string.Concat(new string[]
            {
                text2,
                "\n",
                "ColonyGoodwill".Translate(),
                ": ",
                this.PlayerFavor.ToString("###0")
            });
            if (hostileToPlayer)
            {
                text = text + "\n" + "Hostile".Translate();
            }
            else
            {
                text = text + "\n" + "Neutral".Translate();
            }
            return text;
        }

        public string DebugString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            return stringBuilder.ToString();
        }

        public void DebugDraw()
        {

        }

    }
}
