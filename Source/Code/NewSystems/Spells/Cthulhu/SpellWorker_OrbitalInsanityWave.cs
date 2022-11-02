// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Cthulhu;
using RimWorld;
using Verse;

// ----------------------------------------------------------------------
// These are RimWorld-specific usings. Activate/Deactivate what you need:
// ----------------------------------------------------------------------
// Always needed
//using VerseBase;         // Material/Graphics handling functions are found here
// RimWorld universal objects are here (like 'Building')
// Needed when you do something with the AI
// Needed when you do something with Sound
// Needed when you do something with Noises
// RimWorld specific functions are found here (like 'Building_Battery')

// RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

namespace CultOfCthulhu
{
    public class SpellWorker_OrbitalInsanityWave : SpellWorker
    {
        private const float FogClearRadius = 4.5f;

        private const float RelationWithColonistWeight = 20f;

        public List<Thing> ThingsToAdd(ThingDef def, int count)
        {
            var tempList = new List<Thing>();
            if (count == 0)
            {
                return tempList;
            }

            for (var i = 0; i <= count; i++)
            {
                var thingWithComps = (ThingWithComps) ThingMaker.MakeThing(def: def);
                tempList.Add(item: thingWithComps);
            }

            return tempList;
        }

        public override bool CanSummonNow(Map map)
        {
            return true;
        }

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            //Cthulhu.Utility.DebugReport("CanFire: " + this.def.defName);
            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var map = parms.target as Map;
            float chance = Rand.Range(min: 1, max: 100);
            var container = new List<Thing>();
            string label;
            string text;


            //Get a random cell.
            var intVec = DropCellFinder.RandomDropSpot(map: map);
            //Set the faction of the dude.
            var unused = Find.FactionManager.FirstFactionOfDef(facDef: FactionDefOf.Ancients
            );
            ////Chance of generating soldiers
            //for (int i = 0; i < (int)Rand.Range(0, 2); i++)
            //{
            //    PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.SpaceSoldier, faction, PawnGenerationContext.NonPlayer, map, false, false, false, false, true, true, 20f, false, true, true, null, null, null, null, null, null);
            //    Pawn pawn = PawnGenerator.GeneratePawn(request);
            //    Cthulhu.Utility.ApplySanityLoss(pawn, 1.0f);
            //    container.TryAdd(pawn);
            //}
            ////Chance of generating survivors. One is required to start the pod!
            //for (int i = 0; i < (int)Rand.Range(1, 2); i++)
            //{
            //    PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.SpaceRefugee, faction, PawnGenerationContext.NonPlayer, map, false, false, false, false, true, false, 20f, false, true, true, null, null, null, null, null, null);
            //    Pawn pawn = PawnGenerator.GeneratePawn(request);
            //    Cthulhu.Utility.ApplySanityLoss(pawn, 1.0f);
            //    container.TryAdd(pawn);
            //}
            //Chance of generating downed survivors.
            //for (int i = 0; i < (int)Rand.Range(0, 2); i++)
            //{
            //    PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.SpaceRefugee, faction, PawnGenerationContext.NonPlayer, map, false, false, false, false, true, false, 20f, false, true, true, null, null, null, null, null, null);
            //    Pawn pawn = PawnGenerator.GeneratePawn(request);
            //    Cthulhu.Utility.ApplySanityLoss(pawn, 1.0f);
            //    HealthUtility.GiveInjuriesToForceDowned(pawn);
            //    container.TryAdd(pawn);
            //}
            //Chance of generating dead bodies
            //for (int i = 0; i < (int)Rand.Range(0, 3); i++)
            //{
            //    PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.SpaceRefugee, faction, PawnGenerationContext.NonPlayer, false, false, false, false, true, false, 20f, false, true, true, null, null, null, null, null, null);
            //    Pawn pawn = PawnGenerator.GeneratePawn(request);
            //    HealthUtility.GiveInjuriesToKill(pawn);
            //    container.TryAdd(pawn);
            // }
            //What kind of trade ship was it?

