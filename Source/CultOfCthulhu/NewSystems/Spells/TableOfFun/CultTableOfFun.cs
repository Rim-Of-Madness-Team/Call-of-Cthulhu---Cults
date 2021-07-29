using System.Collections.Generic;
using Cthulhu;
using RimWorld;
using Verse;

namespace CultOfCthulhu
{
    public class FunSpell
    {
        public FunSpell(string newName, float newWeight)
        {
            defName = newName;
            weight = newWeight;
        }

        public string defName { get; set; }
        public float weight { get; set; }
    }

    public class CultTableOfFun
    {
        public List<FunSpell> TableOfFun;

        public CultTableOfFun()
        {
            TableOfFun = new List<FunSpell>
            {
                new FunSpell("Cults_SpellDoubleTheFun", 1), // FINISHED
                new FunSpell("Cults_SpellReanimator", 2), // FINISHED
                new FunSpell("Cults_SpellFlashstorm", 10), // No code required
                new FunSpell("Cults_SpellEcstaticFrenzy", 2), //FINISHED
                new FunSpell("Cults_SpellStarVampireVisit", 5), //FINISHED
                new FunSpell("Cults_SpellBlight", 10), // No code required
                new FunSpell("Cults_SpellNoLongerDomesticated", 2), //FINISHED
                new FunSpell("Cults_SpellEclipse", 10), //FINISHED
                new FunSpell("Cults_SpellFoodSpoilage", 10), //FINISHED
                new FunSpell("Cults_SpellReincarnation", 5), //FINISHED
                new FunSpell("Cults_SpellAuroraEffect", 5), // FINISHED
                new FunSpell("Cults_SpellNeedAHand", 5), // FINISHED
                new FunSpell("Cults_SpellRatsInTheWalls", 5) // FINISHED
            };
        }

        public void RollTableOfFun(Map map)
        {
            var result = TableOfFun.RandomElementByWeight(GetWeight);
            if (result.defName == "Cults_SpellDoubleTheFun")
            {
                Utility.DebugReport("Double The Fun!");
                DoubleTheFun(map);
                return;
            }

            var temp = DefDatabase<IncidentDef>.GetNamed(result.defName);
            if (temp != null)
            {
                map.GetComponent<MapComponent_SacrificeTracker>().lastSideEffect = temp;
                CultUtility.CastSpell(temp, map);
                return;
            }

            Utility.DebugReport("Failed to utilize");
        }


        private void DoubleTheFun(Map map)
        {
            var result = TableOfFun.RandomElementByWeight(GetWeight);
            while (result.defName == "Cults_SpellDoubleTheFun")
            {
                result = TableOfFun.RandomElementByWeight(GetWeight);
            }

            var temp = DefDatabase<IncidentDef>.GetNamed(result.defName);

            var result2 = TableOfFun.RandomElementByWeight(GetWeight);
            while (result2.defName == "Cults_SpellDoubleTheFun")
            {
                result2 = TableOfFun.RandomElementByWeight(GetWeight);
            }

            var temp2 = DefDatabase<IncidentDef>.GetNamed(result2.defName);

            if (temp != null && temp2 != null)
            {
                map.GetComponent<MapComponent_SacrificeTracker>().wasDoubleTheFun = true;
                map.GetComponent<MapComponent_SacrificeTracker>().lastSideEffect = temp;
                map.GetComponent<MapComponent_SacrificeTracker>().lastDoubleSideEffect = temp2;
                CultUtility.CastSpell(temp, map);
                CultUtility.CastSpell(temp2, map);
                return;
            }

            Utility.DebugReport("Failed to utilize " + temp);
            Utility.DebugReport("Failed to utilize " + temp2);
        }

        private static float GetWeight(FunSpell spell)
        {
            return spell.weight;
        }
    }
}