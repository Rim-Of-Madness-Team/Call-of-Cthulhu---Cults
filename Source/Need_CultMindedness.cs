using System;
using System.Collections.Generic;

using RimWorld;
using UnityEngine;
using Verse;

namespace CultOfCthulhu
{

    public class Need_CultMindedness : RimWorld.Need
    {

        //public static ThingDef ColanderThingDef;
        
        public const float BaseGainPerTickRate = 150f;
        public const float BaseFallPerTick = 1E-05f;
        public const float ThreshVeryLow = 0.1f;
        public const float ThreshLow = 0.3f;
        public const float ThreshSatisfied = 0.5f;
        public const float ThreshHigh = 0.7f;
        public const float ThreshVeryHigh = 0.9f;

        private bool baseSet = false;
        public int ticksUntilBaseSet = 500;
        private int lastGainTick;

        WorldComponent_GlobalCultTracker globalCultTracker = Find.World.GetComponent<WorldComponent_GlobalCultTracker>();

        static Need_CultMindedness()
        {
            //ColanderThingDef = 
            //<ThingDef>.GetNamed("Apparel_Colander");
        }
        
        public override int GUIChangeArrow
        {
            get
            {
                return this.GainingNeed ? 1 : -1;
            }
        }

        public override float CurInstantLevel
        {
            get
            {
                return this.CurLevel;
            }
        }

        private bool GainingNeed
        {
            get
            {
                return Find.TickManager.TicksGame < this.lastGainTick + 10;
            }
        }

        public Need_CultMindedness(Pawn pawn) : base(pawn)
        {
            this.lastGainTick = -999;
            this.threshPercents = new List<float>();
            this.threshPercents.Add(ThreshLow);
            this.threshPercents.Add(ThreshHigh);
        }

        public override void SetInitialLevel()
        {
            this.CurLevel = ThreshSatisfied;
        }
        

        public void GainNeed(float amount)
        {
            amount /= 120f;
            amount *= 0.01f;
            amount = Mathf.Min(amount, 1f - this.CurLevel);
            this.curLevelInt += amount;
            this.lastGainTick = Find.TickManager.TicksGame;
        }

        public override void NeedInterval()
        {
            ////Log.Messag("Need Interval");
            if (this.pawn == null) return;
            if (!this.pawn.IsPrisonerOfColony && !this.pawn.IsColonist) return;
            if (globalCultTracker.cultFounder == this.pawn) return;
            if (!baseSet)
            {
                if (ticksUntilBaseSet <= 0) SetBaseLevels();
                ticksUntilBaseSet -= 150;
                return;
            }
            this.curLevelInt -= 0.00005f;
            if (this.curLevelInt <= 0) this.curLevelInt = 0;
        }

        public void SetBaseLevels()
        {
            baseSet = true;
            float temp = CurLevel;
            if (this.pawn == null) return;
            temp += CultUtility.GetBaseCultistModifier(this.pawn);
            if (temp > 0.99f) temp = 0.99f;
            if (temp < 0.01f) temp = 0.01f;

            if (this.pawn.Faction.def.defName == "TheAgency")
            {
                Cthulhu.Utility.DebugReport(this.pawn.Name.ToStringFull + " is a member of the agency. Cult levels set to 1%.");
                temp = 0.01f;
            }
            this.CurLevel = temp;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.baseSet, "baseSet", false, false);
            Scribe_Values.Look<int>(ref this.ticksUntilBaseSet, "ticksUntilBaseSet", 1000, false);
        }


        public override string GetTipString()
        {
            return base.GetTipString();
        }


        public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = int.MaxValue, float customMargin = -1F, bool drawArrows = true, bool doTooltip = true)
        {
            //base.DrawOnGUI(rect, maxThresholdMarkers, customMargin, drawArrows, doTooltip);
            if (rect.height > 70f)
            {
                float num = (rect.height - 70f) / 2f;
                rect.height = 70f;
                rect.y += num;
            }
            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(rect);
            }
            TooltipHandler.TipRegion(rect, new TipSignal(() => this.GetTipString(), rect.GetHashCode()));
            float num2 = 14f;
            float num3 = num2 + 15f;
            if (rect.height < 50f)
            {
                num2 *= Mathf.InverseLerp(0f, 50f, rect.height);
            }
            Text.Font = ((rect.height <= 55f) ? GameFont.Tiny : GameFont.Small);
            Text.Anchor = TextAnchor.LowerLeft;
            Rect rect2 = new Rect(rect.x + num3 + rect.width * 0.1f, rect.y, rect.width - num3 - rect.width * 0.1f, rect.height / 2f);
            Widgets.Label(rect2, this.LabelCap);
            Text.Anchor = TextAnchor.UpperLeft;
            Rect rect3 = new Rect(rect.x, rect.y + rect.height / 2f, rect.width, rect.height / 2f);
            rect3 = new Rect(rect3.x + num3, rect3.y, rect3.width - num3 * 2f, rect3.height - num2);
            Widgets.FillableBar(rect3, this.CurLevelPercentage, Buttons.RedTex);
            //else Widgets.FillableBar(rect3, this.CurLevelPercentage);
            //Widgets.FillableBarChangeArrows(rect3, this.GUIChangeArrow);
            if (this.threshPercents != null)
            {
                for (int i = 0; i < this.threshPercents.Count; i++)
                {
                    this.DrawBarThreshold(rect3, this.threshPercents[i]);
                }
            }
            float curInstantLevelPercentage = this.CurInstantLevelPercentage;
            if (curInstantLevelPercentage >= 0f)
            {
                this.DrawBarInstantMarkerAt(rect3, curInstantLevelPercentage);
            }
            if (!this.def.tutorHighlightTag.NullOrEmpty())
            {
                UIHighlighter.HighlightOpportunity(rect, this.def.tutorHighlightTag);
            }
            Text.Font = GameFont.Small;
        }

        private void DrawBarThreshold(Rect barRect, float threshPct)
        {
            float num = (float)((barRect.width <= 60f) ? 1 : 2);
            Rect position = new Rect(barRect.x + barRect.width * threshPct - (num - 1f), barRect.y + barRect.height / 2f, num, barRect.height / 2f);
            Texture2D image;
            if (threshPct < this.CurLevelPercentage)
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
