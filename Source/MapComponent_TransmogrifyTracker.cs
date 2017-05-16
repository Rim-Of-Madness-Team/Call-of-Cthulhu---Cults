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
using System.Reflection;
using System.Text.RegularExpressions;
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

namespace CultOfCthulhu
{
    class MapComponent_TransmogrifyTracker : MapComponent
    {
        public MapComponent_TransmogrifyTracker(Map map) : base (map)
        {
            this.map = map;
        }

        public Dictionary<int, ThingDef> newDefList = new Dictionary<int, ThingDef>();
        public bool postSave = false;
        public bool postLoad = false;

        public List<Pawn> toBeRemoved = new List<Pawn>();
        public Dictionary<int,ThingDef> toBeRefreshed = new Dictionary<int,ThingDef>();

        public MapComponent_TransmogrifyTracker Get
        {
            get
            {
                MapComponent_TransmogrifyTracker MapComponent_TransmogrifyTracker = map.components.OfType<MapComponent_TransmogrifyTracker>().FirstOrDefault<MapComponent_TransmogrifyTracker>();
                bool flag = MapComponent_TransmogrifyTracker == null;
                if (flag)
                {
                    MapComponent_TransmogrifyTracker = new MapComponent_TransmogrifyTracker(map);
                    map.components.Add(MapComponent_TransmogrifyTracker);
                }
                return MapComponent_TransmogrifyTracker;
            }
        }

        public IEnumerable<Pawn> PetsToTransmogrify
        {
            get {
                   //Get a pet with a master.
                   IEnumerable<Pawn> one =  from Pawn pets in map.mapPawns.AllPawnsSpawned
                   where pets.RaceProps.Animal && pets.Faction == Faction.OfPlayer && !pets.Dead && !pets.Downed && pets.RaceProps.petness > 0f && pets.playerSettings.master != null
                   select pets;
                    //No master? Okay, still search for pets.
                    if (one.Count<Pawn>() == 0)
                {
                    one = from Pawn pets in map.mapPawns.AllPawnsSpawned
                          where pets.RaceProps.Animal && pets.Faction == Faction.OfPlayer && !pets.Dead && !pets.Downed && pets.RaceProps.petness > 0f
                          select pets;
                }
                    //No pets? Okay, search for player animals.
                    if (one.Count<Pawn>() == 0)
                {
                    one = from Pawn pets in map.mapPawns.AllPawnsSpawned
                          where pets.RaceProps.Animal && pets.Faction == Faction.OfPlayer && !pets.Dead && !pets.Downed
                          select pets;
                }   
                    //Return anything if we find anything, or return a null, it's all good.
                return one;

            }
        }
        
        public IEnumerable<ThingDef_Transmogrified> TransmogrifyDefs
        {
            get
            {
                return from ThingDef_Transmogrified def in DefDatabase<ThingDef_Transmogrified>.AllDefs
                       where def.InUse == false
                       select def;

            }
        }

        public Pawn Transmogrify(Pawn pawn=null, bool loadMode=false)
        {
            //No pawn? Okay, find one.
            if (pawn == null)
                pawn = PetsToTransmogrify.RandomElement<Pawn>();
   
            if (loadMode)
            {
                ThingDef prevDef;
                if (!newDefList.TryGetValue(pawn.thingIDNumber, out prevDef))
                {
                    Log.Error("Can't get value from dictionary");
                }
            }

            //Old Pawn
            //Keeps a duplicate inside the loop to prevent problems.
            IntVec3 oldLoc = pawn.Position;
            ThingDef oldDef = pawn.def;
            string oldString = pawn.ToString();
            //Pawn oldPawn = GenerateNewPawnFromSource(pawn.def, pawn);
            pawn.DeSpawn();
            Cthulhu.Utility.DebugReport("DeSpawned " + oldString);

            //New def
            ///////////////ThingDef newDef = GenerateMonstrousDef(oldDef);
            ///////////////TransmogrifyRacialValues(oldDef, newDef);

            //New Pawn with new def
            Pawn newPawn = null; //////// Cthulhu.Utility.GenerateNewPawnFromSource(newDef, pawn);
            UpgradeBody(newPawn);
            GenSpawn.Spawn(newPawn, oldLoc, map);
           
            //Add new pawn and old def to the list
            newDefList.Add(newPawn.thingIDNumber, oldDef);

            pawn.Destroy(0);
            //pawn.Discard();

            if (!loadMode) Messages.Message(newPawn.LabelShort + "'s form has been enhanced.", MessageSound.Benefit);
            
            return newPawn;
        }

        

