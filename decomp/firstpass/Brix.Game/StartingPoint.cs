using System.Collections.Generic;
using Brix.Engine;
using Brix.Legacy;
using Brix.Network;
using UnityEngine.Networking;

namespace Brix.Game;

public class StartingPoint : NetworkGameComponent
{
	private Beacon balise => GetComponent<Beacon>();

	public XYZ position
	{
		get
		{
			return balise.position;
		}
		set
		{
			balise.position = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		GameSignals.Invoke(GameSignals.StartingPointLoaded, this);
	}

	public override bool OnRebuildObservers(HashSet<NetworkConnection> observers, bool initial)
	{
		return true;
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
