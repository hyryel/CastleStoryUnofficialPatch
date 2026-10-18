using System;
using Brix.Engine;
using UnityEngine;

namespace Brix.Input;

[Serializable]
public class HeightConstraint
{
	public HeightConfig min;

	public HeightConfig max;

	public void Compute(Bounds b, out float minResult, out float maxResult)
	{
		int surfaceValue = 0;
		int surfaceValue2 = 0;
		if (RequiresSurfaceCheck())
		{
			Architecte.TopAndBottomInBounds(VoxelBounds.FromVectorBounds(b), out var top, out var bottom);
			surfaceValue = bottom.y;
			surfaceValue2 = top.y;
		}
		minResult = min.Compute(b, surfaceValue);
		maxResult = max.Compute(b, surfaceValue2);
	}

	public bool RequiresSurfaceCheck()
	{
		return min.mode == HeightModes.TerrainMinMax || max.mode == HeightModes.TerrainMinMax || min.mode == HeightModes.TerrainMinMaxPadded || max.mode == HeightModes.TerrainMinMaxPadded;
	}
}
