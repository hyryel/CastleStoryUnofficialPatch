using System;
using Brix.Components;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.Components;
using Brix.Input;
using Brix.Lifecycle.Pooling;
using Brix.UI;
using Brix.Utils.Signals;
using Brix.Utils.Unity;
using UnityEngine;

public class CircleDrag : BrixComponent
{
	[Serializable]
	public class Spawner
	{
		[DisplayRules(DisplayFlags.ReadOnly)]
		[SerializeField]
		private CircleDrag instance;

		[DisplayRules(DisplayFlags.HideAtRuntime)]
		[SerializeField]
		private Transform parent;

		[SerializeField]
		[DisplayRules(DisplayFlags.HideAtRuntime)]
		private string name;

		[DisplayRules(DisplayFlags.HideAtRuntime)]
		[SerializeField]
		private float defaultRadius;

		[DisplayRules(DisplayFlags.HideAtRuntime)]
		[SerializeField]
		public float maxRadius = 16f;

		public float Radius
		{
			get
			{
				return (!instance) ? defaultRadius : instance.Radius;
			}
			set
			{
				if ((bool)instance)
				{
					instance.Radius = value;
				}
				else
				{
					defaultRadius = value;
				}
			}
		}

		public bool Showing
		{
			get
			{
				return (bool)instance && instance.Showing;
			}
			set
			{
				if ((bool)instance)
				{
					instance.Showing = value;
				}
			}
		}

		public void Spawn()
		{
			ObjectPoolSingleton.RequestComponent<CircleDrag>(out instance, Balises.CircleDraggable);
			instance.transform.SetParent(parent);
			instance.transform.localPosition = Vector3.zero;
			instance.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
			instance.name = name;
			instance.Radius = defaultRadius;
			instance.maxRadius = maxRadius;
		}

		public void Release()
		{
			if (!instance.IsNullOrReleased())
			{
				ObjectPoolSingleton.Release(instance.gameObject);
			}
		}

		public void SetCenter(DragHandle handle)
		{
			if ((bool)instance)
			{
				instance.SetCenter(handle);
			}
		}

		public void ConnectDrag(Action<CircleDrag> start, Action<CircleDrag> on, Action<CircleDrag> end, IReadOnlySignal<GameObject> until)
		{
			if ((bool)instance)
			{
				instance.ConnectDrag(start, on, end, until);
			}
		}
	}

	[SerializeField]
	private DragHandle.Spawner flexHandleSpawner;

	[SerializeField]
	private SphereHighlight.Spawner wholeZoneHighlight;

	[SerializeField]
	private BeaconCircleZones zones;

	[SerializeField]
	private CircleHighlight circle;

	[SerializeField]
	public float maxRadius;

	public Signal<CircleDrag> DragStarted = new Signal<CircleDrag>();

	public Signal<CircleDrag> Dragging = new Signal<CircleDrag>();

	public Signal<CircleDrag> DragEnded = new Signal<CircleDrag>();

	public Signal<float> RadiusChanged = new Signal<float>();

	private bool dragging;

	private DragHandle CenterHandle;

	private bool hover;

	private float _radius;

	private DragHandle flexHandle => flexHandleSpawner.Instance;

	public float Radius
	{
		get
		{
			return _radius;
		}
		set
		{
			_radius = value;
			circle.Radius = _radius;
			RadiusChanged.Invoke(_radius);
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

	public void SetCenter(DragHandle handle)
	{
		CenterHandle = handle;
		CenterHandle.DragSignals.Connect(null, CenterDragged, null, base.OnReleaseOrDestroySignal);
		PlaceHandle(CenterHandle.transform.position + Vector3.right * Radius);
	}

	private void RemoveHighlight()
	{
		wholeZoneHighlight.active = false;
	}

	public override void OnRequested()
	{
		base.OnRequested();
		zones.RadiusHover.Connect(OnRadiusHover);
		zones.RadiusExit.Connect(OnRadiusExit);
		zones.PositionEnter.Connect(OnPositionEnter);
		zones.PositionExit.Connect(OnPositionExit);
		flexHandleSpawner.Spawn();
		flexHandle.SynchScale.OfParent().With(base.transform, 1f, inverse: true).When(RadiusChanged);
		flexHandle.DragSignals.Connect(null, FlexDragged, FlexDragEnded, base.OnReleaseOrDestroySignal);
		wholeZoneHighlight.Spawn();
		wholeZoneHighlight.active = false;
	}

	public void ConnectDrag(Action<CircleDrag> start, Action<CircleDrag> on, Action<CircleDrag> end, IReadOnlySignal<GameObject> until)
	{
		if (start != null)
		{
			DragStarted.Connect(start).DisconnectWhen(until);
		}
		if (on != null)
		{
			Dragging.Connect(on).DisconnectWhen(until);
		}
		if (end != null)
		{
			DragEnded.Connect(end).DisconnectWhen(until);
		}
	}

	private void HighlightWholeZone()
	{
		wholeZoneHighlight.active = true;
	}

	private void CenterDragged(DragHandle handle)
	{
		HighlightWholeZone();
	}

	private void FlexDragged(DragHandle handle)
	{
		dragging = true;
		HighlightWholeZone();
		Radius = ComputeRadius(flexHandle.Destination);
		flexHandle.SetLine(base.transform.position, flexHandle.Destination);
		Dragging.Invoke(this);
	}

	private void FlexDragEnded(DragHandle handle)
	{
		dragging = false;
		RemoveHighlight();
		Radius = (int)ComputeRadius(flexHandle.Destination);
		if (hover)
		{
			OnRadiusHover(flexHandle.Destination);
		}
		else
		{
			OnRadiusExit(flexHandle.Destination);
		}
		DragEnded.Invoke(this);
	}

	private void OnPositionEnter(Vector3 point)
	{
		CenterHandle.WakeUp();
	}

	private void OnPositionExit(Vector3 point)
	{
		CenterHandle.Sleep();
		RemoveHighlight();
	}

	private void OnRadiusHover(Vector3 point)
	{
		hover = true;
		if (!dragging)
		{
			PlaceHandle(point);
			flexHandle.Showing = true;
			flexHandle.WakeUp();
		}
	}

	private void PlaceHandle(Vector3 point)
	{
		Vector3 normalized = Vector3.Scale(new Vector3(1f, 0f, 1f), point - base.transform.position).normalized;
		Vector3 origin = base.transform.position + normalized * Radius;
		flexHandle.Origin = origin;
		flexHandle.Towards(normalized);
	}

	private void OnRadiusExit(Vector3 point)
	{
		hover = false;
		if (!dragging)
		{
			flexHandle.Showing = false;
		}
	}

	private float ComputeRadius(Vector3 pos)
	{
		float value = Vector3.Distance(base.transform.position, pos);
		return Mathf.Clamp(value, 1f, maxRadius);
	}
}
