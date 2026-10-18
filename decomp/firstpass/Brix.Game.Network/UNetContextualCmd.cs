using Brix.Engine;
using Brix.Engine.Nature;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Input;
using Brix.Input.ContextualOrders;
using Brix.Legacy;
using Brix.Utils;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Network;

public class UNetContextualCmd : NetworkBehaviour
{
	private static int kCmdCmdBatchedContextualActionVector;

	private static int kCmdCmdBatchedContextualActionGameObject;

	private static int kCmdCmdRemovePlant;

	private static int kRpcRpcRemovePlant;

	private static int kCmdCmdLockDoor;

	private static int kCmdCmdJumpOff;

	private static int kCmdCmdGoAndJumpTo;

	private static int kCmdCmdFireAtObject;

	private static int kCmdCmdFireAt;

	private static int kCmdCmdTurnTo;

	private static int kCmdCmdStop;

	private static int kCmdCmdStartBarrelFuse;

	private static int kCmdCmdEquipTool;

	private static int kCmdCmdUnequipIntoRack;

	private static int kCmdCmdUnequipAllTools;

	private static int kCmdCmdEquipToolRackContent;

	[Command]
	public void CmdBatchedContextualActionVector(GameObject[] arrayGO, Vector3 target, ActionType actionType)
	{
		ContextualActionGroupData contextualActionGroupData = ObjectPool<ContextualActionGroupData>.Reserve().Init(arrayGO);
		foreach (GameObject gameObject in arrayGO)
		{
			AffiliationComponent component = gameObject.GetComponent<AffiliationComponent>();
			if (!component || component.faction.OwnerSyncList.Contains(base.gameObject))
			{
				ContextualAction contextualAction = new ContextualAction(gameObject, contextualActionGroupData, target, actionType, PickingUtility.isInQueueMode());
				contextualAction.Resolve();
			}
		}
		ObjectPool<ContextualActionGroupData>.Release(contextualActionGroupData);
	}

	[Command]
	public void CmdBatchedContextualActionGameObject(GameObject[] arrayGO, GameObject target, ActionType actionType)
	{
		ContextualActionGroupData contextualActionGroupData = ObjectPool<ContextualActionGroupData>.Reserve().Init(arrayGO);
		foreach (GameObject gameObject in arrayGO)
		{
			AffiliationComponent component = gameObject.GetComponent<AffiliationComponent>();
			if (!component || component.faction.OwnerSyncList.Contains(base.gameObject))
			{
				ContextualAction contextualAction = new ContextualAction(gameObject, contextualActionGroupData, target, actionType, PickingUtility.isInQueueMode());
				contextualAction.Resolve();
			}
		}
		ObjectPool<ContextualActionGroupData>.Release(contextualActionGroupData);
	}

	[Command]
	public void CmdRemovePlant(Vector3 p)
	{
		CallRpcRemovePlant(p);
	}

	[ClientRpc]
	public void RpcRemovePlant(Vector3 p)
	{
		PlantRegistry.Remove(XYZ.FromVector3(p));
	}

	[Command]
	public void CmdLockDoor(GameObject instance, bool value)
	{
		if (Affiliation.IsOwnedBy(instance, base.gameObject))
		{
			Porte component = instance.GetComponent<Porte>();
			component.locked = value;
		}
	}

	[Command]
	public void CmdJumpOff(GameObject instance, bool queued)
	{
		if (Affiliation.IsOwnedBy(instance, base.gameObject))
		{
			Labor component = instance.GetComponent<Labor>();
			Order.Move.JumpOff.Send(component, Labor.DecisionSource.User);
		}
	}

	[Command]
	public void CmdGoAndJumpTo(GameObject instance, Vector3 position)
	{
		if (Affiliation.IsOwnedBy(instance, base.gameObject))
		{
			Labor component = instance.GetComponent<Labor>();
			Order.Move.TurnAndJumpToward.Send(component, Location.Of(position), Labor.DecisionSource.User);
		}
	}

	[Command]
	public void CmdFireAtObject(GameObject instance, GameObject target)
	{
		if (Affiliation.IsOwnedBy(instance, base.gameObject))
		{
			Order.Combat.ShootAt.Send(instance.GetComponent<Labor>(), Location.Of(target), Labor.DecisionSource.User);
		}
	}

	[Command]
	public void CmdFireAt(GameObject instance, Vector3 location)
	{
		if (Affiliation.IsOwnedBy(instance, base.gameObject))
		{
			Order.Combat.ShootAt.Send(instance.GetComponent<Labor>(), Location.Of(location), Labor.DecisionSource.User);
		}
	}

