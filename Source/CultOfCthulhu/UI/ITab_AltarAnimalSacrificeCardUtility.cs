// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using UnityEngine;
using Verse;

// ----------------------------------------------------------------------
// These are RimWorld-specific usings. Activate/Deactivate what you need:
// ----------------------------------------------------------------------
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
    [StaticConstructorOnStartup]
    public class ITab_AltarAnimalSacrificeCardUtility
    {
        public static void DrawRename(Building_SacrificialAltar altar)
        {
            var rectRename = new Rect(ITab_AltarWorshipCardUtility.TempleCardSize.x - 85f, 0f, 30f, 30f);
            TooltipHandler.TipRegion(rectRename, "RenameTemple".Translate());
            if (Widgets.ButtonImage(rectRename, Buttons.RenameTex))
            {
                Find.WindowStack.Add(new Dialog_RenameTemple(altar));
            }
        }

        public static void DrawTempleCard(Rect rect, Building_SacrificialAltar altar)
        {
            GUI.BeginGroup(rect);
            var rect3 = new Rect(2f, 0f, ITab_AltarSacrificesCardUtility.ColumnSize,
                ITab_AltarSacrificesCardUtility.ButtonSize);
            Widgets.Label(rect3, "Deity".Translate() + ": ");
            rect3.xMin = rect3.center.x - 15f;
            var label2 = ITab_AltarCardUtility.DeityLabel(altar, ITab_AltarCardUtility.DeityType.SacrificeDeity);
            if (Widgets.ButtonText(rect3, label2, true, false))
            {
                ITab_AltarCardUtility.OpenDeitySelectMenu(altar, ITab_AltarCardUtility.DeityType.SacrificeDeity);
            }

            TooltipHandler.TipRegion(rect3, "DeityDesc".Translate());

            ITab_AltarCardUtility.DrawDeity(altar.tempCurrentSacrificeDeity, rect3);

            var rect4 = rect3;
            rect4.y += ITab_AltarSacrificesCardUtility.ButtonSize + ITab_AltarSacrificesCardUtility.SpacingOffset;
            rect4.width = ITab_AltarSacrificesCardUtility.ColumnSize;
            rect4.x -= rect3.x - 2;
            Widgets.Label(rect4, "Executioner".Translate() + ": ");
            rect4.xMin = rect4.center.x - 15f;
            var label3 = ITab_AltarCardUtility.ExecutionerLabel(altar);
            if (Widgets.ButtonText(rect4, label3, true, false))
            {
                ITab_AltarCardUtility.OpenActorSelectMenu(altar, ITab_AltarCardUtility.ActorType.executioner);
            }

            TooltipHandler.TipRegion(rect4, "ExecutionerDesc".Translate());

            var rect5 = rect4;
            rect5.y += ITab_AltarSacrificesCardUtility.ButtonSize + ITab_AltarSacrificesCardUtility.SpacingOffset;
            rect5.x -= rect4.x - 2;
            //rect5.x += 2f;
            rect5.width = ITab_AltarSacrificesCardUtility.ColumnSize;
            Widgets.Label(rect5, "Sacrifice".Translate() + ": ");
            rect5.xMin = rect5.center.x - 15f;
            var label4 = ITab_AltarCardUtility.SacrificeLabel(altar);
            if (Widgets.ButtonText(rect5, label4, true, false))
            {
                ITab_AltarCardUtility.OpenActorSelectMenu(altar, ITab_AltarCardUtility.ActorType.animalSacrifice);
            }

            TooltipHandler.TipRegion(rect5, "SacrificeAnimalDesc".Translate());

            //Rect rect6 = rect5;
            //rect6.y += 35f;
            //rect6.x -= (rect5.x - 5);
            ////rect6.x += 2f;
            //rect6.width = 210f;
            //rect6.yMax += 35f;
            //Widgets.Label(rect6, "Cults_Spell".Translate() + ": ");
            //rect6.xMin = rect6.center.x - 15f;
            //string label5 = SpellLabel(altar);
            //if (Widgets.ButtonText(rect6, label5, true, false, true))
            //{
            //    ITab_AltarHumanSacrificeCardUtility.OpenSpellSelectMenu(altar);
            //}

            /*
            Rect rect4 = rect3;
            rect4.y += 35f;
            rect4.width = 150f;
            if (Widgets.ButtonText(rect4, "RenameTemple".Translate(), true, false, true))
            {
                Find.WindowStack.Add(new Dialog_RenameTemple(altar));
            }
            Rect rectDebug1 = rect4;
            rectDebug1.y += 25f;
            if (DebugSettings.godMode)
            {
                if (Widgets.ButtonText(rectDebug1, "ForceSermonDebug".Translate(), true, false, true))
                {
                    SermonUtility.ForceSermon(altar);
                }
                Rect rectDebug2 = rectDebug1;
                rectDebug2.y += 25f;
                if (Widgets.ButtonText(rectDebug2, "ForceListenersDebug".Translate(), true, false, true))
                {
                    TempleCardUtility.ForceListenersTest(altar);
                }
            }

            Rect rect5 = rect4;
            rect5.x = rect4.xMax + 5f;
            rect5.width = 200f;
            rect5.y -= 20f;
            Widgets.CheckboxLabeled(rect5, "MorningSermons".Translate(), ref altar.OptionMorning, false);
            Rect rect6 = rect5;
            rect6.y += 20f;
            Widgets.CheckboxLabeled(rect6, "EveningSermons".Translate(), ref altar.OptionEvening, false);

            */
            GUI.EndGroup();
        }
    }
}