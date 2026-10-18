using Brix.Game.AI;
using Brix.UI.Icons;
using UnityEngine;

namespace Brix.Game.Components;

internal class Furnace : CraftingStation
{
	[SerializeField]
	private FurnaceRecipeDisplay furnaceIngots;

	[SerializeField]
	private FurnaceEffects effects;

	protected override bool CanStackOrders => true;

	public override IconKey Icon => IconKeys._Furnace;

	public override string PieMenuTag => "Furnace";

	public override ProjectPriority PriorityWeights => ProjectPriority.Furnace;

	protected override void SetDefault()
	{
		base.AutoStore = true;
	}

	protected override void OnProcess(int ms)
	{
		effects.BurnForSeconds(ms / 1000);
	}

	protected override void OnQueueChanged()
	{
		base.OnQueueChanged();
		RefreshIngots();
	}

	private void RefreshIngots()
	{
		furnaceIngots.Set(base.recepteur.ContentDescription.Value<Iron>(), base.recepteur.ContentDescription.Value<Glass>(), craftingLabor.QueueCount());
	}

	protected override void OnInventoryChanged()
	{
		base.OnInventoryChanged();
		RefreshIngots();
	}
}
