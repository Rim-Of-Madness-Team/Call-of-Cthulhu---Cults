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
                new FunSpell(newName: "Cults_SpellDoubleTheFun", newWeight: 1), // FINISHED
                new FunSpell(newName: "Cults_SpellReanimator", newWeight: 2), // FINISHED
                new FunSpell(newName: "Cults_SpellFlashstorm", newWeight: 10), // No code required
                new FunSpell(newName: "Cults_SpellEcstaticFrenzy", newWeight: 2), //FINISHED
                new FunSpell(newName: "Cults_SpellStarVampireVisit", newWeight: 5), //FINISHED
                new FunSpell(newName: "Cults_SpellBlight", newWeight: 10), // No code required
                new FunSpell(newName: "Cults_SpellNoLongerDomesticated", newWeight: 2), //FINISHED
                new FunSpell(newName: "Cults_SpellEclipse", newWeight: 10), //FINISHED
                new FunSpell(newName: "Cults_SpellFoodSpoilage", newWeight: 10), //FINISHED
                new FunSpell(newName: "Cults_SpellReincarnation", newWeight: 5), //FINISHED
                new FunSpell(newName: "Cults_SpellAuroraEffect", newWeight: 5), // FINISHED
                new FunSpell(newName: "Cults_SpellNeedAHand", newWeight: 5), // FINISHED
                new FunSpell(newName: "Cults_SpellRatsInTheWalls", newWeight: 5) // FINISHED
            };
        }

        public void RollTableOfFun(Map map)
        {
            var result = TableOfFun.RandomElementByWeight(weightSelector: GetWeight);
            if (result.defName == "Cults_SpellDoubleTheFun")
            {
                Utility.DebugReport(x: "Double The Fun!");
                DoubleTheFun(map: map);
                return;
            }

            var temp = DefDatabase<IncidentDef>.GetNamed(defName: result.defName);
            if (temp != null)
            {
                map.GetComponent<MapComponent_SacrificeTracker>().lastSideEffect = temp;
                CultUtility.CastSpell(spell: temp, map: map);
                return;
            }

            Utility.DebugReport(x: "Failed to utilize");
        }


        private void DoubleTheFun(Map map)
        {
            var result = TableOfFun.RandomElementByWeight(weightSelector: GetWeight);
            while (result.defName == "Cults_SpellDoubleTheFun")
            {
                result = TableOfFun.RandomElementByWeight(weightSelector: GetWeight);
            }

            var temp = DefDatabase<IncidentDef>.GetNamed(defName: result.defName);

            var result2 = TableOfFun.RandomElementByWeight(weightSelector: GetWeight);
            while (result2.defName == "Cults_SpellDoubleTheFun")
            {
                result2 = TableOfFun.RandomElementByWeight(weightSelector: GetWeight);
            }

            var temp2 = DefDatabase<IncidentDef>.GetNamed(defName: result2.defName);

            if (temp != null && temp2 != null)
            {
                map.GetComponent<MapComponent_SacrificeTracker>().wasDoubleTheFun = true;
                map.GetComponent<MapComponent_SacrificeTracker>().lastSideEffect = temp;
                map.GetComponent<MapComponent_SacrificeTracker>().lastDoubleSideEffect = temp2;
                CultUtility.CastSpell(spell: temp, map: map);
                CultUtility.CastSpell(spell: temp2, map: map);
                return;
            }

            Utility.DebugReport(x: "Failed to utilize " + temp);
            Utility.DebugReport(x: "Failed to utilize " + temp2);
        }

        private static float GetWeight(FunSpell spell)
        {
            return spell.weight;
        }
    }
}