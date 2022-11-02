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
            root: parent.PositionHeld, map: parent.Map,
            thingReq: ThingRequest.ForGroup(@group: ThingRequestGroup.BuildingArtificial), peMode: PathEndMode.ClosestTouch,
            traverseParams: TraverseMode.ByPawn, maxDistance: 9999, validator: x => x is Building_SacrificialAltar);

        public IEnumerable<IntVec3> CellsInRange =>
            GenRadial.RadialCellsAround(center: parent.Position, radius: Props.rangeRadius, useCenter: true);


        public virtual void Use(bool forced)
        {
            Props.hitSound.PlayOneShot(info: new TargetInfo(cell: parent.Position, map: parent.Map));
            Building_SacrificialAltar.GetWorshipGroup(altar: Altar, inRangeCells: CellsInRange, forced: forced);
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            base.PostDrawExtraSelectionOverlays();
            GenDraw.DrawRadiusRing(center: parent.Position, radius: Props.rangeRadius);
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