using System.Collections.Generic;
using Verse;

namespace BastCult
{
    /// <summary>
    ///     Properties for the Bast Guardian spell.
    /// </summary>
    public class GuardianProperties : DefModExtension
    {
        /// <summary>
        ///     Pawn types who are eligible to be transformed into a Guardian.
        /// </summary>
        public List<ThingDef> eligiblePawnDefs = new List<ThingDef>();

        /// <summary>
        ///     What Def the pawn will be transformed into.
        /// </summary>
        public PawnKindDef guardianDef;
    }
}