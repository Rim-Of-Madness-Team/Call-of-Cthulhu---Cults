using UnityEngine;
using Verse;

namespace CultOfCthulhu
{
    public class Graphic_Wild : Graphic_Collection
    {
        private const int BaseTicksPerFrameChange = 15;

        private const int ExtraTicksPerFrameChange = 10;

        private const float MaxOffset = 0.05f;

        public override Material MatSingle => subGraphics[Rand.Range(min: 0, max: subGraphics.Length)].MatSingle;

        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
        {
            if (thingDef == null)
            {
                Log.ErrorOnce(text: "DrawWorker with null thingDef: " + loc, key: 3427324);
                return;
            }

            if (subGraphics == null)
            {
                Log.ErrorOnce(text: "Graphic_Wild has no subgraphics " + thingDef, key: 358773632);
                return;
            }

            var num = Find.TickManager.TicksGame;
            var num2 = 0;
            var num3 = 0;
            var num4 = 1f;
            //CompFireOverlay compFireOverlay = null;
            if (thing != null)
            {
                //compFireOverlay = thing.TryGetComp<CompFireOverlay>();
                num += Mathf.Abs(value: thing.thingIDNumber ^ 8453458);
                num2 = num / 15;
                num3 = Mathf.Abs(value: num2 ^ (thing.thingIDNumber * 391)) % subGraphics.Length;
                //Fire fire = thing as Fire;
                //if (fire != null)
                //{
                //    num4 = fire.fireSize;
                //}
                //else if (compFireOverlay != null)
                //{
                //    num4 = compFireOverlay.Props.fireSize;
                //}
            }

            if (num3 < 0 || num3 >= subGraphics.Length)
            {
                Log.ErrorOnce(text: "Fire drawing out of range: " + num3, key: 7453435);
                num3 = 0;
            }

            var graphic = subGraphics[num3];
            var num5 = Mathf.Min(a: num4 / 1.2f, b: 1.2f);
            var a = GenRadial.RadialPattern[num2 % GenRadial.RadialPattern.Length].ToVector3() /
                    GenRadial.MaxRadialPatternRadius;
            a *= 0.05f;
            var vector = loc + (a * num4);
            //if (compFireOverlay != null)
            //{
            //    vector += compFireOverlay.Props.offset;
            //}
            var s = new Vector3(x: num5, y: 1f, z: num5);
            Matrix4x4 matrix = default;
            matrix.SetTRS(pos: vector, q: Quaternion.identity, s: s);
            Graphics.DrawMesh(mesh: MeshPool.plane10, matrix: matrix, material: graphic.MatSingle, layer: 0);
        }

        public override string ToString()
        {
            return string.Concat("Flicker(subGraphic[0]=", subGraphics[0].ToString(), ", count=", subGraphics.Length,
                ")");
        }
    }
}