        public void TransmogrifyRacialValues(ThingDef oldDef, ThingDef newDef)
        {
            newDef.race.body = oldDef.race.body;

            newDef.race.intelligence = Intelligence.ToolUser;
            newDef.race.foodType = FoodTypeFlags.CarnivoreAnimalStrict;
            newDef.race.predator = true;

            newDef.race.lifeExpectancy += 50;
            newDef.race.baseBodySize += 1.0f;
            newDef.race.baseHealthScale += 0.5f;
            newDef.race.baseHungerRate += 0.1f;

        }

        public void UpgradeBody(Pawn newPawn)
        {

            //Where is your head, sir?
            BodyPartRecord tempRecord = null;
            foreach (BodyPartRecord current in newPawn.RaceProps.body.AllParts)
            {
                if (current.def == BodyPartDefOf.Brain)
                {
                    tempRecord = current;
                }
            }

            //Where is your body, sir?
            BodyPartRecord tempRecord2 = null;
            foreach (BodyPartRecord current in newPawn.RaceProps.body.AllParts)
            {
                if (current.def == BodyPartDefOf.Heart)
                {
                    tempRecord2 = current;
                }
            }

            //Error catch: Missing head!
            if (tempRecord == null)
            {
                Log.Error("Couldn't find brain of the pawn to upgrade.");
                return;
            }

            //Error catch: Missing body!
            if (tempRecord2 == null)
            {
                Log.Error("Couldn't find heart to upgrade.");
                return;
            }

            //Check if they are already upgraded.
            foreach (Hediff current in newPawn.health.hediffSet.hediffs)
            {
                if (current.def == CultsDefOf.Cults_MonstrousBody)
                {
                    Messages.Message(newPawn.LabelShort + " already posesses " + CultsDefOf.Cults_MonstrousBody.label, MessageSound.Negative);
                    return;
                }
                if (current.def == CultsDefOf.Cults_MonstrousBrain)
                {
                    Messages.Message(newPawn.LabelShort + " already posesses " + CultsDefOf.Cults_MonstrousBrain.label, MessageSound.Negative);
                    return;
                }
            }
            
            newPawn.health.AddHediff(CultsDefOf.Cults_MonstrousBody, tempRecord2, null);
            newPawn.health.AddHediff(CultsDefOf.Cults_MonstrousBrain, tempRecord, null);

        }

        public void DebugDef(ThingDef d)
        {
            string s =
            "ThingClass: " + d.thingClass.ToString() + "@" +
            "Category: " + d.category.ToString() + "@" +
            "Selectable: " + d.selectable.ToString() + "@" +
            "TickerType: " + d.tickerType.ToString() + "@" +
            "altitudeLayer: " + d.altitudeLayer.ToString() + "@" +
            "useHitPoints: " + d.useHitPoints.ToString() + "@" +
            "hasTooltip: " + d.hasTooltip.ToString() + "@" +
            "alwaysHaulable: " + d.alwaysHaulable.ToString() + "@" +
            "socialPropernessMatters " + d.socialPropernessMatters.ToString() + "@" +
            "pathCost: " + d.pathCost.ToString() + "@" +
            "tradeability: " + d.tradeability.ToString() + "@" +
            "soundImpactDefault: " + d.soundImpactDefault.ToString() + "@" +
            "comps: " + d.comps.ToString() + "@" +
            "drawGUIOnMap: " + d.drawGUIOverlay.ToString() + "@" +
            "defName: " + d.defName.ToString() + "@" +
            "label: " + d.label.ToString() + "@" +
            "description: " + d.description.ToString();

            //"inspectorTabs: " + d.inspectorTabs.ToString() + "@" +
            //"tradeTags: " + d.tradeTags.ToString() + "@" +
            //"statBases: " + d.statBases.ToString() + "@" +
            //"race: " + d.race.ToString() + "@" +
            //"recipes: " + d.recipes.ToString() + "@

            s = s.Replace("@", System.Environment.NewLine);

            Cthulhu.Utility.DebugReport
            (
                s
            );
        }

