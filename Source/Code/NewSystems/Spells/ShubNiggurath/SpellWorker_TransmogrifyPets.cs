using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

//using RimWorld.SquadAI;  

namespace CultOfCthulhu
{
    public class SpellWorker_TransmogrifyPets : SpellWorker
    {
        public override bool CanSummonNow(Map map)
        {
            //Cthulhu.Utility.DebugReport("CanFire: " + this.def.defName);
            if (PetsToTransmogrify(map: map).Any())
            {
                return true;
            }

            Messages.Message(text: "No pets to transmogrify", def: MessageTypeDefOf.RejectInput);
            return false;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var map = parms.target as Map;
            Transmogrify(map: map);

            return true;
        }


        public IEnumerable<Pawn> PetsToTransmogrify(Map map)
        {
            //Get a pet with a master.
            var one = from Pawn pets in map.mapPawns.AllPawnsSpawned
                where !(pets?.GetComp<CompTransmogrified>()?.IsTransmogrified ?? true) && pets.RaceProps.Animal &&
                      pets.Faction == Faction.OfPlayer && !pets.Dead && !pets.Downed && pets.RaceProps.petness > 0f &&
                      pets.playerSettings.Master != null
                select pets;
            //No master? Okay, still search for pets.
            if (!one.Any())
            {
                one = from Pawn pets in map.mapPawns.AllPawnsSpawned
                    where !(pets?.GetComp<CompTransmogrified>()?.IsTransmogrified ?? true) && pets.RaceProps.Animal &&
                          pets.Faction == Faction.OfPlayer && !pets.Dead && !pets.Downed && pets.RaceProps.petness > 0f
                    select pets;
            }

            //No pets? Okay, search for player animals.
            if (!one.Any())
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
            if (count <= 0)
            {
                return;
            }
            //No pawn? Okay, find one.

            if (pawn == null)
            {
                pawn = PetsToTransmogrify(map: map).RandomElement();
            }

            Messages.Message(text: "Cults_TransmogrifyAnimalsOnly".Translate(
                arg1: pawn.LabelShort
            ), def: MessageTypeDefOf.NeutralEvent);


            var parms = new TargetingParameters
            {
                canTargetPawns = true
            };
            var foundTarget = false;
            var thisCount = count;

            Find.Targeter.BeginTargeting(targetParams: parms, action: delegate(LocalTargetInfo t)
            {
                if (t.Thing is not Pawn tP)
                {
                    return;
                }

                if (!(tP.RaceProps?.Animal ?? false))
                {
                    return;
                }

                pawn = tP;
                var compTrans = tP.GetComp<CompTransmogrified>();
                if (compTrans == null)
                {
                    return;
                }

                compTrans.IsTransmogrified = true;
                foundTarget = true;
                Messages.Message(text: "Cults_TransmogrifyMessage".Translate(
                    arg1: pawn.LabelShort
                ), def: MessageTypeDefOf.PositiveEvent);
            }, caster: null, actionWhenFinished: delegate
            {
                if (!foundTarget)
                {
                    LongEventHandler.QueueLongEvent(action: delegate { Transmogrify(map: map, pawn: pawn, count: thisCount - 1); },
                        textKey: "transmogrify", doAsynchronously: false, exceptionHandler: null);
                }
            });
        }
    }
}