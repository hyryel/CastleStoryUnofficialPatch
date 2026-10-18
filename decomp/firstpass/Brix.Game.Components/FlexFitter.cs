using Brix.Input;
using Brix.Utils;
using UnityEngine;

namespace Brix.Game.Components;

internal class FlexFitter : DefaultFitter
{
	public enum axis
	{
		Z,
		X,
		Both
	}

	[SerializeField]
	private axis alignTo;

	public override void FitAndOrient(VoxelRaycastHit voxelHit, out Vector3 absolutePosition)
	{
		Vector3 vector = FindFitPosition(voxelHit);
		base.gameObject.transform.rotation = _rotation;
		base.gameObject.transform.position = SnapOnBorder(vector, voxelHit.LocalHitVector);
		base.gameObject.transform.Translate(0f, 0f - _voxelTransform.Get.CenterOffset.y, 0f, Space.Self);
		absolutePosition = vector;
	}

	private bool AlignWith(axis axis)
	{
		return alignTo == axis.Both || axis == Alignment();
	}

	private Vector3 SnapOnBorder(Vector3 voxelHit, Vector3 localHit)
	{
		Vector3 result = voxelHit;
		if (AlignWith(axis.Z))
		{
			if (localHit.z < 0f)
			{
				result.z -= 0.5f;
			}
			else
			{
				result.z += 0.5f;
			}
		}
		if (AlignWith(axis.X))
		{
			if (localHit.x < 0f)
			{
				result.x -= 0.5f;
			}
			else
			{
				result.x += 0.5f;
			}
		}
		return result;
	}

	private axis Alignment()
	{
		if (alignTo == axis.Both)
		{
			return axis.Both;
		}
		if (IsRotated())
		{
			if (alignTo == axis.X)
			{
				return axis.Z;
			}
			if (alignTo == axis.Z)
			{
				return axis.X;
			}
		}
		return alignTo;
	}

	private bool IsRotated()
	{
		return _voxelTransform.Get.AxisOrient == AxisOrient.left || _voxelTransform.Get.AxisOrient == AxisOrient.right;
	}

	public override IFitter CopyTo(GameObject go)
	{
		Object.Destroy((MonoBehaviour)go.GetInterface<IFitter>());
		FlexFitter flexFitter = go.AddComponent<FlexFitter>();
		flexFitter.CloneInternals(this);
		return flexFitter;
	}
}