	[Command]
	public void CmdTurnTo(GameObject instance, Vector3 location)
	{
		if (Affiliation.IsOwnedBy(instance, base.gameObject))
		{
			Order.Move.TurnTo.Send(instance.GetComponent<Labor>(), Location.Of(location), Labor.DecisionSource.User);
		}
	}

	[Command]
	public void CmdStop(GameObject instance)
	{
		if (Affiliation.IsOwnedBy(instance, base.gameObject))
		{
			instance.GetComponent<Labor>().AbortWorkSignal.Invoke(TaskResult.ReceivedNewInstruction);
		}
	}

	[Command]
	public void CmdStartBarrelFuse(GameObject instance)
	{
		if (Affiliation.IsOwnedBy(instance, base.gameObject))
		{
			instance.GetComponent<Explosifs>().CallRpcStartFuse();
		}
	}

	[Command]
	public void CmdEquipTool(GameObject labor, GameObject tool)
	{
		if (Affiliation.IsOwnedBy(labor, base.gameObject))
		{
			Order.AboutTools.GoEquipTool.Send(labor.GetComponent<Labor>(), tool, Labor.DecisionSource.User);
		}
	}

	[Command]
	public void CmdUnequipIntoRack(GameObject labor, GameObject rack)
	{
		if (Affiliation.IsOwnedBy(labor, base.gameObject))
		{
			Order.AboutTools.GoStoreInRack.Send(labor.GetComponent<Labor>(), rack, Labor.DecisionSource.User);
		}
	}

	[Command]
	public void CmdUnequipAllTools(GameObject labor)
	{
		if (Affiliation.IsOwnedBy(labor, base.gameObject))
		{
			Order.AboutTools.UnequipTool.Send(labor.GetComponent<Labor>(), Labor.DecisionSource.User);
		}
	}

	[Command]
	public void CmdEquipToolRackContent(GameObject labor, GameObject rack)
	{
		if (Affiliation.IsOwnedBy(labor, base.gameObject))
		{
			Order.AboutTools.GoEquipRackContent.Send(labor.GetComponent<Labor>(), rack, Labor.DecisionSource.User);
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdBatchedContextualActionVector(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdBatchedContextualActionVector called on client.");
		}
		else
		{
			((UNetContextualCmd)obj).CmdBatchedContextualActionVector(GeneratedNetworkCode._ReadArrayGameObject_None(reader), reader.ReadVector3(), (ActionType)reader.ReadInt32());
		}
	}

	protected static void InvokeCmdCmdBatchedContextualActionGameObject(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdBatchedContextualActionGameObject called on client.");
		}
		else
		{
			((UNetContextualCmd)obj).CmdBatchedContextualActionGameObject(GeneratedNetworkCode._ReadArrayGameObject_None(reader), reader.ReadGameObject(), (ActionType)reader.ReadInt32());
		}
	}

