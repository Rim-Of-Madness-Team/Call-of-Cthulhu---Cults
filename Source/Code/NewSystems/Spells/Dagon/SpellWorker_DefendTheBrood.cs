using System.Collections.Generic;
using System.Linq;
using Cthulhu;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace CultOfCthulhu
{
    public class SpellWorker_DefendTheBrood : SpellWorker
    {
        private const int NUMTOSPAWN = 3;
        private const int HARDLIMIT = 8;

        protected List<Pawn> Brood(Map map)
        {
            return map.GetComponent<MapComponent_SacrificeTracker>().defendTheBroodPawns;
        }

        public override bool CanSummonNow(Map map)
        {
            if (Brood(map: map) == null)
            {
                return true;
            }

            var tempPawns = new List<Pawn>(collection: Brood(map: map));
            foreach (var p in tempPawns)
            {
                if (p.Dead)
                {
                    Brood(map: map).Remove(item: p);
                }
            }

            if (Brood(map: map).Count + NUMTOSPAWN <= HARDLIMIT)
            {
                return true;
            }

            Messages.Message(text: "DefendTheBroodLimit".Translate(), def: MessageTypeDefOf.RejectInput);
            return false;
        }

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            //Cthulhu.Utility.DebugReport("CanFire: " + this.def.defName);
            return true;
        }


        protected List<Pawn> SpawnPawns(IncidentParms parms)
        {
            var unused = (Map) parms.target;
            var list = new List<Pawn>();

            var listBugs = new List<PawnKindDef>
            {
                PawnKindDefOf.Megascarab,
                PawnKindDefOf.Spelopede,
                PawnKindDefOf.Megaspider
            };
            var source = from x in listBugs
                where x.combatPower <= 500f
                select x;
            var maxPawns = NUMTOSPAWN;
            for (var i = 0; i < maxPawns; i++)
            {
                if (Utility.IsCosmicHorrorsLoaded())
                {
                    var pawn = PawnGenerator.GeneratePawn(kindDef: PawnKindDef.Named(defName: "ROM_DeepOne"), faction: parms.faction);
                    if (pawn == null)
                    {
                        continue;
                    }

                    list.Add(item: pawn);
                }
                else
                {
                    if (!source.TryRandomElement(result: out var kindDef))
                    {
                        Log.Error(text: "Unable to get pawnkind for Defend the Brood.");
                        break;
                    }

                    var pawn = PawnGenerator.GeneratePawn(kindDef: kindDef, faction: parms.faction);
                    if (pawn != null)
                    {
                        list.Add(item: pawn);
                    }
                }
            }

            foreach (var current in list)
            {
                var loc = CellFinder.RandomClosewalkCellNear(root: parms.spawnCenter, map: (Map) parms.target, radius: 5);


                if (GenPlace.TryPlaceThing(thing: current, center: loc, map: (Map) parms.target, mode: ThingPlaceMode.Near))
                {
                    continue;
                }

                Find.WorldPawns.PassToWorld(pawn: current, discardMode: PawnDiscardDecideMode.Discard);

                //GenSpawn.Spawn(current, loc, (Map)parms.target);
            }

            //PawnRelationUtility.Notify_PawnsSeenByPlayer(list, out "LetterRelatedPawnsNeutralGroup".Translate(), true);
            return list;
        }

        protected bool TrySetDeepOneFaction(IncidentParms parms)
        {
            if (Utility.IsCosmicHorrorsLoaded())
            {
                parms.faction = Find.FactionManager.FirstFactionOfDef(facDef: FactionDef.Named(defName: "ROM_DeepOne"));
            }
            else
            {
                Messages.Message(text: "UsingInsectoidsInstead".Translate(), def: MessageTypeDefOf.NegativeEvent);
                parms.faction = Find.FactionManager.FirstFactionOfDef(facDef: FactionDef.Named(defName: "ROM_DeepOneAlt"));
            }

            return parms.faction != null;
        }


        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            //Log.Message("Building_LandedShip TrySetDeepOneFaction");
            if (!TrySetDeepOneFaction(parms: parms))
            {
                return false;
            }

            if (!(parms.target is Map map))
            {
                return false;
            }

            //Find a drop spot
            if (!CultUtility.TryFindDropCell(nearLoc: map.Center, map: map, maxDist: 70, pos: out var intVec))
            {
                return false;
            }

            parms.spawnCenter = intVec;
            var list = SpawnPawns(parms: parms);
            if (list.Count == 0)
            {
                return false;
            }

            var unused = RCellFinder.TryFindRandomSpotJustOutsideColony(searcher: list[index: 0], result: out var chillSpot);
            //LordJob_VisitColony lordJob = new LordJob_VisitColony(parms.faction, chillSpot);

            //If they have the sign of dagon, then use it.
            var chillSpot2 = IntVec3.Invalid;
            var dagonSign =
                map.listerBuildings.allBuildingsColonist.FirstOrDefault(predicate: bld => bld.def == CultsDefOf.Cults_SignOfDagon);
            if (dagonSign != null)
            {
                chillSpot2 = dagonSign.Position;
            }

            chillSpot = chillSpot2;

            //Log.Message("SpellWorker_DefendTheBrood LordJob_DefendPoint");
            var lordJob = new LordJob_DefendPoint(point: chillSpot);
            Utility.TemporaryGoodwill(faction: parms.faction);
            LordMaker.MakeNewLord(faction: parms.faction, lordJob: lordJob, map: map, startingPawns: list);
            //Find.LetterStack.ReceiveLetter("DeepOnesArrive".Translate(), "DeepOnesArriveDesc".Translate(), letterDef.Good, list[0]);
            map.GetComponent<MapComponent_SacrificeTracker>().lastLocation = list[index: 0].Position;
            map.GetComponent<MapComponent_SacrificeTracker>().defendTheBroodPawns.AddRange(collection: list);


            return true;
        }
    }
}