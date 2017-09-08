using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace CultOfCthulhu
{
    public class Cult : IExposable
    {
        public string name = "Unnamed Cult";
        public bool active = false;
        public Pawn founder = null;
        public Pawn leader = null;
        public List<Pawn> members = new List<Pawn>();
        public Settlement foundingCity = null;
        public Faction foundingFaction = null;
        public List<CultInfluence> influences = new List<CultInfluence>();
        public int numHumanSacrifices = 0;
        public List<Pawn> MembersAt(Map map)
        {
            if (!active) return null;
            List<Pawn> result = map.mapPawns.AllPawnsSpawned.Where((Pawn x) => x.RaceProps != null && x.RaceProps.Humanlike && this.IsMember(x)).ToList<Pawn>();
            return result;
        }
        public bool IsMember(Pawn pawn)
        {
            if (active && members != null && members.Count > 0)
            {
                if (members.Contains(pawn)) return true;
            }
            return false;
        }

        #region Letters
        public void SendCultLetterDismantled()
        {
            Find.LetterStack.ReceiveLetter("Cults_DismantledACultLabel".Translate(), "Cults_DismantledACultDesc".Translate(new object[]
            {
                name
            }), CultsDefOf.Cults_StandardMessage);
        }

        public void SendCultLetterFounded(Pawn newFounder)
        {
            Find.LetterStack.ReceiveLetter("Cults_FoundedACultLabel".Translate(), "Cults_FoundedACultDesc".Translate(new object[]
            {
                newFounder.LabelShort
            }), CultsDefOf.Cults_StandardMessage);
            if (foundingCity != null)
            {
                Find.WindowStack.Add(new Dialog_NameCult(foundingCity.Map));
            }   
        }
        #endregion Letters

        #region Basics

        public Cult()
        {

        }

        public Cult(Pawn newFounder)
        {
            InitializeCult(newFounder);
        }

        public void InitializeCult(Pawn newFounder)
        {
            Map map = newFounder.Map;
            founder = newFounder;
            leader = newFounder;
            foundingFaction = newFounder.Faction;
            foundingCity = Find.WorldObjects.SettlementAt(newFounder.Map.Tile);
            influences = new List<CultInfluence>();
            foreach (Settlement set in Find.WorldObjects.Settlements)
            {
                if (set == foundingCity) influences.Add(new CultInfluence(set, 1.0f));
                else
                {
                    influences.Add(new CultInfluence(set, 0.0f));
                }
            }
            active = true;
            Find.World.GetComponent<WorldComponent_GlobalCultTracker>().worldCults.Add(this);

            if (foundingFaction == Faction.OfPlayerSilentFail)
            {

                SendCultLetterFounded(newFounder);

                //It's a day to remember
                TaleDef taleToAdd = TaleDef.Named("FoundedCult");
                if ((newFounder.IsColonist || newFounder.HostFaction == Faction.OfPlayer) && taleToAdd != null)
                {
                    TaleRecorder.RecordTale(taleToAdd, new object[]
                    {
                    newFounder,
                    });
                }
                //The founder will remember that, too.
                newFounder.needs.mood.thoughts.memories.TryGainMemory(CultsDefOf.Cults_FoundedCult);
                map.GetComponent<MapComponent_LocalCultTracker>().ResolveTerribleCultFounder(newFounder);
            }
        }

        public void DismantleCult()
        {
            SendCultLetterDismantled();
            if (influences != null && influences.Count > 0)
            {
                influences.Clear();
                influences = null;
            }
            active = false;
        }

        #endregion Basics

        #region Members

        public void SetMember(Pawn cultMember)
        {
            /// Is the list missing? Let's fix that.
            if (members == null)
            {
                members = new List<Pawn>();
            }

            //Does this member already exist as part of the cult?
            //If so, don't add them.
            foreach (Pawn current in members)
            {
                if (current == cultMember)
                {
                    return;
                }
            }

            //Add the cultist to the list.
            members.Add(cultMember);
            //If the cult already exists, show a message to initiate the pawn into the cult.
            if (active)
            {
                Messages.Message(cultMember.LabelShort + " has been initiated into the cult, " + name, MessageSound.Benefit);
                return;
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

            if (members.Count == 0) return;
            List<Pawn> tempList = new List<Pawn>(members);
            foreach (Pawn current in tempList)
            {
                if (current == cultMember)
                {
                    members.Remove(cultMember);
                    if (members.Count == 0)
                    {
                        DismantleCult();
                    }
                }
            }
            tempList = null;
        }

        #endregion Members

        public void ExposeData()
        {
            Scribe_Values.Look<string>(ref this.name, "name", "Unnamed Cult");
            Scribe_Values.Look<bool>(ref this.active, "active", false);
            Scribe_References.Look<Pawn>(ref this.founder, "founder");
            Scribe_References.Look<Pawn>(ref this.leader, "leader");
            Scribe_Collections.Look<Pawn>(ref this.members, "members", LookMode.Reference, new object[0]);
            Scribe_References.Look<Faction>(ref this.foundingFaction, "foundingFaction");
            Scribe_References.Look<Settlement>(ref this.foundingCity, "foundingCity");
            Scribe_Collections.Look<CultInfluence>(ref this.influences, "influences", LookMode.Deep, new object[0]);
            Scribe_Values.Look<int>(ref this.numHumanSacrifices, "numHumanSacrifices", 0);
        }
    }
}
