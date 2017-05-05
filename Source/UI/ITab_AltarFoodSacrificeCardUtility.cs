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
    public class ITab_AltarFoodSacrificeCardUtility
    {
        public static Vector2 TempleCardSize = new Vector2(500f, 320f);

        public static void DrawTempleCard(Rect rect, Building_SacrificialAltar altar)
        {
            GUI.BeginGroup(rect);
            Rect rect3 = new Rect(2f, 0f, ITab_AltarSacrificesCardUtility.ColumnSize, ITab_AltarSacrificesCardUtility.ButtonSize);
            Widgets.Label(rect3, "Deity".Translate() + ": ");
            rect3.xMin = rect3.center.x - 15f;
            string label2 = DeityLabel(altar);
            if (Widgets.ButtonText(rect3, label2, true, false, true))
            {
                ITab_AltarCardUtility.OpenDeitySelectMenu(altar, ITab_AltarCardUtility.DeityType.OfferingDeity);
            }
            TooltipHandler.TipRegion(rect3, "DeityDesc".Translate());

            ITab_AltarCardUtility.DrawDeity(altar.tempCurrentOfferingDeity, rect3);

            Rect rect4 = rect3;
            rect4.y += ITab_AltarSacrificesCardUtility.ButtonSize + 15f;
            rect4.width = ITab_AltarSacrificesCardUtility.ColumnSize;
            rect4.x -= (rect3.x - 2);
            Widgets.Label(rect4, "Offerer".Translate() + ": ");
            rect4.xMin = rect4.center.x - 15f;
            string label3 = OffererLabel(altar);
            if (Widgets.ButtonText(rect4, label3, true, false, true))
            {
                ITab_AltarCardUtility.OpenActorSelectMenu(altar, ITab_AltarCardUtility.ActorType.offerer);
            }
            TooltipHandler.TipRegion(rect4, "OffererDesc".Translate());

            Rect rect5 = rect4;
            rect5.y += ITab_AltarSacrificesCardUtility.ButtonSize + 15f;
            rect5.x -= (rect4.x - 2);
            //rect5.x += 2f;
            rect5.width = ITab_AltarSacrificesCardUtility.ColumnSize;
            Widgets.Label(rect5, "Offering".Translate() + ": ");
            rect5.xMin = rect5.center.x - 15f;
            string label4 = OfferingLabel(altar);
            if (Widgets.ButtonText(rect5, label4, true, false, true))
            {
                ITab_AltarFoodSacrificeCardUtility.OpenOfferingSelectMenu(altar);
            }
            TooltipHandler.TipRegion(rect5, "OfferingDesc".Translate());

            Rect rect6 = rect5;
            rect6.y += ITab_AltarSacrificesCardUtility.ButtonSize + 15f;
            rect6.x -= (rect5.x - 2);
            //rect5.x += 2f;
            rect6.width = ITab_AltarSacrificesCardUtility.ColumnSize;
            Widgets.Label(rect6, "Amount".Translate() + ": ");
            rect6.xMin = rect6.center.x - 15f;
            string label5 = AmountLabel(altar);
            if (Widgets.ButtonText(rect6, label5, true, false, true))
            {
                ITab_AltarFoodSacrificeCardUtility.OpenAmountSelectMenu(altar);
            }
            TooltipHandler.TipRegion(rect6, "AmountDesc".Translate());


            //Rect rect3 = new Rect(2f, 0, 210f, 25f);
            //Widgets.Label(rect3, "Deity".Translate() + ": ");
            //rect3.xMin = rect3.center.x - 15f;
            //string label2 = DeityLabel(altar);
            //if (Widgets.ButtonText(rect3, label2, true, false, true))
            //{
            //    ITab_AltarHumanSacrificeCardUtility.OpenDeitySelectMenu(altar);
            //}

            //Rect secondBox = rect3;
            //secondBox.x += rect3.x + 10f + 30f;

            //secondBox.xMax += 125f;
            //secondBox.height = 35f;
            //Text.Font = GameFont.Medium;
            //Widgets.Label(secondBox, DeityLabel(altar));
            //Text.Font = GameFont.Small;
            //ITab_CardUtility.DrawTier(altar.tempCurrentSacrificeDeity, new Vector2(secondBox.x, secondBox.y + 30f));
            //Rect secondBoxUnder = secondBox;
            //secondBoxUnder.y += 40f;
            //secondBoxUnder.width -= 15f;
            //secondBoxUnder.height = 70f;
            //Widgets.Label(secondBoxUnder, DeityDescription(altar));
            //Rect secondBoxUnder2 = secondBoxUnder;
            //secondBoxUnder2.y += 70;
            //secondBoxUnder2.height = 250f;
            //Widgets.Label(secondBoxUnder2, SpellDescription(altar));

            //Rect rect5 = rect3;
            //rect5.y += 35f;
            ////rect5.x -= (rect3.x - 5);
            //rect5.x -= 2f;
            //rect5.width = 210f;
            //Widgets.Label(rect5, "Offering".Translate() + ": ");
            //rect5.xMin = rect5.center.x - 15f;
            //string label4 = SacrificeLabel(altar);
            //if (Widgets.ButtonText(rect5, label4, true, false, true))
            //{
            //    ITab_AltarHumanSacrificeCardUtility.OpenSacrificeSelectMenu(altar);
            //}

            GUI.EndGroup();
        }


        private static string OfferingLabel(Building_SacrificialAltar altar)
        {
            if (altar.tempOfferingType == CultUtility.SacrificeType.none)
            {
                return "None";
            }
            else
            {
                return altar.tempOfferingType.ToString();
            }
        }

        private static string OffererLabel(Building_SacrificialAltar altar)
        {
            if (altar.tempOfferer == null)
            {
                return "None";
            }
            else
            {
                return altar.tempOfferer.NameStringShort;
            }
        }

        private static string DeityLabel(Building_SacrificialAltar altar)
        {
            if (altar.tempCurrentOfferingDeity == null)
            {
                return "None";
            }
            else
            {
                return altar.tempCurrentOfferingDeity.LabelCap;
            }
        }


        private static string AmountLabel(Building_SacrificialAltar altar)
        {
            if (altar.tempOfferingSize == CultUtility.OfferingSize.none)
            {
                return "None";
            }
            else
            {
                return altar.tempOfferingSize.ToString();
            }
        }

        private static string DeityDescription(Building_SacrificialAltar altar)
        {
            if (altar.tempCurrentOfferingDeity == null)
            {
                return "None";
            }
            else
            {
                StringBuilder stringBuilder = new StringBuilder();
                //stringBuilder.Append(altar.tempCurrentSacrificeDeity.LabelCap);
                stringBuilder.AppendLine();
                stringBuilder.Append(altar.tempCurrentOfferingDeity.def.description);
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

            public static void OpenAmountSelectMenu(Building_SacrificialAltar altar)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            list.Add(new FloatMenuOption("(" + "NoneLower".Translate() + ")", delegate
            {
                altar.tempOfferingSize = CultUtility.OfferingSize.none;
            }, MenuOptionPriority.Default, null, null, 0f, null));

            Action action;
            action = delegate
            {
                altar.tempOfferingSize = CultUtility.OfferingSize.meagre;
            };
            list.Add(new FloatMenuOption("Meagre".Translate(), action, MenuOptionPriority.Default, null, null, 0f, null));

            Action action2;
            action2 = delegate
            {
                altar.tempOfferingSize = CultUtility.OfferingSize.decent;
            };
            list.Add(new FloatMenuOption("Decent".Translate(), action2, MenuOptionPriority.Default, null, null, 0f, null));

            Action action3;
            action3 = delegate
            {
                altar.tempOfferingSize = CultUtility.OfferingSize.sizable;
            };
            list.Add(new FloatMenuOption("Sizable".Translate(), action3, MenuOptionPriority.Default, null, null, 0f, null));

            Action action4;
            action4 = delegate
            {
                altar.tempOfferingSize = CultUtility.OfferingSize.worthy;
            };
            list.Add(new FloatMenuOption("Worthy".Translate(), action4, MenuOptionPriority.Default, null, null, 0f, null));

            Action action5;
            action5 = delegate
            {
                altar.tempOfferingSize = CultUtility.OfferingSize.impressive;
            };
            list.Add(new FloatMenuOption("Impressive".Translate(), action5, MenuOptionPriority.Default, null, null, 0f, null));

            Find.WindowStack.Add(new FloatMenu(list));
        }

        public static void OpenOfferingSelectMenu(Building_SacrificialAltar altar)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            list.Add(new FloatMenuOption("(" + "NoneLower".Translate() + ")", delegate
            {
                altar.tempOfferingType = CultUtility.SacrificeType.none;
            }, MenuOptionPriority.Default, null, null, 0f, null));

            Action action;
            action = delegate
            {
                altar.tempOfferingType = CultUtility.SacrificeType.plants;
            };
            list.Add(new FloatMenuOption("Plants".Translate(), action, MenuOptionPriority.Default, null, null, 0f, null));

            Action action2;
            action2 = delegate
            {
                altar.tempOfferingType = CultUtility.SacrificeType.meat;
            };
            list.Add(new FloatMenuOption("Meat".Translate(), action2, MenuOptionPriority.Default, null, null, 0f, null));

            Action action3;
            action3 = delegate
            {
                altar.tempOfferingType = CultUtility.SacrificeType.meals;
            };
            list.Add(new FloatMenuOption("Meals".Translate(), action3, MenuOptionPriority.Default, null, null, 0f, null));
            
            Find.WindowStack.Add(new FloatMenu(list));
        }

    }
}
