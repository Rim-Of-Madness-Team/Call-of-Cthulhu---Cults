// ----------------------------------------------------------------------
// These are basic usings. Always let them be here.
// ----------------------------------------------------------------------

using System;
using System.Reflection;
using Cthulhu;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

// ----------------------------------------------------------------------
// These are RimWorld-specific usings. Activate/Deactivate what you need:
// ----------------------------------------------------------------------
// Always needed
//using VerseBase;         // Material/Graphics handling functions are found here
// RimWorld universal objects are here (like 'Building')
// Needed when you do something with the AI
// Needed when you do something with Sound
// Needed when you do something with Noises
// RimWorld specific functions are found here (like 'Building_Battery')

// RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

namespace CultOfCthulhu
{
    internal class ReanimatedPawnUtility
    {
        public static Color zombieSkin = new Color(r: 0.37f, g: 0.48f, b: 0.35f, a: 1f);

        public static ReanimatedPawn DoGenerateZombiePawnFromSource(Pawn sourcePawn, bool isBerserk = true,
            bool oathOfHastur = false)
        {
            var pawnKindDef = PawnKindDef.Named(defName: "ReanimatedCorpse");
            var factionDirect = isBerserk
                ? Find.FactionManager.FirstFactionOfDef(facDef: FactionDefOf.AncientsHostile)
                : Faction.OfPlayer;
            var pawn = (ReanimatedPawn) ThingMaker.MakeThing(def: pawnKindDef.race);
            try
            {
                pawn.kindDef = pawnKindDef;
                pawn.SetFactionDirect(newFaction: factionDirect);
                PawnComponentsUtility.CreateInitialComponents(pawn: pawn);
                pawn.gender = sourcePawn.gender;
                pawn.ageTracker.AgeBiologicalTicks = sourcePawn.ageTracker.AgeBiologicalTicks;
                pawn.ageTracker.AgeChronologicalTicks = sourcePawn.ageTracker.AgeChronologicalTicks;
                pawn.workSettings = new Pawn_WorkSettings(pawn: pawn);
                if (pawn.workSettings != null && sourcePawn.Faction.IsPlayer)
                {
                    pawn.workSettings.EnableAndInitialize();
                }

                pawn.needs.SetInitialLevels();
                //Add hediffs?
                //Add relationships?
                if (pawn.RaceProps.Humanlike)
                {
                    var letBiotechResolveSkinColor = false;
                    pawn.story.headType = sourcePawn.story.headType;
                    pawn.story.HairColor = sourcePawn.story.HairColor;
                    pawn.story.Childhood = sourcePawn.story.Childhood;
                    pawn.story.Adulthood = sourcePawn.story.Adulthood;
                    pawn.story.bodyType = sourcePawn.story.bodyType;
                    pawn.story.hairDef = sourcePawn.story.hairDef;
                    if (ModsConfig.BiotechActive)
                    {
                        letBiotechResolveSkinColor = true;
                        foreach (var endogene in sourcePawn.genes.Endogenes)
                            pawn.genes.AddGene(endogene.def, false);
                        foreach (var xenogene in sourcePawn.genes.Xenogenes)
                            pawn.genes.AddGene(xenogene.def, true);
                    }
                    if (!letBiotechResolveSkinColor)
                        pawn.story.skinColorOverride = sourcePawn.story.SkinColor;
                    
                    if (!oathOfHastur)
                    {
                        foreach (var current in sourcePawn.story.traits.allTraits)
                        {
                            pawn.story.traits.GainTrait(trait: current);
                        }
                    }
                    else
                    {
                        pawn.story.traits.GainTrait(trait: new Trait(def: TraitDef.Named(defName: "Cults_OathtakerHastur2"), degree: 0, forced: true));
                        pawn.story.traits.GainTrait(trait: new Trait(def: TraitDefOf.Psychopath, degree: 0, forced: true));

                        SkillFixer(pawn: pawn, sourcePawn: sourcePawn);
                        RelationshipFixer(pawn: pawn, sourcePawn: sourcePawn);
                        AddedPartFixer(pawn: pawn, sourcePawn: sourcePawn);
                    }

                    //pawn.story.GenerateSkillsFromBackstory();
                    var nameTriple = sourcePawn.Name as NameTriple;
                    if (!oathOfHastur)
                    {
                        pawn.Name = new NameTriple(first: nameTriple?.First,
                            nick: string.Concat("* ", "Reanimated".Translate(), " ", nameTriple?.Nick, " *"),
                            last: nameTriple?.Last);
                    }
                    else
                    {
                        pawn.Name = nameTriple;
                    }
                }

                var headGraphicPath = sourcePawn.story.headType.graphicPath;
                typeof(Pawn_StoryTracker).GetField(name: "headGraphicPath", bindingAttr: BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.SetValue(obj: pawn.story, value: headGraphicPath);
                GenerateZombieApparelFromSource(zombie: pawn, sourcePawn: sourcePawn);
                var con = new PawnGenerationRequest();
                PawnInventoryGenerator.GenerateInventoryFor(p: pawn, request: con);
                GiveZombieSkinEffect(pawn: pawn, sourcePawn: sourcePawn as ReanimatedPawn, oathOfHastur: oathOfHastur);
                if (isBerserk)
                {
                    pawn.mindState.mentalStateHandler.TryStartMentalState(stateDef: MentalStateDefOf.Berserk);
                }

                //Log.Message(pawn.Name.ToStringShort);
                return pawn;
            }
            catch (Exception e)
            {
                Utility.DebugReport(x: e.ToString());
            }

            return null;
        }

        public static void AddedPartFixer(ReanimatedPawn pawn, Pawn sourcePawn = null)
        {
            if (sourcePawn?.health.hediffSet.hediffs == null)
            {
                return;
            }

            foreach (var hediff in sourcePawn.health.hediffSet.hediffs)
            {
                if (hediff is Hediff_AddedPart || hediff is Hediff_Implant)
                {
                    pawn.health.AddHediff(hediff: hediff);
                }
            }
        }

        public static void SkillFixer(ReanimatedPawn pawn, Pawn sourcePawn = null)
        {
            //Add in and fix skill levels
            if (sourcePawn == null)
            {
                return;
            }

            foreach (var skill in sourcePawn.skills.skills)
            {
                var pawnSkill = pawn.skills.GetSkill(skillDef: skill.def);
                if (pawnSkill == null)
                {
                    pawn.skills.skills.Add(item: skill);
                }
                else
                {
                    pawnSkill.Level = skill.Level;
                    pawnSkill.passion = skill.passion;
                }
            }
        }

        public static void RelationshipFixer(ReanimatedPawn pawn, Pawn sourcePawn = null)
        {
            //Add in and fix all blood relationships
            if (sourcePawn != null && (sourcePawn.relations.DirectRelations == null ||
                                       sourcePawn.relations.DirectRelations.Count <= 0))
            {
                return;
            }

            if (sourcePawn == null)
            {
                return;
            }

            foreach (var pawnRel in sourcePawn.relations.DirectRelations)
            {
                if (pawnRel.otherPawn != null && pawnRel.def != null)
                {
                    pawn.relations.AddDirectRelation(def: pawnRel.def, otherPawn: pawnRel.otherPawn);
                }
            }

            sourcePawn.relations.ClearAllRelations();
        }

        public static void GiveZombieSkinEffect(ReanimatedPawn pawn, ReanimatedPawn sourcePawn = null,
            bool oathOfHastur = false)
        {
            if (sourcePawn == null)
            {
                sourcePawn = pawn;
            }

            var newSkin = oathOfHastur ? new Color(r: 1, g: 1, b: 1) : zombieSkin;

            var nakedBodyGraphic = GraphicDatabase.Get<Graphic_Multi>(path: sourcePawn.story.bodyType.bodyNakedGraphicPath,
                shader: ShaderDatabase.CutoutSkin, drawSize: Vector2.one, color: newSkin);
            var headGraphic = GraphicDatabase.Get<Graphic_Multi>(path: sourcePawn.story.headType.graphicPath,
                shader: ShaderDatabase.CutoutSkin, drawSize: Vector2.one, color: newSkin);
            var hairGraphic = GraphicDatabase.Get<Graphic_Multi>(path: sourcePawn.story.hairDef.texPath,
                shader: ShaderDatabase.Cutout, drawSize: Vector2.one, color: sourcePawn.story.HairColor);
            pawn.Drawer.renderer.graphics.headGraphic = headGraphic;
            pawn.Drawer.renderer.graphics.nakedGraphic = nakedBodyGraphic;
            pawn.Drawer.renderer.graphics.hairGraphic = hairGraphic;
        }

        public static bool Zombify(ReanimatedPawn pawn)
        {
            if (pawn.Drawer?.renderer?.graphics == null)
            {
                return false;
            }

            if (!pawn.Drawer.renderer.graphics.AllResolved)
            {
                pawn.Drawer.renderer.graphics.ResolveAllGraphics();
            }

            if (pawn.Drawer.renderer.graphics.headGraphic == null)
            {
                return false;
            }

            if (pawn.Drawer.renderer.graphics.nakedGraphic == null)
            {
                return false;
            }

            if (pawn.Drawer.renderer.graphics.headGraphic.path == null)
            {
                return false;
            }

            if (pawn.Drawer.renderer.graphics.nakedGraphic.path == null)
            {
                return false;
            }

            GiveZombieSkinEffect(pawn: pawn);
            return true;
        }

        // Credit goes to Justin C for the Zombie Apocalypse code.
        // Taken from Verse.ZombieMod_Utility
        public static Pawn GenerateZombiePawnFromSource(Pawn sourcePawn)
        {
            var pawnKindDef = PawnKindDef.Named(defName: "ReanimatedCorpse");
            var factionDirect = Find.FactionManager.FirstFactionOfDef(facDef: FactionDefOf.AncientsHostile);
            var pawn = (Pawn) ThingMaker.MakeThing(def: pawnKindDef.race);
            pawn.kindDef = pawnKindDef;
            pawn.SetFactionDirect(newFaction: factionDirect);
            pawn.pather = new Pawn_PathFollower(newPawn: pawn);
            pawn.ageTracker = new Pawn_AgeTracker(newPawn: pawn);
            pawn.health = new Pawn_HealthTracker(pawn: pawn);
            pawn.jobs = new Pawn_JobTracker(newPawn: pawn);
            pawn.mindState = new Pawn_MindState(pawn: pawn);
            pawn.filth = new Pawn_FilthTracker(pawn: pawn);
            pawn.needs = new Pawn_NeedsTracker(newPawn: pawn);
            pawn.stances = new Pawn_StanceTracker(newPawn: pawn);
            pawn.natives = new Pawn_NativeVerbs(pawn: pawn);
            PawnComponentsUtility.CreateInitialComponents(pawn: pawn);
            if (pawn.RaceProps.ToolUser)
            {
                pawn.equipment = new Pawn_EquipmentTracker(newPawn: pawn);
                pawn.carryTracker = new Pawn_CarryTracker(pawn: pawn);
                pawn.apparel = new Pawn_ApparelTracker(pawn: pawn);
                pawn.inventory = new Pawn_InventoryTracker(pawn: pawn);
            }

            if (pawn.RaceProps.Humanlike)
            {
                pawn.ownership = new Pawn_Ownership(pawn: pawn);
                pawn.skills = new Pawn_SkillTracker(newPawn: pawn);
                pawn.relations = new Pawn_RelationsTracker(pawn: pawn);
                pawn.story = new Pawn_StoryTracker(pawn: pawn);
                pawn.workSettings = new Pawn_WorkSettings(pawn: pawn);
            }

            if (pawn.RaceProps.intelligence <= Intelligence.ToolUser)
            {
                pawn.caller = new Pawn_CallTracker(pawn: pawn);
            }

            //pawn.gender = Gender.None;
            pawn.gender = sourcePawn.gender;
            Utility.GenerateRandomAge(pawn: pawn, map: pawn.Map);
            pawn.needs.SetInitialLevels();
            if (pawn.RaceProps.Humanlike)
            {
                var headGraphicPath = sourcePawn.story.headType.graphicPath;
                typeof(Pawn_StoryTracker).GetField(name: "headGraphicPath", bindingAttr: BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.SetValue(obj: pawn.story, value: headGraphicPath);

                if (ModsConfig.BiotechActive)
                {
                    foreach (var endogene in sourcePawn.genes.Endogenes)
                        pawn.genes.AddGene(endogene.def, false);
                    foreach (var xenogene in sourcePawn.genes.Xenogenes)
                        pawn.genes.AddGene(xenogene.def, true);
                }
                else
                {
                    pawn.story.skinColorOverride = sourcePawn.story.SkinColor;
                }
                pawn.story.headType = sourcePawn.story.headType;
                pawn.story.HairColor = sourcePawn.story.HairColor;
                var name = sourcePawn.Name as NameTriple;
                pawn.Name = name;
                pawn.story.Childhood = sourcePawn.story.Childhood;
                pawn.story.Adulthood = sourcePawn.story.Adulthood;
                pawn.story.hairDef = sourcePawn.story.hairDef;
                
                
                foreach (var current in sourcePawn.story.traits.allTraits)
                {
                    pawn.story.traits.GainTrait(trait: current);
                }

                //pawn.story.GenerateSkillsFromBackstory();
            }

            GenerateZombieApparelFromSource(zombie: pawn, sourcePawn: sourcePawn);
            var con = new PawnGenerationRequest();
            PawnInventoryGenerator.GenerateInventoryFor(p: pawn, request: con);
            //Graphic nakedBodyGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(sourcePawn.story.bodyType, ShaderDatabase.CutoutSkin, new Color(0.37f, 0.48f, 0.35f, 1f));
            var nakedBodyGraphic = GraphicDatabase.Get<Graphic_Multi>(path: sourcePawn.story.bodyType.bodyNakedGraphicPath,
                shader: ShaderDatabase.CutoutSkin, drawSize: Vector2.one, color: new Color(r: 0.37f, g: 0.48f, b: 0.35f, a: 1f));
            var headGraphic = GraphicDatabase.Get<Graphic_Multi>(path: sourcePawn.story.headType.graphicPath,
                shader: ShaderDatabase.CutoutSkin, drawSize: Vector2.one, color: new Color(r: 0.37f, g: 0.48f, b: 0.35f, a: 1f));
            var hairGraphic = GraphicDatabase.Get<Graphic_Multi>(path: sourcePawn.story.hairDef.texPath,
                shader: ShaderDatabase.Cutout, drawSize: Vector2.one, color: sourcePawn.story.HairColor);
            pawn.Drawer.renderer.graphics.headGraphic = headGraphic;
            pawn.Drawer.renderer.graphics.nakedGraphic = nakedBodyGraphic;
            pawn.Drawer.renderer.graphics.hairGraphic = hairGraphic;
            return pawn;
        }

        // More of Justin C's work. I can't take credit for this.
        // Verse.ZombieMod_Utility
        public static void GenerateZombieApparelFromSource(Pawn zombie, Pawn sourcePawn)
        {
            if (sourcePawn.apparel == null || sourcePawn.apparel.WornApparelCount == 0)
            {
                return;
            }

            foreach (var current in sourcePawn.apparel.WornApparel)
            {
                Apparel apparel;
                if (current.def.MadeFromStuff)
                {
                    apparel = (Apparel) ThingMaker.MakeThing(def: current.def, stuff: current.Stuff);
                }
                else
                {
                    apparel = (Apparel) ThingMaker.MakeThing(def: current.def);
                }

                apparel.DrawColor = new Color(r: current.DrawColor.r, g: current.DrawColor.g, b: current.DrawColor.b,
                    a: current.DrawColor.a);
                zombie.apparel.Wear(newApparel: apparel);
            }
        }
    }
}