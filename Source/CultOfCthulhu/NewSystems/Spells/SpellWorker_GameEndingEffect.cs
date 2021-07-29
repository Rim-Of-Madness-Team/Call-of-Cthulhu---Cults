// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using System.Text;
using RimWorld;
using UnityEngine;
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
    public abstract class SpellWorker_GameEndingEffect : SpellWorker
    {
        public static Map map;
        public static string message;
        public static float delay;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return true;
        }

        public override bool CanSummonNow(Map map)
        {
            return true;
        }

        public abstract string GetEndScreenText();

        public abstract float GetDelay();

        public string MakeEndScreenText()
        {
            var stringBuilder = new StringBuilder();
            foreach (var current2 in map.mapPawns.FreeColonists)
            {
                if (!current2.Spawned)
                {
                    continue;
                }

                stringBuilder.AppendLine("   " + current2.LabelCap);
                current2.DeSpawn();
            }

            if (stringBuilder.Length == 0)
            {
                stringBuilder.AppendLine("Nobody".Translate().ToLower());
            }

            string preCreditsMessage = GetEndScreenText().Translate(
                stringBuilder.ToString()
            );
            return preCreditsMessage;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            map = parms.target as Map;
            message = MakeEndScreenText();
            delay = GetDelay();
            LongEventHandler.QueueLongEvent(delegate
            {
                var screen_Credits = new Cults_Screen_Credits(message, 10)
                {
                    wonGame = true
                };
                Find.WindowStack.Add(screen_Credits);
                Find.MusicManagerPlay.ForceSilenceFor(999f);
                ScreenFader.StartFade(Color.clear, 3f);
            }, "Cults_SpellGameEndingEffect", false, null);
            return true;
        }
    }
}