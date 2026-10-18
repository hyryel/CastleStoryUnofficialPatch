using System;
using Brix.Components;
using Brix.Engine;
using Brix.External.Factories;
using Brix.Game.Components.Volumes;
using Brix.Game.Components.Volumes.Drawers;
using Brix.Lifecycle.Pooling;
using Brix.Util;
using Brix.Utils.Locomotion;
using Brix.Utils.Unity;
using UnityEngine;

namespace Brix.Input;

public class DraggableVolumePlane : BrixComponent
{
	[Serializable]
	public class Spawner
	{
		[DisplayRules(DisplayFlags.ReadOnly)]
		[SerializeField]
		private DraggableVolumePlane instance;

		[SerializeField]
		private Transform parent;

		[SerializeField]
		private int relativeMaxY;

		[SerializeField]
		private int relativeMinY;

		[SerializeField]
		private Material planeMaterial;

		[SerializeField]
		private Material lineMaterial;

		[SerializeField]
		private Material circleMaterial;

		public DraggableVolumePlane Instance => instance;

		public void Spawn(VoxelVolumeComponent _volume)
		{
			ObjectPoolSingleton.RequestComponent<DraggableVolumePlane>(out instance, VolumesUI.DraggablePlane);
			instance.transform.localPosition = Vector3.zero;
			instance.transform.localRotation = Quaternion.identity;
			instance.transform.localScale = Vector3.one;
			instance.transform.SetParent(parent);
			instance.drawer.Material = planeMaterial;
			instance.handle.SetLineMaterial(lineMaterial);
			instance.handle.SetPlaneColor(circleMaterial);
			instance.volume = _volume;
			instance.SetRelativeConstraints(relativeMinY, relativeMaxY);
		}

		public void Release()
		{
			ObjectPoolSingleton.Release(Instance.gameObject);
		}
	}

	private EdgeSignal<DraggableVolumePlane> drag = new EdgeSignal<DraggableVolumePlane>();

	private EdgeSignal<DraggableVolumePlane> hover = new EdgeSignal<DraggableVolumePlane>();

	[DisplayRules(DisplayFlags.ReadOnly)]
	[SerializeField]
	private VoxelVolumeComponent volume;

	[SerializeField]
	private BoxDrawer.Spawner drawerSpawner;

	[SerializeField]
	private DragHandle.Spawner handleSpawner;

	private Vector3 CornerWithDrag;

	private int relativeMaxY;

	private int relativeMinY;

	private bool dragging;

	private int Y;

	public IReadOnlyEdgeSignal<DraggableVolumePlane> Drag => drag;

	public IReadOnlyEdgeSignal<DraggableVolumePlane> Hover => hover;

	private VoxelBounds Volume => (volume as VoxelVolumeComponent<VoxelBounds>).VolumeNoLeak;

	private BoxDrawer drawer => drawerSpawner.Instance;

	private DragHandle handle => handleSpawner.Instance;

	public bool Dragging => dragging;

	public bool Showing
	{
		get
		{
			return drawer.Showing || handle.Showing;
		}
		set
		{
			if (!value)
			{
				drawer.Showing = value;
			}
			handle.Showing = value;
			base.enabled = value;
			UpdateVisuals();
		}
	}

	protected override void Start()
	{
		if (GetY() == 0)
		{
			SetY((int)base.transform.parent.position.y);
		}
		volume.VolumeChanged.Connect(OnVolumeChanged);
		UpdateVisuals();
	}

	public override void OnRequested()
	{
		drawerSpawner.Spawn();
		drawer.Showing = false;
		handleSpawner.Spawn();
		handle.DragSignals.Connect(OnDragStarted, OnDragged, OnDragEnded, base.OnReleaseOrDestroySignal);
		handle.HoverSignals.Connect(HandleHoverStart, null, HandleHoverEnd, base.OnReleaseOrDestroySignal);
	}

	private void Update()
	{
		UpdateCorner();
	}

	public void SetRelativeConstraints(int min, int max)
	{
		relativeMinY = min;
		relativeMaxY = max;
	}

	public void WakeUpHandle()
	{
		handle.WakeUp();
	}

	public void SleepHandle()
	{
		handle.Sleep();
	}

	public void AddHandleToGroup(HandleGroup2<DragHandle> group)
	{
		group.AddHandle(handle);
	}

	public int GetY()
	{
		return Y;
	}

	public void SetY(int y)
	{
		int min = Volume.Bounds.min.y - relativeMinY;
		int max = Volume.Bounds.max.y + relativeMaxY;
		Y = Mathf.Clamp(y, min, max);
		UpdateVisuals();
	}

	public bool IsAboveThreshold(XYZ voxel)
	{
		return voxel.y > Y;
	}

	private void SetYFromDrag(Vector3 pos)
	{
		int num = (int)Calcule.RoundToward(handle.Destination.y - handle.Origin.y, 0f);
		SetY(Y + num);
	}

	private void UpdateCorner()
	{
		Vector3 vector = Volume.VectorBounds.ClosestCornerTo(Camera.main.transform.position);
		if (vector.Flatten() != CornerWithDrag.Flatten())
		{
			CornerWithDrag = vector;
			UpdateDrag();
		}
	}

	private void UpdateVisuals()
	{
		UpdatePlane();
		UpdateDrag();
	}

	private void HandleHoverStart(DragHandle handle)
	{
		if (handle.Showing)
		{
			drawer.Showing = true;
			hover.start.Invoke(this);
		}
	}

	private void HandleHoverEnd(DragHandle handle)
	{
		if (handle.Showing)
		{
			drawer.Showing = false;
			hover.end.Invoke(this);
		}
	}

	private void OnVolumeChanged()
	{
		UpdateVisuals();
	}

	private void OnDragStarted(DragHandle handle)
	{
		drag.start.Invoke(this);
		dragging = true;
	}

	private void OnDragged(DragHandle handle)
	{
		SetYFromDrag(handle.Destination);
		UpdatePlane();
		drag.on.Invoke(this);
	}

	private void OnDragEnded(DragHandle handle)
	{
		dragging = false;
		SetYFromDrag(handle.Destination);
		UpdateVisuals();
		drag.end.Invoke(this);
	}

	private void UpdatePlane()
	{
		drawer.Redraw(Slice(Volume, Y));
	}

	private IVoxelVolume Slice(IVoxelVolume volume, int y)
	{
		return new VoxelBounds(volume.Bounds.max.Flatten() + XYZ.up * y, volume.Bounds.min.Flatten() + XYZ.up * (y - 1));
	}

	private void UpdateDrag()
	{
		Vector3 max = CornerWithDrag.Flatten(Volume.Bounds.VectorMax);
		Vector3 min = CornerWithDrag.Flatten(Volume.Bounds.VectorMin);
		handle.Origin = CornerWithDrag.Flatten(Vector3.one * ((float)Y + 0.5f));
		handle.SetMinMax(min, max);
	}
}
