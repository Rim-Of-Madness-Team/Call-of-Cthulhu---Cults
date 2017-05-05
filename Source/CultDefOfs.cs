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

    [DefOf]
    public class CultDefOfs
    {

        // ============ DUTY DEFS =============
        
        public static DutyDef Cults_LoadAndEnterTransportersPawn;

        // =============== JOBS ===============

        // Flying Pawn

        public static JobDef EnterTransporterPawn;

        // Offering Related Defs

        public static JobDef GiveOffering;

        public static JobDef ReflectOnOffering;

        // Sacrifice Related Defs

        public static JobDef HoldSacrifice;

        public static JobDef AttendSacrifice;

        public static JobDef ReflectOnResult;

        public static JobDef WaitTiedDown;

        // Worship Related Jobs

        public static JobDef HoldWorship;

        public static JobDef AttendWorship;

        public static JobDef ReflectOnWorship;

        public static JobDef Investigate;

        public static JobDef WriteTheBook;

        // Misc Jobs

        public static JobDef MidnightInquisition;

        public static JobDef PruneAndRepair;

        // ============== FACTIONS =============

        public static FactionDef Cults_Sailors;

        // =============== PAWNS ===============

        public static PawnKindDef BlackIbex;

        public static PawnKindDef Cults_Byakhee;

        public static PawnKindDef Rat;

        public static PawnKindDef Cults_Sailor;

        // ========== MENTAL STATE DEF ==========

        public static MentalStateDef Cults_DeepSleepCarcosa;


        // =============== HEDIFFS ===============

        public static HediffDef Cults_PsionicBrain;

        public static HediffDef Cults_MonstrousBrain;

        public static HediffDef Cults_MonstrousBody;

        public static HediffDef Cults_TentacleArm;

        public static HediffDef Cults_CthulhidTentacle;

        public static HediffDef Cults_CthulhidEyestalk;

        // =============== THOUGHTS ===============

        // 1.2.2 New
        public static ThoughtDef OtherPrisonerWasSacrificed;

        //Sacrifice

        public static ThoughtDef AttendedSuccessfulSacrifice;

        public static ThoughtDef AttendedFailedSacrifice;

        public static ThoughtDef InnocentAttendedSuccessfulSacrifice;

        public static ThoughtDef InnocentAttendedFailedSacrifice;

        //Relationship Sacrifice Thoughts

        public static ThoughtDef ExecutedFamily;

        public static ThoughtDef ExecutedPet;

        public static ThoughtDef ExecutedFriend;

        public static ThoughtDef SacrificedFamily;

        public static ThoughtDef SacrificedPet;

        public static ThoughtDef SacrificedFriend;

        public static ThoughtDef SacrificedRival;

        public static ThoughtDef WitnessedSacrificeBloodlust;

        // Worship

        public static ThoughtDef AttendedIncredibleSermonAsCultist;

        public static ThoughtDef AttendedIncredibleSermonAsInnocent;

        public static ThoughtDef AttendedGreatSermonAsCultist;

        public static ThoughtDef AttendedGreatSermonAsInnocent;

        public static ThoughtDef AttendedGoodSermonAsCultist;

        public static ThoughtDef AttendedGoodSermonAsInnocent;

        public static ThoughtDef AttendedDecentSermonAsCultist;

        public static ThoughtDef AttendedDecentSermonAsInnocent;

        public static ThoughtDef AttendedAwfulSermonAsCultist;

        public static ThoughtDef AttendedAwfulSermonAsInnocent;

        // Misc

        public static ThoughtDef SawAurora;

        // =============== SOUNDS ===============

        public static SoundDef RitualChanting;

        // =============== BUILDINGS ===============

        public static ThingDef Cults_FertilityTotem;

        public static ThingDef TreasureChest;

        public static ThingDef TreasureChest_Relic;

        public static ThingDef SunkenShipChunk;

        public static ThingDef MonolithNightmare;

        public static ThingDef Cults_LandedShip;

        // ============== THINGS ==============
        
        public static ThingDef Cults_ByakheeRace;

        public static ThingDef WombBetweenWorlds;

        public static ThingDef PlantTreeNightmare;

        public static ThingDef Cults_Grimoire;

        public static ThingDef Cults_TheKingInYellow;

        // ============== MAP CONDITIONS ==============

        public static MapConditionDef Cults_Aurora;

        // ============= VANILLA OBJECTS ==============
        
        public static ThingDef Penoxycyline;

        public static ThingDef BlocksSlate;
        
        public static ThingDef BlocksLimestone;

        public static ThingDef BlocksMarble;

        public static ThingDef Neutroamine;

        public static ThingDef Jade;

        // ============= MENTAL STATES ===============

        public static MentalStateDef FireStartingSpree;

        public static MentalStateDef WanderConfused;


    }
}
