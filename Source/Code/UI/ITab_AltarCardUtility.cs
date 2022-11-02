using System;
using System.Collections.Generic;
using System.Text;
using CallOfCthulhu;
using Cthulhu;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CultOfCthulhu
{
    internal class ITab_AltarCardUtility
    {
        /// <summary>
        ///     This will make things easier for processing multiple possibile "actors" at the altar.
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

        public enum DeityType
        {
            WorshipDeity = 0,
            OfferingDeity = 1,
            SacrificeDeity = 2
        }

        public static void DrawRename(Building_SacrificialAltar altar)
        {
            var rectRename = new Rect(x: ITab_AltarWorshipCardUtility.TempleCardSize.x - 85f, y: 0f, width: 30f, height: 30f);
            TooltipHandler.TipRegion(rect: rectRename, tip: "RenameTemple".Translate());
            if (Widgets.ButtonImage(butRect: rectRename, tex: Buttons.RenameTex))
            {
                Find.WindowStack.Add(window: new Dialog_RenameTemple(altar: altar));
            }
        }

        public static void DrawDeity(CosmicEntity entity, Rect rect3, string spellDescription = null, float offset = 0f)
        {
            var entityLabel = "";
            var entityDescrip = "";
            if (entity != null)
            {
                entityLabel = entity.LabelCap;
                entityDescrip = entity.def.description;
            }

            var secondBox = rect3;
            secondBox.x += rect3.x + 10f + 30f + offset;
            secondBox.xMax += 125f;
            secondBox.height = ITab_AltarSacrificesCardUtility.ButtonSize;
            Text.Font = GameFont.Medium;
            Widgets.Label(rect: secondBox, label: entityLabel);
            Text.Font = GameFont.Small;
            var secondBoxUnder = secondBox;
            secondBoxUnder.y += ITab_AltarSacrificesCardUtility.ButtonSize +
                                ITab_AltarSacrificesCardUtility.SpacingOffset;
            secondBoxUnder.width -= 15f;
            secondBoxUnder.height = ITab_AltarSacrificesCardUtility.ButtonSize;
            Widgets.Label(rect: secondBoxUnder, label: entityDescrip);
            DrawTier(entity: entity, topLeft: new Vector2(x: secondBoxUnder.x, y: secondBoxUnder.y + 70f));
            var secondBoxUnder2 = secondBoxUnder;
            secondBoxUnder2.y += (ITab_AltarSacrificesCardUtility.ButtonSize * 2) +
                                 (ITab_AltarSacrificesCardUtility.SpacingOffset * 2);
            secondBoxUnder2.height = 250f;
            if (spellDescription != null)
            {
                Widgets.Label(rect: secondBoxUnder2, label: spellDescription);
            }
        }

        // RimWorld.SkillUI//
        public static void DrawTier(CosmicEntity entity, Vector2 topLeft)
        {
            if (entity == null)
            {
                return;
            }

            _ = entity.PlayerTier.ToString();
            string standingLabel = "Standing".Translate() + ":";
            var tierLabelWidth = Text.CalcSize(text: standingLabel).x;
            var tierCurrentVal = entity.PlayerFavor;
            var tierCurrentMax = entity.currentTierMax;
            var tierPrevMax = entity.prevTierMax;
            var rect = new Rect(x: topLeft.x, y: topLeft.y, width: 150f, height: 24f);
            if (Mouse.IsOver(rect: rect))
            {
                GUI.DrawTexture(position: rect, image: TexUI.HighlightTex);
            }

            GUI.BeginGroup(position: rect);
            Text.Anchor = TextAnchor.MiddleLeft;
            var rect2 = new Rect(x: 0f, y: 0f, width: tierLabelWidth + 5f, height: rect.height);
            Widgets.Label(rect: rect2, label: standingLabel);
            var position = new Rect(x: rect2.xMax, y: 0f, width: 10f, height: 24f);
            var rect3 = new Rect(x: position.xMax, y: 0f, width: rect.width - position.xMax, height: rect.height);
            Widgets.FillableBar(rect: rect3, fillPercent: (tierCurrentVal - tierPrevMax) / (tierCurrentMax - tierPrevMax),
                fillTex: Buttons.TierBarFillTex, bgTex: null, doBorder: false);
            var rect4 = new Rect(x: position.xMax + 4f, y: 0f, width: 999f, height: rect.height);
            //rect4.yMin += 10f;
            rect4.yMax += 18f;
            var label = entity.TierString;
            GenUI.SetLabelAlign(a: TextAnchor.MiddleLeft);
            Widgets.Label(rect: rect4, label: label);
            GenUI.ResetLabelAlign();
            GUI.color = Color.white;
            GUI.EndGroup();
            TooltipHandler.TipRegion(rect: rect,
                tip: new TipSignal(text: GetFavorDescription(entity: entity), uniqueId: entity.def.GetHashCode() * 397945));
        }

        // RimWorld.SkillUI
        private static string GetFavorDescription(CosmicEntity entity)
        {
            var stringBuilder = new StringBuilder();
            if (entity == null)
            {
                stringBuilder.Append(value: "DisabledLower".Translate().CapitalizeFirst());
            }
            else
            {
                stringBuilder.AppendLine(value: string.Concat("Tier".Translate(), " ", entity.PlayerTier, ": ",
                    entity.TierString));
                if (Current.ProgramState == ProgramState.Playing)
                {
                    string text = entity.PlayerTier != CosmicEntity.Tier.Final
                        ? "ProgressToNextLevel".Translate()
                        : "Favor".Translate();
                    stringBuilder.AppendLine(value: string.Concat(args: new object[]
                    {
                        text,
                        ": ",
                        entity.PlayerFavor.ToString(format: "F"),
                        " / ",
                        entity.currentTierMax.ToString(format: "F")
                    }));
                }
            }

            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            stringBuilder.Append(value: "FavorDescription".Translate());
            return stringBuilder.ToString();
        }


        ////////General Stuff

        public static string SacrificeLabel(Building_SacrificialAltar altar)
        {
            return altar.tempSacrifice == null ? "None" : altar.tempSacrifice.Name.ToStringShort;
        }

        public static string ExecutionerLabel(Building_SacrificialAltar altar)
        {
            return altar.tempExecutioner == null ? "None" : altar.tempExecutioner.Name.ToStringShort;
        }

        public static string DeityLabel(Building_SacrificialAltar altar, DeityType deityType)
        {
            switch (deityType)
            {
                case DeityType.OfferingDeity:
                    if (altar.tempCurrentOfferingDeity == null)
                    {
                        return "None";
                    }

                    return altar.tempCurrentOfferingDeity.LabelCap;
                case DeityType.WorshipDeity:
                    if (altar.tempCurrentWorshipDeity == null)
                    {
                        return "None";
                    }

                    return altar.tempCurrentWorshipDeity.LabelCap;
                case DeityType.SacrificeDeity:
                    if (altar.tempCurrentSacrificeDeity == null)
                    {
                        return "None";
                    }

                    return altar.tempCurrentSacrificeDeity.LabelCap;
            }

            return "None";
        }

        public static string SpellLabel(Building_SacrificialAltar altar)
        {
            return altar.tempCurrentSacrificeDeity == null || altar.tempCurrentSpell == null
                ? "None"
                : (string) altar.tempCurrentSpell.LabelCap;
        }

        public static string SpellDescription(Building_SacrificialAltar altar)
        {
            return altar.tempCurrentSacrificeDeity == null || altar.tempCurrentSpell == null
                ? "None"
                : altar.tempCurrentSpell.description;
        }

        public static string DeityDescription(Building_SacrificialAltar altar)
        {
            if (altar.tempCurrentSacrificeDeity == null)
            {
                return "None";
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine();
            stringBuilder.Append(value: altar.tempCurrentSacrificeDeity.def.description);
            return stringBuilder.ToString();
        }

        public static void OpenSacrificeSelectMenu(Building_SacrificialAltar altar)
        {
            var list = new List<FloatMenuOption>
            {
                new FloatMenuOption(label: "(" + "NoneLower".Translate() + ")", action: delegate { altar.tempSacrifice = null; })
            };

            foreach (var current in altar.Map.mapPawns.AllPawnsSpawned)
            {
                if (!current.RaceProps.Animal || current.Faction != Faction.OfPlayer)
                {
                    continue;
                }

                var localCol = current;

                void Action()
                {
                    altar.Map.GetComponent<MapComponent_SacrificeTracker>().lastUsedAltar = altar;
                    altar.Map.GetComponent<MapComponent_SacrificeTracker>().lastSacrificeType =
                        CultUtility.SacrificeType.animal;
                    altar.tempSacrifice = localCol;
                }

                list.Add(item: new FloatMenuOption(label: localCol.LabelShort, action: Action));
            }

            Find.WindowStack.Add(window: new FloatMenu(options: list));
        }

        public static void OpenActorSelectMenu(Building_SacrificialAltar altar, ActorType actorType)
        {
            if (altar == null)
            {
                Utility.ErrorReport(x: "Altar Null Exception");
                return;
            }

            if (altar.Map == null)
            {
                Utility.ErrorReport(x: "Map Null Exception");
            }

            if (altar.Map?.mapPawns == null)
            {
                Utility.ErrorReport(x: "mapPawns Null Exception");
                return;
            }

            if (altar.Map.mapPawns.FreeColonistsSpawned == null)
            {
                Utility.ErrorReport(x: "FreeColonistsSpawned Null Exception");
                return;
            }
            //if (altar.Map.mapPawns.FreeColonistsSpawnedCount <= 0)
            //{
            //    Cthulhu.Utility.ErrorReport("Colonist Count Less Than or Equal To 0 Exception");
            //    return;
            //}

            var actorList = new List<Pawn>();
            var s = new StringBuilder();
            switch (actorType)
            {
                case ActorType.executioner:
                case ActorType.offerer:
                    // Cycle through candidates
                    foreach (var candidate in altar.Map.mapPawns.FreeColonistsSpawned)
                    {
                        if (!CultUtility.IsCultistAvailable(pawn: candidate))
                        {
                            continue;
                        }

                        // Executioners must be able to use tool and move.
                        if (!candidate.health.capacities.CapableOf(capacity: PawnCapacityDefOf.Manipulation) ||
                            !candidate.health.capacities.CapableOf(capacity: PawnCapacityDefOf.Moving))
                        {
                            continue;
                        }

                        // Add the actors.
                        actorList.Add(item: candidate);
                        Utility.DebugReport(x: "Actor List :: Added " + candidate.Name);
                    }

                    Utility.DebugReport(x: s.ToString());
                    break;
                case ActorType.preacher:
                    // Cycle through candidates
                    foreach (var candidate in altar.Map.mapPawns.FreeColonistsSpawned)
                    {
                        if (!CultUtility.IsCultistAvailable(pawn: candidate))
                        {
                            continue;
                        }

                        // Preachers must be able to move and talk.
                        if (!candidate.health.capacities.CapableOf(capacity: PawnCapacityDefOf.Moving) ||
                            !candidate.health.capacities.CapableOf(capacity: PawnCapacityDefOf.Talking))
                        {
                            continue;
                        }

                        // Add the actors.
                        actorList.Add(item: candidate);
                        Utility.DebugReport(x: "Actor List :: Added " + candidate.Name);
                    }

                    Utility.DebugReport(x: s.ToString());
                    break;
                case ActorType.prisoner:

                    if (altar.Map.mapPawns.PrisonersOfColonySpawned == null)
                    {
                        Messages.Message(text: "No prisoners available.", def: MessageTypeDefOf.RejectInput);
                        return;
                    }

                    if (altar.Map.mapPawns.PrisonersOfColonySpawnedCount <= 0)
                    {
                        Messages.Message(text: "No prisoners available.", def: MessageTypeDefOf.RejectInput);
                        return;
                    }


                    // Cycle through possible candidates in the map's prisoner list
                    foreach (var candidate in altar.Map.mapPawns.PrisonersOfColonySpawned)
                    {
                        if (!Utility.IsActorAvailable(preacher: candidate, downedAllowed: true))
                        {
                            continue;
                        }

                        actorList.Add(item: candidate);
                    }

                    break;
                case ActorType.animalSacrifice:

                    if (altar.Map.mapPawns.AllPawnsSpawned == null)
                    {
                        Messages.Message(text: "No " + actorType + "s available.", def: MessageTypeDefOf.RejectInput);
                        return;
                    }

                    if (altar.Map.mapPawns.AllPawnsSpawnedCount <= 0)
                    {
                        Messages.Message(text: "No " + actorType + "s available.", def: MessageTypeDefOf.RejectInput);
                        return;
                    }

                    // Cycle through possible candidates in the player's owned animals list.
                    foreach (var candidate in altar.Map.mapPawns.AllPawnsSpawned)
                    {
                        if (!Utility.IsActorAvailable(preacher: candidate, downedAllowed: true))
                        {
                            continue;
                        }

                        if (candidate.Faction != Faction.OfPlayer)
                        {
                            continue;
                        }

                        if (candidate.RaceProps == null)
                        {
                            continue;
                        }

                        if (!candidate.RaceProps.Animal)
                        {
                            continue;
                        }

                        actorList.Add(item: candidate);
                    }

                    break;
            }

            // Let the player know there are no prisoners available.
            if (actorList.Count <= 0)
            {
                Messages.Message(text: "No " + actorType + "s available.", def: MessageTypeDefOf.RejectInput);
                return;
            }

            //There must always be a none.
            var list = new List<FloatMenuOption>
            {
                new FloatMenuOption(label: "(" + "NoneLower".Translate() + ")", action: delegate { altar.tempExecutioner = null; })
            };

            foreach (var actor in actorList)
            {
                var localCol = actor;

                void Action()
                {
                    switch (actorType)
                    {
                        case ActorType.executioner:
                            MapComponent_SacrificeTracker.Get(map: altar.Map).lastUsedAltar = altar;
                            altar.tempExecutioner = localCol;
                            break;
                        case ActorType.preacher:
                            altar.tempPreacher = localCol;
                            break;
                        case ActorType.offerer:
                            MapComponent_SacrificeTracker.Get(map: altar.Map).lastUsedAltar = altar;
                            altar.tempOfferer = localCol;
                            break;
                        case ActorType.prisoner:
                            MapComponent_SacrificeTracker.Get(map: altar.Map).lastUsedAltar = altar;
                            MapComponent_SacrificeTracker.Get(map: altar.Map).lastSacrificeType =
                                CultUtility.SacrificeType.human;
                            altar.tempSacrifice = localCol;
                            break;
                        case ActorType.animalSacrifice:
                            MapComponent_SacrificeTracker.Get(map: altar.Map).lastUsedAltar = altar;
                            MapComponent_SacrificeTracker.Get(map: altar.Map).lastSacrificeType =
                                CultUtility.SacrificeType.animal;
                            altar.tempSacrifice = localCol;
                            break;
                    }
                }

                list.Add(item: new FloatMenuOption(label: localCol.LabelShort, action: Action));
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

        public static void OpenDeitySelectMenu(Building_SacrificialAltar altar, DeityType deityType)
        {
            var list = new List<FloatMenuOption>
            {
                new FloatMenuOption(label: "(" + "NoneLower".Translate() + ")", action: delegate
                {
                    altar.Map.GetComponent<MapComponent_SacrificeTracker>().lastUsedAltar = altar;
                    altar.tempCurrentSacrificeDeity = null;
                })
            };

            foreach (var current in DeityTracker.Get.DeityCache.Keys)
            {
                if (current.discovered == false)
                {
                    continue;
                }

                var localDeity = current;

                void Action()
                {
                    MapComponent_SacrificeTracker.Get(map: altar.Map).lastUsedAltar = altar;
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
    }
}