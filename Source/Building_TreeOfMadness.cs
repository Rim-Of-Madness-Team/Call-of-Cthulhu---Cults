using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Verse.Sound;
using Verse.AI;
using UnityEngine;
using System.Reflection;

namespace CultOfCthulhu
{
    public class Building_TreeOfMadness : Plant
    {
        public bool isQuiet = false;
        private bool isMuted = false;
        private int ticksUntilQuiet = 960;
        private Sustainer sustainerAmbient;
        private static readonly FloatRange QuietIntervalDays = new FloatRange(1.5f, 2.5f);
        

        public override void SpawnSetup(Map map)
        {
            ticksUntilQuiet += (int)(QuietIntervalDays.RandomInRange * 60000f);
            base.SpawnSetup(map);
            SoundInfo info = SoundInfo.InMap(this, MaintenanceType.None);
            this.sustainerAmbient = this.def.building.soundAmbient.TrySpawnSustainer(info);
        }

        public override void Tick()
        {
            base.Tick();
            DoTickWork();
        }

        public void DoTickWork()
        {
            if (isQuiet) return;
            ticksUntilQuiet--;
            if (ticksUntilQuiet <= 0)
            {
                isQuiet = true;
                sustainerAmbient.End();
            }
        }

        public override void DeSpawn()
        {
            base.DeSpawn();
            if (this.sustainerAmbient != null)
            {
                this.sustainerAmbient.End();
            }
        }


        public Thought_Memory GiveObservedThought()
        {
            if (this.StoringBuilding() == null)
            {
                Thought_MemoryObservation thought_MemoryObservation;
                thought_MemoryObservation = (Thought_MemoryObservation)ThoughtMaker.MakeThought(DefDatabase<ThoughtDef>.GetNamed("ObservedNightmareTree"));
                thought_MemoryObservation.Target = this;
                Pawn Dave = thought_MemoryObservation.pawn;
                if (Dave == null) return null;
                if (!Dave.IsColonist) return thought_MemoryObservation;
                else
                {
                    if (Dave.needs.TryGetNeed<Need_CultMindedness>().CurLevel > 0.7)
                        thought_MemoryObservation = (Thought_MemoryObservation)ThoughtMaker.MakeThought(DefDatabase<ThoughtDef>.GetNamed("ObservedNightmareTreeCultist"));
                }
                return thought_MemoryObservation;
            }
            return null;
        }


        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            ///This code returns all the other float menu options first!
            IEnumerator<FloatMenuOption> enumerator = base.GetFloatMenuOptions(myPawn).GetEnumerator();
            while (enumerator.MoveNext())
            {
                FloatMenuOption current = enumerator.Current;
                yield return current;
            }

            if (CultUtility.AreCultObjectsAvailable(Map) == false)
            {
                if (CultUtility.IsSomeoneInvestigating(Map) == false)
                {
                    Action action0 = delegate
                    {
                        Job job = new Job(CultDefOfs.Investigate, myPawn, this);
                        job.playerForced = true;
                        myPawn.QueueJob(job);
                        myPawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                    };
                    yield return new FloatMenuOption("Investigate", action0, MenuOptionPriority.Default, null, null, 0f, null);
                }
            }
        }


        public void MuteToggle()
        {
            this.isMuted = !this.isMuted;
            if (isMuted)
            {
                sustainerAmbient.End();
            }
            else
            {

                    SoundInfo info = SoundInfo.InMap(this, MaintenanceType.None);
                    sustainerAmbient = new Sustainer(this.def.building.soundAmbient, info);


            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            IEnumerator<Gizmo> enumerator = base.GetGizmos().GetEnumerator();
            while (enumerator.MoveNext())
            {
                Gizmo current = enumerator.Current;
                yield return current;
            }

            Command_Toggle toggleDef = new Command_Toggle();
            toggleDef.hotKey = KeyBindingDefOf.CommandTogglePower;
            toggleDef.icon = ContentFinder<Texture2D>.Get("UI/Icons/Commands/Mute", true);
            toggleDef.defaultLabel = "Mute".Translate();
            toggleDef.defaultDesc = "MuteDesc".Translate();
            toggleDef.isActive = (() => this.isMuted);
            toggleDef.toggleAction = delegate
            {
                MuteToggle();
            };
            yield return toggleDef;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            // Save and load the work variables, so they don't default after loading
            Scribe_Values.LookValue<bool>(ref isMuted, "isMuted", false);

        }
    }
}
