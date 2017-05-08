using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
//using System.Diagnostics;
using Verse;

namespace CultOfCthulhu
{
    public class Building_TreasureChest : Building_Casket, IStoreSettingsParent, IThingHolder
    {
        private StorageSettings storageSettings;

        private bool SpawnedStorage = false;

        public Pawn assignedPawn;

        public bool StorageTabVisible
        {
            get
            {
                return true;
            }
        }

        public StorageSettings GetStoreSettings()
        {
            return this.storageSettings;
        }

        public StorageSettings GetParentStoreSettings()
        {
            return this.def.building.fixedStorageSettings;
        }

        public override void PostMake()
        {
            base.PostMake();
            this.innerContainer = new ThingOwner<Thing>(this, false);
            this.storageSettings = new StorageSettings(this);
            if (this.def.building.defaultStorageSettings != null)
            {
                this.storageSettings.CopyFrom(this.def.building.defaultStorageSettings);
            }
            if (SpawnedStorage == false)
            {
                SpawnedStorage = true;
                if (this.def.defName == "TreasureChest")
                {
                    for (int i = 0; i < 5; i++)
                    {
                        Thing thing1 = ThingMaker.MakeThing(ThingDef.Named("Gold"), null);
                        thing1.stackCount = Rand.Range(20, 40);
                        this.innerContainer.TryAdd(thing1);

                        Thing thing2 = ThingMaker.MakeThing(ThingDef.Named("Silver"), null);
                        thing2.stackCount = Rand.Range(40, 60);
                        this.innerContainer.TryAdd(thing2);

                        Thing thing3 = ThingMaker.MakeThing(ThingDef.Named("Jade"), null);
                        thing3.stackCount = Rand.Range(10, 40);
                        this.innerContainer.TryAdd(thing3);
                    }
                    if (Rand.Value > 0.8f)
                    {
                        Thing thing4 = ThingMaker.MakeThing(ThingDef.Named("SculptureSmall"), ThingDefOf.Gold);
                        this.innerContainer.TryAdd(thing4);
                    }
                }
                if (this.def.defName == "TreasureChest_Relic")
                {
                    if (Rand.Range(1, 100) > 50) this.innerContainer.TryAdd(GenerateLegendaryWeapon());
                    else this.innerContainer.TryAdd(GenerateLegendaryArmor());
                }
            }
        }

        //Selects a random weapon type and improves it to a legendary status
        public ThingWithComps GenerateLegendaryWeapon()
        {
            ThingDef def;
            if (!(from td in DefDatabase<ThingDef>.AllDefs
                    where this.HandlesWeaponDefs(td)
                    select td).TryRandomElement(out def))
            {
                return null;
            }
            ThingWithComps thingWithComps = (ThingWithComps)ThingMaker.MakeThing(def, null);
            CompQuality compQuality = thingWithComps.TryGetComp<CompQuality>();
            compQuality.SetQuality(QualityCategory.Legendary, ArtGenerationContext.Outsider);
            return thingWithComps;
        }
        
        //Industrial Level Legendary Weapons
        public bool HandlesWeaponDefs(ThingDef thingDef)
        {
            return thingDef.IsRangedWeapon && thingDef.tradeability == Tradeability.Stockable && thingDef.techLevel <= TechLevel.Industrial;
        }

        //Same as weapon generation code
        public ThingWithComps GenerateLegendaryArmor()
        {
            ThingDef def;
            if (!(from td in DefDatabase<ThingDef>.AllDefs
                  where this.HandlesArmorDefs(td)
                  select td).TryRandomElement(out def))
            {
                return null;
            }
            ThingWithComps thingWithComps = (ThingWithComps)ThingMaker.MakeThing(def, null);
            CompQuality compQuality = thingWithComps.TryGetComp<CompQuality>();
            compQuality.SetQuality(QualityCategory.Legendary, ArtGenerationContext.Outsider);
            return thingWithComps;
        }

        //Industrial Level Legendary Armor
        private bool HandlesArmorDefs(ThingDef td)
        {
            return td == ThingDefOf.Apparel_ShieldBelt || (td.tradeability == Tradeability.Stockable && td.techLevel <= TechLevel.Industrial && td.IsApparel && (td.GetStatValueAbstract(StatDefOf.ArmorRating_Blunt, null) > 0.15f || td.GetStatValueAbstract(StatDefOf.ArmorRating_Sharp, null) > 0.15f));
        }

        public override void TickRare()
        {
            base.TickRare();
            this.innerContainer.ThingOwnerTickRare();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.SpawnedStorage, "SpawnedStorage", false);
            Scribe_Deep.Look<StorageSettings>(ref this.storageSettings, "storageSettings", new object[]
            {
                this
            });
        }

        public override void EjectContents()
        {
            base.EjectContents();
            Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things);
        }

        public override bool Accepts(Thing thing)
        {
            bool flag = !base.Accepts(thing);
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                bool flag2 = !this.storageSettings.AllowedToAccept(thing);
                result = !flag2;
            }
            return result;
        }

        public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
        {
            bool flag = base.TryAcceptThing(thing, allowSpecialEffects);
            bool result;
            if (flag)
            {
                Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things);
                result = true;
            }
            else
            {
                result = false;
            }
            return result;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            return base.GetGizmos();
        }
    }
}
