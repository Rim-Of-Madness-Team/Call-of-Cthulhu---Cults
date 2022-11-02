// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using RimWorld;
using Verse;

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
    public class SpellWorker_TransdimensionalConvergence : SpellWorker_GameEndingEffect
    {
        protected override bool CanFireNowSub(IncidentParms parms)
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
            return "GameOverTransdimensionalConvergence";
        }

        //protected override bool TryExecuteWorker(IncidentParms parms)
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
        //    string preCreditsMessage = "GameOverTransdimensionalConvergence".Translate(new object[]
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