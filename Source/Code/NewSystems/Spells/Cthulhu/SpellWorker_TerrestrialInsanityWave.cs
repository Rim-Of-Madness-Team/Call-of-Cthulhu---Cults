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
                map?.mapPawns.AllPawnsSpawned.FindAll(match: x => x.RaceProps.intelligence == Intelligence.Humanlike);
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
                if (pawn.Faction == Faction.OfPlayer || !pawn.Faction.HostileTo(other: Faction.OfPlayer) ||
                    pawn.guest.IsPrisoner)
                {
                    Utility.ApplySanityLoss(pawn: pawn, sanityLoss: Rand.Range(min: 0.2f, max: 0.8f));
                }
                else
                {
                    var defaultState = MentalStateDefOf.Berserk;
                    var tempRand = Rand.Range(min: 1, max: 10);
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

                    Utility.ApplySanityLoss(pawn: pawn, sanityLoss: 1.0f);
                    pawn.mindState.mentalStateHandler.TryStartMentalState(stateDef: defaultState);
                }
            }

            return true;
        }
    }
}