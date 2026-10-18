using Brix.Engine.Nature;
using Brix.Input;
using UnityEngine;

namespace Brix.UI.GameObserver;

public class PlantHoverTracker : NatureHoverTracker
{
	protected override Vector3 GetExactNaturePosition(VoxelRaycastHit vrh)
	{
		return PlantRegistry.ExactPositionOfPlantAt(vrh.VoxelPosition);
	}
}
