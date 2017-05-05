using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace CultOfCthulhu
{
    class ITab_AltarCardUtility
    {
        public static void DrawRename(Building_SacrificialAltar altar)
        {
            Rect rectRename = new Rect(ITab_AltarWorshipCardUtility.TempleCardSize.x - 85f, 0f, 30f, 30f);
            TooltipHandler.TipRegion(rectRename, "RenameTemple".Translate());
            if (Widgets.ButtonImage(rectRename, Buttons.RenameTex))
            {
                Find.WindowStack.Add(new Dialog_RenameTemple(altar));
            }
        }

        public static void DrawDeity(CosmicEntity entity, Rect rect3, string spellDescription=null, float offset=0f)
        {
            string entityLabel = "";
            string entityDescrip = "";
            if (entity != null)
            {
                entityLabel = entity.LabelCap;
                entityDescrip = entity.def.description;
            }

            Rect secondBox = rect3;
            secondBox.x += rect3.x + 10f + 30f + offset;
            secondBox.xMax += 125f;
            secondBox.height = ITab_AltarSacrificesCardUtility.ButtonSize;
            Text.Font = GameFont.Medium;
            Widgets.Label(secondBox, entityLabel);
            Text.Font = GameFont.Small;
            Rect secondBoxUnder = secondBox;
            secondBoxUnder.y += ITab_AltarSacrificesCardUtility.ButtonSize + ITab_AltarSacrificesCardUtility.SpacingOffset;
            secondBoxUnder.width -= 15f;
            secondBoxUnder.height = ITab_AltarSacrificesCardUtility.ButtonSize;
            Widgets.Label(secondBoxUnder, entityDescrip);
            ITab_AltarCardUtility.DrawTier(entity, new Vector2(secondBoxUnder.x, secondBoxUnder.y + 70f));
            Rect secondBoxUnder2 = secondBoxUnder;
            secondBoxUnder2.y += ITab_AltarSacrificesCardUtility.ButtonSize * 2 + (ITab_AltarSacrificesCardUtility.SpacingOffset * 2);
            secondBoxUnder2.height = 250f;
            if (spellDescription != null)
                Widgets.Label(secondBoxUnder2, spellDescription);

        }

        // RimWorld.SkillUI//
        public static void DrawTier(CosmicEntity entity, Vector2 topLeft)
        {
            if (entity == null) return;
            string tierLabel = entity.PlayerTier.ToString();
            string standingLabel = "Standing".Translate() + ":";
            float tierLabelWidth = Text.CalcSize(standingLabel).x;
            float tierCurrentVal = entity.PlayerFavor;
            float tierCurrentMax = entity.currentTierMax;
            float tierPrevMax = entity.prevTierMax;
            Rect rect = new Rect(topLeft.x, topLeft.y, 150f, 24f);
            if (Mouse.IsOver(rect))
            {
                GUI.DrawTexture(rect, TexUI.HighlightTex);
            }
            GUI.BeginGroup(rect);
            Text.Anchor = TextAnchor.MiddleLeft;
            Rect rect2 = new Rect(0f, 0f, tierLabelWidth + 5f, rect.height);
            Widgets.Label(rect2, standingLabel);
            Rect position = new Rect(rect2.xMax, 0f, 10f, 24f);
            Rect rect3 = new Rect(position.xMax, 0f, rect.width - position.xMax, rect.height);
            Widgets.FillableBar(rect3, (tierCurrentVal - tierPrevMax) / (tierCurrentMax - tierPrevMax), Buttons.TierBarFillTex, null, false);
            Rect rect4 = new Rect(position.xMax + 4f, 0f, 999f, rect.height);
            //rect4.yMin += 10f;
            rect4.yMax += 18f;
            string label = entity.TierString;
            GenUI.SetLabelAlign(TextAnchor.MiddleLeft);
            Widgets.Label(rect4, label);
            GenUI.ResetLabelAlign();
            GUI.color = Color.white;
            GUI.EndGroup();
            TooltipHandler.TipRegion(rect, new TipSignal(GetFavorDescription(entity), entity.def.GetHashCode() * 397945));
        }

        // RimWorld.SkillUI
        private static string GetFavorDescription(CosmicEntity entity)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (entity == null)
            {
                stringBuilder.Append("DisabledLower".Translate().CapitalizeFirst());
            }
            else
            {
                stringBuilder.AppendLine(string.Concat(new object[]
                {
            "Tier".Translate(),
            " ",
            entity.PlayerTier,
            ": ",
            entity.TierString
                }));
                if (Current.ProgramState == ProgramState.Playing)
                {
                    string text = (entity.PlayerTier != CosmicEntity.Tier.Final) ? "ProgressToNextLevel".Translate() : "Favor".Translate();
                    stringBuilder.AppendLine(string.Concat(new object[]
                    {
                text,
                ": ",
                entity.PlayerFavor.ToString("F"),
                " / ",
                entity.currentTierMax.ToString("F")
                    }));
                }
            }
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            stringBuilder.Append("FavorDescription".Translate());
            return stringBuilder.ToString();
        }


        ////////General Stuff

        public static string SacrificeLabel(Building_SacrificialAltar altar)
        {
            if (altar.tempSacrifice == null)return "None";
            return altar.tempSacrifice.NameStringShort;
        }

        public static string ExecutionerLabel(Building_SacrificialAltar altar)
        {
            if (altar.tempExecutioner == null) return "None";
            return altar.tempExecutioner.NameStringShort;
        }

        public static string DeityLabel(Building_SacrificialAltar altar, DeityType deityType)
        {
            switch (deityType)
            {
                case DeityType.OfferingDeity:
                    if (altar.tempCurrentOfferingDeity == null) return "None";
                    return altar.tempCurrentOfferingDeity.LabelCap;
                case DeityType.WorshipDeity:
                    if (altar.tempCurrentWorshipDeity == null) return "None";
                    return altar.tempCurrentWorshipDeity.LabelCap;
                case DeityType.SacrificeDeity:
                    if (altar.tempCurrentSacrificeDeity == null) return "None";
                    return altar.tempCurrentSacrificeDeity.LabelCap;
            }
            return "None";
        }

        public static string SpellLabel(Building_SacrificialAltar altar)
        {
            if (altar.tempCurrentSacrificeDeity == null || altar.tempCurrentSpell == null) return "None";
            return altar.tempCurrentSpell.LabelCap;
        }

        public static string SpellDescription(Building_SacrificialAltar altar)
        {
            if (altar.tempCurrentSacrificeDeity == null || altar.tempCurrentSpell == null) return "None";
            return altar.tempCurrentSpell.description;
        }

        public static string DeityDescription(Building_SacrificialAltar altar)
        {
            if (altar.tempCurrentSacrificeDeity == null) return "None";
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine();
            stringBuilder.Append(altar.tempCurrentSacrificeDeity.def.description);
            return stringBuilder.ToString();
        }

        public static void OpenSacrificeSelectMenu(Building_SacrificialAltar altar)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            list.Add(new FloatMenuOption("(" + "NoneLower".Translate() + ")", delegate
            {
                altar.tempSacrifice = null;
            }, MenuOptionPriority.Default, null, null, 0f, null));

            foreach (Pawn current in altar.Map.mapPawns.AllPawnsSpawned)
            {
                if (current.RaceProps.Animal && current.Faction == Faction.OfPlayer)
                {
                    Action action;
                    Pawn localCol = current;
                    action = delegate
                    {
                        altar.Map.GetComponent<MapComponent_SacrificeTracker>().lastUsedAltar = altar;
                        altar.Map.GetComponent<MapComponent_SacrificeTracker>().lastSacrificeType = CultUtility.SacrificeType.animal;
                        altar.tempSacrifice = localCol;
                    };
                    list.Add(new FloatMenuOption(localCol.LabelShort, action, MenuOptionPriority.Default, null, null, 0f, null));
                }
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }
        /// <summary>
        /// This will make things easier for processing multiple possibile "actors" at the altar.
        /// </summary>
        public enum ActorType
        {
            executioner = 0,
            preacher = 1,
            offerer = 2,
            attendee = 3,
            prisoner = 4,
            animalSacrifice = 5
        }
        public static void OpenActorSelectMenu(Building_SacrificialAltar altar, ActorType actorType)
        {
            ///
            /// Error Handling
            ///
            if (altar == null)
            {
                Cthulhu.Utility.ErrorReport("Altar Null Exception");
                return;
            }
            if (altar.Map == null)
            {
                Cthulhu.Utility.ErrorReport("Map Null Exception");
            }
            if (altar.Map.mapPawns == null)
            {
                Cthulhu.Utility.ErrorReport("mapPawns Null Exception");
                return;
            }
            if (altar.Map.mapPawns.FreeColonistsSpawned == null)
            {
                Cthulhu.Utility.ErrorReport("FreeColonistsSpawned Null Exception");
                return;
            }
            if (altar.Map.mapPawns.FreeColonistsSpawnedCount <= 0)
            {
                Cthulhu.Utility.ErrorReport("Colonist Count Less Than or Equal To 0 Exception");
                return;
            }

            ///
            /// Get Actor List
            /// 

            List<Pawn> actorList = new List<Pawn>();
            StringBuilder s = new StringBuilder();
            switch (actorType)
            {
                case ActorType.executioner:
                case ActorType.offerer:
                    // Cycle through candidates
                    foreach (Pawn candidate in altar.Map.mapPawns.FreeColonistsSpawned)
                    {
                        if (!CultUtility.IsCultistAvailable(candidate)) continue;

                        // Executioners must be able to use tool and move.
                        if (candidate.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) &&
                            candidate.health.capacities.CapableOf(PawnCapacityDefOf.Moving))
                        {
                            // Add the actors.
                            actorList.Add(candidate);
                            Cthulhu.Utility.DebugReport("Actor List :: Added " + candidate.Name);
                        }
                    }
                    Cthulhu.Utility.DebugReport(s.ToString());
                    break;
                case ActorType.preacher:
                    // Cycle through candidates
                    foreach (Pawn candidate in altar.Map.mapPawns.FreeColonistsSpawned)
                    {
                        if (!CultUtility.IsCultistAvailable(candidate)) continue;

                        // Preachers must be able to move and talk.
                        if (candidate.health.capacities.CapableOf(PawnCapacityDefOf.Moving) &&
                            candidate.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
                        {
                            // Add the actors.
                            actorList.Add(candidate);
                            Cthulhu.Utility.DebugReport("Actor List :: Added " + candidate.Name);
                        }
                    }
                    Cthulhu.Utility.DebugReport(s.ToString());
                    break;
                case ActorType.prisoner:
                    ///
                    /// ERROR HANDLING
                    /// 

                    if (altar.Map.mapPawns.PrisonersOfColonySpawned == null)
                    {
                        Messages.Message("No prisoners available.", MessageSound.RejectInput);
                        return;
                    }
                    if (altar.Map.mapPawns.PrisonersOfColonySpawnedCount <= 0)
                    {
                        Messages.Message("No prisoners available.", MessageSound.RejectInput);
                        return;
                    }


                    /// Cycle through possible candidates in the map's prisoner list
                    foreach (Pawn candidate in altar.Map.mapPawns.PrisonersOfColonySpawned)
                    {
                        if (!Cthulhu.Utility.IsActorAvailable(candidate, true)) continue;
                        actorList.Add(candidate);
                    }
                    break;
                case ActorType.animalSacrifice:
                    ///
                    /// ERROR HANDLING
                    /// 

                    if (altar.Map.mapPawns.AllPawnsSpawned == null)
                    {
                        Messages.Message("No " + actorType.ToString() + "s available.", MessageSound.RejectInput);
                        return;
                    }
                    if (altar.Map.mapPawns.AllPawnsSpawnedCount <= 0)
                    {
                        Messages.Message("No " + actorType.ToString() + "s available.", MessageSound.RejectInput);
                        return;
                    }

                    /// Cycle through possible candidates in the player's owned animals list.
                    foreach (Pawn candidate in altar.Map.mapPawns.AllPawnsSpawned)
                    {
                        if (!Cthulhu.Utility.IsActorAvailable(candidate, true)) continue;
                        if (candidate.Faction != Faction.OfPlayer) continue;
                        if (candidate.RaceProps == null) continue;
                        if (!candidate.RaceProps.Animal) continue;
                        actorList.Add(candidate);
                    }
                    break;
            }

            /// Let the player know there are no prisoners available.
            if (actorList != null)
            {
                if (actorList.Count <= 0)
                {
                    Messages.Message("No " + actorType.ToString() + "s available.", MessageSound.RejectInput);
                    return;
                }
            }

            ///
            /// Create float menus
            ///

            //There must always be a none.
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            list.Add(new FloatMenuOption("(" + "NoneLower".Translate() + ")", delegate
            {
                altar.tempExecutioner = null;
            }, MenuOptionPriority.Default, null, null, 0f, null));

            foreach (Pawn actor in actorList)
            {
                Action action;
                Pawn localCol = actor;
                action = delegate
                {
                    switch (actorType)
                    {
                        case ActorType.executioner:
                            MapComponent_SacrificeTracker.Get(altar.Map).lastUsedAltar = altar;
                            altar.tempExecutioner = localCol;
                            break;
                        case ActorType.preacher:
                            altar.tempPreacher = localCol;
                            break;
                        case ActorType.offerer:
                            MapComponent_SacrificeTracker.Get(altar.Map).lastUsedAltar = altar;
                            altar.tempOfferer = localCol;
                            break;
                        case ActorType.prisoner:
                            MapComponent_SacrificeTracker.Get(altar.Map).lastUsedAltar = altar;
                            MapComponent_SacrificeTracker.Get(altar.Map).lastSacrificeType = CultUtility.SacrificeType.human;
                            altar.tempSacrifice = localCol;
                            break;
                        case ActorType.animalSacrifice:
                            MapComponent_SacrificeTracker.Get(altar.Map).lastUsedAltar = altar;
                            MapComponent_SacrificeTracker.Get(altar.Map).lastSacrificeType = CultUtility.SacrificeType.animal;
                            altar.tempSacrifice = localCol;
                            break;
                    }
                };
                list.Add(new FloatMenuOption(localCol.LabelShort, action, MenuOptionPriority.Default, null, null, 0f, null));
            }

            Find.WindowStack.Add(new FloatMenu(list));
        }

        public enum DeityType
        {
            WorshipDeity = 0,
            OfferingDeity = 1,
            SacrificeDeity = 2
        }
        public static void OpenDeitySelectMenu(Building_SacrificialAltar altar, DeityType deityType)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            list.Add(new FloatMenuOption("(" + "NoneLower".Translate() + ")", delegate
            {
                altar.Map.GetComponent<MapComponent_SacrificeTracker>().lastUsedAltar = altar;
                altar.tempCurrentSacrificeDeity = null;
            }, MenuOptionPriority.Default, null, null, 0f, null));

            foreach (CosmicEntity current in Cthulhu.UtilityWorldObjectManager.GetUtilityWorldObject<UtilityWorldObject_CosmicDeities>().DeityCache.Keys)
            {
                if (current.discovered == false) continue;
                Action action;
                CosmicEntity localDeity = current;
                action = delegate
                {

                    MapComponent_SacrificeTracker.Get(altar.Map).lastUsedAltar = altar;
                    switch (deityType)
                    {
                        case DeityType.WorshipDeity:
                            altar.tempCurrentWorshipDeity = localDeity;
                            break;
                        case DeityType.OfferingDeity:
                            altar.tempCurrentOfferingDeity = localDeity;
                            break;
                        case DeityType.SacrificeDeity:
                            altar.tempCurrentSacrificeDeity = localDeity;
                            altar.tempCurrentSpell = null;
                            break;
                    }
                };
                list.Add(new FloatMenuOption(localDeity.LabelCap, action, MenuOptionPriority.Default, null, null, 0f, null));
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }

    }
}
