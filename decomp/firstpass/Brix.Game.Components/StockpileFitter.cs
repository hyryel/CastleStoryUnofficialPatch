using Brix.Input;
using UnityEngine;

namespace Brix.Game.Components;

internal class StockpileFitter : DefaultFitter
{
	public override void FitAndOrient(VoxelRaycastHit voxelHit, out Vector3 absolutePosition)
	{
		Vector3 vector = FindFitPosition(voxelHit);
		base.gameObject.transform.rotation = _rotation;
		base.gameObject.transform.position = SnapOnCorner(vector, voxelHit.LocalHitVector);
		base.gameObject.transform.Translate(0f, 0f - _voxelTransform.Get.CenterOffset.y, 0f, Space.Self);
		absolutePosition = vector;
	}

	public Vector3 SnapOnCorner(Vector3 voxelHit, Vector3 localHit)
	{
		Vector3 result = voxelHit;
		if (localHit.x < 0f)
		{
			result.x -= 0.5f;
		}
		else
		{
			result.x += 0.5f;
		}
		if (localHit.z < 0f)
		{
			result.z -= 0.5f;
		}
		else
		{
			result.z += 0.5f;
		}
		return result;
	}

	public override IFitter CopyTo(GameObject go)
	{
		Object.Destroy((MonoBehaviour)go.GetInterface<IFitter>());
		StockpileFitter stockpileFitter = go.AddComponent<StockpileFitter>();
		stockpileFitter.CloneInternals(this);
		return stockpileFitter;
	}
}
