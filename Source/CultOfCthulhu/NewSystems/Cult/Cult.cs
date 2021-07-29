using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace CultOfCthulhu
{
    public class Cult : IExposable
    {
        public bool active;
        public Pawn founder;
        public Settlement foundingCity;
        public Faction foundingFaction;
        public List<CultInfluence> influences = new List<CultInfluence>();
        public Pawn leader;
        public List<Pawn> members = new List<Pawn>();
        public string name = "Unnamed Cult";
        public int numHumanSacrifices;

        public Cult()
        {
        }

        public Cult(Pawn newFounder)
        {
            InitializeCult(newFounder);
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref name, "name", "Unnamed Cult");
            Scribe_Values.Look(ref active, "active");
            Scribe_References.Look(ref founder, "founder");
            Scribe_References.Look(ref leader, "leader");
            Scribe_Collections.Look(ref members, "members", LookMode.Reference);
            Scribe_References.Look(ref foundingFaction, "foundingFaction");
            Scribe_References.Look(ref foundingCity, "foundingCity");
            Scribe_Collections.Look(ref influences, "influences", LookMode.Deep);
            Scribe_Values.Look(ref numHumanSacrifices, "numHumanSacrifices");
        }

        public List<Pawn> MembersAt(Map map)
        {
            if (!active)
            {
                return null;
            }

            var result = map.mapPawns.AllPawnsSpawned
                .Where(x => x.RaceProps != null && x.RaceProps.Humanlike && IsMember(x)).ToList();
            return result;
        }

        private bool IsMember(Pawn pawn)
        {
            if (!active || members == null || members.Count <= 0)
            {
                return false;
            }

            if (members.Contains(pawn))
            {
                return true;
            }

            return false;
        }

        private void SendCultLetterDismantled()
        {
            Find.LetterStack.ReceiveLetter("Cults_DismantledACultLabel".Translate(),
                "Cults_DismantledACultDesc".Translate(
                    name
                ), CultsDefOf.Cults_StandardMessage);
        }

        private void SendCultLetterFounded(Pawn newFounder)
        {
            Find.LetterStack.ReceiveLetter("Cults_FoundedACultLabel".Translate(), "Cults_FoundedACultDesc".Translate(
                newFounder.LabelShort
            ), CultsDefOf.Cults_StandardMessage);
            if (foundingCity != null)
            {
                Find.WindowStack.Add(new Dialog_NameCult(foundingCity.Map));
            }
        }

        private void InitializeCult(Pawn newFounder)
        {
            var map = newFounder.Map;
            founder = newFounder;
            leader = newFounder;
            foundingFaction = newFounder.Faction;
            foundingCity = Find.WorldObjects.SettlementAt(newFounder.Map.Tile);
            influences = new List<CultInfluence>();
            foreach (var set in Find.WorldObjects.Settlements)
            {
                influences.Add(set == foundingCity ? new CultInfluence(set, 1.0f) : new CultInfluence(set, 0.0f));
            }

            active = true;
            Find.World.GetComponent<WorldComponent_GlobalCultTracker>().worldCults.Add(this);

            if (foundingFaction != Faction.OfPlayerSilentFail)
            {
                return;
            }

            SendCultLetterFounded(newFounder);

            //It's a day to remember
            var taleToAdd = TaleDef.Named("FoundedCult");
            if ((newFounder.IsColonist || newFounder.HostFaction == Faction.OfPlayer) && taleToAdd != null)
            {
                TaleRecorder.RecordTale(taleToAdd, newFounder);
            }

            //The founder will remember that, too.
            newFounder.needs.mood.thoughts.memories.TryGainMemory(CultsDefOf.Cults_FoundedCult);
            map.GetComponent<MapComponent_LocalCultTracker>().ResolveTerribleCultFounder(newFounder);
        }

        private void DismantleCult()
        {
            SendCultLetterDismantled();
            if (influences != null && influences.Count > 0)
            {
                influences.Clear();
                influences = null;
            }

            active = false;
        }

        public void SetMember(Pawn cultMember)
        {
            // Is the list missing? Let's fix that.
            if (members == null)
            {
                members = new List<Pawn>();
            }

            //Does this member already exist as part of the cult?
            //If so, don't add them.
            if (members?.Count > 0)
            {
                foreach (var current in members)
                {
                    if (current == cultMember)
                    {
                        return;
                    }
                }
            }

            //Add the cultist to the list.
            members.Add(cultMember);
            //If the cult already exists, show a message to initiate the pawn into the cult.
            if (active)
            {
                Messages.Message(cultMember.LabelShort + " has been initiated into the cult, " + name,
                    MessageTypeDefOf.PositiveEvent);
            }
            //If it doesn't already exist, then let's make it so!
            else
            {
                InitializeCult(cultMember);
            }
        }

        public void RemoveMember(Pawn cultMember)
        {
            if (members == null)
            {
                return;
            }

            if (members.Count == 0)
            {
                return;
            }

            var tempList = new List<Pawn>(members);
            foreach (var current in tempList)
            {
                if (current != cultMember)
                {
                    continue;
                }

                members.Remove(cultMember);
                if (members.Count == 0)
                {
                    DismantleCult();
                }
            }
        }
    }
}