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

        public WorldComponent_CosmicDeities(World world) : base(world)
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

            DeityCache.Add(deity, deity.Version);
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

                var x = new CosmicEntity(current);
                //x.Position = new IntVec3();
                //x.SpawnSetup();
                GetCache(x);
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
                    result.Add(entity);
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

            if (DeityCache.Any(pair => !pair.Key.discovered))
            {
                foreach (var entity in undiscoveredEntities())
                {
                    entity.discovered = true;
                    Utility.DebugReport("Change research should be called.");
                    Utility.ChangeResearchProgress(Utility.deityResearch, 0f, true);
                    var message = "Cults_DiscoveredDeityMessage".Translate(entity.Label);
                    Messages.Message(message, MessageTypeDefOf.PositiveEvent);

                    var s = new StringBuilder();
                    s.AppendLine(message);
                    s.AppendLine();
                    s.AppendLine(entity.Info());
                    Find.LetterStack.ReceiveLetter("Cults_Discovered".Translate(), s.ToString(),
                        LetterDefOf.NeutralEvent);
                    break;
                }
            }
            else
            {
                Utility.ChangeResearchProgress(Utility.deityResearch, Utility.deityResearch.baseCost);
                Utility.deityResearchDone = true;
            }
        }

        private void ReloadCosmicEntity(CosmicEntity entity)
        {
            var currentFavor = entity.PlayerFavor;
            var currentDiscovery = entity.discovered;
            //Remove entity
            DeityCache.Remove(entity);

            //New deity
            var x = new CosmicEntity(entity.def);
            x.AffectFavor(currentFavor);
            x.discovered = currentDiscovery;
            DeityCache.Add(x, x.Version);

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
                tempDic.Add(pair.Key, pair.Value);
            }

            //Now, check to see if the saved "version" matches the new "version" we loaded.

            var entitiesToUpdate = new List<CosmicEntity>();
            foreach (var pair in tempDic)
            {
                //Version mismatch, let's update!
                if (pair.Key.Version != pair.Value)
                {
                    entitiesToUpdate.Add(pair.Key);
                    //Cthulhu.Utility.DebugReport("To be updated +1");
                }

                //Cthulhu.Utility.DebugReport("Cycled");
            }


            foreach (var entity in entitiesToUpdate)
            {
                ReloadCosmicEntity(entity);
            }

            //Deities are updated, but let's check if there are new deities.
            foreach (var current in DefDatabase<ThingDef>.AllDefs)
            {
                if (current.thingClass != typeof(CosmicEntity))
                {
                    continue;
                }

                var newDeity = new CosmicEntity(current);

                if (tempDic.Keys.FirstOrDefault(oldDeity => oldDeity.def.defName == newDeity.def.defName) != null)
                {
                    continue;
                }

                newDeity.discovered = false;
                GetCache(newDeity);
                //RevealDeityCheck();
            }

            //Clear that dictionary
        }


        public override void ExposeData()
        {
            Scribe_Collections.Look(ref DeityCache, "Deities", LookMode.Deep, LookMode.Value);
            //Scribe_Collections.Look<CosmicEntity>(ref this.DeityCache, "Deities", LookMode.Deep, new object[0]);
            Scribe_Values.Look(ref AreDeitiesSpawned, "AreDeitiesSpawned");
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