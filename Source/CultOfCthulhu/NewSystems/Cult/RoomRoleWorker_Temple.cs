using Verse;

namespace CultOfCthulhu
{
    public class RoomRoleWorker_Temple : RoomRoleWorker
    {
        public override float GetScore(Room room)
        {
            var num = 0;
            var allContainedThings = room.ContainedAndAdjacentThings;
            foreach (var thing in allContainedThings)
            {
                if (thing.def.category == ThingCategory.Building &&
                    (thing.def.defName == "Cult_SacrificialAltar" ||
                     thing.def.defName == "Cult_AnimalSacrificeAltar" ||
                     thing.def.defName == "Cult_HumanSacrificeAltar")
                )
                {
                    num++;
                }
            }

            return num * 8f;
        }
    }
}