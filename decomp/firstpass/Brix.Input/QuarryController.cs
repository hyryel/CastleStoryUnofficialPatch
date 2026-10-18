using Brix.Engine;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Game.Network;
using Brix.Legacy;
using Brix.Utils.Signals;
using UnityEngine;

namespace Brix.Input;

public class QuarryController : UpdatableGameComponent<QuarryController>
{
	[SerializeField]
	private MineProject project;

	[SerializeField]
	private QuarryGoalProvider2 provider;

	[SerializeField]
	private QuarryProjectUI ui;

	private HandleGroup2<DragHandle> handleGroup;

	protected override float SlowUpdateRate => 5f;

	protected override void Start()
	{
		base.Start();
		ConnectToSignals();
		ui.MaxSize = project.MaxSize;
		InitHandleGroup();
		RefreshVisibility();
	}

	private void InitHandleGroup()
	{
		handleGroup = HandleGroup2<DragHandle>.Default();
		ui.AddHandlesToGroup(handleGroup);
		project.Beacon.AddHandleToGroup(handleGroup);
		handleGroup.Connect((DragHandle h) => h.DragSignals, delegate(DragHandle h)
		{
			h.Showing = false;
		}, delegate(DragHandle h)
		{
			h.Showing = true;
		});
	}

	private void Update()
	{
		handleGroup.Refresh();
	}

	private void ConnectToSignals()
	{
		UIGameObserver.projects.Selected.Connect(delegate
		{
			RefreshVisibility();
		}).DisconnectWhen(base.OnReleaseOrDestroySignal);
		ui.VolumeChange.end.Connect(delegate
		{
			UpdateProvider();
		}).DisconnectWhen(base.OnReleaseOrDestroySignal);
		ui.StairChanged.Connect(UpdateProvider).DisconnectWhen(base.OnReleaseOrDestroySignal);
		provider.Changed.Connect(UpdateUI).DisconnectWhen(base.OnReleaseOrDestroySignal);
		project.Beacon.PositionChange.Connect(OnPositionStartChanging, OnPositionChange, OnPositionDoneChanging, base.OnReleaseOrDestroySignal);
	}

	private void RefreshVisibility()
	{
		bool showing = UIGameObserver.projects.CurrentSelected == project && project.faction.IsOwnedByMe;
		project.Beacon.AcceptHover(!provider.Commited);
		ui.Showing = showing;
		base.enabled = showing;
	}

	private void UpdateProvider()
	{
		VoxelBounds bounds = ui.Bounds;
		StairOrientation stairOrientation = ui.StairOrientation;
		Vector3 boundsMin = bounds.min.ToVector3();
		Vector3 boundsMax = bounds.max.ToVector3();
		Vector3 stairCorner = stairOrientation.corner.ToVector3();
		Vector3 stairDirection = stairOrientation.direction.ToVector3();
		bool commited = provider.Commited;
		User.LocalUser.GetComponent<UNetGroupe>().CallCmdSetMine(base.gameObject, boundsMin, boundsMax, stairCorner, stairDirection, commited);
	}

	private void UpdateUI()
	{
		ui.FromProvider(provider);
		project.Beacon.AcceptHover(!provider.Commited);
		RecenterBeacon();
	}

	private void OnPositionStartChanging(Beacon b)
	{
		provider.CanUpdate = false;
		ui.ShowStairs = false;
	}

	private void OnPositionChange(Beacon b)
	{
		if (!provider.Commited)
		{
			ui.Move(b.position);
		}
	}

	private void OnPositionDoneChanging(Beacon b)
	{
		provider.CanUpdate = true;
		provider.Changed.ConnectOnce(delegate
		{
			ui.ShowStairs = true;
		}).DisconnectWhen(base.OnReleaseOrDestroySignal);
		UpdateProvider();
	}

	protected override void BrixSlowUpdate()
	{
		base.BrixSlowUpdate();
		UpdateUI();
	}

	private void RecenterBeacon()
	{
		if (!provider.Commited)
		{
			XYZ center = ui.Bounds.Center;
			if (!project.Beacon.IsNullOrReleased())
			{
				project.Beacon.SetGameplayPosition(center.ToVector3());
			}
		}
	}
}
