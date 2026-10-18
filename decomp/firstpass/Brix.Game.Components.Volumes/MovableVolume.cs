using Brix.External.Signals;
using Brix.Game.Network;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components.Volumes;

[RequireComponent(typeof(VoxelTransform))]
public class MovableVolume : Volume
{
	[EnumFlag]
	[SerializeField]
	private VolumeRegistryMode _registryMode = VolumeRegistryMode.OnStart | VolumeRegistryMode.OnMove | VolumeRegistryMode.OnParentTracked;

	protected SignalConnection OnMovedConnection = SignalConnection.Empty;

	protected SignalConnection ObjectNewlyTrackedConnection = SignalConnection.Empty;

	protected SignalConnection ObjectNewlyUnTrackedConnection = SignalConnection.Empty;

	protected GenericParentTracker parentTracker;

	public VolumeRegistryMode RegistryMode
	{
		get
		{
			return _registryMode;
		}
		set
		{
			VolumeRegistryMode value2 = _registryMode ^ value;
			_registryMode = value;
			if (!base.Requested)
			{
				return;
			}
			if (value2.HasFlag(VolumeRegistryMode.OnMove))
			{
				if (value.HasFlag(VolumeRegistryMode.OnMove))
				{
					ConnectToMovedSignal();
				}
				else
				{
					DisconnectFromMovedSignal();
				}
			}
			if (!value2.HasFlag(VolumeRegistryMode.OnParentTracked))
			{
				return;
			}
			if (value.HasFlag(VolumeRegistryMode.OnParentTracked))
			{
				ConnectToParentTrackerSignals();
				if (base.Registered && IsParentTracked)
				{
					Unregister();
				}
			}
			else
			{
				DisconnectFromParentTrackerSignals();
				if (!base.Registered && !IsParentTracked)
				{
					Register();
				}
			}
		}
	}

	private bool IsParentTracked => parentTracker != null && parentTracker.IsTracked();

	private void ConnectToParentTrackerSignals()
	{
		if (parentTracker != null)
		{
			ObjectNewlyTrackedConnection = parentTracker.ObjectNewlyTrackedSignal.Connect(delegate(GameObject go)
			{
				go.GetComponent<IVolume>().Unregister();
			});
			ObjectNewlyUnTrackedConnection = parentTracker.ObjectNewlyUntrackedSignal.Connect(delegate(GameObject go)
			{
				go.GetComponent<IVolume>().Register();
			});
		}
	}

	private void DisconnectFromParentTrackerSignals()
	{
		ObjectNewlyTrackedConnection.Disconnect();
		ObjectNewlyUnTrackedConnection.Disconnect();
	}

	private void ConnectToMovedSignal()
	{
		VoxelTransform component = GetComponent<VoxelTransform>();
		if (component != null)
		{
			OnMovedConnection = component.AddMoveListener(OnMoved);
		}
	}

	private void DisconnectFromMovedSignal()
	{
		OnMovedConnection.Disconnect();
	}

	private void OnMoved()
	{
		if (!RegistryMode.HasFlag(VolumeRegistryMode.OnParentTracked) || !IsParentTracked)
		{
			Register();
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		parentTracker = GetComponent<GenericParentTracker>();
	}

	public override void OnRequested()
	{
		base.OnRequested();
		if (RegistryMode.HasFlag(VolumeRegistryMode.OnParentTracked))
		{
			ConnectToParentTrackerSignals();
		}
		if (RegistryMode.HasFlag(VolumeRegistryMode.OnMove))
		{
			ConnectToMovedSignal();
		}
		if (RegistryMode.HasFlag(VolumeRegistryMode.OnStart))
		{
			Register();
		}
	}

	public override void OnStartAnywhere()
	{
		base.OnStartAnywhere();
		if (RegistryMode.HasFlag(VolumeRegistryMode.OnStart))
		{
			Register();
		}
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		DisconnectFromMovedSignal();
		DisconnectFromParentTrackerSignals();
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
