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
        private static readonly List<Type> mapComponents;
        private int lastTicks;
        protected bool monstrousDefsAdded = false;

        private float reinjectTime;

        static MapComponentInjectorBehavior()
        {
            var initializer = new GameObject("JecrellMapCompInjector");
            initializer.AddComponent<MapComponentInjectorBehavior>();
            DontDestroyOnLoad(initializer);
            mapComponents = new List<Type>();
            typeof(MapComponentInjectorBehavior).Assembly.GetTypes()
                .Where(t => t.IsClass && t.IsSubclassOf(typeof(MapComponent))).ToList()
                .ForEach(t => mapComponents.Add(t));
            //mapComponents.ForEach((Type t) => Log.Message(t.Name + "found for MapComponentInjector"));
        }

        public void FixedUpdate()
        {
            try
            {
                if (Find.TickManager == null)
                {
                    return;
                }

                if (Find.TickManager.TicksGame <= lastTicks + 10)
                {
                    return;
                }

                lastTicks = Find.TickManager.TicksGame;
                reinjectTime -= Time.fixedDeltaTime;
                if (!(reinjectTime <= 0))
                {
                    return;
                }

                reinjectTime = 0;
                if (Find.Maps != null)
                {
                    Find.Maps.ForEach(delegate(Map map)
                    {
                        if (map.components != null)
                        {
                            mapComponents.ForEach(delegate(Type t)
                            {
                                if (map.components.Any(mp => mp.GetType() == t))
                                {
                                    return;
                                }

                                var comp = (MapComponent) typeof(MapComponent)
                                    .GetConstructor(Type.EmptyTypes)
                                    ?.Invoke(new object[] {map});
                                map.components.Add(comp);
                            });
                        }
                    });
                }
            }
            catch
            {
                // ignored
            }
        }
    }
}