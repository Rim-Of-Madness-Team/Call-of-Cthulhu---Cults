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
            base.SpawnSetup(map: map, respawningAfterLoad: bla);
            TrySpawnMadSailors();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(value: ref pointsLeft, label: "pointsLeft");
            Scribe_Values.Look(value: ref age, label: "age");
            Scribe_References.Look(refee: ref lord, label: "defenseLord");
        }

        public override string GetInspectString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(value: base.GetInspectString());
            stringBuilder.AppendLine(value: "AwokeDaysAgo".Translate(
                arg1: age.TicksToDays().ToString(format: "F1")
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
            var faction = Find.FactionManager.FirstFactionOfDef(facDef: CultsDefOf.Cults_Sailors);
            Utility.DebugReport(x: faction.ToString());
            //Log.Message("Building_LandedShip LordJob_DefendPoint");
            var lordJob = new LordJob_DefendPoint(point: Position);
            if (pointsLeft <= 0f)
            {
                return;
            }

            if (lord == null)
            {
                lord = LordMaker.MakeNewLord(faction: faction, lordJob: lordJob, map: Map, startingPawns: lordList);
            }

            while (pointsLeft > 0f)
            {
                if (!(from cell in GenAdj.CellsAdjacent8Way(t: this)
                    where cell.Walkable(map: Map)
                    select cell).TryRandomElement(result: out var center))
                {
                    continue;
                }

                var request = new PawnGenerationRequest(kind: CultsDefOf.Cults_Sailor, faction: faction,
                    context: PawnGenerationContext.NonPlayer, tile: Map.Tile, forceGenerateNewPawn: false, 
                    allowDead: false, allowDowned: false, canGeneratePawnRelations: false, 
                    mustBeCapableOfViolence: true, colonistRelationChanceFactor: 0f, 
                    forceAddFreeWarmLayerIfNeeded: true, allowGay: false, allowPregnant: true, allowFood: true, 
                    allowAddictions: false, inhabitant: false, certainlyBeenInCryptosleep: false, 
                    forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 
                    biocodeWeaponChance: 0, biocodeApparelChance: 0, extraPawnForExtraRelationChance: null,
                    relationWithExtraPawnChanceFactor: 0);
                var pawn = PawnGenerator.GeneratePawn(request: request);
                if (!GenPlace.TryPlaceThing(thing: pawn, center: center, map: Map, mode: ThingPlaceMode.Near))
                {
                    continue;
                }

                if (pawn.GetLord() != null)
                {
                    pawn.GetLord().Cleanup();
                    pawn.GetLord().CurLordToil.Cleanup();
                    pawn.GetLord().LordJob.Cleanup();
                }

                lord.AddPawn(p: pawn);
                pointsLeft -= pawn.kindDef.combatPower;
                Utility.ApplySanityLoss(pawn: pawn, sanityLoss: 1f);

                //Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
            }

            pointsLeft = 0f;
            SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera();
        }
    }
}