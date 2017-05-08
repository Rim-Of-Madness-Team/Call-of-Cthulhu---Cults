using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using RimWorld.Planet;

namespace CultOfCthulhu
{
    public enum CultSeedState : int
    {
        NeedSeed = 0,
        NeedSeeing = 1,
        FinishedSeeing = 2,
        NeedWriting = 3,
        FinishedWriting = 4,
        NeedTable = 5
    }

    class WorldComponent_GlobalCultTracker : WorldComponent
    {
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
            ResearchProjectDef.Named("Forbidden_Obelisks")
        };

        public string cultName = "Unnamed Cult";
        public bool doesCultExist = false;
        public List<Pawn> cultMembers = new List<Pawn>();
        public List<Pawn> antiCultists = new List<Pawn>();
        public Pawn cultFounder = null;
        public CultSeedState currentSeedState = CultSeedState.NeedSeed;
        public int numHumanSacrifices = 0;

        public WorldComponent_GlobalCultTracker(World world) : base(world)
        {
        }

        public void InitializeCult(Pawn founder)
        {
            Map map = founder.Map;
            doesCultExist = true;
            Messages.Message(founder.LabelShort + " has founded a cult.", MessageSound.Benefit);
            Find.WindowStack.Add(new Dialog_NameCult(map));

            cultFounder = founder;

            //It's a day to remember
            TaleDef taleToAdd = TaleDef.Named("FoundedCult");
            if ((founder.IsColonist || founder.HostFaction == Faction.OfPlayer) && taleToAdd != null)
            {
                TaleRecorder.RecordTale(taleToAdd, new object[]
                {
                    founder,
                });
            }
            //The founder will remember that, too.
            founder.needs.mood.thoughts.memories.TryGainMemory(CultsDefOfs.Cults_FoundedCult);
            map.GetComponent<MapComponent_LocalCultTracker>().ResolveTerribleCultFounder(founder);
        }
        
        public void DismantleCult()
        {
            doesCultExist = false;
            Messages.Message(cultName + " has been dismantled.", MessageSound.Negative);
            cultName = "Unnamed Cult";
        }


        public void SetMember(Pawn cultMember)
        {
            /// Is the list missing? Let's fix that.
            if (cultMembers == null)
            {
                cultMembers = new List<Pawn>();
            }

            //Does this member already exist as part of the cult?
            //If so, don't add them.
            foreach (Pawn current in cultMembers)
            {
                if (current == cultMember)
                {
                    return;
                }
            }

            //Add the cultist to the list.
            cultMembers.Add(cultMember);
            //If the cult already exists, show a message to initiate the pawn into the cult.
            if (doesCultExist)
            {
                Messages.Message(cultMember.LabelShort + " has been initiated into the cult, " + cultName, MessageSound.Benefit);
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
            if (cultMembers == null)
            {
                return;
            }

            if (cultMembers.Count == 0) return;
            List<Pawn> tempList = new List<Pawn>(cultMembers);
            foreach (Pawn current in tempList)
            {
                if (current == cultMember)
                {
                    cultMembers.Remove(cultMember);
                    if (cultMembers.Count == 0)
                    {
                        DismantleCult();
                    }
                }
            }
            tempList = null;
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
            foreach (Pawn current in antiCultists)
            {
                if (current == antiCultist)
                {
                    return;
                }
            }

            //Are they a prisoner? We don't want those in the list.
            if (!antiCultist.IsColonist) return;

            //Add the anti-cultist to the list.
            antiCultists.Add(antiCultist);
            //If the cult already exists, show a message to initiate the pawn into the inquisitor faction.
            if (doesCultExist)
            {
                Messages.Message(antiCultist.LabelShort + " has begun plotting against the local cult, " + cultName, MessageSound.Benefit);
            }
        }



        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<string>(ref this.cultName, "cultName", "Unnamed Cult", false);
            Scribe_Collections.Look<Pawn>(ref this.cultMembers, "cultMembers", LookMode.Reference, new object[0]);
            Scribe_Collections.Look<Pawn>(ref this.antiCultists, "antiCultists", LookMode.Reference, new object[0]);
            Scribe_Values.Look<bool>(ref this.doesCultExist, "doesCultExist", false, false);
            Scribe_Values.Look<int>(ref this.numHumanSacrifices, "numHumanSacrifices", 0, false);

            Scribe_Values.Look<CultSeedState>(ref this.currentSeedState, "CurrentSeedState", CultSeedState.NeedSeed, false);
        }
    }

}