            //Combat Supplier
            if (chance > 66)
            {
                label = "LetterLabelForcedCrashCombatSuplier".Translate();
                text = "ForcedCrashCombatSuplier".Translate();

                container.AddRange(collection: ThingsToAdd(def: ThingDefOf.Silver, count: Rand.Range(min: 40, max: 60))); //Orig 4000-6000
                container.AddRange(collection: ThingsToAdd(def: ThingDefOf.ComponentSpacer, count: Rand.Range(min: -1, max: 10))); //Original -1~10
                container.AddRange(collection: ThingsToAdd(def: ThingDefOf.Shell_HighExplosive, count: Rand.Range(min: 5, max: 10))); //Original 30-60
                container.AddRange(collection: ThingsToAdd(def: ThingDefOf.MedicineUltratech, count: Rand.Range(min: 1, max: 3))); //Original 30-50
                AddThingsToContainerByTag(container: container, tag: "BodyPartOrImplant", counter: Rand.Range(min: -8, max: 2)); //Original 0~5
                AddThingsToContainerByTag(container: container, tag: "Drugs", counter: Rand.Range(min: -2, max: 2)); //Original 0~5
                var randomInRange = Rand.Range(min: 3, max: 6); //Orig 4~8 guns
                //Weapons Ranged
                for (var i = 0; i < randomInRange; i++)
                {
                    if (!(from t in DefDatabase<ThingDef>.AllDefs
                        where t.IsRangedWeapon && t.tradeability != Tradeability.None &&
                              t.techLevel <= TechLevel.Archotech && t.BaseMarketValue <= 500
                        select t).TryRandomElement(result: out var thingDef))
                    {
                        break;
                    }

                    ThingDef stuff = null;
                    if (thingDef.MadeFromStuff)
                    {
                        stuff = (from st in DefDatabase<ThingDef>.AllDefs
                            where st.IsStuff && st.stuffProps.CanMake(t: thingDef)
                            select st).RandomElementByWeight(weightSelector: st => st.stuffProps.commonality);
                    }

                    var thingWithComps = (ThingWithComps) ThingMaker.MakeThing(def: thingDef, stuff: stuff);
                    container.Add(item: thingWithComps);
                }

                //Weapons Melee
                randomInRange = Rand.Range(min: 1, max: 3); //Orig 3~5 guns
                for (var i = 0; i < randomInRange; i++)
                {
                    if (!(from t in DefDatabase<ThingDef>.AllDefs
                        where t.IsRangedWeapon && t.tradeability != Tradeability.None &&
                              t.techLevel <= TechLevel.Archotech
                        select t).TryRandomElement(result: out var thingDef))
                    {
                        break;
                    }

                    ThingDef stuff = null;
                    if (thingDef.MadeFromStuff)
                    {
                        stuff = (from st in DefDatabase<ThingDef>.AllDefs
                            where st.IsStuff && st.stuffProps.CanMake(t: thingDef)
                            select st).RandomElementByWeight(weightSelector: st => st.stuffProps.commonality);
                    }

                    var thingWithComps = (ThingWithComps) ThingMaker.MakeThing(def: thingDef, stuff: stuff);
                    container.Add(item: thingWithComps);
                }

                //Armor
                randomInRange = Rand.Range(min: 1, max: 2); //Orig 2~4 armor
                for (var i = 0; i < randomInRange; i++)
                {
                    if (!(from t in DefDatabase<ThingDef>.AllDefs
                        where t.IsApparel && t.tradeability != Tradeability.None &&
                              t.techLevel <= TechLevel.Archotech &&
                              (t.GetStatValueAbstract(stat: StatDefOf.ArmorRating_Blunt) > 0.15f ||
                               t.GetStatValueAbstract(stat: StatDefOf.ArmorRating_Sharp) > 0.15f)
                        select t).TryRandomElement(result: out var thingDef))
                    {
                        break;
                    }

                    ThingDef stuff = null;
                    if (thingDef.MadeFromStuff)
                    {
                        stuff = (from st in DefDatabase<ThingDef>.AllDefs
                            where st.IsStuff && st.stuffProps.CanMake(t: thingDef)
                            select st).RandomElementByWeight(weightSelector: st => st.stuffProps.commonality);
                    }

                    var thingWithComps = (ThingWithComps) ThingMaker.MakeThing(def: thingDef, stuff: stuff);
                    container.Add(item: thingWithComps);
                }

                //Clothes
                randomInRange = Rand.Range(min: 1, max: 2); //Orig 4~8 clothes
                for (var i = 0; i < randomInRange; i++)
                {
                    if (!(from t in DefDatabase<ThingDef>.AllDefs
                        where t.IsApparel && t.tradeability != Tradeability.None && t.techLevel <= TechLevel.Archotech
                        select t).TryRandomElement(result: out var thingDef))
                    {
                        break;
                    }

                    ThingDef stuff = null;
                    if (thingDef.MadeFromStuff)
                    {
                        stuff = (from st in DefDatabase<ThingDef>.AllDefs
                            where st.IsStuff && st.stuffProps.CanMake(t: thingDef)
                            select st).RandomElementByWeight(weightSelector: st => st.stuffProps.commonality);
                    }

                    var thingWithComps = (ThingWithComps) ThingMaker.MakeThing(def: thingDef, stuff: stuff);
                    container.Add(item: thingWithComps);
                }
            }

