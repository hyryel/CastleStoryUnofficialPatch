using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Engine.Nature;

public class NatureRegistry : NetworkBehaviour
{
	private static int kRpcRpcRemoveNatureAt;

	private static int kRpcRpcRemoveNatureAtExcept;

	private static int kRpcRpcTranslateNature;

	private static int kRpcRpcDamageAt;

	private static int kRpcRpcBreakAt;

	private static int kRpcRpcHarvestAt;

	public static NatureRegistry Instance { get; set; }

	public void Awake()
	{
		Instance = this;
	}

	public static bool HasNatureAt(XYZ pos)
	{
		return PlantRegistry.IsPlant(pos) || BoulderRegistry.HasBoulders(pos) || IronRegistry.HasMinerals(pos) || PyriteRegistry.HasMinerals(pos);
	}

	public static bool HasNatureBlockerAt(XYZ pos)
	{
		return BoulderRegistry.HasBoulders(pos) || IronRegistry.HasMinerals(pos) || PyriteRegistry.HasMinerals(pos);
	}

	public static NatureData NatureAt(XYZ pos, NatureType type = NatureType.everything)
	{
		NatureData natureData = null;
		if (type.HasFlag(NatureType.plant))
		{
			natureData = PlantRegistry.PlantAt(pos);
		}
		if (natureData == null && type.HasFlag(NatureType.boulder))
		{
			natureData = BoulderRegistry.GetBoulder(pos);
		}
		if (natureData == null && type.HasFlag(NatureType.iron))
		{
			natureData = IronRegistry.GetMineral(pos);
		}
		if (natureData == null && type.HasFlag(NatureType.pyrite))
		{
			natureData = PyriteRegistry.GetMineral(pos);
		}
		return natureData ?? NatureData.NullNature;
	}

	public static NatureData NatureAt(int x, int z, NatureType type = NatureType.everything)
	{
		XYZ pos = new XYZ(x, VoxelEngine.GetColumnTop(x, z) - 1, z);
		return NatureAt(pos, type);
	}

	[ClientRpc]
	private void RpcRemoveNatureAt(Vector3 vpos)
	{
		XYZ xYZ = XYZ.FromVector3(vpos);
		PlantRegistry.Remove(xYZ);
		BoulderRegistry.Remove(xYZ);
		IronRegistry.Remove(xYZ);
		PyriteRegistry.Remove(xYZ);
	}

	public static void RemoveNatureAt(XYZ pos)
	{
		Instance.CallRpcRemoveNatureAt(pos.ToVector3());
	}

	[ClientRpc]
	private void RpcRemoveNatureAtExcept(Vector3 vpos, int natureType)
	{
		XYZ xYZ = XYZ.FromVector3(vpos);
		if (!((NatureType)natureType).HasFlag(NatureType.plant))
		{
			PlantRegistry.Remove(xYZ);
		}
		if (!((NatureType)natureType).HasFlag(NatureType.boulder))
		{
			BoulderRegistry.Remove(xYZ);
		}
		if (!((NatureType)natureType).HasFlag(NatureType.iron))
		{
			IronRegistry.Remove(xYZ);
		}
		if (!((NatureType)natureType).HasFlag(NatureType.pyrite))
		{
			PyriteRegistry.Remove(xYZ);
		}
	}

	public static void RemoveNatureAtExcept(XYZ pos, NatureType except)
	{
		Instance.CallRpcRemoveNatureAtExcept(pos.ToVector3(), (int)except);
	}

	[ClientRpc]
	private void RpcTranslateNature(Vector3 pos)
	{
		XYZ delta = XYZ.FromVector3(pos);
		PlantRegistry.TranslateAllPlants(delta);
		BoulderRegistry.TranslateAllBoulders(delta);
		IronRegistry.TranslateAllMinerals(delta);
		PyriteRegistry.TranslateAllMinerals(delta);
	}

	public static void TranslateNature(XYZ delta)
	{
		Instance.CallRpcTranslateNature(delta.ToVector3());
	}

	[ClientRpc]
	private void RpcDamageAt(Vector3 vpos, float damage)
	{
		XYZ p = XYZ.FromVector3(vpos);
		PlantRegistry.BreakAt(p);
		BoulderRegistry.DamageAt(p, damage);
		IronRegistry.DamageAt(p, damage);
		PyriteRegistry.DamageAt(p, damage);
	}

	public static void DamageAt(XYZ pos, float damage)
	{
		Instance.CallRpcDamageAt(pos.ToVector3(), damage);
	}

	public static float GetDamageAt(XYZ pos)
	{
		if (BoulderRegistry.HasBoulders(pos))
		{
			return BoulderRegistry.GetDamage(pos);
		}
		if (PyriteRegistry.HasMinerals(pos))
		{
			return PyriteRegistry.GetDamage(pos);
		}
		if (IronRegistry.HasMinerals(pos))
		{
			return IronRegistry.GetDamage(pos);
		}
		return 0f;
	}

	public static float GetMaxHpAt(XYZ pos)
	{
		if (BoulderRegistry.HasBoulders(pos))
		{
			return BoulderRegistry.GetMaxHp(pos);
		}
		if (PyriteRegistry.HasMinerals(pos))
		{
			return PyriteRegistry.GetMaxHp(pos);
		}
		if (IronRegistry.HasMinerals(pos))
		{
			return IronRegistry.GetMaxHp(pos);
		}
		return 0f;
	}

	public static float GetCurrentHpAt(XYZ pos)
	{
		if (BoulderRegistry.HasBoulders(pos))
		{
			return BoulderRegistry.GetCurrentHp(pos);
		}
		if (PyriteRegistry.HasMinerals(pos))
		{
			return PyriteRegistry.GetCurrentHp(pos);
		}
		if (IronRegistry.HasMinerals(pos))
		{
			return IronRegistry.GetCurrentHp(pos);
		}
		return 0f;
	}

