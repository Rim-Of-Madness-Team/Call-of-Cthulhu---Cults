using System.Collections.Generic;
using Cthulhu;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CultOfCthulhu
{
    public class PawnFlyersLanded : Thing, IActiveDropPod, IThingHolder
    {
        public int age;

        private ActiveDropPodInfo contents;

        public PawnFlyer pawnFlyer;

        public PawnFlyerDef PawnFlyerDef => pawnFlyer.def as PawnFlyerDef;

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
            if (contents != null)
            {
                outChildren.Add(contents);
            }
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return null;
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

        public override void ExposeData()
        {
            base.ExposeData();
            //Pawn
            Scribe_References.Look(ref pawnFlyer, "pawnFlyer");

            //Vanilla
            Scribe_Values.Look(ref age, "age");
            Scribe_Deep.Look(ref contents, "contents", this);
        }

        public IntVec3 GetPosition()
        {
            return PositionHeld;
        }

        public Map GetMap()
        {
            return MapHeld;
        }

        public override void DrawAt(Vector3 drawLoc, bool flipped)
        {
            if (drawLoc.InBounds(Map))
            {
                pawnFlyer?.Drawer?.DrawAt(drawLoc);
            }
        }

        public override void Tick()
        {
            age++;
            if (contents != null && (int) contents?.openDelay > -1 && age > contents.openDelay)
            {
                DismountAll();
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            contents?.innerContainer?.ClearAndDestroyContents();
            var map = Map;
            base.Destroy(mode);
            if (mode != DestroyMode.KillFinalize)
            {
                return;
            }

            for (var i = 0; i < 1; i++)
            {
                var thing = ThingMaker.MakeThing(ThingDefOf.ChunkSlagSteel);
                GenPlace.TryPlaceThing(thing, Position, map, ThingPlaceMode.Near);
            }
        }

        private void DismountAll()
        {
            if (!pawnFlyer.Spawned)
            {
                if (pawnFlyer.Destroyed)
                {
                    GenSpawn.Spawn(pawnFlyer, Position, Map, Rot4.Random);

                    Utility.DebugReport("Spawned Destroyed PawnFlyer: " + pawnFlyer.Label);
                }
                else
                {
                    GenPlace.TryPlaceThing(pawnFlyer, Position, Map, ThingPlaceMode.Near, out _,
                        delegate { Utility.DebugReport("Successfully Spawned: " + pawnFlyer.Label); });
                }
            }

            foreach (var thing in contents.innerContainer.InRandomOrder())
            {
                //Log.Message("1");
                if (thing.Spawned)
                {
                    continue; //Avoid errors. We already spawned our pawnFlyer.
                }
                //Log.Message("2");

                //this.contents.innerContainer.TryDrop(thing, ThingPlaceMode.Near, out thing2);

                GenPlace.TryPlaceThing(thing, Position, Map, ThingPlaceMode.Near, out var thing2,
                    delegate(Thing placedThing, int _)
                    {
                        //Log.Message("3");

                        if (Find.TickManager.TicksGame < 1200 && TutorSystem.TutorialMode &&
                            placedThing.def.category == ThingCategory.Item)
                        {
                            Find.TutorialState.AddStartingItem(placedThing);
                        }
                    });
                //Log.Message("4");

                if (thing2 is not Pawn pawn)
                {
                    continue;
                }
                //Log.Message("5");

                //if (!pawn.IsPrisoner)
                //{
                //    if (pawn.Faction != pawnFlyer.Faction)
                //        pawn.SetFaction(pawnFlyer.Faction);
                //}
                if (pawn.RaceProps.Humanlike)
                {
                    if (PawnFlyerDef.landedTale != null)
                    {
                        TaleRecorder.RecordTale(PawnFlyerDef.landedTale, pawn);
                    }
                }

                if (pawn.IsColonist && pawn.Spawned && !Map.IsPlayerHome)
                {
                    pawn.drafter.Drafted = true;
                }
            }

            if (contents.leaveSlag)
            {
                for (var j = 0; j < 1; j++)
                {
                    var thing3 = ThingMaker.MakeThing(ThingDefOf.ChunkSlagSteel);
                    GenPlace.TryPlaceThing(thing3, Position, Map, ThingPlaceMode.Near);
                }
            }

            if (PawnFlyerDef.dismountSound != null)
            {
                PawnFlyerDef.dismountSound.PlayOneShot(new TargetInfo(Position, Map));
            }
            else
            {
                Log.Warning("PawnFlyersLanded :: Dismount sound not set");
            }

            Destroy();
        }
    }
}