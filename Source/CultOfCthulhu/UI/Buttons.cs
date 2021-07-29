using UnityEngine;
using Verse;

namespace CultOfCthulhu
{
    [StaticConstructorOnStartup]
    internal class Buttons
    {
        public static readonly Texture2D TierBarFillTex =
            SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.25f));

        public static readonly Texture2D RenameTex = ContentFinder<Texture2D>.Get("UI/SorryTynan_Rename");
        public static readonly Texture2D RedTex = SolidColorMaterials.NewSolidColorTexture(Color.red);
    }
}