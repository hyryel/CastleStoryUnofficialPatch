using System.Collections.Generic;
using Brix.Components;
using Brix.Input;

namespace Brix.Engine.Trackers;

public class GeostaticityTracker : BrixSingleton<GeostaticityTracker>
{
	private readonly HashSet<XYZ> _geostaticVoxels = new HashSet<XYZ>();

	public static void SetGeostatique(XYZ position)
	{
		BrixSingleton<GeostaticityTracker>.Instance._geostaticVoxels.Add(position);
	}

	public static void ClearGeostatique(XYZ position)
	{
		BrixSingleton<GeostaticityTracker>.Instance._geostaticVoxels.Remove(position);
	}

	public static bool IsGeostatic(XYZ position)
	{
		if (InputModeController.GetCurrentMode() == InputMode.worldEditor)
		{
			return true;
		}
		return BrixSingleton<GeostaticityTracker>.Instance._geostaticVoxels.Contains(position);
	}
}
