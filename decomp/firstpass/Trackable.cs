using System;
using Brix.External.Signals;
using Brix.Game;
using Brix.Game.Components;
using Brix.Game.Components.Tracking;
using Brix.Utils.Signals;
using UnityEngine.Networking;

public class Trackable : UpdatableNetworkGameComponent
{
	private static readonly UpdatableNetworkProfiler<Trackable> UpdatableProfiler = new UpdatableNetworkProfiler<Trackable>();

	private VoxelTransform _voxelTransform;

	[EnumFlag]
	public TrackingLayer Layer;

	private TrackingRegistry.TrackingEntry _trackingEntry = TrackingRegistry.TrackingEntry.Empty;

	private Action OnMovedAction;

	private SignalConnection MoveListenerConnection = SignalConnection.Empty;

	private bool Ready;

	public int TrackingBit;

	protected override UpdatableProfiler Updatable => UpdatableProfiler;

	public TrackingRegistry.TrackingEntry TrackingEntry => _trackingEntry;

	protected override bool HasSlowUpdate => _voxelTransform == null;

	protected override float SlowUpdateRate => 2f;

	protected override void Awake()
	{
		base.Awake();
		_voxelTransform = GetComponent<VoxelTransform>();
	}

	public override void OnStartAnywhere()
	{
		base.OnStartAnywhere();
		Register();
		if ((bool)base.affiliation)
		{
			base.affiliation.OnFactionChanged.Connect(delegate(Faction n, Faction o, Trackable self)
			{
				self.AffiliationChanged();
			}, this).DisconnectWhen(base.OnReleaseOrDestroySignal);
		}
		Ready = true;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (Ready)
		{
			Register();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Unregister();
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		Unregister();
	}

	protected override void BrixSlowUpdate()
	{
		OnMoved();
	}

	private void Register()
	{
		if (_trackingEntry.gameComponent.IsNullOrReleasedIgc())
		{
			_trackingEntry = TrackingRegistry.Register(this, Layer, faction);
			if (_voxelTransform != null)
			{
				MoveListenerConnection.Disconnect();
				MoveListenerConnection = _voxelTransform.AddMoveListener(OnMoved);
			}
		}
	}

	private void AffiliationChanged()
	{
		Unregister();
		Register();
	}

	private void Unregister()
	{
		MoveListenerConnection.Disconnect();
		TrackingRegistry.Unregister(_trackingEntry);
		_trackingEntry = TrackingRegistry.TrackingEntry.Empty;
	}

	private void OnMoved()
	{
		TrackingRegistry.Refresh(_trackingEntry);
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		bool flag2 = default(bool);
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
	}
}
