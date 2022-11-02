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

        public MapComponent_SacrificeTracker(Map map) : base(map: map)
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
            Scribe_Values.Look(value: ref ticksUntilResurrection, label: "ticksUntilResurrection");
            //Scribe_Collections.Look<Corpse>(ref this.toBeResurrected, "toBeResurrected", LookMode.Reference);
            Scribe_Collections.Look(list: ref unspeakableOathPawns, label: "unspeakableOathPawns", lookMode: LookMode.Reference);

            //Defend the Brood Spell
            Scribe_Collections.Look(list: ref defendTheBroodPawns, label: "defendTheBroodPawns", lookMode: LookMode.Reference);

            //Sacrifice Variables
            Scribe_Values.Look(value: ref lastSacrificeName, label: "lastSacrificeName", defaultValue: "None");
            Scribe_Values.Look(value: ref wasDoubleTheFun, label: "wasDoubleTheFun");
            Scribe_Values.Look(value: ref ASMwasPet, label: "ASMwasPet");
            Scribe_Values.Look(value: ref ASMwasBonded, label: "ASMwasBonded");
            Scribe_Values.Look(value: ref ASMwasExcMaster, label: "ASMwasExcMaster");
            Scribe_Values.Look(value: ref HSMwasFamily, label: "HSMwasFamily");

            Scribe_Values.Look(value: ref lastLocation, label: "lastLocation", defaultValue: IntVec3.Invalid);
            Scribe_Defs.Look(value: ref lastRelation, label: "lastRelation");
            Scribe_Defs.Look(value: ref lastDoubleSideEffect, label: "lastDoubleSideEffect");
            Scribe_Defs.Look(value: ref lastSideEffect, label: "lastSideEffect");
            Scribe_Defs.Look(value: ref lastSpell, label: "lastSpell");
            Scribe_References.Look(refee: ref lastUsedAltar, label: "lastUsedAltar");
            Scribe_Values.Look(value: ref lastResult, label: "lastResult", defaultValue: CultUtility.SacrificeResult.success);
            Scribe_Values.Look(value: ref lastSacrificeType, label: "lastSacrificeType");
            Scribe_Values.Look(value: ref lastOfferingSize, label: "lastOfferingSize");
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

            mapComponent_SacrificeTracker = new MapComponent_SacrificeTracker(map: map);
            map.components.Add(item: mapComponent_SacrificeTracker);

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
            var ran = Rand.Range(min: 1, max: 40);
            var message = "SacrificeFailMessage" + ran;
            string messageObject = message.Translate(arg1: lastUsedAltar.SacrificeData.Executioner);
            s.Append(value: messageObject);
            return s.ToString();
        }

        public void GenerateSacrificeMessage()
        {
            var s = new StringBuilder();
            var letterDef = LetterDefOf.ThreatSmall;
            s.Append(value: "SacrificeIntro".Translate());
            s.Append(value: " " + lastUsedAltar.SacrificeData.Entity.Label);

            string labelToTranslate;
            string textLabel;
            //The sacrifice is human
            if (lastUsedAltar.SacrificeData.Type == CultUtility.SacrificeType.human)
            {
                s.Append(value: " " + lastUsedAltar.SacrificeData.Spell.letterLabel + ". ");

                //Was the executioner a family member?
                if (HSMwasFamily)
                {
                    if (lastUsedAltar.SacrificeData.Executioner == null)
                    {
                        Log.Error(text: "Executioner null");
                    }

                    if (lastRelation == null)
                    {
                        Log.Error(text: "Null relation");
                    }

                    if (lastSacrificeName == null)
                    {
                        Log.Error(text: "Null name");
                    }

                    string familyString = "HumanSacrificeWasFamily".Translate(
                        arg1: lastUsedAltar.SacrificeData.Executioner?.LabelShort,
                        arg2: lastUsedAltar.SacrificeData.Executioner?.gender.GetPossessive(),
                        arg3: lastRelation?.label,
                        arg4: lastSacrificeName
                    );
                    s.Append(value: familyString + ". ");
                }

                if (lastResult != CultUtility.SacrificeResult.success)
                {
                    s.Append(value: GenerateFailureString());
                }

                if ((int) lastResult <= 3 && (int) lastResult > 1)
                {
                    s.Append(value: " " + lastSideEffect.letterText);
                    if (wasDoubleTheFun)
                    {
                        s.Append(value: " " + lastDoubleSideEffect.letterText);
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
                    s.Append(value: ". " + buts.RandomElement() + ", ");
                }

                if ((int) lastResult > 2)
                {
                    s.Append(value: lastUsedAltar.SacrificeData.Executioner + " " +
                                    lastUsedAltar.SacrificeData.Spell.letterText + ".");
                }

                s.Append(value: " " + "Cults_ritualWas".Translate());

                switch (lastResult)
                {
                    case CultUtility.SacrificeResult.success:
                        s.Append(value: "Cults_ritualSuccess".Translate());
                        letterDef = CultsDefOf.Cults_StandardMessage;
                        break;
                    case CultUtility.SacrificeResult.mixedsuccess:
                        letterDef = CultsDefOf.Cults_StandardMessage;
                        s.Append(value: "Cults_ritualMixedSuccess".Translate());
                        break;
                    case CultUtility.SacrificeResult.failure:
                        s.Append(value: "Cults_ritualFailure".Translate());
                        break;
                    case CultUtility.SacrificeResult.criticalfailure:
                        s.Append(value: "Cults_ritualCompleteFailure".Translate());
                        break;
                    case CultUtility.SacrificeResult.none:
                        s.Append(value: "this should never happen");
                        break;
                }

                labelToTranslate = "SacrificeLabel" + lastResult;
            }
            else if (lastUsedAltar.SacrificeData.Type == CultUtility.SacrificeType.animal)
            {
                s.Append(value: " " + "AnimalSacrificeReason".Translate() + ".");
                if (ASMwasPet)
                {
                    s.Append(value: " " + "AnimalSacrificeWasPet".Translate() + lastUsedAltar.SacrificeData.Entity.Label +
                                    ".");
                }

                if (ASMwasBonded)
                {
                    string bondString = "AnimalSacrificeWasBonded".Translate(
                        arg1: lastSacrificeName,
                        arg2: lastUsedAltar.SacrificeData.Executioner.LabelShort
                    );
                    s.Append(value: " " + bondString + ".");
                }

                if (ASMwasExcMaster)
                {
                    string bondString = "AnimalSacrificeWasExcMaster".Translate(
                        arg1: lastSacrificeName,
                        arg2: lastUsedAltar.SacrificeData.Executioner.LabelShort
                    );
                    s.Append(value: " " + bondString + ".");
                }

                if (ASMwasBonded || ASMwasPet || ASMwasExcMaster)
                {
                    s.Append(value: " " + "GreedilyReceived".Translate() + lastUsedAltar.SacrificeData.Entity.Label + ".");
                }
                else
                {
                    s.Append(value: " " + "ReadilyReceived".Translate() + ".");
                }

                textLabel = "AnimalSacrificeLabel".Translate();
                goto LetterStack;
            }
            else
            {
                s.Append(value: " " + "OfferingReason".Translate() + ".");
                s.Append(value: " " + "ReadilyReceived".Translate() + ".");
                textLabel = "OfferingLabel".Translate();
                goto LetterStack;
            }

            textLabel = labelToTranslate.Translate();
            LetterStack:
            Find.LetterStack.ReceiveLetter(label: textLabel, text: s.ToString(), textLetterDef: letterDef, lookTargets: new TargetInfo(cell: lastLocation, map: map));
        }
    }
}