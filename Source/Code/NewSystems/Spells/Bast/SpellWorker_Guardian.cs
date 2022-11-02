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
            var cat = TryGetClosestCatOnMap(map: map);

            if (cat == null)
            {
                Messages.Message(text: "Cults_BastNoCatOnMap".Translate(), def: MessageTypeDefOf.RejectInput);
            }

            return cat != null;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var map = parms.target as Map;
            var closestCat = TryGetClosestCatOnMap(map: map);

            var guardianProps = def.GetModExtension<GuardianProperties>();

            if (closestCat == null)
            {
                return true;
            }

            //Transform Cat
            //Generate guardian
            var newGuardian = PawnGenerator.GeneratePawn(request: new PawnGenerationRequest(
                kind: guardianProps.guardianDef, faction: Faction.OfPlayer, context: PawnGenerationContext.NonPlayer, tile: -1, forceGenerateNewPawn: true, allowDead: false,
                allowDowned: false, canGeneratePawnRelations: false, mustBeCapableOfViolence: false,
                colonistRelationChanceFactor: 0f, forceAddFreeWarmLayerIfNeeded: true, allowGay: false, allowPregnant: true, allowFood: true, 
                allowAddictions: false, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 
                biocodeWeaponChance: 0, biocodeApparelChance: 0, extraPawnForExtraRelationChance: null, relationWithExtraPawnChanceFactor: 0, validatorPreGear: null, validatorPostGear: null, 
                forcedTraits: null, prohibitedTraits: null,
                minChanceToRedressWorldPawn: null, fixedBiologicalAge: closestCat.ageTracker.AgeBiologicalYears, fixedChronologicalAge: closestCat.ageTracker.AgeChronologicalYears,
                fixedGender: closestCat.gender));

            //Transfer over family trees and relations to guardian from old cat.
            var oldRelations = closestCat.relations;
            var newRelations = newGuardian.relations;

            //Transfer over relations.
            var relationList = new List<DirectPawnRelation>(collection: oldRelations.DirectRelations);
            foreach (var relation in relationList)
            {
                newRelations.AddDirectRelation(def: relation.def, otherPawn: relation.otherPawn);
                oldRelations.RemoveDirectRelation(relation: relation);
            }

            //Fully train.
            foreach (var trainableDef in DefDatabase<TrainableDef>.AllDefs)
            {
                for (var step = 0; step < trainableDef.steps; step++)
                {
                    newGuardian.training.Train(td: trainableDef, trainer: null);
                }
            }

            //Make a new name.
            if (closestCat.Name != null)
            {
                newGuardian.Name = closestCat.gender == Gender.Male
                    ? new NameSingle(name: NameGenerator.GenerateName(rootPack: RulePackDef.Named(defName: "NamerAnimalGenericMale")))
                    : new NameSingle(name: NameGenerator.GenerateName(rootPack: RulePackDef.Named(defName: "NamerAnimalGenericFemale")));
            }

            //Dump inventory, if any.
            closestCat.inventory.DropAllNearPawn(pos: closestCat.Position);

            Letter letter = LetterMaker.MakeLetter(
                label: "Cults_BastGuardianTransformationLabel".Translate(arg1: closestCat.Name?.ToStringShort),
                text: "Cults_BastGuardianTransformationDescription".Translate(arg1: closestCat.Name?.ToStringFull),
                def: LetterDefOf.PositiveEvent, lookTargets: new GlobalTargetInfo(thing: newGuardian));

            //Remove old cat.
            var catPosition = closestCat.Position;
            closestCat.Destroy();

            //Spawn in guardian.
            GenSpawn.Spawn(newThing: newGuardian, loc: catPosition, map: map);
            MoteMaker.MakePowerBeamMote(cell: catPosition, map: map);

            Current.Game.letterStack.ReceiveLetter(@let: letter);

            return true;
        }

        /// <summary>
        ///     Tries to get a cat that is the closest to the altar.
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        protected Pawn TryGetClosestCatOnMap(Map map)
        {
            var mapAltar = altar(map: map);
            var guardianProps = def.GetModExtension<GuardianProperties>();

            if (mapAltar == null || guardianProps == null)
            {
                return null;
            }

            var closestThing = GenClosest.ClosestThingReachable(
                root: mapAltar.InteractionCell, map: map, thingReq: ThingRequest.ForGroup(@group: ThingRequestGroup.Pawn),
                peMode: PathEndMode.ClosestTouch, traverseParams: TraverseParms.For(mode: TraverseMode.PassDoors), maxDistance: 9999,
                validator: lookThing => (lookThing?.Faction?.IsPlayer ?? false) &&
                                        guardianProps.eligiblePawnDefs.Contains(item: lookThing.def));

            //Found a Cat.
            var pawn = closestThing as Pawn;
            return pawn;
        }
    }
}