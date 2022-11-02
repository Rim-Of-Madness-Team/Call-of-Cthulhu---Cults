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

        public ModMain(ModContentPack content) : base(content: content)
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
            Widgets.CheckboxLabeled(rect: new Rect(x: inRect.x + offset, y: inRect.y, width: inRect.width - offset, height: height),
                label: "ForcedInvestigation".Translate(), checkOn: ref settings.cultsForcedInvestigation);
            Widgets.CheckboxLabeled(
                rect: new Rect(x: inRect.x + offset, y: inRect.y + offset + spacer, width: inRect.width - offset, height: height),
                label: "StudySuccessfulCultsIsRepeatable".Translate(),
                checkOn: ref settings.cultsStudySuccessfulCultsIsRepeatable);
            Widgets.CheckboxLabeled(
                rect: new Rect(x: inRect.x + offset, y: inRect.y + offset + spacer + offset + spacer, width: inRect.width - offset,
                    height: height), label: "Cults_MakeWorshipsVoluntary".Translate(), checkOn: ref settings.makeWorshipsVoluntary);
            Widgets.CheckboxLabeled(
                rect: new Rect(x: inRect.x + offset, y: inRect.y + offset + spacer + offset + spacer + offset + spacer,
                    width: inRect.width - offset,
                    height: height), label: "ShowDebugCode".Translate(), checkOn: ref settings.cultsShowDebugCode);
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
            Scribe_Values.Look(value: ref cultsForcedInvestigation, label: "cultsForcedInvestigation", defaultValue: true);
            Scribe_Values.Look(value: ref cultsStudySuccessfulCultsIsRepeatable,
                label: "cultsStudySuccessfulCultsIsRepeatable", defaultValue: true);
            Scribe_Values.Look(value: ref cultsShowDebugCode, label: "cultsShowDebugCode", defaultValue: true);
            Scribe_Values.Look(value: ref makeWorshipsVoluntary, label: "makeWorshipsVoluntary");
        }
    }
}