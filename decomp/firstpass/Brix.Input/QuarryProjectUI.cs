using System;
using System.Collections.Generic;
using Brix.Components;
using Brix.Engine;
using Brix.Engine.Volumes;
using Brix.External.Signals;
using Brix.Game.Components.Volumes.Drawers;
using Brix.Lua;
using Brix.Utils;
using Brix.Utils.Signals;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Brix.Input;

public class QuarryProjectUI : BrixComponent
{
	private EdgeSignal<QuarryProjectUI> volumeChanged = new EdgeSignal<QuarryProjectUI>();

	public Signal StairChanged = new Signal();

	[SerializeField]
	private int initialSize;

	private StairOrientation stairOrientation = StairOrientation.Default;

	[SerializeField]
	private BoxDrawer.Spawner insideDrawerSpawner;

	[SerializeField]
	private BoxDrawer.Spawner outsideDrawerSpawner;

	[SerializeField]
	private RectZone.Spawner rectZoneSpawner;

	[SerializeField]
	private DraggableVolumePlane.Spawner planeSpawner;

	[SerializeField]
	private VoxelStairDrawer stairDrawer;

	private bool lockedDimension;

	private VoxelSet dugVoxel = new VoxelSet();

	public EdgeSignal<QuarryProjectUI> VolumeChange => volumeChanged;

	public StairOrientation StairOrientation => stairOrientation;

	private BoxDrawer insideDrawer => insideDrawerSpawner.Instance;

	private BoxDrawer outsideDrawer => outsideDrawerSpawner.Instance;

	private RectZone rectZone => rectZoneSpawner.Instance;

	private DraggableVolumePlane plane => planeSpawner.Instance;

	public VoxelBounds Bounds => rectZone.VolumeNoLeak.Bounds;

	public int MaxSize
	{
		get
		{
			return rectZone.MaxSize;
		}
		set
		{
			rectZone.MaxSize = value;
		}
	}

	public bool Showing
	{
		get
		{
			return base.gameObject.activeSelf;
		}
		set
		{
			base.gameObject.SetActive(value);
		}
	}

	public bool ShowStairs
	{
		get
		{
			return stairDrawer.Showing;
		}
		set
		{
			stairDrawer.Showing = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		insideDrawerSpawner.Spawn();
		outsideDrawerSpawner.Spawn();
		rectZoneSpawner.Spawn();
		planeSpawner.Spawn(rectZone);
	}

	public override void OnRequested()
	{
		base.OnRequested();
		rectZone.Drag.RelayTo(volumeChanged, this, base.OnReleaseOrDestroySignal);
		rectZone.Drag.End.Connect(delegate
		{
			RefreshBoundDrawers();
		}).DisconnectWhen(base.OnReleaseOrDestroySignal);
		plane.Drag.Connect(OnPlaneDragStart, null, OnPlaneDragEnd, base.OnReleaseOrDestroySignal);
	}

	public void AddHandlesToGroup(HandleGroup2<DragHandle> group)
	{
		plane.AddHandleToGroup(group);
		rectZone.AddHandleToGroup(group);
	}

	public void FromProvider(QuarryGoalProvider2 provider)
	{
		if (provider.Bounds != Bounds)
		{
			VoxelBounds b = provider.Bounds;
			int y = b.min.y;
			int relative = b.min.y - b.max.y;
			SetAbsoluteBottom(y, relative, delegate
			{
				rectZone.ResizeFromBounds(b.ToVectorBounds());
			});
		}
		RefreshPlane();
		RefreshBoundDrawers();
		if (!lockedDimension && provider.Commited)
		{
			LockDimensions();
		}
		List<XYZ> positions = CollectionPool.RequestList<XYZ>();
		List<Faces> directions = CollectionPool.RequestList<Faces>();
		provider.GetStairs(ref positions, ref directions);
		stairOrientation = provider.StairOrientation;
		stairDrawer.Redraw(positions, directions);
		positions.Dispose();
		directions.Dispose();
		stairOrientation = provider.StairOrientation;
	}

	public void Move(XYZ pos)
	{
		rectZone.Move(pos.ToVector3());
		RefreshBoundDrawers();
		RefreshPlane();
	}

	public void AddCompleted(XYZ pos)
	{
		dugVoxel.Add(pos);
	}

	public void NextStairs()
	{
		if (!lockedDimension)
		{
			stairOrientation = stairOrientation.Next();
			StairChanged.Invoke();
		}
	}

	private void LockDimensions()
	{
		rectZone.AcceptHover = false;
		rectZone.Showing = false;
		lockedDimension = true;
	}

	private void RefreshPlane()
	{
		if (!plane.Dragging)
		{
			plane.SetY(rectZone.VolumeNoLeak.Bounds.Min.y - 1);
		}
	}

	private void RefreshBoundDrawers()
	{
		insideDrawer.Redraw(rectZone.VolumeNoLeak);
		outsideDrawer.Redraw(dugVoxel);
	}

	private void OnPlaneDragStart(DraggableVolumePlane plane)
	{
		stairDrawer.Showing = false;
	}

	private void OnPlaneDragEnd(DraggableVolumePlane plane)
	{
		stairDrawer.Showing = true;
		int num = Mathf.Max(plane.GetY() + 1, Architecte.LowestPointIn(Bounds).y);
		int relative = num - Bounds.Max.y;
		SetAbsoluteBottom(num, relative, rectZone.UpdateHeight);
		RefreshBoundDrawers();
		volumeChanged.end.Invoke(this);
	}

	private void SetAbsoluteBottom(int absolute, int relative, Action a)
	{
		rectZone.Height.min.value = absolute;
		rectZone.Height.min.mode = HeightModes.Absolute;
		a();
		rectZone.Height.min.value = relative;
		rectZone.Height.min.mode = HeightModes.TerrainMinMaxPadded;
	}

	[LuaDrivable("NextStairs")]
	private static DynValue lua_NextStairs(ScriptExecutionContext context, CallbackArguments args)
	{
		QuarryProjectUI quarryProjectUI = args.AsComponent<QuarryProjectUI>(0, "NextStairs");
		quarryProjectUI.NextStairs();
		return DynValue.Void;
	}
}
