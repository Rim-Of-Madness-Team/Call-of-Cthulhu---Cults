using AbilityUser;
using RimWorld;
using UnityEngine;
using Verse;

namespace CultOfCthulhu
{
    internal class DamageWorker_PsionicBlast : DamageWorker
    {
        public Vector3 PushResult(Thing thingToPush, Thing Caster, int pushDist, out bool collision)
        {
            var origin = thingToPush.TrueCenter();
            var result = origin;
            var collisionResult = false;
            for (var i = 1; i <= pushDist; i++)
            {
                var pushDistX = i;
                var pushDistZ = i;
                if (origin.x < Caster.TrueCenter().x)
                {
                    pushDistX = -pushDistX;
                }

                if (origin.z < Caster.TrueCenter().z)
                {
                    pushDistZ = -pushDistZ;
                }

                var tempNewLoc = new Vector3(origin.x + pushDistX, 0f, origin.z + pushDistZ);
                if (tempNewLoc.ToIntVec3().Standable(Caster.Map))
                {
                    result = tempNewLoc;
                }
                else
                {
                    if (thingToPush is not Pawn)
                    {
                        continue;
                    }

                    //target.TakeDamage(new DamageInfo(DamageDefOf.Blunt, Rand.Range(3, 6), -1, null, null, null));
                    collisionResult = true;
                    break;
                }
            }

            collision = collisionResult;
            return result;
        }

        public void PushEffect(Thing target, Thing instigator, int distance, bool damageOnCollision = false)
        {
            var Caster = instigator as Pawn;
            if (target is not Pawn)
            {
                return;
            }

            var loc = PushResult(target, Caster, distance, out var applyDamage);
            //if (((Pawn)target).RaceProps.Humanlike) ((Pawn)target).needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("PJ_ThoughtPush"), null);
            var flyingObject = (FlyingObject) GenSpawn.Spawn(ThingDef.Named("Cults_PFlyingObject"), target.Position,
                target.Map);
            if (applyDamage && damageOnCollision)
            {
                flyingObject.Launch(Caster, new LocalTargetInfo(loc.ToIntVec3()), target,
                    new DamageInfo(DamageDefOf.Blunt, Rand.Range(8, 10)));
            }
            else
            {
                flyingObject.Launch(Caster, new LocalTargetInfo(loc.ToIntVec3()), target);
            }
        }


        public override DamageResult Apply(DamageInfo dinfo, Thing victim)
        {
            PushEffect(victim, dinfo.Instigator, Rand.Range(5, 8), true);
            return new DamageResult();
        }
    }
}