﻿// ----------------------------------------------------------------------
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
                var thingWithComps = (ThingWithComps) ThingMaker.MakeThing(def);
                tempList.Add(thingWithComps);
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
            float chance = Rand.Range(1, 100);
            var container = new List<Thing>();
            string label;
            string text;


            //Get a random cell.
            var intVec = DropCellFinder.RandomDropSpot(map);
            //Set the faction of the dude.
            var unused = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Ancients
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

                container.AddRange(ThingsToAdd(ThingDefOf.Silver, Rand.Range(40, 60))); //Orig 4000-6000
                container.AddRange(ThingsToAdd(ThingDefOf.ComponentSpacer, Rand.Range(-1, 10))); //Original -1~10
                container.AddRange(ThingsToAdd(ThingDefOf.Shell_HighExplosive, Rand.Range(5, 10))); //Original 30-60
                container.AddRange(ThingsToAdd(ThingDefOf.MedicineUltratech, Rand.Range(1, 3))); //Original 30-50
                AddThingsToContainerByTag(container, "BodyPartOrImplant", Rand.Range(-8, 2)); //Original 0~5
                AddThingsToContainerByTag(container, "Drugs", Rand.Range(-2, 2)); //Original 0~5
                var randomInRange = Rand.Range(3, 6); //Orig 4~8 guns
                //Weapons Ranged
                for (var i = 0; i < randomInRange; i++)
                {
                    if (!(from t in DefDatabase<ThingDef>.AllDefs
                        where t.IsRangedWeapon && t.tradeability != Tradeability.None &&
                              t.techLevel <= TechLevel.Archotech && t.BaseMarketValue <= 500
                        select t).TryRandomElement(out var thingDef))
                    {
                        break;
                    }

                    ThingDef stuff = null;
                    if (thingDef.MadeFromStuff)
                    {
                        stuff = (from st in DefDatabase<ThingDef>.AllDefs
                            where st.IsStuff && st.stuffProps.CanMake(thingDef)
                            select st).RandomElementByWeight(st => st.stuffProps.commonality);
                    }

                    var thingWithComps = (ThingWithComps) ThingMaker.MakeThing(thingDef, stuff);
                    container.Add(thingWithComps);
                }

                //Weapons Melee
                randomInRange = Rand.Range(1, 3); //Orig 3~5 guns
                for (var i = 0; i < randomInRange; i++)
                {
                    if (!(from t in DefDatabase<ThingDef>.AllDefs
                        where t.IsRangedWeapon && t.tradeability != Tradeability.None &&
                              t.techLevel <= TechLevel.Archotech
                        select t).TryRandomElement(out var thingDef))
                    {
                        break;
                    }

                    ThingDef stuff = null;
                    if (thingDef.MadeFromStuff)
                    {
                        stuff = (from st in DefDatabase<ThingDef>.AllDefs
                            where st.IsStuff && st.stuffProps.CanMake(thingDef)
                            select st).RandomElementByWeight(st => st.stuffProps.commonality);
                    }

                    var thingWithComps = (ThingWithComps) ThingMaker.MakeThing(thingDef, stuff);
                    container.Add(thingWithComps);
                }

                //Armor
                randomInRange = Rand.Range(1, 2); //Orig 2~4 armor
                for (var i = 0; i < randomInRange; i++)
                {
                    if (!(from t in DefDatabase<ThingDef>.AllDefs
                        where t.IsApparel && t.tradeability != Tradeability.None &&
                              t.techLevel <= TechLevel.Archotech &&
                              (t.GetStatValueAbstract(StatDefOf.ArmorRating_Blunt) > 0.15f ||
                               t.GetStatValueAbstract(StatDefOf.ArmorRating_Sharp) > 0.15f)
                        select t).TryRandomElement(out var thingDef))
                    {
                        break;
                    }

                    ThingDef stuff = null;
                    if (thingDef.MadeFromStuff)
                    {
                        stuff = (from st in DefDatabase<ThingDef>.AllDefs
                            where st.IsStuff && st.stuffProps.CanMake(thingDef)
                            select st).RandomElementByWeight(st => st.stuffProps.commonality);
                    }

                    var thingWithComps = (ThingWithComps) ThingMaker.MakeThing(thingDef, stuff);
                    container.Add(thingWithComps);
                }

                //Clothes
                randomInRange = Rand.Range(1, 2); //Orig 4~8 clothes
                for (var i = 0; i < randomInRange; i++)
                {
                    if (!(from t in DefDatabase<ThingDef>.AllDefs
                        where t.IsApparel && t.tradeability != Tradeability.None && t.techLevel <= TechLevel.Archotech
                        select t).TryRandomElement(out var thingDef))
                    {
                        break;
                    }

                    ThingDef stuff = null;
                    if (thingDef.MadeFromStuff)
                    {
                        stuff = (from st in DefDatabase<ThingDef>.AllDefs
                            where st.IsStuff && st.stuffProps.CanMake(thingDef)
                            select st).RandomElementByWeight(st => st.stuffProps.commonality);
                    }

                    var thingWithComps = (ThingWithComps) ThingMaker.MakeThing(thingDef, stuff);
                    container.Add(thingWithComps);
                }
            }

            else if (chance > 33 && chance <= 66)
            {
                label = "LetterLabelForcedCrashBulkGoods".Translate();
                text = "ForcedCrashBulkGoods".Translate();

                //Basic Stuff
                container.AddRange(ThingsToAdd(ThingDefOf.Silver, Rand.Range(20, 100))); //Original 4000-6000
                container.AddRange(ThingsToAdd(ThingDefOf.ComponentIndustrial, Rand.Range(5, 15))); //Original 5-30
                container.AddRange(ThingsToAdd(ThingDefOf.ComponentSpacer, Rand.Range(-5, 5))); //Original 5-30
                container.AddRange(ThingsToAdd(ThingDefOf.Steel, Rand.Range(20, 100))); //Original 800-1500

                //Luxury Goods
                container.AddRange(ThingsToAdd(ThingDefOf.Gold, Rand.Range(5, 50))); //Original 500-2000
                container.AddRange(ThingsToAdd(CultsDefOf.Neutroamine, Rand.Range(5, 15))); //Original 400-800
                container.AddRange(ThingsToAdd(ThingDefOf.Plasteel, Rand.Range(5, 15))); //Original 300-700
                container.AddRange(ThingsToAdd(ThingDefOf.Beer, Rand.Range(-70, 30))); //Original -700-2000
                container.AddRange(ThingsToAdd(ThingDefOf.Chocolate, Rand.Range(-70, 30))); //Original -700-2000
                AddThingsToContainerByTag(container, "Furniture", Rand.Range(0, 1)); //(0-3 kinds) Furniture -1~2

                //Sensitive Materials
                container.AddRange(ThingsToAdd(ThingDefOf.Cloth, Rand.Range(-20, 50))); //Original -200-600
                container.AddRange(ThingsToAdd(ThingDefOf.MedicineIndustrial, Rand.Range(1, 5))); //Original 10-30
                container.AddRange(ThingsToAdd(ThingDefOf.MedicineUltratech, Rand.Range(-10, 5))); //Original 10-30
                AddThingsToContainerByTag(container, "Apparel", Rand.Range(4, 8)); //Original 10-20
                container.AddRange(ThingsToAdd(ThingDefOf.WoodLog, Rand.Range(10, 60))); //Original 800-1500

                //Foodstuffs
                container.AddRange(ThingsToAdd(ThingDef.Named("Pemmican"), Rand.Range(-20, 40))); //NA
                container.AddRange(ThingsToAdd(ThingDef.Named("Kibble"), Rand.Range(-20, 40))); //NA
                //Food meals 2-4
                //AddThingsToContainerByTag(container, "", Rand.Range(0, 1)); //ResourcesRaw 1500 - 3000
                //AddThingsToContainerByTag(container, "", Rand.Range(0, 1)); //(2-8 kinds) FoodRaw 1600 - 5000
                //Animals Original - 2-4 types, 10-20 number, max wildness 0.70
                AddThingsToContainerByTag(container, "Drugs", Rand.Range(2, 4)); //Original 2-4 types, max price of 8000

                //Textiles - Original 2200 - 4000.
                //New Range - 270 - 1100
                container.AddRange(ThingsToAdd(CultsDefOf.BlocksSlate, Rand.Range(-100, 200)));
                container.AddRange(ThingsToAdd(CultsDefOf.BlocksLimestone, Rand.Range(-100, 200)));
                container.AddRange(ThingsToAdd(CultsDefOf.BlocksMarble, Rand.Range(-100, 200)));
                container.AddRange(ThingsToAdd(ThingDef.Named("BlocksGranite"), Rand.Range(-100, 200)));
                container.AddRange(ThingsToAdd(ThingDefOf.Plasteel, Rand.Range(-20, 20)));
                container.AddRange(ThingsToAdd(ThingDefOf.Steel, Rand.Range(-100, 200)));
            }
            //Exotic
            else
            {
                label = "LetterLabelForcedCrashExotic".Translate();
                text = "ForcedCrashExotic".Translate();

                container.AddRange(ThingsToAdd(ThingDefOf.Silver, Rand.Range(50, 100))); //Original 1500-3000
                container.AddRange(ThingsToAdd(ThingDefOf.ComponentIndustrial, Rand.Range(6, 20))); //Original 6-20
                container.AddRange(ThingsToAdd(ThingDefOf.ComponentSpacer, Rand.Range(-20, 5))); //Original 6-20
                container.AddRange(ThingsToAdd(ThingDefOf.Plasteel, Rand.Range(10, 30))); //Original 50-150
                container.AddRange(ThingsToAdd(ThingDefOf.Gold, Rand.Range(10, 20))); //Original 100-300
                container.AddRange(ThingsToAdd(CultsDefOf.Neutroamine, Rand.Range(5, 20))); //Original 25-100
                container.AddRange(ThingsToAdd(CultsDefOf.Penoxycyline, Rand.Range(-25, 25))); //Original (0)
                container.AddRange(ThingsToAdd(ThingDef.Named("Telescope"), Rand.Range(-3, 2))); //Original -2 - 2
                //AddThingsToContainerByTag(container, "Television", Rand.Range(-2, 2)); //Original -2~2
                AddThingsToContainerByTag(container, "BodyPartOrImplant", Rand.Range(1, 2)); //Original 2~4
                //AddThingsToContainerByTag(container, "StandardAnimal", Rand.Range(1, 2));  //Animals - Original 1-3 kinds, 2-6 number. Wildness 0.6 
                AddThingsToContainerByTag(container, "Furniture",
                    Rand.Range(0, 3)); //Furniture - Original 0-3 kinds, -1-3 number
                AddThingsToContainerByTag(container, "Apparel", Rand.Range(1, 2)); //Original 1-2, 3-4 duplicates
                AddThingsToContainerByTag(container, "Artifact", Rand.Range(-2, 1)); //Original 1-1
                AddThingsToContainerByTag(container, "Drugs", Rand.Range(2, 4)); //Original 2-4
                AddThingsToContainerByTag(container, "Exotic", Rand.Range(1, 2)); //Original 2-4 kinds, 1-2
                //Art not included, due to crash.
            }


            //Misc ship crash pieces.
            container.AddRange(ThingsToAdd(ThingDefOf.Steel, Rand.Range(20, 100)));
            container.AddRange(ThingsToAdd(ThingDefOf.ComponentIndustrial, Rand.Range(10, 20)));
            container.AddRange(ThingsToAdd(ThingDefOf.ComponentSpacer, Rand.Range(-20, 5)));
            container.AddRange(ThingsToAdd(ThingDefOf.Steel, Rand.Range(20, 100)));

            //PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref label, pawn);

            Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.PositiveEvent, new TargetInfo(intVec, map));
            var adpInfo = new ActiveDropPodInfo();
            foreach (var thing in container)
            {
                adpInfo.innerContainer.TryAdd(thing);
            }

            DropPodUtility.MakeDropPodAt(intVec, map, adpInfo);
            Utility.ApplyTaleDef("Cults_SpellOrbitalInsanityWave", map);
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
                    where t.tradeTags != null && t.tradeTags.Contains(tag)
                    select t).TryRandomElement(out var thingDef))
                {
                    break;
                }

                ThingDef stuff = null;
                if (thingDef.MadeFromStuff)
                {
                    stuff = (from st in DefDatabase<ThingDef>.AllDefs
                        where st.IsStuff && st.stuffProps.CanMake(thingDef)
                        select st).RandomElementByWeight(st => st.stuffProps.commonality);
                }

                var thingWithComps = (ThingWithComps) ThingMaker.MakeThing(thingDef, stuff);
                container.Add(thingWithComps);
            }
        }
    }
}