﻿using Cthulhu;
using Verse;

namespace CultOfCthulhu
{
    public class Dialog_RenameCult : Dialog_Rename
    {
        private readonly Map map;

        public Dialog_RenameCult(Map newMap)
        {
            map = newMap;
            if (map != null)
            {
                curName = CultTracker.Get.PlayerCult.name;
            }
            else
            {
                Utility.ErrorReport(x: "Missing map to declare as home area");
            }
        }

        protected override AcceptanceReport NameIsValid(string name)
        {
            var result = base.NameIsValid(name: name);
            if (!result.Accepted)
            {
                return result;
            }

            return name.Length == 0 || !CultUtility.CheckValidCultName(str: name)
                ? "NameIsInvalid".Translate()
                : (AcceptanceReport) true;
        }

        protected override void SetName(string name)
        {
            if (map != null)
            {
                CultTracker.Get.PlayerCult.name = name;
            }
            else
            {
                Utility.ErrorReport(x: "Map Reference Null Exception");
            }
        }
    }
}