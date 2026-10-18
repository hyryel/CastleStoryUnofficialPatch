using System;
using Brix.Game.Rules;
using Brix.UI.Icons;
using UnityEngine;

namespace Brix.Game.AI;

public class AutonomyStatus
{
	public enum Ids
	{
		waitForOrder = 0,
		waitForOrderDefensive = 5,
		holdingPosition = 10,
		instinctOnly = 20,
		lockedToOperable = 30,
		freeAgent = 40
	}

	private Ids id;

	private string description;

	private Func<Labor, Sprite> IconFor;

	private bool showMoveBorne;

	private bool canPassThrough;

	private bool canSwitchTask;

	private bool takeOrdersCurrentOperable;

	private bool takeOrdersFromIdleProject;

	private bool takeOrdersFromAnyOperable;

	private bool listenToMeleeInstinct;

	private bool listenToRangeInstinct;

	private bool defensive;

	private int pathfindingBudget = GeneralRules.DefaultSearchPathBudget;

	private Func<AutonomyStatus, AutonomyStatus> onPlayerToggle = (AutonomyStatus self) => self;

	private Func<AutonomyStatus, AutonomyStatus> onFired = (AutonomyStatus self) => self;

	private Func<AutonomyStatus, AutonomyStatus> onPlayerOrder = (AutonomyStatus self) => self;

	private Func<AutonomyStatus, AutonomyStatus> onTaskEnded = (AutonomyStatus self) => self;

	private Func<AutonomyStatus, AutonomyStatus> onInvalid = (AutonomyStatus self) => self;

	private Func<Occupation, AutonomyStatus> onChangeClass = (Occupation o) => o.GetDefaultAutonomyStatus();

	private Func<AutonomyStatus> onExpiration;

	private float expirationTime = float.MaxValue;

	private Func<Labor, bool> expiresFor = (Labor l) => false;

	private Func<Labor, bool> validFor = (Labor l) => true;

	private static AutonomyStatus waitForOrderDefensive = new AutonomyStatus
	{
		id = Ids.waitForOrderDefensive,
		description = "WaitForOrders",
		IconFor = (Labor l) => HoldingPosition.GetIcon(l),
		showMoveBorne = true,
		canPassThrough = true,
		canSwitchTask = false,
		takeOrdersCurrentOperable = false,
		takeOrdersFromIdleProject = false,
		takeOrdersFromAnyOperable = false,
		listenToMeleeInstinct = false,
		listenToRangeInstinct = false,
		defensive = true,
		pathfindingBudget = GeneralRules.DefaultSearchPathBudget,
		onTaskEnded = (AutonomyStatus self) => HoldingPosition,
		onPlayerOrder = (AutonomyStatus self) => WaitForOrdersDefensive,
		onPlayerToggle = (AutonomyStatus self) => InstinctOnly
	};

	private static AutonomyStatus waitForOrder = new AutonomyStatus
	{
		id = Ids.waitForOrder,
		description = "WaitForOrders",
		IconFor = (Labor l) => InstinctOnly.GetIcon(l),
		showMoveBorne = true,
		canPassThrough = true,
		canSwitchTask = false,
		takeOrdersCurrentOperable = false,
		takeOrdersFromIdleProject = false,
		takeOrdersFromAnyOperable = false,
		listenToMeleeInstinct = false,
		listenToRangeInstinct = false,
		pathfindingBudget = GeneralRules.DefaultSearchPathBudget,
		onTaskEnded = (AutonomyStatus self) => InstinctOnly,
		onPlayerOrder = (AutonomyStatus self) => waitForOrder,
		onPlayerToggle = (AutonomyStatus self) => HoldingPosition
	};

	private static AutonomyStatus holdingPosition = new AutonomyStatus
	{
		id = Ids.holdingPosition,
		description = "HoldingPosition",
		IconFor = (Labor l) => l.Profession.HoldIcon().Get32(),
		showMoveBorne = true,
		canPassThrough = false,
		canSwitchTask = false,
		takeOrdersCurrentOperable = false,
		takeOrdersFromIdleProject = false,
		takeOrdersFromAnyOperable = false,
		listenToMeleeInstinct = true,
		listenToRangeInstinct = false,
		defensive = true,
		pathfindingBudget = GeneralRules.DefaultSearchPathBudget,
		onPlayerOrder = (AutonomyStatus self) => WaitForOrdersDefensive,
		onPlayerToggle = (AutonomyStatus self) => InstinctOnly
	};

	private unsafe static AutonomyStatus instinctOnly = new AutonomyStatus
	{
		id = Ids.instinctOnly,
		description = "InstinctOnly",
		IconFor = (Labor l) => l.Profession.InstinctIcon().Get32(),
		showMoveBorne = true,
		canPassThrough = false,
		canSwitchTask = false,
		takeOrdersCurrentOperable = false,
		takeOrdersFromIdleProject = false,
		takeOrdersFromAnyOperable = false,
		listenToMeleeInstinct = true,
		listenToRangeInstinct = true,
		pathfindingBudget = GeneralRules.DefaultSearchPathBudget,
		onPlayerOrder = (AutonomyStatus self) => WaitForOrders,
		onPlayerToggle = (AutonomyStatus self) => HoldingPosition,
		expiresFor = (Labor l) => l.Profession.OccupationType == Occupation.Type.Builder,
		expirationTime = 3f,
		onExpiration = new Func<AutonomyStatus>(null, (nint)(delegate*<AutonomyStatus>)(&AutonomyStatus.get_FreeAgent))
	};

