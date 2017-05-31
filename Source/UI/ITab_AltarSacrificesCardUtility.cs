// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

// ----------------------------------------------------------------------
// These are RimWorld-specific usings. Activate/Deactivate what you need:
// ----------------------------------------------------------------------
using UnityEngine;         // Always needed
//using VerseBase;         // Material/Graphics handling functions are found here
using Verse;               // RimWorld universal objects are here (like 'Building')
using Verse.AI;          // Needed when you do something with the AI
using Verse.AI.Group;
using Verse.Sound;       // Needed when you do something with Sound
using Verse.Noise;       // Needed when you do something with Noises
using RimWorld;            // RimWorld specific functions are found here (like 'Building_Battery')
using RimWorld.Planet;   // RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

namespace CultOfCthulhu
{
    [StaticConstructorOnStartup]
    public class ITab_AltarSacrificesCardUtility
    {
        public static Vector2 SacrificeCardSize = new Vector2(550f, 415f);

        public static float ButtonSize = 40f;

        public static float SpacingOffset = 15f;

        public static float ColumnSize = 245f;

        public enum SacrificeCardTab : byte
        {
            Offering,
            Animal,
            Human
        }

        public static SacrificeCardTab tab = SacrificeCardTab.Offering;

        public static void DrawRename(Building_SacrificialAltar altar)
        {
            Rect rectRename = new Rect(ITab_AltarSacrificesCardUtility.SacrificeCardSize.x - 85f, 0f, 30f, 30f);
            TooltipHandler.TipRegion(rectRename, "RenameTemple".Translate());
            if (Widgets.ButtonImage(rectRename, Buttons.RenameTex))
            {
                Find.WindowStack.Add(new Dialog_RenameTemple(altar));
            }
        }

        public static void DrawSacrificeCard(Rect inRect, Building_SacrificialAltar altar)
        {
            GUI.BeginGroup(inRect);

            if (CultTracker.Get.PlayerCult != null)
            {
                float cultLabelWidth = Text.CalcSize(CultTracker.Get.PlayerCult.name).x;

                Rect rect = new Rect(inRect);
                rect = rect.ContractedBy(14f);
                rect.height = 30f;

                Text.Font = GameFont.Medium;
                Widgets.Label(rect, altar.RoomName);
                Text.Font = GameFont.Small;

                DrawRename(altar);

                Rect rect2 = new Rect(inRect);
                rect2.yMin = rect.yMax + 10;
                rect2.height = 22f;
                rect2.xMin += 15f;
                rect2.width = cultLabelWidth + 5;
                //rect2.yMax -= 38f;
                Widgets.Label(rect2, CultTracker.Get.PlayerCult.name);
                if (Mouse.IsOver(rect2))
                {
                    Widgets.DrawHighlight(rect2);
                }
                if (Mouse.IsOver(rect2) && Event.current.type == EventType.MouseDown)
                {
                    Find.WindowStack.Add(new Dialog_RenameCult(altar.Map));
                }

                Rect rect3 = new Rect(inRect);
                //rect3.height -= 45f;
                //rect3.yMin += 45f;
                rect3.yMin = rect2.yMax + 45f;
                rect3.height = 550f;
                List<TabRecord> list = new List<TabRecord>();
                TabRecord item = new TabRecord("Offering".Translate(), delegate
                {
                    tab = SacrificeCardTab.Offering;
                }, tab == SacrificeCardTab.Offering);
                list.Add(item);
                if (altar.currentFunction >= Building_SacrificialAltar.Function.Level2)
                {
                    TabRecord item2 = new TabRecord("Animal".Translate(), delegate
                    {
                        tab = SacrificeCardTab.Animal;
                    }, tab == SacrificeCardTab.Animal);
                    list.Add(item2);

                }
                if (altar.currentFunction >= Building_SacrificialAltar.Function.Level3)
                {
                    TabRecord item3 = new TabRecord("Human".Translate(), delegate
                    {
                        tab = SacrificeCardTab.Human;
                    }, tab == SacrificeCardTab.Human);
                    list.Add(item3);
                }
                TabDrawer.DrawTabs(rect3, list);
                FillCard(rect3.ContractedBy(10f), altar);
            }
            else
            {
                Rect rect = new Rect(inRect);
                rect = rect.ContractedBy(14f);
                rect.height = 30f;

                Text.Font = GameFont.Medium;
                Widgets.Label(rect, "Cults_NoPlayerCultAvailable".Translate());
                Text.Font = GameFont.Small;

            }


            GUI.EndGroup();        
        }


        protected static void FillCard(Rect cardRect, Building_SacrificialAltar altar)
        {
            if (tab == SacrificeCardTab.Offering)
            {
                ITab_AltarFoodSacrificeCardUtility.DrawTempleCard(cardRect, altar);
            }
            else if (tab == SacrificeCardTab.Animal)
            {
                ITab_AltarAnimalSacrificeCardUtility.DrawTempleCard(cardRect, altar);
            }
            else if (tab == SacrificeCardTab.Human)
            {
                ITab_AltarHumanSacrificeCardUtility.DrawTempleCard(cardRect, altar);
            }
            else
            {
                ITab_AltarFoodSacrificeCardUtility.DrawTempleCard(cardRect, altar);
            }
        }

    }
}
