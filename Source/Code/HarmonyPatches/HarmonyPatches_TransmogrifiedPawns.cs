using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CultOfCthulhu
{
    internal static partial class HarmonyPatches
    {
        static void HarmonyPatches_TransmogrifiedPawns(Harmony harmony)
        {
            //Initializes Transmogrified/Monstrous Animals
            harmony.Patch(original: AccessTools.Method(type: typeof(ThingWithComps), name: nameof(ThingWithComps.InitializeComps)), prefix: null,
                postfix: new HarmonyMethod(methodType: typeof(HarmonyPatches), methodName: nameof(InitializeComps_PostFix)));
            DebugMessage(s: "ThingWithComps.InitializeComps_PostFix Passed");

            //Triples the body size of Transmogrified/Monstrous Animals
            harmony.Patch(original: AccessTools.Property(type: typeof(Pawn), name: nameof(Pawn.BodySize)).GetGetMethod(), prefix: null,
                postfix: new HarmonyMethod(methodType: typeof(HarmonyPatches), methodName: nameof(get_BodySize_PostFix)));
            DebugMessage(s: "Pawn.BodySize.get_BodySize_PostFix Passed");
            
            //Triples monstrous/transmogrified health scale values
            harmony.Patch(original: AccessTools.Property(type: typeof(Pawn), name: nameof(Pawn.HealthScale)).GetGetMethod(), prefix: null,
                postfix: new HarmonyMethod(methodType: typeof(HarmonyPatches), methodName: nameof(get_HealthScale_PostFix)));
            DebugMessage(s: "Pawn.HealthScale.get_HealthScale_PostFix Passed");
            
            //Adds an aura to Transmogrified/Monstrous animals
            harmony.Patch(original: AccessTools.Method(type: typeof(Pawn_DrawTracker), name: nameof(Pawn_DrawTracker.DrawAt)), prefix: null,
                postfix: new HarmonyMethod(methodType: typeof(HarmonyPatches), methodName: nameof(DrawAt_PostFix)));
            DebugMessage(s: "Pawn_DrawTracker.DrawAt_PostFix Passed");

            // Adds the word 'Monstrous' to the name of the transmogrified creature
            harmony.Patch(
                original: AccessTools.Method(type: typeof(GenLabel), name: "BestKindLabel",
                    parameters: new[] {typeof(Pawn), typeof(bool), typeof(bool), typeof(bool), typeof(int)}), prefix: null,
                postfix: new HarmonyMethod(methodType: typeof(HarmonyPatches), methodName: nameof(BestKindLabel_PostFix)));
            DebugMessage(s: "GenLabel.BestKindLabel_PostFix Passed");
        }


        //Initializes Transmogrified/Monstrous Animals
        //Origin: ThingWithComps.InitializeComps
        public static void InitializeComps_PostFix(ThingWithComps __instance)
        {
            if (!(__instance is Pawn p))
            {
                return;
            }

            if (p.RaceProps == null || !p.RaceProps.Animal)
            {
                return;
            }

            var thingComp = (ThingComp) Activator.CreateInstance(type: typeof(CompTransmogrified));
            thingComp.parent = __instance;
            var comps = AccessTools.Field(type: typeof(ThingWithComps), name: "comps").GetValue(obj: __instance);
            ((List<ThingComp>) comps)?.Add(item: thingComp);

            thingComp.Initialize(props: null);
        }

        //Triples monstrous/transmogrified pawn body sizes
        //Origin: Pawn.BodySize
        public static void get_BodySize_PostFix(Pawn __instance, ref float __result)
        {
            if (__instance?.GetComp<CompTransmogrified>() is CompTransmogrified {IsTransmogrified: true})
            {
                __result *= 3;
            }
        }
        //Triples monstrous/transmogrified health scale values
        //Origin: Pawn.HealthScale
        public static void get_HealthScale_PostFix(Pawn __instance, ref float __result)
        {
            if (__instance?.GetComp<CompTransmogrified>() is CompTransmogrified {IsTransmogrified: true})
            {
                __result *= 3;
            }
        }

        
        //Adds an aura to Transmogrified/Monstrous animals
        //RenderPawnAt_PostFix
        // Origin: Verse.PawnRenderer
        public static void DrawAt_PostFix(Pawn_DrawTracker __instance, Vector3 loc)
        {
            var pawn = (Pawn) AccessTools.Field(type: typeof(Pawn_DrawTracker), name: "pawn").GetValue(obj: __instance);
            if (!(pawn?.GetComp<CompTransmogrified>() is CompTransmogrified {IsTransmogrified: true} compTrans) ||
                !pawn.Spawned)
            {
                return;
            }

            var matSingle = CultsDefOf.Cults_TransmogAura.graphicData.Graphic.MatSingle;
            var angle = pawn.Rotation.AsAngle + (compTrans.Hediff.UndulationTicks * 100);

            var xCap = pawn.kindDef.lifeStages[index: 0].bodyGraphicData.drawSize.x + 0.5f;
            var zCap = pawn.kindDef.lifeStages[index: 0].bodyGraphicData.drawSize.y + 0.5f;

            var x = pawn.kindDef.lifeStages[index: 0].bodyGraphicData.drawSize.x;
            var z = pawn.kindDef.lifeStages[index: 0].bodyGraphicData.drawSize.y;
            var drawX = Mathf.Clamp(value: (x + compTrans.Hediff.UndulationTicks) * compTrans.Hediff.graphicDiv, min: 0.01f,
                max: xCap);
            var drawY = AltitudeLayer.Terrain.AltitudeFor();
            var drawZ = Mathf.Clamp(value: (z + compTrans.Hediff.UndulationTicks) * compTrans.Hediff.graphicDiv, min: 0.01f,
                max: zCap);
            var s = new Vector3(x: drawX, y: drawY, z: drawZ);
            Matrix4x4 matrix = default;
            matrix.SetTRS(pos: loc, q: Quaternion.AngleAxis(angle: angle, axis: Vector3.up), s: s);
            Graphics.DrawMesh(mesh: MeshPool.plane10Back, matrix: matrix, material: matSingle, layer: 0);
        }

        // Adds the word 'Monstrous' to the name of the transmogrified creature
        // Origin: RimWorld.GenLabel
        public static void BestKindLabel_PostFix(Pawn pawn, bool mustNoteGender,
            bool mustNoteLifeStage, bool plural, int pluralCount, ref string __result)
        {
            if (pawn?.GetComp<CompTransmogrified>() is CompTransmogrified {IsTransmogrified: true})
            {
                __result = "Cults_Monstrous".Translate(arg1: __result);
            }
        }
    }
}