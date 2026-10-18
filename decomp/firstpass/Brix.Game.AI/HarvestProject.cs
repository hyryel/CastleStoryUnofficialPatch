using System.Collections.Generic;
using Brix.Engine;
using Brix.Engine.Nature;
using Brix.External.Factories;
using Brix.Game.Beacons;
using Brix.Game.Network;
using Brix.Input;
using Brix.Lua;
using MoonSharp.Interpreter;
using Motus.Behavior;
using Smooth.Algebraics;
using Smooth.Slinq;
using Smooth.Slinq.Collections;
using Smooth.Slinq.Context;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.AI;

public class HarvestProject : Project
{
	[SerializeField]
	private HarvestGoalProvider HarvestGoals;

	[SerializeField]
	private TreeGoalProvider TreeGoals;

	[SerializeField]
	private CleanUpGoalProvider CleanUpGoals;

	[SerializeField]
	private LitterGoalProvider LitterGoals;

	[SerializeField]
	private HarvestData data;

	public NatureType AcceptedTypes => data.AcceptedTypes;

	protected override Sprite BeaconSpriteOverride
	{
		get
		{
			Factory.AssetKey harvestMeta = HarvestData.GetHarvestMeta(AcceptedTypes);
			return Factory.Peek(harvestMeta).GetComponent<BeaconMeta>().sprite;
		}
	}

	private bool GetNatureTypeFlag(NatureType type)
	{
		return data.AcceptedTypes.HasFlag(type);
	}

	protected override void Awake()
	{
		base.Awake();
		AssignationSoundFunc = (Labor labor) => labor.Sounds.ActionClean;
		SetGoalSelectionModel(GoalSelector.Type.GoodEnoughInTypeCategory);
		if (NetworkServer.active && !data.Initialized)
		{
			User.LocalUser.GetComponent<UNetProjectCmd>().CallCmdHarvestToggleResource(base.gameObject, (int)AcceptedTypes);
		}
		data.NetworkUpdate.Connect(delegate(HarvestProject self)
		{
			self.OpenCloseProviders();
		}, this);
		data.NetworkUpdate.Connect(delegate
		{
			GameSignals.Invoke(PickingObserver.lua_OverridePickerSetWithOptions);
		}, this);
		AddProvider(CleanUpGoals);
		AddProvider(LitterGoals);
		AddProvider(HarvestGoals);
		AddProvider(TreeGoals);
	}

	public override void OnBecameEmpty()
	{
		base.OnBecameEmpty();
		CheckForAutoDelete();
	}

	private void CheckForAutoDelete()
	{
		if (!AutoDeleteDisabled && !base.selection && base.faction != null && base.faction.OwnerSyncList.Count <= 1 && CrewCount() == 0 && HarvestGoals.GoalCount() <= 0 && CleanUpGoals.GoalCount() <= 0 && TreeGoals.GoalCount() <= 0 && LitterGoals.GoalCount() <= 0)
		{
			RequestDelete();
		}
	}

	private void OpenCloseProviders()
	{
		HarvestGoals.SetAcceptedTypes(AcceptedTypes);
		if (GetComponent<BeaconDriver>().SetMeta(HarvestData.GetHarvestMeta(AcceptedTypes)))
		{
			OnIconChanged();
		}
		TreeGoals.active = AcceptedTypes.HasFlag(NatureType.tree);
	}

	public override bool HasCleanup()
	{
		return CleanUpGoals.GoalCount() > 0;
	}

	protected override Node SelectBestGoal(Labor labor, Var<Goal> result)
	{
		Slinq<Goal, ConcatContext<SelectContext<Goal, TreeGoal, SelectContext<TreeGoal, KeyValuePair<XYZ, TreeGoal>, DictionaryContext<XYZ, TreeGoal>>>, Goal, ConcatContext<SelectContext<Goal, NatureHarvestGoal, SelectContext<NatureHarvestGoal, KeyValuePair<XYZ, NatureHarvestGoal>, PredicateContext<KeyValuePair<XYZ, NatureHarvestGoal>, DictionaryContext<XYZ, NatureHarvestGoal>, NatureType>>>, Goal, ConcatContext<SelectContext<Goal, LitterGoal, IListContext<LitterGoal>>, Goal, SelectContext<Goal, CleanUpGoal, HashSetSlinqContext<CleanUpGoal>>>>>> goals = CleanUpGoals.All().Concat(LitterGoals.All()).Concat(HarvestGoals.All())
			.Concat(TreeGoals.All());
		SlinqEnumerator<Goal, PredicateContext<Goal, SelectContext<Goal, Goal, ConcatContext<SelectContext<Goal, TreeGoal, SelectContext<TreeGoal, KeyValuePair<XYZ, TreeGoal>, DictionaryContext<XYZ, TreeGoal>>>, Goal, ConcatContext<SelectContext<Goal, NatureHarvestGoal, SelectContext<NatureHarvestGoal, KeyValuePair<XYZ, NatureHarvestGoal>, PredicateContext<KeyValuePair<XYZ, NatureHarvestGoal>, DictionaryContext<XYZ, NatureHarvestGoal>, NatureType>>>, Goal, ConcatContext<SelectContext<Goal, LitterGoal, IListContext<LitterGoal>>, Goal, SelectContext<Goal, CleanUpGoal, HashSetSlinqContext<CleanUpGoal>>>>>>, Tuple<Project, Labor>>> enumerator = GoalsNeedingEvaluation(goals, labor).GetEnumerator();
		return SelectBestGoalPooledList(enumerator, labor, result);
	}

	public override Node VerifyWorkFor(Labor labor)
	{
		if (TreeGoals.GoalCount() > 0)
		{
			return Node.Empty;
		}
		return base.VerifyWorkFor(labor);
	}

	public void ToggleResource(NatureType type)
	{
		data.Toggle(type);
	}

	public void SetResource(NatureType type)
	{
		data.Set((int)type);
	}

	[LuaDrivable("ToggleNatureTypeFlag")]
	private static DynValue lua_ToggleNatureTypeFlag(ScriptExecutionContext context, CallbackArguments args)
	{
		HarvestProject harvestProject = args.AsComponent<HarvestProject>(0, "ToggleNatureTypeFlag");
		NatureType type = args.AsEnum<NatureType>(1, "ToggleNatureTypeFlag");
		User.LocalUser.GetComponent<UNetProjectCmd>().CallCmdHarvestToggleResource(harvestProject.gameObject, (int)type);
		return DynValue.Void;
	}

	[LuaDrivable("GetNatureTypeFlag")]
	private static DynValue lua_GetNatureTypeFlag(ScriptExecutionContext context, CallbackArguments args)
	{
		HarvestProject harvestProject = args.AsComponent<HarvestProject>(0, "GetNatureTypeFlag");
		NatureType flag = args.AsEnum<NatureType>(1, "GetNatureTypeFlag");
		return DynValue.NewBoolean(harvestProject.data.AcceptedTypes.HasFlag(flag));
	}
}
