using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace BastCult
{
    /// <summary>
    /// Death worker for Bast Guardians.
    /// </summary>
    public class DeathActionWorker_BastGuardian : DeathActionWorker
    {
        public override void PawnDied(Corpse corpse)
        {
            //Fancy death effect.
            MoteMaker.MakePowerBeamMote(corpse.Position, corpse.Map);

            //Hurt all nearby enemy pawns.
            foreach(IntVec3 cell in GenRadial.RadialCellsAround(corpse.Position, 3f, true))
            {
                List<Thing> thingList = new List<Thing>(cell.GetThingList(corpse.Map));
                foreach (Thing thing in thingList)
                {
                    if(GenHostility.HostileTo(thing, corpse.InnerPawn.Faction))
                    {
                        //Damage.
                        thing.TakeDamage(new DamageInfo(DamageDefOf.Burn, 40));
                    }
                }
            }
        }
    }
}
