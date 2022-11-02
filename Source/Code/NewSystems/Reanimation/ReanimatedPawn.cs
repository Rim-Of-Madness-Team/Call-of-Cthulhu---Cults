using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CultOfCthulhu
{
    /// <summary>
    ///     Originally ZombePawn from JustinC
    /// </summary>
    public class ReanimatedPawn : Pawn
    {
        public bool isRaiding = true;

        public float notRaidingAttackRange = 15f;
        public bool setZombie;

        public bool wasColonist;

        public ReanimatedPawn()
        {
            Init();
        }


        private void Init()
        {
            pather = new Pawn_PathFollower(newPawn: this);
            stances = new Pawn_StanceTracker(newPawn: this);
            health = new Pawn_HealthTracker(pawn: this);
            jobs = new Pawn_JobTracker(newPawn: this);
            filth = new Pawn_FilthTracker(pawn: this);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(value: ref wasColonist, label: "wasColonist");
            //if (Scribe.mode == LoadSaveMode.LoadingVars)
            //{
            //    Cthulhu.Utility.GiveZombieSkinEffect(this);
            //}
        }

        public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            health.PreApplyDamage(dinfo: dinfo, absorbed: out absorbed);
            if (Destroyed || dinfo.Def != DamageDefOf.Cut && dinfo.Def != DamageDefOf.Stab)
            {
                return;
            }

            var num = 0f;
            var num2 = 0f;
            if (dinfo.Instigator is not Pawn)
            {
                return;
            }

            var pawn = dinfo.Instigator as Pawn;
            if (pawn?.skills != null)
            {
                var expr_9B = pawn.skills.GetSkill(skillDef: SkillDefOf.Melee);
                num = expr_9B.Level * 2;
                num2 = expr_9B.Level / 20f * 3f;
            }

            if (Random.Range(min: 0f, max: 100f) < 20f + num)
            {
                dinfo.SetAmount(newAmount: 999);
                dinfo.SetHitPart(forceHitPart: health.hediffSet.GetBrain());
                dinfo.Def.Worker.Apply(dinfo: dinfo, victim: this);
                return;
            }

            dinfo.SetAmount(newAmount: (int) (dinfo.Amount * (1f + num2)));
        }

        public override void Tick()
        {
            try
            {
                if (DebugSettings.noAnimals && RaceProps.Animal)
                {
                    Destroy();
                }
                else if (!Downed)
                {
                    if (Find.TickManager.TicksGame % 250 == 0)
                    {
                        TickRare();
                    }

                    if (Spawned)
                    {
                        pather.PatherTick();
                    }

                    //RW 1.3 was Drawer.DrawTrackerTick();
                    //RW 1.4
                    Drawer.ProcessPostTickVisuals(250);
                    
                    health.HealthTick();
                    records.RecordsTick();
                    if (Spawned)
                    {
                        stances.StanceTrackerTick();
                    }

                    if (Spawned)
                    {
                        verbTracker.VerbsTick();
                    }

                    if (Spawned)
                    {
                        natives.NativeVerbsTick();
                    }

                    equipment?.EquipmentTrackerTick();

                    apparel?.ApparelTrackerTick();

                    if (Spawned)
                    {
                        jobs.JobTrackerTick();
                    }

                    if (!Dead)
                    {
                        carryTracker.CarryHandsTick();
                    }

                    skills?.SkillsTick();

                    inventory?.InventoryTrackerTick();
                }

                if (needs?.food != null && needs.food.CurLevel <= 0.95f)
                {
                    needs.food.CurLevel = 1f;
                }

                if (needs?.joy != null && needs.joy.CurLevel <= 0.95f)
                {
                    needs.joy.CurLevel = 1f;
                }

                if (needs?.beauty != null && needs.beauty.CurLevel <= 0.95f)
                {
                    needs.beauty.CurLevel = 1f;
                }

                if (needs?.comfort != null && needs.comfort.CurLevel <= 0.95f)
                {
                    needs.comfort.CurLevel = 1f;
                }

                if (needs?.rest != null && needs.rest.CurLevel <= 0.95f)
                {
                    needs.rest.CurLevel = 1f;
                }

                if (needs?.mood != null && needs.mood.CurLevel <= 0.45f)
                {
                    needs.mood.CurLevel = 0.5f;
                }

                if (!setZombie)
                {
                    mindState.mentalStateHandler.neverFleeIndividual = true;
                    setZombie = ReanimatedPawnUtility.Zombify(pawn: this);
                    //ZombieMod_Utility.SetZombieName(this);
                }

                if (!Downed && !health.Downed && !health.InPainShock)
                {
                    return;
                }

                var damageInfo = new DamageInfo(def: DamageDefOf.Blunt, amount: 9999, armorPenetration: 1f, angle: -1f, instigator: this);
                damageInfo.SetHitPart(forceHitPart: health.hediffSet.GetBrain());
                //damageInfo.SetPart(new BodyPartDamageInfo(this.health.hediffSet.GetBrain(), false, HediffDefOf.Cut));
                TakeDamage(dinfo: damageInfo);
            }
            catch
            {
                // ignored
            }
        }
    }
}