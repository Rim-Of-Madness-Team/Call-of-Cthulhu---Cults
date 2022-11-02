using UnityEngine;
using Verse;

namespace CultOfCthulhu
{
    [StaticConstructorOnStartup]
    internal class Buttons
    {
        public static readonly Texture2D TierBarFillTex =
            SolidColorMaterials.NewSolidColorTexture(color: new Color(r: 1f, g: 1f, b: 1f, a: 0.25f));

        public static readonly Texture2D RenameTex = ContentFinder<Texture2D>.Get(itemPath: "UI/SorryTynan_Rename");
        public static readonly Texture2D RedTex = SolidColorMaterials.NewSolidColorTexture(color: Color.red);
    }
}