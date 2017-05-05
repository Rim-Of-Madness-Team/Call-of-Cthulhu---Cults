using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using RimWorld;

namespace CultOfCthulhu
{
    public class CompSpawnerWombs : ThingComp
    {
        public const int MaxWombsPerMap = 30;

        private int nextWombSpawnTick = -1;

        private CompProperties_SpawnerWombs Props
        {
            get
            {
                return (CompProperties_SpawnerWombs)this.props;
            }
        }

        private bool CanSpawnChildWomb
        {
            get
            {
                return WombsUtility.TotalSpawnedWombsCount < 30;
            }
        }

        public override void PostSpawnSetup()
        {
            this.CalculateNextWombSpawnTick();
        }

        public override void CompTickRare()
        {
            WombBetweenWorlds Womb = this.parent as WombBetweenWorlds;
            if ((Womb == null || Womb.active) && Find.TickManager.TicksGame >= this.nextWombSpawnTick)
            {
                WombBetweenWorlds Womb2;
                if (this.TrySpawnChildWomb(false, out Womb2))
                {
                    Womb2.nextPawnSpawnTick = Find.TickManager.TicksGame + Rand.Range(150, 350);
                    Messages.Message("MessageWombReproduced".Translate(), Womb2, MessageSound.Negative);
                }
                else
                {
                    this.CalculateNextWombSpawnTick();
                }
            }
        }

        public override string CompInspectStringExtra()
        {
            string text = null;
            if (this.CanSpawnChildWomb)
            {
                text = text + "WombReproducesIn".Translate() + ": " + (this.nextWombSpawnTick - Find.TickManager.TicksGame).ToStringTicksToPeriod(true);
            }
            return text;
        }

        public void CalculateNextWombSpawnTick()
        {
            Room room = this.parent.GetRoom();
            int num = 0;
            int num2 = GenRadial.NumCellsInRadius(9f);
            for (int i = 0; i < num2; i++)
            {
                IntVec3 intVec = this.parent.Position + GenRadial.RadialPattern[i];
                if (intVec.InBounds())
                {
                    if (intVec.GetRoom() == room)
                    {
                        if (intVec.GetThingList().Any((Thing t) => t is Womb))
                        {
                            num++;
                        }
                    }
                }
            }
            float num3 = GenMath.LerpDouble(0f, 7f, 1f, 0.35f, (float)Mathf.Clamp(num, 0, 7));
            this.nextWombSpawnTick = Find.TickManager.TicksGame + (int)(this.Props.WombSpawnIntervalDays.RandomInRange * 60000f / (num3 * Find.Storyteller.difficulty.enemyReproductionRateFactor));
        }

        public bool TrySpawnChildWomb(bool ignoreRoofedRequirement, out Womb newWomb)
        {
            if (!this.CanSpawnChildWomb)
            {
                newWomb = null;
                return false;
            }
            IntVec3 invalid = IntVec3.Invalid;
            for (int i = 0; i < 3; i++)
            {
                float minDist = this.Props.WombSpawnPreferredMinDist;
                if (i == 1)
                {
                    minDist = 0f;
                }
                else if (i == 2)
                {
                    newWomb = null;
                    return false;
                }
                if (CellFinder.TryFindRandomReachableCellNear(this.parent.Position, this.Props.WombSpawnRadius, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), (IntVec3 c) => this.CanSpawnWombAt(c, minDist, ignoreRoofedRequirement), null, out invalid, 999999))
                {
                    break;
                }
            }
            newWomb = (WombBetweenWorlds)GenSpawn.Spawn(this.parent.def, invalid);
            if (newWomb.Faction != this.parent.Faction)
            {
                newWomb.SetFaction(this.parent.Faction, null);
            }
            WombBetweenWorlds Womb = this.parent as WombBetweenWorlds;
            if (Womb != null)
            {
                newWomb.active = Womb.active;
            }
            this.CalculateNextWombSpawnTick();
            return true;
        }

        private bool CanSpawnWombAt(IntVec3 c, float minDist, bool ignoreRoofedRequirement)
        {
            if ((!ignoreRoofedRequirement && !c.Roofed()) || !c.Standable() || (minDist != 0f && c.DistanceToSquared(this.parent.Position) < minDist * minDist))
            {
                return false;
            }
            for (int i = 0; i < 8; i++)
            {
                IntVec3 c2 = c + GenAdj.AdjacentCells[i];
                if (c2.InBounds())
                {
                    List<Thing> thingList = c2.GetThingList();
                    for (int j = 0; j < thingList.Count; j++)
                    {
                        if (thingList[j] is Womb)
                        {
                            return false;
                        }
                    }
                }
            }
            return c.GetThingList().Find((Thing x) => x.def.category == ThingCategory.Building || x.def.category == ThingCategory.Item) == null;
        }

        [DebuggerHidden]
        public override IEnumerable<Command> CompGetGizmosExtra()
        {
            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Reproduce",
                    icon = TexCommand.GatherSpotActive,
                    action = delegate
                    {
                        WombBetweenWorlds Womb;
                        this.TrySpawnChildWomb(false, out Womb);
                    }
                };
            }
            yield break;
        }

        public override void PostExposeData()
        {
            Scribe_Values.LookValue<int>(ref this.nextWombSpawnTick, "nextWombSpawnTick", 0, false);
        }
    }
}
