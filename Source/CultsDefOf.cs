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
using AbilityUser;
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

namespace CultOfCthulhu
{

    [DefOf]
    public class CultsDefOf
    {
        // ============= UNSORTED ============

        public static ThingDef Cults_ElixerOfPower;
        public static ThingDef Cults_BlackIchorMeal;
        public static ThingDef Cults_SignOfDagon;
        public static DamageDef Cults_Psionic;
        public static AbilityUser.AbilityDef Cults_PsionicBlast;
        public static AbilityUser.AbilityDef Cults_PsionicShock;
        public static AbilityUser.AbilityDef Cults_PsionicBurn;
        public static RoomRoleDef Cults_Temple;
        public static ThingDef Cults_ForbiddenKnowledgeCenter;
        public static LetterDef Cults_StandardMessage;
        public static ThoughtDef Cults_PrayedInImpressiveTemple;
        public static ThingDef Cults_TransmogAura;



        // ============ GAME CONDITIONS =======

        public static GameConditionDef CultgameCondition_StarsAreWrong;
        public static GameConditionDef CultgameCondition_StarsAreRight;


        // ============ DUTY DEFS =============

        public static DutyDef Cults_LoadAndEnterTransportersPawn;

        // =============== JOBS ===============

        // Flying Pawn

        public static JobDef Cults_EnterTransporterPawn;

        // Offering Related Defs

        public static JobDef Cults_GiveOffering;

        public static JobDef Cults_ReflectOnOffering;

        // Sacrifice Related Defs

        public static JobDef Cults_HoldSacrifice;

        public static JobDef Cults_AttendSacrifice;

        public static JobDef Cults_ReflectOnResult;

        public static JobDef Cults_WaitTiedDown;

        // Worship Related Jobs

        public static JobDef Cults_HoldWorship;

        public static JobDef Cults_AttendWorship;

        public static JobDef Cults_ReflectOnWorship;

        public static JobDef Cults_Investigate;

        public static JobDef Cults_WriteTheBook;

        // Misc Jobs

        public static JobDef Cults_MidnightInquisition;

        public static JobDef Cults_PruneAndRepair;

        // ============== FACTIONS =============

        public static FactionDef Cults_Sailors;

        // =============== PAWNS ===============

        public static PawnKindDef Cults_BlackGoat;

        public static PawnKindDef Cults_Byakhee;

        public static PawnKindDef Cults_FormlessSpawn;

        public static PawnKindDef Cults_Sailor;

        // ========== MENTAL STATE DEF ==========

        public static MentalStateDef Cults_DeepSleepCarcosa;


        // =============== HEDIFFS ===============

        public static HediffDef Cults_PsionicBrain;

        public static HediffDef Cults_MonstrousBody;

        public static HediffDef Cults_TentacleArm;

        public static HediffDef Cults_CthulhidTentacle;

        public static HediffDef Cults_CthulhidEyestalk;

        public static HediffDef Cults_SleepHediff;

        // =============== THOUGHTS ===============

        // 1.2.2
        public static ThoughtDef Cults_OtherPrisonerWasSacrificed;

        public static ThoughtDef Cults_MadeInvestigation;

        public static ThoughtDef Cults_BlackoutBook;

        public static ThoughtDef Cults_HeldSermon;

        public static ThoughtDef Cults_FoundedCult;

        public static ThoughtDef Cults_MidnightInquisitionThought;

        //Sacrifice

        public static ThoughtDef Cults_AttendedSuccessfulSacrifice;

        public static ThoughtDef Cults_AttendedFailedSacrifice;

        public static ThoughtDef Cults_InnocentAttendedSuccessfulSacrifice;

        public static ThoughtDef Cults_InnocentAttendedFailedSacrifice;

        //Relationship Sacrifice Thoughts

        public static ThoughtDef Cults_ExecutedFamily;

        public static ThoughtDef Cults_ExecutedPet;

        public static ThoughtDef Cults_SacrificedFamily;

        public static ThoughtDef Cults_SacrificedPet;

        public static ThoughtDef Cults_SacrificedFriend;

        public static ThoughtDef Cults_SacrificedRival;

        // Worship

        public static ThoughtDef Cults_AttendedIncredibleSermonAsCultist;

        public static ThoughtDef Cults_AttendedIncredibleSermonAsInnocent;

        public static ThoughtDef Cults_AttendedGreatSermonAsCultist;

        public static ThoughtDef Cults_AttendedGreatSermonAsInnocent;

        public static ThoughtDef Cults_AttendedGoodSermonAsCultist;

        public static ThoughtDef Cults_AttendedGoodSermonAsInnocent;

        public static ThoughtDef Cults_AttendedDecentSermonAsCultist;

        public static ThoughtDef Cults_AttendedDecentSermonAsInnocent;

        public static ThoughtDef Cults_AttendedAwfulSermonAsCultist;

        public static ThoughtDef Cults_AttendedAwfulSermonAsInnocent;

        // Misc

        public static ThoughtDef Cults_SawAurora;

        // =============== SOUNDS ===============

        public static SoundDef RitualChanting;

        // =============== BUILDINGS ===============

        public static ThingDef Cults_SleepTotem;


        public static ThingDef Cults_FertilityTotem;

        public static ThingDef Cults_TreasureChest;

        public static ThingDef Cults_TreasureChest_Relic;

        public static ThingDef Cults_SunkenShipChunk;

        public static ThingDef Cults_MonolithNightmare;

        public static ThingDef Cults_LandedShip;


        // ============== THINGS ==============

        public static ThingDef Cults_WombBetweenWorlds;

        public static ThingDef Cults_PlantTreeNightmare;

        public static ThingDef Cults_Grimoire;

        public static ThingDef Cults_TheKingInYellow;

        // ============== MAP CONDITIONS ==============

        public static GameConditionDef Cults_Aurora;

        // ============= CORE REFERENCES ==============
        
        public static ThingDef Penoxycyline;

        public static ThingDef BlocksSlate;
        
        public static ThingDef BlocksLimestone;

        public static ThingDef BlocksMarble;

        public static ThingDef Neutroamine;

        public static PawnKindDef Rat;
        
        public static MentalStateDef FireStartingSpree;
        
        public static ResearchProjectDef Forbidden_Reports;
    }
}
