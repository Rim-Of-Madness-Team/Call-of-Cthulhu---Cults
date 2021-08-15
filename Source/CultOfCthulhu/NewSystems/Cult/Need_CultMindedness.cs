using System.Collections.Generic;
using Cthulhu;
using RimWorld;
using UnityEngine;
using Verse;

namespace CultOfCthulhu
{
    public class Need_CultMindedness : Need
    {
        //public static ThingDef ColanderThingDef;

        public const float BaseGainPerTickRate = 150f;
        public const float BaseFallPerTick = 1E-05f;
        public const float ThreshVeryLow = CultLevel.PureAntiCultist;
        public const float ThreshLow = CultLevel.AntiCultist;
        public const float ThreshSatisfied = CultLevel.Middling;
        public const float ThreshHigh = CultLevel.Cultist;
        public const float ThreshVeryHigh = CultLevel.PureCultist;

        private readonly WorldComponent_GlobalCultTracker globalCultTracker =
            Find.World.GetComponent<WorldComponent_GlobalCultTracker>();

        private bool baseSet;
        private int lastGainTick;
        public int ticksUntilBaseSet = 500;

        public Need_CultMindedness(Pawn pawn) : base(pawn)
        {
            lastGainTick = -999;
            threshPercents = new List<float>
            {
                ThreshLow,
                ThreshHigh
            };
        }

        public override int GUIChangeArrow => GainingNeed ? 1 : -1;

        public override float CurInstantLevel => CurLevel;

        private bool GainingNeed => Find.TickManager.TicksGame < lastGainTick + 10;

        public override void SetInitialLevel()
        {
            CurLevel = ThreshSatisfied;
        }


        public void GainNeed(float amount)
        {
            amount /= 120f;
            amount *= 0.01f;
            amount = Mathf.Min(amount, 1f - CurLevel);
            curLevelInt += amount;
            lastGainTick = Find.TickManager.TicksGame;
        }

        public override void NeedInterval()
        {
            ////Log.Messag("Need Interval");
            if (!CultTracker.Get.ExposedToCults)
            {
                return;
            }

            if (pawn == null)
            {
                return;
            }

            if (!pawn.IsPrisonerOfColony && !pawn.IsColonist && !pawn.IsSlaveOfColony)
            {
                return;
            }

            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
            {
                return;
            }

            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Hearing))
            {
                return;
            }

            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving))
            {
                return;
            }

            if (!baseSet)
            {
                if (ticksUntilBaseSet <= 0)
                {
                    SetBaseLevels();
                }

                ticksUntilBaseSet -= 150;
                return;
            }

            if (CultTracker.Get.PlayerCult != null)
            {
                if (CultTracker.Get.PlayerCult.founder == pawn ||
                    CultTracker.Get.PlayerCult.leader == pawn)
                {
                    return;
                }
            }

            curLevelInt -= 0.00005f;
            if (curLevelInt <= 0)
            {
                curLevelInt = 0;
            }
        }

        private void SetBaseLevels()
        {
            baseSet = true;
            var temp = CurLevel;
            if (pawn == null)
            {
                return;
            }

            temp += CultUtility.GetBaseCultistModifier(pawn);
            if (temp > 0.99f)
            {
                temp = 0.99f;
            }

            if (temp < 0.01f)
            {
                temp = 0.01f;
            }

            if (pawn?.Faction?.def?.defName == "ROM_TheAgency")
            {
                Utility.DebugReport(pawn.Name.ToStringFull + " is a member of the agency. Cult levels set to 1%.");
                temp = 0.01f;
            }

            CurLevel = temp;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref baseSet, "baseSet");
            Scribe_Values.Look(ref ticksUntilBaseSet, "ticksUntilBaseSet", 1000);
        }


        public override string GetTipString()
        {
            return base.GetTipString();
        }

        public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = int.MaxValue, float customMargin = -1, bool drawArrows = true, bool doTooltip = true, Rect? rectForTooltip = null)
        {
            if (!CultTracker.Get.ExposedToCults)
            {
                return;
            }

            //base.DrawOnGUI(rect, maxThresholdMarkers, customMargin, drawArrows, doTooltip);
            if (rect.height > 70f)
            {
                var num = (rect.height - 70f) / 2f;
                rect.height = 70f;
                rect.y += num;
            }

            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(rect);
            }

            TooltipHandler.TipRegion(rect, new TipSignal(GetTipString, rect.GetHashCode()));
            var num2 = 14f;
            var num3 = num2 + 15f;
            if (rect.height < 50f)
            {
                num2 *= Mathf.InverseLerp(0f, 50f, rect.height);
            }

            Text.Font = rect.height <= 55f ? GameFont.Tiny : GameFont.Small;
            Text.Anchor = TextAnchor.LowerLeft;
            var rect2 = new Rect(rect.x + num3 + (rect.width * 0.1f), rect.y,
                rect.width - num3 - (rect.width * 0.1f), rect.height / 2f);
            Widgets.Label(rect2, LabelCap);
            Text.Anchor = TextAnchor.UpperLeft;
            var rect3 = new Rect(rect.x, rect.y + (rect.height / 2f), rect.width, rect.height / 2f);
            rect3 = new Rect(rect3.x + num3, rect3.y, rect3.width - (num3 * 2f), rect3.height - num2);
            Widgets.FillableBar(rect3, CurLevelPercentage, Buttons.RedTex);
            //else Widgets.FillableBar(rect3, this.CurLevelPercentage);
            //Widgets.FillableBarChangeArrows(rect3, this.GUIChangeArrow);
            if (threshPercents != null)
            {
                foreach (var threshPct in threshPercents)
                {
                    DrawBarThreshold(rect3, threshPct);
                }
            }

            var curInstantLevelPercentage = CurInstantLevelPercentage;
            if (curInstantLevelPercentage >= 0f)
            {
                DrawBarInstantMarkerAt(rect3, curInstantLevelPercentage);
            }

            if (!def.tutorHighlightTag.NullOrEmpty())
            {
                UIHighlighter.HighlightOpportunity(rect, def.tutorHighlightTag);
            }

            Text.Font = GameFont.Small;
        }

        private new void DrawBarThreshold(Rect barRect, float threshPct)
        {
            var num = (float) (barRect.width <= 60f ? 1 : 2);
            var position = new Rect(barRect.x + (barRect.width * threshPct) - (num - 1f),
                barRect.y + (barRect.height / 2f), num, barRect.height / 2f);
            Texture2D image;
            if (threshPct < CurLevelPercentage)
            {
                image = BaseContent.BlackTex;
                GUI.color = new Color(1f, 1f, 1f, 0.9f);
            }
            else
            {
                image = BaseContent.GreyTex;
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
            }

            GUI.DrawTexture(position, image);
            GUI.color = Color.white;
        }
    }
}