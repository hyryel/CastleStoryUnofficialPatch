using Brix.Game.AI;
using Brix.Game.Components.CatapultSpace;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Network;

public class UNetCatapult : NetworkBehaviour
{
	private static int kCmdCmdSetTarget;

	private static int kCmdCmdUnloadFromCatapult;

	private static int kCmdCmdLoadIntoCatapult;

	private static int kCmdCmdWorkOnCatapult;

	private static int kCmdCmdShootCatapult;

	private static int kCmdCmdAssignWorker;

	private static int kCmdCmdClearTarget;

	private static int kCmdCmdRequestHoldFire;

	private static int kCmdCmdRequestFireOnce;

	private static int kCmdCmdRequestFireAtWill;

	[Command]
	public void CmdSetTarget(GameObject instance, Vector3 target, Vector3 normal)
	{
		if (Affiliation.IsOwnedBy(instance, base.gameObject))
		{
			Catapult component = instance.GetComponent<Catapult>();
			component.SetTarget(target, normal);
		}
	}

	[Command]
	public void CmdUnloadFromCatapult(GameObject instanceGO, GameObject laborGO)
	{
		if (Affiliation.IsOwnedBy(instanceGO, base.gameObject))
		{
			CatapultOperations component = instanceGO.GetComponent<CatapultOperations>();
			Labor component2 = laborGO.GetComponent<Labor>();
			component.UnloadAmmoInstuction.Send(component2, Labor.DecisionSource.User);
		}
	}

	[Command]
	public void CmdLoadIntoCatapult(GameObject instanceGO, GameObject laborGO)
	{
		if (Affiliation.IsOwnedBy(instanceGO, base.gameObject))
		{
			CatapultOperations component = instanceGO.GetComponent<CatapultOperations>();
			Labor component2 = laborGO.GetComponent<Labor>();
			component.GoFetchAmmoInstruction.Send(component2, Labor.DecisionSource.User);
		}
	}

	[Command]
	public void CmdWorkOnCatapult(GameObject instanceGO, GameObject laborGO)
	{
		if (Affiliation.IsOwnedBy(instanceGO, base.gameObject))
		{
			CatapultOperations component = instanceGO.GetComponent<CatapultOperations>();
			Labor component2 = laborGO.GetComponent<Labor>();
			component.CrankAndAimInstruction.Send(component2, Labor.DecisionSource.User);
		}
	}

	[Command]
	public void CmdShootCatapult(GameObject instanceGO, GameObject laborGO)
	{
		if (Affiliation.IsOwnedBy(instanceGO, base.gameObject))
		{
			CatapultOperations component = instanceGO.GetComponent<CatapultOperations>();
			Labor component2 = laborGO.GetComponent<Labor>();
			component.RequestFireOnce();
			component.ShootInstruction.Send(component2, Labor.DecisionSource.User);
		}
	}

	[Command]
	public void CmdAssignWorker(GameObject instanceGO, GameObject laborGO)
	{
		if (Affiliation.IsOwnedBy(instanceGO, base.gameObject))
		{
			CatapultOperations component = instanceGO.GetComponent<CatapultOperations>();
			Labor component2 = laborGO.GetComponent<Labor>();
			component2.Autonomy = AutonomyStatus.LockedToOperable;
			component.Hire(component2);
		}
	}

	[Command]
	public void CmdClearTarget(GameObject instanceGO)
	{
		if (Affiliation.IsOwnedBy(instanceGO, base.gameObject))
		{
			CatapultOperations component = instanceGO.GetComponent<CatapultOperations>();
			component.ClearTarget();
		}
	}

	[Command]
	public void CmdRequestHoldFire(GameObject instanceGO)
	{
		if (Affiliation.IsOwnedBy(instanceGO, base.gameObject))
		{
			CatapultOperations component = instanceGO.GetComponent<CatapultOperations>();
			component.RequestHoldFire();
		}
	}

	[Command]
	public void CmdRequestFireOnce(GameObject instanceGO)
	{
		if (Affiliation.IsOwnedBy(instanceGO, base.gameObject))
		{
			CatapultOperations component = instanceGO.GetComponent<CatapultOperations>();
			component.RequestFireOnce();
		}
	}