	private static AutonomyStatus lockedToOperable = new AutonomyStatus
	{
		id = Ids.lockedToOperable,
		description = "LockedToOperable",
		IconFor = (Labor l) => l.GetAssignationIcon(),
		validFor = (Labor l) => l.Occupation.CurrentOccupation != Occupation.Type.Archer && l.Occupation.CurrentOccupation != Occupation.Type.Knight,
		showMoveBorne = false,
		canPassThrough = true,
		canSwitchTask = true,
		takeOrdersCurrentOperable = true,
		takeOrdersFromIdleProject = true,
		takeOrdersFromAnyOperable = false,
		listenToMeleeInstinct = true,
		listenToRangeInstinct = true,
		pathfindingBudget = GeneralRules.FreeAgentSearchPathBudget,
		onInvalid = (AutonomyStatus self) => InstinctOnly,
		onFired = (AutonomyStatus self) => FreeAgent,
		onPlayerOrder = (AutonomyStatus self) => WaitForOrders,
		onPlayerToggle = (AutonomyStatus self) => HoldingPosition
	};

	private static AutonomyStatus freeAgent = new AutonomyStatus
	{
		id = Ids.freeAgent,
		description = "FreeAgent",
		IconFor = (Labor l) => IconKeys.Gear.Get32(),
		validFor = (Labor l) => l.Occupation.CurrentOccupation != Occupation.Type.Archer && l.Occupation.CurrentOccupation != Occupation.Type.Knight,
		showMoveBorne = false,
		canPassThrough = true,
		canSwitchTask = true,
		takeOrdersCurrentOperable = true,
		takeOrdersFromIdleProject = true,
		takeOrdersFromAnyOperable = true,
		listenToMeleeInstinct = true,
		listenToRangeInstinct = true,
		pathfindingBudget = GeneralRules.FreeAgentSearchPathBudget,
		onInvalid = (AutonomyStatus self) => InstinctOnly,
		onPlayerOrder = (AutonomyStatus self) => WaitForOrders,
		onPlayerToggle = (AutonomyStatus self) => HoldingPosition
	};

	public Ids Id => id;

	public string Description => description;

	public bool ShowMoveBorne => showMoveBorne;

	public bool CanPassThrough => canPassThrough;

	public bool CanSwitchTask => canSwitchTask;

	public bool TakeOrdersCurrentOperable => takeOrdersCurrentOperable;

	public bool TakeOrdersFromIdleProject => takeOrdersFromIdleProject;

	public bool TakeOrdersFromAnyOperable => takeOrdersFromAnyOperable;

	public bool ListenToMeleeInstinct => listenToMeleeInstinct;

	public bool ListenToRangeInstinct => listenToRangeInstinct;

	public bool ListenToInstinct => ListenToMeleeInstinct || ListenToRangeInstinct;

	public bool IsDefensive => defensive;

	public int PathfindingBudget => pathfindingBudget;

	public AutonomyStatus OnFiredFromOperable => onFired(this);

	public AutonomyStatus OnPlayerOrder => onPlayerOrder(this);

	public AutonomyStatus OnTaskEnded => onTaskEnded(this);

	public AutonomyStatus OnInvalid => onInvalid(this);

	public AutonomyStatus OnPlayerToggle => onPlayerToggle(this);

	public AutonomyStatus OnExpiration => onExpiration();

	public float ExpirationTime => Time.time + expirationTime;

	private static AutonomyStatus WaitForOrders => waitForOrder;

	private static AutonomyStatus WaitForOrdersDefensive => waitForOrderDefensive;

	public static AutonomyStatus HoldingPosition => holdingPosition;

	public static AutonomyStatus InstinctOnly => instinctOnly;

	public static AutonomyStatus LockedToOperable => lockedToOperable;

	public static AutonomyStatus FreeAgent => freeAgent;

	public Sprite GetIcon(Labor labor)
	{
		return IconFor(labor);
	}

	public AutonomyStatus OnChangeClass(Occupation o)
	{
		return onChangeClass(o);
	}

	public bool ExpiresFor(Labor labor)
	{
		return expiresFor(labor);
	}

	public bool ValidFor(Labor labor)
	{
		return validFor(labor);
	}

	public static AutonomyStatus FromId(Ids id)
	{
		return id switch
		{
			Ids.waitForOrder => WaitForOrders, 
			Ids.waitForOrderDefensive => WaitForOrdersDefensive, 
			Ids.holdingPosition => HoldingPosition, 
			Ids.instinctOnly => InstinctOnly, 
			Ids.lockedToOperable => LockedToOperable, 
			Ids.freeAgent => FreeAgent, 
			_ => throw new IndexOutOfRangeException(), 
		};
	}
}