        public void LoadTransmogrification()
        {
            Cthulhu.Utility.DebugReport("Started Transmogrification");
            if (this.newDefList == null)
            {
                Log.Error("Missing New Def List");
                return;
            }
            try
            {
                int y = newDefList.Count;
                Cthulhu.Utility.DebugReport(y.ToString());
                for (int i = 0; i < y; i++)
                {
                    foreach (KeyValuePair<int, ThingDef> x in this.newDefList)
                    {
                        //this.newDefList.RemoveAll((KeyValuePair<Pawn, ThingDef> x) => x.Key.Destroyed);
                        Pawn p = map.mapPawns.AllPawnsSpawned.Find((Pawn s) => s.thingIDNumber == x.Key);
                        if (!IsMonstrous(p))
                        {
                            Transmogrify(p, true);
                            break;
                        }
                    }
                }
            }
            catch (Exception e) { Cthulhu.Utility.DebugReport(e.ToString()); }
        }

        public bool IsMonstrous(Pawn p)
        {
            if (p == null) return false;
            if (p.def == null) return false;
            if (p.def.defName == null) return false;
            if (p.def.defName.Contains("Monstrous"))
            {
                return true;
            }
            return false;
        }

        public void RefreshMonstrous(Pawn pawn)
        {
            if (!IsMonstrous(pawn))
            {
                pawn.def = toBeRefreshed[pawn.thingIDNumber];
            }
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (this.postSave)
            {
                this.postSave = false;

                foreach (int i in toBeRefreshed.Keys)
                {
                    Pawn p = map.mapPawns.AllPawnsSpawned.Find((Pawn x) => x.thingIDNumber == i);
                    RefreshMonstrous(p);
                }
                toBeRefreshed = new Dictionary<int,ThingDef>();
            }

            if (this.postLoad)
            {
                this.postLoad = false;
                
                //this.newDefList.RemoveAll((KeyValuePair<string, ThingDef> x) => x.Key.Destroyed);
                if (this.newDefList.Count > 0) Cthulhu.Utility.DebugReport(this.newDefList.ToStringFullContents<int, ThingDef>());

                LoadTransmogrification();
            }

        }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                //this.newDefList.RemoveAll((KeyValuePair<string, ThingDef> x) => x.Key.Destroyed);
                if (this.newDefList.Count > 0) Cthulhu.Utility.DebugReport(this.newDefList.ToStringFullContents<int, ThingDef>());

                for (int i = 0; i < newDefList.Count; i++)
                {
                    foreach (KeyValuePair<int, ThingDef> x in newDefList)
                    {
                        Pawn p = map.mapPawns.AllPawnsSpawned.Find((Pawn q) => q.thingIDNumber == x.Key);
                        if (IsMonstrous(p))
                        {
                            toBeRefreshed.Add(x.Key, p.def);
                            p.def = x.Value;
                            Cthulhu.Utility.DebugReport(x.Key.ToString() + " " + x.Value.ToString());
                            
                            break;
                        }
                    }
                }

                postSave = true;
            }

            base.ExposeData();
            
            Scribe_Collections.Look<int, ThingDef>(ref this.newDefList, "newDefList", LookMode.Value, LookMode.Def);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (this.newDefList == null)
                {
                    this.newDefList = new Dictionary<int, ThingDef>();
                }
                postLoad = true;
            }

        }

    }
}
