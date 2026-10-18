using System;
using Assets.Standard_Assets.Brix.Game.Components.Crafting;
using Brix.Game.AI;
using Brix.UI.Icons;
using UnityEngine;

namespace Brix.Game.Components;

internal class Loom : CraftingStation
{
	private const string Sound_WheelStart = "Loom_Spin_Wheel_Start";

	private const string Sound_WheelEnd = "Loom_Spin_Wheel_Stop";

	[SerializeField]
	private LoomEffect loom;

	protected override bool CanStackOrders => false;

	public override IconKey Icon => IconKeys._Loom;

	public override string PieMenuTag => "Loom";

	public override ProjectPriority PriorityWeights => ProjectPriority.WorkBench;

	protected override void SetDefault()
	{
		base.AutoStore = true;
	}

	protected override void OnQueueChanged()
	{
		loom.RefreshContent(base.recepteur, base.FirstRecipe);
	}

	protected override void OnInventoryChanged()
	{
		loom.RefreshContent(base.recepteur, base.FirstRecipe);
	}

	protected override void OnWorkStarted()
	{
		loom.Work(base.FirstRecipe);
		PlaySound("Loom_Spin_Wheel_Start");
	}

	protected override void OnWorkEnded()
	{
		loom.StopWork();
		PlaySound("Loom_Spin_Wheel_Stop");
	}

	protected override Func<Description, GameObject> FindResourceFunc(Labor labor)
	{
		return labor.FindComplete;
	}
}
