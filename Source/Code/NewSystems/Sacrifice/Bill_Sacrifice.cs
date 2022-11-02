using System.Collections.Generic;
using RimWorld;
using Verse;

namespace CultOfCthulhu
{
    public class Bill_Sacrifice : IExposable
    {
        private List<Pawn> congregation;
        private CosmicEntity entity;
        private Pawn executioner;
        private Pawn sacrifice;
        private IncidentDef spell;

        public Bill_Sacrifice()
        {
        }

        public Bill_Sacrifice(Pawn newSacrifice, Pawn newExecutioner, CosmicEntity newEntity, IncidentDef newSpell)
        {
            sacrifice = newSacrifice;
            executioner = newExecutioner;
            entity = newEntity;
            spell = newSpell;
        }

        public Pawn Sacrifice => sacrifice;
        public Pawn Executioner => executioner;

        public List<Pawn> Congregation
        {
            get => congregation;
            set => congregation = value;
        }

        public CosmicEntity Entity => entity;
        public IncidentDef Spell => spell;

        public CultUtility.SacrificeType Type => Sacrifice?.RaceProps?.Animal ?? false
            ? CultUtility.SacrificeType.animal
            : CultUtility.SacrificeType.human;

        public void ExposeData()
        {
            Scribe_References.Look(refee: ref sacrifice, label: "sacrifice");
            Scribe_References.Look(refee: ref executioner, label: "executioner");
            Scribe_Collections.Look(list: ref congregation, label: "congregation", lookMode: LookMode.Reference);
            Scribe_References.Look(refee: ref entity, label: "entity");
            Scribe_Defs.Look(value: ref spell, label: "spell");
        }
    }
}