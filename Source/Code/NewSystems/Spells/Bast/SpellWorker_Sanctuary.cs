using System.Collections.Generic;
using CultOfCthulhu;
using RimWorld;
using Verse;
using Verse.AI;

namespace BastCult
{
    /// <summary>
    ///     This spell forces most enemies to flee in terror.
    /// </summary>
    public class SpellWorker_Sanctuary : SpellWorker
    {
        public override bool CanSummonNow(Map map)
        {
            if (GenHostility.AnyHostileActiveThreatToPlayer(map: map))
            {
                return true;
            }

            Messages.Message(text: "Cults_BastNoEnemiesOnMap".Translate(), def: MessageTypeDefOf.RejectInput);
            return false;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var map = parms.target as Map;

            if (!GenHostility.AnyHostileActiveThreatToPlayer(map: map))
            {
                return true;
            }

            //Grab all hostile targets.
            var hashSet =
                new HashSet<IAttackTarget>(collection: map?.attackTargetsCache.TargetsHostileToFaction(f: Faction.OfPlayer)!);

            if (hashSet.Count <= 0)
            {
                return true;
            }

            foreach (var target in hashSet)
            {
                if (target is Pawn enemyPawn && !enemyPawn.RaceProps.IsMechanoid &&
                    enemyPawn.GetStatValue(stat: StatDefOf.PsychicSensitivity) >= 0.5f)
                {
                    //Force panic fleeing
                    enemyPawn.mindState.mentalStateHandler.TryStartMentalState(stateDef: MentalStateDefOf.PanicFlee,
                        reason: "Cults_BastSanctuaryEnemy".Translate(), forceWake: true);
                }
            }

            return true;
        }
    }
}