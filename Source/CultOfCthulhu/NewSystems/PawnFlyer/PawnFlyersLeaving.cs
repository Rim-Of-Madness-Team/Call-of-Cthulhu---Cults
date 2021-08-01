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
            SkyfallerDrawPosUtility.DrawPos_Accelerate(base.DrawPos, ticksSinceStart, -33f,
                def.skyfaller.speed); //return DropPodAnimationUtility.DrawPosAt(this.ticksSinceStart, base.Position);

        // RimWorld.Skyfaller
        private Material ShadowMaterial
        {
            get
            {
                if (cachedShadowMaterial == null && !def.skyfaller.shadow.NullOrEmpty())
                {
                    cachedShadowMaterial = MaterialPool.MatFrom(def.skyfaller.shadow, ShaderDatabase.Transparent);
                }

                return cachedShadowMaterial;
            }
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
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
            ticksSinceStart = Rand.RangeInclusive(-40, -15);
        }

        public override void ExposeData()
        {
            base.ExposeData();

            //PawnFlyer
            Scribe_References.Look(ref pawnFlyer, "pawnFlyer");

            //Vanilla
            Scribe_Values.Look(ref groupID, "groupID");
            Scribe_Values.Look(ref destinationTile, "destinationTile");
            Scribe_Values.Look(ref destinationCell, "destinationCell");
            Scribe_Values.Look(ref arriveMode, "arriveMode", PawnsArrivalModeDefOf.EdgeDrop);
            Scribe_Values.Look(ref attackOnArrival, "attackOnArrival");
            Scribe_Values.Look(ref ticksSinceStart, "ticksSinceStart");
            Scribe_Deep.Look(ref contents, "contents", this);
            Scribe_Values.Look(ref alreadyLeft, "alreadyLeft");
            Scribe_Values.Look(ref soundPlayed, "soundPlayed");
        }

        public override void Tick()
        {
            if (!soundPlayed && ticksSinceStart >= -10)
            {
                if (PawnFlyerDef.takeOffSound != null)
                {
                    PawnFlyerDef.takeOffSound.PlayOneShot(new TargetInfo(Position, Map));
                }
                else
                {
                    Log.Warning("PawnFlyersLeaving :: Take off sound not set");
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
            if (!drawLoc.InBounds(Map))
            {
                return;
            }

            pawnFlyer.Drawer.DrawAt(drawLoc);
            var shadowMaterial = ShadowMaterial;
            if (!(shadowMaterial == null))
            {
                Skyfaller.DrawDropSpotShadow(base.DrawPos, Rotation, shadowMaterial, def.skyfaller.shadowSize,
                    ticksSinceStart);
            }

            //DropPodAnimationUtility.DrawDropSpotShadow(this, this.ticksSinceStart);
        }

        private void GroupLeftMap()
        {
            if (groupID < 0)
            {
                Log.Error("Drop pod left the map, but its group ID is " + groupID);
                Destroy();
                return;
            }

            if (destinationTile < 0)
            {
                Log.Error("Drop pod left the map, but its destination tile is " + destinationTile);
                Destroy();
                return;
            }

            var lord = FindLord(groupID, Map);
            if (lord != null)
            {
                Map.lordManager.RemoveLord(lord);
            }

            var PawnFlyersTraveling = (PawnFlyersTraveling) WorldObjectMaker.MakeWorldObject(PawnFlyerDef.travelingDef);
            PawnFlyersTraveling.pawnFlyer = pawnFlyer;
            PawnFlyersTraveling.Tile = Map.Tile;
            PawnFlyersTraveling.destinationTile = destinationTile;
            PawnFlyersTraveling.destinationCell = destinationCell;
            PawnFlyersTraveling.arriveMode = arriveMode;
            PawnFlyersTraveling.attackOnArrival = attackOnArrival;
            Find.WorldObjects.Add(PawnFlyersTraveling);
            tmpActiveDropPods.Clear();
            tmpActiveDropPods.AddRange(Map.listerThings.ThingsInGroup(ThingRequestGroup.ActiveDropPod));

            foreach (var thing in tmpActiveDropPods)
            {
                if (thing is not PawnFlyersLeaving pawnFlyerLeaving || pawnFlyerLeaving.groupID != groupID)
                {
                    continue;
                }

                Utility.DebugReport("Transport Already Left");
                pawnFlyerLeaving.alreadyLeft = true;
                PawnFlyersTraveling.AddPod(pawnFlyerLeaving.contents, true);
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