// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cthulhu;
using RimWorld;
using Verse;

// ----------------------------------------------------------------------
// These are RimWorld-specific usings. Activate/Deactivate what you need:
// ----------------------------------------------------------------------
// Always needed
//using VerseBase;         // Material/Graphics handling functions are found here
// RimWorld universal objects are here (like 'Building')
// Needed when you do something with the AI
// Needed when you do something with Sound
// Needed when you do something with Noises
// RimWorld specific functions are found here (like 'Building_Battery')

// RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

namespace CultOfCthulhu
{
    public class CosmicEntity : Thing
    {
        public enum Tier
        {
            Zero = 0,
            One = 1,
            Two = 2,
            Three = 3,
            Final = 4
        }

        private const float MODIFIER_HUMAN = 0.5f;
        private const float MODIFIER_ANIMAL = 0.2f;
        private const float DIVIDER_HUMAN = 300f;
        private const float DIVIDER_FOOD = 700f;
        public readonly List<ThingDef> favoredApparel = new List<ThingDef>();

        private readonly float favorMax = 50f;
        public bool discovered;
        private float favor;

        public IncidentDef finalSpell;

        private bool hostileToPlayer;
        private float lastFavor;

        public List<IncidentDef> tier1Spells;

        //public IncidentDef tier2Spell;
        public List<IncidentDef> tier2Spells;

        //public IncidentDef tier3Spell;
        public List<IncidentDef> tier3Spells;

        public CosmicEntity()
        {
        }

        public CosmicEntity(ThingDef newDef)
        {
            if (!(newDef is CosmicEntityDef currentDef))
            {
                return;
            }

            def = newDef;
            tier1Spells = currentDef.tier1SpellDefs;
            tier2Spells = currentDef.tier2SpellDefs;
            tier3Spells = currentDef.tier3SpellDefs;
            finalSpell = currentDef.finalSpellDef;
            favoredApparel = currentDef.favoredApparel;
        }

        public float LastFavor => lastFavor;
        private float favorTier0Max => favorMax * 0.05f;
        private float favorTier1Max => favorMax * 0.10f;
        private float favorTier2Max => favorMax * 0.5f;
        private float favorTier3Max => favorMax * 0.85f;

        public CosmicEntityDef Def => def as CosmicEntityDef;
        public override string Label => Def.label;
        public override string LabelCap => Def.label.CapitalizeFirst();

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
                if (favor <= favorTier0Max)
                {
                    return Tier.Zero;
                }

                if (favor > favorTier0Max && favor <= favorTier1Max)
                {
                    return Tier.One;
                }

                if (favor > favorTier1Max && favor <= favorTier2Max)
                {
                    return Tier.Two;
                }

                if (favor > favorTier2Max && favor < favorTier3Max)
                {
                    return Tier.Three;
                }

                return favor >= favorTier3Max ? Tier.Final : Tier.Zero;
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
                if (!(def is CosmicEntityDef currentDef))
                {
                    return 0;
                }

