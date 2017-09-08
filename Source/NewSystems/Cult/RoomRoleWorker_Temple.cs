using System;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace CultOfCthulhu
{
    public class RoomRoleWorker_Temple : RoomRoleWorker
    {
        public override float GetScore(Room room)
        {
            int num = 0;
            List<Thing> allContainedThings = room.ContainedAndAdjacentThings;
            for (int i = 0; i < allContainedThings.Count; i++)
            {
                Thing thing = allContainedThings[i];
                if (thing.def.category == ThingCategory.Building && 
                    (thing.def.defName == "Cult_SacrificialAltar" ||
                     thing.def.defName == "Cult_AnimalSacrificeAltar" ||
                     thing.def.defName == "Cult_HumanSacrificeAltar")
                     )
                {
                    num++;
                }
            }
            return (float)num * 8f;
        }
    }
}