            else if (chance > 33 && chance <= 66)
            {
                label = "LetterLabelForcedCrashBulkGoods".Translate();
                text = "ForcedCrashBulkGoods".Translate();

                //Basic Stuff
                container.AddRange(collection: ThingsToAdd(def: ThingDefOf.Silver, count: Rand.Range(min: 20, max: 100))); //Original 4000-6000
                container.AddRange(collection: ThingsToAdd(def: ThingDefOf.ComponentIndustrial, count: Rand.Range(min: 5, max: 15))); //Original 5-30
                container.AddRange(collection: ThingsToAdd(def: ThingDefOf.ComponentSpacer, count: Rand.Range(min: -5, max: 5))); //Original 5-30
                container.AddRange(collection: ThingsToAdd(def: ThingDefOf.Steel, count: Rand.Range(min: 20, max: 100))); //Original 800-1500

                //Luxury Goods
                container.AddRange(collection: ThingsToAdd(def: ThingDefOf.Gold, count: Rand.Range(min: 5, max: 50))); //Original 500-2000
                container.AddRange(collection: ThingsToAdd(def: CultsDefOf.Neutroamine, count: Rand.Range(min: 5, max: 15))); //Original 400-800
                container.AddRange(collection: ThingsToAdd(def: ThingDefOf.Plasteel, count: Rand.Range(min: 5, max: 15))); //Original 300-700
                container.AddRange(collection: ThingsToAdd(def: ThingDefOf.Beer, count: Rand.Range(min: -70, max: 30))); //Original -700-2000
                container.AddRange(collection: ThingsToAdd(def: ThingDefOf.Chocolate, count: Rand.Range(min: -70, max: 30))); //Original -700-2000
                AddThingsToContainerByTag(container: container, tag: "Furniture", counter: Rand.Range(min: 0, max: 1)); //(0-3 kinds) Furniture -1~2

                //Sensitive Materials
                container.AddRange(collection: ThingsToAdd(def: ThingDefOf.Cloth, count: Rand.Range(min: -20, max: 50))); //Original -200-600
                container.AddRange(collection: ThingsToAdd(def: ThingDefOf.MedicineIndustrial, count: Rand.Range(min: 1, max: 5))); //Original 10-30
                container.AddRange(collection: ThingsToAdd(def: ThingDefOf.MedicineUltratech, count: Rand.Range(min: -10, max: 5))); //Original 10-30
                AddThingsToContainerByTag(container: container, tag: "Apparel", counter: Rand.Range(min: 4, max: 8)); //Original 10-20
                container.AddRange(collection: ThingsToAdd(def: ThingDefOf.WoodLog, count: Rand.Range(min: 10, max: 60))); //Original 800-1500

                //Foodstuffs
                container.AddRange(collection: ThingsToAdd(def: ThingDef.Named(defName: "Pemmican"), count: Rand.Range(min: -20, max: 40))); //NA
                container.AddRange(collection: ThingsToAdd(def: ThingDef.Named(defName: "Kibble"), count: Rand.Range(min: -20, max: 40))); //NA
                //Food meals 2-4
                //AddThingsToContainerByTag(container, "", Rand.Range(0, 1)); //ResourcesRaw 1500 - 3000
                //AddThingsToContainerByTag(container, "", Rand.Range(0, 1)); //(2-8 kinds) FoodRaw 1600 - 5000
                //Animals Original - 2-4 types, 10-20 number, max wildness 0.70
                AddThingsToContainerByTag(container: container, tag: "Drugs", counter: Rand.Range(min: 2, max: 4)); //Original 2-4 types, max price of 8000

                //Textiles - Original 2200 - 4000.
                //New Range - 270 - 1100
                container.AddRange(collection: ThingsToAdd(def: CultsDefOf.BlocksSlate, count: Rand.Range(min: -100, max: 200)));
                container.AddRange(collection: ThingsToAdd(def: CultsDefOf.BlocksLimestone, count: Rand.Range(min: -100, max: 200)));
                container.AddRange(collection: ThingsToAdd(def: CultsDefOf.BlocksMarble, count: Rand.Range(min: -100, max: 200)));
                container.AddRange(collection: ThingsToAdd(def: ThingDef.Named(defName: "BlocksGranite"), count: Rand.Range(min: -100, max: 200)));
                container.AddRange(collection: ThingsToAdd(def: ThingDefOf.Plasteel, count: Rand.Range(min: -20, max: 20)));
                container.AddRange(collection: ThingsToAdd(def: ThingDefOf.Steel, count: Rand.Range(min: -100, max: 200)));
            }
            //Exotic
            else
            {
                label = "LetterLabelForcedCrashExotic".Translate();
                text = "ForcedCrashExotic".Translate();

                container.AddRange(collection: ThingsToAdd(def: ThingDefOf.Silver, count: Rand.Range(min: 50, max: 100))); //Original 1500-3000
                container.AddRange(collection: ThingsToAdd(def: ThingDefOf.ComponentIndustrial, count: Rand.Range(min: 6, max: 20))); //Original 6-20
                container.AddRange(collection: ThingsToAdd(def: ThingDefOf.ComponentSpacer, count: Rand.Range(min: -20, max: 5))); //Original 6-20
                container.AddRange(collection: ThingsToAdd(def: ThingDefOf.Plasteel, count: Rand.Range(min: 10, max: 30))); //Original 50-150
                container.AddRange(collection: ThingsToAdd(def: ThingDefOf.Gold, count: Rand.Range(min: 10, max: 20))); //Original 100-300
                container.AddRange(collection: ThingsToAdd(def: CultsDefOf.Neutroamine, count: Rand.Range(min: 5, max: 20))); //Original 25-100
                container.AddRange(collection: ThingsToAdd(def: CultsDefOf.Penoxycyline, count: Rand.Range(min: -25, max: 25))); //Original (0)
                container.AddRange(collection: ThingsToAdd(def: ThingDef.Named(defName: "Telescope"), count: Rand.Range(min: -3, max: 2))); //Original -2 - 2
                //AddThingsToContainerByTag(container, "Television", Rand.Range(-2, 2)); //Original -2~2
                AddThingsToContainerByTag(container: container, tag: "BodyPartOrImplant", counter: Rand.Range(min: 1, max: 2)); //Original 2~4
                //AddThingsToContainerByTag(container, "StandardAnimal", Rand.Range(1, 2));  //Animals - Original 1-3 kinds, 2-6 number. Wildness 0.6 
                AddThingsToContainerByTag(container: container, tag: "Furniture",
                    counter: Rand.Range(min: 0, max: 3)); //Furniture - Original 0-3 kinds, -1-3 number
                AddThingsToContainerByTag(container: container, tag: "Apparel", counter: Rand.Range(min: 1, max: 2)); //Original 1-2, 3-4 duplicates
                AddThingsToContainerByTag(container: container, tag: "Artifact", counter: Rand.Range(min: -2, max: 1)); //Original 1-1
                AddThingsToContainerByTag(container: container, tag: "Drugs", counter: Rand.Range(min: 2, max: 4)); //Original 2-4
                AddThingsToContainerByTag(container: container, tag: "Exotic", counter: Rand.Range(min: 1, max: 2)); //Original 2-4 kinds, 1-2
                //Art not included, due to crash.
            }


