using RimWorld;
using UnityEngine;

namespace CultOfCthulhu
{
    // <summary>
    // Sanity meter
/*    If individuals experience strange circumstances, they will lose sanity. 
    This insanity can spread like a virus in colonies without good social networks,
    outlets for expression, or anti-psychotics, as rejection to express onesself will
    further increase sanity loss. When sanity loss reaches its lowest threshold, 
    the character is permanently marked by madness and receives an insanity trait.
    Changed forever, when other colonists observe this individual marked with insanity,
    they too will be affected with sanity loss -- and so begins the spiral into madness.*/
    // </summary>
    public class Need_Sanity : Need
    {
        public float InsanityTraitTreshold => 0.15f;

        //Sanity is static.
        public override int GUIChangeArrow { get; } = 0;

        //Sanity has no interval of change.
        public override void NeedInterval()
        {
        }

        //Characters start fully sane.
        public override void SetInitialLevel()
        {
            CurLevelPercentage = 1.0f;
        }

        //Social interactions / Strange circumstances use this method to adjust sanity.
        public void AdjustSanity(float amt)
        {
            CurLevelPercentage = Mathf.Clamp01(value: CurLevelPercentage + amt);
        }
    }
}