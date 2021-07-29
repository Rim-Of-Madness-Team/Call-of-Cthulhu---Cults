using System.Collections.Generic;
using Verse;

namespace BastCult
{
    /// <summary>
    ///     Properties for the Feline Aspect spell.
    /// </summary>
    public class FelineAspectProperties : DefModExtension
    {
        /// <summary>
        ///     Body parts classified as hands.
        /// </summary>
        public List<BodyPartDef> handDefs = new List<BodyPartDef>();

        /// <summary>
        ///     Applied to the whole body.
        /// </summary>
        public HediffDef hediffToApplyToBody;

        /// <summary>
        ///     Applied only to hands.
        /// </summary>
        public HediffDef hediffToApplyToHands;
    }
}