            //Misc ship crash pieces.
            container.AddRange(collection: ThingsToAdd(def: ThingDefOf.Steel, count: Rand.Range(min: 20, max: 100)));
            container.AddRange(collection: ThingsToAdd(def: ThingDefOf.ComponentIndustrial, count: Rand.Range(min: 10, max: 20)));
            container.AddRange(collection: ThingsToAdd(def: ThingDefOf.ComponentSpacer, count: Rand.Range(min: -20, max: 5)));
            container.AddRange(collection: ThingsToAdd(def: ThingDefOf.Steel, count: Rand.Range(min: 20, max: 100)));

            //PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref label, pawn);

            Find.LetterStack.ReceiveLetter(label: label, text: text, textLetterDef: LetterDefOf.PositiveEvent, lookTargets: new TargetInfo(cell: intVec, map: map));
            var adpInfo = new ActiveDropPodInfo();
            foreach (var thing in container)
            {
                adpInfo.innerContainer.TryAdd(item: thing);
            }

            DropPodUtility.MakeDropPodAt(c: intVec, map: map, info: adpInfo);
            Utility.ApplyTaleDef(defName: "Cults_SpellOrbitalInsanityWave", map: map);
            return true;
        }

        public void AddThingsToContainerByTag(List<Thing> container, string tag, int counter)
        {
            if (counter <= 0)
            {
                return;
            }

            for (var i = 0; i < counter; i++)
            {
                if (!(from t in DefDatabase<ThingDef>.AllDefs
                    where t.tradeTags != null && t.tradeTags.Contains(item: tag)
                    select t).TryRandomElement(result: out var thingDef))
                {
                    break;
                }

                ThingDef stuff = null;
                if (thingDef.MadeFromStuff)
                {
                    stuff = (from st in DefDatabase<ThingDef>.AllDefs
                        where st.IsStuff && st.stuffProps.CanMake(t: thingDef)
                        select st).RandomElementByWeight(weightSelector: st => st.stuffProps.commonality);
                }

                var thingWithComps = (ThingWithComps) ThingMaker.MakeThing(def: thingDef, stuff: stuff);
                container.Add(item: thingWithComps);
            }
        }
    }
}