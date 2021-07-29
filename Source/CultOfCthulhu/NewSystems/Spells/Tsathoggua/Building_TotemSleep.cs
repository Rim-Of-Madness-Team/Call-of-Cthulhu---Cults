using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;

namespace CultOfCthulhu
{
    public class Building_TotemSleep : Building
    {
        public enum State
        {
            Asleep = 0,
            Drowsy = 1,
            Awake = 2
        }

        private bool cellsDirty = true;

        private Graphic curGraphic;
        private List<IntVec3> tempCells;

        private int ticksToReset = -1;

        public State CurState
        {
            get
            {
                var curState = State.Asleep;
                switch (ActiveVictims.Count)
                {
                    case 0:
                        curState = State.Awake;
                        break;
                    case 1:
                        curState = State.Drowsy;
                        break;
                    case 2:
                        curState = State.Asleep;
                        break;
                }

                curGraphic = null;
                return curState;
            }
        }

        public List<IntVec3> SleepableCells
        {
            get
            {
                if (!tempCells.NullOrEmpty() && !cellsDirty)
                {
                    return tempCells;
                }

                cellsDirty = false;
                tempCells = new List<IntVec3>(GenRadial.RadialCellsAround(Position, def.specialDisplayRadius,
                    true));

                return tempCells;
            }
        }

        public List<Pawn> ActiveVictims { get; set; } = new List<Pawn>();

        public override Graphic Graphic
        {
            get
            {
                if (curGraphic != null)
                {
                    return curGraphic;
                }

                switch (CurState)
                {
                    case State.Asleep:
                        curGraphic = GraphicConstructor(
                            "Building/Exotic/SleepTotem/SleepTotemAsleep");
                        break;
                    case State.Drowsy:
                        curGraphic = DefaultGraphic;
                        break;
                    case State.Awake:
                        curGraphic = GraphicConstructor(
                            "Building/Exotic/SleepTotem/SleepTotemAwake");
                        break;
                }

                return curGraphic;
            }
        }

        public int TicksToReset
        {
            get => ticksToReset;
            set => ticksToReset = value;
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            cellsDirty = true;
        }

        public void TryToSendToSleep(Pawn victim)
        {
            TicksToReset = Find.TickManager.TicksGame + (GenDate.TicksPerHour * 8);
            ActiveVictims.Add(victim);
            //victim.needs.rest.CurLevelPercentage = 0.0f;
            GenExplosion.DoExplosion(victim.PositionHeld, victim.MapHeld, 1f, DamageDefOf.Smoke, this);
            HealthUtility.AdjustSeverity(victim, CultsDefOf.Cults_SleepHediff, 1.0f);
        }

        public override void TickRare()
        {
            base.TickRare();
            if (CurState != State.Asleep)
            {
                var potentialVictim = PotentialVictims()?.RandomElement();
                if (potentialVictim != null)
                {
                    TryToSendToSleep(potentialVictim);
                }
            }

            if (!ActiveVictims.NullOrEmpty() && ticksToReset < Find.TickManager.TicksGame)
            {
                ActiveVictims = new List<Pawn>();
            }
        }

        public override string GetInspectString()
        {
            var s = new StringBuilder();
            var sBase = base.GetInspectString();
            if (sBase != "")
            {
                s.Append(s);
            }

            switch (CurState)
            {
                case State.Asleep:
                    s.AppendLine("Cults_SleepTotem_StateAsleep".Translate());
                    break;
                case State.Drowsy:
                    s.AppendLine("Cults_SleepTotem_StateDrowsy".Translate());
                    break;
                case State.Awake:
                    s.AppendLine("Cults_SleepTotem_StateAwake".Translate());
                    break;
            }

            if (TicksToReset == -1 || TicksToReset <= Find.TickManager.TicksGame)
            {
                return s.ToString().TrimEndNewlines();
            }

            var ticksUntilRecovery = TicksToReset - Find.TickManager.TicksGame;
            s.AppendLine("Cults_SleepTotem_FullyAwakensIn".Translate(ticksUntilRecovery.ToStringTicksToPeriod()));

            return s.ToString().TrimEndNewlines();
        }

        public IEnumerable<Pawn> PotentialVictims()
        {
            if (SleepableCells.NullOrEmpty())
            {
                yield break;
            }

            foreach (var cell in SleepableCells)
            {
                var victim = cell.GetFirstPawn(MapHeld);
                if (victim != null && CanSendToSleep(victim))
                {
                    yield return victim;
                }
            }
        }

        public bool CanSendToSleep(Pawn victim)
        {
            return CurState != State.Asleep && !ActiveVictims.Contains(victim) && !victim.Dead && victim.Spawned &&
                   victim.Faction != null && victim.Faction.HostileTo(Faction) &&
                   !victim.RaceProps.IsMechanoid && victim.needs?.rest != null &&
                   !victim.health.hediffSet.HasHediff(CultsDefOf.Cults_SleepHediff);
        }

        public Graphic GraphicConstructor(string newTexPath)
        {
            var tempData = new GraphicData();
            tempData.CopyFrom(def.graphicData);
            tempData.texPath = newTexPath;
            var result = tempData.GraphicColoredFor(this);
            return result;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksToReset, "ticksToReset", -1);
        }
    }
}