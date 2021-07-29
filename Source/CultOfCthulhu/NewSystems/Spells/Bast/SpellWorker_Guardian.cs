using System.Collections.Generic;
using CultOfCthulhu;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace BastCult
{
    /// <summary>
    ///     This spell tries to transform a normal cat into a potent guardian.
    /// </summary>
    public class SpellWorker_Guardian : SpellWorker
    {
        public override bool CanSummonNow(Map map)
        {
            var cat = TryGetClosestCatOnMap(map);

            if (cat == null)
            {
                Messages.Message("Cults_BastNoCatOnMap".Translate(), MessageTypeDefOf.RejectInput);
            }

            return cat != null;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var map = parms.target as Map;
            var closestCat = TryGetClosestCatOnMap(map);

            var guardianProps = def.GetModExtension<GuardianProperties>();

            if (closestCat == null)
            {
                return true;
            }

            //Transform Cat
            //Generate guardian
            var newGuardian = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                guardianProps.guardianDef, Faction.OfPlayer, PawnGenerationContext.NonPlayer, -1, true, false,
                false, false, false,
                true, 0f, false, true, true, false, false, false, false, false, 0, 0, null, 0, null, null, null, null,
                null, closestCat.ageTracker.AgeBiologicalYears, closestCat.ageTracker.AgeChronologicalYears,
                closestCat.gender));

            //Transfer over family trees and relations to guardian from old cat.
            var oldRelations = closestCat.relations;
            var newRelations = newGuardian.relations;

            //Transfer over relations.
            var relationList = new List<DirectPawnRelation>(oldRelations.DirectRelations);
            foreach (var relation in relationList)
            {
                newRelations.AddDirectRelation(relation.def, relation.otherPawn);
                oldRelations.RemoveDirectRelation(relation);
            }

            //Fully train.
            foreach (var trainableDef in DefDatabase<TrainableDef>.AllDefs)
            {
                for (var step = 0; step < trainableDef.steps; step++)
                {
                    newGuardian.training.Train(trainableDef, null);
                }
            }

            //Make a new name.
            if (closestCat.Name != null)
            {
                newGuardian.Name = closestCat.gender == Gender.Male
                    ? new NameSingle(NameGenerator.GenerateName(RulePackDef.Named("NamerAnimalGenericMale")))
                    : new NameSingle(NameGenerator.GenerateName(RulePackDef.Named("NamerAnimalGenericFemale")));
            }

            //Dump inventory, if any.
            closestCat.inventory.DropAllNearPawn(closestCat.Position);

            Letter letter = LetterMaker.MakeLetter(
                "Cults_BastGuardianTransformationLabel".Translate(closestCat.Name?.ToStringShort),
                "Cults_BastGuardianTransformationDescription".Translate(closestCat.Name?.ToStringFull),
                LetterDefOf.PositiveEvent, new GlobalTargetInfo(newGuardian));

            //Remove old cat.
            var catPosition = closestCat.Position;
            closestCat.Destroy();

            //Spawn in guardian.
            GenSpawn.Spawn(newGuardian, catPosition, map);
            MoteMaker.MakePowerBeamMote(catPosition, map);

            Current.Game.letterStack.ReceiveLetter(letter);

            return true;
        }

        /// <summary>
        ///     Tries to get a cat that is the closest to the altar.
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        protected Pawn TryGetClosestCatOnMap(Map map)
        {
            var mapAltar = altar(map);
            var guardianProps = def.GetModExtension<GuardianProperties>();

            if (mapAltar == null || guardianProps == null)
            {
                return null;
            }

            var closestThing = GenClosest.ClosestThingReachable(
                mapAltar.InteractionCell, map, ThingRequest.ForGroup(ThingRequestGroup.Pawn),
                PathEndMode.ClosestTouch, TraverseParms.For(TraverseMode.PassDoors), 9999,
                lookThing => (lookThing?.Faction?.IsPlayer ?? false) &&
                             guardianProps.eligiblePawnDefs.Contains(lookThing.def));

            //Found a Cat.
            var pawn = closestThing as Pawn;
            return pawn;
        }
    }
}