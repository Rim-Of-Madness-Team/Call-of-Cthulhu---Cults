// ----------------------------------------------------------------------
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

        public static Vector2 SacrificeCardSize = new Vector2(x: 550f, y: 415f);

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
            var rectRename = new Rect(x: SacrificeCardSize.x - 85f, y: 0f, width: 30f, height: 30f);
            TooltipHandler.TipRegion(rect: rectRename, tip: "RenameTemple".Translate());
            if (Widgets.ButtonImage(butRect: rectRename, tex: Buttons.RenameTex))
            {
                Find.WindowStack.Add(window: new Dialog_RenameTemple(altar: altar));
            }
        }

        public static void DrawSacrificeCard(Rect inRect, Building_SacrificialAltar altar)
        {
            GUI.BeginGroup(position: inRect);

            if (CultTracker.Get.PlayerCult != null)
            {
                var cultLabelWidth = Text.CalcSize(text: CultTracker.Get.PlayerCult.name).x;

                var rect = new Rect(source: inRect);
                rect = rect.ContractedBy(margin: 14f);
                rect.height = 30f;

                Text.Font = GameFont.Medium;
                Widgets.Label(rect: rect, label: altar.RoomName);
                Text.Font = GameFont.Small;

                DrawRename(altar: altar);

                var rect2 = new Rect(source: inRect)
                {
                    yMin = rect.yMax + 10,
                    height = 22f
                };
                rect2.xMin += 15f;
                rect2.width = cultLabelWidth + 5;
                //rect2.yMax -= 38f;
                Widgets.Label(rect: rect2, label: CultTracker.Get.PlayerCult.name);
                if (Mouse.IsOver(rect: rect2))
                {
                    Widgets.DrawHighlight(rect: rect2);
                }

                if (Mouse.IsOver(rect: rect2) && Event.current.type == EventType.MouseDown)
                {
                    Find.WindowStack.Add(window: new Dialog_RenameCult(newMap: altar.Map));
                }

                var rect3 = new Rect(source: inRect)
                {
                    //rect3.height -= 45f;
                    //rect3.yMin += 45f;
                    yMin = rect2.yMax + 45f,
                    height = 550f
                };
                var list = new List<TabRecord>();
                var item = new TabRecord(label: "Offering".Translate(), clickedAction: delegate { tab = SacrificeCardTab.Offering; },
                    selected: tab == SacrificeCardTab.Offering);
                list.Add(item: item);
                if (altar.currentFunction >= Building_SacrificialAltar.Function.Level2)
                {
                    var item2 = new TabRecord(label: "Animal".Translate(), clickedAction: delegate { tab = SacrificeCardTab.Animal; },
                        selected: tab == SacrificeCardTab.Animal);
                    list.Add(item: item2);
                }

                if (altar.currentFunction >= Building_SacrificialAltar.Function.Level3)
                {
                    var item3 = new TabRecord(label: "Human".Translate(), clickedAction: delegate { tab = SacrificeCardTab.Human; },
                        selected: tab == SacrificeCardTab.Human);
                    list.Add(item: item3);
                }

                TabDrawer.DrawTabs(baseRect: rect3, tabs: list);
                FillCard(cardRect: rect3.ContractedBy(margin: 10f), altar: altar);
            }
            else
            {
                var rect = new Rect(source: inRect);
                rect = rect.ContractedBy(margin: 14f);
                rect.height = 30f;

                Text.Font = GameFont.Medium;
                Widgets.Label(rect: rect, label: "Cults_NoPlayerCultAvailable".Translate());
                Text.Font = GameFont.Small;
            }


            GUI.EndGroup();
        }

        protected static void FillCard(Rect cardRect, Building_SacrificialAltar altar)
        {
            if (tab == SacrificeCardTab.Offering)
            {
                ITab_AltarFoodSacrificeCardUtility.DrawTempleCard(rect: cardRect, altar: altar);
            }
            else if (tab == SacrificeCardTab.Animal)
            {
                ITab_AltarAnimalSacrificeCardUtility.DrawTempleCard(rect: cardRect, altar: altar);
            }
            else if (tab == SacrificeCardTab.Human)
            {
                ITab_AltarHumanSacrificeCardUtility.DrawTempleCard(rect: cardRect, altar: altar);
            }
            else
            {
                ITab_AltarFoodSacrificeCardUtility.DrawTempleCard(rect: cardRect, altar: altar);
            }
        }
    }
}