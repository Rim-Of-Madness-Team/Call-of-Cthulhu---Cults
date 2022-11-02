using System.Collections.Generic;
using System.Linq;
using Cthulhu;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CultOfCthulhu
{
    public class PawnFlyersIncoming : Thing, IActiveDropPod, IThingHolder
    {
        protected const int MinTicksToImpact = 120;

        protected const int MaxTicksToImpact = 200;

        protected const int RoofHitPreDelay = 15;

        private const int SoundAnticipationTicks = 100;

        private float angle;

        // RimWorld.Skyfaller
        private Material cachedShadowMaterial;

        private ActiveDropPodInfo contents;
        public PawnFlyer pawnFlyer;

        private bool soundPlayed;

        protected int ticksToImpact = 120;

        public override Vector3 DrawPos
        {
            get
            {
                //switch (this.def.skyfaller.movementType)
                //{
                //    case SkyfallerMovementType.Accelerate:
                //        result = SkyfallerDrawPosUtility.DrawPos_Accelerate(base.DrawPos, this.ticksToImpact, this.angle, this.def.skyfaller.speed);
                //        break;
                //    case SkyfallerMovementType.ConstantSpeed:
                //        result = SkyfallerDrawPosUtility.DrawPos_ConstantSpeed(base.DrawPos, this.ticksToImpact, this.angle, this.def.skyfaller.speed);
                //        break;
                //    case SkyfallerMovementType.Decelerate:
                //        result = SkyfallerDrawPosUtility.DrawPos_Decelerate(base.DrawPos, this.ticksToImpact, this.angle, this.def.skyfaller.speed);
                //        break;
                //    default:
                //        Log.ErrorOnce("SkyfallerMovementType not handled: " + this.def.skyfaller.movementType, this.thingIDNumber ^ 1948576711);
                var result = SkyfallerDrawPosUtility.DrawPos_Accelerate(center: base.DrawPos, ticksToImpact: ticksToImpact, angle: angle,
                    speed: def.skyfaller.speed);
                //break;
                //}
                return result;
                //return DropPodAnimationUtility.DrawPosAt(this.ticksToImpact, base.Position);
            }
        }

        private PawnFlyerDef PawnFlyerDef => pawnFlyer.def as PawnFlyerDef;

        // RimWorld.Skyfaller
        private Material ShadowMaterial
        {
            get
            {
                if (cachedShadowMaterial == null && !def.skyfaller.shadow.NullOrEmpty())
                {
                    cachedShadowMaterial = MaterialPool.MatFrom(texPath: def.skyfaller.shadow, shader: ShaderDatabase.Transparent);
                }

                return cachedShadowMaterial;
            }
        }

        public ActiveDropPodInfo Contents
        {
            get => contents;
            set
            {
                if (contents != null)
                {
                    contents.parent = null;
                }

                if (value != null)
                {
                    value.parent = this;
                }

                contents = value;
            }
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outThingsHolders: outChildren, container: GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return contents.innerContainer;
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map: map, respawningAfterLoad: respawningAfterLoad);
            // RimWorld.Skyfaller
            base.SpawnSetup(map: map, respawningAfterLoad: respawningAfterLoad);
            if (respawningAfterLoad)
            {
                return;
            }

            ticksToImpact = def.skyfaller.ticksToImpactRange.RandomInRange;
            angle = -33.7f;
            if (def.rotatable && this.TryGetInnerInteractableThingOwner().Any)
            {
                Rotation = this.TryGetInnerInteractableThingOwner()[index: 0].Rotation;
            }
        }

        public IntVec3 GetPosition()
        {
            return PositionHeld;
        }

        public Map GetMap()
        {
            return MapHeld;
        }

        public override void PostMake()
        {
            base.PostMake();
            ticksToImpact = Rand.RangeInclusive(min: 120, max: 200);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            //PawnFlyer
            Scribe_References.Look(refee: ref pawnFlyer, label: "pawnFlyer");

            //Vanilla
            Scribe_Values.Look(value: ref ticksToImpact, label: "ticksToImpact");
            Scribe_Deep.Look(target: ref contents, label: "contents", this);
        }

        public override void Tick()
        {
            ticksToImpact--;
            if (ticksToImpact == 15)
            {
                HitRoof();
            }

            if (ticksToImpact <= 0)
            {
                Impact();
            }

            if (soundPlayed || ticksToImpact >= 100)
            {
                return;
            }

            soundPlayed = true;


            if (PawnFlyerDef.landingSound != null)
            {
                PawnFlyerDef.landingSound.PlayOneShot(info: new TargetInfo(cell: Position, map: Map));
            }
            else
            {
                Log.Warning(text: "PawnFlyersIncoming :: Landing sound not set");
            }
        }

        private void HitRoof()
        {
            if (!Position.Roofed(map: Map))
            {
                return;
            }

            RoofCollapserImmediate.DropRoofInCells(cells: this.OccupiedRect().ExpandedBy(dist: 1).Cells.Where(predicate: delegate(IntVec3 c)
            {
                if (!c.InBounds(map: Map))
                {
                    return false;
                }

                if (c == Position)
                {
                    return true;
                }

                if (Map.thingGrid.CellContains(c: c, cat: ThingCategory.Pawn))
                {
                    return false;
                }

                var edifice = c.GetEdifice(map: Map);
                return edifice == null || !edifice.def.holdsRoof;
            }), map: Map);
        }


        public override void DrawAt(Vector3 drawLoc, bool flipped)
        {
            if (!drawLoc.InBounds(map: Map))
            {
                return;
            }

            pawnFlyer.Drawer.DrawAt(loc: drawLoc);

            var shadowMaterial = ShadowMaterial;
            if (!(shadowMaterial == null))
            {
                Skyfaller.DrawDropSpotShadow(center: base.DrawPos, rot: Rotation, material: shadowMaterial, shadowSize: def.skyfaller.shadowSize,
                    ticksToImpact: ticksToImpact);
            }

            //DropPodAnimationUtility.DrawDropSpotShadow(this, this.ticksToImpact);
        }

        private void Impact()
        {
            Utility.DebugReport(x: "Impacted Called");
            for (var i = 0; i < 6; i++)
            {
                var loc = Position.ToVector3Shifted() + Gen.RandomHorizontalVector(max: 1f);
                FleckMaker.ThrowDustPuff(loc: loc, map: Map, scale: 1.2f);
            }

            FleckMaker.ThrowLightningGlow(loc: Position.ToVector3Shifted(), map: Map, size: 2f);
            var pawnFlyerLanded = (PawnFlyersLanded) ThingMaker.MakeThing(def: PawnFlyerDef.landedDef);
            pawnFlyerLanded.pawnFlyer = pawnFlyer;
            pawnFlyerLanded.Contents = contents;
            if (!pawnFlyerLanded.Contents.innerContainer.Contains(item: pawnFlyer))
            {
                pawnFlyerLanded.Contents.innerContainer.TryAdd(item: pawnFlyer);
            }

            GenSpawn.Spawn(newThing: pawnFlyerLanded, loc: Position, map: Map, rot: Rotation);
            var roof = Position.GetRoof(map: Map);
            if (roof != null)
            {
                if (!roof.soundPunchThrough.NullOrUndefined())
                {
                    roof.soundPunchThrough.PlayOneShot(info: new TargetInfo(cell: Position, map: Map));
                }

                if (roof.filthLeaving != null)
                {
                    for (var j = 0; j < 3; j++)
                    {
                        FilthMaker.TryMakeFilth(c: Position, map: Map, filthDef: roof.filthLeaving);
                    }
                }
            }

            Destroy();
        }
    }
}