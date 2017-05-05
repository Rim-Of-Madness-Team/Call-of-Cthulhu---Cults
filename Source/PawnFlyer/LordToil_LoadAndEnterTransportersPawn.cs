using System;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;

namespace CultOfCthulhu
{
    public class LordToil_LoadAndEnterTransportersPawn : LordToil
    {
        private int transportersGroup = -1;

        public override bool AllowSatisfyLongNeeds
        {
            get
            {
                return false;
            }
        }

        public LordToil_LoadAndEnterTransportersPawn(int transportersGroup)
        {
            Cthulhu.Utility.DebugReport("LordToil_LoadAndCenterTranpsortersPawn Called");
            this.transportersGroup = transportersGroup;
        }

        public override void UpdateAllDuties()
        {
            for (int i = 0; i < this.lord.ownedPawns.Count; i++)
            {
                PawnDuty pawnDuty = new PawnDuty(CultDefOfs.Cults_LoadAndEnterTransportersPawn);
                pawnDuty.transportersGroup = this.transportersGroup;
                this.lord.ownedPawns[i].mindState.duty = pawnDuty;
            }
        }
    }
}
