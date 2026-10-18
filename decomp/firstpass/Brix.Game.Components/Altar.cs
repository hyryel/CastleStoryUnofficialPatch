using Brix.Game.AI;
using Brix.Game.Locomotion;
using Brix.UI.Icons;
using UnityEngine;

namespace Brix.Game.Components;

internal class Altar : CraftingStation
{
	[SerializeField]
	private RecipeDisplay recipeDisplay;

	[SerializeField]
	private AltarEffects effects;

	protected override bool CanStackOrders => false;

	public override IconKey Icon => IconKeys._Altar;

	public override string PieMenuTag => "Altar";

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
		effects.StarWork();
	}

	protected override void OnWorkEnded()
	{
		effects.EndWork();
	}

	public override Working.WorkStates WorkAnimation()
	{
		return Working.WorkStates.Altar;
	}
}
