using UnityEngine;
using Verse;

namespace CultOfCthulhu
{
    [StaticConstructorOnStartup]
    public static class ModSettings_Data
    {
        public static bool cultsForcedInvestigation = true;
        public static bool cultsStudySuccessfulCultsIsRepeatable = true;
        public static bool cultsShowDebugCode;
        public static bool makeWorshipsVoluntary;
    }

    public class ModMain : Mod
    {
        private readonly Settings settings;

        public ModMain(ModContentPack content) : base(content)
        {
            settings = GetSettings<Settings>();
            ModSettings_Data.cultsForcedInvestigation = settings.cultsForcedInvestigation;
            ModSettings_Data.makeWorshipsVoluntary = settings.makeWorshipsVoluntary;
            ModSettings_Data.cultsStudySuccessfulCultsIsRepeatable =
                settings.cultsStudySuccessfulCultsIsRepeatable;
            ModSettings_Data.cultsShowDebugCode = settings.cultsShowDebugCode;
        }

        public override string SettingsCategory()
        {
            return "Call of Cthulhu - Cults";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var offset = 30;
            var spacer = 5;
            var height = 30;
            Widgets.CheckboxLabeled(new Rect(inRect.x + offset, inRect.y, inRect.width - offset, height),
                "ForcedInvestigation".Translate(), ref settings.cultsForcedInvestigation);
            Widgets.CheckboxLabeled(
                new Rect(inRect.x + offset, inRect.y + offset + spacer, inRect.width - offset, height),
                "StudySuccessfulCultsIsRepeatable".Translate(),
                ref settings.cultsStudySuccessfulCultsIsRepeatable);
            Widgets.CheckboxLabeled(
                new Rect(inRect.x + offset, inRect.y + offset + spacer + offset + spacer, inRect.width - offset,
                    height), "Cults_MakeWorshipsVoluntary".Translate(), ref settings.makeWorshipsVoluntary);
            Widgets.CheckboxLabeled(
                new Rect(inRect.x + offset, inRect.y + offset + spacer + offset + spacer + offset + spacer,
                    inRect.width - offset,
                    height), "ShowDebugCode".Translate(), ref settings.cultsShowDebugCode);
            settings.Write();
        }
    }

    public class Settings : ModSettings
    {
        public bool cultsForcedInvestigation = true;
        public bool cultsShowDebugCode;
        public bool cultsStudySuccessfulCultsIsRepeatable = true;
        public bool makeWorshipsVoluntary;


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref cultsForcedInvestigation, "cultsForcedInvestigation", true);
            Scribe_Values.Look(ref cultsStudySuccessfulCultsIsRepeatable,
                "cultsStudySuccessfulCultsIsRepeatable", true);
            Scribe_Values.Look(ref cultsShowDebugCode, "cultsShowDebugCode", true);
            Scribe_Values.Look(ref makeWorshipsVoluntary, "makeWorshipsVoluntary");
        }
    }
}