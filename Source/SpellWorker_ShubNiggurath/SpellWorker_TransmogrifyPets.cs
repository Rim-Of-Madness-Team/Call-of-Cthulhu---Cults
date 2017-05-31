
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;                
using Verse;              
using Verse.AI;          
using Verse.AI.Group;
using Verse.Sound;       
using Verse.Noise;       
using RimWorld;            
using RimWorld.Planet;   
//using RimWorld.SquadAI;  

namespace CultOfCthulhu
{

    public class SpellWorker_TransmogrifyPets : SpellWorker
    {


        public override bool CanSummonNow(Map map)
        {

            //Cthulhu.Utility.DebugReport("CanFire: " + this.def.defName);
            if (PetsToTransmogrify(map).Count<Pawn>() > 0)
                return true;
            Messages.Message("No pets to transmogrify", MessageSound.RejectInput);
            return false;
        }

        public override bool TryExecute(IncidentParms parms)
        {
            Map map = parms.target as Map;
            Transmogrify(map);

            return true;
        }


        public IEnumerable<Pawn> PetsToTransmogrify(Map map)
        {
                //Get a pet with a master.
                IEnumerable<Pawn> one = from Pawn pets in map.mapPawns.AllPawnsSpawned
                                        where pets.RaceProps.Animal && pets.Faction == Faction.OfPlayer && !pets.Dead && !pets.Downed && pets.RaceProps.petness > 0f && pets.playerSettings.master != null
                                        select pets;
                //No master? Okay, still search for pets.
                if (one.Count<Pawn>() == 0)
                {
                    one = from Pawn pets in map.mapPawns.AllPawnsSpawned
                          where pets.RaceProps.Animal && pets.Faction == Faction.OfPlayer && !pets.Dead && !pets.Downed && pets.RaceProps.petness > 0f
                          select pets;
                }
                //No pets? Okay, search for player animals.
                if (one.Count<Pawn>() == 0)
                {
                    one = from Pawn pets in map.mapPawns.AllPawnsSpawned
                          where pets.RaceProps.Animal && pets.Faction == Faction.OfPlayer && !pets.Dead && !pets.Downed
                          select pets;
                }
                //Return anything if we find anything, or return a null, it's all good.
                return one;
            
        }

        public void Transmogrify(Map map, Pawn pawn = null)
        {
            //No pawn? Okay, find one.
            if (pawn == null)
                pawn = PetsToTransmogrify(map).RandomElement<Pawn>();

            CompTransmogrified compTrans = pawn.GetComp<CompTransmogrified>();
            if (compTrans != null)
            {
                compTrans.isTransmogrified = true;
            }

            Messages.Message("Cults_TransmogrifyMessage".Translate(new object[]
                {
                    pawn.LabelShort
                }), MessageSound.Benefit);
        }

    }
}
