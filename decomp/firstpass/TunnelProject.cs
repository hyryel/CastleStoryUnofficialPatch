using System.Collections.Generic;
using System.Runtime.Serialization;
using Brix.Engine;
using Brix.Engine.Nature;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Game.Components.Tunnel;
using Brix.Game.Network;
using Brix.Legacy;
using Brix.Lua;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Motus.Behavior;
using Newtonsoft.Json;
using Smooth.Algebraics;
using Smooth.Slinq;
using Smooth.Slinq.Collections;
using Smooth.Slinq.Context;
using UnityEngine;
using UnityEngine.Networking;

public class TunnelProject : Project
{
	public Signal<Goal> goalDestroyedSignal = new Signal<Goal>();

	private VoxelGoalsStorage goalStorage;

	private VoxelGoalsStorage stairGoalsStorage;

	[SerializeField]
	public TunnelVolume tunnelVolume;

	[SerializeField]
	private TunnelData tunnelData;

	private bool ScheduledRefresh;

	[JsonProperty]
	private List<XYZ> UnitsPositionsList;

	public static GameSignals.GameSignal ResourceTypeKeptChanged = new GameSignals.GameSignal();

	private static LuaEnumDescriptor<TunnelType> _enumTunnelType = new LuaEnumDescriptor<TunnelType>();

	[LuaDrivable("stairs")]
	private static DynValue lua_TunnelTypes = _enumTunnelType.CreateDynValue();

	[LuaDrivable("hk_OnSetTunnelType")]
	protected static LuaHook hk_OnSetTunnelType = new LuaHook(TunnelVolume.ExpansionTunnelTypeChanged);

	[LuaDrivable("hk_ResourceTypeKeptChanged")]
	protected static LuaHook hk_ResourceTypeKeptChanged = new LuaHook(ResourceTypeKeptChanged);

	public Location DropPoint
	{
		get
		{
			Beacon point = GetPoint(Beacon.BalisePointsType.Drop);
			if (!point)
			{
				return null;
			}
			return Location.Of(point.transform.position);
		}
	}

	public GroundResource TypesToHarvest => tunnelData.TypesToHarvest;

	protected override void Awake()
	{
		base.Awake();
		base.ConflictResolutionModel = ConflictResolver.ResolutionModel.Mining;
		goalStorage = new VoxelGoalsStorage(tunnelVolume.Volume, base.goalParent, Test.DigGoal, GoalSetup, VoxelNeedsADigGoal);
		stairGoalsStorage = new VoxelGoalsStorage(tunnelVolume.Volume, base.goalParent, Test.BuildStairsGoal, StairGoalSetup, VoxelNeedsAStairGoal);
		SetGoalSelectionModel(GoalSelector.Type.CompareAll);
		goalStorage.DigGoalSetting = tunnelData;
		AssignationSoundFunc = (Labor labor) => labor.Sounds.ActionRepair;
		AddOrSetSequence((StaticNode)Node.Sequence.Do(delegate(TunnelProject self)
		{
			self.RefreshGoal();
		}, this), 100, "RefreshGoal");
		AddOrSetSequence((StaticNode)Node.Sequence.Do(delegate(TunnelProject self)
		{
			self.ApplyScheduledGoalRefresh();
		}, this), 1, "ApplyScheduledGoalRefresh");
		SelfDestructWhen(tunnelVolume.BecomesEmpty);
		AddProvider(stairGoalsStorage);
		AddProvider(goalStorage);
	}

	protected override void Start()
	{
		base.Start();
		Deserialize();
	}

	protected override Node SelectBestGoal(Labor labor, Var<Goal> result)
	{
		Slinq<Goal, ConcatContext<SelectContext<Goal, KeyValuePair<XYZ, Goal>, DictionaryContext<XYZ, Goal>>, Goal, SelectContext<Goal, KeyValuePair<XYZ, Goal>, DictionaryContext<XYZ, Goal>>>> goals = goalStorage.All().Concat(stairGoalsStorage.All());
		SlinqEnumerator<Goal, PredicateContext<Goal, SelectContext<Goal, Goal, ConcatContext<SelectContext<Goal, KeyValuePair<XYZ, Goal>, DictionaryContext<XYZ, Goal>>, Goal, SelectContext<Goal, KeyValuePair<XYZ, Goal>, DictionaryContext<XYZ, Goal>>>>, Tuple<Project, Labor>>> enumerator = GoalsNeedingEvaluation(goals, labor).GetEnumerator();
		return SelectBestGoalPooledList(enumerator, labor, result);
	}

