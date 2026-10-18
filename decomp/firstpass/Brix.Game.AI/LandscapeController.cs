using Brix.Engine;
using Brix.Game.AI.UI;
using Brix.Game.Components;
using Brix.Game.Network;
using Brix.Input;
using Brix.Legacy;
using Brix.Utils.Signals;
using UnityEngine;

namespace Brix.Game.AI;

public class LandscapeController : UpdatableGameComponent<LandscapeController>
{
	[SerializeField]
	private LandscapeProjectUI ui;

	[SerializeField]
	private LandscapeProject project;

	[SerializeField]
	private LandscapeGoalProvider provider;

	private HandleGroup2<DragHandle> handleGroup;

	protected override void Start()
	{
		base.Start();
		ui.MaxSize = project.MaxSize;
		InitHandleGroup();
		ConnectSignals();
		OnProviderUpdated();
	}

	private void Update()
	{
		handleGroup.Refresh();
	}

	private void InitHandleGroup()
	{
		handleGroup = HandleGroup2<DragHandle>.Default();
		ui.AddHandleToGroup(handleGroup);
		project.Beacon.AddHandleToGroup(handleGroup);
		handleGroup.Connect((DragHandle h) => h.DragSignals, delegate(DragHandle h)
		{
			h.Showing = false;
		}, delegate(DragHandle h)
		{
			h.Showing = true;
		});
	}

	private void ConnectSignals()
	{
		provider.DrawingInfoReady.Connect(RedrawGoal).DisconnectWhen(base.OnReleaseOrDestroySignal);
		provider.Updated.Connect(OnProviderUpdated).DisconnectWhen(base.OnReleaseOrDestroySignal);
		ui.ThresholdDrag.End.Connect(delegate
		{
			UpdateProvider();
		}).DisconnectWhen(base.OnReleaseOrDestroySignal);
		ui.Drag.Connect(OnDragStart, null, OnDragEnd, base.OnReleaseOrDestroySignal);
		project.Beacon.PositionChange.Connect(OnBeaconMoveStart, OnBeaconMove, OnBeaconMoveEnd, base.OnReleaseOrDestroySignal);
		UIGameObserver.projects.Selected.Connect(delegate
		{
			RefreshVisibility();
		}).DisconnectWhen(base.OnReleaseOrDestroySignal);
	}

	private void OnProviderUpdated()
	{
		ui.FromProvider(provider);
		RecenterBeacon();
		RefreshVisibility();
		ui.ShowGoals = false;
	}

	private void RedrawGoal()
	{
		ui.GetGoalsFrom(provider);
		RefreshVisibility();
	}

	private void OnBeaconMoveStart(Beacon beacon)
	{
		ui.ShowThreshold = false;
		ui.ShowGoals = false;
	}

	private void OnBeaconMove(Beacon beacon)
	{
		ui.Move(beacon.position.ToVector3());
	}

	private void OnBeaconMoveEnd(Beacon beacon)
	{
		ui.ShowThreshold = true;
		ui.AcceptHover = true;
		UpdateProvider();
	}

	private void OnDragStart(LandscapeProjectUI ui)
	{
		ui.ShowGoals = false;
		ui.ShowThreshold = false;
	}

	private void OnDragEnd(LandscapeProjectUI ui)
	{
		ui.ShowThreshold = true;
		UpdateProvider();
	}

	private void UpdateProvider()
	{
		int thresholdY = ui.GetThresholdY();
		VoxelBounds voxelBounds = ui.GetVolume();
		Vector3 min = voxelBounds.min.ToVector3();
		Vector3 max = voxelBounds.max.ToVector3();
		User.LocalUser.GetComponent<UNetGroupe>().CallCmdSetLandscape(base.gameObject, min, max, thresholdY);
	}

	private void RefreshVisibility()
	{
		bool showing = project.faction.IsOwnedByMe && UIGameObserver.projects.CurrentSelected == project;
		ui.Showing = showing;
		base.enabled = showing;
	}

	private void RecenterBeacon()
	{
		if (!project.Beacon.IsNullOrReleased())
		{
			project.Beacon.SetGameplayPosition(ui.VolumeCenter().ToVector3());
		}
	}
}
