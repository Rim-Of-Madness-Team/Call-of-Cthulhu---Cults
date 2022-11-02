using System.Collections.Generic;
using Cthulhu;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace CultOfCthulhu
{
    public class PawnFlyersLeaving : Thing, IActiveDropPod, IThingHolder
    {
        private const int MinTicksSinceStart = -40;

        private const int MaxTicksSinceStart = -15;

        private const int TicksSinceStartToPlaySound = -10;

        private const int LeaveMapAfterTicks = 220;

        private static readonly List<Thing> tmpActiveDropPods = new List<Thing>();

        private bool alreadyLeft;

        public PawnsArrivalModeDef arriveMode;

        public bool attackOnArrival;

        // RimWorld.Skyfaller
        private Material cachedShadowMaterial;

        private ActiveDropPodInfo contents;

        public IntVec3 destinationCell = IntVec3.Invalid;

        public int destinationTile = -1;

        public int groupID = -1;
        public PawnFlyer pawnFlyer;

        private bool soundPlayed;

        private int ticksSinceStart;

        private PawnFlyerDef PawnFlyerDef => pawnFlyer.def as PawnFlyerDef;

        public override Vector3 DrawPos =>
            SkyfallerDrawPosUtility.DrawPos_Accelerate(center: base.DrawPos, ticksToImpact: ticksSinceStart, angle: -33f,
                speed: def.skyfaller.speed); //return DropPodAnimationUtility.DrawPosAt(this.ticksSinceStart, base.Position);

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

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outThingsHolders: outChildren, container: GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return contents.innerContainer;
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
            ticksSinceStart = Rand.RangeInclusive(min: -40, max: -15);
        }

        public override void ExposeData()
        {
            base.ExposeData();

            //PawnFlyer
            Scribe_References.Look(refee: ref pawnFlyer, label: "pawnFlyer");

            //Vanilla
            Scribe_Values.Look(value: ref groupID, label: "groupID");
            Scribe_Values.Look(value: ref destinationTile, label: "destinationTile");
            Scribe_Values.Look(value: ref destinationCell, label: "destinationCell");
            Scribe_Values.Look(value: ref arriveMode, label: "arriveMode", defaultValue: PawnsArrivalModeDefOf.EdgeDrop);
            Scribe_Values.Look(value: ref attackOnArrival, label: "attackOnArrival");
            Scribe_Values.Look(value: ref ticksSinceStart, label: "ticksSinceStart");
            Scribe_Deep.Look(target: ref contents, label: "contents", this);
            Scribe_Values.Look(value: ref alreadyLeft, label: "alreadyLeft");
            Scribe_Values.Look(value: ref soundPlayed, label: "soundPlayed");
        }

        public override void Tick()
        {
            if (!soundPlayed && ticksSinceStart >= -10)
            {
                if (PawnFlyerDef.takeOffSound != null)
                {
                    PawnFlyerDef.takeOffSound.PlayOneShot(info: new TargetInfo(cell: Position, map: Map));
                }
                else
                {
                    Log.Warning(text: "PawnFlyersLeaving :: Take off sound not set");
                }

                soundPlayed = true;
            }

            ticksSinceStart++;
            if (!alreadyLeft && ticksSinceStart >= 220)
            {
                GroupLeftMap();
            }
        }

        public override void DrawAt(Vector3 drawLoc, bool flip)
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
                    ticksToImpact: ticksSinceStart);
            }

            //DropPodAnimationUtility.DrawDropSpotShadow(this, this.ticksSinceStart);
        }

        private void GroupLeftMap()
        {
            if (groupID < 0)
            {
                Log.Error(text: "Drop pod left the map, but its group ID is " + groupID);
                Destroy();
                return;
            }

            if (destinationTile < 0)
            {
                Log.Error(text: "Drop pod left the map, but its destination tile is " + destinationTile);
                Destroy();
                return;
            }

            var lord = FindLord(transportersGroup: groupID, map: Map);
            if (lord != null)
            {
                Map.lordManager.RemoveLord(oldLord: lord);
            }

            var PawnFlyersTraveling = (PawnFlyersTraveling) WorldObjectMaker.MakeWorldObject(def: PawnFlyerDef.travelingDef);
            PawnFlyersTraveling.pawnFlyer = pawnFlyer;
            PawnFlyersTraveling.Tile = Map.Tile;
            PawnFlyersTraveling.destinationTile = destinationTile;
            PawnFlyersTraveling.destinationCell = destinationCell;
            PawnFlyersTraveling.arriveMode = arriveMode;
            PawnFlyersTraveling.attackOnArrival = attackOnArrival;
            Find.WorldObjects.Add(o: PawnFlyersTraveling);
            tmpActiveDropPods.Clear();
            tmpActiveDropPods.AddRange(collection: Map.listerThings.ThingsInGroup(@group: ThingRequestGroup.ActiveDropPod));

            foreach (var thing in tmpActiveDropPods)
            {
                if (thing is not PawnFlyersLeaving pawnFlyerLeaving || pawnFlyerLeaving.groupID != groupID)
                {
                    continue;
                }

                Utility.DebugReport(x: "Transport Already Left");
                pawnFlyerLeaving.alreadyLeft = true;
                PawnFlyersTraveling.AddPod(contents: pawnFlyerLeaving.contents, justLeftTheMap: true);
                pawnFlyerLeaving.contents = null;
                pawnFlyerLeaving.Destroy();
            }
        }

        // RimWorld.TransporterUtility
        public static Lord FindLord(int transportersGroup, Map map)
        {
            var lords = map.lordManager.lords;
            foreach (var findLord in lords)
            {
                if (findLord.LordJob is LordJob_LoadAndEnterTransportersPawn lordJob_LoadAndEnterTransporters &&
                    lordJob_LoadAndEnterTransporters.transportersGroup == transportersGroup)
                {
                    return findLord;
                }
            }

            return null;
        }
    }
}