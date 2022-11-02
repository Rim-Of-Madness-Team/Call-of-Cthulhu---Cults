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
            base.ProcessInput(ev: ev);
            if (transporters == null)
            {
                transporters = new List<CompTransporterPawn>();
            }

            if (!transporters.Contains(item: transComp))
            {
                transporters.Add(item: transComp);
            }

            _ = transComp.Launchable;
            foreach (var compTransporterPawn in transporters)
            {
                if (compTransporterPawn == transComp)
                {
                    continue;
                }

                if (transComp.Map.reachability.CanReach(start: transComp.parent.Position, dest: compTransporterPawn.parent,
                    peMode: PathEndMode.Touch, traverseParams: TraverseParms.For(mode: TraverseMode.PassDoors)))
                {
                    continue;
                }

                Messages.Message(text: "MessageTransporterUnreachable".Translate(), lookTargets: compTransporterPawn.parent,
                    def: MessageTypeDefOf.RejectInput);
                return;
            }

            Find.WindowStack.Add(window: new Dialog_LoadTransportersPawn(map: transComp.Map, transporters: transporters));
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

            transporters.Add(item: command_LoadToTransporter.transComp);
            return false;
        }
    }
}