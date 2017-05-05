﻿using System;
using Verse;
using Verse.Sound;
using RimWorld;
using UnityEngine;

namespace CultOfCthulhu
{
    public class PawnFlyersLanded : Thing, IActiveDropPod, IThingContainerOwner
    {
        public int age;

        public PawnFlyer pawnFlyer;

        public PawnFlyerDef PawnFlyerDef
        {
            get
            {
                return pawnFlyer.def as PawnFlyerDef;
            }
        }

        private ActiveDropPodInfo contents;

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

        public override void ExposeData()
        {
            base.ExposeData();
            //Pawn
            Scribe_References.LookReference<PawnFlyer>(ref this.pawnFlyer, "pawnFlyer");

            //Vanilla
            Scribe_Values.LookValue<int>(ref this.age, "age", 0, false);
            Scribe_Deep.LookDeep<ActiveDropPodInfo>(ref this.contents, "contents", new object[]
            {
                this
            });
        }

        public ThingContainer GetInnerContainer()
        {
            return this.contents.innerContainer;
        }

        public IntVec3 GetPosition()
        {
            return base.PositionHeld;
        }

        public Map GetMap()
        {
            return base.MapHeld;
        }

        public override void DrawAt(Vector3 drawLoc)
        {
            if (drawLoc.InBounds(Map))
            {
                this.pawnFlyer.Drawer.DrawAt(drawLoc);
            }
        }

        public override void Tick()
        {
            this.age++;
            if (this.age > this.contents.openDelay)
            {
                this.DismountAll();
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            this.contents.innerContainer.ClearAndDestroyContents(DestroyMode.Vanish);
            Map map = base.Map;
            base.Destroy(mode);
            if (mode == DestroyMode.Kill)
            {
                for (int i = 0; i < 1; i++)
                {
                    Thing thing = ThingMaker.MakeThing(ThingDefOf.ChunkSlagSteel, null);
                    GenPlace.TryPlaceThing(thing, base.Position, map, ThingPlaceMode.Near, null);
                }
            }
        }

        private void DismountAll()
        {
            if (!this.pawnFlyer.Spawned)
            {
                if (this.pawnFlyer.Destroyed)
                {
                    GenSpawn.Spawn(this.pawnFlyer, base.Position, base.Map, Rot4.Random);

                    Cthulhu.Utility.DebugReport("Spawned Destroyed PawnFlyer: " + pawnFlyer.Label);
                }
                else
                {
                    Thing pawnFlyer2;
                    GenPlace.TryPlaceThing(this.pawnFlyer, base.Position, base.Map, ThingPlaceMode.Near, out pawnFlyer2, delegate (Thing placedThing, int count)
                    {
                        Cthulhu.Utility.DebugReport("Successfully Spawned: " + pawnFlyer.Label);
                    });
                }
            }
            for (int i = 0; i < this.contents.innerContainer.Count; i++)
            {
                Thing thing = this.contents.innerContainer[i];
                if (thing == this.pawnFlyer) continue; //Avoid errors. We already spawned our pawnFlyer.

                Thing thing2;
                GenPlace.TryPlaceThing(thing, base.Position, base.Map, ThingPlaceMode.Near, out thing2, delegate (Thing placedThing, int count)
                {
                    if (Find.TickManager.TicksGame < 1200 && TutorSystem.TutorialMode && placedThing.def.category == ThingCategory.Item)
                    {
                        Find.TutorialState.AddStartingItem(placedThing);
                    }
                });
                Pawn pawn = thing2 as Pawn;
                if (pawn != null)
                {
                    if (!pawn.IsPrisoner)
                    {
                        if (pawn.Faction != pawnFlyer.Faction)
                            pawn.SetFaction(pawnFlyer.Faction);
                    }
                    if (pawn.RaceProps.Humanlike)
                    {
                        if (PawnFlyerDef.landedTale != null)
                        {
                            TaleRecorder.RecordTale(PawnFlyerDef.landedTale, new object[]
                            {
                            pawn
                            });
                        }
                    }
                    if (pawn.IsColonist && pawn.Spawned && !base.Map.IsPlayerHome)
                    {
                        pawn.drafter.Drafted = true;
                    }
                }
            }
            this.contents.innerContainer.Clear();
            if (this.contents.leaveSlag)
            {
                for (int j = 0; j < 1; j++)
                {
                    Thing thing3 = ThingMaker.MakeThing(ThingDefOf.ChunkSlagSteel, null);
                    GenPlace.TryPlaceThing(thing3, base.Position, base.Map, ThingPlaceMode.Near, null);
                }
            }
            if (PawnFlyerDef.dismountSound != null)
            {
                PawnFlyerDef.dismountSound.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
            }
            else
            {
                Log.Warning("PawnFlyersLanded :: Dismount sound not set");
            }
            this.Destroy(DestroyMode.Vanish);
        }
    }
}
