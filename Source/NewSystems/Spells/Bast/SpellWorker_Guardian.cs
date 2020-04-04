using CultOfCthulhu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using RimWorld.Planet;

namespace BastCult
{
    /// <summary>
    /// This spell tries to transform a normal cat into a potent guardian.
    /// </summary>
    public class SpellWorker_Guardian : SpellWorker
    {
        public override bool CanSummonNow(Map map)
        {
            Pawn cat = TryGetClosestCatOnMap(map);

            if (cat == null)
                Messages.Message("Cults_BastNoCatOnMap".Translate(), MessageTypeDefOf.RejectInput);

            return cat != null;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = parms.target as Map;
            Pawn closestCat = TryGetClosestCatOnMap(map);

            GuardianProperties guardianProps = def.GetModExtension<GuardianProperties>();

            if (closestCat != null)
            {
                //Transform Cat
                //Generate guardian
                Pawn newGuardian = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                    guardianProps.guardianDef, Faction.OfPlayer, PawnGenerationContext.NonPlayer, -1, true, false, false, false, false,
                    true, 0f, false, true, true, false, false, false, false, false, 0, null, 0, null, null, null, null, null,closestCat.ageTracker.AgeBiologicalYears, closestCat.ageTracker.AgeChronologicalYears, closestCat.gender, null, null));

                //Transfer over family trees and relations to guardian from old cat.
                Pawn_RelationsTracker oldRelations = closestCat.relations;
                Pawn_RelationsTracker newRelations = newGuardian.relations;

                //Transfer over relations.
                List<DirectPawnRelation> relationList = new List<DirectPawnRelation>(oldRelations.DirectRelations);
                foreach(DirectPawnRelation relation in relationList)
                {
                    newRelations.AddDirectRelation(relation.def, relation.otherPawn);
                    oldRelations.RemoveDirectRelation(relation);
                }

                //Fully train.
                foreach(TrainableDef trainableDef in DefDatabase<TrainableDef>.AllDefs)
                {
                    for(int step = 0; step < trainableDef.steps; step++)
                    {
                        newGuardian.training.Train(trainableDef, null);
                    }
                }

                //Make a new name.
                if(closestCat.Name != null)
                {
                    if(closestCat.gender == Gender.Male)
                        newGuardian.Name = new NameSingle(NameGenerator.GenerateName(RulePackDef.Named("NamerAnimalGenericMale")), false);
                    else
                        newGuardian.Name = new NameSingle(NameGenerator.GenerateName(RulePackDef.Named("NamerAnimalGenericFemale")), false);
                }

                //Dump inventory, if any.
                closestCat?.inventory.DropAllNearPawn(closestCat.Position);

                Letter letter = LetterMaker.MakeLetter("Cults_BastGuardianTransformationLabel".Translate(closestCat.Name.ToStringShort), "Cults_BastGuardianTransformationDescription".Translate(closestCat.Name.ToStringFull), LetterDefOf.PositiveEvent, new GlobalTargetInfo(newGuardian));

                //Remove old cat.
                IntVec3 catPosition = closestCat.Position;
                closestCat.Destroy(DestroyMode.Vanish);

                //Spawn in guardian.
                GenSpawn.Spawn(newGuardian, catPosition, map);
                MoteMaker.MakePowerBeamMote(catPosition, map);

                Current.Game.letterStack.ReceiveLetter(letter);
            }

            return true;
        }

        /// <summary>
        /// Tries to get a cat that is the closest to the altar.
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        protected Pawn TryGetClosestCatOnMap(Map map)
        {
            Building_SacrificialAltar mapAltar = altar(map);
            GuardianProperties guardianProps = def.GetModExtension<GuardianProperties>();

            if (mapAltar != null && guardianProps != null)
            {
                Thing closestThing = GenClosest.ClosestThingReachable(
                    mapAltar.InteractionCell, map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), 
                    Verse.AI.PathEndMode.ClosestTouch, TraverseParms.For(TraverseMode.PassDoors), 9999, lookThing => (lookThing?.Faction?.IsPlayer ?? false) && guardianProps.eligiblePawnDefs.Contains(lookThing.def));

                //Found a Cat.
                if(closestThing != null && closestThing is Pawn)
                {
                    return closestThing as Pawn;
                }
            }

            return null;
        }
    }
}
