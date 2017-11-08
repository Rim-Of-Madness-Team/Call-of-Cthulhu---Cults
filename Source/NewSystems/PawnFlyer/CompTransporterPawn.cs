using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;
using RimWorld;
using System.Linq;

namespace CultOfCthulhu
{
    [StaticConstructorOnStartup]
    public class CompTransporterPawn : ThingComp, IThingHolder
    {
        public int groupID = -1;

        private ThingOwner innerContainer;

        public List<TransferableOneWay> leftToLoad;

        private CompLaunchablePawn cachedCompLaunchablePawn;

        public static readonly Texture2D CancelLoadCommandTex = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true);

        public static readonly Texture2D LoadCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/LoadTransporter", true);

        public static readonly Texture2D SelectPreviousInGroupCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/SelectPreviousTransporter", true);

        public static readonly Texture2D SelectAllInGroupCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/SelectAllTransporters", true);

        public static readonly Texture2D SelectNextInGroupCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/SelectNextTransporter", true);

        public static List<CompTransporterPawn> tmpTransportersInGroup = new List<CompTransporterPawn>();

        public CompProperties_TransporterPawn Props
        {
            get
            {
                return (CompProperties_TransporterPawn)this.props;
            }
        }

        public Map Map
        {
            get
            {
                return this.parent.MapHeld;
            }
        }

        public bool Spawned
        {
            get
            {
                return this.parent.Spawned;
            }
        }

        public bool AnythingLeftToLoad
        {
            get
            {
                return this.FirstThingLeftToLoad != null;
            }
        }

        public bool LoadingInProgressOrReadyToLaunch
        {
            get
            {
                return this.groupID >= 0;
            }
        }

        public bool AnyInGroupHasAnythingLeftToLoad
        {
            get
            {
                return this.FirstThingLeftToLoadInGroup != null;
            }
        }

        public CompLaunchablePawn Launchable
        {
            get
            {
                if (this.cachedCompLaunchablePawn == null)
                {
                    this.cachedCompLaunchablePawn = this.parent.GetComp<CompLaunchablePawn>();
                }
                return this.cachedCompLaunchablePawn;
            }
        }

        public Thing FirstThingLeftToLoad
        {
            get
            {
                if (this.leftToLoad == null)
                {
                    return null;
                }
                TransferableOneWay transferableOneWay = this.leftToLoad.Find((TransferableOneWay x) => x.CountToTransfer != 0 && x.HasAnyThing);
                if (transferableOneWay != null)
                {
                    return transferableOneWay.AnyThing;
                }
                return null;
            }
        }

        public Thing FirstThingLeftToLoadInGroup
        {
            get
            {
                List<CompTransporterPawn> list = this.TransportersInGroup(this.parent.Map);
                for (int i = 0; i < list.Count; i++)
                {
                    Thing firstThingLeftToLoad = list[i].FirstThingLeftToLoad;
                    if (firstThingLeftToLoad != null)
                    {
                        return firstThingLeftToLoad;
                    }
                }
                return null;
            }
        }

