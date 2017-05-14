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
    class MapComponent_SacrificeTracker : MapComponent
    {
        
        public static int resurrectionTicks = 10000;

        /// Unspeakable Oath Variables
        public Stack<Corpse> toBeResurrected = new Stack<Corpse>();
        public int ticksUntilResurrection = -999;

        /// Defend the Brood Variables
        public List<Pawn> defendTheBroodPawns = new List<Pawn>();

        /// Sacrifice Tracker Variables
        public List<Pawn> lastSacrificeCongregation = new List<Pawn>();
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
        public CultUtility.SacrificeResult lastResult = CultUtility.SacrificeResult.none; // Default
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
            lastSacrificeCongregation = null;
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

        /// <summary>
        /// This keeps track of dead colonists who have taken the Unspeakable Oath.
        /// They will need to be resurrected. This begins the process.
        /// </summary>
        public void ResolveHasturOathtakers()
        {
            try
            {

                if (Find.TickManager.TicksGame % 100 != 0) return;
                if (unspeakableOathPawns == null) return;
                if (unspeakableOathPawns.Count <= 0) return;
                List<Pawn> tempOathList = new List<Pawn>(unspeakableOathPawns);
                if (tempOathList != null)
                {
                    foreach (Pawn oathtaker in tempOathList)
                    {
                        if (oathtaker.Dead)
                        {
                            if (unspeakableOathPawns != null) unspeakableOathPawns.Remove(oathtaker);
                            if (toBeResurrected == null) toBeResurrected = new Stack<Corpse>();
                            Corpse resurrectee = oathtaker.Corpse;
                            toBeResurrected.Push(resurrectee);
                            Cthulhu.Utility.DebugReport("Started Resurrection Process");
                            ticksUntilResurrection = resurrectionTicks;
                        }
                    }
                }
            }
            catch (NullReferenceException)
            { }
        }

        /// <summary>
        /// When Oathtakers die, they need to be resurrected after a period of time.
        /// 
        /// </summary>
        public void ResolveHasturResurrections()
        {
            //Check ticks

            if (ticksUntilResurrection == -999) return;

            if (ticksUntilResurrection > 0)
            {
                ticksUntilResurrection--;
                return;
            }
            ticksUntilResurrection = -999;


            //Ticks passed. Commence resurrection!
            HasturResurrection();

            //Do we still have colonists that need resurrection? If so, let's proceed with another round of resurrection.
            //Reset the timer, and let's get to work.
            if (toBeResurrected != null)
            {
                if (toBeResurrected.Count > 0)
                {
                    ticksUntilResurrection = resurrectionTicks;
                }
            }
        }

        public void HasturResurrection()
        {
            Corpse sourceCorpse = toBeResurrected.Pop();
            IntVec3 spawnLoc = IntVec3.Invalid;
            Map map = null;
            
            if (sourceCorpse.InnerPawn != null)
            {
                map = sourceCorpse.MapHeld;
                spawnLoc = sourceCorpse.PositionHeld;
                if (spawnLoc == IntVec3.Invalid) spawnLoc = sourceCorpse.Position;
                if (spawnLoc == IntVec3.Invalid) spawnLoc = DropCellFinder.RandomDropSpot(map);

                ReanimatedPawn newPawn = ReanimatedPawnUtility.DoGenerateZombiePawnFromSource(sourceCorpse.InnerPawn, false, true);

                //Hops / Other storage buildings
                Building building = StoreUtility.StoringBuilding(sourceCorpse);
                if (building != null)
                {
                    Building_Storage buildingS = building as Building_Storage;
                    if (buildingS != null)
                    {
                        buildingS.Notify_LostThing(sourceCorpse);
                    }

                }
                if (sourceCorpse.holdingOwner != null)
                {
                    if (sourceCorpse.holdingOwner.Owner != null)
                    {
                        Building_Casket casket = sourceCorpse.holdingOwner.Owner as Building_Casket;
                        if (casket != null)
                        {
                            casket.EjectContents();
                            Cthulhu.Utility.DebugReport("Resurection:: Casket/grave/sarcophogi opened.");
                        }

                    }
                }

                Messages.Message("ReanimatedOath".Translate(new object[] {
                    sourceCorpse.InnerPawn.Name
                }), MessageSound.SeriousAlert);
                //Log.Message(newPawn.NameStringShort);
                //Log.Message(spawnLoc.ToString());
                //Log.Message(map.ToString());

                GenSpawn.Spawn(newPawn, spawnLoc, map);
                sourceCorpse.Destroy(0);
            }
        }

        #endregion Tick

        #region Reports
        public string GenerateFailureString()
        {
            StringBuilder s = new StringBuilder();
            int ran = Rand.Range(1, 40);
            string message = "SacrificeFailMessage" + ran.ToString();
            string messageObject = message.Translate(new object[]
            {
                    lastUsedAltar.executioner
            });
            s.Append(messageObject);
            return s.ToString();
        }

        public void GenerateSacrificeMessage()
        {
            StringBuilder s = new StringBuilder();
            string labelToTranslate = "Error";
            string textLabel = "Error";
            LetterDef letterDef = LetterDefOf.BadNonUrgent;
            s.Append("SacrificeIntro".Translate());
            s.Append(" " + lastUsedAltar.currentSacrificeDeity.Label);

            //The sacrifice is human
            if (lastSacrificeType == CultUtility.SacrificeType.human)
            {
             
                s.Append(" " + lastUsedAltar.currentSpell.letterLabel + ". ");

                //Was the executioner a family member?
                if (HSMwasFamily)
                {
                    if (lastUsedAltar.executioner == null) Log.Error("Executioner null");
                    if (lastRelation == null) Log.Error("Null relation");
                    if (lastSacrificeName == null) Log.Error("Null name");
                    string familyString = "HumanSacrificeWasFamily".Translate((new object[]
                    {
                            lastUsedAltar.executioner.LabelShort,
                            lastUsedAltar.executioner.gender.GetPossessive(),
                            lastRelation.label,
                            lastSacrificeName
                    }));
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
                    "Even so",
                    "Fortunately",
                    "Despite this setback",
                    "Luckily"
                };
                    s.Append(". " + buts.RandomElement<string>() + ", ");
                }
                if ((int)lastResult > 2)
                    s.Append(lastUsedAltar.executioner.ToString() + " " + lastUsedAltar.currentSpell.letterText + ".");
                s.Append(" The ritual was a ");

                switch (lastResult)
                {
                    case CultUtility.SacrificeResult.success:
                        s.Append("complete success.");
                        letterDef = CultsDefOfs.Cults_StandardMessage;
                        break;
                    case CultUtility.SacrificeResult.mixedsuccess:
                        letterDef = CultsDefOfs.Cults_StandardMessage;
                        s.Append("mixed success.");
                        break;
                    case CultUtility.SacrificeResult.failure:
                        s.Append("failure.");
                        break;
                    case CultUtility.SacrificeResult.criticalfailure:
                        s.Append("complete failure.");
                        break;
                    case CultUtility.SacrificeResult.none:
                        s.Append("this should never happen");
                        break;
                }
                labelToTranslate = "SacrificeLabel" + lastResult.ToString();
            }
            else if (lastSacrificeType == CultUtility.SacrificeType.animal)
            {
                s.Append(" " + "AnimalSacrificeReason".Translate() + ".");
                if (ASMwasPet) s.Append(" " + "AnimalSacrificeWasPet".Translate() + lastUsedAltar.currentSacrificeDeity.Label + ".");
                if (ASMwasBonded)
                {
                    string bondString = "AnimalSacrificeWasBonded".Translate((new object[]
                    {
                            lastSacrificeName,
                            lastUsedAltar.executioner.LabelShort
                    }));
                    s.Append(" " + bondString +".");
                }
                if (ASMwasExcMaster)
                {
                    string bondString = "AnimalSacrificeWasExcMaster".Translate((new object[]
                    {
                            lastSacrificeName,
                            lastUsedAltar.executioner.LabelShort
                    }));
                    s.Append(" " + bondString + ".");
                }
                if (ASMwasBonded || ASMwasPet || ASMwasExcMaster)
                {
                    s.Append(" " + "GreedilyReceived".Translate() + lastUsedAltar.currentSacrificeDeity.Label + ".");
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
            Scribe_Collections.Look<Corpse>(ref this.toBeResurrected, "toBeResurrected", LookMode.Reference);
            Scribe_Values.Look<int>(ref this.ticksUntilResurrection, "ticksUntilResurrection", -999);
            Scribe_Collections.Look<Pawn>(ref this.unspeakableOathPawns, "unspeakableOathPawns", LookMode.Reference, new object[0]);
            
            //Defend the Brood Spell
            Scribe_Collections.Look<Pawn>(ref this.defendTheBroodPawns, "defendTheBroodPawns", LookMode.Reference, new object[0]);
            
            //Sacrifice Variables
            Scribe_Values.Look<CultUtility.SacrificeResult>(ref this.lastResult, "lastResult", CultUtility.SacrificeResult.none, false);
            Scribe_References.Look<Building_SacrificialAltar>(ref this.lastUsedAltar, "lastUsedAltar", false);
            Scribe_Defs.Look<IncidentDef>(ref this.lastSideEffect, "lastSideEffect");
            Scribe_Values.Look<bool>(ref this.wasDoubleTheFun, "wasDoubleTheFun", false, false);
            Scribe_Values.Look<IntVec3>(ref this.lastLocation, "lastLocation", IntVec3.Invalid, false);
            Scribe_Defs.Look<IncidentDef>(ref this.lastDoubleSideEffect, "lastDoubleSideEffect");
            Scribe_Defs.Look<IncidentDef>(ref this.lastSpell, "lastSpell");
            base.ExposeData();

        }
    }
}