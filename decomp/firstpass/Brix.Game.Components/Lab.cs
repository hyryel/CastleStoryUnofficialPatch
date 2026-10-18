using Assets.Standard_Assets.Brix.Game.Components.Crafting;
using Brix.Game.AI;
using Brix.Game.Locomotion;
using Brix.UI.Icons;
using UnityEngine;

namespace Brix.Game.Components;

internal class Lab : CraftingStation
{
	[SerializeField]
	private RecipeDisplay recipeDisplay;

	[SerializeField]
	private LabEffect effects;

	protected override bool CanStackOrders => false;

	public override IconKey Icon => IconKeys._Lab;

	public override string PieMenuTag => "Lab";

	public override ProjectPriority PriorityWeights => ProjectPriority.WorkBench;

	protected override void SetDefault()
	{
		base.AutoStore = true;
	}

	protected override void OnInventoryChanged()
	{
		recipeDisplay.FromStation(this);
	}

	protected override void OnQueueChanged()
	{
		recipeDisplay.FromStation(this);
	}

	protected override void OnWorkStarted()
	{
		effects.StartWork();
	}

	protected override void OnWorkEnded()
	{
		effects.EndWork();
	}

	public override Working.WorkStates WorkAnimation()
	{
		return Working.WorkStates.Lab;
	}
}
