using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Noise;
using Verse.Sound;
using RimWorld;

namespace CultOfCthulhu
{
    internal class Building_LandedShip : Building
    {

        public float pointsLeft = 300f;

        protected int age;

        private Lord lord;
        
        private static HashSet<IntVec3> reachableCells = new HashSet<IntVec3>();

        public override void SpawnSetup(Map map, bool bla)
        {
            base.SpawnSetup(map, bla);
            TrySpawnMadSailors();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref this.pointsLeft, "pointsLeft", 0f, false);
            Scribe_Values.Look<int>(ref this.age, "age", 0, false);
            Scribe_References.Look<Lord>(ref this.lord, "defenseLord", false);
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(base.GetInspectString());
            stringBuilder.AppendLine("AwokeDaysAgo".Translate(
                this.age.TicksToDays().ToString("F1")
            ));
            return stringBuilder.ToString();
        }

        public override void Tick()
        {
            base.Tick();
            this.age++;
        }
       
        private void TrySpawnMadSailors()
        {
            List<Pawn> lordList = new List<Pawn>();
            Faction faction = Find.FactionManager.FirstFactionOfDef(CultsDefOf.Cults_Sailors);
            Cthulhu.Utility.DebugReport(faction.ToString());
            LordJob_DefendPoint lordJob = new LordJob_DefendPoint(this.Position);
            if (this.pointsLeft <= 0f)
            {
                return;
            }
            if (this.lord == null)
            {
                this.lord = LordMaker.MakeNewLord(faction, lordJob, Map, lordList);
            }
            while (pointsLeft > 0f)
            {
                IntVec3 center;
                if ((from cell in GenAdj.CellsAdjacent8Way(this)
                     where cell.Walkable(Map)
                     select cell).TryRandomElement(out center))
                {
                        PawnGenerationRequest request = new PawnGenerationRequest(CultsDefOf.Cults_Sailor, faction, PawnGenerationContext.NonPlayer, Map.Tile, false, false, false, false, true, true, 20f, false, true, true, false, false, false, false, false, 0, null, 0, null, null, null);
                        Pawn pawn = PawnGenerator.GeneratePawn(request);
                        if (GenPlace.TryPlaceThing(pawn, center, Map, ThingPlaceMode.Near, null))
                        {

                            if (LordUtility.GetLord(pawn) != null)
                        {
                            LordUtility.GetLord(pawn).Cleanup();
                            LordUtility.GetLord(pawn).CurLordToil.Cleanup();
                            LordUtility.GetLord(pawn).LordJob.Cleanup();
                        }
                            this.lord.AddPawn(pawn);
                            this.pointsLeft -= pawn.kindDef.combatPower;
                            Cthulhu.Utility.ApplySanityLoss(pawn, 1f);
                            continue;
                        }
                        //Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
                }
            }
            this.pointsLeft = 0f;
            SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera();
            return;
        }
        
    }
}
