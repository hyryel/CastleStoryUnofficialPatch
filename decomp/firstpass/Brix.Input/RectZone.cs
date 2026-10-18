using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Factories;
using Brix.Game.Components.Volumes;
using Brix.Game.Components.Volumes.Drawers;
using Brix.Lifecycle.Pooling;
using Brix.Util;
using Brix.Utils;
using Brix.Utils.Locomotion;
using Brix.Utils.Unity;
using UnityEngine;

namespace Brix.Input;

public class RectZone : VoxelVolumeComponent<VoxelBounds>
{
	[Serializable]
	public class Spawner
	{
		[DisplayRules(DisplayFlags.ReadOnly)]
		[SerializeField]
		private RectZone instance;

		[DisplayRules(DisplayFlags.HideAtRuntime)]
		[SerializeField]
		private Transform parent;

		[DisplayRules(DisplayFlags.HideAtRuntime)]
		[SerializeField]
		private string name;

		[SerializeField]
		[DisplayRules(DisplayFlags.HideAtRuntime)]
		private HeightConstraint height;

		public RectZone Instance => instance;

		public void Spawn()
		{
			ObjectPoolSingleton.RequestComponent<RectZone>(out instance, VolumesUI.RectZone);
			instance.transform.SetParent(parent);
			instance.name = name;
			instance.transform.localPosition = Vector3.zero;
			instance.transform.localRotation = Quaternion.identity;
			instance.transform.localScale = Vector3.one;
			instance.height = height;
		}

		public void Release()
		{
			ObjectPoolSingleton.Release(instance.gameObject);
		}
	}

	[Serializable]
	public class Parameters
	{
		public float detectionWidth;
	}

	private EdgeSignal<RectZone> drag = new EdgeSignal<RectZone>();

	private EdgeSignal<RectZone> handleHover = new EdgeSignal<RectZone>();

	private EdgeSignal<RectZone> rectHover = new EdgeSignal<RectZone>();

	[SerializeField]
	private BoxDrawer.Spawner terrainDrawerSpawner;

	[SerializeField]
	private DragHandle.Spawner handleSpawner;

	[SerializeField]
	private BoxCollider boxCollider;

	private HeightConstraint height = new HeightConstraint();

	private int maxSize = 16;

	private HandleGroup2<Vector3> group;

	private float MaxY;

	private float MinY;

	private Vector3 expandDirection;

	private bool dragging;

	private bool acceptHover = true;

	[SerializeField]
	private Parameters parameters;

	public IReadOnlyEdgeSignal<RectZone> Drag => drag;

	public IReadOnlyEdgeSignal<RectZone> HandleHover => handleHover;

	public IReadOnlyEdgeSignal<RectZone> RectHover => rectHover;

	private BoxDrawer terrainDrawer => terrainDrawerSpawner.Instance;

	private DragHandle handle => handleSpawner.Instance;

	public HeightConstraint Height => height;

	public int MaxSize
	{
		get
		{
			return maxSize;
		}
		set
		{
			maxSize = value;
		}
	}

	public override IVoxelVolume Volume => VolumeNoLeak;

	public override VoxelBounds VolumeNoLeak
	{
		get
		{
			VoxelBounds result = VoxelBounds.FromVectorBoundsAdjustedToMax(FittedBounds);
			XYZ min = result.Min;
			min.y = (int)MinY;
			result.min = min;
			return result;
		}
	}

	private Vector3 Center
	{
		get
		{
			return base.transform.position;
		}
		set
		{
			base.transform.position = value;
		}
	}

	private Vector3 Size
	{
		get
		{
			return base.transform.localScale;
		}
		set
		{
			base.transform.localScale = value;
		}
	}

	private Bounds ThinBounds => new Bounds(Center, Size);

	private Bounds SurfaceSearchBounds
	{
		get
		{
			Vector3 min = ThinBounds.min.Flatten(Center - Vector3.up * 40f);
			Vector3 max = ThinBounds.max.Flatten(Center + Vector3.up * 40f);
			Bounds result = default(Bounds);
			result.SetMinMax(min, max);
			return result;
		}
	}