	[ClientRpc]
	private void RpcBreakAt(Vector3 vpos)
	{
		XYZ p = XYZ.FromVector3(vpos);
		PlantRegistry.Remove(p);
		BoulderRegistry.BreakAt(p);
		IronRegistry.BreakAt(p);
		PyriteRegistry.BreakAt(p);
	}

	public static void BreakAt(XYZ pos)
	{
		Instance.CallRpcBreakAt(pos.ToVector3());
	}

	public static void GetAllBlockerPositions(ref List<XYZ> list)
	{
		BoulderRegistry.AddAllBoulderPositionsToList(ref list);
		IronRegistry.AddAllMineralPositionsToList(ref list);
		PyriteRegistry.AddAllMineralPositionsToList(ref list);
	}

	[ClientRpc]
	private void RpcHarvestAt(Vector3 vpos)
	{
		XYZ p = XYZ.FromVector3(vpos);
		PlantRegistry.BreakAt(p);
		BoulderRegistry.Harvest(p);
		IronRegistry.Harvest(p);
		PyriteRegistry.Harvest(p);
	}

	public static void HarvestAt(XYZ pos)
	{
		Instance.CallRpcHarvestAt(pos.ToVector3());
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcRemoveNatureAt(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRemoveNatureAt called on server.");
		}
		else
		{
			((NatureRegistry)obj).RpcRemoveNatureAt(reader.ReadVector3());
		}
	}

	protected static void InvokeRpcRpcRemoveNatureAtExcept(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRemoveNatureAtExcept called on server.");
		}
		else
		{
			((NatureRegistry)obj).RpcRemoveNatureAtExcept(reader.ReadVector3(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcTranslateNature(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcTranslateNature called on server.");
		}
		else
		{
			((NatureRegistry)obj).RpcTranslateNature(reader.ReadVector3());
		}
	}

	protected static void InvokeRpcRpcDamageAt(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDamageAt called on server.");
		}
		else
		{
			((NatureRegistry)obj).RpcDamageAt(reader.ReadVector3(), reader.ReadSingle());
		}
	}

	protected static void InvokeRpcRpcBreakAt(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcBreakAt called on server.");
		}
		else
		{
			((NatureRegistry)obj).RpcBreakAt(reader.ReadVector3());
		}
	}

	protected static void InvokeRpcRpcHarvestAt(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcHarvestAt called on server.");
		}
		else
		{
			((NatureRegistry)obj).RpcHarvestAt(reader.ReadVector3());
		}
	}

	public void CallRpcRemoveNatureAt(Vector3 vpos)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcRemoveNatureAt called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcRemoveNatureAt);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(vpos);
		SendRPCInternal(networkWriter, 0, "RpcRemoveNatureAt");
	}

	public void CallRpcRemoveNatureAtExcept(Vector3 vpos, int natureType)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcRemoveNatureAtExcept called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcRemoveNatureAtExcept);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(vpos);
		networkWriter.WritePackedUInt32((uint)natureType);
		SendRPCInternal(networkWriter, 0, "RpcRemoveNatureAtExcept");
	}

	public void CallRpcTranslateNature(Vector3 pos)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcTranslateNature called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcTranslateNature);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(pos);
		SendRPCInternal(networkWriter, 0, "RpcTranslateNature");
	}

	public void CallRpcDamageAt(Vector3 vpos, float damage)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcDamageAt called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcDamageAt);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(vpos);
		networkWriter.Write(damage);
		SendRPCInternal(networkWriter, 0, "RpcDamageAt");
	}

	public void CallRpcBreakAt(Vector3 vpos)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcBreakAt called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcBreakAt);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(vpos);
		SendRPCInternal(networkWriter, 0, "RpcBreakAt");
	}

	public void CallRpcHarvestAt(Vector3 vpos)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcHarvestAt called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcHarvestAt);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(vpos);
		SendRPCInternal(networkWriter, 0, "RpcHarvestAt");
	}

	static NatureRegistry()
	{
		kRpcRpcRemoveNatureAt = 2138747320;
		NetworkBehaviour.RegisterRpcDelegate(typeof(NatureRegistry), kRpcRpcRemoveNatureAt, InvokeRpcRpcRemoveNatureAt);
		kRpcRpcRemoveNatureAtExcept = 754219249;
		NetworkBehaviour.RegisterRpcDelegate(typeof(NatureRegistry), kRpcRpcRemoveNatureAtExcept, InvokeRpcRpcRemoveNatureAtExcept);
		kRpcRpcTranslateNature = -1356087045;
		NetworkBehaviour.RegisterRpcDelegate(typeof(NatureRegistry), kRpcRpcTranslateNature, InvokeRpcRpcTranslateNature);
		kRpcRpcDamageAt = 1358201180;
		NetworkBehaviour.RegisterRpcDelegate(typeof(NatureRegistry), kRpcRpcDamageAt, InvokeRpcRpcDamageAt);
		kRpcRpcBreakAt = 1380515064;
		NetworkBehaviour.RegisterRpcDelegate(typeof(NatureRegistry), kRpcRpcBreakAt, InvokeRpcRpcBreakAt);
		kRpcRpcHarvestAt = 1257061218;
		NetworkBehaviour.RegisterRpcDelegate(typeof(NatureRegistry), kRpcRpcHarvestAt, InvokeRpcRpcHarvestAt);
		NetworkCRC.RegisterBehaviour("NatureRegistry", 0);
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
