// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

// ----------------------------------------------------------------------
// These are RimWorld-specific usings. Activate/Deactivate what you need:
// ----------------------------------------------------------------------

using RimWorld;
using UnityEngine;
using Verse;

// Always needed
//using VerseBase;         // Material/Graphics handling functions are found here
// RimWorld universal objects are here (like 'Building')
// Needed when you do something with the AI
// Needed when you do something with Sound
// Needed when you do something with Noises

// RimWorld specific functions are found here (like 'Building_Battery')

// RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

namespace CultOfCthulhu
{
    public class ITab_Worship : ITab
    {
        public ITab_Worship()
        {
            size = ITab_AltarWorshipCardUtility.TempleCardSize;
            labelKey = "TabWorship";
        }

        protected Building_SacrificialAltar SelAltar => (Building_SacrificialAltar) SelThing;

        protected override void FillTab()
        {
            var rect = new Rect(0f, 0f, size.x, size.y).ContractedBy(5f);
            ITab_AltarWorshipCardUtility.DrawTempleCard(rect, SelAltar);
        }
    }
}