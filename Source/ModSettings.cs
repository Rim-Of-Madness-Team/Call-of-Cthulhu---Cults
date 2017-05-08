using UnityEngine;
using Verse;

namespace CultOfCthulhu
{
    [StaticConstructorOnStartup]
    public static class ModSettings_Data
    {
        public static bool cultsForcedInvestigation = true;
        public static bool cultsStudySuccessfulCultsIsRepeatable = true;
        public static bool cultsShowDebugCode = true;
    }

    public class ModMain : Mod
    {
        Settings settings;

        public ModMain(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<Settings>();
            ModSettings_Data.cultsForcedInvestigation = this.settings.cultsForcedInvestigation;
            ModSettings_Data.cultsStudySuccessfulCultsIsRepeatable = this.settings.cultsStudySuccessfulCultsIsRepeatable;
            ModSettings_Data.cultsShowDebugCode = this.settings.cultsShowDebugCode;
        }

        public override string SettingsCategory() => "Call of Cthulhu - Cults";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            int offset = 30;
            int spacer = 5;
            int height = 30;
            Widgets.CheckboxLabeled(new Rect(inRect.x + offset, inRect.y, inRect.width - offset, height), "ForcedInvestigation".Translate(), ref this.settings.cultsForcedInvestigation);
            Widgets.CheckboxLabeled(new Rect(inRect.x + offset, inRect.y + offset + spacer, inRect.width - offset, height), "StudySuccessfulCultsIsRepeatable".Translate(), ref this.settings.cultsStudySuccessfulCultsIsRepeatable);
            Widgets.CheckboxLabeled(new Rect(inRect.x + offset, inRect.y + offset + spacer + offset + spacer, inRect.width - offset, height), "ShowDebugCode".Translate(), ref this.settings.cultsForcedInvestigation);
            this.settings.Write();

        }
        
    }

    public class Settings : ModSettings
    {
        public bool cultsForcedInvestigation = true;
        public bool cultsStudySuccessfulCultsIsRepeatable = true;
        public bool cultsShowDebugCode = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.cultsForcedInvestigation, "cultsForcedInvestigation", true);
            Scribe_Values.Look<bool>(ref this.cultsStudySuccessfulCultsIsRepeatable, "cultsStudySuccessfulCultsIsRepeatable", true);
            Scribe_Values.Look<bool>(ref this.cultsShowDebugCode, "cultsShowDebugCode", true);
        }
    }
    
}
