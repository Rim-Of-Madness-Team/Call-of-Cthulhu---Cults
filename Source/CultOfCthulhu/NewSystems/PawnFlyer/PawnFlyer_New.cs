using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CultOfCthulhu
{
    public class PawnFlyer_New : Pawn
    {

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }

            var command_Action = new Command_Action
            {
                action = delegate
                {
                    SoundDefOf.ThingSelected.PlayOneShotOnCamera();
                    ReplaceByakheeWithHune();
                },
                defaultLabel = "Cults_CommandByakheeHune".Translate(),
                defaultDesc = "Cults_CommandByakheeHune".Translate(),
                icon = TexCommand.Draft,
                };

            yield return command_Action;
        }


        private void ReplaceByakheeWithHune()
        {

            //Copy the important values.
            var currentLocation = Position;


            Thing pod = ThingMaker.MakeThing(ThingDef.Named("Cults_TransportByakhee"));
            pod.SetFaction(this.Faction);
            GenSpawn.Spawn(pod, currentLocation, Map);
            DeSpawn();
            pod.TryGetComp<CompTransporter>().GetDirectlyHeldThings().TryAdd(this);
            
        }

    }
}