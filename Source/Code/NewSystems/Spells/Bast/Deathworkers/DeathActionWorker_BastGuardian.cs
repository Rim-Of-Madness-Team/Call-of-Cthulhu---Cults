using System.Collections.Generic;
using RimWorld;
using Verse;

namespace BastCult
{
    /// <summary>
    ///     Death worker for Bast Guardians.
    /// </summary>
    public class DeathActionWorker_BastGuardian : DeathActionWorker
    {
        public override void PawnDied(Corpse corpse)
        {
            //Fancy death effect.
            MoteMaker.MakePowerBeamMote(cell: corpse.Position, map: corpse.Map);

            //Hurt all nearby enemy pawns.
            foreach (var cell in GenRadial.RadialCellsAround(center: corpse.Position, radius: 3f, useCenter: true))
            {
                var thingList = new List<Thing>(collection: cell.GetThingList(map: corpse.Map));
                foreach (var thing in thingList)
                {
                    if (thing.HostileTo(fac: corpse.InnerPawn.Faction))
                    {
                        //Damage.
                        thing.TakeDamage(dinfo: new DamageInfo(def: DamageDefOf.Burn, amount: 40));
                    }
                }
            }
        }
    }
}