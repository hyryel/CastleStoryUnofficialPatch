using Brix.Engine;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components.Tunnel;

public class TunnelNetworkDeserialization : NetworkBehaviour
{
	public class SyncListVector : SyncListStruct<Vector3>
	{
		public override void SerializeItem(NetworkWriter writer, Vector3 item)
		{
			writer.Write(item.x);
			writer.Write(item.y);
			writer.Write(item.z);
		}

		public override Vector3 DeserializeItem(NetworkReader reader)
		{
			return new Vector3
			{
				x = reader.ReadSingle(),
				y = reader.ReadSingle(),
				z = reader.ReadSingle()
			};
		}
	}

	public SyncListVector UnitsPositionsNetwork = new SyncListVector();

	private static int kListUnitsPositionsNetwork;

	private void Awake()
	{
		if (!NetworkServer.active)
		{
			UnitsPositionsNetwork.Callback = OnNetworkListChanged;
		}
		UnitsPositionsNetwork.InitializeBehaviour(this, kListUnitsPositionsNetwork);
	}

	private void Start()
	{
	}

	private void OnNetworkListChanged(SyncList<Vector3>.Operation op, int index)
	{
		if (op == SyncList<Vector3>.Operation.OP_ADD)
		{
			GetComponent<TunnelProject>().tunnelVolume.Add(XYZ.FromVector3(UnitsPositionsNetwork[index]), Camera.main.transform.forward, TunnelType.straight, debug: false, deserializing: true);
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeSyncListUnitsPositionsNetwork(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("SyncList UnitsPositionsNetwork called on server.");
		}
		else
		{
			((TunnelNetworkDeserialization)obj).UnitsPositionsNetwork.HandleMsg(reader);
		}
	}

	static TunnelNetworkDeserialization()
	{
		kListUnitsPositionsNetwork = -1751608184;
		NetworkBehaviour.RegisterSyncListDelegate(typeof(TunnelNetworkDeserialization), kListUnitsPositionsNetwork, InvokeSyncListUnitsPositionsNetwork);
		NetworkCRC.RegisterBehaviour("TunnelNetworkDeserialization", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			GeneratedNetworkCode._WriteStructSyncListVector_TunnelNetworkDeserialization(writer, UnitsPositionsNetwork);
			return true;
		}
		bool flag = false;
		if ((base.syncVarDirtyBits & 1) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			GeneratedNetworkCode._WriteStructSyncListVector_TunnelNetworkDeserialization(writer, UnitsPositionsNetwork);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			GeneratedNetworkCode._ReadStructSyncListVector_TunnelNetworkDeserialization(reader, UnitsPositionsNetwork);
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			GeneratedNetworkCode._ReadStructSyncListVector_TunnelNetworkDeserialization(reader, UnitsPositionsNetwork);
		}
	}
}
