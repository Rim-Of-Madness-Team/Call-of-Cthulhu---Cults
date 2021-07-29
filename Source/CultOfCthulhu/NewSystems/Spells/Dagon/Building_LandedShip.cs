using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cthulhu;
using RimWorld;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace CultOfCthulhu
{
    internal class Building_LandedShip : Building
    {
        private static readonly HashSet<IntVec3> reachableCells = new HashSet<IntVec3>();

        protected int age;

        private Lord lord;

        public float pointsLeft = 300f;

        public override void SpawnSetup(Map map, bool bla)
        {
            base.SpawnSetup(map, bla);
            TrySpawnMadSailors();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref pointsLeft, "pointsLeft");
            Scribe_Values.Look(ref age, "age");
            Scribe_References.Look(ref lord, "defenseLord");
        }

        public override string GetInspectString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(base.GetInspectString());
            stringBuilder.AppendLine("AwokeDaysAgo".Translate(
                age.TicksToDays().ToString("F1")
            ));
            return stringBuilder.ToString();
        }

        public override void Tick()
        {
            base.Tick();
            age++;
        }

        private void TrySpawnMadSailors()
        {
            var lordList = new List<Pawn>();
            var faction = Find.FactionManager.FirstFactionOfDef(CultsDefOf.Cults_Sailors);
            Utility.DebugReport(faction.ToString());
            //Log.Message("Building_LandedShip LordJob_DefendPoint");
            var lordJob = new LordJob_DefendPoint(Position);
            if (pointsLeft <= 0f)
            {
                return;
            }

            if (lord == null)
            {
                lord = LordMaker.MakeNewLord(faction, lordJob, Map, lordList);
            }

            while (pointsLeft > 0f)
            {
                if (!(from cell in GenAdj.CellsAdjacent8Way(this)
                    where cell.Walkable(Map)
                    select cell).TryRandomElement(out var center))
                {
                    continue;
                }

                var request = new PawnGenerationRequest(CultsDefOf.Cults_Sailor, faction,
                    PawnGenerationContext.NonPlayer, Map.Tile, false, false, false, false, true, true, 20f, false,
                    true, true, false, false, false, false, false, 0, 0, null, 0);
                var pawn = PawnGenerator.GeneratePawn(request);
                if (!GenPlace.TryPlaceThing(pawn, center, Map, ThingPlaceMode.Near))
                {
                    continue;
                }

                if (pawn.GetLord() != null)
                {
                    pawn.GetLord().Cleanup();
                    pawn.GetLord().CurLordToil.Cleanup();
                    pawn.GetLord().LordJob.Cleanup();
                }

                lord.AddPawn(pawn);
                pointsLeft -= pawn.kindDef.combatPower;
                Utility.ApplySanityLoss(pawn, 1f);

                //Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
            }

            pointsLeft = 0f;
            SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera();
        }
    }
}