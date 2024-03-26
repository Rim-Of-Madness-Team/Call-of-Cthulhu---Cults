// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using CallOfCthulhu;
using HarmonyLib;
using Multiplayer.API;
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
        public static Vector2 TempleCardSize = new Vector2(x: 600f, y: 500f);

        public static void DrawTempleCard(Rect rect, Building_SacrificialAltar altar)
        {
            GUI.BeginGroup(position: rect);

            if (CultTracker.Get.PlayerCult != null)
            {
                var cultLabelWidth = Text.CalcSize(text: CultTracker.Get.PlayerCult.name).x + 15;

                //Headings
                _ = new Rect(source: rect);
                var rect1 = rect.ContractedBy(margin: 14f);
                rect1.height = 30f;

                //Unnamed Temple
                Text.Font = GameFont.Medium;
                Widgets.Label(rect: rect1, label: altar.RoomName);
                Text.Font = GameFont.Small;

                //Rename Icon
                ITab_AltarCardUtility.DrawRename(altar: altar);
                var rect2 = new Rect(source: rect1)
                {
                    yMin = rect1.yMax + 10,
                    height = 25f,
                    width = cultLabelWidth + 5
                };

                //Esoteric Order of Dagon
                Widgets.Label(rect: rect2, label: CultTracker.Get.PlayerCult.name);
                if (Mouse.IsOver(rect: rect2))
                {
                    Widgets.DrawHighlight(rect: rect2);
                }

                if (Mouse.IsOver(rect: rect2) && Event.current.type == EventType.MouseDown)
                {
                    Find.WindowStack.Add(window: new Dialog_RenameCult(newMap: altar.Map));
                }

                Widgets.DrawLineHorizontal(x: rect2.x - 10, y: rect2.yMax, length: rect.width - 15f);
                //---------------------------------------------------------------------

                var rectMain = new Rect(x: 0 + 15f, y: 0 + 30f, width: TempleCardSize.x,
                    height: ITab_AltarSacrificesCardUtility.ButtonSize * 1.15f);

                //Deity -> Cthulhu
                var rect4 = rectMain;
                rect4.yMin = rectMain.yMax + 5f;
                rect4.y = rectMain.yMax + 20f;
                rect4.x += 5f;
                rect4.width = ITab_AltarSacrificesCardUtility.ColumnSize;
                rect4.height = ITab_AltarSacrificesCardUtility.ButtonSize;
                Widgets.Label(rect: rect4, label: "Deity".Translate() + ": ");
                rect4.xMin = rect4.center.x;
                var label4 = DeityLabel(altar: altar);
                if (Widgets.ButtonText(rect: rect4, label: label4, drawBackground: true, doMouseoverSound: false))
                {
                    OpenDeitySelectMenu(altar: altar);
                }

                TooltipHandler.TipRegion(rect: rect4, tip: "DeityDesc".Translate());

                //Cthulhu - He who waits dreaming.
                ITab_AltarCardUtility.DrawDeity(entity: altar.tempCurrentWorshipDeity, rect3: rect4, spellDescription: null, offset: -30f);

                //Preacher
                var rect5 = rect4;
                rect5.y += ITab_AltarSacrificesCardUtility.ButtonSize + 15f;
                //rect5.y = rect4.yMax + 30f;
                rect5.x -= rect4.x - 5;
                rect5.x += 15f;
                rect5.width = ITab_AltarSacrificesCardUtility.ColumnSize;
                Widgets.Label(rect: rect5, label: "Preacher".Translate() + ": ");
                rect5.xMin = rect5.center.x;
                var label2 = PreacherLabel(altar: altar);
                if (Widgets.ButtonText(rect: rect5, label: label2, drawBackground: true, doMouseoverSound: false))
                {
                    OpenPreacherSelectMenu(altar: altar);
                }

                TooltipHandler.TipRegion(rect: rect5, tip: "PreacherDesc".Translate());

                var rect6 = rect5;
                rect6.y += ITab_AltarSacrificesCardUtility.ButtonSize + ITab_AltarSacrificesCardUtility.SpacingOffset;
                rect6.height = ITab_AltarSacrificesCardUtility.ButtonSize * 2;
                rect6.width = ITab_AltarSacrificesCardUtility.ColumnSize;
                rect6.x -= rect5.x - 5;
                rect6.x += 15f;
                if (altar.tempCurrentWorshipDeity != null)
                {
                    Widgets.Label(rect: rect6.BottomHalf(), label: "Cults_SeasonDays".Translate());

                    Text.Font = GameFont.Tiny;
                    //Text.Anchor = TextAnchor.LowerLeft;
                    var num = 15f;
                    var num2 = 270f;
                    var hourWidth = 20.833334f;
                    for (var day = 0; day <= 14; day++)
                    {
                        var rect9 = new Rect(x: num + 4f, y: num2 + 0f, width: hourWidth, height: 20f);
                        Widgets.Label(rect: rect9, label: (day + 1).ToString());
                        var rect10 = new Rect(x: num, y: num2 + 20f, width: hourWidth, height: 30f);
                        rect10 = rect10.ContractedBy(margin: 1f);
                        var texture = TimeAssignmentDefOf.Anything.ColorTexture;
                        switch (altar.seasonSchedule[index: day])
                        {
                            case 1:
                                texture = SolidColorMaterials.NewSolidColorTexture(color: Color.red);
                                break;
                            case 2:
                                texture = SolidColorMaterials.NewSolidColorTexture(color: Color.blue);
                                break;
                            case 3:
                                texture = SolidColorMaterials.NewSolidColorTexture(color: Color.magenta);
                                break;
                        }

                        GUI.DrawTexture(position: rect10, image: texture);
                        if (Mouse.IsOver(rect: rect10))
                        {
                            Widgets.DrawBox(rect: rect10, thickness: 2);
                            //if (Input.GetMouseButton(0))
                            if (Widgets.ButtonInvisible(butRect: rect10))
                            {
                                altar.seasonSchedule[index: day] = (altar.seasonSchedule[index: day] % 4) + 1;
                                SoundDefOf.Designate_DragStandard_Changed.PlayOneShotOnCamera();
                                //p.timetable.SetAssignment(hour, this.selectedAssignment);
                            }
                        }

                        num += hourWidth;
                    }

                    num2 += 60f;
                    var rect11 = new Rect(x: 15f, y: num2 + 3, width: hourWidth / 2, height: hourWidth / 2);
                    rect11 = rect11.ContractedBy(margin: 1f);
                    GUI.DrawTexture(position: rect11, image: TimeAssignmentDefOf.Anything.ColorTexture);
                    var rect12 = new Rect(x: 15f + hourWidth, y: num2, width: 150f, height: (hourWidth / 2) + 6);
                    Widgets.Label(rect: rect12, label: "NoSermonLabel".Translate());

                    var rect13 = new Rect(x: 15f + 170f, y: num2 + 3, width: hourWidth / 2, height: hourWidth / 2);
                    rect13 = rect13.ContractedBy(margin: 1f);
                    GUI.DrawTexture(position: rect13, image: SolidColorMaterials.NewSolidColorTexture(color: Color.magenta));
                    var rect14 = new Rect(x: 15f + hourWidth + 170f, y: num2, width: 150f, height: (hourWidth / 2) + 6);
                    Widgets.Label(rect: rect14, label: "BothSermonLabel".Translate());

                    num2 += 30f;
                    var rect15 = new Rect(x: 15f, y: num2 + 3, width: hourWidth / 2, height: hourWidth / 2);
                    rect15 = rect15.ContractedBy(margin: 1f);
                    GUI.DrawTexture(position: rect15, image: SolidColorMaterials.NewSolidColorTexture(color: Color.red));
                    var rect16 = new Rect(x: 15f + hourWidth, y: num2, width: 150f, height: (hourWidth / 2) + 6);
                    Widgets.Label(rect: rect16, label: "MorningSermonLabel".Translate());

                    var rect17 = new Rect(x: 15f + 170f, y: num2 + 3, width: hourWidth / 2, height: hourWidth / 2);
                    rect17 = rect17.ContractedBy(margin: 1f);
                    GUI.DrawTexture(position: rect17, image: SolidColorMaterials.NewSolidColorTexture(color: Color.blue));
                    var rect18 = new Rect(x: 15f + hourWidth + 170f, y: num2, width: 150f, height: (hourWidth / 2) + 6);
                    Widgets.Label(rect: rect18, label: "EveningSermonLabel".Translate());

                    num2 += 35f;
                    var rect19 = new Rect(x: 15f, y: num2, width: 150f, height: (hourWidth / 2) + 6);
                    Widgets.Label(rect: rect19, label: "Cults_SermonStartLabel".Translate());

                    var dist = 5f;
                    var button3 = new Rect(x: rect6.x + dist, y: rect6.y + 215f, width: 140f, height: 30f);
                    var morningHour = altar.morningHour + ":00h";
                    if (Widgets.ButtonText(rect: button3, label: "Cults_MorningSermonStart".Translate() + morningHour, drawBackground: true, doMouseoverSound: false))
                    {
                        listHours(altar: altar, morning: true);
                    }

                    var button4 = new Rect(x: rect6.x + dist + 150f, y: rect6.y + 215f, width: 140f, height: 30f);
                    var eveningHour = altar.eveningHour + ":00h";
                    if (Widgets.ButtonText(rect: button4, label: "Cults_EveningSermonStart".Translate() + eveningHour, drawBackground: true, doMouseoverSound: false))
                    {
                        listHours(altar: altar, morning: false);
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
                var newRect = new Rect(source: rect);
                newRect = newRect.ContractedBy(margin: 14f);
                newRect.height = 30f;

                Text.Font = GameFont.Medium;
                Widgets.Label(rect: newRect, label: "Cults_NoPlayerCultAvailable".Translate());
                Text.Font = GameFont.Small;
            }

            GUI.EndGroup();
        }

        public static void listHours(Building_SacrificialAltar altar, bool morning)
        {
            var list = new List<FloatMenuOption>();
            var availableHours = new List<int>(collection: new[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11});
            if (!morning)
            {
                availableHours = new List<int>(collection: new[] {12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23});
            }

            foreach (var i in availableHours)
            {
                list.Add(item: new FloatMenuOption(label: i + ":00h", action: delegate
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


            Find.WindowStack.Add(window: new FloatMenu(options: list));
        }

        [SyncMethod]
        private static string PreacherLabel(Building_SacrificialAltar altar)
        {
            if (altar.tempPreacher != null)
            {
                return altar.tempPreacher.Name.ToStringShort;
            }

            altar.tempPreacher = CultUtility.DetermineBestPreacher(map: altar.Map);
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
            stringBuilder.Append(value: altar.tempCurrentWorshipDeity.def.description);
            return stringBuilder.ToString();
        }

        public static void OpenPreacherSelectMenu(Building_SacrificialAltar altar)
        {
            var list = new List<FloatMenuOption>
            {
                new FloatMenuOption(label: "(" + "Auto".Translate() + ")",
                    action: delegate { altar.tempPreacher = CultUtility.DetermineBestPreacher(map: altar.Map); })
            };

            foreach (var current in CultTracker.Get.PlayerCult.MembersAt(map: altar.Map))
            {
                if (!current.health.capacities.CapableOf(capacity: PawnCapacityDefOf.Talking) ||
                    !current.health.capacities.CapableOf(capacity: PawnCapacityDefOf.Moving))
                {
                    continue;
                }

                var localCol = current;

                //void Action()
                ////{
                ////    //Map.GetComponent<MapComponent_SacrificeTracker>().lastUsedAltar = altar;
                ////    altar.tempPreacher = localCol;
                ////}
                //Line below was added to make delegate action a private static void for syncmethod.
                Action selectAction = () => PreacherSelectMenuAction(altar: altar, localCol: localCol);
                list.Add(item: new FloatMenuOption(label: localCol.LabelShort, action: selectAction));
            }

            Find.WindowStack.Add(window: new FloatMenu(options: list));
        }
        [SyncMethod]
        private static void PreacherSelectMenuAction(Building_SacrificialAltar altar, Pawn localCol)
        {
            //Map.GetComponent<MapComponent_SacrificeTracker>().lastUsedAltar = altar;
            altar.tempPreacher = localCol;
        }

        public static void OpenDeitySelectMenu(Building_SacrificialAltar altar)
        {
            var list = new List<FloatMenuOption>
            {
                new FloatMenuOption(label: "(" + "NoneLower".Translate() + ")", action: delegate
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
                    return DeityInfoCardButton(x: rect.x + 5f, y: rect.y + ((rect.height - 24f) / 2f), entity: current);
                }

                list.Add(item: new FloatMenuOption(label: localDeity.LabelCap, action: Action, priority: MenuOptionPriority.Default, mouseoverGuiAction: null, revalidateClickTarget: null, extraPartWidth: 29f,
                    extraPartOnGUI: extraPartOnGUI));
            }

            Find.WindowStack.Add(window: new FloatMenu(options: list));
        }

        public static bool DeityInfoCardButton(float x, float y, CosmicEntity entity)
        {
            bool result;
            var methodObj = AccessTools.Method(
                type: typeof(Widgets),
                name: "InfoCardButtonWorker",
                parameters: new Type[]
                {
                                typeof(float),
                                typeof(float)
                });
            if ((bool)methodObj.Invoke(obj: null, parameters: new object[] {x, y}))
            {
                Find.WindowStack.Add(window: new Dialog_CosmicEntityInfoBox(entity: entity));
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