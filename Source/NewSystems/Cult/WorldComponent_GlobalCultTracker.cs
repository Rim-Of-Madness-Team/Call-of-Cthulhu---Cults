using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using RimWorld.Planet;

namespace CultOfCthulhu
{
    public static class CultTracker
    {
        public static WorldComponent_GlobalCultTracker Get
        {
            get
            {
                return Find.World.GetComponent<WorldComponent_GlobalCultTracker>();
            }
        }
    }

    public enum CultSeedState : int
    {
        NeedSeed = 0,
        NeedSeeing = 1,
        FinishedSeeing = 2,
        NeedWriting = 3,
        FinishedWriting = 4,
        NeedTable = 5
    }

    public class WorldComponent_GlobalCultTracker : WorldComponent
    {
        public bool needPreacher = false;
        public bool doingInquisition = false;
        public bool exposedToCults = false;
        public bool ExposedToCults
        {
            get => exposedToCults;
            set
            {
                if (value != exposedToCults && value == true)
                    Find.LetterStack.ReceiveLetter("Cults_InitialExposureLabel".Translate(),
                        "Cults_InitialExposureDesc".Translate(), CultsDefOf.Cults_StandardMessage, null);
                exposedToCults = value;
            }
        }


        #region stuff
        public List<ResearchProjectDef> cultResearch = new List<ResearchProjectDef>()
        {
            ResearchProjectDef.Named("Forbidden_Studies"),
            ResearchProjectDef.Named("Forbidden_Deities"),
            ResearchProjectDef.Named("Forbidden_Lore"),
            ResearchProjectDef.Named("Forbidden_Sculptures"),
            ResearchProjectDef.Named("Forbidden_Attire"),
            ResearchProjectDef.Named("Forbidden_Altar"),
            ResearchProjectDef.Named("Forbidden_Sacrifice"),
            ResearchProjectDef.Named("Forbidden_Human"),
            ResearchProjectDef.Named("Forbidden_Obelisks"),
            ResearchProjectDef.Named("Forbidden_Reports")
        };

        public List<Cult> worldCults = null;
        public List<Pawn> antiCultists = new List<Pawn>();
        public CultSeedState currentSeedState = CultSeedState.NeedSeed;
        public Dictionary<Pawn, CultistExperience> cultistExperiences = new Dictionary<Pawn, CultistExperience>();
        public int numHumanSacrifices = 0;

        public Cult PlayerCult
        {
            get
            {
                Cult result = null;
                if (worldCults != null && worldCults.Count > 0)
                    result = worldCults.FirstOrDefault((Cult x) => x.foundingFaction == Faction.OfPlayerSilentFail);
                return result;
            }
        }

        public void GainExperience(Pawn p, bool carriedOutSacrifice = false)
        {
            if (cultistExperiences == null || cultistExperiences?.Count == 0)
                cultistExperiences = new Dictionary<Pawn, CultistExperience>();
            
            if (!cultistExperiences?.ContainsKey(p) ?? true)
                cultistExperiences.Add(p, new CultistExperience(0,0));

            if (!carriedOutSacrifice)
                cultistExperiences[p].PreachCount += 1;
            else
                cultistExperiences[p].SacrificeCount += 1;
        }

        public int GetExperience(Pawn p, bool sacrifice = false)
        {
            if (cultistExperiences == null)
                cultistExperiences = new Dictionary<Pawn, CultistExperience>();
            if (!cultistExperiences.ContainsKey(p))
                cultistExperiences.Add(p, new CultistExperience(0,0));
            return !sacrifice ? cultistExperiences[p].PreachCount : cultistExperiences[p].SacrificeCount;
        }

        public Cult LocalDominantCult(Map map)
        {
            Cult result = null;
            Settlement settlement = Find.WorldObjects.SettlementAt(map.Tile);
            if (settlement != null)
            {
                if (worldCults.Count > 0)
                {
                    result = worldCults.FirstOrDefault((Cult x) => x.influences.FirstOrDefault((CultInfluence y) => y.settlement == settlement && y.dominant) != null);
                }
            }
            return result;
        }

        public WorldComponent_GlobalCultTracker(World world) : base(world)
        {
            worldCults = new List<Cult>();
        }

        public void RemoveInquisitor(Pawn inquisitor)
        {
            if (antiCultists == null)
            {
                return;
            }
            if (antiCultists.Count == 0) return;
            List<Pawn> tempList = new List<Pawn>(antiCultists);
            foreach (Pawn current in tempList)
            {
                if (current == inquisitor)
                {
                    antiCultists.Remove(inquisitor);
                }
            }
        }

        public void SetInquisitor(Pawn antiCultist)
        {
            /// Is the list missing? Let's fix that.
            if (antiCultists == null)
            {
                antiCultists = new List<Pawn>();
            }

            //Does this member already exist as part of the anti cultists?
            //If so, don't add them.
            if (Enumerable.Any(antiCultists, current => current == antiCultist))
                return;

            //Are they a prisoner? We don't want those in the list.
            if (!antiCultist.IsColonist) return;

            //Add the anti-cultist to the list.
            antiCultists.Add(antiCultist);
            //If the cult already exists, show a message to initiate the pawn into the inquisitor faction.
            if (PlayerCult == null || !PlayerCult.active) return;
            Messages.Message(
                "Cults_InquisitionPlotBegins".Translate(antiCultist.LabelShort, PlayerCult.name),
                MessageTypeDefOf.PositiveEvent);
        }
        #endregion stuff

        private List<Pawn> workingPawns = new List<Pawn>();
        private List<CultistExperience> workingInts = new List<CultistExperience>();
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<Cult>(ref this.worldCults, "worldCults", LookMode.Deep, new object[0]);
            Scribe_Collections.Look<Pawn>(ref this.antiCultists, "antiCultists", LookMode.Reference, new object[0]);
            Scribe_Values.Look<CultSeedState>(ref this.currentSeedState, "CurrentSeedState", CultSeedState.NeedSeed, false);
            Scribe_Values.Look<bool>(ref this.exposedToCults, "exposedToCults", false);
            //Scribe_Collections.Look<Pawn, int[]>(ref this.cultistExperiences, "cultistExperiences", LookMode.Reference, LookMode.Value);
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                workingPawns = new List<Pawn>();
                workingInts = new List<CultistExperience>();
                if (cultistExperiences != null && cultistExperiences?.Count > 0)
                    foreach (var keyPair in cultistExperiences)
                    {
                        workingPawns.Add(keyPair.Key);
                        workingInts.Add(keyPair.Value);
                    }
            }
            Scribe_Collections.Look<Pawn>(ref this.workingPawns, "workingPawns", LookMode.Reference);
            Scribe_Collections.Look<CultistExperience>(ref this.workingInts, "workingInts", LookMode.Deep);
            if (Scribe.mode != LoadSaveMode.PostLoadInit) return;
            cultistExperiences = new Dictionary<Pawn, CultistExperience>();
            for (var i = 0; i < workingPawns.Count; i++)
            {
                cultistExperiences.Add(workingPawns[i], workingInts[i]);
            }
        }
    }

}
