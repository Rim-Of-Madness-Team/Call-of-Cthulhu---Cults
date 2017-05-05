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
    public class ITab_AltarSacrificeCardUtility
    {
        public static Vector2 TempleCardSize = new Vector2(500f, 320f);

        public static void DrawRename(Building_SacrificialAltar altar)
        {
            Rect rectRename = new Rect(ITab_AltarWorshipCardUtility.TempleCardSize.x - 85f, 0f, 30f, 30f);
            TooltipHandler.TipRegion(rectRename, "RenameTemple".Translate());
            if (Widgets.ButtonImage(rectRename, Buttons.RenameTex))
            {
                Find.WindowStack.Add(new Dialog_RenameTemple(altar));
            }
        }

        public static void DrawTempleCard(Rect rect, Building_SacrificialAltar altar)
        {
            GUI.BeginGroup(rect);
            Rect rect2 = new Rect(rect.x + 2f, rect.y + 20f, 250f, 55f);
            Text.Font = GameFont.Medium;
            Widgets.Label(rect2, altar.RoomName);
            Text.Font = GameFont.Small;

            DrawRename(altar);

            Rect rect3 = rect2;
            //rect3.x += 2f;
            rect3.y = rect2.yMax + 30f;
            rect3.width = 210f;
            rect3.height = 25f;
            Widgets.Label(rect3, "Deity".Translate() + ": ");
            rect3.xMin = rect3.center.x - 15f;
            string label2 = DeityLabel(altar);
            if (Widgets.ButtonText(rect3, label2, true, false, true))
            {
                ITab_AltarSacrificeCardUtility.OpenDeitySelectMenu(altar);
            }

            Rect secondBox = rect3;
            secondBox.x += rect3.x + 10f + 30f;
            secondBox.xMax += 125f;
            secondBox.height = 35f;
            Text.Font = GameFont.Medium;
            Widgets.Label(secondBox, DeityLabel(altar));
            Text.Font = GameFont.Small;
            ITab_CardUtility.DrawTier(altar.tempCurrentSacrificeDeity, new Vector2(secondBox.x, secondBox.y + 30f));
            Rect secondBoxUnder = secondBox;
            secondBoxUnder.y += 40f;
            secondBoxUnder.width -= 15f;
            secondBoxUnder.height = 70f;
            Widgets.Label(secondBoxUnder, DeityDescription(altar));
            Rect secondBoxUnder2 = secondBoxUnder;
            secondBoxUnder2.y += 70;
            secondBoxUnder2.height = 250f;
            Widgets.Label(secondBoxUnder2, SpellDescription(altar));

            Rect rect4 = rect3;
            rect4.y += 35f;
            rect4.width = 210f;
            rect4.x -= (rect3.x - 5);
            rect4.x += 2f;
            Widgets.Label(rect4, "Executioner".Translate() + ": ");
            rect4.xMin = rect4.center.x - 15f;
            string label3 = ExecutionerLabel(altar);
            if (Widgets.ButtonText(rect4, label3, true, false, true))
            {
                ITab_AltarSacrificeCardUtility.OpenExecutionerSelectMenu(altar);
            }

            Rect rect5 = rect4;
            rect5.y += 35f;
            rect5.x -= (rect4.x - 5);
            rect5.x += 2f;
            rect5.width = 210f;
            Widgets.Label(rect5, "Sacrifice".Translate() + ": ");
            rect5.xMin = rect5.center.x - 15f;
            string label4 = SacrificeLabel(altar);
            if (Widgets.ButtonText(rect5, label4, true, false, true))
            {
                ITab_AltarSacrificeCardUtility.OpenSacrificeSelectMenu(altar);
            }

            Rect rect6 = rect5;
            rect6.y += 35f;
            rect6.x -= (rect5.x - 5);
            rect6.x += 2f;
            rect6.width = 210f;
            rect6.yMax += 35f;
            Widgets.Label(rect6, "Spell".Translate() + ": ");
            rect6.xMin = rect6.center.x - 15f;
            string label5 = SpellLabel(altar);
            if (Widgets.ButtonText(rect6, label5, true, false, true))
            {
                ITab_AltarSacrificeCardUtility.OpenSpellSelectMenu(altar);
            }

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


        private static string SacrificeLabel(Building_SacrificialAltar altar)
        {
            if (altar.tempSacrifice == null)
            {
                return "None";
            }
            else
            {
                return altar.tempSacrifice.NameStringShort;
            }
        }

        private static string ExecutionerLabel(Building_SacrificialAltar altar)
        {
            if (altar.tempExecutioner == null)
            {
                return "None";
            }
            else
            {
                return altar.tempExecutioner.NameStringShort;
            }
        }

        private static string DeityLabel(Building_SacrificialAltar altar)
        {
            if (altar.tempCurrentSacrificeDeity == null)
            {
                return "None";
            }
            else
            {
                return altar.tempCurrentSacrificeDeity.LabelCap;
            }
        }


        private static string SpellLabel(Building_SacrificialAltar altar)
        {
            if (altar.tempCurrentSacrificeDeity == null || altar.tempCurrentSpell == null)
            {
                return "None";
            }
            else
            {
                return altar.tempCurrentSpell.LabelCap;
            }
        }


        private static string SpellDescription(Building_SacrificialAltar altar)
        {
            if (altar.tempCurrentSacrificeDeity == null || altar.tempCurrentSpell == null)
            {
                return "None";
            }
            else
            {
                return altar.tempCurrentSpell.description;
            }
        }

        private static string DeityDescription(Building_SacrificialAltar altar)
        {
            if (altar.tempCurrentSacrificeDeity == null)
            {
                return "None";
            }
            else
            {
                StringBuilder stringBuilder = new StringBuilder();
                //stringBuilder.Append(altar.tempCurrentSacrificeDeity.LabelCap);
                stringBuilder.AppendLine();
                stringBuilder.Append(altar.tempCurrentSacrificeDeity.def.description);
                //stringBuilder.AppendLine();
                //stringBuilder.Append("Standing: " + altar.tempCurrentSacrificeDeity.PlayerFavor.ToString());
                //stringBuilder.AppendLine();
                //stringBuilder.Append("Tier: " + altar.tempCurrentSacrificeDeity.PlayerTier.ToString());
                return stringBuilder.ToString();
            }
        }

        /*
        private static void ForceListenersTest(Building_SacrificialAltar altar)
        {

            IntVec3 result;
            Building chair;
            foreach (Pawn p in Find.MapPawns.AllPawnsSpawned.FindAll(x => x.RaceProps.intelligence == Intelligence.Humanlike))
            {
                if (!SermonUtility.IsPreacher(p))
                {
                    if (!WatchBuildingUtility.TryFindBestWatchCell(altar, p, true, out result, out chair))
                    {

                        if (!WatchBuildingUtility.TryFindBestWatchCell(altar as Thing, p, false, out result, out chair))
                        {
                            return;
                        }
                    }
                    if (chair != null)
                    {
                        Job J = new Job(CorruptionDefOfs.AttendSermon, altar.preacher, altar, chair);
                        p.QueueJob(J);
                        p.jobs.EndCurrentJob(JobCondition.InterruptForced);
                    }
                    else
                    {
                        Job J = new Job(CorruptionDefOfs.AttendSermon, altar.preacher, altar, result);
                        p.QueueJob(J);
                        p.jobs.EndCurrentJob(JobCondition.InterruptForced);
                    }
                }
            }
        }
        */

        public static void OpenSacrificeSelectMenu(Building_SacrificialAltar altar)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            list.Add(new FloatMenuOption("(" + "NoneLower".Translate() + ")", delegate
            {
                altar.tempSacrifice = null;
            }, MenuOptionPriority.Medium, null, null, 0f, null));

            foreach (Pawn current in Find.MapPawns.PrisonersOfColony)
            {
                    Action action;
                    Pawn localCol = current;
                    action = delegate
                    {
                        MapComponent_SacrificeTracker.Get.lastUsedAltar = altar;
                        altar.tempSacrifice = localCol;
                    };
                    list.Add(new FloatMenuOption(localCol.LabelShort, action, MenuOptionPriority.Medium, null, null, 0f, null));
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }

        public static void OpenExecutionerSelectMenu(Building_SacrificialAltar altar)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            list.Add(new FloatMenuOption("(" + "NoneLower".Translate() + ")", delegate
            {
                altar.tempExecutioner = null;
            }, MenuOptionPriority.Medium, null, null, 0f, null));

            foreach (Pawn current in Find.MapPawns.FreeColonists)
            {
                Action action;
                Pawn localCol = current;
                action = delegate
                {

                    MapComponent_SacrificeTracker.Get.lastUsedAltar = altar;
                    altar.tempExecutioner = localCol;
                };
                list.Add(new FloatMenuOption(localCol.LabelShort, action, MenuOptionPriority.Medium, null, null, 0f, null));
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }


        public static void OpenDeitySelectMenu(Building_SacrificialAltar altar)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            list.Add(new FloatMenuOption("(" + "NoneLower".Translate() + ")", delegate
            {
                MapComponent_SacrificeTracker.Get.lastUsedAltar = altar;
                altar.tempCurrentSacrificeDeity = null;
            }, MenuOptionPriority.Medium, null, null, 0f, null));

            foreach (CosmicEntity current in MapComponent_CosmicDeities.Get.DeityCache.Keys)
            {
                if (current.discovered)
                {
                    Action action;
                    CosmicEntity localDeity = current;
                    action = delegate
                    {

                        MapComponent_SacrificeTracker.Get.lastUsedAltar = altar;
                        altar.tempCurrentSacrificeDeity = localDeity;
                        altar.tempCurrentSpell = null;
                    };
                    list.Add(new FloatMenuOption(localDeity.LabelCap, action, MenuOptionPriority.Medium, null, null, 0f, null));
                }
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }

        public static void OpenSpellSelectMenu(Building_SacrificialAltar altar)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            list.Add(new FloatMenuOption("(" + "NoneLower".Translate() + ")", delegate
            {
                altar.tempCurrentSpell = null;
            }, MenuOptionPriority.Medium, null, null, 0f, null));

            if (altar.tempCurrentSacrificeDeity != null)
            {
                if (altar.tempCurrentSacrificeDeity.tier1Spells != null && altar.tempCurrentSacrificeDeity.PlayerTier > 0)
                {
                    foreach (IncidentDef spell in altar.tempCurrentSacrificeDeity.tier1Spells)
                    {
                        Action action;
                        IncidentDef localSpell = spell;
                        action = delegate
                        {

                            MapComponent_SacrificeTracker.Get.lastUsedAltar = altar;
                            altar.tempCurrentSpell = localSpell;
                        };
                        list.Add(new FloatMenuOption(localSpell.LabelCap, action, MenuOptionPriority.Medium, null, null, 0f, null));
                    }
                }
                if (altar.tempCurrentSacrificeDeity.tier2Spells != null && altar.tempCurrentSacrificeDeity.PlayerTier > CosmicEntity.Tier.One)
                {

                    foreach (IncidentDef spell in altar.tempCurrentSacrificeDeity.tier2Spells)
                    {
                        Action action2;
                        IncidentDef localSpell = spell;
                        action2 = delegate
                        {

                            MapComponent_SacrificeTracker.Get.lastUsedAltar = altar;
                            altar.tempCurrentSpell = localSpell;
                        };
                        list.Add(new FloatMenuOption(localSpell.LabelCap, action2, MenuOptionPriority.Medium, null, null, 0f, null));
                    }
                }

                if (altar.tempCurrentSacrificeDeity.tier3Spells != null && altar.tempCurrentSacrificeDeity.PlayerTier > CosmicEntity.Tier.Two)
                {

                    foreach (IncidentDef spell in altar.tempCurrentSacrificeDeity.tier3Spells)
                    {
                        Action action3;
                        IncidentDef localSpell = spell;
                        action3 = delegate
                        {

                            MapComponent_SacrificeTracker.Get.lastUsedAltar = altar;
                            altar.tempCurrentSpell = localSpell;
                        };
                        list.Add(new FloatMenuOption(localSpell.LabelCap, action3, MenuOptionPriority.Medium, null, null, 0f, null));
                    }
                }
                if (altar.tempCurrentSacrificeDeity.finalSpell != null && altar.tempCurrentSacrificeDeity.PlayerTier > CosmicEntity.Tier.Three)
                {
                    Action action4;
                    IncidentDef localSpell = altar.tempCurrentSacrificeDeity.finalSpell;
                    action4 = delegate
                    {

                        MapComponent_SacrificeTracker.Get.lastUsedAltar = altar;
                        altar.tempCurrentSpell = localSpell;
                    };
                    list.Add(new FloatMenuOption(localSpell.LabelCap, action4, MenuOptionPriority.Medium, null, null, 0f, null));
                }
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }
    }
}
