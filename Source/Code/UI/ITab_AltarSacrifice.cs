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
    public class ITab_Sacrifice : ITab
    {
        public ITab_Sacrifice()
        {
            size = ITab_AltarSacrificesCardUtility.SacrificeCardSize;
            labelKey = "TabSacrifice";
        }

        protected Building_SacrificialAltar SelAltar => (Building_SacrificialAltar) SelThing;

        protected override void FillTab()
        {
            var rect = new Rect(x: 0f, y: 0f, width: size.x, height: size.y).ContractedBy(margin: 5f);
            ITab_AltarSacrificesCardUtility.DrawSacrificeCard(inRect: rect, altar: SelAltar);
        }
    }
}