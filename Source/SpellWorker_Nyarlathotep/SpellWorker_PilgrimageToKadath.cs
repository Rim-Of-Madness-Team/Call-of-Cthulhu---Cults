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
    public class SpellWorker_PilgrimageToKadath : SpellWorker_GameEndingEffect
    {
        protected override bool CanFireNowSub(IIncidentTarget target)
        {

            //Cthulhu.Utility.DebugReport("CanFire: " + this.def.defName);
            return true;
        }
        public override bool CanSummonNow(Map map)
        {
            return true;
        }

        public override float GetDelay()
        {
            return 10f;
        }

        public override string GetEndScreenText()
        {
            return "GameOverPilgrimageToKadath";
        }

        //public override bool TryExecute(IncidentParms parms)
        //{
        //    Map map = parms.target as Map;
        //    StringBuilder stringBuilder = new StringBuilder();
        //    foreach (Pawn current2 in map.mapPawns.FreeColonists)
        //    {
        //        if (current2.Spawned)
        //        {
        //            stringBuilder.AppendLine("   " + current2.LabelCap);
        //            current2.DeSpawn();
        //        }
        //    }
        //    if (stringBuilder.Length == 0)
        //    {
        //        stringBuilder.AppendLine("Nobody".Translate().ToLower());
        //    }
        //    string preCreditsMessage = "GameOverPilgrimageToKadath".Translate(new object[]
        //    {
        //        stringBuilder.ToString()
        //    });
        //    Cults_Screen_Credits screen_Credits = new Cults_Screen_Credits(preCreditsMessage, 10);
        //    screen_Credits.wonGame = true;
        //    Find.WindowStack.Add(screen_Credits);
        //    Find.MusicManagerPlay.ForceSilenceFor(999f);
        //    ScreenFader.StartFade(Color.clear, 3f);
        //    return true;
        //}

    }
}
