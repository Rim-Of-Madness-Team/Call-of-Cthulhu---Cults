using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Cthulhu;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace CultOfCthulhu
{
    public class Building_WombBetweenWorlds : ThingWithComps
    {
        private const int InitialPawnSpawnDelay = 960;

        private const int PawnSpawnRadius = 5;

        private const float MaxSpawnedPawnsPoints = 500f;

        private const int InitialPawnsPoints = 260;

        //private static readonly FloatRange PawnSpawnIntervalDays = new FloatRange(0.85f, 1.1f);
        private static readonly FloatRange PawnSpawnIntervalDays = new FloatRange(min: 3.85f, max: 3.1f);

        public bool active = true;

        private Lord lord;

        public int nextPawnSpawnTick = -1;

        public List<Pawn> spawnedPawns = new List<Pawn>();

        private int ticksToSpawnInitialPawns = -1;


        private float SpawnedPawnsPoints
        {
            get
            {
                FilterOutUnspawnedPawns();
                var num = 0f;
                foreach (var pawn in spawnedPawns)
                {
                    num += pawn.kindDef.combatPower;
                }

                return num;
            }
        }

        public override void SpawnSetup(Map map, bool bla)
        {
            base.SpawnSetup(map: map, respawningAfterLoad: bla);
            if (Faction == null)
            {
                SetFaction(newFaction: Faction.OfInsects);
            }
        }

        public void StartInitialPawnSpawnCountdown()
        {
            ticksToSpawnInitialPawns = 960;
        }

        private void SpawnInitialPawnsNow()
        {
            ticksToSpawnInitialPawns = -1;
            while (SpawnedPawnsPoints < 260f)
            {
                if (!TrySpawnPawn(pawn: out _, map: Map))
                {
                    return;
                }
            }

            CalculateNextPawnSpawnTick();
        }

        public override void TickRare()
        {
            base.TickRare();
            FilterOutUnspawnedPawns();
            if (!active && !Position.Fogged(map: Map))
            {
                Activate();
            }

            if (!active)
            {
                return;
            }

            if (ticksToSpawnInitialPawns > 0)
            {
                ticksToSpawnInitialPawns -= 250;
                if (ticksToSpawnInitialPawns <= 0)
                {
                    SpawnInitialPawnsNow();
                }
            }

            if (Find.TickManager.TicksGame < nextPawnSpawnTick)
            {
                return;
            }

            if (SpawnedPawnsPoints < MaxSpawnedPawnsPoints)
            {
                var flag = TrySpawnPawn(pawn: out var pawn, map: Map);
                if (flag)
                {
                    pawn.caller?.DoCall();
                }
            }

            CalculateNextPawnSpawnTick();
        }

        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            if (dinfo.Def.ExternalViolenceFor(thing: dinfo.IntendedTarget) && dinfo.Instigator != null)
            {
                if (ticksToSpawnInitialPawns > 0)
                {
                    SpawnInitialPawnsNow();
                }

                //Lord lord = this.Lord;
                //if (lord != null)
                //{
                //    lord.ReceiveMemo("HiveAttacked");
                //}
            }

            base.PostApplyDamage(dinfo: dinfo, totalDamageDealt: totalDamageDealt);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(value: ref active, label: "active");
            Scribe_Values.Look(value: ref nextPawnSpawnTick, label: "nextPawnSpawnTick");
            Scribe_Collections.Look(list: ref spawnedPawns, label: "spawnedPawns", lookMode: LookMode.Reference);
            Scribe_Values.Look(value: ref ticksToSpawnInitialPawns, label: "ticksToSpawnInitialPawns");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                spawnedPawns.RemoveAll(match: x => x == null);
            }
        }

        private void Activate()
        {
            active = true;
            nextPawnSpawnTick = Find.TickManager.TicksGame + Rand.Range(min: 200, max: 400);
            //CompSpawnerHives comp = base.GetComp<CompSpawnerHives>();
            //if (comp != null)
            //{
            //    comp.CalculateNextHiveSpawnTick();
            //}
        }

        public override string GetInspectString()
        {
            var s = new StringBuilder();
            s.Append(value: base.GetInspectString());
            var text = string.Empty;

            if (CanSpawnPawns())
            {
                text = text + "DarkYoungSpawnsIn".Translate() + ": " +
                       (nextPawnSpawnTick - Find.TickManager.TicksGame).ToStringTicksToPeriodVague();
            }

            s.Append(value: text);
            return s.ToString();
        }

        public bool CanSpawnPawns()
        {
            return SpawnedPawnsPoints < MaxSpawnedPawnsPoints;
        }

        private void CalculateNextPawnSpawnTick()
        {
            var num = GenMath.LerpDouble(inFrom: 0f, inTo: 5f, outFrom: 1f, outTo: 0.5f, x: spawnedPawns.Count);
            nextPawnSpawnTick = Find.TickManager.TicksGame + (int) (PawnSpawnIntervalDays.RandomInRange * 60000f /
                                                                    (num * Find.Storyteller.difficulty
                                                                        .enemyReproductionRateFactor));
            //this.nextPawnSpawnTick = Find.TickManager.TicksGame + (int)(Building_WombBetweenWorlds.PawnSpawnIntervalDays.RandomInRange * 60000f);
        }

        private void FilterOutUnspawnedPawns()
        {
            spawnedPawns.RemoveAll(match: x => !x.Spawned);
        }

        private bool TrySpawnPawn(out Pawn pawn, Map map)
        {
            var kindDef = Utility.IsCosmicHorrorsLoaded()
                ? PawnKindDef.Named(defName: "ROM_DarkYoung")
                : PawnKindDefOf.Megaspider;
            pawn = PawnGenerator.GeneratePawn(kindDef: kindDef, faction: Faction);
            try
            {
                var pos = Position;
                for (var i = 0; i < 3; i++)
                {
                    pos += GenAdj.CardinalDirections[2];
                }

                GenSpawn.Spawn(newThing: pawn, loc: CellFinder.RandomClosewalkCellNear(root: pos, map: map, radius: 1), map: map); //
                spawnedPawns.Add(item: pawn);
                if (Faction != Faction.OfPlayer)
                {
                    if (lord == null)
                    {
                        lord = CreateNewLord();
                    }

                    lord.AddPawn(p: pawn);
                }

                Messages.Message(text: "Cults_NewDarkYoung".Translate(), lookTargets: pawn, def: MessageTypeDefOf.PositiveEvent);
                return true;
            }
            catch
            {
                return true;
            }
        }

        [DebuggerHidden]
        public override IEnumerable<Gizmo> GetGizmos()
        {
            using var enumerator = base.GetGizmos().GetEnumerator();
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                yield return current;
            }

            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Spawn pawn",
                    icon = TexCommand.ReleaseAnimals,
                    action = delegate { TrySpawnPawn(pawn: out _, map: Map); }
                };
            }
        }

        public override bool PreventPlayerSellingThingsNearby(out string reason)
        {
            if (spawnedPawns.Count > 0)
            {
                if (spawnedPawns.Any(predicate: p => !p.Downed))
                {
                    reason = def.label;
                    return true;
                }
            }

            reason = null;
            return false;
        }

        private Lord CreateNewLord()
        {
            //Log.Message("Building_WombBetweenWorlds LordJob_DefendPoint");
            return LordMaker.MakeNewLord(faction: Faction, lordJob: new LordJob_DefendPoint(point: Position), map: null);
        }
    }
}