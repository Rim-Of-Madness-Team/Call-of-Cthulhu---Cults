using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;
using RimWorld;
using RimWorld.Planet;
using System.Text;

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
            if (Brood(map) != null)
            {
                List<Pawn> tempPawns = new List<Pawn>(Brood(map));
                foreach (Pawn p in tempPawns)
                {
                    if (p.Dead) Brood(map).Remove(p);
                }
                if ((Brood(map).Count + NUMTOSPAWN) > HARDLIMIT)
                {
                    Messages.Message("DefendTheBroodLimit".Translate(), MessageSound.RejectInput);
                    return false;
                }
            }
            return true;
        }

        protected override bool CanFireNowSub(IIncidentTarget target)
        {
            //Cthulhu.Utility.DebugReport("CanFire: " + this.def.defName);
            return true;
        }
        

        protected List<Pawn> SpawnPawns(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            List<Pawn> list = new List<Pawn>();

            List<PawnKindDef> listBugs = new List<PawnKindDef>();
            listBugs.Add(PawnKindDefOf.Megascarab);
            listBugs.Add(PawnKindDefOf.Spelopede);
            listBugs.Add(PawnKindDefOf.Megaspider);
            IEnumerable<PawnKindDef> source = from x in listBugs
                                              where x.combatPower <= 500f
                                              select x;
            int maxPawns = NUMTOSPAWN;
            for (int i = 0; i < maxPawns; i++)
            {
                if (Cthulhu.Utility.IsCosmicHorrorsLoaded())
                {
                    Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDef.Named("DeepOne"), parms.faction);
                    if (pawn == null) continue;
                    list.Add(pawn);
                }
                else
                {
                    PawnKindDef kindDef;
                    if (!source.TryRandomElement(out kindDef))
                    {
                        Log.Error("Unable to get pawnkind for Defend the Brood.");
                        break;
                    }
                    Pawn pawn = PawnGenerator.GeneratePawn(kindDef, parms.faction);
                    if (pawn != null) list.Add(pawn);

                }
            }
            foreach (Pawn current in list)
            {
                IntVec3 loc = CellFinder.RandomClosewalkCellNear(parms.spawnCenter, (Map)parms.target, 5);


                if (GenPlace.TryPlaceThing(current, loc, (Map)parms.target, ThingPlaceMode.Near, null))
                {
                    continue;
                }
                Find.WorldPawns.PassToWorld(current, PawnDiscardDecideMode.Discard);

                //GenSpawn.Spawn(current, loc, (Map)parms.target);
            }
            //PawnRelationUtility.Notify_PawnsSeenByPlayer(list, out "LetterRelatedPawnsNeutralGroup".Translate(), true);
            return list;
        }

        protected bool TrySetDeepOneFaction(IncidentParms parms)
        {
            if (Cthulhu.Utility.IsCosmicHorrorsLoaded())
            {
                parms.faction = Find.FactionManager.FirstFactionOfDef(FactionDef.Named("DeepOne"));
            }
            else
            {
                Messages.Message("UsingInsectoidsInstead".Translate(), MessageSound.Negative);
                parms.faction = Find.FactionManager.FirstFactionOfDef(FactionDef.Named("DeepOneAlt"));
            }
            return parms.faction != null;
        }

        
        public override bool TryExecute(IncidentParms parms)
        {
            if (!this.TrySetDeepOneFaction(parms))
            {
                return false;
            }
            Map map = parms.target as Map;
            IntVec3 intVec;
            //Find a drop spot
            if (!ShipChunkDropCellFinder.TryFindShipChunkDropCell(map.Center, map, 70, out intVec))
            {
                return false;
            }
            parms.spawnCenter = intVec;
            List<Pawn> list = this.SpawnPawns(parms);
            if (list.Count == 0)
            {
                return false;
            }
            IntVec3 chillSpot;
            RCellFinder.TryFindRandomSpotJustOutsideColony(list[0], out chillSpot);
            //LordJob_VisitColony lordJob = new LordJob_VisitColony(parms.faction, chillSpot);

            //If they have the sign of dagon, then use it.
            IntVec3 chillSpot2 = IntVec3.Invalid;
            Building dagonSign = map.listerBuildings.allBuildingsColonist.FirstOrDefault((Building bld) => bld.def.defName.Equals("SignOfDagon"));
            if (dagonSign != null) chillSpot2 = dagonSign.Position;
            if (chillSpot2 != null) chillSpot = chillSpot2;

            LordJob_DefendPoint lordJob = new LordJob_DefendPoint(chillSpot);
            Cthulhu.Utility.TemporaryGoodwill(parms.faction, false);
            LordMaker.MakeNewLord(parms.faction, lordJob, map, list);
            //Find.LetterStack.ReceiveLetter("DeepOnesArrive".Translate(), "DeepOnesArriveDesc".Translate(), LetterType.Good, list[0]);
            map.GetComponent<MapComponent_SacrificeTracker>().lastLocation = list[0].Position;
            map.GetComponent<MapComponent_SacrificeTracker>().defendTheBroodPawns.AddRange(list);


            return true;
        }
        
    }
}
