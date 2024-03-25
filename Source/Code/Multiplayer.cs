using HarmonyLib;
using Multiplayer.API;
using RimWorld;
using Verse;

namespace CultOfCthulhu;

[StaticConstructorOnStartup]
internal static class Multiplayer
{
    static Multiplayer()
    {
        if (!MP.enabled)
        {
            return;
        }

        MP.RegisterAll();
    }
    //Remove me before PR. Example of decent syncworker register.
    //private static void IHaulDestinationWorker(SyncWorker sync, ref IHaulDestination destination)
    //{
    //    if (sync.isWriting)
    //    {
    //        sync.Write(destination.Map);
    //        sync.Write(destination.Position);
    //    }
    //    else
    //    {
    //        var map = sync.Read<Map>();
    //        var pos = sync.Read<IntVec3>();

    //        destination = map.haulDestinationManager.AllHaulDestinationsListForReading
    //            .FirstOrDefault(d => d.Position == pos);
    //    }
    //}
}