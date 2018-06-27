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
            Messages.Message("No pets to transmogrify", MessageTypeDefOf.RejectInput);
            return false;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = parms.target as Map;
            Transmogrify(map);

            return true;
        }


        public IEnumerable<Pawn> PetsToTransmogrify(Map map)
        {
            //Get a pet with a master.
            IEnumerable<Pawn> one = from Pawn pets in map.mapPawns.AllPawnsSpawned
                where !(pets?.GetComp<CompTransmogrified>()?.IsTransmogrified ?? true) && pets.RaceProps.Animal &&
                      pets.Faction == Faction.OfPlayer && !pets.Dead && !pets.Downed && pets.RaceProps.petness > 0f &&
                      pets.playerSettings.Master != null
                select pets;
            //No master? Okay, still search for pets.
            if (one.Count<Pawn>() == 0)
            {
                one = from Pawn pets in map.mapPawns.AllPawnsSpawned
                    where !(pets?.GetComp<CompTransmogrified>()?.IsTransmogrified ?? true) && pets.RaceProps.Animal &&
                          pets.Faction == Faction.OfPlayer && !pets.Dead && !pets.Downed && pets.RaceProps.petness > 0f
                    select pets;
            }
            //No pets? Okay, search for player animals.
            if (one.Count<Pawn>() == 0)
            {
                one = from Pawn pets in map.mapPawns.AllPawnsSpawned
                    where !(pets?.GetComp<CompTransmogrified>()?.IsTransmogrified ?? true) && pets.RaceProps.Animal &&
                          pets.Faction == Faction.OfPlayer && !pets.Dead && !pets.Downed
                    select pets;
            }
            //Return anything if we find anything, or return a null, it's all good.
            return one;
        }

        public void Transmogrify(Map map, Pawn pawn = null, int count = 3)
        {
            if (count <= 0) return;
            //No pawn? Okay, find one.

            if (pawn == null)
            {
                pawn = PetsToTransmogrify(map).RandomElement<Pawn>();
            }

            Messages.Message("Cults_TransmogrifyAnimalsOnly".Translate(new object[]
            {
                pawn.LabelShort
            }), MessageTypeDefOf.NeutralEvent);


            TargetingParameters parms = new TargetingParameters();
            parms.canTargetPawns = true;
            bool foundTarget = false;
            int thisCount = count;

            Find.Targeter.BeginTargeting(parms, delegate(LocalTargetInfo t)
            {
                if (t.Thing is Pawn tP)
                {
                    if (tP?.RaceProps?.Animal ?? false)
                    {
                        pawn = tP;
                        CompTransmogrified compTrans = tP.GetComp<CompTransmogrified>();
                        if (compTrans != null)
                        {
                            compTrans.IsTransmogrified = true;
                            foundTarget = true;
                            Messages.Message("Cults_TransmogrifyMessage".Translate(
                                new object[] //Cults_AspectOfCthulhu_TargetACharacter
                                {
                                    pawn.LabelShort
                                }), MessageTypeDefOf.PositiveEvent);
                        }
                    }
                }
            }, null, delegate
            {
                if (!foundTarget)
                {
                    LongEventHandler.QueueLongEvent(delegate { this.Transmogrify(map, pawn, thisCount - 1); },
                        "transmogrify", false, null);
                }
            }, null);
        }
    }
}