	[Command]
	public void CmdRequestFireAtWill(GameObject instanceGO)
	{
		if (Affiliation.IsOwnedBy(instanceGO, base.gameObject))
		{
			CatapultOperations component = instanceGO.GetComponent<CatapultOperations>();
			component.RequestFireAtWill();
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdSetTarget(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetTarget called on client.");
		}
		else
		{
			((UNetCatapult)obj).CmdSetTarget(reader.ReadGameObject(), reader.ReadVector3(), reader.ReadVector3());
		}
	}

	protected static void InvokeCmdCmdUnloadFromCatapult(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUnloadFromCatapult called on client.");
		}
		else
		{
			((UNetCatapult)obj).CmdUnloadFromCatapult(reader.ReadGameObject(), reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdLoadIntoCatapult(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdLoadIntoCatapult called on client.");
		}
		else
		{
			((UNetCatapult)obj).CmdLoadIntoCatapult(reader.ReadGameObject(), reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdWorkOnCatapult(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdWorkOnCatapult called on client.");
		}
		else
		{
			((UNetCatapult)obj).CmdWorkOnCatapult(reader.ReadGameObject(), reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdShootCatapult(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdShootCatapult called on client.");
		}
		else
		{
			((UNetCatapult)obj).CmdShootCatapult(reader.ReadGameObject(), reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdAssignWorker(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAssignWorker called on client.");
		}
		else
		{
			((UNetCatapult)obj).CmdAssignWorker(reader.ReadGameObject(), reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdClearTarget(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdClearTarget called on client.");
		}
		else
		{
			((UNetCatapult)obj).CmdClearTarget(reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdRequestHoldFire(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestHoldFire called on client.");
		}
		else
		{
			((UNetCatapult)obj).CmdRequestHoldFire(reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdRequestFireOnce(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestFireOnce called on client.");
		}
		else
		{
			((UNetCatapult)obj).CmdRequestFireOnce(reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdRequestFireAtWill(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestFireAtWill called on client.");
		}
		else
		{
			((UNetCatapult)obj).CmdRequestFireAtWill(reader.ReadGameObject());
		}
	}

	public void CallCmdSetTarget(GameObject instance, Vector3 target, Vector3 normal)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetTarget called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetTarget(instance, target, normal);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetTarget);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(instance);
		networkWriter.Write(target);
		networkWriter.Write(normal);
		SendCommandInternal(networkWriter, 0, "CmdSetTarget");
	}

	public void CallCmdUnloadFromCatapult(GameObject instanceGO, GameObject laborGO)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdUnloadFromCatapult called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdUnloadFromCatapult(instanceGO, laborGO);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdUnloadFromCatapult);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(instanceGO);
		networkWriter.Write(laborGO);
		SendCommandInternal(networkWriter, 0, "CmdUnloadFromCatapult");
	}

	public void CallCmdLoadIntoCatapult(GameObject instanceGO, GameObject laborGO)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdLoadIntoCatapult called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdLoadIntoCatapult(instanceGO, laborGO);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdLoadIntoCatapult);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(instanceGO);
		networkWriter.Write(laborGO);
		SendCommandInternal(networkWriter, 0, "CmdLoadIntoCatapult");
	}

	public void CallCmdWorkOnCatapult(GameObject instanceGO, GameObject laborGO)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdWorkOnCatapult called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdWorkOnCatapult(instanceGO, laborGO);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdWorkOnCatapult);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(instanceGO);
		networkWriter.Write(laborGO);
		SendCommandInternal(networkWriter, 0, "CmdWorkOnCatapult");
	}

	public void CallCmdShootCatapult(GameObject instanceGO, GameObject laborGO)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdShootCatapult called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdShootCatapult(instanceGO, laborGO);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdShootCatapult);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(instanceGO);
		networkWriter.Write(laborGO);
		SendCommandInternal(networkWriter, 0, "CmdShootCatapult");
	}

	public void CallCmdAssignWorker(GameObject instanceGO, GameObject laborGO)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdAssignWorker called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdAssignWorker(instanceGO, laborGO);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdAssignWorker);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(instanceGO);
		networkWriter.Write(laborGO);
		SendCommandInternal(networkWriter, 0, "CmdAssignWorker");
	}

	public void CallCmdClearTarget(GameObject instanceGO)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdClearTarget called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdClearTarget(instanceGO);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdClearTarget);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(instanceGO);
		SendCommandInternal(networkWriter, 0, "CmdClearTarget");
	}

	public void CallCmdRequestHoldFire(GameObject instanceGO)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRequestHoldFire called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRequestHoldFire(instanceGO);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRequestHoldFire);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(instanceGO);
		SendCommandInternal(networkWriter, 0, "CmdRequestHoldFire");
	}

	public void CallCmdRequestFireOnce(GameObject instanceGO)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRequestFireOnce called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRequestFireOnce(instanceGO);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRequestFireOnce);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(instanceGO);
		SendCommandInternal(networkWriter, 0, "CmdRequestFireOnce");
	}

	public void CallCmdRequestFireAtWill(GameObject instanceGO)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRequestFireAtWill called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRequestFireAtWill(instanceGO);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRequestFireAtWill);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(instanceGO);
		SendCommandInternal(networkWriter, 0, "CmdRequestFireAtWill");
	}

	static UNetCatapult()
	{
		kCmdCmdSetTarget = -1273086477;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetCatapult), kCmdCmdSetTarget, InvokeCmdCmdSetTarget);
		kCmdCmdUnloadFromCatapult = 619184417;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetCatapult), kCmdCmdUnloadFromCatapult, InvokeCmdCmdUnloadFromCatapult);
		kCmdCmdLoadIntoCatapult = -408050370;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetCatapult), kCmdCmdLoadIntoCatapult, InvokeCmdCmdLoadIntoCatapult);
		kCmdCmdWorkOnCatapult = 649353128;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetCatapult), kCmdCmdWorkOnCatapult, InvokeCmdCmdWorkOnCatapult);
		kCmdCmdShootCatapult = -1701991753;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetCatapult), kCmdCmdShootCatapult, InvokeCmdCmdShootCatapult);
		kCmdCmdAssignWorker = -611246099;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetCatapult), kCmdCmdAssignWorker, InvokeCmdCmdAssignWorker);
		kCmdCmdClearTarget = 1802765214;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetCatapult), kCmdCmdClearTarget, InvokeCmdCmdClearTarget);
		kCmdCmdRequestHoldFire = 1359686820;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetCatapult), kCmdCmdRequestHoldFire, InvokeCmdCmdRequestHoldFire);
		kCmdCmdRequestFireOnce = 1311949542;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetCatapult), kCmdCmdRequestFireOnce, InvokeCmdCmdRequestFireOnce);
		kCmdCmdRequestFireAtWill = 1962474954;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetCatapult), kCmdCmdRequestFireAtWill, InvokeCmdCmdRequestFireAtWill);
		NetworkCRC.RegisterBehaviour("UNetCatapult", 0);
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
