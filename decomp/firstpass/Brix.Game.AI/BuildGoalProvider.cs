using System;
using System.Collections;
using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.AI.KnowledgeSpace;
using Brix.Game.Components;
using Brix.Game.Network;
using Brix.Game.Rules;
using Brix.Game.Storage;
using Brix.Input;
using Brix.Lifecycle.Pooling;
using Brix.Lua;
using Brix.Network;
using Brix.Pathfinding.Test;
using Brix.UI.GameSelector;
using Brix.Utils;
using Brix.Utils.Collections;
using Brix.Utils.Signals;
using MoonSharp.Interpreter;
using Smooth.Algebraics;
using Smooth.Delegates;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.AI;

public class BuildGoalProvider : NetworkGameComponent, IBlueprintTracker, IGoalProvider, IBlueprintMaterialProvider, IObjectTracker
{
	public struct FulfillableEnumerator : IEnumerator<Goal>, IDisposable, IEnumerator
	{
		private readonly BuildGoalProvider provider;

		private readonly Labor labor;

		private readonly HashSet<Factory.AssetKey> fetcheableUnroll;

		private readonly BuildGoalFinder finder;

		private bool ready;

		private int pathfindingDirtyDelay;

		private int count;

		private int searched;

		object IEnumerator.Current => Current;

		public Goal Current { get; private set; }

		public FulfillableEnumerator(BuildGoalProvider provider, Labor labor)
		{
			this = default(FulfillableEnumerator);
			this.provider = provider;
			this.labor = labor;
			fetcheableUnroll = provider._laborFetchableBuffers.GetValueOrCreate(labor);
			fetcheableUnroll.Clear();
			if (labor.recepteur.IsEmpty())
			{
				Knowledge.Instance.UniqueResourcesReacheableFrom(labor.gameObject, fetcheableUnroll);
			}
			else
			{
				labor.recepteur.UniqueStoredItemsKeys(fetcheableUnroll);
			}
			finder = provider._laborFinderBuffers.GetValueOrCreate(labor);
			pathfindingDirtyDelay = 20;
		}

		public bool MoveNext()
		{
			if (!ready)
			{
				if (WaitForPathfinding())
				{
					return true;
				}
				finder.PrepareStep(labor, fetcheableUnroll, provider.project.goalParent.transform);
				ready = true;
				return true;
			}
			while (true)
			{
				if (count < finder.Results.Count)
				{
					BuildGoal buildGoal = finder.Results[count++];
					if (buildGoal.IsNullOrReleased())
					{
						continue;
					}
					Current = buildGoal;
					return true;
				}
				if (WaitForPathfinding())
				{
					Current = null;
					return true;
				}
				if (finder.SearchStep())
				{
					finder.ResultStep();
					return false;
				}
				searched += 10;
				if (searched >= GeneralRules.BuildProjectSearchPathBudget)
				{
					break;
				}
			}
			searched = 0;
			Current = null;
			return true;
		}

		private bool WaitForPathfinding()
		{
			return (TestPathfinding.Instance.DirtyVoxels.Count > 0 || TestPathfinding.Instance.QueuedVoxels.Count > 0) && pathfindingDirtyDelay-- > 0;
		}

