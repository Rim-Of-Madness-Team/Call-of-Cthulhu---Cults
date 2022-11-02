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
            base.SpawnSetup(map: map, respawningAfterLoad: bla);
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
                f = Find.FactionManager.FirstFactionOfDef(facDef: FactionDef.Named(defName: "ROM_DeepOne"));
            }
            else
            {
                Messages.Message(text: "Cosmic horrors mod is not loaded. Using insectoids instead.",
                    def: MessageTypeDefOf.NegativeEvent);
                f = Find.FactionManager.FirstFactionOfDef(facDef: FactionDef.Named(defName: "ROM_DeepOneAlt"));
            }

            Lord lord = null;
            //Log.Message("Building_SignOfDagon LordJob_DefendPoint");
            var lordJob = new LordJob_DefendPoint(point: Position);
            Utility.TemporaryGoodwill(faction: f);
            foreach (var current in list)
            {
                if (lord == null)
                {
                    lord = current.GetLord();
                }

                if (lord != null)
                {
                    map.lordManager.RemoveLord(oldLord: lord);
                }
            }

            LordMaker.MakeNewLord(faction: f, lordJob: lordJob, map: map, startingPawns: list);
        }
    }
}