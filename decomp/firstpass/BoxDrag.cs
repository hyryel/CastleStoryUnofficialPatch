using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Game.Components.Volumes;
using Brix.Game.Components.Volumes.Drawers;
using Brix.Input;
using Brix.Utils;
using Smooth.Slinq;
using UnityEngine;

public class BoxDrag : VoxelVolumeComponent
{
	[SerializeField]
	private VoxelFaces faces;

	[SerializeField]
	private BoxDrawer.Spawner boxDrawerSpawner;

	[SerializeField]
	private BoxDrawer.Spawner previewDrawerSpawner;

	[SerializeField]
	private BoxDrawer.Spawner outlineDrawerSpawner;

	[SerializeField]
	private List<SynchScale> scalers;

	[SerializeField]
	private Material AddMaterial;

	[SerializeField]
	private Material RemoveMaterial;

	public Signal<BoxDrag> DragStarted = new Signal<BoxDrag>();

	public Signal<BoxDrag> Dragged = new Signal<BoxDrag>();

	public Signal<BoxDrag> DragEnded = new Signal<BoxDrag>();

	public Signal<BoxDrag> Emptied = new Signal<BoxDrag>();

	public Signal<BoxDrag> Hover = new Signal<BoxDrag>();

	public Signal<BoxDrag> HoverEnd = new Signal<BoxDrag>();

	private bool HandleHover;

	private bool Dragging;

	private bool Moving;

	private bool RefuseBoxHover;

	private Plane[] planes = new Plane[6];

	private Bounds extendedBounds;

	private VoxelBounds previousBounds;

	public float MinimumSizeInAnyDimension = 2f;

	private BoxDrawer boxDrawer => boxDrawerSpawner.Instance;

	private BoxDrawer previewDrawer => previewDrawerSpawner.Instance;

	private BoxDrawer outlineDrawer => outlineDrawerSpawner.Instance;

	public override IVoxelVolume Volume => VoxelBounds.FromVectorBounds(GetComponent<BoxCollider>().bounds);

