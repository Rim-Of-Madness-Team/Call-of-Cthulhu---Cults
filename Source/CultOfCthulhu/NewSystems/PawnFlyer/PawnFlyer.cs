using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CultOfCthulhu
{
    public class PawnFlyer : Pawn
    {
        private CompLaunchablePawn compLaunchablePawn;
        private CompTransporterPawn compTransporterPawn;

        public override void SpawnSetup(Map map, bool bla)
        {
            compTransporterPawn = this.TryGetComp<CompTransporterPawn>();
            compLaunchablePawn = this.TryGetComp<CompLaunchablePawn>();
            DecrementMapIndex();
            base.SpawnSetup(map, bla);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            using var enumerator = base.GetGizmos().GetEnumerator();
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                yield return current;
            }

            if (Faction != Faction.OfPlayer || Dead || Dead)
            {
                yield break;
            }

            if (compTransporterPawn.LoadingInProgressOrReadyToLaunch)
            {
                var command_Action = new Command_Action
                {
                    defaultLabel = "CommandLaunchGroup".Translate(),
                    defaultDesc = "CommandLaunchGroupDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Icons/Commands/FlyingTarget"),
                    action = delegate
                    {
                        if (compTransporterPawn.AnyInGroupHasAnythingLeftToLoad)
                        {
                            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                                "ConfirmSendNotCompletelyLoadedPods".Translate(
                                    compTransporterPawn.FirstThingLeftToLoadInGroup.LabelCap
                                ), compLaunchablePawn.StartChoosingDestination));
                        }
                        else
                        {
                            compLaunchablePawn.StartChoosingDestination();
                        }
                    }
                };
                if (compLaunchablePawn.AnyInGroupIsUnderRoof)
                {
                    command_Action.Disable("CommandLaunchGroupFailUnderRoof".Translate());
                }

                yield return command_Action;
            }

            if (compTransporterPawn.LoadingInProgressOrReadyToLaunch)
            {
                yield return new Command_Action
                {
                    defaultLabel = "CommandCancelLoad".Translate(),
                    defaultDesc = "CommandCancelLoadDesc".Translate(),
                    icon = CompTransporterPawn.CancelLoadCommandTex,
                    action = delegate
                    {
                        SoundDefOf.Designate_Cancel.PlayOneShotOnCamera();
                        compTransporterPawn.CancelLoad();
                    }
                };
            }

            var command_LoadToTransporter = new Command_LoadToTransporterPawn();
            var num = 0;
            for (var i = 0; i < Find.Selector.NumSelected; i++)
            {
                if (Find.Selector.SelectedObjectsListForReading[i] is not Thing thing || thing.def != def)
                {
                    continue;
                }

                var compLaunchable = thing.TryGetComp<CompLaunchablePawn>();
                if (compLaunchable != null)
                {
                    num++;
                }
            }

            command_LoadToTransporter.defaultLabel = "CommandLoadTransporter".Translate(
                num.ToString()
            );
            command_LoadToTransporter.defaultDesc = "CommandLoadTransporterDesc".Translate();
            command_LoadToTransporter.icon = CompTransporterPawn.LoadCommandTex;
            command_LoadToTransporter.transComp = compTransporterPawn;
            var launchable = compTransporterPawn.Launchable;
            yield return command_LoadToTransporter;
        }
    }
}