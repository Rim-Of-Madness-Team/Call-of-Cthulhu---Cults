using System.Collections.Generic;
using System.Linq;
using Cthulhu;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CultOfCthulhu
{
    public class PawnFlyersIncoming : Skyfaller, IActiveDropPod, IThingHolder
    {
        public ActiveDropPodInfo Contents
        {
            get { return ((ActiveDropPod)this.innerContainer[0]).Contents; }
            set { ((ActiveDropPod)this.innerContainer[0]).Contents = value; }
        }

        protected override void SpawnThings()
        {
            if (this.Contents.spawnWipeMode == null)
            {
                //Skyfaller.SpawnThings()
                for (int i = this.innerContainer.Count - 1; i >= 0; i--)
                {
                    GenPlace.TryPlaceThing(this.innerContainer[i], base.Position, base.Map, ThingPlaceMode.Near,
                        delegate(Thing thing, int count)
                        {
                            if (thing is PawnFlyer pawnFlyer)
                                pawnFlyer.mindState.Active = true;
                            PawnUtility.RecoverFromUnwalkablePositionOrKill(thing.Position, thing.Map);
                            if (thing.def.Fillage == FillCategory.Full && this.def.skyfaller.CausesExplosion &&
                                this.def.skyfaller.explosionDamage.isExplosive &&
                                thing.Position.InHorDistOf(base.Position, this.def.skyfaller.explosionRadius))
                            {
                                base.Map.terrainGrid.Notify_TerrainDestroyed(thing.Position);
                            }
                        }, null, this.innerContainer[i].def.defaultPlacingRot);
                    return;
                }
            }

            for (int i = this.innerContainer.Count - 1; i >= 0; i--)
            {
                GenSpawn.Spawn(this.innerContainer[i], base.Position, base.Map, this.Contents.spawnWipeMode.Value);
                if (this.innerContainer[i] is PawnFlyer pawnFlyer)
                    pawnFlyer.mindState.Active = true;
            }
        }

        protected override void Impact()
        {
            for (int i = 0; i < 6; i++)
            {
                FleckMaker.ThrowDustPuff(base.Position.ToVector3Shifted() + Gen.RandomHorizontalVector(1f), base.Map,
                    1.2f);
            }

            FleckMaker.ThrowLightningGlow(base.Position.ToVector3Shifted(), base.Map, 2f);
            GenClamor.DoClamor(this, 15f, ClamorDefOf.Impact);
            base.Impact();
        }
    }
}