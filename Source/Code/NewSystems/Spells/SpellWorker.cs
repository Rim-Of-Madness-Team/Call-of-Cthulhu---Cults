using RimWorld;
using Verse;

namespace CultOfCthulhu
{
    public class SpellWorker : IncidentWorker
    {
        public virtual bool CanSummonNow(Map map)
        {
            return true;
        }

        public virtual Building_SacrificialAltar altar(Map map)
        {
            return map.GetComponent<MapComponent_SacrificeTracker>().lastUsedAltar;
        }

        public virtual Pawn executioner(Map map)
        {
            return map.GetComponent<MapComponent_SacrificeTracker>().lastUsedAltar.SacrificeData.Executioner;
        }

        public virtual Pawn sacrifice(Map map)
        {
            return map.GetComponent<MapComponent_SacrificeTracker>().lastUsedAltar.SacrificeData.Sacrifice;
        }

        public virtual Pawn TempExecutioner(Map map)
        {
            return map.GetComponent<MapComponent_SacrificeTracker>().lastUsedAltar.tempExecutioner;
        }

        public virtual Pawn TempSacrifice(Map map)
        {
            return map.GetComponent<MapComponent_SacrificeTracker>().lastUsedAltar.tempSacrifice;
        }
    }
}