	private void Deserialize()
	{
		if (!NetworkServer.active || UnitsPositionsList == null)
		{
			return;
		}
		foreach (XYZ unitsPositions in UnitsPositionsList)
		{
			GetComponent<TunnelNetworkDeserialization>().UnitsPositionsNetwork.Add(unitsPositions.ToVector3());
			tunnelVolume.Add(unitsPositions, Camera.main.transform.forward, TunnelType.straight, debug: false);
		}
	}

	[OnSerializing]
	internal void OnSerializing(StreamingContext context)
	{
		UnitsPositionsList = new List<XYZ>();
		Dictionary<XYZ, TunnelUnit> units = tunnelVolume.GetUnits();
		foreach (KeyValuePair<XYZ, TunnelUnit> item in units)
		{
			UnitsPositionsList.Add(item.Key);
		}
	}

	private void ApplyScheduledGoalRefresh()
	{
		if (ScheduledRefresh)
		{
			ScheduledRefresh = false;
			goalStorage.RefreshGoals();
			stairGoalsStorage.RefreshGoals();
		}
	}

	public void RefreshGoal()
	{
		ScheduledRefresh = true;
	}

	public void RebuildDigGoalNodes()
	{
		goalStorage.RebuildDigGoalNodes();
	}

	private void OnGoalDestroyed(Goal goal)
	{
		goalStorage.RemoveGoal(goal);
		goalDestroyedSignal.Invoke(goal);
	}

	private Vector3 StairsDirection(XYZ voxel)
	{
		if (!Voxel.IsNonEmpty(voxel - XYZ.up))
		{
			return Vector3.zero;
		}
		if (!tunnelVolume.Volume.Contains(voxel + XYZ.forward) && Voxel.IsNonEmpty(voxel + XYZ.forward) && tunnelVolume.Volume.Contains(voxel + XYZ.forward + XYZ.up))
		{
			return Vector3.forward;
		}
		if (!tunnelVolume.Volume.Contains(voxel + XYZ.right) && Voxel.IsNonEmpty(voxel + XYZ.right) && tunnelVolume.Volume.Contains(voxel + XYZ.right + XYZ.up))
		{
			return Vector3.right;
		}
		if (!tunnelVolume.Volume.Contains(voxel - XYZ.right) && Voxel.IsNonEmpty(voxel - XYZ.right) && tunnelVolume.Volume.Contains(voxel - XYZ.right + XYZ.up))
		{
			return Vector3.left;
		}
		if (!tunnelVolume.Volume.Contains(voxel - XYZ.forward) && Voxel.IsNonEmpty(voxel - XYZ.forward) && tunnelVolume.Volume.Contains(voxel - XYZ.forward + XYZ.up))
		{
			return Vector3.back;
		}
		return Vector3.zero;
	}

	private bool VoxelNeedsAStairGoal(XYZ voxel)
	{
		return !QuarryGoalProvider2.IsStair(voxel) && NeedStairs(voxel) && Voxel.IsEmpty(voxel) && Voxel.IsFull(voxel - XYZ.up) && !Voxel.IsInvincible(voxel);
	}

	private bool NeedStairs(XYZ voxel)
	{
		return StairsDirection(voxel) != Vector3.zero;
	}

	private static bool VoxelNeedsADigGoal(XYZ voxel)
	{
		if (!Voxel.IsTerrain(voxel) || Voxel.IsInvincible(voxel))
		{
			return false;
		}
		if (Voxel.IsEmpty(voxel) || !Voxel.HasEmptyNeighbor(voxel))
		{
			return false;
		}
		if (NatureRegistry.HasNatureBlockerAt(voxel) || NatureRegistry.HasNatureBlockerAt(voxel + XYZ.up))
		{
			return false;
		}
		return true;
	}

