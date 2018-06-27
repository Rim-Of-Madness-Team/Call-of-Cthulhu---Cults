using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse;
using Verse.AI.Group;
using RimWorld;
using System.Text;

namespace CultOfCthulhu
{

    public class Building_WombBetweenWorlds : ThingWithComps
    {
        private const int InitialPawnSpawnDelay = 960;

        private const int PawnSpawnRadius = 5;

        private const float MaxSpawnedPawnsPoints = 500f;

        private const int InitialPawnsPoints = 260;

        public bool active = true;

        public int nextPawnSpawnTick = -1;

        public List<Pawn> spawnedPawns = new List<Pawn>();

        private Lord lord;

        private int ticksToSpawnInitialPawns = -1;

        //private static readonly FloatRange PawnSpawnIntervalDays = new FloatRange(0.85f, 1.1f);
        private static readonly FloatRange PawnSpawnIntervalDays = new FloatRange(3.85f, 3.1f);

        


        private float SpawnedPawnsPoints
        {
            get
            {
                this.FilterOutUnspawnedPawns();
                float num = 0f;
                for (int i = 0; i < this.spawnedPawns.Count; i++)
                {
                    num += this.spawnedPawns[i].kindDef.combatPower;
                }
                return num;
            }
        }

        public override void SpawnSetup(Map map, bool bla)
        {
            base.SpawnSetup(map, bla);
            if (base.Faction == null)
            {
                this.SetFaction(Faction.OfInsects, null);
            }
        }

        public void StartInitialPawnSpawnCountdown()
        {
            this.ticksToSpawnInitialPawns = 960;
        }

        private void SpawnInitialPawnsNow()
        {
            this.ticksToSpawnInitialPawns = -1;
            while (this.SpawnedPawnsPoints < 260f)
            {
                Pawn pawn;
                if (!this.TrySpawnPawn(out pawn, Map))
                {
                    return;
                }
            }
            this.CalculateNextPawnSpawnTick();
        }

