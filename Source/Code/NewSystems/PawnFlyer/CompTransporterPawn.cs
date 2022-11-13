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
        public static readonly Texture2D CancelLoadCommandTex = ContentFinder<Texture2D>.Get(itemPath: "UI/Designators/Cancel");

        public static readonly Texture2D LoadCommandTex = ContentFinder<Texture2D>.Get(itemPath: "UI/Commands/LoadTransporter");

        public static readonly Texture2D SelectPreviousInGroupCommandTex =
            ContentFinder<Texture2D>.Get(itemPath: "UI/Commands/SelectPreviousTransporter");

        public static readonly Texture2D SelectAllInGroupCommandTex =
            ContentFinder<Texture2D>.Get(itemPath: "UI/Commands/SelectAllTransporters");

        public static readonly Texture2D SelectNextInGroupCommandTex =
            ContentFinder<Texture2D>.Get(itemPath: "UI/Commands/SelectNextTransporter");

        public static readonly List<CompTransporterPawn> tmpTransportersInGroup = new List<CompTransporterPawn>();

        private CompLaunchablePawn cachedCompLaunchablePawn;
        public int groupID = -1;

        private ThingOwner innerContainer;

        public List<TransferableOneWay> leftToLoad;
        
        private CompShuttle cachedCompShuttle;

        public CompTransporterPawn()
        {
            innerContainer = new ThingOwner<Thing>(owner: this, oneStackOnly: false);
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
                var transferableOneWay = leftToLoad?.Find(match: x => x.CountToTransfer != 0 && x.HasAnyThing);
                return transferableOneWay?.AnyThing;
            }
        }

        public Thing FirstThingLeftToLoadInGroup
        {
            get
            {
                var list = TransportersInGroup(map: parent.Map);
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
            ThingOwnerUtility.AppendThingHoldersFromThings(outThingsHolders: outChildren, container: GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(value: ref groupID, label: "groupID");
            Scribe_Deep.Look(target: ref innerContainer, label: "innerContainer", this);
            Scribe_Collections.Look(list: ref leftToLoad, label: "leftToLoad", lookMode: LookMode.Deep);
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
            var list = new List<Pawn>(collection: listSel);
            foreach (var pawn in list)
            {
                var compTransporter = pawn.TryGetComp<CompTransporterPawn>();
                if (compTransporter.groupID == groupID)
                {
                    tmpTransportersInGroup.Add(item: compTransporter);
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
                if (Find.Selector.SelectedObjectsListForReading[index: i] is not Thing thing || thing.def != parent.def)
                {
                    continue;
                }

                var CompLaunchablePawn = thing.TryGetComp<CompLaunchablePawn>();
                if (CompLaunchablePawn == null)
                {
                    num++;
                }
            }

            Command_LoadToTransporterPawn.defaultLabel = "PawnFlyer_CommandLoad".Translate(
                arg1: num.ToString()
            );
            Command_LoadToTransporterPawn.defaultDesc = "PawnFlyer_CommandLoadDesc".Translate();
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
            base.PostDeSpawn(map: map);
            if (CancelLoad(map: map))
            {
                Messages.Message(text: "MessageTransportersLoadCanceled_TransporterDestroyed".Translate(),
                    def: MessageTypeDefOf.NegativeEvent);
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

            if (TransferableUtility.TransferableMatching(thing: t.AnyThing, transferables: leftToLoad,
                mode: TransferAsOneMode.PodsOrCaravanPacking) != null)
            {
                Log.Error(text: "Transferable already exists.");
                return;
            }

            var transferableOneWay = new TransferableOneWay();
            leftToLoad.Add(item: transferableOneWay);
            transferableOneWay.things.AddRange(collection: t.things);
            transferableOneWay.AdjustTo(destination: count);
        }

        public void Notify_ThingAdded(Thing t)
        {
            SubtractFromToLoadList(t: t, count: t.stackCount);
        }

        public void Notify_PawnEnteredTransporterOnHisOwn(Pawn p)
        {
            SubtractFromToLoadList(t: p, count: 1);
        }

        public bool CancelLoad()
        {
            return CancelLoad(map: Map);
        }

        public bool CancelLoad(Map map)
        {
            if (!LoadingInProgressOrReadyToLaunch)
            {
                return false;
            }

            TryRemoveLord(map: map);
            var list = TransportersInGroup(map: map);
            foreach (var compTransporterPawn in list)
            {
                compTransporterPawn.CleanUpLoadingVars(map: map);
            }

            CleanUpLoadingVars(map: map);
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

            var lord = FindLord(transportersGroup: groupID, map: map);
            if (lord != null)
            {
                map.lordManager.RemoveLord(oldLord: lord);
            }
            
        }

        public void CleanUpLoadingVars(Map map)
        {
            groupID = -1;
            innerContainer.TryDropAll(dropLoc: parent.Position, map: map, mode: ThingPlaceMode.Near);
            leftToLoad?.Clear();
        }

        private void SubtractFromToLoadList(Thing t, int count)
        {
            if (leftToLoad == null)
            {
                return;
            }

            var transferableOneWay =
                TransferableUtility.TransferableMatching(thing: t, transferables: leftToLoad, mode: TransferAsOneMode.PodsOrCaravanPacking);
            if (transferableOneWay == null)
            {
                return;
            }

            transferableOneWay.AdjustBy(adjustment: -count);
            if (transferableOneWay.CountToTransfer <= 0)
            {
                leftToLoad.Remove(item: transferableOneWay);
            }

            if (!AnyInGroupHasAnythingLeftToLoad)
            {
                Messages.Message(text: "MessageFinishedLoadingTransporters".Translate(), lookTargets: parent,
                    def: MessageTypeDefOf.PositiveEvent);
            }
        }

        private void SelectPreviousInGroup()
        {
            var list = TransportersInGroup(map: Map);
            var num = list.IndexOf(item: this);
            CameraJumper.TryJumpAndSelect(target: list[index: GenMath.PositiveMod(x: num - 1, m: list.Count)].parent);
        }

        private void SelectAllInGroup()
        {
            var list = TransportersInGroup(map: Map);
            var selector = Find.Selector;
            selector.ClearSelection();
            foreach (var compTransporterPawn in list)
            {
                selector.Select(obj: compTransporterPawn.parent);
            }
        }

        private void SelectNextInGroup()
        {
            var list = TransportersInGroup(map: Map);
            var num = list.IndexOf(item: this);
            CameraJumper.TryJumpAndSelect(target: list[index: (num + 1) % list.Count].parent);
        }
        
        public CompShuttle Shuttle
        {
            get
            {
                if (this.cachedCompShuttle == null)
                {
                    this.cachedCompShuttle = this.parent.GetComp<CompShuttle>();
                }
                return this.cachedCompShuttle;
            }
        }
    }
}