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

        public static float fertilityBonus = 1.0f;
        public static float fertilityMax = 1.75f;
        public bool listNeedsUpdate = false;

        public List<Building_TotemFertility> fertilityTotems;
        public List<Building_TotemFertility> FertilityTotems
        {
            get
            {
                if (fertilityTotems.NullOrEmpty())
                {
                    fertilityTotems = new List<Building_TotemFertility>();
                }
                return fertilityTotems;
            }
            set
            {
                fertilityTotems = value;
            }
        }

        private List<IntVec3> tempList;
        public List<IntVec3> ActiveCells
        {
            get
            {
                if (tempList.NullOrEmpty() || listNeedsUpdate)
                {
                    listNeedsUpdate = false;
                    tempList = new List<IntVec3>();
                    foreach (Building_TotemFertility totem in FertilityTotems)
                    {
                        foreach (IntVec3 cell in totem.GrowableCells)
                        {
                            tempList.Add(cell);
                        }
                    }
                }
                return tempList;
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
            listNeedsUpdate = true;
            if (GrowableCells == null)
            {
                Log.Error("Missing Growable Cells List");
                return;
            }
        }

        public void UnfertilizeCells(List<IntVec3> GrowableCells)
        {
            listNeedsUpdate = true;

            List<IntVec3> cells = GrowableCells.ToList();
            if (cells == null)
            {
                Log.Error("Missing Growable Cells List");
                return;
            }
            GrowableCells.RemoveAll(x => cells.Contains(x));
            listNeedsUpdate = true;

        }

        //public static void CopyTerrain(TerrainDef oldTerrain, TerrainDef newTerrain)
        //{
        //    //Copy BuildableDef parts
        //    StringBuilder s = new StringBuilder();
        //    s.Append("Copy BuildableDef parts");
        //    s.AppendLine();
        //    newTerrain.passability = oldTerrain.passability;
        //    s.Append("CostList");
        //    s.AppendLine();
        //    if (oldTerrain.costList != null)
        //    {
        //        newTerrain.costList = new List<ThingCountClass>();
        //        foreach (ThingCountClass count in oldTerrain.costList)
        //        {
        //            newTerrain.costList.Add(count);
        //        }
        //    }
        //    newTerrain.costStuffCount = oldTerrain.costStuffCount;

        //    s.Append("StuffCategories");
        //    s.AppendLine();
        //    if (oldTerrain.stuffCategories != null)
        //    {
        //        newTerrain.stuffCategories = new List<StuffCategoryDef>();
        //        foreach (StuffCategoryDef stuff in oldTerrain.stuffCategories)
        //        {
        //            newTerrain.stuffCategories.Add(stuff);
        //        }
        //    }
        //    s.Append("BuildingPrerequisites");
        //    s.AppendLine();
        //    if (oldTerrain.buildingPrerequisites != null)
        //    {
        //        newTerrain.buildingPrerequisites = new List<ThingDef>();
        //        foreach (ThingDef def in oldTerrain.buildingPrerequisites)
        //        {
        //            newTerrain.buildingPrerequisites.Add(def);
        //        }
        //    }

        //    s.Append("ResearchPrerequisities");
        //    s.AppendLine();
        //    if (oldTerrain.researchPrerequisites != null)
        //    {
        //        newTerrain.researchPrerequisites = new List<ResearchProjectDef>();
        //        foreach (ResearchProjectDef def in oldTerrain.researchPrerequisites)
        //        {
        //            newTerrain.researchPrerequisites.Add(def);
        //        }
        //    }
        //    newTerrain.placingDraggableDimensions = oldTerrain.placingDraggableDimensions;
        //    newTerrain.repairEffect = oldTerrain.repairEffect;
        //    newTerrain.defaultPlacingRot = oldTerrain.defaultPlacingRot;
        //    newTerrain.blueprintDef = oldTerrain.blueprintDef;
        //    newTerrain.installBlueprintDef = oldTerrain.installBlueprintDef;
        //    newTerrain.frameDef = oldTerrain.frameDef;
        //    newTerrain.uiIconPath = oldTerrain.uiIconPath;
        //    newTerrain.altitudeLayer = oldTerrain.altitudeLayer;
        //    newTerrain.uiIcon = oldTerrain.uiIcon;
        //    newTerrain.graphic = oldTerrain.graphic;
        //    newTerrain.menuHidden = true;
        //    newTerrain.specialDisplayRadius = oldTerrain.specialDisplayRadius;


        //    s.Append("Placeworkers");
        //    s.AppendLine();
        //    if (oldTerrain.placeWorkers != null)
        //    {
        //        newTerrain.placeWorkers = new List<Type>();
        //        foreach (Type worker in oldTerrain.placeWorkers)
        //        {
        //            newTerrain.placeWorkers.Add(worker);
        //        }
        //    }
        //    newTerrain.designationHotKey = oldTerrain.designationHotKey;

        //    //Floor Base-like
        //    newTerrain.layerable = oldTerrain.layerable;

        //    s.Append("Affordances");
        //    s.AppendLine();
        //    if (oldTerrain.affordances != null)
        //    {
        //        newTerrain.affordances = new List<TerrainAffordance>();
        //        foreach (TerrainAffordance affordance in oldTerrain.affordances)
        //        {
        //            newTerrain.affordances.Add(affordance);
        //        }
        //    }

        //    s.Append("StatBases");
        //    s.AppendLine();
        //    if (oldTerrain.statBases != null)
        //    {
        //        newTerrain.statBases = new List<StatModifier>();
        //        foreach (StatModifier modifier in oldTerrain.statBases)
        //        {
        //            newTerrain.statBases.Add(modifier);
        //        }
        //    }
        //    newTerrain.designationCategory = oldTerrain.designationCategory;
        //    newTerrain.fertility = oldTerrain.fertility;
        //    newTerrain.constructEffect = oldTerrain.constructEffect;
        //    newTerrain.acceptTerrainSourceFilth = oldTerrain.acceptTerrainSourceFilth;
        //    newTerrain.terrainAffordanceNeeded = oldTerrain.terrainAffordanceNeeded;


        //    //Floor defs
        //    newTerrain.defName = oldTerrain.defName;
        //    newTerrain.label = oldTerrain.label;
        //    newTerrain.description = oldTerrain.description;
        //    newTerrain.color = oldTerrain.color;
        //    newTerrain.texturePath = oldTerrain.texturePath;
        //    newTerrain.edgeType = oldTerrain.edgeType;
        //    newTerrain.renderPrecedence = oldTerrain.renderPrecedence;
        //    newTerrain.pathCost = oldTerrain.pathCost;
        //    newTerrain.statBases = oldTerrain.statBases;
        //    newTerrain.scatterType = oldTerrain.scatterType;

        //    newTerrain.takeFootprints = oldTerrain.takeFootprints;
        //    newTerrain.driesTo = oldTerrain.driesTo;

        //    Cthulhu.Utility.DebugReport(s.ToString());
        //}

    }
}
