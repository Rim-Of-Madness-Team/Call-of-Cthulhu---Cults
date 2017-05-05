using System;
using Verse.AI.Group;
using RimWorld;
using Verse;

namespace CultOfCthulhu
{
    public class LordJob_DefendAndExpandWomb : LordJob
    {
        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil_DefendAndExpandWomb lordToil_DefendAndExpandWomb = new LordToil_DefendAndExpandWomb();
            lordToil_DefendAndExpandWomb.distToHiveToAttack = 10f;
            stateGraph.StartingToil = lordToil_DefendAndExpandWomb;
            LordToil_DefendAndExpandWomb lordToil_DefendAndExpandWomb2 = new LordToil_DefendAndExpandWomb();
            lordToil_DefendAndExpandWomb2.distToHiveToAttack = 32f;
            stateGraph.AddToil(lordToil_DefendAndExpandWomb2);
            Transition transition = new Transition(lordToil_DefendAndExpandWomb, lordToil_DefendAndExpandWomb2);
            transition.AddTrigger(new Trigger_PawnHarmed());
            transition.AddTrigger(new Trigger_Memo("HiveAttacked"));
            stateGraph.AddTransition(transition);
            Transition transition2 = new Transition(lordToil_DefendAndExpandWomb, lordToil_DefendAndExpandWomb2);
            transition2.canMoveToSameState = true;
            transition2.AddSource(lordToil_DefendAndExpandWomb2);
            transition2.AddTrigger(new Trigger_Memo("HiveDestroyed"));
            stateGraph.AddTransition(transition2);
            Transition transition3 = new Transition(lordToil_DefendAndExpandWomb2, lordToil_DefendAndExpandWomb);
            transition3.AddTrigger(new Trigger_TicksPassedWithoutHarm(500));
            stateGraph.AddTransition(transition3);
            return stateGraph;
        }
    }
}
