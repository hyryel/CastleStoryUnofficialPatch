using Brix.Engine;
using UnityEngine;

namespace Brix.Game.Components.Volumes;

public interface IPassThrough
{
	bool IsWalkable();

	bool CanSearchThrough(GameObject character);

	bool CanPassThrough(GameObject character);

	int GetNiveauxAt(XYZ voxel);

	float GetWalkHeight(XYZ voxel, float dx, float dz);
}