	private Bounds FittedBounds
	{
		get
		{
			Vector3 min = ThinBounds.min.Flatten(Vector3.up * MinY);
			Vector3 max = ThinBounds.max.Flatten(Vector3.up * MaxY);
			Bounds result = default(Bounds);
			result.SetMinMax(min, max);
			return result;
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

	public bool AcceptHover
	{
		get
		{
			return acceptHover;
		}
		set
		{
			handle.Showing = value;
			acceptHover = value;
		}
	}

	public void OnHoverEnded()
	{
	}

	public void OnHover(Vector3 point)
	{
	}

	public override void OnRequested()
	{
		base.OnRequested();
		terrainDrawerSpawner.Spawn();
		group = new HandleGroup2<Vector3>(PlaceHandle, null, (Vector3 v) => v);
		group.maxRange = 60f;
		handleSpawner.Spawn();
		handle.SynchScale.OfParent().With(base.transform, 1f, inverse: true);
		handle.DragSignals.Connect(OnDragStart, null, OnDragEnd, base.OnReleaseOrDestroySignal);
		handle.HoverSignals.RelayTo(handleHover, this, base.OnReleaseOrDestroySignal);
	}

	protected override void Start()
	{
		base.Start();
		handle.SynchScale.Now();
		handle.Sleep();
	}

	private void Update()
	{
		if (!dragging && acceptHover)
		{
			group.Refresh();
		}
	}

	public void ResizeFromBounds(Bounds bound)
	{
		Vector3 center = bound.center;
		Vector3 vector = bound.size.Flatten(Vector3.up);
		if (!(center == Center) || !(vector == Size))
		{
			Center = bound.center;
			Size = bound.size.Flatten(Vector3.up);
			handle.SynchScale.Now();
			UpdateHeight();
			UpdateGroup(bound);
			terrainDrawer.Redraw(VolumeNoLeak);
		}
	}

	public void Move(Vector3 pos)
	{
		if (Center != pos)
		{
			Center = pos;
			UpdateHeight();
			UpdateGroup(ThinBounds);
			terrainDrawer.Redraw(VolumeNoLeak);
		}
	}

	private void OnDragStart(DragHandle handle)
	{
		expandDirection = ThinBounds.SideOf(handle.Origin);
		dragging = true;
		drag.start.Invoke(this);
	}

	private void OnDragEnd(DragHandle handle)
	{
		Vector3 point = RoundedDestination(handle.Origin, handle.Destination);
		ResizeFromBounds(ThinBounds.ExtendToPoint(expandDirection, point));
		dragging = false;
		drag.end.Invoke(this);
	}

	private Vector3 RoundedDestination(Vector3 origin, Vector3 destination)
	{
		Vector3 toRound = destination - origin;
		XYZ xYZ = Calcule.RoundVecteurToward(toRound, Vector3.zero);
		return handle.Origin + xYZ.ToVector3();
	}

	public void UpdateHeight()
	{
		height.Compute(SurfaceSearchBounds, out MinY, out MaxY);
		Vector3 vector = Center.Flatten(Vector3.up * (MinY + MaxY) / 2f);
		boxCollider.size = boxCollider.size.Flatten(Vector3.up * (MaxY - MinY));
		boxCollider.center = Vector3.zero.Flatten(vector - Center);
	}

	public void AddHandleToGroup(HandleGroup2<DragHandle> group)
	{
		group.AddHandle(handle);
	}

	private void UpdateGroup(Bounds b)
	{
		group.Clear();
		List<Vector3> features = CollectionPool.RequestList<Vector3>();
		b.SpecialPoints(ref features);
		for (int i = 0; i < features.Count; i++)
		{
			features[i] = features[i].Flatten(Center);
			if (Architecte.ExactTerrainSurfaceAt(features[i], out var result, Vector3.down))
			{
				features[i] = features[i].Flatten(result);
			}
			else
			{
				features[i] = features[i].Flatten(Center);
			}
		}
		group.AddRange(features);
		features.Dispose();
	}

	private void PlaceHandle(Vector3 at)
	{
		handle.Origin = at;
		if (ThinBounds.IsCenter(at))
		{
			handle.Active = false;
		}
		else if (ThinBounds.IsCorner(at))
		{
			handle.Active = true;
			handle.Type = HandleType.HorizontalPlane;
		}
		else
		{
			handle.Active = true;
			handle.Type = HandleType.Horizontal;
			handle.Towards(ThinBounds.SideOf(at));
		}
	}
}
