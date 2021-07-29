// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

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
    public class SpellWorker_AuroraEffect : SpellWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            //Cthulhu.Utility.DebugReport("CanFire: " + this.def.defName);
            var map = (Map) parms.target;
            return !map.GameConditionManager.ConditionIsActive(CultsDefOf.Cults_Aurora);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            DoConditionAndLetter(Mathf.RoundToInt(def.durationDays.RandomInRange * 60000f), parms.target);
            SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera();
            return true;
        }

        protected void DoConditionAndLetter(int duration, IIncidentTarget target)
        {
            var map = (Map) target;
            //Cthulhu.Utility.DebugReport("Generating Map Condition");
            var GameCondition =
                (GameCondition_AuroraEffect) GameConditionMaker.MakeCondition(CultsDefOf.Cults_Aurora, duration);
            //Cthulhu.Utility.DebugReport("Getting coords.");
            var coords = Find.WorldGrid.LongLatOf(map.Tile);
            var text3 = coords.y >= 74 ? "Borealis" : "Australis";

            //Cthulhu.Utility.DebugReport("Getting label");
            string textLabel = "LetterLabelAurora".Translate(
                text3
            );
            //Cthulhu.Utility.DebugReport("Registering Conditions");
            map.GameConditionManager.RegisterCondition(GameCondition);
            string textDesc = "LetterIncidentAurora".Translate();
            //Cthulhu.Utility.DebugReport("Sending letter");
            Find.LetterStack.ReceiveLetter(textLabel, textDesc, LetterDefOf.PositiveEvent);
            map.GetComponent<MapComponent_SacrificeTracker>().lastLocation = IntVec3.Invalid;
        }
    }
}