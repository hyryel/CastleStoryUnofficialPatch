using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Factories;
using Brix.Game.Network;
using Brix.Lua;
using MoonSharp.Interpreter;
using Motus.Behavior;
using Smooth.Algebraics;
using Smooth.Slinq.Collections;
using Smooth.Slinq.Context;
using UnityEngine;

namespace Brix.Game.AI;

public class MineProject : Project
{
	[SerializeField]
	private QuarryGoalProvider2 goalProvider;

	public static GameSignals.GameSignal ResourceTypeKeptChanged = new GameSignals.GameSignal();

	[LuaDrivable("hk_ResourceTypeKeptChanged")]
	protected static LuaHook hk_ResourceTypeKeptChanged = new LuaHook(ResourceTypeKeptChanged);

	public GroundResource TypesToHarvest => goalProvider.TypesToHarvest;

	protected override void Awake()
	{
		base.Awake();
		base.ConflictResolutionModel = ConflictResolver.ResolutionModel.Mining;
		SetGoalSelectionModel(GoalSelector.Type.CompareAll);
		AssignationSoundFunc = (Labor labor) => labor.Sounds.ActionMine;
		AddProvider(goalProvider);
	}

	protected override Node SelectBestGoal(Labor labor, Var<Goal> result)
	{
		SlinqEnumerator<Goal, PredicateContext<Goal, SelectContext<Goal, Goal, SelectContext<Goal, KeyValuePair<XYZ, SpawnOrder>, DictionaryContext<XYZ, SpawnOrder>>>, Tuple<Project, Labor>>> enumerator = GoalsNeedingEvaluation(goalProvider.Goals(), labor).GetEnumerator();
		return SelectBestGoalPooledList(enumerator, labor, result);
	}

	public override void Commit()
	{
		goalProvider.Commit();
		base.Commit();
	}

	public void ToggleResource(GroundResource type)
	{
		goalProvider.ToggleResource(type);
	}

	public void SetResource(GroundResource type)
	{
		goalProvider.SetResource(type);
	}

	public void RebuildDigGoalNodes()
	{
		goalProvider.RebuildDigGoalNodes();
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

	[LuaDrivable("ToggleResourceTypeFlag")]
	private static DynValue lua_ToggleResourceTypeFlag(ScriptExecutionContext context, CallbackArguments args)
	{
		MineProject mineProject = args.AsComponent<MineProject>(0, "ToggleResourceTypeFlag");
		int type = args.AsInt(1, "ToggleResourceTypeFlag");
		User.LocalUser.GetComponent<UNetProjectCmd>().CallCmdQuarryToggleResource(mineProject.gameObject, type);
		return DynValue.Void;
	}

	[LuaDrivable("GetResourceTypeFlag")]
	private static DynValue lua_GetResourceTypeFlag(ScriptExecutionContext context, CallbackArguments args)
	{
		MineProject mineProject = args.AsComponent<MineProject>(0, "GetResourceTypeFlag");
		int flag = args.AsInt(1, "GetResourceTypeFlag");
		return DynValue.NewBoolean(mineProject.TypesToHarvest.HasFlag((GroundResource)flag));
	}
}
