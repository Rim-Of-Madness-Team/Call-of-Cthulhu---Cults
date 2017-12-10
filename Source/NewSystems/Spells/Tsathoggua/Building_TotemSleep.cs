using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
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
        
        public State CurState {
            get
            {
                State curState = State.Asleep;
                switch (ActiveVictims.Count())
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
        
        private bool cellsDirty = true;
        List<IntVec3> tempCells;
        public List<IntVec3> SleepableCells
        {
            get
            {
                if (tempCells.NullOrEmpty() || cellsDirty)
                {
                    cellsDirty = false;
                    tempCells = new List<IntVec3>(GenRadial.RadialCellsAround(base.Position, this.def.specialDisplayRadius, true));
                }
                return tempCells;
            }
        }


        private List<Pawn> activeVictims = new List<Pawn>();
        public List<Pawn> ActiveVictims { get => activeVictims; set => activeVictims = value; }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            cellsDirty = true;
        }

        private Graphic curGraphic = null;
        public override Graphic Graphic
        {
            get
            {
                if (curGraphic == null)
                {
                    switch (CurState)
                    {
                        case State.Asleep:
                            curGraphic = GraphicConstructor(
                                "Building/Exotic/SleepTotem/SleepTotemAsleep");
                            break;
                        case State.Drowsy:
                            curGraphic = this.DefaultGraphic;
                            break;
                        case State.Awake:
                            curGraphic = GraphicConstructor(
                                "Building/Exotic/SleepTotem/SleepTotemAwake");
                            break;

                    }
                }
                return curGraphic;
            }
        }

        private int ticksToReset = -1;
        public int TicksToReset { get => ticksToReset; set => ticksToReset = value; }

        public void TryToSendToSleep(Pawn victim)
        {
            TicksToReset = Find.TickManager.TicksGame + GenDate.TicksPerHour * 8;
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
                Pawn potentialVictim = PotentialVictims()?.RandomElement() ?? null;
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
            StringBuilder s = new StringBuilder();
            string sBase = base.GetInspectString();
            if (sBase != "")
                s.Append(s);
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
            if (TicksToReset != -1 && TicksToReset > Find.TickManager.TicksGame)
            {
                int ticksUntilRecovery = TicksToReset - Find.TickManager.TicksGame;
                s.AppendLine("Cults_SleepTotem_FullyAwakensIn".Translate(ticksUntilRecovery.ToStringTicksToPeriod()));
            }
            return s.ToString().TrimEndNewlines();
        }

        public IEnumerable<Pawn> PotentialVictims()
        {
            if (!SleepableCells.NullOrEmpty())
            {
                foreach (IntVec3 cell in SleepableCells)
                {
                    Pawn victim = cell.GetFirstPawn(this.MapHeld);
                    if (victim != null && CanSendToSleep(victim))
                        yield return victim;
                }
            }
        }

        public bool CanSendToSleep(Pawn victim)
            => CurState != State.Asleep && !ActiveVictims.Contains(victim) && !victim.Dead && victim.Spawned &&
            victim.Faction != null && victim.Faction.HostileTo(this.Faction)  &&
            !victim.RaceProps.IsMechanoid && victim?.needs?.rest != null && !victim.health.hediffSet.HasHediff(CultsDefOf.Cults_SleepHediff);

        public Graphic GraphicConstructor(string newTexPath)
        {
            Graphic result = null;
            GraphicData tempData = new GraphicData();
            tempData.CopyFrom(this.def.graphicData);
            tempData.texPath = newTexPath;
            result = tempData.GraphicColoredFor(this);
            return result;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.ticksToReset, "ticksToReset", -1);
        }
    }
}
