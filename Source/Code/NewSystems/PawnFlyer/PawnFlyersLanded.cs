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
            ThingOwnerUtility.AppendThingHoldersFromThings(outThingsHolders: outChildren, container: GetDirectlyHeldThings());
            if (contents != null)
            {
                outChildren.Add(item: contents);
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
            Scribe_References.Look(refee: ref pawnFlyer, label: "pawnFlyer");

            //Vanilla
            Scribe_Values.Look(value: ref age, label: "age");
            Scribe_Deep.Look(target: ref contents, label: "contents", this);
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
            if (drawLoc.InBounds(map: Map))
            {
                pawnFlyer?.Drawer?.DrawAt(loc: drawLoc);
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
            base.Destroy(mode: mode);
            if (mode != DestroyMode.KillFinalize)
            {
                return;
            }

            for (var i = 0; i < 1; i++)
            {
                var thing = ThingMaker.MakeThing(def: ThingDefOf.ChunkSlagSteel);
                GenPlace.TryPlaceThing(thing: thing, center: Position, map: map, mode: ThingPlaceMode.Near);
            }
        }

        private void DismountAll()
        {
            if (!pawnFlyer.Spawned)
            {
                if (pawnFlyer.Destroyed)
                {
                    GenSpawn.Spawn(newThing: pawnFlyer, loc: Position, map: Map, rot: Rot4.Random);

                    Utility.DebugReport(x: "Spawned Destroyed PawnFlyer: " + pawnFlyer.Label);
                }
                else
                {
                    GenPlace.TryPlaceThing(thing: pawnFlyer, center: Position, map: Map, mode: ThingPlaceMode.Near, lastResultingThing: out _,
                        placedAction: delegate { Utility.DebugReport(x: "Successfully Spawned: " + pawnFlyer.Label); });
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

                GenPlace.TryPlaceThing(thing: thing, center: Position, map: Map, mode: ThingPlaceMode.Near, lastResultingThing: out var thing2,
                    placedAction: delegate(Thing placedThing, int _)
                    {
                        //Log.Message("3");

                        if (Find.TickManager.TicksGame < 1200 && TutorSystem.TutorialMode &&
                            placedThing.def.category == ThingCategory.Item)
                        {
                            Find.TutorialState.AddStartingItem(t: placedThing);
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
                        TaleRecorder.RecordTale(def: PawnFlyerDef.landedTale, pawn);
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
                    var thing3 = ThingMaker.MakeThing(def: ThingDefOf.ChunkSlagSteel);
                    GenPlace.TryPlaceThing(thing: thing3, center: Position, map: Map, mode: ThingPlaceMode.Near);
                }
            }

            if (PawnFlyerDef.dismountSound != null)
            {
                PawnFlyerDef.dismountSound.PlayOneShot(info: new TargetInfo(cell: Position, map: Map));
            }
            else
            {
                Log.Warning(text: "PawnFlyersLanded :: Dismount sound not set");
            }

            Destroy();
        }
    }
}