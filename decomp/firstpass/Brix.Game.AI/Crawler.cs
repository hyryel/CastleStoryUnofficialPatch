using UnityEngine.Networking;

namespace Brix.Game.AI;

public class Crawler : Minitron
{
	public override bool CanClimbWall()
	{
		return true;
	}

	public override bool CanBreakVoxels()
	{
		return false;
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
