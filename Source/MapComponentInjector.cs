using CultOfCthulhu;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Cthulhu
{
    [StaticConstructorOnStartup]
    public class MapComponentInjectorBehavior : MonoBehaviour
    {
        static MapComponentInjectorBehavior()
        {
            GameObject initializer = new UnityEngine.GameObject("JecrellMapCompInjector");
            initializer.AddComponent<MapComponentInjectorBehavior>();
            UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object) initializer);
            mapComponents = new List<Type>();
            typeof(MapComponentInjectorBehavior).Assembly.GetTypes()
                .Where((Type t) => t.IsClass && t.IsSubclassOf(typeof(MapComponent))).ToList()
                .ForEach((Type t) => mapComponents.Add(t));
            //mapComponents.ForEach((Type t) => Log.Message(t.Name + "found for MapComponentInjector"));
        }

        protected float reinjectTime = 0;
        protected bool monstrousDefsAdded = false;
        int lastTicks;
        static List<Type> mapComponents;

        public void FixedUpdate()
        {
            try
            {
                if (Find.TickManager != null)
                {
                    if (Find.TickManager.TicksGame > lastTicks + 10)
                    {
                        lastTicks = Find.TickManager.TicksGame;
                        reinjectTime -= Time.fixedDeltaTime;
                        if (reinjectTime <= 0)
                        {
                            reinjectTime = 0;
                            if (Find.Maps != null)
                            {
                                Find.Maps.ForEach(delegate(Map map)
                                {
                                    if (map.components != null)
                                    {
                                        mapComponents.ForEach(delegate(Type t)
                                        {
                                            if (!map.components.Any((MapComponent mp) => mp.GetType() == t))
                                            {
                                                MapComponent comp = (MapComponent) typeof(MapComponent)
                                                    .GetConstructor(Type.EmptyTypes).Invoke(new object[] {map});
                                                map.components.Add(comp);
                                            }
                                        });
                                    }
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}