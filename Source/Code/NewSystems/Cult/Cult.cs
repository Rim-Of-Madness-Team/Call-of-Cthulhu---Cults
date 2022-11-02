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
            InitializeCult(newFounder: newFounder);
        }

        public void ExposeData()
        {
            Scribe_Values.Look(value: ref name, label: "name", defaultValue: "Unnamed Cult");
            Scribe_Values.Look(value: ref active, label: "active");
            Scribe_References.Look(refee: ref founder, label: "founder");
            Scribe_References.Look(refee: ref leader, label: "leader");
            Scribe_Collections.Look(list: ref members, label: "members", lookMode: LookMode.Reference);
            Scribe_References.Look(refee: ref foundingFaction, label: "foundingFaction");
            Scribe_References.Look(refee: ref foundingCity, label: "foundingCity");
            Scribe_Collections.Look(list: ref influences, label: "influences", lookMode: LookMode.Deep);
            Scribe_Values.Look(value: ref numHumanSacrifices, label: "numHumanSacrifices");
        }

        public List<Pawn> MembersAt(Map map)
        {
            if (!active)
            {
                return null;
            }

            var result = map.mapPawns.AllPawnsSpawned
                .Where(predicate: x => x.RaceProps != null && x.RaceProps.Humanlike && IsMember(pawn: x)).ToList();
            return result;
        }

        private bool IsMember(Pawn pawn)
        {
            if (!active || members == null || members.Count <= 0)
            {
                return false;
            }

            if (members.Contains(item: pawn))
            {
                return true;
            }

            return false;
        }

        private void SendCultLetterDismantled()
        {
            Find.LetterStack.ReceiveLetter(label: "Cults_DismantledACultLabel".Translate(),
                text: "Cults_DismantledACultDesc".Translate(
                    arg1: name
                ), textLetterDef: CultsDefOf.Cults_StandardMessage);
        }

        private void SendCultLetterFounded(Pawn newFounder)
        {
            Find.LetterStack.ReceiveLetter(label: "Cults_FoundedACultLabel".Translate(), text: "Cults_FoundedACultDesc".Translate(
                arg1: newFounder.LabelShort
            ), textLetterDef: CultsDefOf.Cults_StandardMessage);
            if (foundingCity != null)
            {
                Find.WindowStack.Add(window: new Dialog_NameCult(map: foundingCity.Map));
            }
        }

        private void InitializeCult(Pawn newFounder)
        {
            var map = newFounder.Map;
            founder = newFounder;
            leader = newFounder;
            foundingFaction = newFounder.Faction;
            foundingCity = Find.WorldObjects.SettlementAt(tile: newFounder.Map.Tile);
            influences = new List<CultInfluence>();
            foreach (var set in Find.WorldObjects.Settlements)
            {
                influences.Add(item: set == foundingCity ? new CultInfluence(newSettlement: set, newInfluence: 1.0f) : new CultInfluence(newSettlement: set, newInfluence: 0.0f));
            }

            active = true;
            Find.World.GetComponent<WorldComponent_GlobalCultTracker>().worldCults.Add(item: this);

            if (foundingFaction != Faction.OfPlayerSilentFail)
            {
                return;
            }

            SendCultLetterFounded(newFounder: newFounder);

            //It's a day to remember
            var taleToAdd = TaleDef.Named(str: "FoundedCult");
            if ((newFounder.IsColonist || newFounder.HostFaction == Faction.OfPlayer) && taleToAdd != null)
            {
                TaleRecorder.RecordTale(def: taleToAdd, newFounder);
            }

            //The founder will remember that, too.
            newFounder.needs.mood.thoughts.memories.TryGainMemory(def: CultsDefOf.Cults_FoundedCult);
            map.GetComponent<MapComponent_LocalCultTracker>().ResolveTerribleCultFounder(founder: newFounder);
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
            members.Add(item: cultMember);
            //If the cult already exists, show a message to initiate the pawn into the cult.
            if (active)
            {
                Messages.Message(text: cultMember.LabelShort + " has been initiated into the cult, " + name,
                    def: MessageTypeDefOf.PositiveEvent);
            }
            //If it doesn't already exist, then let's make it so!
            else
            {
                InitializeCult(newFounder: cultMember);
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

            var tempList = new List<Pawn>(collection: members);
            foreach (var current in tempList)
            {
                if (current != cultMember)
                {
                    continue;
                }

                members.Remove(item: cultMember);
                if (members.Count == 0)
                {
                    DismantleCult();
                }
            }
        }
    }
}