using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace CultOfCthulhu
{
    public class Building_Monolith : Building
    {
        private bool isMuted;

        public Thought_Memory GiveObservedThought()
        {
            if (this.StoringThing() != null)
            {
                return null;
            }

            var thought_MemoryObservation = (Thought_MemoryObservation) ThoughtMaker.MakeThought(
                DefDatabase<ThoughtDef>.GetNamed("Cults_ObservedNightmareMonolith"));
            thought_MemoryObservation.Target = this;
            var Dave = thought_MemoryObservation.pawn;
            if (Dave == null)
            {
                return null;
            }

            if (!Dave.IsColonist)
            {
                return thought_MemoryObservation;
            }

            if (Dave.needs.TryGetNeed<Need_CultMindedness>().CurLevel > 0.7)
            {
                thought_MemoryObservation =
                    (Thought_MemoryObservation) ThoughtMaker.MakeThought(
                        DefDatabase<ThoughtDef>.GetNamed("Cults_ObservedNightmareMonolithCultist"));
            }

            return thought_MemoryObservation;
        }


        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            using var enumerator = base.GetFloatMenuOptions(myPawn).GetEnumerator();
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                yield return current;
            }

            if (def == null || this is not Building_Monolith)
            {
                yield break;
            }

            if (!def.hasInteractionCell)
            {
                yield break;
            }

            if (CultUtility.AreCultObjectsAvailable(Map))
            {
                yield break;
            }

            if (CultUtility.IsSomeoneInvestigating(Map))
            {
                yield break;
            }

            if (!Map.reservationManager.CanReserve(myPawn, this))
            {
                yield break;
            }

            void action0()
            {
                var job = new Job(CultsDefOf.Cults_Investigate, myPawn, this)
                {
                    playerForced = true
                };
                myPawn.jobs.TryTakeOrderedJob(job);
                //mypawn.CurJob.EndCurrentJob(JobCondition.InterruptForced);
            }

            yield return new FloatMenuOption("Investigate", action0);
        }

        private void MuteToggle()
        {
            isMuted = !isMuted;
            if (isMuted)
            {
                var sustainer = (Sustainer) typeof(Building)
                    .GetField("sustainerAmbient", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.GetValue(this);
                sustainer?.End();
            }
            else
            {
                _ = (Sustainer) typeof(Building)
                    .GetField("sustainerAmbient", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.GetValue(this);

                var info = SoundInfo.InMap(this);
                _ = new Sustainer(def.building.soundAmbient, info);
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            using var enumerator = base.GetGizmos().GetEnumerator();
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                yield return current;
            }

            var toggleDef = new Command_Toggle
            {
                hotKey = KeyBindingDefOf.Command_TogglePower,
                icon = ContentFinder<Texture2D>.Get("UI/Icons/Commands/Mute"),
                defaultLabel = "Mute".Translate(),
                defaultDesc = "MuteDesc".Translate(),
                isActive = () => isMuted,
                toggleAction = MuteToggle
            };
            yield return toggleDef;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            // Save and load the work variables, so they don't default after loading
            Scribe_Values.Look(ref isMuted, "isMuted");
        }
    }
}