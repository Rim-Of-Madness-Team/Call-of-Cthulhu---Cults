using Verse;
using Verse.AI;

namespace CultOfCthulhu
{
    public class MentalStateWorker_Disillusioned : MentalStateWorker
    {
        public override bool StateCanOccur(Pawn pawn)
        {
            if (!base.StateCanOccur(pawn: pawn))
            {
                return false;
            }

            if (!pawn.Spawned)
            {
                return false;
            }

            var cultMind = pawn.needs.TryGetNeed<Need_CultMindedness>();
            if (cultMind == null)
            {
                return false;
            }

            return cultMind.CurLevel > 0.8;
        }
    }
}