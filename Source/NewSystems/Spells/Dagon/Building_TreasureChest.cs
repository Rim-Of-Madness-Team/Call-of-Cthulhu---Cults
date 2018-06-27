using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
//using System.Diagnostics;
using Verse;

namespace CultOfCthulhu
{
    public class Building_TreasureChest : Building, IOpenable, IThingHolder, IStoreSettingsParent
    {
        protected ThingOwner innerContainer;

        protected StorageSettings storageSettings;

        protected bool contentsKnown;

        protected bool SpawnedStorage = false;

        public bool HasAnyContents
        {
            get
            {
                return this.innerContainer.Count > 0;
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
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
            this.storageSettings = new StorageSettings(this);
            if (this.def.building.defaultStorageSettings != null)
            {
                this.storageSettings.CopyFrom(this.def.building.defaultStorageSettings);
            }
            if (SpawnedStorage == false)
            {
                SpawnedStorage = true;
                if (this.def == CultsDefOf.Cults_TreasureChest)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        Thing thing1 = ThingMaker.MakeThing(ThingDefOf.Gold, null);
                        thing1.stackCount = Rand.Range(20, 40);
                        this.GetDirectlyHeldThings().TryAdd(thing1);

                        Thing thing2 = ThingMaker.MakeThing(ThingDefOf.Silver, null);
                        thing2.stackCount = Rand.Range(40, 60);
                        this.GetDirectlyHeldThings().TryAdd(thing2);

                        Thing thing3 = ThingMaker.MakeThing(ThingDef.Named("Jade"), null);
                        thing3.stackCount = Rand.Range(10, 40);
                        this.GetDirectlyHeldThings().TryAdd(thing3);
                    }
                    if (Rand.Value > 0.8f)
                    {
                        Thing thing4 = ThingMaker.MakeThing(ThingDef.Named("SculptureSmall"), ThingDefOf.Gold);
                        thing4.stackCount = 1;
                        this.GetDirectlyHeldThings().TryAdd(thing4);
                    }
                }
                if (this.def == CultsDefOf.Cults_TreasureChest_Relic)
                {
                    if (Rand.Range(1, 100) > 50) this.GetDirectlyHeldThings().TryAdd(GenerateLegendaryWeapon());
                    else this.GetDirectlyHeldThings().TryAdd(GenerateLegendaryArmor());
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
            return thingDef.IsRangedWeapon && thingDef.tradeability != Tradeability.None && thingDef.techLevel <= TechLevel.Industrial;
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
            thingWithComps.stackCount = 1;
            CompQuality compQuality = thingWithComps.TryGetComp<CompQuality>();
            compQuality.SetQuality(QualityCategory.Legendary, ArtGenerationContext.Outsider);
            return thingWithComps;
        }

        //Industrial Level Legendary Armor
        private bool HandlesArmorDefs(ThingDef td)
        {
            return td == ThingDefOf.Apparel_ShieldBelt || (td.tradeability != Tradeability.None && td.techLevel <= TechLevel.Industrial && td.IsApparel && (td.GetStatValueAbstract(StatDefOf.ArmorRating_Blunt, null) > 0.15f || td.GetStatValueAbstract(StatDefOf.ArmorRating_Sharp, null) > 0.15f));
        }

        public Thing ContainedThing
        {
            get
            {
                return (this.innerContainer.Count != 0) ? this.innerContainer[0] : null;
            }
        }

        public bool CanOpen
        {
            get
            {
                return this.HasAnyContents;
            }
        }

        public bool StorageTabVisible
        {
            get
            {
                return false;
            }
        }

        public Building_TreasureChest()
        {
            this.innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return this.innerContainer;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        public override void TickRare()
        {
            base.TickRare();
            this.innerContainer.ThingOwnerTickRare(true);
        }

        public override void Tick()
        {
            base.Tick();
            this.innerContainer.ThingOwnerTick(true);
        }

        public virtual void Open()
        {
            if (!this.HasAnyContents)
            {
                return;
            }
            this.EjectContents();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look<ThingOwner>(ref this.innerContainer, "innerContainer", new object[]
            {
                this
            });
            Scribe_Values.Look<bool>(ref this.contentsKnown, "contentsKnown", false, false);
            Scribe_Deep.Look<StorageSettings>(ref this.storageSettings, "storageSettings", new object[]
                {
                    this
                });
            Scribe_Values.Look<bool>(ref this.SpawnedStorage, "SpawnedStorage", false);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (base.Faction != null && base.Faction.IsPlayer)
            {
                this.contentsKnown = true;
            }
        }

        public override bool ClaimableBy(Faction fac)
        {
            if (this.innerContainer.Any)
            {
                for (int i = 0; i < this.innerContainer.Count; i++)
                {
                    if (this.innerContainer[i].Faction == fac)
                    {
                        return true;
                    }
                }
                return false;
            }
            return base.ClaimableBy(fac);
        }

        public virtual bool Accepts(Thing thing)
        {
            return this.innerContainer.Count < 10 && this.innerContainer.CanAcceptAnyOf(thing, true);
        }

        public virtual bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
        {
            if (!this.Accepts(thing))
            {
                return false;
            }
            bool flag;
            if (thing.holdingOwner != null)
            {
                thing.holdingOwner.TryTransferToContainer(thing, this.innerContainer, thing.stackCount, true);
                flag = true;
            }
            else
            {
                flag = this.innerContainer.TryAdd(thing, true);
            }
            if (flag)
            {
                if (thing.Faction != null && thing.Faction.IsPlayer)
                {
                    this.contentsKnown = true;
                }
                return true;
            }
            return false;
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (this.innerContainer.Count > 0 && (mode == DestroyMode.Deconstruct || mode == DestroyMode.KillFinalize))
            {
                if (mode != DestroyMode.Deconstruct)
                {
                    List<Pawn> list = new List<Pawn>();
                    foreach (Thing current in ((IEnumerable<Thing>)this.innerContainer))
                    {
                        Pawn pawn = current as Pawn;
                        if (pawn != null)
                        {
                            list.Add(pawn);
                        }
                    }
                    foreach (Pawn current2 in list)
                    {
                        HealthUtility.DamageUntilDowned(current2);
                    }
                }
                this.EjectContents();
            }
            this.innerContainer.ClearAndDestroyContents(DestroyMode.Vanish);
            base.Destroy(mode);
        }

        public virtual void EjectContents()
        {
            this.innerContainer.TryDropAll(this.InteractionCell, base.Map, ThingPlaceMode.Near);
            this.contentsKnown = true;
        }

        public override string GetInspectString()
        {
            string text = base.GetInspectString();
            string str;
            if (!this.contentsKnown)
            {
                str = "UnknownLower".Translate();
            }
            else
            {
                str = this.innerContainer.ContentsString;
            }
            if (!text.NullOrEmpty())
            {
                text += "\n";
            }
            return text + "CasketContains".Translate() + ": " + str;
        }

        //public virtual IThingHolder ParentHolder
        //{
        //    get
        //    {
        //        return base.ParentHolder;
        //    }
        //}

        public StorageSettings GetStoreSettings()
        {
            return this.storageSettings;
        }

        public StorageSettings GetParentStoreSettings()
        {
            return this.def.building.fixedStorageSettings;
        }
    }
}
