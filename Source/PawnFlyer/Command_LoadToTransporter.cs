using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

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
            if (this.transporters == null)
            {
                this.transporters = new List<CompTransporterPawn>();
            }
            if (!this.transporters.Contains(this.transComp))
            {
                this.transporters.Add(this.transComp);
            }
            CompLaunchablePawn launchable = this.transComp.Launchable;
            for (int j = 0; j < this.transporters.Count; j++)
            {
                if (this.transporters[j] != this.transComp)
                {
                    if (!this.transComp.Map.reachability.CanReach(this.transComp.parent.Position, this.transporters[j].parent, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false)))
                    {
                        Messages.Message("MessageTransporterUnreachable".Translate(), this.transporters[j].parent, MessageSound.RejectInput);
                        return;
                    }
                }
            }
            Find.WindowStack.Add(new Dialog_LoadTransportersPawn(this.transComp.Map, this.transporters));
        }

        public override bool InheritInteractionsFrom(Gizmo other)
        {
            Command_LoadToTransporterPawn command_LoadToTransporter = (Command_LoadToTransporterPawn)other;
            if (command_LoadToTransporter.transComp.parent.def != this.transComp.parent.def)
            {
                return false;
            }
            if (this.transporters == null)
            {
                this.transporters = new List<CompTransporterPawn>();
            }
            this.transporters.Add(command_LoadToTransporter.transComp);
            return false;
        }
    }
}
