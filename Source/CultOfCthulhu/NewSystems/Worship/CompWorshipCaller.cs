using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace CultOfCthulhu
{
    public class CompWorshipCaller : ThingComp
    {
        public CompProperties_WorshipCaller Props => props as CompProperties_WorshipCaller;

        public Building_SacrificialAltar Altar => (Building_SacrificialAltar) GenClosest.ClosestThingReachable(
            parent.PositionHeld, parent.Map,
            ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.ClosestTouch,
            TraverseMode.ByPawn, 9999, x => x is Building_SacrificialAltar);

        public IEnumerable<IntVec3> CellsInRange =>
            GenRadial.RadialCellsAround(parent.Position, Props.rangeRadius, true);


        public virtual void Use(bool forced)
        {
            Props.hitSound.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
            Building_SacrificialAltar.GetWorshipGroup(Altar, CellsInRange, forced);
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            base.PostDrawExtraSelectionOverlays();
            GenDraw.DrawRadiusRing(parent.Position, Props.rangeRadius);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var g in base.CompGetGizmosExtra())
            {
                yield return g;
            }
        }
    }
}