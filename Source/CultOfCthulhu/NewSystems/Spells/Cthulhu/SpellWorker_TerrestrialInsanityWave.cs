// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using Cthulhu;
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
    public class SpellWorker_TerrestrialInsanityWave : SpellWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            //Cthulhu.Utility.DebugReport("
            //: " + this.def.defName);
            return true;
        }

        public override bool CanSummonNow(Map map)
        {
            return true;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var map = parms.target as Map;
            var listeners =
                map?.mapPawns.AllPawnsSpawned.FindAll(x => x.RaceProps.intelligence == Intelligence.Humanlike);
            if (listeners != null)
            {
                var unused = new bool[listeners.Count];
            }

            if (listeners == null)
            {
                return true;
            }

            foreach (var pawn in listeners)
            {
                if (pawn.Faction == Faction.OfPlayer || !pawn.Faction.HostileTo(Faction.OfPlayer) ||
                    pawn.guest.IsPrisoner)
                {
                    Utility.ApplySanityLoss(pawn, Rand.Range(0.2f, 0.8f));
                }
                else
                {
                    var defaultState = MentalStateDefOf.Berserk;
                    var tempRand = Rand.Range(1, 10);
                    switch (tempRand)
                    {
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                            break;
                        case 7:
                        case 8:
                        case 9:
                            defaultState = MentalStateDefOf.PanicFlee;
                            break;
                        case 10:
                            defaultState = CultsDefOf.FireStartingSpree;
                            break;
                    }

                    Utility.ApplySanityLoss(pawn, 1.0f);
                    pawn.mindState.mentalStateHandler.TryStartMentalState(defaultState);
                }
            }

            return true;
        }
    }
}