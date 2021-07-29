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
            MoteMaker.MakePowerBeamMote(corpse.Position, corpse.Map);

            //Hurt all nearby enemy pawns.
            foreach (var cell in GenRadial.RadialCellsAround(corpse.Position, 3f, true))
            {
                var thingList = new List<Thing>(cell.GetThingList(corpse.Map));
                foreach (var thing in thingList)
                {
                    if (thing.HostileTo(corpse.InnerPawn.Faction))
                    {
                        //Damage.
                        thing.TakeDamage(new DamageInfo(DamageDefOf.Burn, 40));
                    }
                }
            }
        }
    }
}