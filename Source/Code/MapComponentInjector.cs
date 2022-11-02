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
            var initializer = new GameObject(name: "JecrellMapCompInjector");
            initializer.AddComponent<MapComponentInjectorBehavior>();
            DontDestroyOnLoad(target: initializer);
            mapComponents = new List<Type>();
            typeof(MapComponentInjectorBehavior).Assembly.GetTypes()
                .Where(predicate: t => t.IsClass && t.IsSubclassOf(c: typeof(MapComponent))).ToList()
                .ForEach(action: t => mapComponents.Add(item: t));
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
                    Find.Maps.ForEach(action: delegate(Map map)
                    {
                        if (map.components != null)
                        {
                            mapComponents.ForEach(action: delegate(Type t)
                            {
                                if (map.components.Any(predicate: mp => mp.GetType() == t))
                                {
                                    return;
                                }

                                var comp = (MapComponent) typeof(MapComponent)
                                    .GetConstructor(types: Type.EmptyTypes)
                                    ?.Invoke(parameters: new object[] {map});
                                map.components.Add(item: comp);
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