        public CompTransporterPawn()
        {
            this.innerContainer = new ThingOwner<Thing>(this, false);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref this.groupID, "groupID", 0, false);
            Scribe_Deep.Look<ThingOwner>(ref this.innerContainer, "innerContainer", new object[]
            {
                this
            });
            Scribe_Collections.Look<TransferableOneWay>(ref this.leftToLoad, "leftToLoad", LookMode.Deep, new object[0]);
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return this.innerContainer;
        }

        public IntVec3 GetPosition()
        {
            return this.parent.PositionHeld;
        }

        public Map GetMap()
        {
            return this.parent.MapHeld;
        }

        public List<CompTransporterPawn> TransportersInGroup(Map map)
        {
            if (!this.LoadingInProgressOrReadyToLaunch)
            {
                return null;
            }

            tmpTransportersInGroup.Clear();
            if (this.groupID < 0)
            {
                return null;
            }
            IEnumerable<Pawn> listSel = from Pawn pawns in map.mapPawns.AllPawnsSpawned
                                        where pawns is PawnFlyer
                                        select pawns;
            List<Pawn> list = new List<Pawn>(listSel);
            for (int i = 0; i < list.Count; i++)
            {
                CompTransporterPawn compTransporter = list[i].TryGetComp<CompTransporterPawn>();
                if (compTransporter.groupID == this.groupID)
                {
                    tmpTransportersInGroup.Add(compTransporter);
                }
            }

            return CompTransporterPawn.tmpTransportersInGroup;
        }

        [DebuggerHidden]
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            IEnumerator<Gizmo> enumerator = base.CompGetGizmosExtra().GetEnumerator();
            while (enumerator.MoveNext())
            {
                Gizmo current = enumerator.Current;
                yield return current;
            }
            if (this.LoadingInProgressOrReadyToLaunch)
            {
                yield return new Command_Action
                {
                    defaultLabel = "CommandCancelLoad".Translate(),
                    defaultDesc = "CommandCancelLoadDesc".Translate(),
                    icon = CompTransporterPawn.CancelLoadCommandTex,
                    action = delegate
                    {
                        SoundDefOf.DesignateCancel.PlayOneShotOnCamera();
                        this.CancelLoad();
                    }
                };
            }
            Command_LoadToTransporterPawn Command_LoadToTransporterPawn = new Command_LoadToTransporterPawn();
            int num = 0;
            for (int i = 0; i < Find.Selector.NumSelected; i++)
            {
                Thing thing = Find.Selector.SelectedObjectsListForReading[i] as Thing;
                if (thing != null && thing.def == this.parent.def)
                {
                    CompLaunchablePawn CompLaunchablePawn = thing.TryGetComp<CompLaunchablePawn>();
                    if (CompLaunchablePawn == null)
                    {
                        num++;
                    }
                }
            }
            Command_LoadToTransporterPawn.defaultLabel = "CommandLoadTransporter".Translate(new object[]
            {
                num.ToString()
            });
            Command_LoadToTransporterPawn.defaultDesc = "CommandLoadTransporterDesc".Translate();
            Command_LoadToTransporterPawn.icon = CompTransporterPawn.LoadCommandTex;
            Command_LoadToTransporterPawn.transComp = this;
            CompLaunchablePawn launchable = this.Launchable;
            //if (launchable != null)
            //{
            //    if (!launchable.ConnectedToFuelingPort)
            //    {
            //        Command_LoadToTransporterPawn.Disable("CommandLoadTransporterFailNotConnectedToFuelingPort".Translate());
            //    }
            //    else if (!launchable.FuelingPortSourceHasAnyFuel)
            //    {
            //        Command_LoadToTransporterPawn.Disable("CommandLoadTransporterFailNoFuel".Translate());
            //    }
            //}
            yield return Command_LoadToTransporterPawn;
            yield break;
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            if (this.CancelLoad(map))
            {
                Messages.Message("MessageTransportersLoadCanceled_TransporterDestroyed".Translate(), MessageTypeDefOf.NegativeEvent);
            }
        }

        public void AddToTheToLoadList(TransferableOneWay t, int count)
        {
            if (!t.HasAnyThing || t.CountToTransfer <= 0)
            {
                return;
            }
            if (this.leftToLoad == null)
            {
                this.leftToLoad = new List<TransferableOneWay>();
            }
            if (TransferableUtility.TransferableMatching<TransferableOneWay>(t.AnyThing, this.leftToLoad) != null)
            {
                Log.Error("Transferable already exists.");
                return;
            }
            TransferableOneWay transferableOneWay = new TransferableOneWay();
            this.leftToLoad.Add(transferableOneWay);
            transferableOneWay.things.AddRange(t.things);
            transferableOneWay.AdjustTo(count);
        }

        public void Notify_ThingAdded(Thing t)
        {
            this.SubtractFromToLoadList(t, t.stackCount);
        }

        public void Notify_PawnEnteredTransporterOnHisOwn(Pawn p)
        {
            this.SubtractFromToLoadList(p, 1);
        }

        public bool CancelLoad()
        {
            return this.CancelLoad(this.Map);
        }

        public bool CancelLoad(Map map)
        {
            if (!this.LoadingInProgressOrReadyToLaunch)
            {
                return false;
            }
            this.TryRemoveLord(map);
            List<CompTransporterPawn> list = this.TransportersInGroup(map);
            for (int i = 0; i < list.Count; i++)
            {
                list[i].CleanUpLoadingVars(map);
            }
            this.CleanUpLoadingVars(map);
            return true;
        }

        // RimWorld.TransporterUtility
        public static Lord FindLord(int transportersGroup, Map map)
        {
            List<Lord> lords = map.lordManager.lords;
            for (int i = 0; i < lords.Count; i++)
            {
                LordJob_LoadAndEnterTransportersPawn lordJob_LoadAndEnterTransporters = lords[i].LordJob as LordJob_LoadAndEnterTransportersPawn;
                if (lordJob_LoadAndEnterTransporters != null && lordJob_LoadAndEnterTransporters.transportersGroup == transportersGroup)
                {
                    return lords[i];
                }
            }
            return null;
        }

        public void TryRemoveLord(Map map)
        {
            if (!this.LoadingInProgressOrReadyToLaunch)
            {
                return;
            }
            Lord lord = FindLord(this.groupID, map);
            if (lord != null)
            {
                map.lordManager.RemoveLord(lord);
            }
        }

        public void CleanUpLoadingVars(Map map)
        {
            this.groupID = -1;
            this.innerContainer.TryDropAll(this.parent.Position, map, ThingPlaceMode.Near);
            if (this.leftToLoad != null)
            {
                this.leftToLoad.Clear();
            }
        }

        private void SubtractFromToLoadList(Thing t, int count)
        {
            if (this.leftToLoad == null)
            {
                return;
            }
            TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatching<TransferableOneWay>(t, this.leftToLoad);
            if (transferableOneWay == null)
            {
                return;
            }
            transferableOneWay.AdjustBy(-count);
            if (transferableOneWay.CountToTransfer <= 0)
            {
                this.leftToLoad.Remove(transferableOneWay);
            }
            if (!this.AnyInGroupHasAnythingLeftToLoad)
            {
                Messages.Message("MessageFinishedLoadingTransporters".Translate(), this.parent, MessageTypeDefOf.PositiveEvent);
            }
        }

        private void SelectPreviousInGroup()
        {
            List<CompTransporterPawn> list = this.TransportersInGroup(this.Map);
            int num = list.IndexOf(this);
            CameraJumper.TryJumpAndSelect(list[GenMath.PositiveMod(num - 1, list.Count)].parent);
        }

        private void SelectAllInGroup()
        {
            List<CompTransporterPawn> list = this.TransportersInGroup(this.Map);
            Selector selector = Find.Selector;
            selector.ClearSelection();
            for (int i = 0; i < list.Count; i++)
            {
                selector.Select(list[i].parent, true, true);
            }
        }

        private void SelectNextInGroup()
        {
            List<CompTransporterPawn> list = this.TransportersInGroup(this.Map);
            int num = list.IndexOf(this);
            CameraJumper.TryJumpAndSelect(list[(num + 1) % list.Count].parent);
        }
    }
}
