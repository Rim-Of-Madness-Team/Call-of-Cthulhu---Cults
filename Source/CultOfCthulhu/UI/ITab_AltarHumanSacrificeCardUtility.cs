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
    public class ITab_AltarHumanSacrificeCardUtility
    {
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

            ITab_AltarCardUtility.DrawDeity(altar.tempCurrentSacrificeDeity, rect3, SpellDescription(altar));

            var rect4 = rect3;
            rect4.y += ITab_AltarSacrificesCardUtility.ButtonSize + 15f;
            rect4.width = ITab_AltarSacrificesCardUtility.ColumnSize;
            rect4.x -= rect3.x;
            rect4.x += 2f;
            Widgets.Label(rect4, "Executioner".Translate() + ": ");
            rect4.xMin = rect4.center.x - 15f;
            var label3 = ITab_AltarCardUtility.ExecutionerLabel(altar);
            if (Widgets.ButtonText(rect4, label3, true, false))
            {
                ITab_AltarCardUtility.OpenActorSelectMenu(altar, ITab_AltarCardUtility.ActorType.executioner);
            }

            TooltipHandler.TipRegion(rect4, "ExecutionerDesc".Translate());

            var rect5 = rect4;
            rect5.y += ITab_AltarSacrificesCardUtility.ButtonSize + 15f;
            rect5.x -= rect4.x;
            rect5.x += 2f;
            rect5.width = ITab_AltarSacrificesCardUtility.ColumnSize;
            Widgets.Label(rect5, "Sacrifice".Translate() + ": ");
            rect5.xMin = rect5.center.x - 15f;
            var label4 = ITab_AltarCardUtility.SacrificeLabel(altar);
            if (Widgets.ButtonText(rect5, label4, true, false))
            {
                ITab_AltarCardUtility.OpenActorSelectMenu(altar, ITab_AltarCardUtility.ActorType.prisoner);
            }

            TooltipHandler.TipRegion(rect5, "SacrificeDesc".Translate());

            var rect6 = rect5;
            rect6.y += ITab_AltarSacrificesCardUtility.ButtonSize + 15f;
            rect6.x -= rect5.x;
            rect6.x += 2f;
            rect6.width = ITab_AltarSacrificesCardUtility.ColumnSize;
            rect6.yMax += ITab_AltarSacrificesCardUtility.ButtonSize + 15f;
            Widgets.Label(rect6, "Cults_Spell".Translate() + ": ");
            rect6.xMin = rect6.center.x - 15f;
            var label5 = SpellLabel(altar);
            if (Widgets.ButtonText(rect6, label5, true, false))
            {
                OpenSpellSelectMenu(altar);
            }

            TooltipHandler.TipRegion(rect6, "Cults_SpellDesc".Translate());

            GUI.EndGroup();
        }


        //private static string SacrificeLabel(Building_SacrificialAltar altar)
        //{
        //    if (altar.tempSacrifice == null)
        //    {
        //        return "None";
        //    }
        //    else
        //    {
        //        return altar.tempSacrifice.Name.ToStringShort;
        //    }
        //}

        //private static string ExecutionerLabel(Building_SacrificialAltar altar)
        //{
        //    if (altar.tempExecutioner == null)
        //    {
        //        return "None";
        //    }
        //    else
        //    {
        //        return altar.tempExecutioner.Name.ToStringShort;
        //    }
        //}

        //private static string DeityLabel(Building_SacrificialAltar altar)
        //{
        //    if (altar.tempCurrentSacrificeDeity == null)
        //    {
        //        return "None";
        //    }
        //    else
        //    {
        //        return altar.tempCurrentSacrificeDeity.LabelCap;
        //    }
        //}


        private static string SpellLabel(Building_SacrificialAltar altar)
        {
            return altar.tempCurrentSacrificeDeity == null || altar.tempCurrentSpell == null
                ? "None"
                : (string) altar.tempCurrentSpell.LabelCap;
        }


        private static string SpellDescription(Building_SacrificialAltar altar)
        {
            return altar.tempCurrentSacrificeDeity == null || altar.tempCurrentSpell == null
                ? "None"
                : altar.tempCurrentSpell.description;
        }

        //private static string DeityDescription(Building_SacrificialAltar altar)
        //{
        //    if (altar.tempCurrentSacrificeDeity == null)
        //    {
        //        return "None";
        //    }
        //    else
        //    {
        //        StringBuilder stringBuilder = new StringBuilder();
        //        //stringBuilder.Append(altar.tempCurrentSacrificeDeity.LabelCap);
        //        stringBuilder.AppendLine();
        //        stringBuilder.Append(altar.tempCurrentSacrificeDeity.def.description);
        //        //stringBuilder.AppendLine();
        //        //stringBuilder.Append("Standing: " + altar.tempCurrentSacrificeDeity.PlayerFavor.ToString());
        //        //stringBuilder.AppendLine();
        //        //stringBuilder.Append("Tier: " + altar.tempCurrentSacrificeDeity.PlayerTier.ToString());
        //        return stringBuilder.ToString();
        //    }
        //}

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

        //public static void OpenSacrificeSelectMenu(Building_SacrificialAltar altar)
        //{
        //    List<FloatMenuOption> list = new List<FloatMenuOption>();
        //    list.Add(new FloatMenuOption("(" + "NoneLower".Translate() + ")", delegate
        //    {
        //        altar.tempSacrifice = null;
        //    }, MenuOptionPriority.Default, null, null, 0f, null));

        //    foreach (Pawn current in altar.Map.mapPawns.PrisonersOfColonySpawned)
        //    {
        //        if (!current.Dead && !current.InAggroMentalState)
        //        {
        //            Action action;
        //            Pawn localCol = current;
        //            action = delegate
        //            {
        //                altar.Map.GetComponent<MapComponent_SacrificeTracker>().lastUsedAltar = altar;
        //                altar.Map.GetComponent<MapComponent_SacrificeTracker>().lastSacrificeType = CultUtility.SacrificeType.human;
        //                altar.tempSacrifice = localCol;
        //            };
        //            list.Add(new FloatMenuOption(localCol.LabelShort, action, MenuOptionPriority.Default, null, null, 0f, null));
        //        }
        //    }
        //    Find.WindowStack.Add(new FloatMenu(list));
        //}

        //public static void OpenExecutionerSelectMenu(Building_SacrificialAltar altar)
        //{
        //    List<FloatMenuOption> list = new List<FloatMenuOption>();
        //    list.Add(new FloatMenuOption("(" + "NoneLower".Translate() + ")", delegate
        //    {
        //        altar.tempExecutioner = null;
        //    }, MenuOptionPriority.Default, null, null, 0f, null));

        //    if (altar != null)
        //    {
        //        list.Add(new FloatMenuOption("(" + "NoneLower".Translate() + ")", delegate
        //        {
        //            altar.tempExecutioner = null;
        //        }, MenuOptionPriority.Default, null, null, 0f, null));

        //        foreach (Pawn current in altar.Map.mapPawns.FreeColonists)
        //        {
        //            if (CultUtility.IsCultMinded(current))
        //            {
        //                if (current == null) continue;
        //                if (current.Dead) continue;
        //                if (current.IsPrisoner) continue;
        //                if (current.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) &&
        //                    current.health.capacities.CapableOf(PawnCapacityDefOf.Moving))
        //                {
        //                    Action action;
        //                    Pawn localCol = current;
        //                    action = delegate
        //                    {
        //                        altar.Map.GetComponent<MapComponent_SacrificeTracker>().lastUsedAltar = altar;
        //                        altar.tempExecutioner = localCol;
        //                    };
        //                    list.Add(new FloatMenuOption(localCol.LabelShort, action, MenuOptionPriority.Default, null, null, 0f, null));
        //                }
        //            }
        //        }
        //    }
        //    Find.WindowStack.Add(new FloatMenu(list));
        //}


        //public static void OpenDeitySelectMenu(Building_SacrificialAltar altar)
        //{
        //    List<FloatMenuOption> list = new List<FloatMenuOption>();
        //    list.Add(new FloatMenuOption("(" + "NoneLower".Translate() + ")", delegate
        //    {
        //        altar.Map.GetComponent<MapComponent_SacrificeTracker>().lastUsedAltar = altar;
        //        altar.tempCurrentSacrificeDeity = null;
        //    }, MenuOptionPriority.Default, null, null, 0f, null));

        //    foreach (CosmicEntity current in DeityTracker.Get.DeityCache.Keys)
        //    {
        //        if (current.discovered == false) continue;
        //        Action action;
        //        CosmicEntity localDeity = current;
        //        action = delegate
        //        {

        //            altar.Map.GetComponent<MapComponent_SacrificeTracker>().lastUsedAltar = altar;
        //            altar.tempCurrentSacrificeDeity = localDeity;
        //            altar.tempCurrentSpell = null;
        //        };
        //        list.Add(new FloatMenuOption(localDeity.LabelCap, action, MenuOptionPriority.Default, null, null, 0f, null));
        //    }
        //    Find.WindowStack.Add(new FloatMenu(list));
        //}

        public static void OpenSpellSelectMenu(Building_SacrificialAltar altar)
        {
            var list = new List<FloatMenuOption>
            {
                new FloatMenuOption("(" + "NoneLower".Translate() + ")", delegate { altar.tempCurrentSpell = null; })
            };

            if (altar.tempCurrentSacrificeDeity != null)
            {
                if (altar.tempCurrentSacrificeDeity.tier1Spells != null &&
                    altar.tempCurrentSacrificeDeity.PlayerTier > 0)
                {
                    foreach (var spell in altar.tempCurrentSacrificeDeity.tier1Spells)
                    {
                        var localSpell = spell;

                        void Action()
                        {
                            altar.Map.GetComponent<MapComponent_SacrificeTracker>().lastUsedAltar = altar;
                            altar.tempCurrentSpell = localSpell;
                        }

                        list.Add(new FloatMenuOption(localSpell.LabelCap, Action));
                    }
                }

                if (altar.tempCurrentSacrificeDeity.tier2Spells != null &&
                    altar.tempCurrentSacrificeDeity.PlayerTier > CosmicEntity.Tier.One)
                {
                    foreach (var spell in altar.tempCurrentSacrificeDeity.tier2Spells)
                    {
                        var localSpell = spell;

                        void Action2()
                        {
                            altar.Map.GetComponent<MapComponent_SacrificeTracker>().lastUsedAltar = altar;
                            altar.tempCurrentSpell = localSpell;
                        }

                        list.Add(new FloatMenuOption(localSpell.LabelCap, Action2));
                    }
                }

                if (altar.tempCurrentSacrificeDeity.tier3Spells != null &&
                    altar.tempCurrentSacrificeDeity.PlayerTier > CosmicEntity.Tier.Two)
                {
                    foreach (var spell in altar.tempCurrentSacrificeDeity.tier3Spells)
                    {
                        var localSpell = spell;

                        void Action3()
                        {
                            altar.Map.GetComponent<MapComponent_SacrificeTracker>().lastUsedAltar = altar;
                            altar.tempCurrentSpell = localSpell;
                        }

                        list.Add(new FloatMenuOption(localSpell.LabelCap, Action3));
                    }
                }

                if (altar.tempCurrentSacrificeDeity.finalSpell != null &&
                    altar.tempCurrentSacrificeDeity.PlayerTier > CosmicEntity.Tier.Three)
                {
                    var localSpell = altar.tempCurrentSacrificeDeity.finalSpell;

                    void Action4()
                    {
                        altar.Map.GetComponent<MapComponent_SacrificeTracker>().lastUsedAltar = altar;
                        altar.tempCurrentSpell = localSpell;
                    }

                    list.Add(new FloatMenuOption(localSpell.LabelCap, Action4));
                }
            }

            Find.WindowStack.Add(new FloatMenu(list));
        }
    }
}