                var i = currentDef.Version;
                Utility.DebugReport(x: Label + " version retrieved " + i);
                return i;
            }
        }

        public float PlayerFavor => favor;

        public bool FavorsOutdoorWorship => ((CosmicEntityDef) def).favorsOutdoorWorship;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(list: ref tier1Spells, label: "tier1spells");
            Scribe_Collections.Look(list: ref tier2Spells, label: "tier2spells");
            Scribe_Collections.Look(list: ref tier3Spells, label: "tier3spells");
            Scribe_Defs.Look(value: ref finalSpell, label: "finalSpell");
            Scribe_Values.Look(value: ref hostileToPlayer, label: "hostileToPlayer");
            Scribe_Values.Look(value: ref favor, label: "favor");
            Scribe_Values.Look(value: ref lastFavor, label: "lastFavor");
            Scribe_Values.Look(value: ref discovered, label: "discovered");
            //Scribe_Deep.Look<KidnappedPawnsTracker>(ref this.kidnapped, "kidnapped", new object[]
            //{
            //    this
            //});
        }

        public bool IsOffering(Thing thingToCheck, Building_SacrificialAltar altarDictionary)
        {
            foreach (var dictionary in altarDictionary.determinedOfferings)
            {
                if (thingToCheck.def == dictionary.Thing.def)
                {
                    return true;
                }
            }

            return false;
        }

        private void ConsumeOfferings(Thing offering)
        {
            var tierModifier = (float) ((int) PlayerTier + 1);
            var foodModifier = DIVIDER_FOOD;
            var math = offering.MarketValue * offering.stackCount / (foodModifier * tierModifier);
            offering.Destroy();
            AffectFavor(favorChange: math);
        }

        public void ReceiveOffering(Pawn offerer, Building_SacrificialAltar altar, List<Thing> offerings)
        {
            var s = new StringBuilder();
            s.Append(value: "Offering Report");
            s.AppendLine();
            s.Append(value: "===============");

            foreach (var offering in offerings)
            {
                s.AppendLine();
                s.Append(value: offering.stackCount + " " + offering + ": $" + offering.MarketValue +
                                " each. Total: $" + (offering.MarketValue * offering.stackCount));
                ConsumeOfferings(offering: offering);
            }

            Utility.DebugReport(x: s.ToString());
        }

        private float SacrificeBonus(Pawn sacrifice, Map map, bool favorSpell = false, bool starsAreRight = false,
            bool starsAreWrong = false)
        {
            var s = new StringBuilder();
            s.Append(value: "Sacrifice Bonus Calculation:");
            var result = 0f;
            if (sacrifice == null)
            {
                return result;
            }

            if (sacrifice.RaceProps == null)
            {
                return result;
            }

            var tracker = map.GetComponent<MapComponent_SacrificeTracker>();
            if (tracker == null)
            {
                return result;
            }

            tracker.lastSacrificeName = sacrifice.LabelShort;

            //Divide by 0 exception
            if (sacrifice.MarketValue == 0f)
            {
                return result;
            }

            //Animal Sacrifice Bonuses
            if (tracker.lastSacrificeType == CultUtility.SacrificeType.animal)
            {
                //Default Animal Sacrifice Bonus
                result += sacrifice.MarketValue * MODIFIER_ANIMAL; // 20% bonus for animal sacrifice

                //Pet Bonus handling code
                if (sacrifice.RaceProps.petness > 0f)
                {
                    result += sacrifice.MarketValue * 3; //Triple the sacrifice value of pets sacrificed
                    s.AppendLine();
                    s.Append(value: "Subtotal: " + result + " Applied Pet-like Modifier");
                    tracker.ASMwasPet = true;
                    if (sacrifice.playerSettings.Master != null)
                    {
                        result += sacrifice.MarketValue * 2; //Add even more sacrifice value to pets with masters
                        s.AppendLine();
                        s.Append(value: "Subtotal: " + result + " Applied Bonded Modifier");
                        tracker.ASMwasBonded = true;
                    }

                    if (tracker.lastUsedAltar.SacrificeData.Executioner.relations.DirectRelationExists(
                        def: PawnRelationDefOf.Bond, otherPawn: sacrifice))
                    {
                        result += sacrifice.MarketValue * 2; //Even more sacrifice value for sacrificing one's own pet
                        s.AppendLine();
                        s.Append(value: "Subtotal: " + result + " Applied Master As Executioner Modifier");
                        tracker.ASMwasExcMaster = true;
                    }
                }
            }

            //Human sacrifice bonuses
            if (tracker.lastSacrificeType == CultUtility.SacrificeType.human)
            {
                //Default human sacrifice bonus
                result += sacrifice.MarketValue * MODIFIER_HUMAN; // 50% bonus for human sacrifice

                //Family bonus handling code
                var ex = tracker.lastUsedAltar.SacrificeData.Executioner;
                if (ex.relations.FamilyByBlood.Contains(value: sacrifice))
                {
                    result += sacrifice.MarketValue * 3; //Three times the value for family members
                    ex.needs.mood.thoughts.memories.TryGainMemory(def: ThoughtDefOf.WitnessedDeathFamily);
                    s.AppendLine();

                    var importantRelation = ex.GetMostImportantRelation(other: sacrifice);
                    s.Append(value: "Subtotal: " + result + " Applied Family Member, " + importantRelation.label +
                                    " as Executioner Modifier");
                    tracker.HSMwasFamily = true;
                    tracker.lastRelation = importantRelation;
                }

                //Favor Only Bonus
                if (favorSpell)
                {
                    result += result * 0.2f; // 20% bonus
                }
            }

            //Stars are right
            if (starsAreRight)
            {
                result += result * 0.5f; //50% bonus
            }

            //Stars are wrong
            if (starsAreWrong)
            {
                result *= 0.5f; //Lose 50% of value
            }

            Utility.DebugReport(x: s.ToString());
            return result;
        }

        public void ReceiveSacrifice(Pawn sacrifice, Map map, bool favorSpell = false, bool starsAreRight = false,
            bool starsAreWrong = false)
        {
            var tracker = map.GetComponent<MapComponent_SacrificeTracker>();
            var value =
                (sacrifice.MarketValue + SacrificeBonus(sacrifice: sacrifice, map: map, favorSpell: favorSpell, starsAreRight: starsAreRight, starsAreWrong: starsAreWrong)) /
                DIVIDER_HUMAN;
            value += CultUtility.CongregationBonus(congregationIn: tracker.lastUsedAltar.SacrificeData.Congregation, entity: this, perfect: out _,
                sacrificialDagger: out _);
            Utility.DebugReport(x: "Sacrifice Value: " + value);
            AffectFavor(favorChange: value);
        }

        public void ReceiveWorship(Pawn preacher)
        {
            var preacherSoc = preacher.skills.GetSkill(skillDef: SkillDefOf.Social).Level;
            var
                congregation =
                    CultTracker.Get.PlayerCult
                        .members; //preacher.Map.GetComponent<MapComponent_LocalCultTracker>().LocalCultMembers;

            if (congregation == null)
            {
                Log.Error(text: "Congregation is null");
            }

            var value = (float) Math.Max(val1: 1, val2: preacherSoc);
            if (value != 0)
            {
                value /= 20 * (float) ((int) PlayerTier + 1);
            }

            value += CultUtility.CongregationBonus(congregationIn: congregation, entity: this, perfect: out _, sacrificialDagger: out _);
            Utility.DebugReport(x: "Worship Value: " + value);
            AffectFavor(favorChange: value);
        }

        public void ResetFavor()
        {
            favor = 0f;
        }

        public void AffectFavor(float favorChange)
        {
            var newFavor = (float) Math.Round(value: favorChange, digits: 2);
            var value = favor + newFavor;
            Utility.DebugReport(x: Label + "'s favor affected: " + newFavor + " Total: " +
                                   value);
            favor = value;
        }

        public string Info()
        {
            var s = new StringBuilder();
            s.AppendLine(value: "Box_Titles".Translate() + ": " + Def.titles);
            s.AppendLine();
            s.AppendLine(value: "Box_Domains".Translate() + ": " + Def.domains);
            s.AppendLine();
            s.AppendLine(value: "Box_Description".Translate() + ": ");
            s.AppendLine(value: Def.descriptionLong);
            return s.ToString();
        }

        public string GetInfoText()
        {
            string text = def.LabelCap;
            var text2 = text;
            text = string.Concat(text2, "\n", "ColonyGoodwill".Translate(), ": ", PlayerFavor.ToString(format: "###0"));
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
            var stringBuilder = new StringBuilder();
            return stringBuilder.ToString();
        }

        public void DebugDraw()
        {
        }
    }
}