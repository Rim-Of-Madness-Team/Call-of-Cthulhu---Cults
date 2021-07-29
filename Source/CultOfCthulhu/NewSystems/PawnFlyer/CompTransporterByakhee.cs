using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace CultOfCthulhu
	{
		[StaticConstructorOnStartup]
		public class CompTransporterByakhee : ThingComp, IThingHolder
		{
			public int groupID = -1;

			public ThingOwner innerContainer;

			public List<TransferableOneWay> leftToLoad;

			private bool notifiedCantLoadMore;

			private CompLaunchable cachedCompLaunchable;

			private CompShuttle cachedCompShuttle;

			public static readonly Texture2D CancelLoadCommandTex = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true);

			private static readonly Texture2D LoadCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/LoadTransporter", true);

			private static readonly Texture2D SelectPreviousInGroupCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/SelectPreviousTransporter", true);

			private static readonly Texture2D SelectAllInGroupCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/SelectAllTransporters", true);

			private static readonly Texture2D SelectNextInGroupCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/SelectNextTransporter", true);

			private List<Thing> tmpThings = new List<Thing>();

			private List<Pawn> tmpSavedPawns = new List<Pawn>();

			private static List<CompTransporterByakhee> tmpTransportersInGroup = new List<CompTransporterByakhee>();

			public CompProperties_Transporter Props
			{
				get
				{
					return (CompProperties_Transporter)this.props;
				}
			}

			public Map Map
			{
				get
				{
					return this.parent.MapHeld;
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

			public CompLaunchable Launchable
			{
				get
				{
					if (this.cachedCompLaunchable == null)
					{
						this.cachedCompLaunchable = this.parent.GetComp<CompLaunchable>();
					}
					return this.cachedCompLaunchable;
				}
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

			public Thing FirstThingLeftToLoad
			{
				get
				{
					if (this.leftToLoad == null)
					{
						return null;
					}
					for (int i = 0; i < this.leftToLoad.Count; i++)
					{
						if (this.leftToLoad[i].CountToTransfer != 0 && this.leftToLoad[i].HasAnyThing)
						{
							return this.leftToLoad[i].AnyThing;
						}
					}
					return null;
				}
			}

			public Thing FirstThingLeftToLoadInGroup
			{
				get
				{
					List<CompTransporterByakhee> list = this.TransportersInGroup(this.parent.Map);
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

			public bool AnyInGroupNotifiedCantLoadMore
			{
				get
				{
					List<CompTransporterByakhee> list = this.TransportersInGroup(this.parent.Map);
					for (int i = 0; i < list.Count; i++)
					{
						if (list[i].notifiedCantLoadMore)
						{
							return true;
						}
					}
					return false;
				}
			}

			public bool AnyPawnCanLoadAnythingNow
			{
				get
				{
					if (!this.AnythingLeftToLoad)
					{
						return false;
					}
					if (!this.parent.Spawned)
					{
						return false;
					}
					List<Pawn> allPawnsSpawned = this.parent.Map.mapPawns.AllPawnsSpawned;
					for (int i = 0; i < allPawnsSpawned.Count; i++)
					{
						if (allPawnsSpawned[i].CurJobDef == JobDefOf.HaulToTransporter)
						{
						CompTransporterByakhee transporter = ((JobDriver_HaulToTransporter)allPawnsSpawned[i].jobs.curDriver).Transporter;
							if (transporter != null && transporter.groupID == this.groupID)
							{
								return true;
							}
						}
						if (allPawnsSpawned[i].CurJobDef == JobDefOf.EnterTransporter)
						{
						CompTransporterByakhee transporter2 = ((JobDriver_EnterTransporter)allPawnsSpawned[i].jobs.curDriver).Transporter;
							if (transporter2 != null && transporter2.groupID == this.groupID)
							{
								return true;
							}
						}
					}
					List<CompTransporterByakhee> list = this.TransportersInGroup(this.parent.Map);
					for (int j = 0; j < allPawnsSpawned.Count; j++)
					{
						if (allPawnsSpawned[j].mindState.duty != null && allPawnsSpawned[j].mindState.duty.transportersGroup == this.groupID)
						{
						CompTransporterByakhee compTransporter = JobGiver_EnterTransporter.FindMyTransporter(list, allPawnsSpawned[j]);
							if (compTransporter != null && allPawnsSpawned[j].CanReach(compTransporter.parent, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn))
							{
								return true;
							}
						}
					}
					for (int k = 0; k < allPawnsSpawned.Count; k++)
					{
						if (allPawnsSpawned[k].IsColonist)
						{
							for (int l = 0; l < list.Count; l++)
							{
								if (LoadTransportersJobUtility.HasJobOnTransporter(allPawnsSpawned[k], list[l]))
								{
									return true;
								}
							}
						}
					}
					return false;
				}
			}

			public CompTransporterByakhee()
			{
				this.innerContainer = new ThingOwner<Thing>(this);
			}

			public override void PostExposeData()
			{
				base.PostExposeData();
				bool flag = !this.parent.SpawnedOrAnyParentSpawned;
				if (flag && Scribe.mode == LoadSaveMode.Saving)
				{
					this.tmpThings.Clear();
					this.tmpThings.AddRange(this.innerContainer);
					this.tmpSavedPawns.Clear();
					for (int i = 0; i < this.tmpThings.Count; i++)
					{
						Pawn pawn;
						if ((pawn = (this.tmpThings[i] as Pawn)) != null)
						{
							this.innerContainer.Remove(pawn);
							this.tmpSavedPawns.Add(pawn);
							if (!pawn.IsWorldPawn())
							{
								Log.Error("Trying to save a non-world pawn (" + pawn + ") as a reference in a transporter.");
							}
						}
					}
					this.tmpThings.Clear();
				}
				Scribe_Collections.Look<Pawn>(ref this.tmpSavedPawns, "tmpSavedPawns", LookMode.Reference, Array.Empty<object>());
				Scribe_Values.Look<int>(ref this.groupID, "groupID", 0, false);
				Scribe_Deep.Look<ThingOwner>(ref this.innerContainer, "innerContainer", new object[]
				{
				this
				});
				Scribe_Collections.Look<TransferableOneWay>(ref this.leftToLoad, "leftToLoad", LookMode.Deep, Array.Empty<object>());
				Scribe_Values.Look<bool>(ref this.notifiedCantLoadMore, "notifiedCantLoadMore", false, false);
				if (flag && (Scribe.mode == LoadSaveMode.PostLoadInit || Scribe.mode == LoadSaveMode.Saving))
				{
					for (int j = 0; j < this.tmpSavedPawns.Count; j++)
					{
						this.innerContainer.TryAdd(this.tmpSavedPawns[j], true);
					}
					this.tmpSavedPawns.Clear();
				}
			}

			public ThingOwner GetDirectlyHeldThings()
			{
				return this.innerContainer;
			}

			public void GetChildHolders(List<IThingHolder> outChildren)
			{
				ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
			}

			public override void CompTick()
			{
				base.CompTick();
				this.innerContainer.ThingOwnerTick(true);
				if (this.Props.restEffectiveness != 0f)
				{
					for (int i = 0; i < this.innerContainer.Count; i++)
					{
						Pawn pawn = this.innerContainer[i] as Pawn;
						if (pawn != null && !pawn.Dead && pawn.needs.rest != null)
						{
							pawn.needs.rest.TickResting(this.Props.restEffectiveness);
						}
					}
				}
				if (this.parent.IsHashIntervalTick(60) && this.parent.Spawned && this.LoadingInProgressOrReadyToLaunch && this.AnyInGroupHasAnythingLeftToLoad && !this.AnyInGroupNotifiedCantLoadMore && !this.AnyPawnCanLoadAnythingNow && (this.Shuttle == null || !this.Shuttle.Autoload))
				{
					this.notifiedCantLoadMore = true;
					Messages.Message("MessageCantLoadMoreIntoTransporters".Translate(this.FirstThingLeftToLoadInGroup.LabelNoCount, Faction.OfPlayer.def.pawnsPlural, this.FirstThingLeftToLoadInGroup), this.parent, MessageTypeDefOf.CautionInput, true);
				}
			}

			public List<CompTransporterByakhee> TransportersInGroup(Map map)
			{
				if (!this.LoadingInProgressOrReadyToLaunch)
				{
					return null;
				}
				GetTransportersInGroup(this.groupID, map, CompTransporterByakhee.tmpTransportersInGroup);
				return CompTransporterByakhee.tmpTransportersInGroup;
			}

			public static void GetTransportersInGroup(int transportersGroup, Map map, List<CompTransporterByakhee> outTransporters)
			{
				outTransporters.Clear();
				if (transportersGroup < 0)
				{
					return;
				}
				List<Thing> list = map.listerThings.ThingsOfDef(ThingDef.Named("Cults_TransportByakhee"));
				for (int i = 0; i < list.Count; i++)
				{
					CompTransporterByakhee compTransporter = list[i].TryGetComp<CompTransporterByakhee>();
					if (compTransporter.groupID == transportersGroup)
					{
						outTransporters.Add(compTransporter);
					}
				}
			}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
			{
				foreach (Gizmo gizmo in base.CompGetGizmosExtra())
				{
					yield return gizmo;
				}
				IEnumerator<Gizmo> enumerator = null;
				if (this.Shuttle != null && !this.Shuttle.ShowLoadingGizmos)
				{
					yield break;
				}
				if (this.LoadingInProgressOrReadyToLaunch)
				{
					if (this.Shuttle == null || !this.Shuttle.Autoload)
					{
						yield return new Command_Action
						{
							defaultLabel = "CommandCancelLoad".Translate(),
							defaultDesc = "CommandCancelLoadDesc".Translate(),
							icon = CompTransporter.CancelLoadCommandTex,
							action = delegate ()
							{
								SoundDefOf.Designate_Cancel.PlayOneShotOnCamera(null);
								this.CancelLoad();
							}
						};
					}
					if (!this.Props.max1PerGroup)
					{
						yield return new Command_Action
						{
							defaultLabel = "CommandSelectPreviousTransporter".Translate(),
							defaultDesc = "CommandSelectPreviousTransporterDesc".Translate(),
							icon = CompTransporterByakhee.SelectPreviousInGroupCommandTex,
							action = delegate ()
							{
								this.SelectPreviousInGroup();
							}
						};
						yield return new Command_Action
						{
							defaultLabel = "CommandSelectAllTransporters".Translate(),
							defaultDesc = "CommandSelectAllTransportersDesc".Translate(),
							icon = CompTransporterByakhee.SelectAllInGroupCommandTex,
							action = delegate ()
							{
								this.SelectAllInGroup();
							}
						};
						yield return new Command_Action
						{
							defaultLabel = "CommandSelectNextTransporter".Translate(),
							defaultDesc = "CommandSelectNextTransporterDesc".Translate(),
							icon = CompTransporterByakhee.SelectNextInGroupCommandTex,
							action = delegate ()
							{
								this.SelectNextInGroup();
							}
						};
					}
					if (this.Props.canChangeAssignedThingsAfterStarting && (this.Shuttle == null || !this.Shuttle.Autoload))
					{
						yield return new Command_LoadToTransporter
						{
							defaultLabel = "CommandSetToLoadTransporter".Translate(),
							defaultDesc = "CommandSetToLoadTransporterDesc".Translate(),
							icon = CompTransporterByakhee.LoadCommandTex,
							transComp = this
						};
					}
				}
				else
				{
					Command_LoadToTransporter command_LoadToTransporter = new Command_LoadToTransporter();
					if (this.Props.max1PerGroup)
					{
						if (this.Props.canChangeAssignedThingsAfterStarting)
						{
							command_LoadToTransporter.defaultLabel = "CommandSetToLoadTransporter".Translate();
							command_LoadToTransporter.defaultDesc = "CommandSetToLoadTransporterDesc".Translate();
						}
						else
						{
							command_LoadToTransporter.defaultLabel = "CommandLoadTransporterSingle".Translate();
							command_LoadToTransporter.defaultDesc = "CommandLoadTransporterSingleDesc".Translate();
						}
					}
					else
					{
						int num = 0;
						for (int i = 0; i < Find.Selector.NumSelected; i++)
						{
							Thing thing = Find.Selector.SelectedObjectsListForReading[i] as Thing;
							if (thing != null && thing.def == this.parent.def)
							{
								CompLaunchable compLaunchable = thing.TryGetComp<CompLaunchable>();
								if (compLaunchable == null || (compLaunchable.FuelingPortSource != null && compLaunchable.FuelingPortSourceHasAnyFuel))
								{
									num++;
								}
							}
						}
						command_LoadToTransporter.defaultLabel = "CommandLoadTransporter".Translate(num.ToString());
						command_LoadToTransporter.defaultDesc = "CommandLoadTransporterDesc".Translate();
					}
					command_LoadToTransporter.icon = CompTransporterByakhee.LoadCommandTex;
					command_LoadToTransporter.transComp = this;
					CompLaunchable launchable = this.Launchable;
					if (launchable != null)
					{
						if (!launchable.ConnectedToFuelingPort)
						{
							command_LoadToTransporter.Disable("CommandLoadTransporterFailNotConnectedToFuelingPort".Translate());
						}
						else if (!launchable.FuelingPortSourceHasAnyFuel)
						{
							command_LoadToTransporter.Disable("CommandLoadTransporterFailNoFuel".Translate());
						}
					}
					yield return command_LoadToTransporter;
				}
				yield break;
				yield break;
			}

			public override void PostDeSpawn(Map map)
			{
				base.PostDeSpawn(map);
				if (this.CancelLoad(map) && this.Shuttle == null)
				{
					if (this.Props.max1PerGroup)
					{
						Messages.Message("MessageTransporterSingleLoadCanceled_TransporterDestroyed".Translate(), MessageTypeDefOf.NegativeEvent, true);
					}
					else
					{
						Messages.Message("MessageTransportersLoadCanceled_TransporterDestroyed".Translate(), MessageTypeDefOf.NegativeEvent, true);
					}
				}
				this.innerContainer.TryDropAll(this.parent.Position, map, ThingPlaceMode.Near, null, null, true);
			}

			public override string CompInspectStringExtra()
			{
				return "Contents".Translate() + ": " + this.innerContainer.ContentsString.CapitalizeFirst();
			}

			public void AddToTheToLoadList(TransferableOneWay t, int count)
			{
				if (!t.HasAnyThing || count <= 0)
				{
					return;
				}
				if (this.leftToLoad == null)
				{
					this.leftToLoad = new List<TransferableOneWay>();
				}
				TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatching<TransferableOneWay>(t.AnyThing, this.leftToLoad, TransferAsOneMode.PodsOrCaravanPacking);
				if (transferableOneWay != null)
				{
					for (int i = 0; i < t.things.Count; i++)
					{
						if (!transferableOneWay.things.Contains(t.things[i]))
						{
							transferableOneWay.things.Add(t.things[i]);
						}
					}
					if (transferableOneWay.CanAdjustBy(count).Accepted)
					{
						transferableOneWay.AdjustBy(count);
						return;
					}
				}
				else
				{
					TransferableOneWay transferableOneWay2 = new TransferableOneWay();
					this.leftToLoad.Add(transferableOneWay2);
					transferableOneWay2.things.AddRange(t.things);
					transferableOneWay2.AdjustTo(count);
				}
			}

			public bool LeftToLoadContains(Thing thing)
			{
				if (this.leftToLoad == null)
				{
					return false;
				}
				for (int i = 0; i < this.leftToLoad.Count; i++)
				{
					for (int j = 0; j < this.leftToLoad[i].things.Count; j++)
					{
						if (this.leftToLoad[i].things[j] == thing)
						{
							return true;
						}
					}
				}
				return false;
			}

			public void Notify_ThingAdded(Thing t)
			{
				this.SubtractFromToLoadList(t, t.stackCount, true);
				if (this.parent.Spawned && this.Props.pawnLoadedSound != null && t is Pawn)
				{
					this.Props.pawnLoadedSound.PlayOneShot(new TargetInfo(this.parent.Position, this.parent.Map, false));
				}
				QuestUtility.SendQuestTargetSignals(this.parent.questTags, "ThingAdded", t.Named("SUBJECT"));
			}

			public void Notify_ThingRemoved(Thing t)
			{
				if (this.Props.pawnExitSound != null && t is Pawn)
				{
					this.Props.pawnExitSound.PlayOneShot(new TargetInfo(this.parent.Position, this.parent.Map, false));
				}
			}

			public void Notify_ThingAddedAndMergedWith(Thing t, int mergedCount)
			{
				this.SubtractFromToLoadList(t, mergedCount, true);
			}

			public bool CancelLoad()
			{
				CompShuttle shuttle = this.Shuttle;
				if (shuttle == null)
				{
					return this.CancelLoad(this.Map);
				}
				if (shuttle.shipParent != null)
				{
					shuttle.shipParent.ForceJob_DelayCurrent(ShipJobMaker.MakeShipJob(ShipJobDefOf.Unload));
					return true;
				}
				return this.CancelLoad(this.Map);
			}

			public bool CancelLoad(Map map)
			{
				if (!this.LoadingInProgressOrReadyToLaunch)
				{
					return false;
				}
				this.TryRemoveLord(map);
				List<CompTransporterByakhee> list = this.TransportersInGroup(map);
				for (int i = 0; i < list.Count; i++)
				{
					list[i].CleanUpLoadingVars(map);
				}
				this.CleanUpLoadingVars(map);
				return true;
			}

			public void TryRemoveLord(Map map)
			{
				if (!this.LoadingInProgressOrReadyToLaunch)
				{
					return;
				}
				Lord lord = TransporterUtility.FindLord(this.groupID, map);
				if (lord != null)
				{
					map.lordManager.RemoveLord(lord);
				}
			}

			public void CleanUpLoadingVars(Map map)
			{
				this.groupID = -1;
				this.innerContainer.TryDropAll(this.parent.Position, map, ThingPlaceMode.Near, null, null, true);
				if (this.leftToLoad != null)
				{
					this.leftToLoad.Clear();
				}
				CompShuttle shuttle = this.Shuttle;
				if (shuttle != null)
				{
					shuttle.CleanUpLoadingVars();
				}
			}

			public int SubtractFromToLoadList(Thing t, int count, bool sendMessageOnFinished = true)
			{
				if (this.leftToLoad == null)
				{
					return 0;
				}
				TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatchingDesperate(t, this.leftToLoad, TransferAsOneMode.PodsOrCaravanPacking);
				if (transferableOneWay == null)
				{
					return 0;
				}
				if (transferableOneWay.CountToTransfer <= 0)
				{
					return 0;
				}
				int num = Mathf.Min(count, transferableOneWay.CountToTransfer);
				transferableOneWay.AdjustBy(-num);
				if (transferableOneWay.CountToTransfer <= 0)
				{
					this.leftToLoad.Remove(transferableOneWay);
				}
				if (sendMessageOnFinished && !this.AnyInGroupHasAnythingLeftToLoad)
				{
					CompShuttle comp = this.parent.GetComp<CompShuttle>();
					if (comp == null || comp.AllRequiredThingsLoaded)
					{
						if (this.Props.max1PerGroup)
						{
							Messages.Message("MessageFinishedLoadingTransporterSingle".Translate(), this.parent, MessageTypeDefOf.TaskCompletion, true);
						}
						else
						{
							Messages.Message("MessageFinishedLoadingTransporters".Translate(), this.parent, MessageTypeDefOf.TaskCompletion, true);
						}
					}
				}
				return num;
			}

			private void SelectPreviousInGroup()
			{
				List<CompTransporterByakhee> list = this.TransportersInGroup(this.Map);
				int num = list.IndexOf(this);
				CameraJumper.TryJumpAndSelect(list[GenMath.PositiveMod(num - 1, list.Count)].parent);
			}

			private void SelectAllInGroup()
			{
				List<CompTransporterByakhee> list = this.TransportersInGroup(this.Map);
				Selector selector = Find.Selector;
				selector.ClearSelection();
				for (int i = 0; i < list.Count; i++)
				{
					selector.Select(list[i].parent, true, true);
				}
			}

			private void SelectNextInGroup()
			{
				List<CompTransporterByakhee> list = this.TransportersInGroup(this.Map);
				int num = list.IndexOf(this);
				CameraJumper.TryJumpAndSelect(list[(num + 1) % list.Count].parent);
			}
		}
	}

