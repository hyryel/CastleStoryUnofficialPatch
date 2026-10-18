using System;
using System.Collections.Generic;
using Brix.Common.Collections;
using Brix.External.Signals;
using Brix.Input;
using Brix.Util;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;

public class VoxelFaces : MonoBehaviour
{
	public DragHandle.Spawner UpSpawner;

	public DragHandle.Spawner ForwardSpawner;

	public DragHandle.Spawner BackSpawner;

	public DragHandle.Spawner RightSpawner;

	public DragHandle.Spawner LeftSpawner;

	public DragHandle.Spawner DownSpawner;

	private EdgeSignal<DragHandle> hover = new EdgeSignal<DragHandle>();

	private EdgeSignal<DragHandle> drag = new EdgeSignal<DragHandle>();

	private Signal<GameObject> release = new Signal<GameObject>();

	private bool showing = true;

	public DragHandle Up => UpSpawner.Instance;

	public DragHandle Forward => ForwardSpawner.Instance;

	public DragHandle Back => BackSpawner.Instance;

	public DragHandle Right => RightSpawner.Instance;

	public DragHandle Left => LeftSpawner.Instance;

	public DragHandle Down => DownSpawner.Instance;

	public IReadOnlyEdgeSignal<DragHandle> Hover => hover;

	public IReadOnlyEdgeSignal<DragHandle> Drag => drag;

	public bool Showing
	{
		get
		{
			return showing;
		}
		set
		{
			showing = value;
			foreach (DragHandle handle in Handles)
			{
				handle.Showing = value;
			}
		}
	}

	public Bounds Bounds
	{
		get
		{
			Slinq<Vector3, SelectContext<Vector3, Transform, IEnumerableContext<Transform>>> slinq = from t in Children.Slinq()
				select t.position;
			Vector3 vector = Vector3.one * 10000f;
			Vector3 vector2 = Vector3.one * -10000f;
			foreach (Vector3 item in slinq.NoLeak())
			{
				vector = Calcule.MinVecteur(vector, item);
				vector2 = Calcule.MaxVecteur(vector2, item);
			}
			Bounds result = default(Bounds);
			result.SetMinMax(vector, vector2);
			return result;
		}
		set
		{
			Spawn();
			Vector3 center = value.center;
			Vector3 extents = value.extents;
			foreach (Transform child in Children)
			{
				child.position = center + Mathf.Abs(Vector3.Dot(extents, child.forward)) * child.forward;
			}
		}
	}

	public IEnumerable<DragHandle> Handles
	{
		get
		{
			yield return Up;
			yield return Forward;
			yield return Back;
			yield return Right;
			yield return Left;
			yield return Down;
		}
	}

	private IEnumerable<DragHandle.Spawner> HandleSpawners
	{
		get
		{
			yield return UpSpawner;
			yield return ForwardSpawner;
			yield return BackSpawner;
			yield return RightSpawner;
			yield return LeftSpawner;
			yield return DownSpawner;
		}
	}

	public IEnumerable<Transform> Children => (from d in Handles.Slinq()
		select base.transform).NoLeak();

	public void Spawn()
	{
		foreach (DragHandle.Spawner handleSpawner in HandleSpawners)
		{
			handleSpawner.Spawn();
			if (!(handleSpawner.Instance == null))
			{
				handleSpawner.Instance.DragSignals.RelayTo(drag, release);
				handleSpawner.Instance.HoverSignals.RelayTo(hover, release);
			}
		}
	}

	public void Release()
	{
		release.Invoke(null);
		foreach (DragHandle.Spawner handleSpawner in HandleSpawners)
		{
			handleSpawner.Release();
		}
	}

	public DragHandle HandleOf(Faces face)
	{
		return face switch
		{
			Faces.up => Up, 
			Faces.front => Forward, 
			Faces.back => Back, 
			Faces.left => Left, 
			Faces.right => Right, 
			Faces.bottom => Down, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
