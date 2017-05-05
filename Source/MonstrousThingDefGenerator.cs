using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Verse;

namespace CultOfCthulhu
{
    [StaticConstructorOnStartup]
    public static class MonstrousThingDefGenerator
    {
        static MonstrousThingDefGenerator()
        {
            IEnumerable<ThingDef> enumerable = MonstrousThingDefGenerator.ImpliedMonstrousDefs();
            foreach (ThingDef current in enumerable)
            {
                current.PostLoad();
                DefDatabase<ThingDef>.Add(current);
            }
            CrossRefLoader.ResolveAllWantedCrossReferences(FailMode.Silent);
        }

        public static IEnumerable<ThingDef> ImpliedMonstrousDefs()
        {
            List<ThingDef>.Enumerator enumerator = DefDatabase<ThingDef>.AllDefs.ToList<ThingDef>().GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    ThingDef current = enumerator.Current;
                            if (current.category == ThingCategory.Pawn)
                            {
                                if (current.race != null)
                                {
                                    if (current.race.Animal)
                                    {
                                ThingDef newDef = new ThingDef();
                                newDef = GenerateMonstrousDef(current);
                                        yield return newDef;
                                    }
                                }
                            }
                }
            }
            finally
            {
            }
            yield break;
        }

        public static ThingDef GenerateMonstrousDef(ThingDef oldDef)
        {
            ThingDef newDef = new ThingDef();
            try
            {
                #region baseproperties
                //Copy every base thing

                //newDef.Verbs = oldDef.Verbs;
                //Pawn_MeleeVerbs tempVerbs = oldDef.Verbs;
                newDef.tradeTags = new List<string>();
                //Cthulhu.Utility.DebugReport("trade tags");

                newDef.altitudeLayer = oldDef.altitudeLayer;
                newDef.category = oldDef.category;
                newDef.thingClass = oldDef.thingClass;
                newDef.selectable = oldDef.selectable;
                newDef.tickerType = TickerType.Normal;
                newDef.useHitPoints = oldDef.useHitPoints;
                newDef.hasTooltip = oldDef.hasTooltip;
                newDef.alwaysHaulable = oldDef.alwaysHaulable;
                newDef.socialPropernessMatters = oldDef.socialPropernessMatters;
                newDef.pathCost = oldDef.pathCost;
                newDef.tradeability = oldDef.tradeability;
                newDef.soundImpactDefault = oldDef.soundImpactDefault;
                newDef.inspectorTabs = new List<Type>();
                foreach (Type tab in oldDef.inspectorTabs)
                {
                    newDef.inspectorTabs.Add(tab);
                    //Cthulhu.Utility.DebugReport("Added " + tab.ToString());
                }
                newDef.inspectorTabsResolved = new List<InspectTabBase>();
                foreach (InspectTabBase tab in oldDef.inspectorTabsResolved)
                {
                    newDef.inspectorTabsResolved.Add(tab);
                }
                newDef.comps = new List<CompProperties>();
                foreach (CompProperties comp in oldDef.comps)
                {
                    newDef.comps.Add(comp);
                }
                newDef.drawGUIOverlay = oldDef.drawGUIOverlay;
                #endregion baseproperties


                //Copy every animal base thing
                newDef.statBases = new List<StatModifier>();
                foreach (StatModifier mod in oldDef.statBases)
                {
                    newDef.statBases.Add(mod);
                }
                newDef.race = oldDef.race;

                newDef.recipes = new List<RecipeDef>();
                foreach (RecipeDef recipe in oldDef.recipes)
                {
                    newDef.recipes.Add(recipe);
                }

                foreach (string s in oldDef.tradeTags)
                {
                    newDef.tradeTags.Add(s);
                }
                if (newDef.thingCategories == null)
                {
                    newDef.thingCategories = new List<ThingCategoryDef>();
                }

                string oldName = oldDef.defName;
                string newName = Regex.Replace(oldName, "[0-9]", "");
                if (!newName.Contains("Monstrous")) newName = newName + "_Monstrous";
                Cthulhu.Utility.DebugReport(oldName);
                Cthulhu.Utility.DebugReport(newName);
                newDef.defName = oldDef.defName + "_Monstrous";
                newDef.label = "Monstrous " + oldDef.label;
                newDef.description = oldDef.description;

                CrossRefLoader.RegisterListWantsCrossRef<ThingCategoryDef>(newDef.thingCategories, "Animal");


            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
            return newDef;
        }

    }
}
