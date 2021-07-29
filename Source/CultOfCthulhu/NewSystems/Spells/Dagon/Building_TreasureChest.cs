using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

//using System.Diagnostics;

namespace CultOfCthulhu
{
    public class Building_TreasureChest : Building, IOpenable, IThingHolder, IStoreSettingsParent
    {
        protected bool contentsKnown;
        protected ThingOwner innerContainer;

        protected bool SpawnedStorage;

        protected StorageSettings storageSettings;

        public Building_TreasureChest()
        {
            innerContainer = new ThingOwner<Thing>(this, false);
        }

        public bool HasAnyContents => innerContainer.Count > 0;

        public Thing ContainedThing => innerContainer.Count != 0 ? innerContainer[0] : null;

        public bool CanOpen => HasAnyContents;

        public virtual void Open()
        {
            if (!HasAnyContents)
            {
                return;
            }

            EjectContents();
        }

        public bool StorageTabVisible => false;

        public int OpenTicks => 300;
        

        //public virtual IThingHolder ParentHolder
        //{
        //    get
        //    {
        //        return base.ParentHolder;
        //    }
        //}

        public StorageSettings GetStoreSettings()
        {
            return storageSettings;
        }

        public StorageSettings GetParentStoreSettings()
        {
            return def.building.fixedStorageSettings;
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }

            //foreach (Gizmo g2 in StorageSettingsClipboard.CopyPasteGizmosFor(this.storageSettings))
            //{
            //    yield return g2;
            //}
        }

        public override void PostMake()
        {
            base.PostMake();
            //this.innerContainer = new ThingOwner<Thing>(this, false);
            storageSettings = new StorageSettings(this);
            if (def.building.defaultStorageSettings != null)
            {
                storageSettings.CopyFrom(def.building.defaultStorageSettings);
            }

            if (SpawnedStorage)
            {
                return;
            }

            SpawnedStorage = true;
            if (def == CultsDefOf.Cults_TreasureChest)
            {
                for (var i = 0; i < 5; i++)
                {
                    var thing1 = ThingMaker.MakeThing(ThingDefOf.Gold);
                    thing1.stackCount = Rand.Range(20, 40);
                    GetDirectlyHeldThings().TryAdd(thing1);

                    var thing2 = ThingMaker.MakeThing(ThingDefOf.Silver);
                    thing2.stackCount = Rand.Range(40, 60);
                    GetDirectlyHeldThings().TryAdd(thing2);

                    var thing3 = ThingMaker.MakeThing(ThingDef.Named("Jade"));
                    thing3.stackCount = Rand.Range(10, 40);
                    GetDirectlyHeldThings().TryAdd(thing3);
                }

                if (Rand.Value > 0.8f)
                {
                    var thing4 = ThingMaker.MakeThing(ThingDef.Named("SculptureSmall"), ThingDefOf.Gold);
                    thing4.stackCount = 1;
                    GetDirectlyHeldThings().TryAdd(thing4);
                }
            }

            if (def != CultsDefOf.Cults_TreasureChest_Relic)
            {
                return;
            }

            GetDirectlyHeldThings()
                .TryAdd(Rand.Range(1, 100) > 50 ? GenerateLegendaryWeapon() : GenerateLegendaryArmor());
        }

        //Selects a random weapon type and improves it to a legendary status
        public ThingWithComps GenerateLegendaryWeapon()
        {
            if (!(from td in DefDatabase<ThingDef>.AllDefs
                where HandlesWeaponDefs(td)
                select td).TryRandomElement(out var thingDef))
            {
                return null;
            }

            var thingWithComps = (ThingWithComps) ThingMaker.MakeThing(thingDef);
            var compQuality = thingWithComps.TryGetComp<CompQuality>();
            compQuality.SetQuality(QualityCategory.Legendary, ArtGenerationContext.Outsider);
            return thingWithComps;
        }

