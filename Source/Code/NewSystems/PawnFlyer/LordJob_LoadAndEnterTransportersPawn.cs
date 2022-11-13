using Cthulhu;
using Verse;
using Verse.AI.Group;

namespace CultOfCthulhu
{
    public class LordJob_LoadAndEnterTransportersPawn : LordJob
    {
        public int transportersGroup = -1;

        public LordJob_LoadAndEnterTransportersPawn()
        {
            Utility.DebugReport(x: "LoadAndEnterTransportersPawn LordJob Constructed");
        }

        public LordJob_LoadAndEnterTransportersPawn(int transportersGroup)
        {
            Utility.DebugReport(x: "LoadAndEnterTransportersPawn LordJob Constructed");
            this.transportersGroup = transportersGroup;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(value: ref transportersGroup, label: "transportersGroup");
        }

        public override StateGraph CreateGraph()
        {
            var stateGraph = new StateGraph();
            var lordToil_LoadAndEnterTransporters = new LordToil_LoadAndEnterTransportersPawn(transportersGroup: transportersGroup);
            stateGraph.StartingToil = lordToil_LoadAndEnterTransporters;
            var lordToil_End = new LordToil_End();
            stateGraph.AddToil(toil: lordToil_End);
            var transition = new Transition(firstSource: lordToil_LoadAndEnterTransporters, target: lordToil_End);
            transition.AddTrigger(trigger: new Trigger_PawnLost());
            transition.AddPreAction(action: new TransitionAction_Custom(action: CancelLoadingProcess));
            stateGraph.AddTransition(transition: transition);
            return stateGraph;
        }

        private void CancelLoadingProcess()
        {
            var list = lord.Map.listerThings.ThingsInGroup(@group: ThingRequestGroup.Pawn);
            foreach (var thing in list)
            {
                if (thing == null)
                {
                    continue;
                }

                if (thing is not Pawn)
                {
                    continue;
                }

                var compTransporter = thing.TryGetComp<CompTransporterPawn>();
                if (compTransporter == null)
                {
                    continue;
                }

                if (compTransporter.groupID != transportersGroup)
                {
                    continue;
                }

                compTransporter.CancelLoad();
                break;
            }
        }
    }
}