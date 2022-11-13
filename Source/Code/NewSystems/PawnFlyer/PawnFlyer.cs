using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
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
            base.SpawnSetup(map: map, respawningAfterLoad: bla);
            ClearMind();
            mindState.Active = true;
            if (map?.IsPlayerHome == false)
            {
                if (map.mapPawns?.FreeColonists.First() is { } colonist)
                {
                    var lord = colonist.GetLord();
                    if (lord != null)
                        lord.AddPawn(this);
                    
                    ThinkNode_JobGiver obj = (ThinkNode_JobGiver)Activator.CreateInstance(typeof(JobGiver_WanderMapEdge));
                    obj.ResolveReferences();
                    ThinkResult thinkResult = obj.TryIssueJobPackage(this, default(JobIssueParams));
                    if (thinkResult.Job != null)
                    {
                        this.jobs.StartJob(thinkResult.Job);
                    }
                    
                }
            }
        }

        public override void Kill(DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            if (compTransporterPawn is { LoadingInProgressOrReadyToLaunch: true })
                compTransporterPawn.CancelLoad();
            base.Kill(dinfo, exactCulprit);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (compTransporterPawn is { LoadingInProgressOrReadyToLaunch: true })
                compTransporterPawn.CancelLoad();
            base.Destroy(mode);
        }

        //
        // public override IEnumerable<Gizmo> GetGizmos()
        // {
        //     using var enumerator = base.GetGizmos().GetEnumerator();
        //     while (enumerator.MoveNext())
        //     {
        //         var current = enumerator.Current;
        //         yield return current;
        //     }
        //
        //     
        //     if (Faction != Faction.OfPlayer || Dead || Dead)
        //     {
        //         yield break;
        //     }
        //
        //     if (compTransporterPawn.LoadingInProgressOrReadyToLaunch)
        //     {
        //         var command_Action = new Command_Action
        //         {
        //             defaultLabel = "CommandLaunchGroup".Translate(),
        //             defaultDesc = "CommandLaunchGroupDesc".Translate(),
        //             icon = ContentFinder<Texture2D>.Get(itemPath: "UI/Icons/Commands/FlyingTarget"),
        //             action = delegate
        //             {
        //                 if (compTransporterPawn.AnyInGroupHasAnythingLeftToLoad)
        //                 {
        //                     Find.WindowStack.Add(window: Dialog_MessageBox.CreateConfirmation(
        //                         text: "ConfirmSendNotCompletelyLoadedPods".Translate(
        //                             arg1: compTransporterPawn.FirstThingLeftToLoadInGroup.LabelCap
        //                         ), confirmedAct: compLaunchablePawn.StartChoosingDestination));
        //                 }
        //                 else
        //                 {
        //                     compLaunchablePawn.StartChoosingDestination();
        //                 }
        //             }
        //         };
        //         if (compLaunchablePawn.AnyInGroupIsUnderRoof)
        //         {
        //             command_Action.Disable(reason: "CommandLaunchGroupFailUnderRoof".Translate());
        //         }
        //
        //         yield return command_Action;
        //     }
        //
        //     if (compTransporterPawn.LoadingInProgressOrReadyToLaunch)
        //     {
        //         yield return new Command_Action
        //         {
        //             defaultLabel = "CommandCancelLoad".Translate(),
        //             defaultDesc = "CommandCancelLoadDesc".Translate(),
        //             icon = CompTransporterPawn.CancelLoadCommandTex,
        //             action = delegate
        //             {
        //                 SoundDefOf.Designate_Cancel.PlayOneShotOnCamera();
        //                 compTransporterPawn.CancelLoad();
        //             }
        //         };
        //     }
        //
        //     var command_LoadToTransporter = new Command_LoadToTransporterPawn();
        //     var num = 0;
        //     for (var i = 0; i < Find.Selector.NumSelected; i++)
        //     {
        //         if (Find.Selector.SelectedObjectsListForReading[index: i] is not Thing thing || thing.def != def)
        //         {
        //             continue;
        //         }
        //
        //         var compLaunchable = thing.TryGetComp<CompLaunchablePawn>();
        //         if (compLaunchable != null)
        //         {
        //             num++;
        //         }
        //     }
        //
        //     command_LoadToTransporter.defaultLabel = "CommandLoadTransporter".Translate(
        //         arg1: num.ToString()
        //     );
        //     command_LoadToTransporter.defaultDesc = "CommandLoadTransporterDesc".Translate();
        //     command_LoadToTransporter.icon = CompTransporterPawn.LoadCommandTex;
        //     command_LoadToTransporter.transComp = compTransporterPawn;
        //     var launchable = compTransporterPawn.Launchable;
        //     yield return command_LoadToTransporter;
        // }
    }
}