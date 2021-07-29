using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace CultOfCthulhu
{
    public static class CultTracker
    {
        public static WorldComponent_GlobalCultTracker Get =>
            Find.World.GetComponent<WorldComponent_GlobalCultTracker>();
    }

    public enum CultSeedState
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
        public readonly List<ResearchProjectDef> cultResearch = new List<ResearchProjectDef>
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

        public List<Pawn> antiCultists = new List<Pawn>();
        public Dictionary<Pawn, CultistExperience> cultistExperiences = new Dictionary<Pawn, CultistExperience>();
        public CultSeedState currentSeedState = CultSeedState.NeedSeed;
        public bool doingInquisition = false;
        public bool exposedToCults;
        public bool needPreacher = false;
        public int numHumanSacrifices = 0;
        private List<CultistExperience> workingInts = new List<CultistExperience>();

        private List<Pawn> workingPawns = new List<Pawn>();

        public List<Cult> worldCults;

        public WorldComponent_GlobalCultTracker(World world) : base(world)
        {
            worldCults = new List<Cult>();
        }

        public bool ExposedToCults
        {
            get => exposedToCults;
            set
            {
                if (value != exposedToCults && value)
                {
                    Find.LetterStack.ReceiveLetter("Cults_InitialExposureLabel".Translate(),
                        "Cults_InitialExposureDesc".Translate(), CultsDefOf.Cults_StandardMessage, null);
                }

                exposedToCults = value;
            }
        }

        public Cult PlayerCult
        {
            get
            {
                Cult result = null;
                if (worldCults != null && worldCults.Count > 0)
                {
                    result = worldCults.FirstOrDefault(x => x.foundingFaction == Faction.OfPlayerSilentFail);
                }

                return result;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref worldCults, "worldCults", LookMode.Deep);
            Scribe_Collections.Look(ref antiCultists, "antiCultists", LookMode.Reference);
            Scribe_Values.Look(ref currentSeedState, "CurrentSeedState");
            Scribe_Values.Look(ref exposedToCults, "exposedToCults");
            //Scribe_Collections.Look<Pawn, int[]>(ref this.cultistExperiences, "cultistExperiences", LookMode.Reference, LookMode.Value);
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                workingPawns = new List<Pawn>();
                workingInts = new List<CultistExperience>();
                if (cultistExperiences != null && cultistExperiences?.Count > 0)
                {
                    foreach (var keyPair in cultistExperiences)
                    {
                        workingPawns.Add(keyPair.Key);
                        workingInts.Add(keyPair.Value);
                    }
                }
            }

            Scribe_Collections.Look(ref workingPawns, "workingPawns", LookMode.Reference);
            Scribe_Collections.Look(ref workingInts, "workingInts", LookMode.Deep);
            if (Scribe.mode != LoadSaveMode.PostLoadInit)
            {
                return;
            }

            cultistExperiences = new Dictionary<Pawn, CultistExperience>();
            for (var i = 0; i < workingPawns.Count; i++)
            {
                cultistExperiences.Add(workingPawns[i], workingInts[i]);
            }
        }

        public void GainExperience(Pawn p, bool carriedOutSacrifice = false)
        {
            if (cultistExperiences == null || cultistExperiences?.Count == 0)
            {
                cultistExperiences = new Dictionary<Pawn, CultistExperience>();
            }

            if (!cultistExperiences?.ContainsKey(p) ?? true)
            {
                cultistExperiences.Add(p, new CultistExperience(0, 0));
            }

            if (!carriedOutSacrifice)
            {
                cultistExperiences[p].PreachCount += 1;
            }
            else
            {
                cultistExperiences[p].SacrificeCount += 1;
            }
        }

        public int GetExperience(Pawn p, bool sacrifice = false)
        {
            if (cultistExperiences == null)
            {
                cultistExperiences = new Dictionary<Pawn, CultistExperience>();
            }

            if (!cultistExperiences.ContainsKey(p))
            {
                cultistExperiences.Add(p, new CultistExperience(0, 0));
            }

            return !sacrifice ? cultistExperiences[p].PreachCount : cultistExperiences[p].SacrificeCount;
        }

        public Cult LocalDominantCult(Map map)
        {
            Cult result = null;
            var settlement = Find.WorldObjects.SettlementAt(map.Tile);
            if (settlement == null)
            {
                return null;
            }

            if (worldCults.Count > 0)
            {
                result = worldCults.FirstOrDefault(x =>
                    x.influences.FirstOrDefault(y => y.settlement == settlement && y.dominant) != null);
            }

            return result;
        }

        public void RemoveInquisitor(Pawn inquisitor)
        {
            if (antiCultists == null)
            {
                return;
            }

            if (antiCultists.Count == 0)
            {
                return;
            }

            var tempList = new List<Pawn>(antiCultists);
            foreach (var current in tempList)
            {
                if (current == inquisitor)
                {
                    antiCultists.Remove(inquisitor);
                }
            }
        }

        public void SetInquisitor(Pawn antiCultist)
        {
            // Is the list missing? Let's fix that.
            if (antiCultists == null)
            {
                antiCultists = new List<Pawn>();
            }

            //Does this member already exist as part of the anti cultists?
            //If so, don't add them.
            if (Enumerable.Any(antiCultists, current => current == antiCultist))
            {
                return;
            }

            //Are they a prisoner? We don't want those in the list.
            if (!antiCultist.IsColonist)
            {
                return;
            }


            //Add the anti-cultist to the list.
            antiCultists.Add(antiCultist);
            //If the cult already exists, show a message to initiate the pawn into the inquisitor faction.
            if (PlayerCult == null || !PlayerCult.active)
            {
                return;
            }

            Messages.Message(
                "Cults_InquisitionPlotBegins".Translate(antiCultist.LabelShort, PlayerCult.name),
                MessageTypeDefOf.PositiveEvent);
        }
    }
}