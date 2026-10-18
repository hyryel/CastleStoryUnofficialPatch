using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.IO.Serialization;

[JsonObject(MemberSerialization.OptIn)]
public class TestComponentWithSyncList : NetworkBehaviour
{
	public class GOSyncList : SyncList<GameObject>
	{
		protected override void SerializeItem(NetworkWriter writer, GameObject item)
		{
			throw new NotImplementedException();
		}

		protected override GameObject DeserializeItem(NetworkReader reader)
		{
			throw new NotImplementedException();
		}
	}

	[JsonProperty]
	public GOSyncList list = new GOSyncList();
}
