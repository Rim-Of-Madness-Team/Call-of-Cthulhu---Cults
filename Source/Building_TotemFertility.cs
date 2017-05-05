// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

// ----------------------------------------------------------------------
// These are RimWorld-specific usings. Activate/Deactivate what you need:
// ----------------------------------------------------------------------
using UnityEngine;         // Always needed
//using VerseBase;         // Material/Graphics handling functions are found here
using Verse;               // RimWorld universal objects are here (like 'Building')
//using Verse.AI;          // Needed when you do something with the AI
//using Verse.AI.Group;
//using Verse.Sound;       // Needed when you do something with Sound
//using Verse.Noise;       // Needed when you do something with Noises
using RimWorld;            // RimWorld specific functions are found here (like 'Building_Battery')
//using RimWorld.Planet;   // RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

namespace CultOfCthulhu
{
    public class Building_TotemFertility : Building
    {
        public float fertilityBonus = 0.5f;
        public float fertilityMax = 2.0f;
        public float ticksUntilDestroyed = -1f;
        public float daysUntilDestroyed = 7f;

        public IEnumerable<IntVec3> GrowableCells
        {
            get
            {
                return GenRadial.RadialCellsAround(base.Position, this.def.specialDisplayRadius, true);
            }
        }
        
        public int TicksUntilDisappearing
        {
            get
            {
                if (ticksUntilDestroyed <= 0f)
                {
                    return Mathf.RoundToInt(this.ticksUntilDestroyed = this.daysUntilDestroyed * 60000f);
                }
                return Mathf.RoundToInt(this.ticksUntilDestroyed);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
                        
            Scribe_Values.LookValue<float>(ref this.ticksUntilDestroyed, "ticksUntilDestroyed", -1f, false);
            Scribe_Values.LookValue<float>(ref this.daysUntilDestroyed, "daysUntilDestroyed", 7f, false);
            Scribe_Values.LookValue<float>(ref this.fertilityBonus, "fertilityBonus", 1.5f, false);
        }

        public override void Tick()
        {
            base.Tick();
            if (ticksUntilDestroyed > 0)
            {
                if (ticksUntilDestroyed < 100)
                {
                    this.DeSpawn();
                }
                else
                {
                    ticksUntilDestroyed -= 1;
                }
            }
        }


        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            // Add the inspections string from the base
            stringBuilder.Append(base.GetInspectString());
            if (!(stringBuilder.ToString().Contains("installed")))
            { 
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("FertilityTotemTimer".Translate(new object[]
                {
                TicksUntilDisappearing.ToStringTicksToPeriodVagueMax()
                }));
            }
            return stringBuilder.ToString();
        }

        public override void SpawnSetup(Map map)
        {
            base.SpawnSetup(map);
            List<IntVec3> temp = new List<IntVec3>();
            foreach (IntVec3 vec in GrowableCells)
            {
                temp.Add(vec);
            }
            map.GetComponent<MapComponent_FertilityMods>().FertilizeCells(temp);
        }


        public override void DeSpawn()
        {
            Map map = this.Map;
            base.DeSpawn();
            List<IntVec3> temp = new List<IntVec3>();
            foreach (IntVec3 vec in GrowableCells)
            {
                temp.Add(vec);
            }
            map.GetComponent<MapComponent_FertilityMods>().UnfertilizeCells(temp);

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
            yield return new Command_Action
            {
                action = new Action(this.MakeMatchingGrowZone),
                hotKey = KeyBindingDefOf.Misc2,
                defaultDesc = "CommandSunLampMakeGrowingZoneDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Designators/ZoneCreate_Growing", true),
                defaultLabel = "CommandSunLampMakeGrowingZoneLabel".Translate()
            };
            yield break;
        }

        private void MakeMatchingGrowZone()
        {
            Designator_ZoneAdd_Growing designator = new Designator_ZoneAdd_Growing();
            designator.DesignateMultiCell(from tempCell in this.GrowableCells
                                          where designator.CanDesignateCell(tempCell).Accepted
                                          select tempCell);
        }
    }
}
