using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace CultOfCthulhu
{
    [StaticConstructorOnStartup]
    public class CompTransporterPawn : ThingComp, IThingHolder
    {
        public static readonly Texture2D CancelLoadCommandTex = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");

        public static readonly Texture2D LoadCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/LoadTransporter");

        public static readonly Texture2D SelectPreviousInGroupCommandTex =
            ContentFinder<Texture2D>.Get("UI/Commands/SelectPreviousTransporter");

        public static readonly Texture2D SelectAllInGroupCommandTex =
            ContentFinder<Texture2D>.Get("UI/Commands/SelectAllTransporters");

        public static readonly Texture2D SelectNextInGroupCommandTex =
            ContentFinder<Texture2D>.Get("UI/Commands/SelectNextTransporter");

        public static readonly List<CompTransporterPawn> tmpTransportersInGroup = new List<CompTransporterPawn>();

        private CompLaunchablePawn cachedCompLaunchablePawn;
        public int groupID = -1;

        private ThingOwner innerContainer;

        public List<TransferableOneWay> leftToLoad;

        public CompTransporterPawn()
        {
            innerContainer = new ThingOwner<Thing>(this, false);
        }

        public CompProperties_TransporterPawn Props => (CompProperties_TransporterPawn) props;

        public Map Map => parent.MapHeld;

        public bool Spawned => parent.Spawned;

        public bool AnythingLeftToLoad => FirstThingLeftToLoad != null;

        public bool LoadingInProgressOrReadyToLaunch => groupID >= 0;

        public bool AnyInGroupHasAnythingLeftToLoad => FirstThingLeftToLoadInGroup != null;

        public CompLaunchablePawn Launchable
        {
            get
            {
                if (cachedCompLaunchablePawn == null)
                {
                    cachedCompLaunchablePawn = parent.GetComp<CompLaunchablePawn>();
                }

                return cachedCompLaunchablePawn;
            }
        }

        public Thing FirstThingLeftToLoad
        {
            get
            {
                var transferableOneWay = leftToLoad?.Find(x => x.CountToTransfer != 0 && x.HasAnyThing);
                return transferableOneWay?.AnyThing;
            }
        }

        public Thing FirstThingLeftToLoadInGroup
        {
            get
            {
                var list = TransportersInGroup(parent.Map);
                foreach (var compTransporterPawn in list)
                {
                    var firstThingLeftToLoad = compTransporterPawn.FirstThingLeftToLoad;
                    if (firstThingLeftToLoad != null)
                    {
                        return firstThingLeftToLoad;
                    }
                }

                return null;
            }
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref groupID, "groupID");
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
            Scribe_Collections.Look(ref leftToLoad, "leftToLoad", LookMode.Deep);
        }

        public IntVec3 GetPosition()
        {
            return parent.PositionHeld;
        }

        public Map GetMap()
        {
            return parent.MapHeld;
        }

        public List<CompTransporterPawn> TransportersInGroup(Map map)
        {
            if (!LoadingInProgressOrReadyToLaunch)
            {
                return null;
            }

            tmpTransportersInGroup.Clear();
            if (groupID < 0)
            {
                return null;
            }

            var listSel = from Pawn pawns in map.mapPawns.AllPawnsSpawned
                where pawns is PawnFlyer
                select pawns;
            var list = new List<Pawn>(listSel);
            foreach (var pawn in list)
            {
                var compTransporter = pawn.TryGetComp<CompTransporterPawn>();
                if (compTransporter.groupID == groupID)
                {
                    tmpTransportersInGroup.Add(compTransporter);
                }
            }

            return tmpTransportersInGroup;
        }

        [DebuggerHidden]
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            using var enumerator = base.CompGetGizmosExtra().GetEnumerator();
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                yield return current;
            }

            if (LoadingInProgressOrReadyToLaunch)
            {
                yield return new Command_Action
                {
                    defaultLabel = "CommandCancelLoad".Translate(),
                    defaultDesc = "CommandCancelLoadDesc".Translate(),
                    icon = CancelLoadCommandTex,
                    action = delegate
                    {
                        SoundDefOf.Designate_Cancel.PlayOneShotOnCamera();
                        CancelLoad();
                    }
                };
            }

            var Command_LoadToTransporterPawn = new Command_LoadToTransporterPawn();
            var num = 0;
            for (var i = 0; i < Find.Selector.NumSelected; i++)
            {
                if (Find.Selector.SelectedObjectsListForReading[i] is not Thing thing || thing.def != parent.def)
                {
                    continue;
                }

                var CompLaunchablePawn = thing.TryGetComp<CompLaunchablePawn>();
                if (CompLaunchablePawn == null)
                {
                    num++;
                }
            }

            Command_LoadToTransporterPawn.defaultLabel = "CommandLoadTransporter".Translate(
                num.ToString()
            );
            Command_LoadToTransporterPawn.defaultDesc = "CommandLoadTransporterDesc".Translate();
            Command_LoadToTransporterPawn.icon = LoadCommandTex;
            Command_LoadToTransporterPawn.transComp = this;
            var launchable = Launchable;
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
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            if (CancelLoad(map))
            {
                Messages.Message("MessageTransportersLoadCanceled_TransporterDestroyed".Translate(),
                    MessageTypeDefOf.NegativeEvent);
            }
        }

        public void AddToTheToLoadList(TransferableOneWay t, int count)
        {
            if (!t.HasAnyThing || t.CountToTransfer <= 0)
            {
                return;
            }

            if (leftToLoad == null)
            {
                leftToLoad = new List<TransferableOneWay>();
            }

            if (TransferableUtility.TransferableMatching(t.AnyThing, leftToLoad,
                TransferAsOneMode.PodsOrCaravanPacking) != null)
            {
                Log.Error("Transferable already exists.");
                return;
            }

            var transferableOneWay = new TransferableOneWay();
            leftToLoad.Add(transferableOneWay);
            transferableOneWay.things.AddRange(t.things);
            transferableOneWay.AdjustTo(count);
        }

        public void Notify_ThingAdded(Thing t)
        {
            SubtractFromToLoadList(t, t.stackCount);
        }

        public void Notify_PawnEnteredTransporterOnHisOwn(Pawn p)
        {
            SubtractFromToLoadList(p, 1);
        }

        public bool CancelLoad()
        {
            return CancelLoad(Map);
        }

        public bool CancelLoad(Map map)
        {
            if (!LoadingInProgressOrReadyToLaunch)
            {
                return false;
            }

            TryRemoveLord(map);
            var list = TransportersInGroup(map);
            foreach (var compTransporterPawn in list)
            {
                compTransporterPawn.CleanUpLoadingVars(map);
            }

            CleanUpLoadingVars(map);
            return true;
        }

        // RimWorld.TransporterUtility
        public static Lord FindLord(int transportersGroup, Map map)
        {
            var lords = map.lordManager.lords;
            foreach (var findLord in lords)
            {
                if (findLord.LordJob is LordJob_LoadAndEnterTransportersPawn lordJob_LoadAndEnterTransporters &&
                    lordJob_LoadAndEnterTransporters.transportersGroup == transportersGroup)
                {
                    return findLord;
                }
            }

            return null;
        }

        public void TryRemoveLord(Map map)
        {
            if (!LoadingInProgressOrReadyToLaunch)
            {
                return;
            }

            var lord = FindLord(groupID, map);
            if (lord != null)
            {
                map.lordManager.RemoveLord(lord);
            }
        }

        public void CleanUpLoadingVars(Map map)
        {
            groupID = -1;
            innerContainer.TryDropAll(parent.Position, map, ThingPlaceMode.Near);
            leftToLoad?.Clear();
        }

        private void SubtractFromToLoadList(Thing t, int count)
        {
            if (leftToLoad == null)
            {
                return;
            }

            var transferableOneWay =
                TransferableUtility.TransferableMatching(t, leftToLoad, TransferAsOneMode.PodsOrCaravanPacking);
            if (transferableOneWay == null)
            {
                return;
            }

            transferableOneWay.AdjustBy(-count);
            if (transferableOneWay.CountToTransfer <= 0)
            {
                leftToLoad.Remove(transferableOneWay);
            }

            if (!AnyInGroupHasAnythingLeftToLoad)
            {
                Messages.Message("MessageFinishedLoadingTransporters".Translate(), parent,
                    MessageTypeDefOf.PositiveEvent);
            }
        }

        private void SelectPreviousInGroup()
        {
            var list = TransportersInGroup(Map);
            var num = list.IndexOf(this);
            CameraJumper.TryJumpAndSelect(list[GenMath.PositiveMod(num - 1, list.Count)].parent);
        }

        private void SelectAllInGroup()
        {
            var list = TransportersInGroup(Map);
            var selector = Find.Selector;
            selector.ClearSelection();
            foreach (var compTransporterPawn in list)
            {
                selector.Select(compTransporterPawn.parent);
            }
        }

        private void SelectNextInGroup()
        {
            var list = TransportersInGroup(Map);
            var num = list.IndexOf(this);
            CameraJumper.TryJumpAndSelect(list[(num + 1) % list.Count].parent);
        }
    }
}