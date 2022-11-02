using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cthulhu;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace CultOfCthulhu
{
    public class WorldComponent_CosmicDeities : WorldComponent
    {
        private bool AreDeitiesSpawned;
        public Dictionary<CosmicEntity, int> DeityCache = new Dictionary<CosmicEntity, int>();
        public bool WasCultMindednessInitialized = false;

        public WorldComponent_CosmicDeities(World world) : base(world: world)
        {
        }

        public CosmicEntity GetCache(CosmicEntity deity)
        {
            CosmicEntity result;
            var flag1 = DeityCache == null;
            if (flag1)
            {
                DeityCache = new Dictionary<CosmicEntity, int>();
            }

            foreach (var current in DeityCache.Keys)
            {
                if (current != deity)
                {
                    continue;
                }

                result = current;
                return result;
            }

            DeityCache.Add(key: deity, value: deity.Version);
            result = deity;
            return result;
        }

        public void orGenerate()
        {
            if (AreDeitiesSpawned)
            {
                return;
            }

            foreach (var current in DefDatabase<ThingDef>.AllDefs)
            {
                if (current.thingClass != typeof(CosmicEntity))
                {
                    continue;
                }

                var x = new CosmicEntity(newDef: current);
                //x.Position = new IntVec3();
                //x.SpawnSetup();
                GetCache(deity: x);
            }

            AreDeitiesSpawned = true;
            //Cthulhu.Utility.DebugReport("Cosmic Deities Spawned");
        }

        public override void WorldComponentTick()
        {
            orGenerate();
            RevealDeityCheck();
            base.WorldComponentTick();
        }

        private List<CosmicEntity> undiscoveredEntities()
        {
            var result = new List<CosmicEntity>();
            foreach (var entity in DeityCache.Keys.InRandomOrder())
            {
                if (entity.discovered == false)
                {
                    result.Add(item: entity);
                    //Cthulhu.Utility.DebugReport(entity.Label);
                }
            }

            return result;
        }


        private void RevealDeityCheck()
        {
            //Cthulhu.Utility.DebugReport("Reveal Deity Check");
            if (Utility.deityResearchDone || !Utility.deityResearch.IsFinished)
            {
                return;
            }

            if (DeityCache.Any(predicate: pair => !pair.Key.discovered))
            {
                foreach (var entity in undiscoveredEntities())
                {
                    entity.discovered = true;
                    Utility.DebugReport(x: "Change research should be called.");
                    Utility.ChangeResearchProgress(projectDef: Utility.deityResearch, progressValue: 0f, deselectCurrentResearch: true);
                    var message = "Cults_DiscoveredDeityMessage".Translate(arg1: entity.Label);
                    Messages.Message(text: message, def: MessageTypeDefOf.PositiveEvent);

                    var s = new StringBuilder();
                    s.AppendLine(value: message);
                    s.AppendLine();
                    s.AppendLine(value: entity.Info());
                    Find.LetterStack.ReceiveLetter(label: "Cults_Discovered".Translate(), text: s.ToString(),
                        textLetterDef: LetterDefOf.NeutralEvent);
                    break;
                }
            }
            else
            {
                Utility.ChangeResearchProgress(projectDef: Utility.deityResearch, progressValue: Utility.deityResearch.baseCost);
                Utility.deityResearchDone = true;
            }
        }

        private void ReloadCosmicEntity(CosmicEntity entity)
        {
            var currentFavor = entity.PlayerFavor;
            var currentDiscovery = entity.discovered;
            //Remove entity
            DeityCache.Remove(key: entity);

            //New deity
            var x = new CosmicEntity(newDef: entity.def);
            x.AffectFavor(favorChange: currentFavor);
            x.discovered = currentDiscovery;
            DeityCache.Add(key: x, value: x.Version);

            //Destroy deity
            entity.Destroy();
            //Cthulhu.Utility.DebugReport("Reloaded " + entity.Label);
        }

        private void CheckForUpdates()
        {
            //Create a temporary dictionary.
            //Load all the current deities into it.

            var tempDic = new Dictionary<CosmicEntity, int>();
            foreach (var pair in DeityCache)
            {
                tempDic.Add(key: pair.Key, value: pair.Value);
            }

            //Now, check to see if the saved "version" matches the new "version" we loaded.

            var entitiesToUpdate = new List<CosmicEntity>();
            foreach (var pair in tempDic)
            {
                //Version mismatch, let's update!
                if (pair.Key.Version != pair.Value)
                {
                    entitiesToUpdate.Add(item: pair.Key);
                    //Cthulhu.Utility.DebugReport("To be updated +1");
                }

                //Cthulhu.Utility.DebugReport("Cycled");
            }


            foreach (var entity in entitiesToUpdate)
            {
                ReloadCosmicEntity(entity: entity);
            }

            //Deities are updated, but let's check if there are new deities.
            foreach (var current in DefDatabase<ThingDef>.AllDefs)
            {
                if (current.thingClass != typeof(CosmicEntity))
                {
                    continue;
                }

                var newDeity = new CosmicEntity(newDef: current);

                if (tempDic.Keys.FirstOrDefault(predicate: oldDeity => oldDeity.def.defName == newDeity.def.defName) != null)
                {
                    continue;
                }

                newDeity.discovered = false;
                GetCache(deity: newDeity);
                //RevealDeityCheck();
            }

            //Clear that dictionary
        }


        public override void ExposeData()
        {
            Scribe_Collections.Look(dict: ref DeityCache, label: "Deities", keyLookMode: LookMode.Deep, valueLookMode: LookMode.Value);
            //Scribe_Collections.Look<CosmicEntity>(ref this.DeityCache, "Deities", LookMode.Deep, new object[0]);
            Scribe_Values.Look(value: ref AreDeitiesSpawned, label: "AreDeitiesSpawned");
            base.ExposeData();
            if (DeityCache == null)
            {
                DeityCache = new Dictionary<CosmicEntity, int>();
            }

            if (Scribe.mode != LoadSaveMode.PostLoadInit)
            {
                return;
            }

            orGenerate();
            CheckForUpdates();
        }
    }
}