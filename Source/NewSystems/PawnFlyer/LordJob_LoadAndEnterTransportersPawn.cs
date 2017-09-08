using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;
using RimWorld;

namespace CultOfCthulhu
{

    public class LordJob_LoadAndEnterTransportersPawn : LordJob
    {
        public int transportersGroup = -1;

        public LordJob_LoadAndEnterTransportersPawn()
        {
            Cthulhu.Utility.DebugReport("LoadAndEnterTransportersPawn LordJob Constructed");
        }

        public LordJob_LoadAndEnterTransportersPawn(int transportersGroup)
        {
            Cthulhu.Utility.DebugReport("LoadAndEnterTransportersPawn LordJob Constructed");
            this.transportersGroup = transportersGroup;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look<int>(ref this.transportersGroup, "transportersGroup", 0, false);
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil_LoadAndEnterTransportersPawn lordToil_LoadAndEnterTransporters = new LordToil_LoadAndEnterTransportersPawn(this.transportersGroup);
            stateGraph.StartingToil = lordToil_LoadAndEnterTransporters;
            LordToil_End lordToil_End = new LordToil_End();
            stateGraph.AddToil(lordToil_End);
            Transition transition = new Transition(lordToil_LoadAndEnterTransporters, lordToil_End);
            transition.AddTrigger(new Trigger_PawnLost());
            //transition.AddPreAction(new TransitionAction_Message("MessageFailedToLoadTransportersBecauseColonistLost".Translate(), MessageSound.Negative));
            transition.AddPreAction(new TransitionAction_Custom(new Action(this.CancelLoadingProcess)));
            stateGraph.AddTransition(transition);
            return stateGraph;
        }

        private void CancelLoadingProcess()
        {
            List<Thing> list = this.lord.Map.listerThings.ThingsInGroup(ThingRequestGroup.Pawn);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null)
                {
                    if (list[i] is Pawn || list[i] is PawnFlyer)
                    {
                        CompTransporterPawn compTransporter = list[i].TryGetComp<CompTransporterPawn>();
                        if (compTransporter != null)
                        {
                            if (compTransporter.groupID == this.transportersGroup)
                            {
                                compTransporter.CancelLoad();
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
