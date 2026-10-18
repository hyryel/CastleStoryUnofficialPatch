using System.Collections.Generic;
using Brix.Engine;
using Brix.Game.Components;
using Brix.Input;
using Brix.Lua;
using Brix.Utils;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.AI.UI;

[JsonObject(MemberSerialization.OptIn)]
public class LandscapeProjectUI : GameComponent
{
	private EdgeSignal<LandscapeProjectUI> drag = new EdgeSignal<LandscapeProjectUI>();

	private EdgeSignal<LandscapeProjectUI> thresholdUpdate = new EdgeSignal<LandscapeProjectUI>();

	[SerializeField]
	private LandscapeGoalsDrawer goalDrawer;

	[SerializeField]
	private DraggableVolumePlane.Spawner thresholdSpawner;

	[SerializeField]
	private RectZone.Spawner RectZoneSpawner;

	public IReadOnlyEdgeSignal<LandscapeProjectUI> Drag => drag;

	public IReadOnlyEdgeSignal<LandscapeProjectUI> ThresholdDrag => thresholdUpdate;

	private DraggableVolumePlane threshold => thresholdSpawner.Instance;

	private RectZone RectZone => RectZoneSpawner.Instance;

	public int MaxSize
	{
		get
		{
			return RectZone.MaxSize;
		}
		set
		{
			RectZone.MaxSize = value;
		}
	}

	public bool ShowGoals
	{
		get
		{
			return goalDrawer.Showing;
		}
		set
		{
			goalDrawer.Showing = value;
			if (value)
			{
				goalDrawer.Refresh();
			}
		}
	}

	public bool ShowThreshold
	{
		get
		{
			return threshold.Showing;
		}
		set
		{
			threshold.Showing = value;
		}
	}

	public bool AcceptHover
	{
		get
		{
			return RectZone.AcceptHover;
		}
		set
		{
			RectZone.AcceptHover = value;
		}
	}

	public bool Showing
	{
		get
		{
			return threshold.Showing || ShowGoals || RectZone.Showing;
		}
		set
		{
			threshold.Showing = value;
			ShowGoals = value;
			RectZone.Showing = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		RectZoneSpawner.Spawn();
		thresholdSpawner.Spawn(RectZone);
	}

	public override void OnRequested()
	{
		base.OnRequested();
		ConnectToSignals();
	}

	private void ConnectToSignals()
	{
		RectZone.Drag.RelayTo(drag, this, base.OnReleaseOrDestroySignal);
		threshold.Hover.Connect(OnThresholdHoverStart, null, OnThresholdHoverEnd, base.OnReleaseOrDestroySignal);
		threshold.Drag.RelayTo(thresholdUpdate, this, base.OnReleaseOrDestroySignal);
	}

	private void OnThresholdHoverStart(DraggableVolumePlane plane)
	{
		AcceptHover = false;
	}

	private void OnThresholdHoverEnd(DraggableVolumePlane plane)
	{
		AcceptHover = true;
	}

	public void AddHandleToGroup(HandleGroup2<DragHandle> group)
	{
		RectZone.AddHandleToGroup(group);
		threshold.AddHandleToGroup(group);
	}

	public void Move(Vector3 pos)
	{
		RectZone.Move(pos);
		int y = ComputeThreshold();
		threshold.SetY(y);
	}

	private int ComputeThreshold()
	{
		VoxelBounds bounds = RectZone.VolumeNoLeak.Bounds;
		int result = bounds.Center.y;
		int y = threshold.GetY();
		if (y >= bounds.min.y && y <= bounds.max.y)
		{
			result = y;
		}
		return result;
	}

	public void FromProvider(LandscapeGoalProvider provider)
	{
		RectZone.ResizeFromBounds(provider.Bounds.ToVectorBounds());
		threshold.SetY(provider.Threshold);
	}

	public void GetGoalsFrom(LandscapeGoalProvider provider)
	{
		ClearGoals();
		List<XYZ> _toDig = CollectionPool.RequestList<XYZ>();
		List<XYZ> _toFill = CollectionPool.RequestList<XYZ>();
		provider.DrawingInfo(ref _toFill, ref _toDig);
		_toFill.Slinq().ForEach(delegate(XYZ p)
		{
			AddOrSetGoal(p, LandscapeGoal.Landscape.Fill);
		});
		_toDig.Slinq().ForEach(delegate(XYZ p)
		{
			AddOrSetGoal(p, LandscapeGoal.Landscape.Dig);
		});
		_toDig.Dispose();
		_toFill.Dispose();
	}

	public void AddOrSetGoal(XYZ pos, LandscapeGoal.Landscape type)
	{
		goalDrawer.AddorSetVoxel(pos, type);
		if (goalDrawer.Showing)
		{
			goalDrawer.Refresh();
		}
	}

	public void ClearGoals()
	{
		goalDrawer.Clear();
	}

	public VoxelBounds GetVolume()
	{
		return RectZone.VolumeNoLeak.Bounds;
	}

	public int GetThresholdY()
	{
		return threshold.GetY();
	}

	public XYZ VolumeCenter()
	{
		return RectZone.VolumeNoLeak.Bounds.Center;
	}

	public void SetMaxThreshold()
	{
		threshold.SetY(RectZone.VolumeNoLeak.Bounds.Max.y);
		thresholdUpdate.end.Invoke(this);
	}

	public void SetMinThreshold()
	{
		threshold.SetY(RectZone.VolumeNoLeak.Bounds.Min.y - 1);
		thresholdUpdate.end.Invoke(this);
	}

	[LuaDrivable("RaiseAll")]
	private static DynValue lua_SetMaxThreshold(ScriptExecutionContext context, CallbackArguments args)
	{
		LandscapeProjectUI landscapeProjectUI = args.AsComponent<LandscapeProjectUI>(0, "RaiseAll");
		landscapeProjectUI.SetMaxThreshold();
		return DynValue.Void;
	}

	[LuaDrivable("DigAll")]
	private static DynValue lua_SetMinThreshold(ScriptExecutionContext context, CallbackArguments args)
	{
		LandscapeProjectUI landscapeProjectUI = args.AsComponent<LandscapeProjectUI>(0, "DigAll");
		landscapeProjectUI.SetMinThreshold();
		return DynValue.Void;
	}
}
