using Brix.Engine;

namespace Brix.Utils.Locomotion;

public static class XYZLocoExtension
{
	public static XYZ Flatten(this XYZ a)
	{
		return new XYZ(a.x, 0, a.z);
	}

	public static int Manhattan(this XYZ a)
	{
		return XYZ.ManhattanMagnitude(a);
	}
}
