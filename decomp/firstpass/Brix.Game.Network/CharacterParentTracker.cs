using Brix.Game.AI;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Network;

public class CharacterParentTracker : GenericParentTracker
{
	private Project project;

	public Project Project
	{
		get
		{
			if (currentTracker.IsValid)
			{
				return project;
			}
			return null;
		}
	}

	protected override void OnTrackNewParent(GameObject o)
	{
		project = currentTracker.go.GetComponent<Project>();
	}

	public override TrackingTypes TrackingType()
	{
		return TrackingTypes.character;
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
