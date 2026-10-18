using UnityEngine.Networking;

namespace Brix.Game.Network;

public class GripParentTracker : GenericParentTracker
{
	public override TrackingTypes TrackingType()
	{
		return TrackingTypes.grip;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		bool flag2 = default(bool);
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
	}
}
