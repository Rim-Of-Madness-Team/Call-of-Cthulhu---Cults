﻿// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using System.Collections.Generic;
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
    public class ITab_AltarSacrificesCardUtility
    {
        public enum SacrificeCardTab : byte
        {
            Offering,
            Animal,
            Human
        }

        public static Vector2 SacrificeCardSize = new Vector2(550f, 415f);

        public static float ButtonSize = 40f;

        public static float SpacingOffset = 15f;

        public static float ColumnSize = 245f;

        public static SacrificeCardTab tab = SacrificeCardTab.Offering;


        public static SacrificeCardTab Tab
        {
            get => tab;
            set => tab = value;
        }

        public static void DrawRename(Building_SacrificialAltar altar)
        {
            var rectRename = new Rect(SacrificeCardSize.x - 85f, 0f, 30f, 30f);
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
                var cultLabelWidth = Text.CalcSize(CultTracker.Get.PlayerCult.name).x;

                var rect = new Rect(inRect);
                rect = rect.ContractedBy(14f);
                rect.height = 30f;

                Text.Font = GameFont.Medium;
                Widgets.Label(rect, altar.RoomName);
                Text.Font = GameFont.Small;

                DrawRename(altar);

                var rect2 = new Rect(inRect)
                {
                    yMin = rect.yMax + 10,
                    height = 22f
                };
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

                var rect3 = new Rect(inRect)
                {
                    //rect3.height -= 45f;
                    //rect3.yMin += 45f;
                    yMin = rect2.yMax + 45f,
                    height = 550f
                };
                var list = new List<TabRecord>();
                var item = new TabRecord("Offering".Translate(), delegate { tab = SacrificeCardTab.Offering; },
                    tab == SacrificeCardTab.Offering);
                list.Add(item);
                if (altar.currentFunction >= Building_SacrificialAltar.Function.Level2)
                {
                    var item2 = new TabRecord("Animal".Translate(), delegate { tab = SacrificeCardTab.Animal; },
                        tab == SacrificeCardTab.Animal);
                    list.Add(item2);
                }

                if (altar.currentFunction >= Building_SacrificialAltar.Function.Level3)
                {
                    var item3 = new TabRecord("Human".Translate(), delegate { tab = SacrificeCardTab.Human; },
                        tab == SacrificeCardTab.Human);
                    list.Add(item3);
                }

                TabDrawer.DrawTabs(rect3, list);
                FillCard(rect3.ContractedBy(10f), altar);
            }
            else
            {
                var rect = new Rect(inRect);
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