	private void GoalSetup(Goal goal)
	{
		DigGoal digGoal = goal as DigGoal;
		if (digGoal != null)
		{
			digGoal.UseHeightAdjustment = false;
		}
		goal.OnReleaseSignal.Connect(delegate(TunnelProject self, Goal g)
		{
			self.OnGoalDestroyed(g);
		}, this, goal);
	}

	private void StairGoalSetup(Goal goal)
	{
		goal.transform.forward = -StairsDirection(goal.voxel.position);
		GoalSetup(goal);
		goal.OnReleaseSignal.ConnectOnce(delegate(TunnelProject self, Goal g)
		{
			Highlighter.remove.At(g.voxel.position);
		}, this, goal);
	}

	public void ToggleResource(GroundResource type)
	{
		tunnelData.Toggle(type);
	}

	public void SetResource(GroundResource type)
	{
		tunnelData.Set((int)type);
	}

	protected override bool NeedsToStore()
	{
		return base.NeedsToStore() && TypesToHarvest != GroundResource.None;
	}

	protected override HashSet<Factory.AssetKey> ItemsRequiringStorage()
	{
		HashSet<Factory.AssetKey> hashSet = base.ItemsRequiringStorage();
		if (TypesToHarvest.HasFlag(GroundResource.Stone))
		{
			hashSet.Add(RawResources.RawStone);
		}
		if (TypesToHarvest.HasFlag(GroundResource.Iron))
		{
			hashSet.Add(RawResources.RawIron);
		}
		if (TypesToHarvest.HasFlag(GroundResource.Brimstone))
		{
			hashSet.Add(RawResources.RawOrangeCrystal);
		}
		return hashSet;
	}

	public override Node VerifyWorkFor(Labor labor)
	{
		if (!NeedsToStore())
		{
			return Node.Empty;
		}
		Var<bool> var = Var<bool>.Request(x: true);
		StaticNode verifyAllStorageNode = GetVerifyAllStorageNode(labor, var);
		return (StaticNode)Node.Sequence.Do(verifyAllStorageNode).Label("Evaluate project storing needs").Do((Var<bool> _result) => (!_result.Get()) ? Node.FailWork<Exceptions.NoCorrespondingStorage>() : Node.Empty, var)
			.Finally(delegate(Var<bool> _result)
			{
				_result.Dispose();
			}, var);
	}

	[LuaDrivable("SetTunnelType")]
	protected static DynValue lua_SetTunnelType(ScriptExecutionContext context, CallbackArguments args)
	{
		TunnelProject tunnelProject = args.AsComponent<TunnelProject>(0, "SetTunnelType");
		TunnelType expansionType = args.AsEnum<TunnelType>(1, "SetTunnelType");
		tunnelProject.tunnelVolume.SetExpansionType(expansionType);
		return DynValue.Void;
	}

	[LuaDrivable("GetTunnelType")]
	protected static DynValue lua_GetTunnelType(ScriptExecutionContext context, CallbackArguments args)
	{
		TunnelProject tunnelProject = args.AsComponent<TunnelProject>(0, "GetTunnelType");
		return DynValue.FromObject(context.OwnerScript, tunnelProject.tunnelVolume.GetExpansionType());
	}

	[LuaDrivable("ToggleResourceTypeFlag")]
	private static DynValue lua_ToggleResourceTypeFlag(ScriptExecutionContext context, CallbackArguments args)
	{
		TunnelProject tunnelProject = args.AsComponent<TunnelProject>(0, "ToggleResourceTypeFlag");
		int type = args.AsInt(1, "ToggleResourceTypeFlag");
		User.LocalUser.GetComponent<UNetProjectCmd>().CallCmdTunnelToggleResource(tunnelProject.gameObject, type);
		return DynValue.Void;
	}

	[LuaDrivable("GetResourceTypeFlag")]
	private static DynValue lua_GetResourceTypeFlag(ScriptExecutionContext context, CallbackArguments args)
	{
		TunnelProject tunnelProject = args.AsComponent<TunnelProject>(0, "GetResourceTypeFlag");
		int flag = args.AsInt(1, "GetResourceTypeFlag");
		return DynValue.NewBoolean(tunnelProject.TypesToHarvest.HasFlag((GroundResource)flag));
	}
}