	protected override void Start()
	{
		faces.Spawn();
		boxDrawerSpawner.Spawn();
		previewDrawerSpawner.Spawn();
		outlineDrawerSpawner.Spawn();
		faces.Drag.Connect(delegate
		{
			DragStarted.Invoke(this);
		}, OnDrag, OnDragEnded, base.OnReleaseOrDestroySignal);
		faces.Hover.Connect(OnDragHover, null, OnDragHoverEnded, base.OnReleaseOrDestroySignal);
		UpdateDraw();
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void AcceptBoxHovers()
	{
		RefuseBoxHover = false;
	}

	public void RefuseBoxHovers()
	{
		RefuseBoxHover = true;
		OnBoxeHoverEnded();
	}

	public void Move(Vector3 pos)
	{
		UpdateBoxFromBounds(new Bounds(pos, Volume.Bounds.Size.ToVector3()));
		UpdateDraw();
		VolumeChanged.Invoke();
	}

	public void ResizeFromBounds(Bounds bounds)
	{
		UpdateBoxFromBounds(bounds);
		ReScaleAll();
		UpdateDraw();
		VolumeChanged.Invoke();
	}

	private float SizeInDimensionOf(IVoxelVolume volume, Vector3 axis)
	{
		return Mathf.Abs(Vector3.Dot(volume.Bounds.VectorBounds.size, axis));
	}

	private void UpdateBoxFromDrag(Vector3 dragOrigin, Vector3 dragDestination, Vector3 dragForward)
	{
		float num = Vector3.Dot(dragDestination - dragOrigin, dragForward);
		Vector3 vector = Axis(dragOrigin, dragDestination);
		float num2 = SizeInDimensionOf(Volume, vector);
		if (!IsExpanding(Volume, dragOrigin, dragDestination))
		{
			float num3 = num2 - MinimumSizeInAnyDimension;
			num = Mathf.Clamp(num, 0f - num3, num3);
		}
		base.transform.localScale += vector * num;
		base.transform.position += dragForward * (num / 2f);
	}

	private void UpdateBoxFromBounds(Bounds bounds)
	{
		base.transform.position = bounds.center;
		base.transform.localScale = bounds.extents * 2f;
	}

	private void UpdateDraw()
	{
		boxDrawer.Redraw(Volume);
	}

	public void OnStartMove()
	{
		Moving = true;
		CollapseAllHandles();
		outlineDrawer.Showing = false;
	}

	public void OnEndMove()
	{
		Moving = false;
	}

	private void ReScaleAll()
	{
		scalers.Slinq().ForEach(delegate(SynchScale s)
		{
			s.Now();
		});
	}

	private void OnDragHover(DragHandle drag)
	{
		if (!Moving)
		{
			HandleHover = true;
			if (!Dragging)
			{
				outlineDrawer.OnlyTheseFaces.Clear();
				outlineDrawer.OnlyTheseFaces.Add(drag.Face);
				outlineDrawer.Redraw(Volume);
			}
		}
	}

	private void OnDragHoverEnded(DragHandle obj)
	{
		HandleHover = false;
	}

	private void OnDrag(DragHandle handle)
	{
		HandleHover = true;
		Dragging = true;
		if (!PointMoved(handle, handle.Destination) || CannotFurtherReduce(handle, handle.Destination))
		{
			HidePreviewVolume(handle);
		}
		else
		{
			DisplayPreviewVolume(handle, handle.Destination);
		}
	}

	private bool CannotFurtherReduce(DragHandle drag, Vector3 p)
	{
		bool flag = IsExpanding(Volume, drag.transform.position, p);
		float num = SizeInDimensionOf(Volume, Axis(drag.transform.position, p));
		bool flag2 = num <= MinimumSizeInAnyDimension;
		return !flag && flag2;
	}

	private void OnDragEnded(DragHandle handle)
	{
		Dragging = false;
		UpdateBoxFromDrag(handle.transform.position, handle.Destination, handle.transform.forward);
		if (IsEmpty())
		{
			Emptied.Invoke(this);
		}
		ReScaleAll();
		UpdateDraw();
		previewDrawer.Showing = false;
		outlineDrawer.Showing = false;
		CollapseAllHandles();
		DragEnded.Invoke(this);
		VolumeChanged.Invoke();
	}

	public void OnBoxHover(Ray ray)
	{
		if (!Dragging && !Moving && !RefuseBoxHover)
		{
			RefreshFacesCrossedBy(ray, HandleHover);
			Hover.Invoke(this);
		}
	}

	public void OnBoxeHoverEnded()
	{
		if (!HandleHover)
		{
			outlineDrawer.Showing = false;
			CollapseAllHandles();
			HoverEnd.Invoke(this);
		}
	}

	private void DisplayPreviewVolume(DragHandle drag, Vector3 p)
	{
		Material material = ((!IsExpanding(Volume, drag.transform.position, drag.Destination)) ? RemoveMaterial : AddMaterial);
		Vector3 intersection;
		VoxelBounds voxelBounds = ComputeDragBounds(drag.transform.position, p, out intersection);
		previewDrawer.Material = material;
		previewDrawer.Redraw(voxelBounds);
		drag.IntersectWith(intersection);
		Dragged.Invoke(this);
	}

	private bool IsEmpty()
	{
		return base.transform.localScale.x < 1f || base.transform.localScale.y < 1f || base.transform.localScale.z < 1f;
	}

	private void HidePreviewVolume(DragHandle drag)
	{
		previewDrawer.Showing = false;
		drag.DontIntersect();
	}

	private void VolumePlanes(Bounds bounds, ref Plane[] planes)
	{
		ref Plane reference = ref planes[0];
		reference = new Plane(Vector3.up, bounds.max);
		ref Plane reference2 = ref planes[1];
		reference2 = new Plane(Vector3.forward, bounds.max);
		ref Plane reference3 = ref planes[2];
		reference3 = new Plane(Vector3.back, bounds.min);
		ref Plane reference4 = ref planes[3];
		reference4 = new Plane(Vector3.left, bounds.max);
		ref Plane reference5 = ref planes[4];
		reference5 = new Plane(Vector3.right, bounds.min);
		ref Plane reference6 = ref planes[5];
		reference6 = new Plane(Vector3.down, bounds.min);
	}

	private void VolumeRayHits(Bounds bounds, Ray ray, ref List<Faces> facesHit)
	{
		VolumePlanes(bounds, ref planes);
		extendedBounds.SetMinMax(bounds.min, bounds.max);
		extendedBounds.Expand(0.05f);
		for (int i = 0; i < 6; i++)
		{
			planes[i].Raycast(ray, out var enter);
			Vector3 point = ray.GetPoint(enter);
			if (extendedBounds.Contains(point))
			{
				facesHit.Add((Faces)i);
			}
		}
	}

	private void RefreshFacesCrossedBy(Ray ray, bool hoveringHandle)
	{
		List<Faces> facesHit = CollectionPool.RequestList<Faces>();
		VolumeRayHits(Volume.VectorBounds, ray, ref facesHit);
		if (!hoveringHandle)
		{
			outlineDrawer.OnlyTheseFaces.Clear();
			outlineDrawer.OnlyTheseFaces.AddRange(facesHit);
			outlineDrawer.Redraw(Volume);
		}
		ExpandHandlesOfFaces(facesHit);
		facesHit.Dispose();
	}

	private void ExpandHandlesOfFaces(List<Faces> hoveredFaces)
	{
		foreach (DragHandle handle in faces.Handles)
		{
			if (hoveredFaces.Contains(handle.Face))
			{
				handle.WakeUp();
			}
			else
			{
				handle.Sleep();
			}
		}
	}

	private void CollapseAllHandles()
	{
		foreach (DragHandle handle in faces.Handles)
		{
			handle.Sleep();
		}
	}

	private Vector3 Axis(Vector3 origin, Vector3 destination)
	{
		Vector3 vector = destination - origin;
		return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z)).normalized;
	}

	private Vector3 Direction(Vector3 origin, Vector3 destination)
	{
		Vector3 vector = destination - origin;
		return new Vector3(vector.x, vector.y, vector.z).normalized;
	}

	private VoxelBounds ComputeDragBounds(Vector3 origin, Vector3 destination, out Vector3 intersection)
	{
		XYZ xYZ = XYZ.FromVector3(Axis(origin, destination));
		XYZ xYZ2 = XYZ.FromVector3(Direction(origin, destination));
		XYZ b = XYZ.one - xYZ;
		bool floor = XYZ.Dot(xYZ2, xYZ) < 0;
		float max = XYZ.Mul(Volume.Bounds.Size, xYZ).ToVector3().magnitude - MinimumSizeInAnyDimension;
		float num = Mathf.FloorToInt((destination - origin).magnitude);
		if (!IsExpanding(Volume, origin, destination))
		{
			num = Mathf.Clamp(num, 0f, max);
		}
		XYZ xYZ3 = XYZ.Mul(Volume.Bounds.Min, b);
		XYZ a = xYZ3 + XYZ.Mul(FloorOrCeil(origin, floor), xYZ);
		XYZ b2 = XYZ.Mul(Volume.Bounds.Max, b);
		XYZ xYZ4 = XYZ.Mul(a, xYZ) + xYZ2 * ((int)num - 1);
		b2 += xYZ4;
		intersection = Vector3.Scale(origin, b.ToVector3()) + xYZ4.ToVector3() + xYZ2.ToVector3() * 0.5f;
		return new VoxelBounds(a, b2);
	}

	private bool PointMoved(DragHandle d, Vector3 p)
	{
		return Vector3.Distance(d.transform.position, p) > 1f;
	}

	private bool IsExpanding(IVoxelVolume v, Vector3 origin, Vector3 destination)
	{
		return !AisOnBSideOfC(destination, v.Bounds.Center.ToVector3(), origin);
	}

	private bool AisOnBSideOfC(Vector3 A, Vector3 B, Vector3 C)
	{
		Plane plane = new Plane((B - C).normalized, C);
		return plane.GetSide(A);
	}

	private XYZ FloorOrCeil(Vector3 pos, bool floor)
	{
		if (floor)
		{
			return new XYZ(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
		}
		return new XYZ(Mathf.CeilToInt(pos.x), Mathf.CeilToInt(pos.y), Mathf.CeilToInt(pos.z));
	}
}
