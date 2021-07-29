// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using System.Collections.Generic;
using System.Text;
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
    public class ITab_AltarFoodSacrificeCardUtility
    {
        public static Vector2 TempleCardSize = new Vector2(500f, 320f);

        public static void DrawTempleCard(Rect rect, Building_SacrificialAltar altar)
        {
            GUI.BeginGroup(rect);
            var rect3 = new Rect(2f, 0f, ITab_AltarSacrificesCardUtility.ColumnSize,
                ITab_AltarSacrificesCardUtility.ButtonSize);
            Widgets.Label(rect3, "Deity".Translate() + ": ");
            rect3.xMin = rect3.center.x - 15f;
            var label2 = DeityLabel(altar);
            if (Widgets.ButtonText(rect3, label2, true, false))
            {
                ITab_AltarCardUtility.OpenDeitySelectMenu(altar, ITab_AltarCardUtility.DeityType.OfferingDeity);
            }

            TooltipHandler.TipRegion(rect3, "DeityDesc".Translate());

            ITab_AltarCardUtility.DrawDeity(altar.tempCurrentOfferingDeity, rect3);

            var rect4 = rect3;
            rect4.y += ITab_AltarSacrificesCardUtility.ButtonSize + 15f;
            rect4.width = ITab_AltarSacrificesCardUtility.ColumnSize;
            rect4.x -= rect3.x - 2;
            Widgets.Label(rect4, "Offerer".Translate() + ": ");
            rect4.xMin = rect4.center.x - 15f;
            var label3 = OffererLabel(altar);
            if (Widgets.ButtonText(rect4, label3, true, false))
            {
                ITab_AltarCardUtility.OpenActorSelectMenu(altar, ITab_AltarCardUtility.ActorType.offerer);
            }

            TooltipHandler.TipRegion(rect4, "OffererDesc".Translate());

            var rect5 = rect4;
            rect5.y += ITab_AltarSacrificesCardUtility.ButtonSize + 15f;
            rect5.x -= rect4.x - 2;
            //rect5.x += 2f;
            rect5.width = ITab_AltarSacrificesCardUtility.ColumnSize;
            Widgets.Label(rect5, "Offering".Translate() + ": ");
            rect5.xMin = rect5.center.x - 15f;
            var label4 = OfferingLabel(altar);
            if (Widgets.ButtonText(rect5, label4, true, false))
            {
                OpenOfferingSelectMenu(altar);
            }

            TooltipHandler.TipRegion(rect5, "OfferingDesc".Translate());

            var rect6 = rect5;
            rect6.y += ITab_AltarSacrificesCardUtility.ButtonSize + 15f;
            rect6.x -= rect5.x - 2;
            //rect5.x += 2f;
            rect6.width = ITab_AltarSacrificesCardUtility.ColumnSize;
            Widgets.Label(rect6, "Amount".Translate() + ": ");
            rect6.xMin = rect6.center.x - 15f;
            var label5 = AmountLabel(altar);
            if (Widgets.ButtonText(rect6, label5, true, false))
            {
                OpenAmountSelectMenu(altar);
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
            return altar.tempOfferingType == CultUtility.SacrificeType.none
                ? "None"
                : altar.tempOfferingType.ToString().CapitalizeFirst();
        }

        private static string OffererLabel(Building_SacrificialAltar altar)
        {
            return altar.tempOfferer == null ? "None" : altar.tempOfferer.Name.ToStringShort;
        }

        private static string DeityLabel(Building_SacrificialAltar altar)
        {
            return altar.tempCurrentOfferingDeity == null ? "None" : altar.tempCurrentOfferingDeity.LabelCap;
        }


        private static string AmountLabel(Building_SacrificialAltar altar)
        {
            return altar.tempOfferingSize == CultUtility.OfferingSize.none
                ? "None"
                : altar.tempOfferingSize.ToString().CapitalizeFirst();
        }

        private static string DeityDescription(Building_SacrificialAltar altar)
        {
            if (altar.tempCurrentOfferingDeity == null)
            {
                return "None";
            }

            var stringBuilder = new StringBuilder();
            //stringBuilder.Append(altar.tempCurrentSacrificeDeity.LabelCap);
            stringBuilder.AppendLine();
            stringBuilder.Append(altar.tempCurrentOfferingDeity.def.description);
            //stringBuilder.AppendLine();
            //stringBuilder.Append("Standing: " + altar.tempCurrentSacrificeDeity.PlayerFavor.ToString());
            //stringBuilder.AppendLine();
            //stringBuilder.Append("Tier: " + altar.tempCurrentSacrificeDeity.PlayerTier.ToString());
            return stringBuilder.ToString();
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
                        p.CurJob.EndCurrentJob(JobCondition.InterruptForced);
                    }
                    else
                    {
                        Job J = new Job(CorruptionDefOfs.AttendSermon, altar.preacher, altar, result);
                        p.QueueJob(J);
                        p.CurJob.EndCurrentJob(JobCondition.InterruptForced);
                    }
                }
            }
        }
        */

        public static void OpenAmountSelectMenu(Building_SacrificialAltar altar)
        {
            var list = new List<FloatMenuOption>
            {
                new FloatMenuOption("(" + "NoneLower".Translate() + ")",
                    delegate { altar.tempOfferingSize = CultUtility.OfferingSize.none; })
            };

            void action()
            {
                altar.tempOfferingSize = CultUtility.OfferingSize.meagre;
            }

            list.Add(new FloatMenuOption("Meagre".Translate(), action));

            void action2()
            {
                altar.tempOfferingSize = CultUtility.OfferingSize.decent;
            }

            list.Add(new FloatMenuOption("Decent".Translate(), action2));

            void action3()
            {
                altar.tempOfferingSize = CultUtility.OfferingSize.sizable;
            }

            list.Add(new FloatMenuOption("Sizable".Translate(), action3));

            void action4()
            {
                altar.tempOfferingSize = CultUtility.OfferingSize.worthy;
            }

            list.Add(new FloatMenuOption("Worthy".Translate(), action4));

            void action5()
            {
                altar.tempOfferingSize = CultUtility.OfferingSize.impressive;
            }

            list.Add(new FloatMenuOption("Impressive".Translate(), action5));

            Find.WindowStack.Add(new FloatMenu(list));
        }

        public static void OpenOfferingSelectMenu(Building_SacrificialAltar altar)
        {
            var list = new List<FloatMenuOption>
            {
                new FloatMenuOption("(" + "NoneLower".Translate() + ")",
                    delegate { altar.tempOfferingType = CultUtility.SacrificeType.none; })
            };

            void action()
            {
                altar.tempOfferingType = CultUtility.SacrificeType.plants;
            }

            list.Add(new FloatMenuOption("Plants".Translate(), action));

            void action2()
            {
                altar.tempOfferingType = CultUtility.SacrificeType.meat;
            }

            list.Add(new FloatMenuOption("Meat".Translate(), action2));

            void action3()
            {
                altar.tempOfferingType = CultUtility.SacrificeType.meals;
            }

            list.Add(new FloatMenuOption("Meals".Translate(), action3));

            Find.WindowStack.Add(new FloatMenu(list));
        }
    }
}