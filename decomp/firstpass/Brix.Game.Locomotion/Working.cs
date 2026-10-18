using System;
using System.Collections.Generic;
using Brix.External.Factories;

namespace Brix.Game.Locomotion;

[Serializable]
public class Working : LocomotionCommonState
{
	public enum WorkStates
	{
		None = 0,
		Catapult = 2,
		Hammer = 3,
		Axe = 4,
		Pick = 5,
		Harvest = 6,
		Resurect = 7,
		Workbench = 100,
		Forge = 101,
		Lab = 102,
		MachineShop = 103,
		ChoppingBlock = 104,
		Altar = 105
	}

	public static readonly Dictionary<WorkStates, Factory.AssetKey> RecommendedTools = new Dictionary<WorkStates, Factory.AssetKey>
	{
		{
			WorkStates.Axe,
			Accessories.Axe
		},
		{
			WorkStates.Hammer,
			Accessories.Hammer
		},
		{
			WorkStates.Pick,
			Accessories.Pick
		},
		{
			WorkStates.Catapult,
			Accessories.Hammer
		},
		{
			WorkStates.Harvest,
			null
		},
		{
			WorkStates.Resurect,
			null
		},
		{
			WorkStates.Workbench,
			Accessories.Hammer
		},
		{
			WorkStates.Forge,
			Accessories.Hammer
		},
		{
			WorkStates.Lab,
			Accessories.Hammer
		},
		{
			WorkStates.MachineShop,
			Accessories.Hammer
		},
		{
			WorkStates.ChoppingBlock,
			Accessories.Hammer
		},
		{
			WorkStates.Altar,
			Accessories.Hammer
		}
	};

	public void Work(WorkStates state)
	{
		base.controller.WorkState.Set((int)state);
	}

	public void WorkToward(WorkStates state, float deltaY)
	{
		base.controller.WorkState.Set((int)state);
		base.controller.WorkHeight.Set(deltaY);
	}

	public void EndWork()
	{
		base.controller.WorkState.Set(0);
	}

	public override void Exit()
	{
	}
}
