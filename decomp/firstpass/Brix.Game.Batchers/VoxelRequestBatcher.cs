using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Game.Components;
using Brix.Network;
using Brix.Utils;
using Brix.Utils.Logging;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Batchers;

public class VoxelRequestBatcher : NetworkBehaviour
{
	private static BrixLogger logger;

	public static VoxelRequestBatcher Instance;

	private Signal<XYZ, int> _deblocRequestSignal = new Signal<XYZ, int>
	{
		MaximumConnections = 4096
	};

	private Signal<XYZ, VoxelTerrainType, int, bool, bool> _addTerrainResourcesRequestSignal = new Signal<XYZ, VoxelTerrainType, int, bool, bool>
	{
		MaximumConnections = 1024
	};

	private SignalStorePoco signalStore = new SignalStorePoco();

	private readonly VoxelTerrainResources _voxelResources = new VoxelTerrainResources();

	private static Collider[] hits;

	private static int kRpcRpcAddTerrainResources;

	private static int kRpcRpcAddTerrainResources_Multi;

	public static IReadOnlySignal<XYZ, int> DeblocRequestSignal => Instance._deblocRequestSignal;

	public void QueueDebloc(int x, int y, int z, int id)
	{
		QueueDebloc(new XYZ(x, y, z), id);
	}

	public void QueueDebloc(XYZ position, int id)
	{
		logger.DebugFormat("Debloc request done at position {0}", position);
		_deblocRequestSignal.Invoke(position, id);
	}

	[ClientRpc]
	public void RpcAddTerrainResources(int x, int y, int z, VoxelTerrainType type, int amount, bool compacted, bool pushOut)
	{
		_addTerrainResourcesRequestSignal.Invoke(new XYZ(x, y, z), type, amount, compacted, pushOut);
	}

	[ClientRpc]
	public void RpcAddTerrainResources_Multi(int[] xs, int[] ys, int[] zs, int[] types, int[] amounts, bool compacted, bool pushOut)
	{
		AddTerrainResources_Multi(xs, ys, zs, types, amounts, compacted, pushOut);
	}

	private void GetVoxelEngine()
	{
		base.enabled = true;
	}

	private void Awake()
	{
		Instance = this;
		GameSignals.Connect(GameSignals.TerrainLoaded, GetVoxelEngine);
		signalStore.Connect(_deblocRequestSignal, ProcessDebloc);
		_addTerrainResourcesRequestSignal.Connect(AddTerrainResources_Single);
		base.enabled = false;
	}

	private void Start()
	{
		DeblocRequestSignal.Connect(UNetManager.instance.DeblocMessageHandler.AddDeblocToSendQueue);
	}

