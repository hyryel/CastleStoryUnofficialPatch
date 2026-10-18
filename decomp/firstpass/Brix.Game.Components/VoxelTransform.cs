using System;
using Brix.Engine;
using Brix.External.Signals;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components;

public class VoxelTransform : BaseVoxelTransform
{
	protected Signal VoxelTransformChangedSignal = new Signal();

	private bool _positionIsValid;

	protected XYZ _position;

	private bool _rotationIndexIsValid;

	protected int _rotationIndex;

	public override Vector3 PlacementOffset
	{
		get
		{
			return placementOffset;
		}
		set
		{
			base.NetworkplacementOffset = value;
			_positionIsValid = false;
		}
	}

	public override bool MoveTransform(Vector3 p)
	{
		XYZ xYZ = _position;
		if (!base.MoveTransform(p))
		{
			return false;
		}
		_position = base.position;
		if (xYZ == _position)
		{
			return false;
		}
		VoxelTransformChangedSignal.Invoke();
		return true;
	}

	public override bool MovePivot(XYZ p)
	{
		if (!base.MovePivot(p))
		{
			return false;
		}
		VoxelTransformChangedSignal.Invoke();
		return true;
	}

	public SignalConnection AddMoveListener(Action onMoved)
	{
		return VoxelTransformChangedSignal.Connect(onMoved);
	}

	protected override void Awake()
	{
		base.Awake();
		InvalidateCache();
	}

	protected override XYZ PositionToXYZ()
	{
		if (!base.transform.hasChanged)
		{
			if (_positionIsValid)
			{
				return _position;
			}
		}
		else
		{
			InvalidateCache();
		}
		_position = base.PositionToXYZ();
		_positionIsValid = true;
		return _position;
	}

	protected override int RotationToRotationIndex()
	{
		if (!base.transform.hasChanged)
		{
			if (_rotationIndexIsValid)
			{
				return _rotationIndex;
			}
		}
		else
		{
			InvalidateCache();
		}
		_rotationIndex = base.RotationToRotationIndex();
		_rotationIndexIsValid = true;
		return _rotationIndex;
	}

	protected override void XYZToPosition(XYZ xyz)
	{
		_position = xyz;
		base.XYZToPosition(xyz);
		InvalidateCache();
		_positionIsValid = true;
	}

	protected override void RotationIndexToRotation(int rotationIndex)
	{
		_rotationIndex = rotationIndex;
		base.RotationIndexToRotation(rotationIndex);
		InvalidateCache();
		_rotationIndexIsValid = true;
	}

	protected virtual void InvalidateCache()
	{
		base.transform.hasChanged = false;
		_positionIsValid = false;
		_rotationIndexIsValid = false;
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
