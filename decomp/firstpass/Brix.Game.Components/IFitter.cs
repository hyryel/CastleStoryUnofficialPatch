using Brix.Input;
using UnityEngine;

namespace Brix.Game.Components;

public interface IFitter
{
	void FitAndOrient(VoxelRaycastHit voxelHit);

	void FitAndOrient(VoxelRaycastHit voxelHit, out Vector3 absolutePosition);

	void FitForDrag(VoxelRaycastHit voxelHit);

	void FitForDrag(VoxelRaycastHit voxelHit, out Vector3 absolutePosition);

	void ChangeFit();

	void CloneInternals(IFitter original);

	IFitter CopyTo(GameObject go);
}
