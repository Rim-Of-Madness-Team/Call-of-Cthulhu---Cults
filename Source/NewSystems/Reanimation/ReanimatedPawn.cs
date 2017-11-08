using RimWorld;
using System;
using UnityEngine;
using Verse.AI;
using Verse;

namespace CultOfCthulhu
{
    /// <summary>
    /// Originally ZombePawn from JustinC
    /// </summary>
    public class ReanimatedPawn : Pawn
    {
        public bool setZombie = false;
    
        public bool isRaiding = true;

        public bool wasColonist;

        public float notRaidingAttackRange = 15f;

        public ReanimatedPawn()
        {
            this.Init();
        }
        

        private void Init()
        {
            this.pather = new Pawn_PathFollower(this);
            this.stances = new Pawn_StanceTracker(this);
            this.health = new Pawn_HealthTracker(this);
            this.jobs = new Pawn_JobTracker(this);
            this.filth = new Pawn_FilthTracker(this);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.wasColonist, "wasColonist", false, false);
            //if (Scribe.mode == LoadSaveMode.LoadingVars)
            //{
            //    Cthulhu.Utility.GiveZombieSkinEffect(this);
            //}
        }

        public override void PreApplyDamage(DamageInfo dinfo, out bool absorbed)
        {
            this.health.PreApplyDamage(dinfo, out absorbed);
            if (!base.Destroyed && (dinfo.Def == DamageDefOf.Cut || dinfo.Def == DamageDefOf.Stab))
            {
                float num = 0f;
                float num2 = 0f;
                if (dinfo.Instigator != null && dinfo.Instigator is Pawn)
                {
                    Pawn pawn = dinfo.Instigator as Pawn;
                    if (pawn.skills != null)
                    {
                        SkillRecord expr_9B = pawn.skills.GetSkill(SkillDefOf.Melee);
                        num = (float)(expr_9B.Level * 2);
                        num2 = (float)expr_9B.Level / 20f * 3f;
                    }
                    if (UnityEngine.Random.Range(0f, 100f) < 20f + num)
                    {
                        dinfo.SetAmount(999);
                        dinfo.SetHitPart(this.health.hediffSet.GetBrain());
                        dinfo.Def.Worker.Apply(dinfo, this);
                        return;
                    }
                    dinfo.SetAmount((int)((float)dinfo.Amount * (1f + num2)));
                }
            }
        }

        public override void Tick()
        {
            try
            {
                if (DebugSettings.noAnimals && base.RaceProps.Animal)
                {
                    this.Destroy(0);
                }
                else if (!base.Downed)
                {
                    if (Find.TickManager.TicksGame % 250 == 0)
                    {
                        this.TickRare();
                    }
                    if (base.Spawned)
                    {
                        this.pather.PatherTick();
                    }
                    base.Drawer.DrawTrackerTick();
                    this.health.HealthTick();
                    this.records.RecordsTick();
                    if (base.Spawned)
                    {
                        this.stances.StanceTrackerTick();
                    }
                    if (base.Spawned)
                    {
                        this.verbTracker.VerbsTick();
                    }
                    if (base.Spawned)
                    {
                        this.natives.NativeVerbsTick();
                    }
                    if (this.equipment != null)
                    {
                        this.equipment.EquipmentTrackerTick();
                    }
                    if (this.apparel != null)
                    {
                        this.apparel.ApparelTrackerTick();
                    }
                    if (base.Spawned)
                    {
                        this.jobs.JobTrackerTick();
                    }
                    if (!base.Dead)
                    {
                        this.carryTracker.CarryHandsTick();
                    }
                    if (this.skills != null)
                    {
                        this.skills.SkillsTick();
                    }
                    if (this.inventory != null)
                    {
                        this.inventory.InventoryTrackerTick();
                    }
                }
                if (this.needs != null && this.needs.food != null && this.needs.food.CurLevel <= 0.95f)
                {
                    this.needs.food.CurLevel = 1f;
                }
                if (this.needs != null && this.needs.joy != null && this.needs.joy.CurLevel <= 0.95f)
                {
                    this.needs.joy.CurLevel = 1f;
                }
                if (this.needs != null && this.needs.beauty != null && this.needs.beauty.CurLevel <= 0.95f)
                {
                    this.needs.beauty.CurLevel = 1f;
                }
                if (this.needs != null && this.needs.comfort != null && this.needs.comfort.CurLevel <= 0.95f)
                {
                    this.needs.comfort.CurLevel = 1f;
                }
                if (this.needs != null && this.needs.rest != null && this.needs.rest.CurLevel <= 0.95f)
                {
                    this.needs.rest.CurLevel = 1f;
                }
                if (this.needs != null && this.needs.mood != null && this.needs.mood.CurLevel <= 0.45f)
                {
                    this.needs.mood.CurLevel = 0.5f;
                }
                if (!this.setZombie)
                {
                    this.mindState.mentalStateHandler.neverFleeIndividual = true;
                    this.setZombie = ReanimatedPawnUtility.Zombify(this);
                    //ZombieMod_Utility.SetZombieName(this);
                }
                if (base.Downed || this.health.Downed || this.health.InPainShock)
                {
                    DamageInfo damageInfo = new DamageInfo(DamageDefOf.Blunt, 9999, -1f, this, null, null);
                    damageInfo.SetHitPart(this.health.hediffSet.GetBrain());
                    //damageInfo.SetPart(new BodyPartDamageInfo(this.health.hediffSet.GetBrain(), false, HediffDefOf.Cut));
                    base.TakeDamage(damageInfo);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
