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
    public partial class MapComponent_SacrificeTracker : MapComponent
    {
        
        public static int resurrectionTicks = 10000;

        /// Unspeakable Oath Variables
        public List<Pawn> toBeResurrected = new List<Pawn>();
        public int ticksUntilResurrection = -999;

        /// Defend the Brood Variables
        public List<Pawn> defendTheBroodPawns = new List<Pawn>();

        /// Sacrifice Tracker Variables
        //public List<Pawn> lastSacrificeCongregation = new List<Pawn>();
        public List<Pawn> unspeakableOathPawns = new List<Pawn>();


        public string lastSacrificeName = "";
        public bool wasDoubleTheFun = false;
        public bool ASMwasPet = false;
        public bool ASMwasBonded = false;
        public bool ASMwasExcMaster = false;
        public bool HSMwasFamily = false;
        public IntVec3 lastLocation = IntVec3.Invalid;
        public PawnRelationDef lastRelation = null;
        public IncidentDef lastDoubleSideEffect = null;
        public IncidentDef lastSideEffect = null;
        public IncidentDef lastSpell = null;
        public Building_SacrificialAltar lastUsedAltar = null; //Default
        public CultUtility.SacrificeResult lastResult = CultUtility.SacrificeResult.success; // Default is a success, so that in-case of a bug, it's a positive bug.
        public CultUtility.SacrificeType lastSacrificeType = CultUtility.SacrificeType.none;
        public CultUtility.OfferingSize lastOfferingSize = CultUtility.OfferingSize.none;

        #region Setup

        //In-case our component injector doesn't pick up the map component,
        //this method causes a new map component to be generated from a static method.
        public static MapComponent_SacrificeTracker Get(Map map)
        {
            MapComponent_SacrificeTracker mapComponent_SacrificeTracker = map.components.OfType<MapComponent_SacrificeTracker>().FirstOrDefault<MapComponent_SacrificeTracker>();
            if (mapComponent_SacrificeTracker == null)
            {
                mapComponent_SacrificeTracker = new MapComponent_SacrificeTracker(map);
                map.components.Add(mapComponent_SacrificeTracker);
            }
            return mapComponent_SacrificeTracker;
        }

        public MapComponent_SacrificeTracker(Map map) : base(map)
        {
            this.map = map;
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

#endregion Setup

        #region Tick

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            ResolveHasturOathtakers();
            ResolveHasturResurrections();
        }

        #endregion Tick

        #region Reports
        public string GenerateFailureString()
        {
            StringBuilder s = new StringBuilder();
            int ran = Rand.Range(1, 40);
            string message = "SacrificeFailMessage" + ran.ToString();
            string messageObject = message.Translate(lastUsedAltar.SacrificeData.Executioner);
            s.Append(messageObject);
            return s.ToString();
        }

        public void GenerateSacrificeMessage()
        {
            StringBuilder s = new StringBuilder();
            string labelToTranslate = "Error";
            string textLabel = "Error";
            LetterDef letterDef = LetterDefOf.ThreatSmall;
            s.Append("SacrificeIntro".Translate());
            s.Append(" " + lastUsedAltar.SacrificeData.Entity.Label);

            //The sacrifice is human
            if (lastUsedAltar.SacrificeData.Type == CultUtility.SacrificeType.human)
            {
             
                s.Append(" " + lastUsedAltar.SacrificeData.Spell.letterLabel + ". ");

                //Was the executioner a family member?
                if (HSMwasFamily)
                {
                    if (lastUsedAltar.SacrificeData.Executioner == null) Log.Error("Executioner null");
                    if (lastRelation == null) Log.Error("Null relation");
                    if (lastSacrificeName == null) Log.Error("Null name");
                    string familyString = "HumanSacrificeWasFamily".Translate(
                            lastUsedAltar.SacrificeData.Executioner.LabelShort,
                            lastUsedAltar.SacrificeData.Executioner.gender.GetPossessive(),
                            lastRelation.label,
                            lastSacrificeName
                    );
                    s.Append(familyString + ". ");
                }

                if (lastResult != CultUtility.SacrificeResult.success)
                    s.Append(GenerateFailureString());
                if ((int)lastResult <= 3 && (int)lastResult > 1)
                {
                    s.Append(" " + lastSideEffect.letterText);
                    if (wasDoubleTheFun)
                    {
                        s.Append(" " + lastDoubleSideEffect.letterText);
                    }
                }
                if (lastResult == CultUtility.SacrificeResult.mixedsuccess)
                {
                    List<string> buts = new List<string> {
                    "Cults_butsOne".Translate(),
                    "Cults_butsTwo".Translate(),
                    "Cults_butsThree".Translate(),
                    "Cults_butsFour".Translate()
                };
                    s.Append(". " + buts.RandomElement<string>() + ", ");
                }
                if ((int)lastResult > 2)
                    s.Append(lastUsedAltar.SacrificeData.Executioner.ToString() + " " + lastUsedAltar.SacrificeData.Spell.letterText + ".");
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
                labelToTranslate = "SacrificeLabel" + lastResult.ToString();
            }
            else if (lastUsedAltar.SacrificeData.Type == CultUtility.SacrificeType.animal)
            {
                s.Append(" " + "AnimalSacrificeReason".Translate() + ".");
                if (ASMwasPet) s.Append(" " + "AnimalSacrificeWasPet".Translate() + lastUsedAltar.SacrificeData.Entity.Label + ".");
                if (ASMwasBonded)
                {
                    string bondString = "AnimalSacrificeWasBonded".Translate(
                            lastSacrificeName,
                            lastUsedAltar.SacrificeData.Executioner.LabelShort
                    );
                    s.Append(" " + bondString +".");
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
        #endregion Reports
        
        public override void ExposeData()
        {
            //Unspeakable Oath Spell
            Scribe_Values.Look<int>(ref this.ticksUntilResurrection, "ticksUntilResurrection", -999);
            //Scribe_Collections.Look<Corpse>(ref this.toBeResurrected, "toBeResurrected", LookMode.Reference);
            Scribe_Collections.Look<Pawn>(ref this.unspeakableOathPawns, "unspeakableOathPawns", LookMode.Reference);
            
            //Defend the Brood Spell
            Scribe_Collections.Look<Pawn>(ref this.defendTheBroodPawns, "defendTheBroodPawns", LookMode.Reference, new object[0]);

            //Sacrifice Variables
            Scribe_Values.Look<string>(ref this.lastSacrificeName, "lastSacrificeName", "None", false);
            Scribe_Values.Look<bool>(ref this.wasDoubleTheFun, "wasDoubleTheFun", false, false);
            Scribe_Values.Look<bool>(ref this.ASMwasPet, "ASMwasPet", false, false);
            Scribe_Values.Look<bool>(ref this.ASMwasBonded, "ASMwasBonded", false, false);
            Scribe_Values.Look<bool>(ref this.ASMwasExcMaster, "ASMwasExcMaster", false, false);
            Scribe_Values.Look<bool>(ref this.HSMwasFamily, "HSMwasFamily", false, false);

            Scribe_Values.Look<IntVec3>(ref this.lastLocation, "lastLocation", IntVec3.Invalid, false);
            Scribe_Defs.Look<PawnRelationDef>(ref this.lastRelation, "lastRelation");
            Scribe_Defs.Look<IncidentDef>(ref this.lastDoubleSideEffect, "lastDoubleSideEffect");
            Scribe_Defs.Look<IncidentDef>(ref this.lastSideEffect, "lastSideEffect");
            Scribe_Defs.Look<IncidentDef>(ref this.lastSpell, "lastSpell");
            Scribe_References.Look<Building_SacrificialAltar>(ref this.lastUsedAltar, "lastUsedAltar", false);
            Scribe_Values.Look<CultUtility.SacrificeResult>(ref this.lastResult, "lastResult", CultUtility.SacrificeResult.success, false);
            Scribe_Values.Look<CultUtility.SacrificeType>(ref this.lastSacrificeType, "lastSacrificeType", CultUtility.SacrificeType.none, false);
            Scribe_Values.Look<CultUtility.OfferingSize>(ref this.lastOfferingSize, "lastOfferingSize", CultUtility.OfferingSize.none, false);
            base.ExposeData();

        }
    }
}