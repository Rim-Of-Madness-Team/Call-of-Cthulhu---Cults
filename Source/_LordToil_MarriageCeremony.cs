using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Verse;
using RimWorld;
using UnityEngine;
using RimWorld.Planet;

namespace Cthulhu.Detour
{
    internal static class _LordToil_MarriageCeremony
    {

        [Detour(typeof(LordToil_MarriageCeremony), bindingFlags = (BindingFlags.Instance | BindingFlags.NonPublic))]
        internal static Thing GetMarriageSpotAt(this LordToil_MarriageCeremony _this, IntVec3 cell)
        {
            Cthulhu.Utility.DebugReport("Get Marriage Spot Called");
            return cell.GetThingList(_this.lord.lordManager.map).Find((Thing x) => x is Building_MarriageSpot);
        }
        
        
    }
}
