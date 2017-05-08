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

    public class MapComponent_FertilityMods : MapComponent
    {
        public MapComponent_FertilityMods(Map map) : base (map)
        {
            this.map = map;
        }

        //public static Dictionary<IntVec3, TerrainDef> newTerrainList = new Dictionary<IntVec3, TerrainDef>();
        //public static Dictionary<IntVec3, TerrainDef> oldTerrainList = new Dictionary<IntVec3, TerrainDef>();

        public Dictionary<IntVec3, TerrainDef> newTerrainSaveList = new Dictionary<IntVec3, TerrainDef>();
        public Dictionary<IntVec3, TerrainDef> oldTerrainSaveList = new Dictionary<IntVec3, TerrainDef>();

        public static float fertilityBonus = 1.0f;
        public static float fertilityMax = 1.75f;

        public IEnumerable<Building_TotemFertility> FertilityTotems
        {
            get {
                return from Building_TotemFertility totems in map.listerBuildings.AllBuildingsColonistOfClass<Building_TotemFertility>()
                       where totems != null
                   select totems;
            }
        }
        public IEnumerable<IntVec3> ActiveCells
        {
            get
            {
                List<IntVec3> tempList = new List<IntVec3>();
                foreach (Building_TotemFertility totem in FertilityTotems)
                {
                    foreach (IntVec3 cell in totem.GrowableCells)
                    {
                        tempList.Add(cell);
                    }
                }
                return (IEnumerable<IntVec3>)tempList;
            }
        }

        public MapComponent_FertilityMods Get
        {
            get
            {
                MapComponent_FertilityMods MapComponent_FertilityMods = map.components.OfType<MapComponent_FertilityMods>().FirstOrDefault<MapComponent_FertilityMods>();
                bool flag = MapComponent_FertilityMods == null;
                if (flag)
                {
                    MapComponent_FertilityMods = new MapComponent_FertilityMods(map);
                    map.components.Add(MapComponent_FertilityMods);
                }
                return MapComponent_FertilityMods;
            }
        }

        public void FertilizeCells(List<IntVec3> GrowableCells)
        {
            if (GrowableCells == null)
            {
                Log.Error("Missing Growable Cells List");
                return;
            }
              
            foreach (IntVec3 cell in GrowableCells)
            {
                if (cell != null)
                {
                    ModifyFertility(cell);
                }
            }
            
        }

        public void UnfertilizeCells(List<IntVec3> GrowableCells)
        {
            if (oldTerrainSaveList == null)
            {
                Log.Error("Missing Terrain List");
                return;
            }
            if (GrowableCells == null)
            {
                Log.Error("Missing Growable Cells List");
                return;
            }
            Cthulhu.Utility.DebugReport("Cleaning Up");
            foreach (IntVec3 vec in GrowableCells)
            {
                if (!ActiveCells.Contains<IntVec3>(vec))
                {
                    newTerrainSaveList.Remove(vec);
                    TerrainDef temp = new TerrainDef();
                    CopyTerrain(oldTerrainSaveList[vec], temp);
                    OriginFertility(temp);
                    map.terrainGrid.SetTerrain(vec, temp);
                }
            }
        }

        public static void CopyTerrain(TerrainDef oldTerrain, TerrainDef newTerrain)
        {
            //Copy BuildableDef parts
            StringBuilder s = new StringBuilder();
            s.Append("Copy BuildableDef parts");
            s.AppendLine();
            newTerrain.passability = oldTerrain.passability;
            s.Append("CostList");
            s.AppendLine();
            if (oldTerrain.costList != null)
            {
                newTerrain.costList = new List<ThingCountClass>();
                foreach (ThingCountClass count in oldTerrain.costList)
                {
                    newTerrain.costList.Add(count);
                }
            }
            newTerrain.costStuffCount = oldTerrain.costStuffCount;
           
            s.Append("StuffCategories");
            s.AppendLine();
            if (oldTerrain.stuffCategories != null)
            {
                newTerrain.stuffCategories = new List<StuffCategoryDef>();
                foreach (StuffCategoryDef stuff in oldTerrain.stuffCategories)
                {
                    newTerrain.stuffCategories.Add(stuff);
                }
            }
            s.Append("BuildingPrerequisites");
            s.AppendLine();
            if (oldTerrain.buildingPrerequisites != null)
            {
                newTerrain.buildingPrerequisites = new List<ThingDef>();
                foreach (ThingDef def in oldTerrain.buildingPrerequisites)
                {
                    newTerrain.buildingPrerequisites.Add(def);
                }
            }

            s.Append("ResearchPrerequisities");
            s.AppendLine();
            if (oldTerrain.researchPrerequisites != null)
            {
                newTerrain.researchPrerequisites = new List<ResearchProjectDef>();
                foreach (ResearchProjectDef def in oldTerrain.researchPrerequisites)
                {
                    newTerrain.researchPrerequisites.Add(def);
                }
            }
            newTerrain.placingDraggableDimensions = oldTerrain.placingDraggableDimensions;
            newTerrain.repairEffect = oldTerrain.repairEffect;
            newTerrain.defaultPlacingRot = oldTerrain.defaultPlacingRot;
            newTerrain.blueprintDef = oldTerrain.blueprintDef;
            newTerrain.installBlueprintDef = oldTerrain.installBlueprintDef;
            newTerrain.frameDef = oldTerrain.frameDef;
            newTerrain.uiIconPath = oldTerrain.uiIconPath;
            newTerrain.altitudeLayer = oldTerrain.altitudeLayer;
            newTerrain.uiIcon = oldTerrain.uiIcon;
            newTerrain.graphic = oldTerrain.graphic;
            newTerrain.menuHidden = true;
            newTerrain.specialDisplayRadius = oldTerrain.specialDisplayRadius;


            s.Append("Placeworkers");
            s.AppendLine();
            if (oldTerrain.placeWorkers != null)
            {
                newTerrain.placeWorkers = new List<Type>();
                foreach (Type worker in oldTerrain.placeWorkers)
                {
                    newTerrain.placeWorkers.Add(worker);
                }
            }
            newTerrain.designationHotKey = oldTerrain.designationHotKey;

            //Floor Base-like
            newTerrain.layerable = oldTerrain.layerable;

            s.Append("Affordances");
            s.AppendLine();
            if (oldTerrain.affordances != null)
            {
                newTerrain.affordances = new List<TerrainAffordance>();
                foreach (TerrainAffordance affordance in oldTerrain.affordances)
                {
                    newTerrain.affordances.Add(affordance);
                }
            }

            s.Append("StatBases");
            s.AppendLine();
            if (oldTerrain.statBases != null)
            {
                newTerrain.statBases = new List<StatModifier>();
                foreach (StatModifier modifier in oldTerrain.statBases)
                {
                    newTerrain.statBases.Add(modifier);
                }
            }
            newTerrain.designationCategory = oldTerrain.designationCategory;
            newTerrain.fertility = oldTerrain.fertility;
            newTerrain.constructEffect = oldTerrain.constructEffect;
            newTerrain.acceptTerrainSourceFilth = oldTerrain.acceptTerrainSourceFilth;
            newTerrain.terrainAffordanceNeeded = oldTerrain.terrainAffordanceNeeded;


            //Floor defs
            newTerrain.defName = oldTerrain.defName;
            newTerrain.label = oldTerrain.label;
            newTerrain.description = oldTerrain.description;
            newTerrain.color = oldTerrain.color;
            newTerrain.texturePath = oldTerrain.texturePath;
            newTerrain.edgeType = oldTerrain.edgeType;
            newTerrain.renderPrecedence = oldTerrain.renderPrecedence;
            newTerrain.pathCost = oldTerrain.pathCost;
            newTerrain.statBases = oldTerrain.statBases;
            newTerrain.scatterType = oldTerrain.scatterType;

            newTerrain.takeFootprints = oldTerrain.takeFootprints;
            newTerrain.driesTo = oldTerrain.driesTo;

            Cthulhu.Utility.DebugReport(s.ToString());
        }

        public void ModifyFertility(IntVec3 cell)
        {
            try
            {
                if (map.terrainGrid.TerrainAt(cell) != null)
                {
                    //Copy the old terrain type
                    
                    TerrainDef oldTerrain = map.terrainGrid.TerrainAt(cell);
                    TerrainDef saveTerrain = new TerrainDef();
                    TerrainDef newTerrain = new TerrainDef();
                    TerrainDef junkdef;
                    
                    //Copy Settings
                    CopyTerrain(oldTerrain, saveTerrain);

                    //Copy the original def to the list.
                    //Orginally oldTerrainList
                    if (oldTerrainSaveList == null) oldTerrainSaveList = new Dictionary<IntVec3, TerrainDef>();
                    if (oldTerrainSaveList.TryGetValue(cell, out junkdef))
                    {
                        oldTerrainSaveList[cell] = saveTerrain;
                    }
                    else
                    {
                        oldTerrainSaveList.Add(cell, saveTerrain);
                    }

                    //Copy Settings
                    CopyTerrain(oldTerrain, newTerrain);

                    //Modify the fertility rate.
                    NewFertility(newTerrain);

                    //Copy the new terrain
                    if (newTerrainSaveList == null) newTerrainSaveList = new Dictionary<IntVec3, TerrainDef>();
                    if (newTerrainSaveList.TryGetValue(cell, out junkdef))
                    {
                        newTerrainSaveList[cell] = newTerrain;
                    }
                    else
                    {
                        newTerrainSaveList.Add(cell, newTerrain);
                    }

                    //Set terrain.
                    map.terrainGrid.SetTerrain(cell, newTerrain);
                }

            }
            catch
            {
                Cthulhu.Utility.DebugReport("Oh snap.");
            }
        }
        public static float OriginFertility(TerrainDef originTerrain)
        {
            //Cthulhu.Utility.DebugReport("Searching original fertility");
            return originTerrain.fertility = DefDatabase<TerrainDef>.GetNamed(originTerrain.defName).fertility;
        }

        public static float NewFertility(TerrainDef newTerrain)
        {
            //Cthulhu.Utility.DebugReport("New Fertility ");
            newTerrain.fertility = DefDatabase<TerrainDef>.GetNamed(newTerrain.defName).fertility;
            return newTerrain.fertility = (newTerrain.fertility < fertilityMax) ? newTerrain.fertility += fertilityBonus : newTerrain.fertility = fertilityMax;
        }

        public void SaveFertility()
        {
            //if (oldTerrainList == null)
            if (newTerrainSaveList == null)
            {
                Log.Error("Missing New Terrain List");
                return;
            }
            //foreach (KeyValuePair<IntVec3, TerrainDef> pair in oldTerrainList)
            foreach (KeyValuePair<IntVec3, TerrainDef> pair in newTerrainSaveList)
            {
                //Make a temp terrain
                TerrainDef temp = new TerrainDef();
                //Copy the terrain def
                CopyTerrain(pair.Value, temp);
                //Place the new terrain def where the old one was
                map.terrainGrid.SetTerrain(pair.Key, temp);
            }
        }

        public void LoadFertility()
        {
            if (newTerrainSaveList == null)
            {
                Log.Error("Missing New Terrain List");
                return;
            }
            //Load the old terrain first.
            foreach (KeyValuePair<IntVec3, TerrainDef> pair in oldTerrainSaveList)
            {
                TerrainDef temp = new TerrainDef();
                CopyTerrain(pair.Value, temp);
                OriginFertility(temp);
                map.terrainGrid.SetTerrain(pair.Key, temp);
            }
            //Apply new terrain effects again.
            foreach (KeyValuePair<IntVec3, TerrainDef> pair in newTerrainSaveList)
            {
                TerrainDef temp = new TerrainDef();
                CopyTerrain(pair.Value, temp);
                NewFertility(temp);
                map.terrainGrid.SetTerrain(pair.Key, temp);
            }
        }

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                //oldTerrainSaveList = oldTerrainList;
                //newTerrainSaveList = newTerrainList;
                //this.SaveFertility();
            }

            Scribe_Collections.Look<IntVec3, TerrainDef>(ref this.oldTerrainSaveList, "oldTerrainList");
            Scribe_Collections.Look<IntVec3, TerrainDef>(ref this.newTerrainSaveList, "newTerrainList");
            base.ExposeData();
            
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                //oldTerrainList = oldTerrainSaveList;
                //newTerrainList = newTerrainSaveList;
                this.LoadFertility();
            }
        }



    }
}
