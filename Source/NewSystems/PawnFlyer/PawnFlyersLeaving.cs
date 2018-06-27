using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;
using RimWorld;

namespace CultOfCthulhu
{
    public class PawnFlyersLeaving : Thing, IActiveDropPod, IThingHolder
    {
        public PawnFlyer pawnFlyer;

        private const int MinTicksSinceStart = -40;

        private const int MaxTicksSinceStart = -15;

        private const int TicksSinceStartToPlaySound = -10;

        private const int LeaveMapAfterTicks = 220;

        private ActiveDropPodInfo contents;

        public int groupID = -1;

        public int destinationTile = -1;

        public IntVec3 destinationCell = IntVec3.Invalid;

        public PawnsArrivalModeDef arriveMode;

        public bool attackOnArrival;

        private int ticksSinceStart;

        private bool alreadyLeft;

        private bool soundPlayed;

        private static List<Thing> tmpActiveDropPods = new List<Thing>();

        private PawnFlyerDef PawnFlyerDef
        {
            get
            {
                return pawnFlyer.def as PawnFlyerDef;
            }
        }

        public override Vector3 DrawPos
        {
            get
            {
                return SkyfallerDrawPosUtility.DrawPos_Accelerate(base.DrawPos, this.ticksSinceStart, -33f, this.def.skyfaller.speed);

                //return DropPodAnimationUtility.DrawPosAt(this.ticksSinceStart, base.Position);
            }
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return this.contents.innerContainer;
        }

        public ActiveDropPodInfo Contents
        {
            get
            {
                return this.contents;
            }
            set
            {
                if (this.contents != null)
                {
                    this.contents.parent = null;
                }
                if (value != null)
                {
                    value.parent = this;
                }
                this.contents = value;
            }
        }

        public IntVec3 GetPosition()
        {
            return base.PositionHeld;
        }

        public Map GetMap()
        {
            return base.MapHeld;
        }

        public override void PostMake()
        {
            base.PostMake();
            this.ticksSinceStart = Rand.RangeInclusive(-40, -15);
        }

        public override void ExposeData()
        {
            base.ExposeData();

            //PawnFlyer
            Scribe_References.Look<PawnFlyer>(ref this.pawnFlyer, "pawnFlyer");

            //Vanilla
            Scribe_Values.Look<int>(ref this.groupID, "groupID", 0, false);
            Scribe_Values.Look<int>(ref this.destinationTile, "destinationTile", 0, false);
            Scribe_Values.Look<IntVec3>(ref this.destinationCell, "destinationCell", default(IntVec3), false);
            Scribe_Values.Look<PawnsArrivalModeDef>(ref this.arriveMode, "arriveMode", PawnsArrivalModeDefOf.EdgeDrop, false);
            Scribe_Values.Look<bool>(ref this.attackOnArrival, "attackOnArrival", false, false);
            Scribe_Values.Look<int>(ref this.ticksSinceStart, "ticksSinceStart", 0, false);
            Scribe_Deep.Look<ActiveDropPodInfo>(ref this.contents, "contents", new object[]
            {
                this
            });
            Scribe_Values.Look<bool>(ref this.alreadyLeft, "alreadyLeft", false, false);
            Scribe_Values.Look<bool>(ref this.soundPlayed, "soundPlayed", false, false);
        }

        public override void Tick()
        {
            if (!this.soundPlayed && this.ticksSinceStart >= -10)
            {

                if (PawnFlyerDef.takeOffSound != null)
                {
                    PawnFlyerDef.takeOffSound.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
                }
                else
                {
                    Log.Warning("PawnFlyersLeaving :: Take off sound not set");
                }
                this.soundPlayed = true;
            }
            this.ticksSinceStart++;
            if (!this.alreadyLeft && this.ticksSinceStart >= 220)
            {
                this.GroupLeftMap();
            }
        }

        // RimWorld.Skyfaller
        private Material cachedShadowMaterial;

