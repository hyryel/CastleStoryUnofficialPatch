using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Game.Semantique;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Network;

public class UNetRecepteurCmd : NetworkBehaviour
{
	private static int kCmdCmdAddExclusion;

	private static int kRpcRpcAddExclusion;

	private static int kCmdCmdRemoveExclusion;

	private static int kRpcRpcRemoveExclusion;

	private static int kCmdCmdManualConversion;

	private static int kCmdCmdSetAutoConvert;

	[Command]
	public void CmdAddExclusion(GameObject recepteurGO, string ressourceString)
	{
		Recepteur component = recepteurGO.GetComponent<Recepteur>();
		Adjectif adjectif = Adjectif.StringToAdjectif(ressourceString);
		component._queuedAddedExclusion = (Ressource)adjectif;
		component._AddExclusion((Ressource)adjectif);
		CallRpcAddExclusion(recepteurGO, ressourceString);
	}

	[ClientRpc]
	public void RpcAddExclusion(GameObject recepteurGO, string ressourceString)
	{
		if (!NetworkServer.active && !recepteurGO.IsNullOrReleased())
		{
			Recepteur component = recepteurGO.GetComponent<Recepteur>();
			Adjectif adjectif = Adjectif.StringToAdjectif(ressourceString);
			component._queuedAddedExclusion = (Ressource)adjectif;
			component._AddExclusion((Ressource)adjectif);
		}
	}

	[Command]
	public void CmdRemoveExclusion(GameObject recepteurGO, string ressourceString)
	{
		Recepteur component = recepteurGO.GetComponent<Recepteur>();
		Adjectif adjectif = Adjectif.StringToAdjectif(ressourceString);
		component._queuedRemovedExclusion = (Ressource)adjectif;
		component._RemoveExclusion((Ressource)adjectif);
		CallRpcRemoveExclusion(recepteurGO, ressourceString);
	}

	[ClientRpc]
	public void RpcRemoveExclusion(GameObject recepteurGO, string ressourceString)
	{
		if (!recepteurGO.IsNullOrReleased())
		{
			Recepteur component = recepteurGO.GetComponent<Recepteur>();
			Adjectif adjectif = Adjectif.StringToAdjectif(ressourceString);
			component._queuedRemovedExclusion = (Ressource)adjectif;
			component._RemoveExclusion((Ressource)adjectif);
		}
	}

	[Command]
	public void CmdManualConversion(GameObject RessourceConverterGO)
	{
		ResourceConverter component = RessourceConverterGO.GetComponent<ResourceConverter>();
		component.CurrentConvertRule?.WipeAndFill(component.GetComponent<Recepteur>());
	}

	[Command]
	public void CmdSetAutoConvert(GameObject RessourceConverterGO, bool value)
	{
		if (Affiliation.IsOwnedBy(RessourceConverterGO, base.gameObject))
		{
			RessourceConverterGO.GetComponent<ResourceConverter>().UpdateAutoConvert(value);
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdAddExclusion(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAddExclusion called on client.");
		}
		else
		{
			((UNetRecepteurCmd)obj).CmdAddExclusion(reader.ReadGameObject(), reader.ReadString());
		}
	}

	protected static void InvokeCmdCmdRemoveExclusion(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRemoveExclusion called on client.");
		}
		else
		{
			((UNetRecepteurCmd)obj).CmdRemoveExclusion(reader.ReadGameObject(), reader.ReadString());
		}
	}

	protected static void InvokeCmdCmdManualConversion(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdManualConversion called on client.");
		}
		else
		{
			((UNetRecepteurCmd)obj).CmdManualConversion(reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdSetAutoConvert(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetAutoConvert called on client.");
		}
		else
		{
			((UNetRecepteurCmd)obj).CmdSetAutoConvert(reader.ReadGameObject(), reader.ReadBoolean());
		}
	}

	public void CallCmdAddExclusion(GameObject recepteurGO, string ressourceString)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdAddExclusion called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdAddExclusion(recepteurGO, ressourceString);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdAddExclusion);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(recepteurGO);
		networkWriter.Write(ressourceString);
		SendCommandInternal(networkWriter, 0, "CmdAddExclusion");
	}

	public void CallCmdRemoveExclusion(GameObject recepteurGO, string ressourceString)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRemoveExclusion called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRemoveExclusion(recepteurGO, ressourceString);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRemoveExclusion);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(recepteurGO);
		networkWriter.Write(ressourceString);
		SendCommandInternal(networkWriter, 0, "CmdRemoveExclusion");
	}

	public void CallCmdManualConversion(GameObject RessourceConverterGO)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdManualConversion called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdManualConversion(RessourceConverterGO);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdManualConversion);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(RessourceConverterGO);
		SendCommandInternal(networkWriter, 0, "CmdManualConversion");
	}

	public void CallCmdSetAutoConvert(GameObject RessourceConverterGO, bool value)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetAutoConvert called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetAutoConvert(RessourceConverterGO, value);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetAutoConvert);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(RessourceConverterGO);
		networkWriter.Write(value);
		SendCommandInternal(networkWriter, 0, "CmdSetAutoConvert");
	}

	protected static void InvokeRpcRpcAddExclusion(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAddExclusion called on server.");
		}
		else
		{
			((UNetRecepteurCmd)obj).RpcAddExclusion(reader.ReadGameObject(), reader.ReadString());
		}
	}

	protected static void InvokeRpcRpcRemoveExclusion(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRemoveExclusion called on server.");
		}
		else
		{
			((UNetRecepteurCmd)obj).RpcRemoveExclusion(reader.ReadGameObject(), reader.ReadString());
		}
	}

	public void CallRpcAddExclusion(GameObject recepteurGO, string ressourceString)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcAddExclusion called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcAddExclusion);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(recepteurGO);
		networkWriter.Write(ressourceString);
		SendRPCInternal(networkWriter, 0, "RpcAddExclusion");
	}

	public void CallRpcRemoveExclusion(GameObject recepteurGO, string ressourceString)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcRemoveExclusion called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcRemoveExclusion);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(recepteurGO);
		networkWriter.Write(ressourceString);
		SendRPCInternal(networkWriter, 0, "RpcRemoveExclusion");
	}

	static UNetRecepteurCmd()
	{
		kCmdCmdAddExclusion = -1355738586;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetRecepteurCmd), kCmdCmdAddExclusion, InvokeCmdCmdAddExclusion);
		kCmdCmdRemoveExclusion = -709721327;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetRecepteurCmd), kCmdCmdRemoveExclusion, InvokeCmdCmdRemoveExclusion);
		kCmdCmdManualConversion = 1213099253;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetRecepteurCmd), kCmdCmdManualConversion, InvokeCmdCmdManualConversion);
		kCmdCmdSetAutoConvert = 1990790395;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetRecepteurCmd), kCmdCmdSetAutoConvert, InvokeCmdCmdSetAutoConvert);
		kRpcRpcAddExclusion = -1272306436;
		NetworkBehaviour.RegisterRpcDelegate(typeof(UNetRecepteurCmd), kRpcRpcAddExclusion, InvokeRpcRpcAddExclusion);
		kRpcRpcRemoveExclusion = -1968605061;
		NetworkBehaviour.RegisterRpcDelegate(typeof(UNetRecepteurCmd), kRpcRpcRemoveExclusion, InvokeRpcRpcRemoveExclusion);
		NetworkCRC.RegisterBehaviour("UNetRecepteurCmd", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = default(bool);
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}
}
