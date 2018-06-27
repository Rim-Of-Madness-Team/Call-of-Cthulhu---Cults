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
using CultOfCthulhu;
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

namespace CultOfCthulhu
{
    [StaticConstructorOnStartup]
    public class ITab_AltarWorshipCardUtility
    {
        public static Vector2 TempleCardSize = new Vector2(600f, 400f);

        public static void DrawTempleCard(Rect rect, Building_SacrificialAltar altar)
        {
            GUI.BeginGroup(rect);

            if (CultTracker.Get.PlayerCult != null)
            {
                float cultLabelWidth = Text.CalcSize(CultTracker.Get.PlayerCult.name).x + 15;

                //Headings
                Rect rect1 = new Rect(rect);
                rect1 = rect.ContractedBy(14f);
                rect1.height = 30f;

                //Unnamed Temple
                Text.Font = GameFont.Medium;
                Widgets.Label(rect1, altar.RoomName);
                Text.Font = GameFont.Small;

                //Rename Icon
                ITab_AltarCardUtility.DrawRename(altar);
                Rect rect2 = new Rect(rect1);
                rect2.yMin = rect1.yMax + 10;
                rect2.height = 25f;
                rect2.width = cultLabelWidth + 5;

                //Esoteric Order of Dagon
                Widgets.Label(rect2, CultTracker.Get.PlayerCult.name);
                if (Mouse.IsOver(rect2))
                {
                    Widgets.DrawHighlight(rect2);
                }
                if (Mouse.IsOver(rect2) && Event.current.type == EventType.MouseDown)
                {
                    Find.WindowStack.Add(new Dialog_RenameCult(altar.Map));
                }
                Widgets.DrawLineHorizontal(rect2.x - 10, rect2.yMax, rect.width - 15f);
                //---------------------------------------------------------------------

                Rect rectMain = new Rect(0 + 15f, 0 + 30f, TempleCardSize.x, ITab_AltarSacrificesCardUtility.ButtonSize * 1.15f);

                //Deity -> Cthulhu
                Rect rect4 = rectMain;
                rect4.yMin = rectMain.yMax + 5f;
                rect4.y = rectMain.yMax + 20f;
                rect4.x += 5f;
                rect4.width = ITab_AltarSacrificesCardUtility.ColumnSize;
                rect4.height = ITab_AltarSacrificesCardUtility.ButtonSize;
                Widgets.Label(rect4, "Deity".Translate() + ": ");
                rect4.xMin = rect4.center.x;
                string label4 = DeityLabel(altar);
                if (Widgets.ButtonText(rect4, label4, true, false, true))
                {
                    ITab_AltarWorshipCardUtility.OpenDeitySelectMenu(altar);
                }
                TooltipHandler.TipRegion(rect4, "DeityDesc".Translate());

                //Cthulhu - He who waits dreaming.
                ITab_AltarCardUtility.DrawDeity(altar.tempCurrentWorshipDeity, rect4, null, -30f);

                //Preacher
                Rect rect5 = rect4;
                rect5.y += ITab_AltarSacrificesCardUtility.ButtonSize + 15f;
                //rect5.y = rect4.yMax + 30f;
                rect5.x -= (rect4.x - 5);
                rect5.x += 15f;
                rect5.width = ITab_AltarSacrificesCardUtility.ColumnSize;
                Widgets.Label(rect5, "Preacher".Translate() + ": ");
                rect5.xMin = rect5.center.x;
                string label2 = PreacherLabel(altar);
                if (Widgets.ButtonText(rect5, label2, true, false, true))
                {
                    ITab_AltarWorshipCardUtility.OpenPreacherSelectMenu(altar);
                }
                TooltipHandler.TipRegion(rect5, "PreacherDesc".Translate());

                Rect rect6 = rect5;
                rect6.y += ITab_AltarSacrificesCardUtility.ButtonSize + ITab_AltarSacrificesCardUtility.SpacingOffset;
                rect6.height = ITab_AltarSacrificesCardUtility.ButtonSize;
                rect6.width = ITab_AltarSacrificesCardUtility.ColumnSize;
                rect6.x -= (rect5.x - 5);
                rect6.x += 15f;
                bool disabled = (altar.tempCurrentWorshipDeity == null);
                Widgets.CheckboxLabeled(rect6.BottomHalf(), "MorningSermons".Translate(), ref altar.OptionMorning, disabled);
                if (Mouse.IsOver(rect6) && Event.current.type == EventType.MouseDown && !disabled)
                {
                    altar.TryChangeWorshipValues(Building_SacrificialAltar.ChangeWorshipType.MorningWorship, altar.OptionMorning);
                }
                Rect rect7 = rect6;
                rect7.y += ITab_AltarSacrificesCardUtility.ButtonSize + ITab_AltarSacrificesCardUtility.SpacingOffset;
                rect7.height = ITab_AltarSacrificesCardUtility.ButtonSize;
                Widgets.CheckboxLabeled(rect7.TopHalf(), "EveningSermons".Translate(), ref altar.OptionEvening, disabled);
                if (Mouse.IsOver(rect7) && Event.current.type == EventType.MouseDown && !disabled)
                {
                    altar.TryChangeWorshipValues(Building_SacrificialAltar.ChangeWorshipType.EveningWorship, altar.OptionEvening);
                }

                TooltipHandler.TipRegion(rect6, "MorningSermonsDesc".Translate());
                TooltipHandler.TipRegion(rect7, "EveningSermonsDesc".Translate());

            }
            else
            {
                Rect newRect = new Rect(rect);
                newRect = newRect.ContractedBy(14f);
                newRect.height = 30f;

                Text.Font = GameFont.Medium;
                Widgets.Label(newRect, "Cults_NoPlayerCultAvailable".Translate());
                Text.Font = GameFont.Small;
            }

            GUI.EndGroup();
        }
        
