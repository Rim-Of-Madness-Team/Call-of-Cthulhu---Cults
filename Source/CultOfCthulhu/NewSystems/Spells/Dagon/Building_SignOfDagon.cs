using Cthulhu;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace CultOfCthulhu
{
    public class Building_SignOfDagon : Building
    {
        public override void SpawnSetup(Map map, bool bla)
        {
            //Log.Message("Building_SignOfDagon SpawnSetup");
            base.SpawnSetup(map, bla);
            Building_SignOfDagon toDestroy = null;
            foreach (var bld in map.listerBuildings.allBuildingsColonist)
            {
                if (bld == this)
                {
                    continue;
                }

                if (bld is Building_SignOfDagon dagon)
                {
                    toDestroy = dagon;
                }
            }

            toDestroy?.Destroy();

            var list = map.GetComponent<MapComponent_SacrificeTracker>().defendTheBroodPawns;
            if (list == null)
            {
                return;
            }

            if (list.Count <= 0)
            {
                return;
            }

            Faction f;
            if (Utility.IsCosmicHorrorsLoaded())
            {
                f = Find.FactionManager.FirstFactionOfDef(FactionDef.Named("ROM_DeepOne"));
            }
            else
            {
                Messages.Message("Cosmic horrors mod is not loaded. Using insectoids instead.",
                    MessageTypeDefOf.NegativeEvent);
                f = Find.FactionManager.FirstFactionOfDef(FactionDef.Named("ROM_DeepOneAlt"));
            }

            Lord lord = null;
            //Log.Message("Building_SignOfDagon LordJob_DefendPoint");
            var lordJob = new LordJob_DefendPoint(Position);
            Utility.TemporaryGoodwill(f);
            foreach (var current in list)
            {
                if (lord == null)
                {
                    lord = current.GetLord();
                }

                if (lord != null)
                {
                    map.lordManager.RemoveLord(lord);
                }
            }

            LordMaker.MakeNewLord(f, lordJob, map, list);
        }
    }
}