	private void FixedUpdate()
	{
		Voxel.Engine.BeginChanges();
		signalStore.Poll();
		Voxel.Engine.ApplyChanges();
		Architecte.AppliquerChangements();
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	public void ProcessDebloc(XYZ position, int blockID)
	{
		using IVoxel voxel = Voxel.GetVoxelDataWithId(position, blockID);
		voxel.DeBlockClient(blockID);
	}

	private void AddTerrainResources_Single(XYZ p, VoxelTerrainType type, int amount, bool compacted, bool pushOut)
	{
		_voxelResources.Clear();
		_voxelResources.Add((int)type, amount);
		Voxel.Engine.BeginChanges();
		bool flag = AddTerrainResources_Pre(p, _voxelResources, compacted);
		Voxel.Engine.ApplyChanges();
		if (flag)
		{
			AddTerrainResources_Post(p, pushOut);
		}
	}

	private void AddTerrainResources_Multi(int[] xs, int[] ys, int[] zs, int[] types, int[] amounts, bool compacted, bool pushOut)
	{
		Dictionary<XYZ, VoxelTerrainResources> dictionary = CollectionPool.RequestDictionary<XYZ, VoxelTerrainResources>();
		for (int i = 0; i < xs.Length; i++)
		{
			dictionary.GetValueOrCreate(new XYZ(xs[i], ys[i], zs[i])).Add(types[i], amounts[i]);
		}
		Voxel.Engine.BeginChanges();
		dictionary.RemoveAll((KeyValuePair<XYZ, VoxelTerrainResources> kv) => !AddTerrainResources_Pre(kv.Key, kv.Value, compacted));
		Voxel.Engine.ApplyChanges();
		if (dictionary.Count > 0)
		{
			foreach (KeyValuePair<XYZ, VoxelTerrainResources> item in dictionary)
			{
				AddTerrainResources_Post(item.Key, pushOut);
			}
		}
		dictionary.Dispose();
	}

	private bool AddTerrainResources_Pre(XYZ p, VoxelTerrainResources r, bool compacted)
	{
		VoxelHandle voxelHandle = Voxel.Engine.GetVoxelHandle(p);
		int voxelType = Voxel.Engine.GetVoxelType(voxelHandle);
		if (!compacted && (voxelType == 0 || voxelType == 255))
		{
			return false;
		}
		Voxel.Engine.AddTerrainResources(voxelHandle, r);
		if (compacted)
		{
			Voxel.Engine.CompactCrust(Voxel.Engine.GetVoxelHandle(p));
		}
		return true;
	}

	private void AddTerrainResources_Post(XYZ p, bool pushOut)
	{
		GameSignals.Invoke(GameSignals.VoxelChanged, p);
		XYZ xYZ = new XYZ(p.x, p.y + 1, p.z);
		GameSignals.Invoke(GameSignals.VoxelChanged, xYZ);
		GameSignals.Invoke(GameSignals.VoxelChanged, p + XYZ.right);
		GameSignals.Invoke(GameSignals.VoxelChanged, p - XYZ.right);
		GameSignals.Invoke(GameSignals.VoxelChanged, p + XYZ.forward);
		GameSignals.Invoke(GameSignals.VoxelChanged, p - XYZ.forward);
		Architecte.Changement(p);
		Architecte.Changement(xYZ);
		Architecte.Changement(p - XYZ.up);
		List<Collider> list = CollectionPool.RequestList<Collider>();
		GameSignals.Invoke(GameSignals.TerrainChanged, p);
		if (pushOut)
		{
			int length = Physics.OverlapSphereNonAlloc(p.ToVector3(), 0.886f, hits, 24576);
			hits.AddToList(ref list);
			Array.Clear(hits, 0, length);
		}
		p += XYZ.up;
		if (!Voxel.IsEmpty(p))
		{
			GameSignals.Invoke(GameSignals.TerrainChanged, p);
			if (pushOut)
			{
				int length2 = Physics.OverlapSphereNonAlloc(p.ToVector3(), 0.886f, hits, 24576);
				hits.AddToList(ref list);
				Array.Clear(hits, 0, length2);
			}
		}
		if (pushOut)
		{
			foreach (Collider item in list)
			{
				if (item != null)
				{
					item.GetOrAddComponent<BuryBehavior>();
				}
			}
		}
		list.Dispose();
	}

	static VoxelRequestBatcher()
	{
		logger = new BrixLogger(typeof(VoxelRequestBatcher).FullName);
		hits = new Collider[1000];
		kRpcRpcAddTerrainResources = 507302493;
		NetworkBehaviour.RegisterRpcDelegate(typeof(VoxelRequestBatcher), kRpcRpcAddTerrainResources, InvokeRpcRpcAddTerrainResources);
		kRpcRpcAddTerrainResources_Multi = 1007362231;
		NetworkBehaviour.RegisterRpcDelegate(typeof(VoxelRequestBatcher), kRpcRpcAddTerrainResources_Multi, InvokeRpcRpcAddTerrainResources_Multi);
		NetworkCRC.RegisterBehaviour("VoxelRequestBatcher", 0);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcAddTerrainResources(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAddTerrainResources called on server.");
		}
		else
		{
			((VoxelRequestBatcher)obj).RpcAddTerrainResources((int)reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32(), (VoxelTerrainType)reader.ReadInt32(), (int)reader.ReadPackedUInt32(), reader.ReadBoolean(), reader.ReadBoolean());
		}
	}

	protected static void InvokeRpcRpcAddTerrainResources_Multi(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAddTerrainResources_Multi called on server.");
		}
		else
		{
			((VoxelRequestBatcher)obj).RpcAddTerrainResources_Multi(GeneratedNetworkCode._ReadArrayInt32_None(reader), GeneratedNetworkCode._ReadArrayInt32_None(reader), GeneratedNetworkCode._ReadArrayInt32_None(reader), GeneratedNetworkCode._ReadArrayInt32_None(reader), GeneratedNetworkCode._ReadArrayInt32_None(reader), reader.ReadBoolean(), reader.ReadBoolean());
		}
	}

	public void CallRpcAddTerrainResources(int x, int y, int z, VoxelTerrainType type, int amount, bool compacted, bool pushOut)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcAddTerrainResources called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcAddTerrainResources);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)x);
		networkWriter.WritePackedUInt32((uint)y);
		networkWriter.WritePackedUInt32((uint)z);
		networkWriter.Write((int)type);
		networkWriter.WritePackedUInt32((uint)amount);
		networkWriter.Write(compacted);
		networkWriter.Write(pushOut);
		SendRPCInternal(networkWriter, 0, "RpcAddTerrainResources");
	}

	public void CallRpcAddTerrainResources_Multi(int[] xs, int[] ys, int[] zs, int[] types, int[] amounts, bool compacted, bool pushOut)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcAddTerrainResources_Multi called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcAddTerrainResources_Multi);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		GeneratedNetworkCode._WriteArrayInt32_None(networkWriter, xs);
		GeneratedNetworkCode._WriteArrayInt32_None(networkWriter, ys);
		GeneratedNetworkCode._WriteArrayInt32_None(networkWriter, zs);
		GeneratedNetworkCode._WriteArrayInt32_None(networkWriter, types);
		GeneratedNetworkCode._WriteArrayInt32_None(networkWriter, amounts);
		networkWriter.Write(compacted);
		networkWriter.Write(pushOut);
		SendRPCInternal(networkWriter, 0, "RpcAddTerrainResources_Multi");
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
