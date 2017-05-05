using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace CultOfCthulhu
{
    public class FunSpell
    {
        public string defName { get; set; }
        public float weight { get; set; }


        public FunSpell(String newName, float newWeight)
        {
            defName = newName;
            weight = newWeight;
        }
    }

    public class CultTableOfFun
    {
        public List<FunSpell> TableOfFun;
        
        public CultTableOfFun()
        {
            TableOfFun = new List<FunSpell>
            {
            new FunSpell("SpellDoubleTheFun", 1), // FINISHED
            new FunSpell("SpellReanimator", 2), // FINISHED
            new FunSpell("SpellFlashstorm", 10), // No code required
            new FunSpell("SpellEcstaticFrenzy", 2), //FINISHED
            new FunSpell("SpellStarVampireVisit", 5), //FINISHED
            new FunSpell("SpellBlight", 10), // No code required
            new FunSpell("SpellNoLongerDomesticated", 2), //FINISHED
            new FunSpell("SpellEclipse", 10), //FINISHED
            new FunSpell("SpellFoodSpoilage", 10), //FINISHED
            new FunSpell("SpellReincarnation", 5), //FINISHED
            new FunSpell("SpellAuroraEffect", 5), // FINISHED
            new FunSpell("SpellNeedAHand", 5), // FINISHED
            new FunSpell("SpellRatsInTheWalls", 5) // FINISHED
            };
        }
        
        public void RollTableOfFun(Map map)
        {
            FunSpell result = GenCollection.RandomElementByWeight<FunSpell>(TableOfFun, GetWeight);
            if (result.defName == "SpellDoubleTheFun")
            {
                Cthulhu.Utility.DebugReport("Double The Fun!");
                DoubleTheFun(map);
                return;
            }
            IncidentDef temp = DefDatabase<IncidentDef>.GetNamed(result.defName);
            if (temp != null)
            {
                map.GetComponent<MapComponent_SacrificeTracker>().lastSideEffect = temp;
                CultUtility.CastSpell(temp, map);
                return;
            }
            Cthulhu.Utility.DebugReport("Failed to utilize " + temp.ToString());
        }


        private void DoubleTheFun(Map map)
        {
            FunSpell result = GenCollection.RandomElementByWeight<FunSpell>(TableOfFun, GetWeight);
            while (result.defName == "SpellDoubleTheFun")
            {
                result = GenCollection.RandomElementByWeight<FunSpell>(TableOfFun, GetWeight);
            }
            IncidentDef temp = DefDatabase<IncidentDef>.GetNamed(result.defName);

            FunSpell result2 = GenCollection.RandomElementByWeight<FunSpell>(TableOfFun, GetWeight);
            while (result2.defName == "SpellDoubleTheFun")
            {
                result2 = GenCollection.RandomElementByWeight<FunSpell>(TableOfFun, GetWeight);
            }
            IncidentDef temp2 = DefDatabase<IncidentDef>.GetNamed(result2.defName);

            if (temp != null && temp2 != null)
            {
                map.GetComponent<MapComponent_SacrificeTracker>().wasDoubleTheFun = true;
                map.GetComponent<MapComponent_SacrificeTracker>().lastSideEffect = temp;
                map.GetComponent<MapComponent_SacrificeTracker>().lastDoubleSideEffect = temp2;
                CultUtility.CastSpell(temp, map);
                CultUtility.CastSpell(temp2, map);
                return;
            }
            Cthulhu.Utility.DebugReport("Failed to utilize " + temp.ToString());
            Cthulhu.Utility.DebugReport("Failed to utilize " + temp2.ToString());
        }

        private static float GetWeight(FunSpell spell)
        {
            return spell.weight;
        }
    }
}
