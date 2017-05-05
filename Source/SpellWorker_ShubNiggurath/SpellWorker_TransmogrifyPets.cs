
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
            if (map.GetComponent<MapComponent_TransmogrifyTracker>().PetsToTransmogrify.Count<Pawn>() > 0)
                return true;
            Messages.Message("No pets to transmogrify", MessageSound.RejectInput);
            return false;
        }

        public override bool TryExecute(IncidentParms parms)
        {
            Map map = parms.target as Map;
            map.GetComponent<MapComponent_TransmogrifyTracker>().Transmogrify();

            return true;
        }

    }
}
