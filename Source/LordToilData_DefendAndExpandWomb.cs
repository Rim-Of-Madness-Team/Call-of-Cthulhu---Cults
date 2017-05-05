using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;
using RimWorld;

namespace CultOfCthulhu
{
    public class LordToilData_DefendAndExpandWomb : LordToilData
    {
        public Dictionary<Pawn, WombBetweenWorlds> assignedHives = new Dictionary<Pawn, WombBetweenWorlds>();

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                this.assignedHives.RemoveAll((KeyValuePair<Pawn, WombBetweenWorlds> x) => x.Key.Destroyed);
            }
            Scribe_Collections.LookDictionary<Pawn, WombBetweenWorlds>(ref this.assignedHives, "assignedHives", LookMode.MapReference, LookMode.MapReference);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                this.assignedHives.RemoveAll((KeyValuePair<Pawn, WombBetweenWorlds> x) => x.Value == null);
            }
        }
    }
}
