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
    public class Building_Monolith : Building
    {
        private bool isMuted = false;

        public Thought_Memory GiveObservedThought()
        {
            if (this.StoringBuilding() == null)
            {
                Thought_MemoryObservation thought_MemoryObservation;
                thought_MemoryObservation = (Thought_MemoryObservation)ThoughtMaker.MakeThought(DefDatabase<ThoughtDef>.GetNamed("Cults_ObservedNightmareMonolith"));
                thought_MemoryObservation.Target = this;
                Pawn Dave = thought_MemoryObservation.pawn;
                if (Dave == null) return null;
                if (!Dave.IsColonist) return thought_MemoryObservation;
                else
                {
                    if (Dave.needs.TryGetNeed<Need_CultMindedness>().CurLevel > 0.7)
                        thought_MemoryObservation = (Thought_MemoryObservation)ThoughtMaker.MakeThought(DefDatabase<ThoughtDef>.GetNamed("Cults_ObservedNightmareMonolithCultist"));
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

            if (this.def != null)
            {
                if (this.def.hasInteractionCell)
                {
                    if (CultUtility.AreCultObjectsAvailable(Map) == false)
                    {
                        if (CultUtility.IsSomeoneInvestigating(Map) == false)
                        {
                            if (Map.reservationManager.CanReserve(myPawn, this))
                            {
                                Action action0 = delegate
                                {
                                    Job job = new Job(CultsDefOf.Cults_Investigate, myPawn, this);
                                    job.playerForced = true;
                                    myPawn.jobs.TryTakeOrderedJob(job);
                                    //mypawn.CurJob.EndCurrentJob(JobCondition.InterruptForced);
                                };
                                yield return new FloatMenuOption("Investigate", action0, MenuOptionPriority.Default, null, null, 0f, null);
                            }
                        }
                    }
                }
            }
        }

        public void MuteToggle()
        {
            this.isMuted = !this.isMuted;
            if (isMuted)
            {
                Sustainer sustainer = (Sustainer)typeof(Building).GetField("sustainerAmbient", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
                sustainer.End();
            }
            else
            {
                Sustainer sustainer = (Sustainer)typeof(Building).GetField("sustainerAmbient", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);

                    SoundInfo info = SoundInfo.InMap(this, MaintenanceType.None);
                    sustainer =  new Sustainer(this.def.building.soundAmbient, info);
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
            Scribe_Values.Look<bool>(ref isMuted, "isMuted", false);

        }

    }
}