        // RimWorld.Skyfaller
        private Material ShadowMaterial
        {
            get
            {
                if (this.cachedShadowMaterial == null && !this.def.skyfaller.shadow.NullOrEmpty())
                {
                    this.cachedShadowMaterial = MaterialPool.MatFrom(this.def.skyfaller.shadow, ShaderDatabase.Transparent);
                }
                return this.cachedShadowMaterial;
            }
        }

        public override void DrawAt(Vector3 drawLoc, bool flip)
        {
            if (drawLoc.InBounds(Map))
            {
                this.pawnFlyer.Drawer.DrawAt(drawLoc);
                Material shadowMaterial = this.ShadowMaterial;
                if (!(shadowMaterial == null))
                {
                    Skyfaller.DrawDropSpotShadow(base.DrawPos, base.Rotation, shadowMaterial, this.def.skyfaller.shadowSize, this.ticksSinceStart);
                }
                //DropPodAnimationUtility.DrawDropSpotShadow(this, this.ticksSinceStart);
            }
        }

        private void GroupLeftMap()
        {

            if (this.groupID < 0)
            {
                Log.Error("Drop pod left the map, but its group ID is " + this.groupID);
                this.Destroy(DestroyMode.Vanish);
                return;
            }

            if (this.destinationTile < 0)
            {
                Log.Error("Drop pod left the map, but its destination tile is " + this.destinationTile);
                this.Destroy(DestroyMode.Vanish);
                return;
            }

            Lord lord = FindLord(this.groupID, base.Map);
            if (lord != null)
            {
                base.Map.lordManager.RemoveLord(lord);
            }

            PawnFlyersTraveling PawnFlyersTraveling = (PawnFlyersTraveling)WorldObjectMaker.MakeWorldObject(PawnFlyerDef.travelingDef);
            PawnFlyersTraveling.pawnFlyer = this.pawnFlyer;
            PawnFlyersTraveling.Tile = base.Map.Tile;
            PawnFlyersTraveling.destinationTile = this.destinationTile;
            PawnFlyersTraveling.destinationCell = this.destinationCell;
            PawnFlyersTraveling.arriveMode = this.arriveMode;
            PawnFlyersTraveling.attackOnArrival = this.attackOnArrival;
            Find.WorldObjects.Add(PawnFlyersTraveling);
            PawnFlyersLeaving.tmpActiveDropPods.Clear();
            PawnFlyersLeaving.tmpActiveDropPods.AddRange(base.Map.listerThings.ThingsInGroup(ThingRequestGroup.ActiveDropPod));

            for (int i = 0; i < PawnFlyersLeaving.tmpActiveDropPods.Count; i++)
            {
                PawnFlyersLeaving pawnFlyerLeaving = PawnFlyersLeaving.tmpActiveDropPods[i] as PawnFlyersLeaving;
                if (pawnFlyerLeaving != null && pawnFlyerLeaving.groupID == this.groupID)
                {
                    Cthulhu.Utility.DebugReport("Transport Already Left");
                    pawnFlyerLeaving.alreadyLeft = true;
                    PawnFlyersTraveling.AddPod(pawnFlyerLeaving.contents, true);
                    pawnFlyerLeaving.contents = null;
                    pawnFlyerLeaving.Destroy(DestroyMode.Vanish);
                }
            }

        }

        // RimWorld.TransporterUtility
        public static Lord FindLord(int transportersGroup, Map map)
        {
            List<Lord> lords = map.lordManager.lords;
            for (int i = 0; i < lords.Count; i++)
            {
                LordJob_LoadAndEnterTransportersPawn lordJob_LoadAndEnterTransporters = lords[i].LordJob as LordJob_LoadAndEnterTransportersPawn;
                if (lordJob_LoadAndEnterTransporters != null && lordJob_LoadAndEnterTransporters.transportersGroup == transportersGroup)
                {
                    return lords[i];
                }
            }
            return null;
        }
    }
}
