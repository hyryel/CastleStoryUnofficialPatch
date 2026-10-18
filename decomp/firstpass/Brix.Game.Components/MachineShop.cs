using Assets.Standard_Assets.Brix.Game.Components.Crafting;
using Brix.Engine;
using Brix.Game.AI;
using Brix.Game.Locomotion;
using Brix.UI.Icons;
using UnityEngine;

namespace Brix.Game.Components;

internal class MachineShop : CraftingStation
{
	[SerializeField]
	private RecipeDisplay recipeDisplay;

	[SerializeField]
	private MachineShopAnimations effects;

	[SerializeField]
	private GameObject hammer;

	protected override bool CanStackOrders => false;

	public override IconKey Icon => IconKeys._Machine_Shop;

	public override string PieMenuTag => "MachineShop";

	public override ProjectPriority PriorityWeights => ProjectPriority.WorkBench;

	protected override void SetDefault()
	{
		base.AutoStore = true;
	}

	protected override void OnWorkStarted()
	{
		effects.Working(value: true);
		hammer.SetActive(value: false);
	}

	protected override void OnWorkEnded()
	{
		Neo.Relais(delegate
		{
			effects.Working(value: false);
		}, 1f, "MachineShop Lens");
		hammer.SetActive(value: true);
	}

	protected override void OnInventoryChanged()
	{
		recipeDisplay.FromStation(this);
	}

	protected override void OnQueueChanged()
	{
		recipeDisplay.FromStation(this);
	}

	public override Working.WorkStates WorkAnimation()
	{
		return Working.WorkStates.MachineShop;
	}
}
