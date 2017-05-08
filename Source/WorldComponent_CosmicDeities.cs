using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using RimWorld.Planet;

namespace CultOfCthulhu
{
    public class WorldComponent_CosmicDeities : WorldComponent
    {
        public Dictionary<CosmicEntity, int> DeityCache = new Dictionary<CosmicEntity, int>();
        public bool AreDeitiesSpawned = false;
        public bool WasCultMindednessInitialized = false;

        public WorldComponent_CosmicDeities(World world) : base(world)
        {
        }

        public CosmicEntity GetCache(CosmicEntity deity)
        {
            CosmicEntity result;
            bool flag1 = DeityCache == null;
            if (flag1)
            {
                DeityCache = new Dictionary<CosmicEntity, int>();
            }

            foreach (CosmicEntity current in DeityCache.Keys)
            {
                if (current == deity)
                {
                    result = current;
                    return result;
                }
            }

            CosmicEntity cosmicEntity = deity;
            DeityCache.Add(deity, deity.Version);
            result = deity;
            return result;
        }

        public void GenerateCosmicEntitiesIntoWorld()
        {
            if (!AreDeitiesSpawned)
            {
                foreach (ThingDef current in DefDatabase<ThingDef>.AllDefs)
                {
                    if (current.thingClass == typeof(CosmicEntity))
                    {
                        CosmicEntity x = new CosmicEntity(current);
                        //x.Position = new IntVec3();
                        //x.SpawnSetup();
                        GetCache(x);
                    }
                }
                AreDeitiesSpawned = true;
                //Cthulhu.Utility.DebugReport("Cosmic Deities Spawned");

            }
            return;
        }

        public override void WorldComponentTick()
        {
            GenerateCosmicEntitiesIntoWorld();
            RevealDeityCheck();
            base.WorldComponentTick();
        }

        public List<CosmicEntity> undiscoveredEntities()
        {
            List<CosmicEntity> result = new List<CosmicEntity>();
            foreach (CosmicEntity entity in this.DeityCache.Keys.InRandomOrder<CosmicEntity>())
            {
                if (entity.discovered == false)
                {
                    result.Add(entity);
                    //Cthulhu.Utility.DebugReport(entity.Label);
                }
            }
            return result;
        }

        public void RevealDeityCheck()
        {

            //Cthulhu.Utility.DebugReport("Reveal Deity Check");
            ResearchProjectDef deityResearch = ResearchProjectDef.Named("Forbidden_Deities");


            if (deityResearch.IsFinished && undiscoveredEntities().Count > 0)
            {
                foreach (CosmicEntity entity in undiscoveredEntities())
                {
                    entity.discovered = true;
                    Cthulhu.Utility.DebugReport("Change research should be called.");
                    Cthulhu.Utility.ChangeResearchProgress(deityResearch, 0f, true);
                    Messages.Message(entity.Label + " was discovered amongst the strange symbols.", MessageSound.Benefit);
                    break;
                }
            }
            else if (undiscoveredEntities().Count == 0)
            {
                Cthulhu.Utility.ChangeResearchProgress(deityResearch, deityResearch.baseCost);
            }
        }

        public void ReloadCosmicEntity(CosmicEntity entity)
        {
            float currentFavor = entity.PlayerFavor;
            bool currentDiscovery = entity.discovered;
            //Remove entity
            DeityCache.Remove(entity);

            //New deity
            CosmicEntity x = new CosmicEntity(entity.def);
            x.AffectFavor(currentFavor);
            x.discovered = currentDiscovery;
            DeityCache.Add(x, x.Version);

            //Destroy deity
            entity.Destroy(0);
            //Cthulhu.Utility.DebugReport("Reloaded " + entity.Label);
            return;
        }

        public void CheckForUpdates()
        {

            //Create a temporary dictionary.
            //Load all the current deities into it.

            Dictionary<CosmicEntity, int> tempDic = new Dictionary<CosmicEntity, int>();
            foreach (KeyValuePair<CosmicEntity, int> pair in DeityCache)
            {
                tempDic.Add(pair.Key, pair.Value);
            }

            //Now, check to see if the saved "version" matches the new "version" we loaded.

            List<CosmicEntity> entitiesToUpdate = new List<CosmicEntity>();
            foreach (KeyValuePair<CosmicEntity, int> pair in tempDic)
            {
                //Version mismatch, let's update!
                if (pair.Key.Version != pair.Value)
                {
                    entitiesToUpdate.Add(pair.Key);
                    //Cthulhu.Utility.DebugReport("To be updated +1");
                }
                //Cthulhu.Utility.DebugReport("Cycled");
            }


            foreach (CosmicEntity entity in entitiesToUpdate)
            {
                ReloadCosmicEntity(entity);
            }

            //Deities are updated, but let's check if there are new deities.
            foreach (ThingDef current in DefDatabase<ThingDef>.AllDefs)
            {
                if (current.thingClass == typeof(CosmicEntity))
                {
                    CosmicEntity newDeity = new CosmicEntity(current);

                    if (tempDic.Keys.FirstOrDefault((CosmicEntity oldDeity) => oldDeity.def.defName == newDeity.def.defName) != null)
                    {
                        continue;
                    }
                    newDeity.discovered = false;
                    GetCache(newDeity);
                    //RevealDeityCheck();
                }
            }

            //Clear that dictionary
            tempDic = null;

        }


        public override void ExposeData()
        {
            Scribe_Collections.Look<CosmicEntity, int>(ref this.DeityCache, "Deities", LookMode.Deep, LookMode.Value);
            //Scribe_Collections.Look<CosmicEntity>(ref this.DeityCache, "Deities", LookMode.Deep, new object[0]);
            Scribe_Values.Look<bool>(ref this.AreDeitiesSpawned, "AreDeitiesSpawned", false, false);
            base.ExposeData();
            if (DeityCache == null)
            {
                DeityCache = new Dictionary<CosmicEntity, int>();
            }
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                GenerateCosmicEntitiesIntoWorld();
                CheckForUpdates();
            }

        }
    }

}