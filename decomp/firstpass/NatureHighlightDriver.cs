using Brix.Engine;
using Brix.Game.Components;
using Brix.Utils.Signals;
using UnityEngine;

public class NatureHighlightDriver : GameComponent
{
	private float TreeRadius = 1.5f;

	private float PlantRadius = 0.65f;

	protected override void Start()
	{
		base.Start();
		GameSignals.LateConnect(UIGameObserver.IsReady, ConnectToSignals);
	}

	private void ConnectToSignals()
	{
		UIGameObserver.dynamicObjects.TreeHovered.Connect(OnTreeHovered).DisconnectWhen(base.OnReleaseOrDestroySignal);
		UIGameObserver.dynamicObjects.TreeUnhovered.Connect(OnTreeUnhovered).DisconnectWhen(base.OnReleaseOrDestroySignal);
		UIGameObserver.dynamicObjects.PlantHovered.Connect(OnPlantHovered).DisconnectWhen(base.OnReleaseOrDestroySignal);
		UIGameObserver.dynamicObjects.PlantUnhovered.Connect(OnPlantUnhovered).DisconnectWhen(base.OnReleaseOrDestroySignal);
	}

	private void OnPlantHovered(Vector3 plantPosition)
	{
		SelectionCircle selectionCircle = SelectionCircleDispenser.Instance.AddOrGet(plantPosition);
		selectionCircle.Radius(PlantRadius);
		selectionCircle.Selected(value: true);
	}

	private void OnPlantUnhovered(Vector3 plantPosition)
	{
		SelectionCircleDispenser.Instance.Remove(plantPosition);
	}

	private void OnTreeHovered(Vector3 treePosition)
	{
		SelectionCircle selectionCircle = SelectionCircleDispenser.Instance.AddOrGet(treePosition);
		selectionCircle.Radius(TreeRadius);
		selectionCircle.Selected(value: true);
	}

	private void OnTreeUnhovered(Vector3 treePosition)
	{
		SelectionCircleDispenser.Instance.Remove(treePosition);
	}
}
