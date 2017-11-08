using System;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using System.Reflection;

namespace Cthulhu
{
    /**
     * From UnlimitedHugs, bless them.
	 * This handles utility WorldObjects of custom types.
	 * Utility WorldObjects are a map-independent storage method for custom data. 
	 * All UWO's share the same def and aren't visible on the world, but are saved and loaded with it.
	 */
    [StaticConstructorOnStartup]
    public static class UtilityWorldObjectManager
    {
        public const string InjecteddefName = "Cults_UtilityWorldObject";
        public const int UtilityObjectTile = 0;

        // Returns an existing UWO or creates a new one, adding it to the world.
        public static T GetUtilityWorldObject<T>() where T : UtilityWorldObject
        {
            //Cthulhu.Utility.DebugReport("UtilityWorldObject Manager Initialized");
            var worldObjects = GetHolder();
            var obj = (T)worldObjects.ObjectsAt(UtilityObjectTile).FirstOrDefault(o => o is T);
            if (obj == null)
            {
                var def = DefDatabase<WorldObjectDef>.GetNamed(InjecteddefName);
                def.worldObjectClass = typeof(T);
                obj = (T)WorldObjectMaker.MakeWorldObject(def);
                def.worldObjectClass = typeof(WorldObject);
                obj.Tile = UtilityObjectTile;
                worldObjects.Add(obj);
            }
            return obj;
        }

        public static bool UtilityWorldObjectExists<T>() where T : UtilityWorldObject
        {
            return GetHolder().ObjectsAt(UtilityObjectTile).Any(o => o is T);
        }

        internal static void OnDefsLoaded()
        {
            InjectUtilityObjectDef();
        }

        private static WorldObjectsHolder GetHolder()
        {
            if (Current.Game == null || Current.Game.World == null) throw new Exception("A world must be loaded to get a WorldObject");
            return Current.Game.World.worldObjects;
        }

        private static void InjectUtilityObjectDef()
        {
            var def = new WorldObjectDef
            {
                defName = InjecteddefName,
                worldObjectClass = typeof(WorldObject),
                canHaveFaction = false,
                selectable = false,
                neverMultiSelect = true,
                useDynamicDrawer = true
            };

            AddSHortHashToInjectedDef(def);
            DefDatabase<WorldObjectDef>.Add(def);
        }

        // Adds a hash to a manually instantiated def to avoid def collisions
        public static void AddSHortHashToInjectedDef(Def newDef)
        {
            var hashMethod = typeof(ShortHashGiver).GetMethod("GiveShortHash", BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(Def) }, null);
            if (hashMethod == null)
            {
                Log.Error("Failed to reflect ShortHashGiver.GiveShortHash");
                return;
            }
            hashMethod.Invoke(null, new object[] { newDef });
        }
    }
}