        public override void TickRare()
        {
            base.TickRare();
            this.FilterOutUnspawnedPawns();
            if (!this.active && !base.Position.Fogged(Map))
            {
                this.Activate();
            }
            if (this.active)
            {
                if (this.ticksToSpawnInitialPawns > 0)
                {
                    this.ticksToSpawnInitialPawns -= 250;
                    if (this.ticksToSpawnInitialPawns <= 0)
                    {
                        this.SpawnInitialPawnsNow();
                    }
                }
                if (Find.TickManager.TicksGame >= this.nextPawnSpawnTick)
                {
                    if (this.SpawnedPawnsPoints < MaxSpawnedPawnsPoints)
                    {
                        Pawn pawn;
                        bool flag = this.TrySpawnPawn(out pawn, Map);
                        if (flag && pawn.caller != null)
                        {
                            pawn.caller.DoCall();
                        }
                    }
                    this.CalculateNextPawnSpawnTick();
                }
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);
            //List<Lord> lords = Find.LordManager.lords;
            //for (int i = 0; i < lords.Count; i++)
            //{
            //    lords[i].ReceiveMemo("HiveDestroyed");
            //}
        }

        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            if (dinfo.Def.externalViolence && dinfo.Instigator != null)
            {
                if (this.ticksToSpawnInitialPawns > 0)
                {
                    this.SpawnInitialPawnsNow();
                }
                //Lord lord = this.Lord;
                //if (lord != null)
                //{
                //    lord.ReceiveMemo("HiveAttacked");
                //}
            }
            base.PostApplyDamage(dinfo, totalDamageDealt);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.active, "active", false, false);
            Scribe_Values.Look<int>(ref this.nextPawnSpawnTick, "nextPawnSpawnTick", 0, false);
            Scribe_Collections.Look<Pawn>(ref this.spawnedPawns, "spawnedPawns", LookMode.Reference, new object[0]);
            Scribe_Values.Look<int>(ref this.ticksToSpawnInitialPawns, "ticksToSpawnInitialPawns", 0, false);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                this.spawnedPawns.RemoveAll((Pawn x) => x == null);
            }
        }

        private void Activate()
        {
            this.active = true;
            this.nextPawnSpawnTick = Find.TickManager.TicksGame + Rand.Range(200, 400);
            //CompSpawnerHives comp = base.GetComp<CompSpawnerHives>();
            //if (comp != null)
            //{
            //    comp.CalculateNextHiveSpawnTick();
            //}
        }

        public override string GetInspectString()
        {
            StringBuilder s = new StringBuilder();
            s.Append(base.GetInspectString());
            string text = String.Empty;

            if (this.CanSpawnPawns())
            {
                text = text + "DarkYoungSpawnsIn".Translate() + ": " + (this.nextPawnSpawnTick - Find.TickManager.TicksGame).ToStringTicksToPeriodVagueMax();
            }
            else
            {

            }
            s.Append(text);
            return s.ToString();
        }

        public bool CanSpawnPawns()
        {
            if (this.SpawnedPawnsPoints < MaxSpawnedPawnsPoints) return true;
            else
            {
                return false;
            }
        }

        private void CalculateNextPawnSpawnTick()
        {
            float num = GenMath.LerpDouble(0f, 5f, 1f, 0.5f, (float)this.spawnedPawns.Count);
            this.nextPawnSpawnTick = Find.TickManager.TicksGame + (int)(Building_WombBetweenWorlds.PawnSpawnIntervalDays.RandomInRange * 60000f / (num * Find.Storyteller.difficulty.enemyReproductionRateFactor));
            //this.nextPawnSpawnTick = Find.TickManager.TicksGame + (int)(Building_WombBetweenWorlds.PawnSpawnIntervalDays.RandomInRange * 60000f);
        }

        private void FilterOutUnspawnedPawns()
        {
            this.spawnedPawns.RemoveAll((Pawn x) => !x.Spawned);
        }

        private bool TrySpawnPawn(out Pawn pawn, Map map)
        {
            var kindDef = (Cthulhu.Utility.IsCosmicHorrorsLoaded() ? PawnKindDef.Named("ROM_DarkYoung") : PawnKindDefOf.Megaspider);
            pawn = PawnGenerator.GeneratePawn(kindDef, base.Faction);
            try
            {
                IntVec3 pos = base.Position;
                for (int i = 0; i < 3; i++)
                {
                    pos += GenAdj.CardinalDirections[2];
                }
                GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(pos, map, 1), map); //
                this.spawnedPawns.Add(pawn);
                if (this.Faction != Faction.OfPlayer)
                {
                    if (lord == null)
                    {
                        lord = this.CreateNewLord();
                    }
                    lord.AddPawn(pawn);
                }

                Messages.Message("Cults_NewDarkYoung".Translate(), pawn, MessageTypeDefOf.PositiveEvent);
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
            IEnumerator<Gizmo> enumerator = base.GetGizmos().GetEnumerator();
            while (enumerator.MoveNext())
            {
                Gizmo current = enumerator.Current;
                yield return current;
            }
            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Spawn pawn",
                    icon = TexCommand.ReleaseAnimals,
                    action = delegate
                    {
                        Pawn pawn;
                        this.TrySpawnPawn(out pawn, Map);
                    }
                };
            }
            yield break;
        }

        public override bool PreventPlayerSellingThingsNearby(out string reason)
        {
            if (this.spawnedPawns.Count > 0)
            {
                if (this.spawnedPawns.Any((Pawn p) => !p.Downed))
                {
                    reason = this.def.label;
                    return true;
                }
            }
            reason = null;
            return false;
        }

        private Lord CreateNewLord()
        {
            return LordMaker.MakeNewLord(base.Faction, new LordJob_DefendPoint(this.Position), null);
        }
    }
}
