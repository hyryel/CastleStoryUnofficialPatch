using Brix.External.Factories;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Game.Locomotion;
using Brix.UI.Icons;
using UnityEngine;

namespace Assets.Standard_Assets.Brix.Game.Components;

internal class ChoppingBlock : CraftingStation
{
	public enum ChoppingState
	{
		empty,
		blueprint,
		log,
		plank
	}

	private const string Sound_PlaceLog = "ChoppingBlock_PlaceLog";

	private const string Sound_AxeHit = "ChoppingBlock_Axe_Hit";

	[SerializeField]
	private Animator animator;

	protected override bool CanStackOrders => false;

	public override IconKey Icon => IconKeys._Chopping_Block;

	public override string PieMenuTag => "ChoppingBlock";

	public override ProjectPriority PriorityWeights => ProjectPriority.Furnace;

	protected override void SetDefault()
	{
		craftingLabor.ProduceRecipe(CookBook.Planks);
		base.AutoStore = true;
	}

	protected override void OnInventoryChanged()
	{
		RefreshDisplay();
		if (base.recepteur.Has(Blocks.Wood_DoubleLog_AA))
		{
			PlaySound("ChoppingBlock_PlaceLog");
		}
	}

	protected override void OnQueueChanged()
	{
		RefreshDisplay();
	}

	protected override void OnWork()
	{
		animator.SetTrigger("Shake");
		PlaySound("ChoppingBlock_Axe_Hit");
	}

	private void RefreshDisplay()
	{
		if (base.recepteur.Carries(Adjectif.plankBlock))
		{
			animator.SetInteger("State", 3);
		}
		else if (base.recepteur.Carries(Adjectif.woodBlock))
		{
			animator.SetInteger("State", 2);
		}
		else if (base.FirstRecipe != null)
		{
			animator.SetInteger("State", 1);
		}
		else
		{
			animator.SetInteger("State", 0);
		}
	}

	public override Working.WorkStates WorkAnimation()
	{
		return Working.WorkStates.Axe;
	}
}
