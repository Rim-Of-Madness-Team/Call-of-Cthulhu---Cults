using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using HugsLib;
using HugsLib.Settings;

namespace CultOfCthulhu
{
    [StaticConstructorOnStartup]
    public static class HugsModOptionalCode
    {
        static HugsModOptionalCode()
        {
            // Thank god Zhentar & Erdelf
            LongEventHandler.QueueLongEvent(() =>
            {

                cultsForcedInvestigation = () => true;

                cultsStudySuccessfulCultsIsRepeatable = () => true;
                
                cultsShowDebugCode = () => false;

                try
                {
                    ((Action)(() =>
                    {
                        //Modpack Settings
                        ModSettingsPack settings = HugsLibController.Instance.Settings.GetModSettings("CultOfCthulhu");

                        settings.EntryName = "CallOfCthulhuCults".Translate();


                        object forcedInvestigation = settings.GetHandle<bool>(
                            "cultsForcedInvestigation",
                            "ForcedInvestigation".Translate(),
                            "ForcedInvestigationDesc".Translate(),
                            true);


                        object studySuccessfulCultsIsRepeatable = settings.GetHandle<bool>(
                            "cultsStudySuccessfulCultsIsRepeatable",
                            "StudySuccessfulCultsIsRepeatable".Translate(),
                            "StudySuccessfulCultsIsRepeatableDesc".Translate(),
                            true);

                        object showDebugCode = settings.GetHandle<bool>(
                            "cultsShowDebugCode",
                            "ShowDebugCode".Translate(),
                            "ShowDebugCodeDesc".Translate(),
                            false);

                        cultsForcedInvestigation = () => (SettingHandle<bool>)forcedInvestigation;
                        cultsStudySuccessfulCultsIsRepeatable = () => (SettingHandle<bool>)studySuccessfulCultsIsRepeatable;
                        cultsShowDebugCode = () => (SettingHandle<bool>)showDebugCode;

                    }))();
                }
                catch (TypeLoadException)
                { }
            }, "queueHugsLibCultOfCthulhu", false, null);
        
        }

        public static Func<bool> cultsForcedInvestigation;

        public static Func<bool> cultsStudySuccessfulCultsIsRepeatable;

        public static Func<bool> cultsShowDebugCode;

    }
}
