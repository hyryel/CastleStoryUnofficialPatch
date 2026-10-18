using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Network;

public class ToolParentTracker : GenericParentTracker
{
	[JsonProperty]
	[SerializeField]
	private TrackingTypes trackedType = TrackingTypes.ressource;

	public override TrackingTypes TrackingType()
	{
		return trackedType;
	}

	public void SetTrackerType(TrackingTypes orderType)
	{
		trackedType = orderType;
		ForceRefresh();
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
