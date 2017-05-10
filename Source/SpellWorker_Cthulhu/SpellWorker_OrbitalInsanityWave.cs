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
    public class SpellWorker_OrbitalInsanityWave : SpellWorker
    {
        private const float FogClearRadius = 4.5f;

        private const float RelationWithColonistWeight = 20f;

        public List<Thing> ThingsToAdd(ThingDef def, int count)
        {

            List<Thing> tempList = new List<Thing>();
            if (count == 0) return tempList;

            for (int i = 0; i <= count; i++)
            {
                ThingWithComps thingWithComps = (ThingWithComps)ThingMaker.MakeThing(def, null);
                tempList.Add(thingWithComps);
            }
            return tempList;
        }
        public override bool CanSummonNow(Map map)
        {
            return true;
        }

        protected override bool CanFireNowSub(IIncidentTarget target)
        {

            //Cthulhu.Utility.DebugReport("CanFire: " + this.def.defName);
            return true;
        }

        public override bool TryExecute(IncidentParms parms)
        {

            Map map = parms.target as Map;
            float chance = Rand.Range(1, 100);
            List<Thing> container = new List<Thing>();
            string label = "LetterLabelRefugeePodCrash".Translate();
            string text = "RefugeePodCrash".Translate();


            //Get a random cell.
            IntVec3 intVec = DropCellFinder.RandomDropSpot(map);
            //Set the faction of the dude.
            Faction faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Spacer
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
                container.AddRange(ThingsToAdd(ThingDefOf.Component, Rand.Range(-1, 10))); //Original -1~10
                container.AddRange(ThingsToAdd(ThingDefOf.MortarShell, Rand.Range(5, 10))); //Original 30-60
                container.AddRange(ThingsToAdd(ThingDefOf.Medicine, Rand.Range(5, 15))); //Original 30-50
                AddThingsToContainerByTag(container, "BodyPartOrImplant", Rand.Range(-8, 2)); //Original 0~5
                AddThingsToContainerByTag(container, "Drugs", Rand.Range(-2, 2)); //Original 0~5
                int randomInRange = Rand.Range(3, 6); //Orig 4~8 guns
                //Weapons Ranged
                for (int i = 0; i < randomInRange; i++)
                {
                    ThingDef def;
                    if (!(from t in DefDatabase<ThingDef>.AllDefs
                          where t.IsRangedWeapon && t.tradeability == Tradeability.Stockable && t.techLevel <= TechLevel.Transcendent && t.BaseMarketValue <= 500
                          select t).TryRandomElement(out def))
                    {
                        break;
                    }
                    ThingDef stuff = null;
                    if (def.MadeFromStuff)
                    {
                        stuff = (from st in DefDatabase<ThingDef>.AllDefs
                                 where st.IsStuff && st.stuffProps.CanMake(def)
                                 select st).RandomElementByWeight((ThingDef st) => st.stuffProps.commonality);
                    }
                    ThingWithComps thingWithComps = (ThingWithComps)ThingMaker.MakeThing(def, stuff);
                    container.Add(thingWithComps);
                }
                //Weapons Melee
                randomInRange = Rand.Range(1, 3); //Orig 3~5 guns
                for (int i = 0; i < randomInRange; i++)
                {
                    ThingDef def;
                    if (!(from t in DefDatabase<ThingDef>.AllDefs
                          where t.IsRangedWeapon && t.tradeability == Tradeability.Stockable && t.techLevel <= TechLevel.Transcendent
                          select t).TryRandomElement(out def))
                    {
                        break;
                    }
                    ThingDef stuff = null;
                    if (def.MadeFromStuff)
                    {
                        stuff = (from st in DefDatabase<ThingDef>.AllDefs
                                 where st.IsStuff && st.stuffProps.CanMake(def)
                                 select st).RandomElementByWeight((ThingDef st) => st.stuffProps.commonality);
                    }
                    ThingWithComps thingWithComps = (ThingWithComps)ThingMaker.MakeThing(def, stuff);
                    container.Add(thingWithComps);
                }
                //Armor
                randomInRange = Rand.Range(1, 2); //Orig 2~4 armor
                for (int i = 0; i < randomInRange; i++)
                {
                    ThingDef def;
                    if (!(from t in DefDatabase<ThingDef>.AllDefs
                          where t.IsApparel && t.tradeability == Tradeability.Stockable && t.techLevel <= TechLevel.Transcendent && (t.GetStatValueAbstract(StatDefOf.ArmorRating_Blunt, null) > 0.15f || t.GetStatValueAbstract(StatDefOf.ArmorRating_Sharp, null) > 0.15f)
                          select t).TryRandomElement(out def))
                    {
                        break;
                    }
                    ThingDef stuff = null;
                    if (def.MadeFromStuff)
                    {
                        stuff = (from st in DefDatabase<ThingDef>.AllDefs
                                 where st.IsStuff && st.stuffProps.CanMake(def)
                                 select st).RandomElementByWeight((ThingDef st) => st.stuffProps.commonality);
                    }
                    ThingWithComps thingWithComps = (ThingWithComps)ThingMaker.MakeThing(def, stuff);
                    container.Add(thingWithComps);
                }

                    //Clothes
                    randomInRange = Rand.Range(1, 2); //Orig 4~8 clothes
                for (int i = 0; i < randomInRange; i++)
                {
                    ThingDef def;
                    if (!(from t in DefDatabase<ThingDef>.AllDefs
                          where t.IsApparel && t.tradeability == Tradeability.Stockable && t.techLevel <= TechLevel.Transcendent
                          select t).TryRandomElement(out def))
                    {
                        break;
                    }
                    ThingDef stuff = null;
                    if (def.MadeFromStuff)
                    {
                        stuff = (from st in DefDatabase<ThingDef>.AllDefs
                                 where st.IsStuff && st.stuffProps.CanMake(def)
                                 select st).RandomElementByWeight((ThingDef st) => st.stuffProps.commonality);
                    }
                    ThingWithComps thingWithComps = (ThingWithComps)ThingMaker.MakeThing(def, stuff);
                    container.Add(thingWithComps);
                }
            }

            else if (chance > 33 && chance <= 66)
            {
                label = "LetterLabelForcedCrashBulkGoods".Translate();
                text = "ForcedCrashBulkGoods".Translate();

                //Basic Stuff
                container.AddRange(ThingsToAdd(ThingDefOf.Silver, Rand.Range(20, 100))); //Original 4000-6000
                container.AddRange(ThingsToAdd(ThingDefOf.Component, Rand.Range(5, 15))); //Original 5-30
                container.AddRange(ThingsToAdd(ThingDefOf.Steel, Rand.Range(20, 100))); //Original 800-1500

                //Luxury Goods
                container.AddRange(ThingsToAdd(ThingDefOf.Gold, Rand.Range(5, 50))); //Original 500-2000
                container.AddRange(ThingsToAdd(CultsDefOfs.Neutroamine, Rand.Range(5, 15))); //Original 400-800
                container.AddRange(ThingsToAdd(ThingDefOf.Plasteel, Rand.Range(5, 15))); //Original 300-700
                container.AddRange(ThingsToAdd(ThingDefOf.Beer, Rand.Range(-70, 30))); //Original -700-2000
                container.AddRange(ThingsToAdd(ThingDefOf.Chocolate, Rand.Range(-70, 30))); //Original -700-2000
                AddThingsToContainerByTag(container, "Furniture", Rand.Range(0, 1)); //(0-3 kinds) Furniture -1~2

                //Sensitive Materials
                container.AddRange(ThingsToAdd(ThingDefOf.Cloth, Rand.Range(-20, 50))); //Original -200-600
                container.AddRange(ThingsToAdd(ThingDefOf.Medicine, Rand.Range(1, 5))); //Original 10-30
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
                container.AddRange(ThingsToAdd(CultsDefOfs.BlocksSlate, Rand.Range(-100, 200)));
                container.AddRange(ThingsToAdd(CultsDefOfs.BlocksLimestone, Rand.Range(-100, 200)));
                container.AddRange(ThingsToAdd(CultsDefOfs.BlocksMarble, Rand.Range(-100, 200)));
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
                container.AddRange(ThingsToAdd(ThingDefOf.Component, Rand.Range(6, 20))); //Original 6-20
                container.AddRange(ThingsToAdd(ThingDefOf.Plasteel, Rand.Range(10, 30))); //Original 50-150
                container.AddRange(ThingsToAdd(ThingDefOf.Gold, Rand.Range(10, 20))); //Original 100-300
                container.AddRange(ThingsToAdd(CultsDefOfs.Neutroamine, Rand.Range(5, 20))); //Original 25-100
                container.AddRange(ThingsToAdd(CultsDefOfs.Penoxycyline, Rand.Range(-25, 25))); //Original (0)
                container.AddRange(ThingsToAdd(ThingDefOf.GlitterworldMedicine, Rand.Range(-10, 4))); //Original -5 - 4
                container.AddRange(ThingsToAdd(ThingDef.Named("Telescope"), Rand.Range(-3, 2))); //Original -2 - 2
                //AddThingsToContainerByTag(container, "Television", Rand.Range(-2, 2)); //Original -2~2
                AddThingsToContainerByTag(container, "BodyPartOrImplant", Rand.Range(1, 2)); //Original 2~4
                //AddThingsToContainerByTag(container, "StandardAnimal", Rand.Range(1, 2));  //Animals - Original 1-3 kinds, 2-6 number. Wildness 0.6 
                AddThingsToContainerByTag(container, "Furniture", Rand.Range(0, 3)); //Furniture - Original 0-3 kinds, -1-3 number
                AddThingsToContainerByTag(container, "Apparel", Rand.Range(1, 2)); //Original 1-2, 3-4 duplicates
                AddThingsToContainerByTag(container, "Artifact", Rand.Range(-2, 1)); //Original 1-1
                AddThingsToContainerByTag(container, "Drugs", Rand.Range(2, 4)); //Original 2-4
                AddThingsToContainerByTag(container, "Exotic", Rand.Range(1, 2)); //Original 2-4 kinds, 1-2
                //Art not included, due to crash.

            }


            //Misc ship crash pieces.
            container.AddRange(ThingsToAdd(ThingDefOf.Steel, Rand.Range(20, 100)));
            container.AddRange(ThingsToAdd(ThingDefOf.Component, Rand.Range(10, 20)));
            container.AddRange(ThingsToAdd(ThingDefOf.Steel, Rand.Range(20, 100)));

            //PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref label, pawn);

            Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.Good, new TargetInfo(intVec, map), null);
            ActiveDropPodInfo adpInfo = new ActiveDropPodInfo();
            foreach (Thing thing in container)
            {
                adpInfo.innerContainer.TryAdd(thing);
            }
            DropPodUtility.MakeDropPodAt(intVec, map, adpInfo);
            Cthulhu.Utility.ApplyTaleDef("Cults_SpellOrbitalInsanityWave", map);
            return true;
        }

        public void AddThingsToContainerByTag(List<Thing> container, string tag, int counter)
        {
            if (counter <= 0) return;
            for (int i = 0; i < counter; i++)
            {
                ThingDef def;
                if (!(from t in DefDatabase<ThingDef>.AllDefs
                      where t.tradeTags != null && t.tradeTags.Contains(tag)
                      select t).TryRandomElement(out def))
                {
                    break;
                }
                ThingDef stuff = null;
                if (def.MadeFromStuff)
                {
                    stuff = (from st in DefDatabase<ThingDef>.AllDefs
                             where st.IsStuff && st.stuffProps.CanMake(def)
                             select st).RandomElementByWeight((ThingDef st) => st.stuffProps.commonality);
                }
                ThingWithComps thingWithComps = (ThingWithComps)ThingMaker.MakeThing(def, stuff);
                container.Add(thingWithComps);
            }
        }

    }
}