		public void Reset()
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			fetcheableUnroll.Clear();
			finder.Clear();
		}
	}

	private LayeredGoals goals = new LayeredGoals();

	[SerializeField]
	private bool AutoCommit;

	[SerializeField]
	private GameObject Content;

	private Project project;

	[SerializeField]
	protected List<GameObject> allICanFetchForDebug = new List<GameObject>();

	private readonly Dictionary<Labor, HashSet<Factory.AssetKey>> _laborFetchableBuffers = new Dictionary<Labor, HashSet<Factory.AssetKey>>();

	private readonly Dictionary<Labor, BuildGoalFinder> _laborFinderBuffers = new Dictionary<Labor, BuildGoalFinder>();

	public Signal<Blueprint> _blueprintAddedSignal = new Signal<Blueprint>();

	private HashSet<Blueprint> blueprints = new HashSet<Blueprint>();

	public Signal<Blueprint> _blueprintRemovedSignal = new Signal<Blueprint>();

	private BuildGoalMaterialProvider materialProvider = new BuildGoalMaterialProvider();

	public bool GoalVisibles => InputModeController.IsInEditMode() && !InputModeController.PausedInEditMode() && faction == User.LocalUser.faction;

	public bool HasFocus => UIGameSelector.IsSelected(base.gameObject);

	public IReadOnlySignal<Blueprint> BlueprintAddedSignal => _blueprintAddedSignal;

	public HashSet<Blueprint> TrackedBlueprints => blueprints;

	public IReadOnlySignal<Blueprint> BlueprintRemovedSignal => _blueprintRemovedSignal;

	public override void OnRequested()
	{
		base.OnRequested();
		GameSignals.Connect(InputModeController.InputModeChanged, InputModeChanged).DisconnectWhen(base.OnReleaseOrDestroySignal);
		GameSignals.LateConnectOnce(GameSignals.LevelReady, delegate
		{
			OnLevelReady();
		});
		project = GetComponent<Project>();
		project.GoalMatchedSignal.Connect(ReactToAcceptedGoal).DisconnectWhen(base.OnReleaseOrDestroySignal);
	}

	private void OnLevelReady()
	{
		ToggleSharedMaterialFocus(value: false);
		RefreshAllBlueprintsVisiblity();
	}

	private void InputModeChanged(InputMode obj)
	{
		RefreshAllBlueprintsVisiblity();
	}

	public int GoalCount()
	{
		return blueprints.Count;
	}

	public void Commit()
	{
		foreach (Blueprint trackedBlueprint in TrackedBlueprints)
		{
			BuildGoal component = trackedBlueprint.GetComponent<BuildGoal>();
			if (component != null)
			{
				component.Commit();
			}
		}
	}

	public void DeleteUncommited()
	{
		List<BuildGoal> list = CollectionPool.RequestList<BuildGoal>();
		(from b in TrackedBlueprints.Slinq()
			select b.GetComponent<BuildGoal>() into g
			where !g.Commited
			select g).AddTo(list);
		list.ForEach(delegate(BuildGoal g)
		{
			ObjectPoolSingleton.Release(g.gameObject);
		});
		list.Dispose();
	}

	public bool HasUncommited()
	{
		return blueprints.Slinq().Any((Blueprint b) => !b.commited);
	}

	public void GetAll(ICollection<Goal> results)
	{
		goals.Slinq().Select((DelegateFunc<BuildGoal, Goal>)((BuildGoal g) => g)).AddTo(results);
	}

	public void ClearGoalsOutside(IVoxelVolume bounds)
	{
		List<BuildGoal> list = goals.Slinq().ToPooledList();
		foreach (BuildGoal item in list)
		{
			if (!bounds.Contains(item.voxel.position))
			{
				ObjectPoolSingleton.Release(item.gameObject);
			}
		}
		list.Dispose();
	}

	public FulfillableEnumerator FulfillableBy(Labor labor)
	{
		return new FulfillableEnumerator(this, labor);
	}

	public void ReactToAcceptedGoal(Labor labor, Goal goal)
	{
		BuildGoalFinder valueOrCreate = _laborFinderBuffers.GetValueOrCreate(labor);
		valueOrCreate.ConstrainBounds(goal.transform.position);
	}

	public Transform ContainerFor(GameObject item)
	{
		return Content.transform;
	}

	public void OnObjectTracked(GameObject blueprintGO)
	{
		Blueprint component = blueprintGO.GetComponent<Blueprint>();
		component.State |= PlacementFlags.placed;
		blueprints.Add(component);
		BuildGoal component2 = component.GetComponent<BuildGoal>();
		component2.Provider = this;
		component2.ConnectProject(project);
		if (component.State.HasFlag(PlacementFlags.buildable))
		{
			goals.Add(component2);
		}
		StateRenderer component3 = component.GetComponent<StateRenderer>();
		component3.Show(GoalVisibles);
		project.SignalCommitmentChange();
		RecepteurDisplay component4 = component2.Blueprint.GetComponent<RecepteurDisplay>();
		if ((bool)component4)
		{
			component2.Blueprint.VisibilitySignalConnection.Disconnect();
			component2.Blueprint.VisibilitySignalConnection = component2.Blueprint.recepteur.InventoryChanged.Connect(delegate(Tuple<BuildGoal, bool, BuildGoalProvider> tuple)
			{
				tuple._3.ToggleMaterialVisibility(tuple._1, tuple._2);
			}, Tuple.Create(component2, GoalVisibles, this)).DisconnectWhen(component2.Blueprint.OnReleaseOrDestroySignal);
		}
		component.RefreshVisual();
		if (AutoCommit)
		{
			component2.Commit();
		}
		_blueprintAddedSignal.Invoke(component);
	}

	public void ObjectUntracked(GameObject blueprintGO)
	{
		Blueprint component = blueprintGO.GetComponent<Blueprint>();
		blueprints.Remove(component);
		_blueprintRemovedSignal.Invoke(component.GetComponent<Blueprint>());
		project.SignalCommitmentChange();
		goals.Remove(blueprintGO.GetComponent<BuildGoal>());
	}

	public void OnBlueprintChangedState(Blueprint blueprint, PlacementFlags previousState, PlacementFlags nextState)
	{
		if (previousState.HasFlag(PlacementFlags.buildable) && !nextState.HasFlag(PlacementFlags.buildable))
		{
			goals.Remove(blueprint.GetComponent<BuildGoal>());
		}
		if (!previousState.HasFlag(PlacementFlags.buildable) && nextState.HasFlag(PlacementFlags.buildable))
		{
			goals.Add(blueprint.GetComponent<BuildGoal>());
		}
	}

	public Slinq<Goal, SelectContext<Goal, BuildGoal, HashSetSlinqContext<BuildGoal>>> All()
	{
		return goals.Slinq().Select((DelegateFunc<BuildGoal, Goal>)((BuildGoal g) => g));
	}

	public bool HasNonStorable()
	{
		return GoalCount() > 0;
	}

	public void UniqueItemsGiven(ref HashSet<Factory.AssetKey> keys)
	{
	}

	public void UniqueItemsNeeded(ref HashSet<Factory.AssetKey> keys)
	{
		goals.OneGoalPerAssetKey().ForEach(delegate(BuildGoal goal, HashSet<Factory.AssetKey> _keys)
		{
			goal.recepteur.BaseCapacity.UniqueRepresentatives(ref _keys);
		}, keys);
	}

	[LuaDrivable("GoalsCount")]
	private static DynValue GoalCount(ScriptExecutionContext context, CallbackArguments args)
	{
		BuildGoalProvider buildGoalProvider = args.AsComponent<BuildGoalProvider>(0, "GoalsCount");
		return DynValue.NewNumber(buildGoalProvider.blueprints.Count);
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		materialProvider.Clearcache();
	}

	public BlueprintMaterialData GetSharedMaterialForAsset(Factory.AssetKey asset, PlacementFlags mode)
	{
		return materialProvider.GetSharedMaterialForAsset(asset, mode);
	}

	public void ToggleSharedMaterialFocus(bool value)
	{
		materialProvider.ToggleSharedMaterialFocus(value);
	}

	public void RefreshAllBlueprintsVisiblity()
	{
		BuildGoal[] componentsInChildren = GetComponent<Project>().goalParent.GetComponentsInChildren<BuildGoal>();
		foreach (BuildGoal goal in componentsInChildren)
		{
			ToggleMaterialVisibility(goal, GoalVisibles);
		}
	}

	public void ToggleMaterialVisibility(BuildGoal goal, bool value)
	{
		bool value2 = (!goal.Blueprint.recepteur.IsEmpty() || value) && goal.faction == User.LocalUser.faction;
		goal.GetComponent<StateRenderer>().Show(value2);
	}

	GameObject IObjectTracker.get_gameObject()
	{
		return base.gameObject;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		bool flag2 = default(bool);
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
	}
}