        private static string PreacherLabel(Building_SacrificialAltar altar)
        {
            if (altar.tempPreacher == null)
            {
                altar.tempPreacher = CultUtility.DetermineBestPreacher(altar.Map);
                if (altar.tempPreacher == null) return "None";
                return altar.tempPreacher.Name.ToStringShort;
            }
            else
            {
                return altar.tempPreacher.Name.ToStringShort;
            }
        }

        private static string DeityLabel(Building_SacrificialAltar altar)
        {
            if (altar.tempCurrentWorshipDeity == null)
            {
                return "None";
            }
            else
            {
                return altar.tempCurrentWorshipDeity.LabelCap;
            }
        }

        private static string DeityDescription(Building_SacrificialAltar altar)
        {
            if (altar.tempCurrentWorshipDeity == null)
            {
                return "None";
            }
            else
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(altar.tempCurrentWorshipDeity.def.description);
                return stringBuilder.ToString();
            }
        }
        
        public static void OpenPreacherSelectMenu(Building_SacrificialAltar altar)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            list.Add(new FloatMenuOption("(" + "Auto".Translate() + ")", delegate
            {
                altar.tempPreacher = CultUtility.DetermineBestPreacher(altar.Map);
            }, MenuOptionPriority.Default, null, null, 0f, null));

            foreach (Pawn current in CultTracker.Get.PlayerCult.MembersAt(altar.Map))
            {
                if (current.health.capacities.CapableOf(PawnCapacityDefOf.Talking) &&
                    current.health.capacities.CapableOf(PawnCapacityDefOf.Moving))
                {
                    Action action;
                    Pawn localCol = current;
                    action = delegate
                    {

                        //Map.GetComponent<MapComponent_SacrificeTracker>().lastUsedAltar = altar;
                        altar.tempPreacher = localCol;
                    };
                    list.Add(new FloatMenuOption(localCol.LabelShort, action, MenuOptionPriority.Default, null, null, 0f, null));
                }
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }


        public static void OpenDeitySelectMenu(Building_SacrificialAltar altar)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            list.Add(new FloatMenuOption("(" + "NoneLower".Translate() + ")", delegate
            {
                //Map.GetComponent<MapComponent_SacrificeTracker>().lastUsedAltar = altar;
                altar.tempCurrentWorshipDeity = null;
            }, MenuOptionPriority.Default, null, null, 0f, null));

            foreach (CosmicEntity current in DeityTracker.Get.DeityCache.Keys)
            {
                if (!current.discovered) continue;
                Action action;
                CosmicEntity localDeity = current;
                action = delegate
                {

                    //Map.GetComponent<MapComponent_SacrificeTracker>().lastUsedAltar = altar;
                    altar.tempCurrentWorshipDeity = localDeity;
                    //altar.tempCurrentSpell = null;
                };
                list.Add(new FloatMenuOption(localDeity.LabelCap, action, MenuOptionPriority.Default, null, null, 0f, null));
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }

    }
}