        //Industrial Level Legendary Weapons
        public bool HandlesWeaponDefs(ThingDef thingDef)
        {
            return thingDef.IsRangedWeapon && thingDef.tradeability != Tradeability.None &&
                   thingDef.techLevel <= TechLevel.Industrial;
        }

        //Same as weapon generation code
        public ThingWithComps GenerateLegendaryArmor()
        {
            if (!(from td in DefDatabase<ThingDef>.AllDefs
                where HandlesArmorDefs(td)
                select td).TryRandomElement(out var thingDef))
            {
                return null;
            }

            var thingWithComps = (ThingWithComps) ThingMaker.MakeThing(thingDef);
            thingWithComps.stackCount = 1;
            var compQuality = thingWithComps.TryGetComp<CompQuality>();
            compQuality.SetQuality(QualityCategory.Legendary, ArtGenerationContext.Outsider);
            return thingWithComps;
        }

        //Industrial Level Legendary Armor
        private bool HandlesArmorDefs(ThingDef td)
        {
            return td == ThingDefOf.Apparel_ShieldBelt || td.tradeability != Tradeability.None &&
                td.techLevel <= TechLevel.Industrial && td.IsApparel &&
                (td.GetStatValueAbstract(StatDefOf.ArmorRating_Blunt) > 0.15f ||
                 td.GetStatValueAbstract(StatDefOf.ArmorRating_Sharp) > 0.15f);
        }

        public override void TickRare()
        {
            base.TickRare();
            innerContainer.ThingOwnerTickRare();
        }

        public override void Tick()
        {
            base.Tick();
            innerContainer.ThingOwnerTick();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
            Scribe_Values.Look(ref contentsKnown, "contentsKnown");
            Scribe_Deep.Look(ref storageSettings, "storageSettings", this);
            Scribe_Values.Look(ref SpawnedStorage, "SpawnedStorage");
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (Faction != null && Faction.IsPlayer)
            {
                contentsKnown = true;
            }
        }

        public override bool ClaimableBy(Faction fac)
        {
            if (!innerContainer.Any)
            {
                return base.ClaimableBy(fac);
            }

            foreach (var thing in innerContainer)
            {
                if (thing.Faction == fac)
                {
                    return true;
                }
            }

            return false;
        }

        public virtual bool Accepts(Thing thing)
        {
            return innerContainer.Count < 10 && innerContainer.CanAcceptAnyOf(thing);
        }

        public virtual bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
        {
            if (!Accepts(thing))
            {
                return false;
            }

            bool flag;
            if (thing.holdingOwner != null)
            {
                thing.holdingOwner.TryTransferToContainer(thing, innerContainer, thing.stackCount);
                flag = true;
            }
            else
            {
                flag = innerContainer.TryAdd(thing);
            }

            if (!flag)
            {
                return false;
            }

            if (thing.Faction != null && thing.Faction.IsPlayer)
            {
                contentsKnown = true;
            }

            return true;
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (innerContainer.Count > 0 && (mode == DestroyMode.Deconstruct || mode == DestroyMode.KillFinalize))
            {
                if (mode != DestroyMode.Deconstruct)
                {
                    var list = new List<Pawn>();
                    foreach (var current in innerContainer)
                    {
                        if (current is Pawn pawn)
                        {
                            list.Add(pawn);
                        }
                    }

                    foreach (var current2 in list)
                    {
                        HealthUtility.DamageUntilDowned(current2);
                    }
                }

                EjectContents();
            }

            innerContainer.ClearAndDestroyContents();
            base.Destroy(mode);
        }

        public virtual void EjectContents()
        {
            innerContainer.TryDropAll(InteractionCell, Map, ThingPlaceMode.Near);
            contentsKnown = true;
        }

        public override string GetInspectString()
        {
            var text = base.GetInspectString();
            string str;
            if (!contentsKnown)
            {
                str = "UnknownLower".Translate();
            }
            else
            {
                str = innerContainer.ContentsString;
            }

            if (!text.NullOrEmpty())
            {
                text += "\n";
            }

            return text + "CasketContains".Translate() + ": " + str;
        }
    }
}