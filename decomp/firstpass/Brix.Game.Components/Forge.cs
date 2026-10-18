using Brix.Game.AI;
using Brix.Game.Components.Crafting;
using Brix.Game.Locomotion;
using Brix.UI.Icons;
using UnityEngine;

namespace Brix.Game.Components;

internal class Forge : CraftingStation
{
	[SerializeField]
	private RecipeDisplay recipeDisplay;

	[SerializeField]
	private GameObject hammer;

	[SerializeField]
	private ForgeEffects effects;

	protected override bool CanStackOrders => false;

	public override IconKey Icon => IconKeys._Forge;

	public override string PieMenuTag => "Forge";

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
		effects.Fire(value: true);
	}

	protected override void OnWorkEnded()
	{
		hammer.SetActive(value: true);
		effects.Fire(value: false);
	}

	public override Working.WorkStates WorkAnimation()
	{
		return Working.WorkStates.Forge;
	}
}
