using System;
using UnityEngine;

namespace Brix.Input;

[Serializable]
public class HeightConfig
{
	public HeightModes mode;

	public int value;

	public float Compute(Bounds bounds, int surfaceValue)
	{
		if (mode == HeightModes.TerrainMinMax)
		{
			return surfaceValue;
		}
		if (mode == HeightModes.TerrainMinMaxPadded)
		{
			return surfaceValue + value;
		}
		if (mode == HeightModes.Absolute)
		{
			return value;
		}
		if (mode == HeightModes.Relative)
		{
			return bounds.center.y + (float)value;
		}
		throw new ArgumentOutOfRangeException();
	}
}
