// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public partial class MapComponent_SacrificeTracker : MapComponent
    {
        public static int resurrectionTicks = 10000;
        public bool ASMwasBonded;
        public bool ASMwasExcMaster;
        public bool ASMwasPet;

        /// Defend the Brood Variables
        public List<Pawn> defendTheBroodPawns = new List<Pawn>();

        public bool HSMwasFamily;
        public IncidentDef lastDoubleSideEffect;
        public IntVec3 lastLocation = IntVec3.Invalid;
        public CultUtility.OfferingSize lastOfferingSize = CultUtility.OfferingSize.none;
        public PawnRelationDef lastRelation;

        public CultUtility.SacrificeResult
            lastResult =
                CultUtility.SacrificeResult
                    .success; // Default is a success, so that in-case of a bug, it's a positive bug.


        public string lastSacrificeName = "";
        public CultUtility.SacrificeType lastSacrificeType = CultUtility.SacrificeType.none;
        public IncidentDef lastSideEffect;
        public IncidentDef lastSpell;
        public Building_SacrificialAltar lastUsedAltar; //Default
        public int ticksUntilResurrection = -999;

        /// Unspeakable Oath Variables
        public List<Pawn> toBeResurrected = new List<Pawn>();

        /// Sacrifice Tracker Variables
        //public List<Pawn> lastSacrificeCongregation = new List<Pawn>();
        public List<Pawn> unspeakableOathPawns = new List<Pawn>();

        public bool wasDoubleTheFun;

        public MapComponent_SacrificeTracker(Map map) : base(map)
        {
            this.map = map;
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            ResolveHasturOathtakers();
            ResolveHasturResurrections();
        }

        public override void ExposeData()
        {
            //Unspeakable Oath Spell
            Scribe_Values.Look(ref ticksUntilResurrection, "ticksUntilResurrection");
            //Scribe_Collections.Look<Corpse>(ref this.toBeResurrected, "toBeResurrected", LookMode.Reference);
            Scribe_Collections.Look(ref unspeakableOathPawns, "unspeakableOathPawns", LookMode.Reference);

            //Defend the Brood Spell
            Scribe_Collections.Look(ref defendTheBroodPawns, "defendTheBroodPawns", LookMode.Reference);

            //Sacrifice Variables
            Scribe_Values.Look(ref lastSacrificeName, "lastSacrificeName", "None");
            Scribe_Values.Look(ref wasDoubleTheFun, "wasDoubleTheFun");
            Scribe_Values.Look(ref ASMwasPet, "ASMwasPet");
            Scribe_Values.Look(ref ASMwasBonded, "ASMwasBonded");
            Scribe_Values.Look(ref ASMwasExcMaster, "ASMwasExcMaster");
            Scribe_Values.Look(ref HSMwasFamily, "HSMwasFamily");

            Scribe_Values.Look(ref lastLocation, "lastLocation", IntVec3.Invalid);
            Scribe_Defs.Look(ref lastRelation, "lastRelation");
            Scribe_Defs.Look(ref lastDoubleSideEffect, "lastDoubleSideEffect");
            Scribe_Defs.Look(ref lastSideEffect, "lastSideEffect");
            Scribe_Defs.Look(ref lastSpell, "lastSpell");
            Scribe_References.Look(ref lastUsedAltar, "lastUsedAltar");
            Scribe_Values.Look(ref lastResult, "lastResult", CultUtility.SacrificeResult.success);
            Scribe_Values.Look(ref lastSacrificeType, "lastSacrificeType");
            Scribe_Values.Look(ref lastOfferingSize, "lastOfferingSize");
            base.ExposeData();
        }

        //In-case our component injector doesn't pick up the map component,
        //this method causes a new map component to be generated from a static method.
        public static MapComponent_SacrificeTracker Get(Map map)
        {
            var mapComponent_SacrificeTracker = map.components.OfType<MapComponent_SacrificeTracker>().FirstOrDefault();
            if (mapComponent_SacrificeTracker != null)
            {
                return mapComponent_SacrificeTracker;
            }

            mapComponent_SacrificeTracker = new MapComponent_SacrificeTracker(map);
            map.components.Add(mapComponent_SacrificeTracker);

            return mapComponent_SacrificeTracker;
        }


        public void ClearSacrificeVariables()
        {
            lastUsedAltar.tempSacrifice = null;
            lastResult = CultUtility.SacrificeResult.none; // Default
            lastSacrificeType = CultUtility.SacrificeType.none;
            lastUsedAltar = null; //Default
            lastSideEffect = null;
            lastLocation = IntVec3.Invalid;
            wasDoubleTheFun = false;
            lastDoubleSideEffect = null;
            //lastSacrificeCongregation = null;
            lastRelation = null;
            lastSacrificeName = "";
            ASMwasPet = false;
            ASMwasBonded = false;
            ASMwasExcMaster = false;
            HSMwasFamily = false;
        }

        public string GenerateFailureString()
        {
            var s = new StringBuilder();
            var ran = Rand.Range(1, 40);
            var message = "SacrificeFailMessage" + ran;
            string messageObject = message.Translate(lastUsedAltar.SacrificeData.Executioner);
            s.Append(messageObject);
            return s.ToString();
        }

        public void GenerateSacrificeMessage()
        {
            var s = new StringBuilder();
            var letterDef = LetterDefOf.ThreatSmall;
            s.Append("SacrificeIntro".Translate());
            s.Append(" " + lastUsedAltar.SacrificeData.Entity.Label);

            string labelToTranslate;
            string textLabel;
            //The sacrifice is human
            if (lastUsedAltar.SacrificeData.Type == CultUtility.SacrificeType.human)
            {
                s.Append(" " + lastUsedAltar.SacrificeData.Spell.letterLabel + ". ");

                //Was the executioner a family member?
                if (HSMwasFamily)
                {
                    if (lastUsedAltar.SacrificeData.Executioner == null)
                    {
                        Log.Error("Executioner null");
                    }

                    if (lastRelation == null)
                    {
                        Log.Error("Null relation");
                    }

                    if (lastSacrificeName == null)
                    {
                        Log.Error("Null name");
                    }

                    string familyString = "HumanSacrificeWasFamily".Translate(
                        lastUsedAltar.SacrificeData.Executioner?.LabelShort,
                        lastUsedAltar.SacrificeData.Executioner?.gender.GetPossessive(),
                        lastRelation?.label,
                        lastSacrificeName
                    );
                    s.Append(familyString + ". ");
                }

                if (lastResult != CultUtility.SacrificeResult.success)
                {
                    s.Append(GenerateFailureString());
                }

                if ((int) lastResult <= 3 && (int) lastResult > 1)
                {
                    s.Append(" " + lastSideEffect.letterText);
                    if (wasDoubleTheFun)
                    {
                        s.Append(" " + lastDoubleSideEffect.letterText);
                    }
                }

                if (lastResult == CultUtility.SacrificeResult.mixedsuccess)
                {
                    var buts = new List<string>
                    {
                        "Cults_butsOne".Translate(),
                        "Cults_butsTwo".Translate(),
                        "Cults_butsThree".Translate(),
                        "Cults_butsFour".Translate()
                    };
                    s.Append(". " + buts.RandomElement() + ", ");
                }

                if ((int) lastResult > 2)
                {
                    s.Append(lastUsedAltar.SacrificeData.Executioner + " " +
                             lastUsedAltar.SacrificeData.Spell.letterText + ".");
                }

                s.Append(" " + "Cults_ritualWas".Translate());

                switch (lastResult)
                {
                    case CultUtility.SacrificeResult.success:
                        s.Append("Cults_ritualSuccess".Translate());
                        letterDef = CultsDefOf.Cults_StandardMessage;
                        break;
                    case CultUtility.SacrificeResult.mixedsuccess:
                        letterDef = CultsDefOf.Cults_StandardMessage;
                        s.Append("Cults_ritualMixedSuccess".Translate());
                        break;
                    case CultUtility.SacrificeResult.failure:
                        s.Append("Cults_ritualFailure".Translate());
                        break;
                    case CultUtility.SacrificeResult.criticalfailure:
                        s.Append("Cults_ritualCompleteFailure".Translate());
                        break;
                    case CultUtility.SacrificeResult.none:
                        s.Append("this should never happen");
                        break;
                }

                labelToTranslate = "SacrificeLabel" + lastResult;
            }
            else if (lastUsedAltar.SacrificeData.Type == CultUtility.SacrificeType.animal)
            {
                s.Append(" " + "AnimalSacrificeReason".Translate() + ".");
                if (ASMwasPet)
                {
                    s.Append(" " + "AnimalSacrificeWasPet".Translate() + lastUsedAltar.SacrificeData.Entity.Label +
                             ".");
                }

                if (ASMwasBonded)
                {
                    string bondString = "AnimalSacrificeWasBonded".Translate(
                        lastSacrificeName,
                        lastUsedAltar.SacrificeData.Executioner.LabelShort
                    );
                    s.Append(" " + bondString + ".");
                }

                if (ASMwasExcMaster)
                {
                    string bondString = "AnimalSacrificeWasExcMaster".Translate(
                        lastSacrificeName,
                        lastUsedAltar.SacrificeData.Executioner.LabelShort
                    );
                    s.Append(" " + bondString + ".");
                }

                if (ASMwasBonded || ASMwasPet || ASMwasExcMaster)
                {
                    s.Append(" " + "GreedilyReceived".Translate() + lastUsedAltar.SacrificeData.Entity.Label + ".");
                }
                else
                {
                    s.Append(" " + "ReadilyReceived".Translate() + ".");
                }

                textLabel = "AnimalSacrificeLabel".Translate();
                goto LetterStack;
            }
            else
            {
                s.Append(" " + "OfferingReason".Translate() + ".");
                s.Append(" " + "ReadilyReceived".Translate() + ".");
                textLabel = "OfferingLabel".Translate();
                goto LetterStack;
            }

            textLabel = labelToTranslate.Translate();
            LetterStack:
            Find.LetterStack.ReceiveLetter(textLabel, s.ToString(), letterDef, new TargetInfo(lastLocation, map));
        }
    }
}