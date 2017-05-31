using System;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace CultOfCthulhu
{
    public class Dialog_RenameTemple : Dialog_Rename
    {
        private Building_SacrificialAltar altar;

        private Map map;

        public Dialog_RenameTemple(Building_SacrificialAltar altar)
        {
            this.altar = altar;
            this.curName = altar.RoomName;
            this.map = altar.Map;
        }
        
        protected override AcceptanceReport NameIsValid(string name)
        {
            AcceptanceReport result = base.NameIsValid(name);
            if (!result.Accepted)
            {
                return result;
            }
            if (name.Length == 0 || name.Length > 27)
            {
                return "NameIsInvalid".Translate();
            }
            return true;
        }

        protected override void SetName(string name)
        {
            this.altar.RoomName = name;
        }
    }
}
