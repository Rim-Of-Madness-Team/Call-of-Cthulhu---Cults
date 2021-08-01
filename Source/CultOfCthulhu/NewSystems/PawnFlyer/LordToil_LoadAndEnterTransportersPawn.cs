using Cthulhu;
using Verse.AI;
using Verse.AI.Group;

namespace CultOfCthulhu
{
    public class LordToil_LoadAndEnterTransportersPawn : LordToil
    {
        private readonly int transportersGroup;

        public LordToil_LoadAndEnterTransportersPawn(int transportersGroup)
        {
            Utility.DebugReport("LordToil_LoadAndCenterTranpsortersPawn Called");
            this.transportersGroup = transportersGroup;
        }

        public override bool AllowSatisfyLongNeeds => false;

        public override void UpdateAllDuties()
        {
            foreach (var pawn in lord.ownedPawns)
            {
                var pawnDuty = new PawnDuty(CultsDefOf.Cults_LoadAndEnterTransportersPawn)
                {
                    transportersGroup = transportersGroup
                };
                pawn.mindState.duty = pawnDuty;
            }
        }
    }
}