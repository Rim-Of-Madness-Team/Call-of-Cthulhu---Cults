// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

// ----------------------------------------------------------------------
// These are RimWorld-specific usings. Activate/Deactivate what you need:
// ----------------------------------------------------------------------
using UnityEngine;         // Always needed
//using VerseBase;         // Material/Graphics handling functions are found here
using Verse;               // RimWorld universal objects are here (like 'Building')
using Verse.AI;          // Needed when you do something with the AI
using Verse.AI.Group;
using Verse.Sound;       // Needed when you do something with Sound
using Verse.Noise;       // Needed when you do something with Noises
using RimWorld;            // RimWorld specific functions are found here (like 'Building_Battery')
using RimWorld.Planet;   // RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 


namespace CultOfCthulhu
{
    public class SpellWorker_AuroraEffect : SpellWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            //Cthulhu.Utility.DebugReport("CanFire: " + this.def.defName);
            Map map = (Map)parms.target;
            return !map.GameConditionManager.ConditionIsActive(CultsDefOf.Cults_Aurora);
        }
        
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            this.DoConditionAndLetter(Mathf.RoundToInt(this.def.durationDays.RandomInRange * 60000f), parms.target);
            SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera();
            return true;
        }

        protected void DoConditionAndLetter(int duration, IIncidentTarget target)
        {
            Map map = (Map)target;
            //Cthulhu.Utility.DebugReport("Generating Map Condition");
            GameCondition_AuroraEffect GameCondition = (GameCondition_AuroraEffect)GameConditionMaker.MakeCondition(CultsDefOf.Cults_Aurora, duration, 0);
            string text3 = "";
            //Cthulhu.Utility.DebugReport("Getting coords.");
            Vector2 coords = Find.WorldGrid.LongLatOf(map.Tile);
            if (coords.y >= 74)
            {
                text3 = "Borealis";
            }
            else
            {
                text3 = "Australis";
            }
            //Cthulhu.Utility.DebugReport("Getting label");
            string textLabel = "LetterLabelAurora".Translate(new object[]
            {
                text3
            });
            //Cthulhu.Utility.DebugReport("Registering Conditions");
            map.GameConditionManager.RegisterCondition(GameCondition);
            string textDesc = "LetterIncidentAurora".Translate();
            //Cthulhu.Utility.DebugReport("Sending letter");
            Find.LetterStack.ReceiveLetter(textLabel, textDesc, LetterDefOf.PositiveEvent, null);
            map.GetComponent<MapComponent_SacrificeTracker>().lastLocation = IntVec3.Invalid;
        }
    }
}
