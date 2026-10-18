using Brix.Engine;
using Brix.Engine.Volumes;
using Brix.External.Signals;
using Brix.Game.AI;
using Brix.Game.Beacons;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Input;
using Brix.Utils.Unity;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.Legacy;

[JsonObject(MemberSerialization.OptIn)]
public class Beacon : UpdatableGameComponent<Beacon>, PieMenuable
{
	public enum BalisePointsType
	{
		Idle,
		Drop,
		Danger
	}

	private EdgeSignal<Beacon> positionChange = new EdgeSignal<Beacon>();

	private EdgeSignal<Beacon> hoverSignal = new EdgeSignal<Beacon>();

	private EdgeSignal<Beacon> radiusChange = new EdgeSignal<Beacon>();

	public Signal ClosenessChanged = new Signal();

	[SerializeField]
	public BaliseVisuals Visuals;

	public int MaxRadius;

	[SerializeField]
	private DragHandle.Spawner positionDragSpawner;

	[SerializeField]
	private CircleDrag.Spawner circleDrag;

	[DisplayRules(DisplayFlags.ReadOnly)]
	[JsonProperty]
	public VoxelVolumeComponent VolumeComponent;

	public Signal MadeMini = new Signal();

	public bool isMini;

	public Signal<string> BeaconNameChanged = new Signal<string>();

	public Signal<bool> VisibilityChanged = new Signal<bool>();

	private bool _editing;

	private Vector3 startPos;

	public bool isCloseEnough;

	public IReadOnlyEdgeSignal<Beacon> PositionChange => positionChange;

	public IReadOnlyEdgeSignal<Beacon> Hover => hoverSignal;

	public IReadOnlyEdgeSignal<Beacon> RadiusChange => radiusChange;

	public Project project => this.GetComponentInParent<Project>(includeInactive: true);

	public BeaconMeta Meta { get; set; }

	private DragHandle positionDrag => positionDragSpawner.Instance;

	[SerializeField]
	[JsonProperty]
	public float radius
	{
		get
		{
			return circleDrag.Radius;
		}
		set
		{
			circleDrag.Radius = Mathf.Clamp(value, 0f, MaxRadius);
		}
	}

	[SerializeField]
	[JsonProperty]
	public XYZ position
	{
		get
		{
			if (base.transform.position.y == Mathf.Floor(base.transform.position.y))
			{
				return XYZ.Floor(base.transform.position);
			}
			return XYZ.Floor(base.transform.position) + XYZ.up;
		}
		set
		{
			base.transform.position = value.ToVector3();
		}
	}

	public new IVoxelVolume volume
	{
		get
		{
			if (VolumeComponent != null)
			{
				return VolumeComponent.Volume;
			}
			VoxelCylinder voxelCylinder = new VoxelCylinder();
			voxelCylinder.center = XYZ.FromVector3(base.transform.position);
			voxelCylinder.extents = new XYZ((int)radius, 100, (int)radius);
			voxelCylinder.orientation = VoxelCylinder.Orientation.Up;
			return voxelCylinder;
		}
	}

	public bool Editing
	{
		get
		{
			return _editing;
		}
		private set
		{
			_editing = value;
		}
	}

	public bool ShowingCircle
	{
		get
		{
			return circleDrag.Showing;
		}
		set
		{
			circleDrag.Showing = value;
		}
	}

	public bool ShowingPositionDrag
	{
		get
		{
			return (bool)positionDrag && positionDrag.Showing;
		}
		set
		{
			if ((bool)positionDrag)
			{
				positionDrag.Showing = value;
			}
		}
	}

	public bool Showing { get; set; }

	public void Init()
	{
		if (position == XYZ.zero)
		{
			position = XYZ.FromVector3(base.transform.position);
		}
	}

	public override void ResetComponent()
	{
		positionDragSpawner.Release();
		circleDrag.Release();
	}

	public void SetName(string argName)
	{
		base.name = argName;
		BeaconNameChanged.Invoke(base.name);
	}

	public void InstantiateCircleDrag()
	{
		circleDrag.maxRadius = project.MaxSize;
		circleDrag.Spawn();
		circleDrag.SetCenter(positionDrag);
		circleDrag.ConnectDrag(OnRadiusDragStarted, null, OnRadiusDragEnded, base.OnReleaseOrDestroySignal);
	}

	public void InstantiateHandle()
	{
		positionDragSpawner.Spawn();
		positionDrag.DragSignals.Connect(OnDragStarted, OnDrag, OnDragEnded, base.OnReleaseOrDestroySignal);
		positionDrag.HoverSignals.RelayTo(hoverSignal, this, base.OnReleaseOrDestroySignal);
	}

	public void AcceptHover(bool value)
	{
		positionDrag.Active = value;
	}

	public void SetVisualPosition(Vector3 pos)
	{
		base.transform.position = pos;
	}

	private Vector3 GetSurface(Vector3 pos)
	{
		Vector3 result = pos;
		if (Physics.Raycast(pos + Vector3.up * 10f, Vector3.down, out var hitInfo, 200f, 1024, QueryTriggerInteraction.Ignore) && !hitInfo.transform.gameObject.CompareTag("Void"))
		{
			result = Architecte.SurfaceRaycast(hitInfo.point) + Vector3.up * 0.05f;
		}
		return result;
	}

	public void SetGameplayPosition(Vector3 pos)
	{
		position = XYZ.FromVector3(GetSurface(pos));
	}

	private void OnDragStarted(DragHandle handle)
	{
		startPos = handle.Origin;
		Editing = true;
		positionChange.start.Invoke(this);
	}

	private void OnDrag(DragHandle handle)
	{
		if (!((handle.Destination - startPos).sqrMagnitude < 0.25f))
		{
			SetVisualPosition(handle.Destination);
			positionChange.on.Invoke(this);
		}
	}

	private void OnDragEnded(DragHandle handle)
	{
		Editing = false;
		SetGameplayPosition(handle.Destination);
		positionChange.end.Invoke(this);
	}

	private void OnRadiusDragStarted(CircleDrag circle)
	{
		Editing = true;
		radiusChange.start.Invoke(this);
	}

	private void OnRadiusDragEnded(CircleDrag circle)
	{
		Editing = false;
		radiusChange.end.Invoke(this);
	}

	public void CheckCloseEnough()
	{
		bool flag = Vector3.Distance(Camera.main.transform.position, base.transform.position) < (float)Screen.height / 5f;
		if (flag != isCloseEnough)
		{
			isCloseEnough = flag;
			ClosenessChanged.Invoke();
		}
	}

	public void AddHandleToGroup(HandleGroup2<DragHandle> group)
	{
		group.AddHandle(positionDrag);
	}

	public void MakeMini()
	{
		Visuals.Height = 0.5f;
		isMini = true;
		MadeMini.Invoke();
	}

	public bool IsInRadius(XYZ position)
	{
		return radius >= Vector3.Distance(position.ToVector3(), this.position.ToVector3());
	}

	protected override void BrixUpdate()
	{
		CheckCloseEnough();
	}
}
