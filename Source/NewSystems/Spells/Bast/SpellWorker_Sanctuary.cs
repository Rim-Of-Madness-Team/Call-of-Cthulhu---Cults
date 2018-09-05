using CultOfCthulhu;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace BastCult
{
    /// <summary>
    /// This spell forces most enemies to flee in terror.
    /// </summary>
    public class SpellWorker_Sanctuary : SpellWorker
    {
        public override bool CanSummonNow(Map map)
        {
            if(!GenHostility.AnyHostileActiveThreatToPlayer(map))
            {
                Messages.Message("Cults_BastNoEnemiesOnMap".Translate(), MessageTypeDefOf.RejectInput);
                return false;
            }

            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = parms.target as Map;

            if (GenHostility.AnyHostileActiveThreatToPlayer(map))
            {
                //Grab all hostile targets.
                HashSet<IAttackTarget> hashSet = new HashSet<IAttackTarget>(map.attackTargetsCache.TargetsHostileToFaction(Faction.OfPlayer));

                if(hashSet != null && hashSet.Count > 0)
                {
                    foreach (IAttackTarget target in hashSet)
                    {
                        Pawn enemyPawn = target as Pawn;
                        if (enemyPawn != null && !enemyPawn.RaceProps.IsMechanoid && enemyPawn.GetStatValue(StatDefOf.PsychicSensitivity) >= 0.5f)
                        {
                            //Force panic fleeing
                            enemyPawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.PanicFlee, "Cults_BastSanctuaryEnemy".Translate(), true);
                        }
                    }
                }
            }

            return true;
        }
    }
}
