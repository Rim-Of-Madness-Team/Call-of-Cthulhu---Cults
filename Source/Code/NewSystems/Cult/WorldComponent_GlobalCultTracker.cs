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
            ResearchProjectDef.Named(defName: "Forbidden_Studies"),
            ResearchProjectDef.Named(defName: "Forbidden_Deities"),
            ResearchProjectDef.Named(defName: "Forbidden_Lore"),
            ResearchProjectDef.Named(defName: "Forbidden_Sculptures"),
            ResearchProjectDef.Named(defName: "Forbidden_Attire"),
            ResearchProjectDef.Named(defName: "Forbidden_Altar"),
            ResearchProjectDef.Named(defName: "Forbidden_Sacrifice"),
            ResearchProjectDef.Named(defName: "Forbidden_Human"),
            ResearchProjectDef.Named(defName: "Forbidden_Obelisks"),
            ResearchProjectDef.Named(defName: "Forbidden_Reports")
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

        public WorldComponent_GlobalCultTracker(World world) : base(world: world)
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
                    Find.LetterStack.ReceiveLetter(label: "Cults_InitialExposureLabel".Translate(),
                        text: "Cults_InitialExposureDesc".Translate(), textLetterDef: CultsDefOf.Cults_StandardMessage, debugInfo: null);
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
                    result = worldCults.FirstOrDefault(predicate: x => x.foundingFaction == Faction.OfPlayerSilentFail);
                }

                return result;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(list: ref worldCults, label: "worldCults", lookMode: LookMode.Deep);
            Scribe_Collections.Look(list: ref antiCultists, label: "antiCultists", lookMode: LookMode.Reference);
            Scribe_Values.Look(value: ref currentSeedState, label: "CurrentSeedState");
            Scribe_Values.Look(value: ref exposedToCults, label: "exposedToCults");
            //Scribe_Collections.Look<Pawn, int[]>(ref this.cultistExperiences, "cultistExperiences", LookMode.Reference, LookMode.Value);
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                workingPawns = new List<Pawn>();
                workingInts = new List<CultistExperience>();
                if (cultistExperiences != null && cultistExperiences?.Count > 0)
                {
                    foreach (var keyPair in cultistExperiences)
                    {
                        workingPawns.Add(item: keyPair.Key);
                        workingInts.Add(item: keyPair.Value);
                    }
                }
            }

            Scribe_Collections.Look(list: ref workingPawns, label: "workingPawns", lookMode: LookMode.Reference);
            Scribe_Collections.Look(list: ref workingInts, label: "workingInts", lookMode: LookMode.Deep);
            if (Scribe.mode != LoadSaveMode.PostLoadInit)
            {
                return;
            }

            cultistExperiences = new Dictionary<Pawn, CultistExperience>();
            for (var i = 0; i < workingPawns.Count; i++)
            {
                cultistExperiences.Add(key: workingPawns[index: i], value: workingInts[index: i]);
            }
        }

        public void GainExperience(Pawn p, bool carriedOutSacrifice = false)
        {
            if (cultistExperiences == null || cultistExperiences?.Count == 0)
            {
                cultistExperiences = new Dictionary<Pawn, CultistExperience>();
            }

            if (!cultistExperiences?.ContainsKey(key: p) ?? true)
            {
                cultistExperiences.Add(key: p, value: new CultistExperience(sacrificeCount: 0, preachCount: 0));
            }

            if (!carriedOutSacrifice)
            {
                cultistExperiences[key: p].PreachCount += 1;
            }
            else
            {
                cultistExperiences[key: p].SacrificeCount += 1;
            }
        }

        public int GetExperience(Pawn p, bool sacrifice = false)
        {
            if (cultistExperiences == null)
            {
                cultistExperiences = new Dictionary<Pawn, CultistExperience>();
            }

            if (!cultistExperiences.ContainsKey(key: p))
            {
                cultistExperiences.Add(key: p, value: new CultistExperience(sacrificeCount: 0, preachCount: 0));
            }

            return !sacrifice ? cultistExperiences[key: p].PreachCount : cultistExperiences[key: p].SacrificeCount;
        }

        public Cult LocalDominantCult(Map map)
        {
            Cult result = null;
            var settlement = Find.WorldObjects.SettlementAt(tile: map.Tile);
            if (settlement == null)
            {
                return null;
            }

            if (worldCults.Count > 0)
            {
                result = worldCults.FirstOrDefault(predicate: x =>
                    x.influences.FirstOrDefault(predicate: y => y.settlement == settlement && y.dominant) != null);
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

            var tempList = new List<Pawn>(collection: antiCultists);
            foreach (var current in tempList)
            {
                if (current == inquisitor)
                {
                    antiCultists.Remove(item: inquisitor);
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
            if (Enumerable.Any(source: antiCultists, predicate: current => current == antiCultist))
            {
                return;
            }

            //Are they a prisoner? We don't want those in the list.
            if (!antiCultist.IsColonist)
            {
                return;
            }


            //Add the anti-cultist to the list.
            antiCultists.Add(item: antiCultist);
            //If the cult already exists, show a message to initiate the pawn into the inquisitor faction.
            if (PlayerCult == null || !PlayerCult.active)
            {
                return;
            }

            Messages.Message(
                text: "Cults_InquisitionPlotBegins".Translate(arg1: antiCultist.LabelShort, arg2: PlayerCult.name),
                def: MessageTypeDefOf.PositiveEvent);
        }
    }
}