using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using AbilityUser;
using UnityEngine;

namespace CultOfCthulhu
{
    class DamageWorker_PsionicBlast : DamageWorker
    {
        public Vector3 PushResult(Thing thingToPush, Thing Caster, int pushDist, out bool collision)
        {
            Vector3 origin = thingToPush.TrueCenter();
            Vector3 result = origin;
            bool collisionResult = false;
            for (int i = 1; i <= pushDist; i++)
            {
                int pushDistX = i;
                int pushDistZ = i;
                if (origin.x < Caster.TrueCenter().x) pushDistX = -pushDistX;
                if (origin.z < Caster.TrueCenter().z) pushDistZ = -pushDistZ;
                Vector3 tempNewLoc = new Vector3(origin.x + pushDistX, 0f, origin.z + pushDistZ);
                if (GenGrid.Standable(tempNewLoc.ToIntVec3(), Caster.Map))
                {
                    result = tempNewLoc;
                }
                else
                {
                    if (thingToPush is Pawn)
                    {
                        //target.TakeDamage(new DamageInfo(DamageDefOf.Blunt, Rand.Range(3, 6), -1, null, null, null));
                        collisionResult = true;
                        break;
                    }
                }
            }
            collision = collisionResult;
            return result;
        }

        public void PushEffect(Thing target, Thing instigator, int distance, bool damageOnCollision = false)
        {
            Pawn Caster = instigator as Pawn;
            if (target != null && target is Pawn)
            {
                bool applyDamage;
                Vector3 loc = PushResult(target, Caster, distance, out applyDamage);
                //if (((Pawn)target).RaceProps.Humanlike) ((Pawn)target).needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("PJ_ThoughtPush"), null);
                FlyingObject flyingObject = (FlyingObject)GenSpawn.Spawn(ThingDef.Named("Cults_PFlyingObject"), target.Position, target.Map);
                if (applyDamage && damageOnCollision) flyingObject.Launch(Caster, new LocalTargetInfo(loc.ToIntVec3()), target, new DamageInfo(DamageDefOf.Blunt, Rand.Range(8, 10)));
                else flyingObject.Launch(Caster, new LocalTargetInfo(loc.ToIntVec3()), target);
            }
        }


        public override float Apply(DamageInfo dinfo, Thing victim)
        {
            PushEffect(victim, dinfo.Instigator, Rand.Range(5, 8), true);
            return 0f;
        }
    }
}
