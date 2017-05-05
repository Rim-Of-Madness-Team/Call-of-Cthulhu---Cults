using RimWorld;
using RimWorld.Planet;
using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace CthulhuFactions
{ 
    [StaticConstructorOnStartup]
    public class PawnComponentInjectorBehavior : MonoBehaviour
    {
        static PawnComponentInjectorBehavior()
        {
            GameObject initializer = new UnityEngine.GameObject("AgencyPawnCompInjector");
            initializer.AddComponent<PawnComponentInjectorBehavior>();
            UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)initializer);
        }


        protected bool reinjectNeeded = false;
        protected float reinjectTime = 0;
        int lastTicks;

        public void OnLevelWasLoaded(int level)
        {
            reinjectNeeded = true;
            if (level >= 0)
            {
                reinjectTime = 1;
            }
            else
            {
                reinjectTime = 0;
            }
            lastTicks = 0;
        }

        public void FixedUpdate()
        {
            if (Find.TickManager.TicksGame > lastTicks+10)
            {
                lastTicks = Find.TickManager.TicksGame;
                if (reinjectNeeded)
                {
                    reinjectTime -= Time.fixedDeltaTime;
                    if (reinjectTime <= 0)
                    {
                        reinjectTime = 0;
                        Find.Maps.ForEach(delegate (Map map)
                        {
                            map.mapPawns.AllPawnsSpawned.Where(
                                (Pawn p) => p.Name != null &&  p.TryGetComp<PawnComponent_Agency>() == null && p.Faction != null && 
                                p.Faction.def.defName.EqualsIgnoreCase("TheAgency")).ToList().ForEach(
                            delegate (Pawn p)
                            {
                                PawnComponent_Agency pca = new PawnComponent_Agency();
                                pca.parent = p;
                                p.AllComps.Add(pca);
                            });
                        });
                    }
                }
            }
        }

        public void Start()
        {
            OnLevelWasLoaded(-1);
        }
    }
}