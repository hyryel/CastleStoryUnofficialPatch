using Brix.Game.AI;
using Brix.UI.Icons;
using UnityEngine;

namespace Brix.Game.Components;

internal class Workbench : CraftingStation
{
	[SerializeField]
	private RecipeDisplay recipeDisplay;

	[SerializeField]
	private GameObject hammer;

	protected override bool CanStackOrders => false;

	public override IconKey Icon => IconKeys._Workbench;

	public override string PieMenuTag => "Workbench";

	public override ProjectPriority PriorityWeights => ProjectPriority.WorkBench;

	protected override void SetDefault()
	{
		base.AutoStore = true;
	}

	protected override void OnQueueChanged()
	{
		recipeDisplay.FromStation(this);
	}

	protected override void OnInventoryChanged()
	{
		recipeDisplay.FromStation(this);
	}

	protected override void OnWorkStarted()
	{
		hammer.SetActive(value: false);
	}

	protected override void OnWorkEnded()
	{
		hammer.SetActive(value: true);
	}
}
