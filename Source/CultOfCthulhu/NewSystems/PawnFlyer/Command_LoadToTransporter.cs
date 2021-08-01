using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CultOfCthulhu
{
    [StaticConstructorOnStartup]
    public class Command_LoadToTransporterPawn : Command
    {
        public CompTransporterPawn transComp;

        private List<CompTransporterPawn> transporters;

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            if (transporters == null)
            {
                transporters = new List<CompTransporterPawn>();
            }

            if (!transporters.Contains(transComp))
            {
                transporters.Add(transComp);
            }

            _ = transComp.Launchable;
            foreach (var compTransporterPawn in transporters)
            {
                if (compTransporterPawn == transComp)
                {
                    continue;
                }

                if (transComp.Map.reachability.CanReach(transComp.parent.Position, compTransporterPawn.parent,
                    PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors)))
                {
                    continue;
                }

                Messages.Message("MessageTransporterUnreachable".Translate(), compTransporterPawn.parent,
                    MessageTypeDefOf.RejectInput);
                return;
            }

            Find.WindowStack.Add(new Dialog_LoadTransportersPawn(transComp.Map, transporters));
        }

        public override bool InheritInteractionsFrom(Gizmo other)
        {
            var command_LoadToTransporter = (Command_LoadToTransporterPawn) other;
            if (command_LoadToTransporter.transComp.parent.def != transComp.parent.def)
            {
                return false;
            }

            if (transporters == null)
            {
                transporters = new List<CompTransporterPawn>();
            }

            transporters.Add(command_LoadToTransporter.transComp);
            return false;
        }
    }
}