	protected static void InvokeCmdCmdRemovePlant(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRemovePlant called on client.");
		}
		else
		{
			((UNetContextualCmd)obj).CmdRemovePlant(reader.ReadVector3());
		}
	}

	protected static void InvokeCmdCmdLockDoor(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdLockDoor called on client.");
		}
		else
		{
			((UNetContextualCmd)obj).CmdLockDoor(reader.ReadGameObject(), reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdJumpOff(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdJumpOff called on client.");
		}
		else
		{
			((UNetContextualCmd)obj).CmdJumpOff(reader.ReadGameObject(), reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdGoAndJumpTo(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdGoAndJumpTo called on client.");
		}
		else
		{
			((UNetContextualCmd)obj).CmdGoAndJumpTo(reader.ReadGameObject(), reader.ReadVector3());
		}
	}

	protected static void InvokeCmdCmdFireAtObject(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdFireAtObject called on client.");
		}
		else
		{
			((UNetContextualCmd)obj).CmdFireAtObject(reader.ReadGameObject(), reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdFireAt(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdFireAt called on client.");
		}
		else
		{
			((UNetContextualCmd)obj).CmdFireAt(reader.ReadGameObject(), reader.ReadVector3());
		}
	}

	protected static void InvokeCmdCmdTurnTo(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTurnTo called on client.");
		}
		else
		{
			((UNetContextualCmd)obj).CmdTurnTo(reader.ReadGameObject(), reader.ReadVector3());
		}
	}

	protected static void InvokeCmdCmdStop(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdStop called on client.");
		}
		else
		{
			((UNetContextualCmd)obj).CmdStop(reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdStartBarrelFuse(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdStartBarrelFuse called on client.");
		}
		else
		{
			((UNetContextualCmd)obj).CmdStartBarrelFuse(reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdEquipTool(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdEquipTool called on client.");
		}
		else
		{
			((UNetContextualCmd)obj).CmdEquipTool(reader.ReadGameObject(), reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdUnequipIntoRack(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUnequipIntoRack called on client.");
		}
		else
		{
			((UNetContextualCmd)obj).CmdUnequipIntoRack(reader.ReadGameObject(), reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdUnequipAllTools(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUnequipAllTools called on client.");
		}
		else
		{
			((UNetContextualCmd)obj).CmdUnequipAllTools(reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdEquipToolRackContent(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdEquipToolRackContent called on client.");
		}
		else
		{
			((UNetContextualCmd)obj).CmdEquipToolRackContent(reader.ReadGameObject(), reader.ReadGameObject());
		}
	}

	public void CallCmdBatchedContextualActionVector(GameObject[] arrayGO, Vector3 target, ActionType actionType)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdBatchedContextualActionVector called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdBatchedContextualActionVector(arrayGO, target, actionType);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdBatchedContextualActionVector);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		GeneratedNetworkCode._WriteArrayGameObject_None(networkWriter, arrayGO);
		networkWriter.Write(target);
		networkWriter.Write((int)actionType);
		SendCommandInternal(networkWriter, 0, "CmdBatchedContextualActionVector");
	}

	public void CallCmdBatchedContextualActionGameObject(GameObject[] arrayGO, GameObject target, ActionType actionType)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdBatchedContextualActionGameObject called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdBatchedContextualActionGameObject(arrayGO, target, actionType);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdBatchedContextualActionGameObject);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		GeneratedNetworkCode._WriteArrayGameObject_None(networkWriter, arrayGO);
		networkWriter.Write(target);
		networkWriter.Write((int)actionType);
		SendCommandInternal(networkWriter, 0, "CmdBatchedContextualActionGameObject");
	}

	public void CallCmdRemovePlant(Vector3 p)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRemovePlant called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRemovePlant(p);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRemovePlant);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(p);
		SendCommandInternal(networkWriter, 0, "CmdRemovePlant");
	}

	public void CallCmdLockDoor(GameObject instance, bool value)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdLockDoor called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdLockDoor(instance, value);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdLockDoor);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(instance);
		networkWriter.Write(value);
		SendCommandInternal(networkWriter, 0, "CmdLockDoor");
	}

	public void CallCmdJumpOff(GameObject instance, bool queued)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdJumpOff called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdJumpOff(instance, queued);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdJumpOff);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(instance);
		networkWriter.Write(queued);
		SendCommandInternal(networkWriter, 0, "CmdJumpOff");
	}

	public void CallCmdGoAndJumpTo(GameObject instance, Vector3 position)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdGoAndJumpTo called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdGoAndJumpTo(instance, position);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdGoAndJumpTo);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(instance);
		networkWriter.Write(position);
		SendCommandInternal(networkWriter, 0, "CmdGoAndJumpTo");
	}

	public void CallCmdFireAtObject(GameObject instance, GameObject target)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdFireAtObject called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdFireAtObject(instance, target);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdFireAtObject);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(instance);
		networkWriter.Write(target);
		SendCommandInternal(networkWriter, 0, "CmdFireAtObject");
	}

	public void CallCmdFireAt(GameObject instance, Vector3 location)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdFireAt called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdFireAt(instance, location);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdFireAt);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(instance);
		networkWriter.Write(location);
		SendCommandInternal(networkWriter, 0, "CmdFireAt");
	}

	public void CallCmdTurnTo(GameObject instance, Vector3 location)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdTurnTo called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdTurnTo(instance, location);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdTurnTo);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(instance);
		networkWriter.Write(location);
		SendCommandInternal(networkWriter, 0, "CmdTurnTo");
	}

	public void CallCmdStop(GameObject instance)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdStop called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdStop(instance);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdStop);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(instance);
		SendCommandInternal(networkWriter, 0, "CmdStop");
	}

	public void CallCmdStartBarrelFuse(GameObject instance)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdStartBarrelFuse called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdStartBarrelFuse(instance);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdStartBarrelFuse);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(instance);
		SendCommandInternal(networkWriter, 0, "CmdStartBarrelFuse");
	}

	public void CallCmdEquipTool(GameObject labor, GameObject tool)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdEquipTool called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdEquipTool(labor, tool);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdEquipTool);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(labor);
		networkWriter.Write(tool);
		SendCommandInternal(networkWriter, 0, "CmdEquipTool");
	}

	public void CallCmdUnequipIntoRack(GameObject labor, GameObject rack)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdUnequipIntoRack called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdUnequipIntoRack(labor, rack);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdUnequipIntoRack);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(labor);
		networkWriter.Write(rack);
		SendCommandInternal(networkWriter, 0, "CmdUnequipIntoRack");
	}

	public void CallCmdUnequipAllTools(GameObject labor)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdUnequipAllTools called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdUnequipAllTools(labor);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdUnequipAllTools);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(labor);
		SendCommandInternal(networkWriter, 0, "CmdUnequipAllTools");
	}

	public void CallCmdEquipToolRackContent(GameObject labor, GameObject rack)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdEquipToolRackContent called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdEquipToolRackContent(labor, rack);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdEquipToolRackContent);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(labor);
		networkWriter.Write(rack);
		SendCommandInternal(networkWriter, 0, "CmdEquipToolRackContent");
	}

	protected static void InvokeRpcRpcRemovePlant(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRemovePlant called on server.");
		}
		else
		{
			((UNetContextualCmd)obj).RpcRemovePlant(reader.ReadVector3());
		}
	}

	public void CallRpcRemovePlant(Vector3 p)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcRemovePlant called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcRemovePlant);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(p);
		SendRPCInternal(networkWriter, 0, "RpcRemovePlant");
	}

	static UNetContextualCmd()
	{
		kCmdCmdBatchedContextualActionVector = -43898622;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetContextualCmd), kCmdCmdBatchedContextualActionVector, InvokeCmdCmdBatchedContextualActionVector);
		kCmdCmdBatchedContextualActionGameObject = -622465520;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetContextualCmd), kCmdCmdBatchedContextualActionGameObject, InvokeCmdCmdBatchedContextualActionGameObject);
		kCmdCmdRemovePlant = -1690233530;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetContextualCmd), kCmdCmdRemovePlant, InvokeCmdCmdRemovePlant);
		kCmdCmdLockDoor = 2126649050;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetContextualCmd), kCmdCmdLockDoor, InvokeCmdCmdLockDoor);
		kCmdCmdJumpOff = -1215776;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetContextualCmd), kCmdCmdJumpOff, InvokeCmdCmdJumpOff);
		kCmdCmdGoAndJumpTo = -126968585;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetContextualCmd), kCmdCmdGoAndJumpTo, InvokeCmdCmdGoAndJumpTo);
		kCmdCmdFireAtObject = 1417993353;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetContextualCmd), kCmdCmdFireAtObject, InvokeCmdCmdFireAtObject);
		kCmdCmdFireAt = -264047446;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetContextualCmd), kCmdCmdFireAt, InvokeCmdCmdFireAt);
		kCmdCmdTurnTo = 147852153;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetContextualCmd), kCmdCmdTurnTo, InvokeCmdCmdTurnTo);
		kCmdCmdStop = 737552355;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetContextualCmd), kCmdCmdStop, InvokeCmdCmdStop);
		kCmdCmdStartBarrelFuse = -386819160;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetContextualCmd), kCmdCmdStartBarrelFuse, InvokeCmdCmdStartBarrelFuse);
		kCmdCmdEquipTool = -762686809;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetContextualCmd), kCmdCmdEquipTool, InvokeCmdCmdEquipTool);
		kCmdCmdUnequipIntoRack = 587636013;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetContextualCmd), kCmdCmdUnequipIntoRack, InvokeCmdCmdUnequipIntoRack);
		kCmdCmdUnequipAllTools = 1796924560;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetContextualCmd), kCmdCmdUnequipAllTools, InvokeCmdCmdUnequipAllTools);
		kCmdCmdEquipToolRackContent = -1649743749;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetContextualCmd), kCmdCmdEquipToolRackContent, InvokeCmdCmdEquipToolRackContent);
		kRpcRpcRemovePlant = 806309808;
		NetworkBehaviour.RegisterRpcDelegate(typeof(UNetContextualCmd), kRpcRpcRemovePlant, InvokeRpcRpcRemovePlant);
		NetworkCRC.RegisterBehaviour("UNetContextualCmd", 0);
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
