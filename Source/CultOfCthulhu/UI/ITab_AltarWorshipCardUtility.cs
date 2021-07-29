// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using CallOfCthulhu;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

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
    public class ITab_AltarWorshipCardUtility
    {
        public static Vector2 TempleCardSize = new Vector2(600f, 500f);

        public static void DrawTempleCard(Rect rect, Building_SacrificialAltar altar)
        {
            GUI.BeginGroup(rect);

            if (CultTracker.Get.PlayerCult != null)
            {
                var cultLabelWidth = Text.CalcSize(CultTracker.Get.PlayerCult.name).x + 15;

                //Headings
                _ = new Rect(rect);
                var rect1 = rect.ContractedBy(14f);
                rect1.height = 30f;

                //Unnamed Temple
                Text.Font = GameFont.Medium;
                Widgets.Label(rect1, altar.RoomName);
                Text.Font = GameFont.Small;

                //Rename Icon
                ITab_AltarCardUtility.DrawRename(altar);
                var rect2 = new Rect(rect1)
                {
                    yMin = rect1.yMax + 10,
                    height = 25f,
                    width = cultLabelWidth + 5
                };

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

                var rectMain = new Rect(0 + 15f, 0 + 30f, TempleCardSize.x,
                    ITab_AltarSacrificesCardUtility.ButtonSize * 1.15f);

                //Deity -> Cthulhu
                var rect4 = rectMain;
                rect4.yMin = rectMain.yMax + 5f;
                rect4.y = rectMain.yMax + 20f;
                rect4.x += 5f;
                rect4.width = ITab_AltarSacrificesCardUtility.ColumnSize;
                rect4.height = ITab_AltarSacrificesCardUtility.ButtonSize;
                Widgets.Label(rect4, "Deity".Translate() + ": ");
                rect4.xMin = rect4.center.x;
                var label4 = DeityLabel(altar);
                if (Widgets.ButtonText(rect4, label4, true, false))
                {
                    OpenDeitySelectMenu(altar);
                }

                TooltipHandler.TipRegion(rect4, "DeityDesc".Translate());

                //Cthulhu - He who waits dreaming.
                ITab_AltarCardUtility.DrawDeity(altar.tempCurrentWorshipDeity, rect4, null, -30f);

                //Preacher
                var rect5 = rect4;
                rect5.y += ITab_AltarSacrificesCardUtility.ButtonSize + 15f;
                //rect5.y = rect4.yMax + 30f;
                rect5.x -= rect4.x - 5;
                rect5.x += 15f;
                rect5.width = ITab_AltarSacrificesCardUtility.ColumnSize;
                Widgets.Label(rect5, "Preacher".Translate() + ": ");
                rect5.xMin = rect5.center.x;
                var label2 = PreacherLabel(altar);
                if (Widgets.ButtonText(rect5, label2, true, false))
                {
                    OpenPreacherSelectMenu(altar);
                }

                TooltipHandler.TipRegion(rect5, "PreacherDesc".Translate());

                var rect6 = rect5;
                rect6.y += ITab_AltarSacrificesCardUtility.ButtonSize + ITab_AltarSacrificesCardUtility.SpacingOffset;
                rect6.height = ITab_AltarSacrificesCardUtility.ButtonSize * 2;
                rect6.width = ITab_AltarSacrificesCardUtility.ColumnSize;
                rect6.x -= rect5.x - 5;
                rect6.x += 15f;
                if (altar.tempCurrentWorshipDeity != null)
                {
                    Widgets.Label(rect6.BottomHalf(), "Cults_SeasonDays".Translate());

                    Text.Font = GameFont.Tiny;
                    //Text.Anchor = TextAnchor.LowerLeft;
                    var num = 15f;
                    var num2 = 270f;
                    var hourWidth = 20.833334f;
                    for (var day = 0; day <= 14; day++)
                    {
                        var rect9 = new Rect(num + 4f, num2 + 0f, hourWidth, 20f);
                        Widgets.Label(rect9, (day + 1).ToString());
                        var rect10 = new Rect(num, num2 + 20f, hourWidth, 30f);
                        rect10 = rect10.ContractedBy(1f);
                        var texture = TimeAssignmentDefOf.Anything.ColorTexture;
                        switch (altar.seasonSchedule[day])
                        {
                            case 1:
                                texture = SolidColorMaterials.NewSolidColorTexture(Color.red);
                                break;
                            case 2:
                                texture = SolidColorMaterials.NewSolidColorTexture(Color.blue);
                                break;
                            case 3:
                                texture = SolidColorMaterials.NewSolidColorTexture(Color.magenta);
                                break;
                        }

                        GUI.DrawTexture(rect10, texture);
                        if (Mouse.IsOver(rect10))
                        {
                            Widgets.DrawBox(rect10, 2);
                            //if (Input.GetMouseButton(0))
                            if (Widgets.ButtonInvisible(rect10))
                            {
                                altar.seasonSchedule[day] = (altar.seasonSchedule[day] % 4) + 1;
                                SoundDefOf.Designate_DragStandard_Changed.PlayOneShotOnCamera();
                                //p.timetable.SetAssignment(hour, this.selectedAssignment);
                            }
                        }

                        num += hourWidth;
                    }

                    num2 += 60f;
                    var rect11 = new Rect(15f, num2 + 3, hourWidth / 2, hourWidth / 2);
                    rect11 = rect11.ContractedBy(1f);
                    GUI.DrawTexture(rect11, TimeAssignmentDefOf.Anything.ColorTexture);
                    var rect12 = new Rect(15f + hourWidth, num2, 150f, (hourWidth / 2) + 6);
                    Widgets.Label(rect12, "NoSermonLabel".Translate());

                    var rect13 = new Rect(15f + 170f, num2 + 3, hourWidth / 2, hourWidth / 2);
                    rect13 = rect13.ContractedBy(1f);
                    GUI.DrawTexture(rect13, SolidColorMaterials.NewSolidColorTexture(Color.magenta));
                    var rect14 = new Rect(15f + hourWidth + 170f, num2, 150f, (hourWidth / 2) + 6);
                    Widgets.Label(rect14, "BothSermonLabel".Translate());

                    num2 += 30f;
                    var rect15 = new Rect(15f, num2 + 3, hourWidth / 2, hourWidth / 2);
                    rect15 = rect15.ContractedBy(1f);
                    GUI.DrawTexture(rect15, SolidColorMaterials.NewSolidColorTexture(Color.red));
                    var rect16 = new Rect(15f + hourWidth, num2, 150f, (hourWidth / 2) + 6);
                    Widgets.Label(rect16, "MorningSermonLabel".Translate());

                    var rect17 = new Rect(15f + 170f, num2 + 3, hourWidth / 2, hourWidth / 2);
                    rect17 = rect17.ContractedBy(1f);
                    GUI.DrawTexture(rect17, SolidColorMaterials.NewSolidColorTexture(Color.blue));
                    var rect18 = new Rect(15f + hourWidth + 170f, num2, 150f, (hourWidth / 2) + 6);
                    Widgets.Label(rect18, "EveningSermonLabel".Translate());

                    num2 += 35f;
                    var rect19 = new Rect(15f, num2, 150f, (hourWidth / 2) + 6);
                    Widgets.Label(rect19, "Cults_SermonStartLabel".Translate());

                    var dist = 5f;
                    var button3 = new Rect(rect6.x + dist, rect6.y + 215f, 140f, 30f);
                    var morningHour = altar.morningHour + ":00h";
                    if (Widgets.ButtonText(button3, "Cults_MorningSermonStart".Translate() + morningHour, true, false))
                    {
                        listHours(altar, true);
                    }

                    var button4 = new Rect(rect6.x + dist + 150f, rect6.y + 215f, 140f, 30f);
                    var eveningHour = altar.eveningHour + ":00h";
                    if (Widgets.ButtonText(button4, "Cults_EveningSermonStart".Translate() + eveningHour, true, false))
                    {
                        listHours(altar, false);
                    }
                }

                // Old code with only morning/evening setting

                //Widgets.CheckboxLabeled(rect6.BottomHalf(), "MorningSermons".Translate(), ref altar.OptionMorning, disabled);
                //if (Mouse.IsOver(rect6) && Event.current.type == EventType.MouseDown && !disabled)
                //{
                //    altar.TryChangeWorshipValues(Building_SacrificialAltar.ChangeWorshipType.MorningWorship, altar.OptionMorning);
                //}
                //Rect rect7 = rect6;
                //rect7.y += ITab_AltarSacrificesCardUtility.ButtonSize + ITab_AltarSacrificesCardUtility.SpacingOffset;
                //rect7.height = ITab_AltarSacrificesCardUtility.ButtonSize;
                //Widgets.CheckboxLabeled(rect7.TopHalf(), "EveningSermons".Translate(), ref altar.OptionEvening, disabled);
                //if (Mouse.IsOver(rect7) && Event.current.type == EventType.MouseDown && !disabled)
                //{
                //    altar.TryChangeWorshipValues(Building_SacrificialAltar.ChangeWorshipType.EveningWorship, altar.OptionEvening);
                //}

                //TooltipHandler.TipRegion(rect6, "MorningSermonsDesc".Translate());
                //TooltipHandler.TipRegion(rect7, "EveningSermonsDesc".Translate());
            }
            else
            {
                var newRect = new Rect(rect);
                newRect = newRect.ContractedBy(14f);
                newRect.height = 30f;

                Text.Font = GameFont.Medium;
                Widgets.Label(newRect, "Cults_NoPlayerCultAvailable".Translate());
                Text.Font = GameFont.Small;
            }

            GUI.EndGroup();
        }

        public static void listHours(Building_SacrificialAltar altar, bool morning)
        {
            var list = new List<FloatMenuOption>();
            var availableHours = new List<int>(new[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11});
            if (!morning)
            {
                availableHours = new List<int>(new[] {12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23});
            }

            foreach (var i in availableHours)
            {
                list.Add(new FloatMenuOption(i + ":00h", delegate
                {
                    if (morning)
                    {
                        altar.morningHour = i;
                    }
                    else
                    {
                        altar.eveningHour = i;
                    }
                }));
            }


            Find.WindowStack.Add(new FloatMenu(list));
        }

        private static string PreacherLabel(Building_SacrificialAltar altar)
        {
            if (altar.tempPreacher != null)
            {
                return altar.tempPreacher.Name.ToStringShort;
            }

            altar.tempPreacher = CultUtility.DetermineBestPreacher(altar.Map);
            return altar.tempPreacher == null ? "None" : altar.tempPreacher.Name.ToStringShort;
        }

        private static string DeityLabel(Building_SacrificialAltar altar)
        {
            return altar.tempCurrentWorshipDeity == null ? "None" : altar.tempCurrentWorshipDeity.LabelCap;
        }

        private static string DeityDescription(Building_SacrificialAltar altar)
        {
            if (altar.tempCurrentWorshipDeity == null)
            {
                return "None";
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.Append(altar.tempCurrentWorshipDeity.def.description);
            return stringBuilder.ToString();
        }

        public static void OpenPreacherSelectMenu(Building_SacrificialAltar altar)
        {
            var list = new List<FloatMenuOption>
            {
                new FloatMenuOption("(" + "Auto".Translate() + ")",
                    delegate { altar.tempPreacher = CultUtility.DetermineBestPreacher(altar.Map); })
            };

            foreach (var current in CultTracker.Get.PlayerCult.MembersAt(altar.Map))
            {
                if (!current.health.capacities.CapableOf(PawnCapacityDefOf.Talking) ||
                    !current.health.capacities.CapableOf(PawnCapacityDefOf.Moving))
                {
                    continue;
                }

                var localCol = current;

                void Action()
                {
                    //Map.GetComponent<MapComponent_SacrificeTracker>().lastUsedAltar = altar;
                    altar.tempPreacher = localCol;
                }

                list.Add(new FloatMenuOption(localCol.LabelShort, Action));
            }

            Find.WindowStack.Add(new FloatMenu(list));
        }


        public static void OpenDeitySelectMenu(Building_SacrificialAltar altar)
        {
            var list = new List<FloatMenuOption>
            {
                new FloatMenuOption("(" + "NoneLower".Translate() + ")", delegate
                {
                    //Map.GetComponent<MapComponent_SacrificeTracker>().lastUsedAltar = altar;
                    altar.tempCurrentWorshipDeity = null;
                })
            };

            foreach (var current in DeityTracker.Get.DeityCache.Keys)
            {
                if (!current.discovered)
                {
                    continue;
                }

                var localDeity = current;

                void Action()
                {
                    //Map.GetComponent<MapComponent_SacrificeTracker>().lastUsedAltar = altar;
                    altar.tempCurrentWorshipDeity = localDeity;
                    //altar.tempCurrentSpell = null;
                }

                bool extraPartOnGUI(Rect rect)
                {
                    return DeityInfoCardButton(rect.x + 5f, rect.y + ((rect.height - 24f) / 2f), current);
                }

                list.Add(new FloatMenuOption(localDeity.LabelCap, Action, MenuOptionPriority.Default, null, null, 29f,
                    extraPartOnGUI));
            }

            Find.WindowStack.Add(new FloatMenu(list));
        }

        public static bool DeityInfoCardButton(float x, float y, CosmicEntity entity)
        {
            bool result;
            var methodObj = AccessTools.Method(
                typeof(Widgets),
                "InfoCardButtonWorker",
                new Type[]
                {
                                typeof(float),
                                typeof(float)
                });
            if ((bool)methodObj.Invoke(null, new object[] {x, y}))
            {
                Find.WindowStack.Add(new Dialog_CosmicEntityInfoBox(entity));
                result = true;
            }
            else
            {
                result = false;
            }

            return result;
        }
    }
}