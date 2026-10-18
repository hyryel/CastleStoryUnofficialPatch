using Brix.Engine;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components;

public class TrackableVoxelTransform : VoxelTransform
{
	private static readonly UpdatableNetworkProfiler<TrackableVoxelTransform> UpdatableProfiler = new UpdatableNetworkProfiler<TrackableVoxelTransform>();

	[SerializeField]
	private bool trackRotation = true;

	private Quaternion _lastRotation;

	private int _lastRotationIndex;

	private XYZ _lastPosition;

	protected override UpdatableProfiler Updatable => UpdatableProfiler;

	protected override void Awake()
	{
		base.Awake();
		TrackRotation();
		TrackPosition();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_lastPosition = XYZ.zero;
		_lastRotation = Quaternion.identity;
	}

	protected override void BrixFixedUpdate()
	{
		Track();
	}

	public void Track()
	{
		bool flag = TrackPosition();
		if ((trackRotation && TrackRotation()) || flag)
		{
			SendMoveSignal();
		}
	}

	private bool TrackRotation()
	{
		Quaternion rotation = base.transform.rotation;
		if (_lastRotation != rotation)
		{
			_lastRotation = rotation;
			int rotationIndex = base.RotationIndex;
			if (rotationIndex != _lastRotationIndex)
			{
				_lastRotationIndex = rotationIndex;
				return true;
			}
		}
		return false;
	}

	private bool TrackPosition()
	{
		XYZ xYZ = base.position;
		if (xYZ != _lastPosition)
		{
			_lastPosition = xYZ;
			return true;
		}
		return false;
	}

	private void SendMoveSignal()
	{
		VoxelTransformChangedSignal.Invoke();
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
