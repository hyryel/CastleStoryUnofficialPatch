using Brix.Engine;
using Brix.Game.Components.Volumes;
using UnityEngine;

namespace Brix.Game.AI;

public class OperableLocation : ObjectLocation
{
	public OperableLocation(IOperable<Labor> operable)
		: base(operable.gameObject)
	{
	}

	public override bool TargetReached(GameObject source, Vector3 position)
	{
		if (Object == null)
		{
			return false;
		}
		XYZ position2 = XYZ.FromVector3(position);
		return VolumeRegistry.HasVolumesWithPropertyAt(position2, VProperty.operation, IsTargetFunc);
	}
}
