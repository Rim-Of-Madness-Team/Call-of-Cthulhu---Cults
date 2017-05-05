using System;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace CultOfCthulhu
{
    public class Dialog_RenameCult : Dialog_Rename
    {

        private Map map;

        public Dialog_RenameCult(Map newMap)
        {
            this.map = newMap;
            if (map != null)
            {
                this.curName = map.GetComponent<MapComponent_LocalCultTracker>().CultName;
            }
            else
            {
                Cthulhu.Utility.ErrorReport("Missing map to declare as home area");
            }
        }

        protected override AcceptanceReport NameIsValid(string name)
        {
            AcceptanceReport result = base.NameIsValid(name);
            if (!result.Accepted)
            {
                return result;
            }
            if (name.Length == 0 || (!CultUtility.CheckValidCultName(name)))
            {
                return "NameIsInvalid".Translate();
            }
            return true;
        }

        protected override void SetName(string name)
        {
            if (map != null)
            {
                map.GetComponent<MapComponent_LocalCultTracker>().CultName = name;
            }
            else
            {
                Cthulhu.Utility.ErrorReport("Map Reference Null Exception");
            }
        }
    }
}
