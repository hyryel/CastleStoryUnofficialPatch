using System;
using System.Collections;
using System.Collections.Generic;
using Brix.Audio;
using Brix.Components;
using Brix.Engine;
using Brix.Engine.Abstract;
using Brix.Engine.Blocks;
using Brix.Engine.Environment;
using Brix.External.Factories;
using Brix.Game.Components;
using Brix.Lifecycle.Pooling;
using Brix.Network;
using Brix.Utils;
using Brix.Utils.Logging;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Legacy;

public class DamagedTree : NetworkGameComponent
{
	private static BrixLogger logger;

	public static Dictionary<XYZ, DamagedTree> AllDamagedTrees;

	public Factory.AssetKey particleSystemPrefab;

	public int maxHp = 180;

	public int spawnedQty = 3;

	public float maxSpawnDelay = 5f;

	[JsonProperty]
	private int info;

	[JsonProperty]
	private Vector3 random;

	private bool _ready;

	private int _hp;

	private float _lastStrike;

	private float _lastRegeneration;

	private XYZ _pos;

	private bool _wasCut;

	private Transform _enfant;

	private Rigidbody _rigidbody;

	private Rigidbody[] _speedProbes;

	public Material arbreMat;

	public Texture2D arbreTex;

	private int _logsPostProcessed;

	private static int kRpcRpcStrike;

	private static int kRpcRpcCut;

	private static int kRpcRpcFallingLogsPostprocess;

	public int Hp => _hp;

	public bool Ready => _ready;

	public bool IsFull => _hp >= maxHp;

	public bool IsDying => _hp < 1;

	protected override void Start()
	{
		base.Start();
		if (AllDamagedTrees.Count == 0)
		{
			GameSignals.Once(GameSignals.LeaveGame, AllDamagedTrees.Clear);
		}
		GameSignals.LateConnectOnce(GameSignals.TerrainReady, Go);
		GameSignals.DisableUntil(GameSignals.TerrainReady, this);
	}

	private void Go()
	{
		_pos = ((!(base.voxel.position == XYZ.zero)) ? base.voxel.position : XYZ.FromVector3(base.transform.position));
		AllDamagedTrees[_pos] = this;
		if (!NetworkServer.active || !GetAt(_pos).IsNullOrReleased())
		{
			_ready = true;
			_hp = maxHp;
			_lastRegeneration = Time.time;
			if (info == 0)
			{
				info = TreeRegistry.Instance.GetTree(_pos);
			}
			GameSignals.Invoke(GameSignals.TreeChanged, _pos);
			random = UnityEngine.Random.insideUnitSphere;
			base.gameObject.name = "DamagedTree " + _pos;
			base.voxel.position = _pos;
			base.transform.rotation = GetRotation(info);
			Biome biome = GameParam.BiomeToLoad();
			_enfant = base.transform.GetChild(0);
			_enfant.GetComponent<MeshFilter>().sharedMesh = biome.Trees.GetComponent<LodMeshFilter>().lod0Mesh;
			_enfant.position = _pos.ToVector3() - Vector3.up / 2f;
			_enfant.rotation = Quaternion.identity;
			List<XYZ> list = CollectionPool.RequestList<XYZ>();
			list.Add(_pos);
			arbreMat = new Material(MegaVoxel.Common.treeMaterials[0]);
			MegaVoxel.Common.treeDrawer.GetWarpTex(ref arbreTex, list, _pos);
			arbreMat.SetTexture("_WarpTex", arbreTex);
			arbreMat.SetVector("_TCenter", -Vector3.up / 2f);
			_enfant.GetComponent<MeshFilter>().sharedMesh = MegaVoxel.Common.treeDrawer.GetCombinedMesh(list.Count, list.Count, 0, arbreTex.width, 0);
			_enfant.GetComponent<MeshRenderer>().sharedMaterial = arbreMat;
			list.Dispose();
			Vector3 position = _enfant.transform.position + _enfant.transform.up * 6f;
			Quaternion rotation = _enfant.transform.rotation;
			ObjectPoolSingleton.Request(out var go, particleSystemPrefab, position, rotation);
			go.transform.SetParent(_enfant);
			SoundEngine.AddEmitter("SFX", base.gameObject);
		}
		else
		{
			logger.ErrorFormat("Damaged tree at {0} was not created created via GetOrCreate. Releasing it.", _pos);
			ObjectPoolSingleton.Release(base.gameObject);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (arbreMat != null)
		{
			UnityEngine.Object.Destroy(arbreMat);
		}
		if (arbreTex != null)
		{
			UnityEngine.Object.Destroy(arbreTex);
		}
		if (AllDamagedTrees.ContainsKey(_pos))
		{
			AllDamagedTrees.Remove(_pos);
		}
		if (!_wasCut)
		{
			ReRoot();
		}
		else
		{
			TreeRegistry.Instance.Remove(_pos);
		}
	}

	public static DamagedTree GetAt(XYZ position)
	{
		DamagedTree valueOrDefault = AllDamagedTrees.GetValueOrDefault(position);
		if ((bool)valueOrDefault)
		{
			return valueOrDefault;
		}
		valueOrDefault = AllDamagedTrees.GetValueOrDefault(position - XYZ.up);
		if ((bool)valueOrDefault)
		{
			return valueOrDefault;
		}
		valueOrDefault = AllDamagedTrees.GetValueOrDefault(position + XYZ.up);
		if ((bool)valueOrDefault)
		{
			return valueOrDefault;
		}
		return null;
	}

	public static DamagedTree GetAtOrCreate(XYZ position)
	{
		DamagedTree t = GetAt(position);
		if (t != null)
		{
			return t;
		}
		Quaternion rotation = GetRotation(TreeRegistry.Instance.GetTree(position));
		ObjectPoolSingleton.RequestComponent<DamagedTree>(out t, ObjetsDynamiques.DamagedTree, position.ToVector3(), rotation);
		NetworkServer.Spawn(t.gameObject, ObjetsDynamiques.DamagedTree.AssetId);
		t._pos = position;
		AllDamagedTrees[position] = t;
		return t;
	}

	public void Blast()
	{
		if (NetworkServer.active)
		{
			CallRpcCut(base.gameObject, _pos.ToScalaire(), explosion: true);
		}
	}

	private static Quaternion GetRotation(int info)
	{
		return Quaternion.identity;
	}

	[ServerCallback]
	private void Update()
	{
		if (NetworkServer.active && Ready && !IsDying && Time.time >= _lastRegeneration + 2f && Time.time >= _lastStrike + 20f)
		{
			_lastRegeneration = Time.time;
			Regenerate();
		}
	}

	private void Regenerate()
	{
		logger.InfoFormat("DamagedTree regenerates: {0} -> {1}", _hp, Mathf.Min(_hp + 1, maxHp));
		_hp = Mathf.Min(_hp + 60, maxHp);
		if (IsFull)
		{
			ObjectPoolSingleton.Release(base.gameObject);
		}
	}

	private void ReRoot()
	{
		if (!_ready)
		{
			logger.WarnFormat("DamagedTree.ReRoot invoked on a tree that wasn't started properly: {0}", _pos);
			return;
		}
		logger.InfoFormat("DamagedTree.ReRoot() Position={0}, info={1}", _pos, info);
		TreeRegistry.Instance.Remove(_pos);
		TreeRegistry.Instance.Add(_pos, info);
		Brix.Engine.Architecte.AppliquerChangements();
	}

	public void Strike(int origine, int damage = 60)
	{
		if (!this.IsNullOrReleased())
		{
			CallRpcStrike(base.gameObject);
			_lastStrike = Time.time;
			_hp -= damage;
			if (IsDying)
			{
				CallRpcCut(base.gameObject, origine, explosion: false);
			}
		}
	}

	[ClientRpc]
	public void RpcStrike(GameObject TreeGO)
	{
		if (!TreeGO.IsNullOrReleased())
		{
			DamagedTree component = TreeGO.GetComponent<DamagedTree>();
			component.TryPlayAnimation("Vibration");
			SoundEngine.Play("Tree_Hit", base.gameObject);
		}
	}

	[ClientRpc]
	public void RpcCut(GameObject TreeGO, int origine, bool explosion)
	{
		if (!TreeGO.IsNullOrReleased())
		{
			DamagedTree component = TreeGO.GetComponent<DamagedTree>();
			component.Cut(TreeGO, origine, explosion);
		}
	}

	private void Cut(GameObject TreeGO, int origine, bool explosion)
	{
		if (_wasCut)
		{
			return;
		}
		if (!_ready)
		{
			logger.WarnFormat("DamagedTree.Cut invoked on a tree that wasn't started properly: {0}", _pos);
			return;
		}
		_wasCut = true;
		DamagedTree component = TreeGO.GetComponent<DamagedTree>();
		CapsuleCollider capsuleCollider = component._enfant.gameObject.AddComponent<CapsuleCollider>();
		Vector3 vector = (component._enfant.position - Brix.Engine.Abstract.Voxel.GetVecteur(origine)).normalized * 1.1f + random;
		vector.y = 0f;
		vector = Vector3.Cross(vector.normalized, Vector3.up) * 10f;
		capsuleCollider.direction = 1;
		capsuleCollider.height = 6f;
		capsuleCollider.radius = 0.5f;
		capsuleCollider.center = new Vector3(0f, capsuleCollider.height / 2f + 1f, 0f);
		component._rigidbody = _enfant.gameObject.AddComponent<Rigidbody>();
		component._rigidbody.AddTorque(vector, ForceMode.Force);
		component._rigidbody.maxAngularVelocity = 1f;
		_speedProbes = new Rigidbody[component.spawnedQty];
		for (int i = 0; i < component.spawnedQty; i++)
		{
			GameObject gameObject = new GameObject();
			Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
			rigidbody.constraints = RigidbodyConstraints.FreezeAll;
			gameObject.transform.SetParent(TreeGO.transform);
			gameObject.transform.position = component._enfant.position + Vector3.up * ((i + 1) * 2);
			_speedProbes[i] = rigidbody;
		}
		SoundEngine.Play("Tree_Hit", TreeGO);
		Brix.Engine.Neo.Relais(delegate
		{
			SoundEngine.Play("Tree_Falls", TreeGO);
		}, "tree fall sound");
		StartCoroutine(Burst(TreeGO, explosion));
	}

	private IEnumerator Burst(GameObject TreeGO, bool explosion = false)
	{
		if (base.isServer && !explosion)
		{
			BrixSingleton<BlockEngine>.Instance.CreateBlock(XYZ.FromVector3(TreeGO.transform.position), Quaternion.Euler(0f, UnityEngine.Random.Range(0, 360), 0f), Blocks.Wood_Stump.ToBlockInfo());
		}
		DamagedTree tree = TreeGO.GetComponent<DamagedTree>();
		if (!explosion)
		{
			float t = Time.time + maxSpawnDelay;
			while (Vector3.Angle(tree.transform.up, tree._enfant.up) < 25f && Time.time <= t)
			{
				yield return new WaitForFixedUpdate();
			}
		}
		for (int i = 0; i < tree.spawnedQty; i++)
		{
			Factory.AssetKey wood_DoubleLog_AA = Blocks.Wood_DoubleLog_AA;
			Vector3 vector = tree._enfant.position + tree._enfant.up * ((i + 1) * 2);
			Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, tree._enfant.up);
			if (base.isServer)
			{
				ObjectPoolSingleton.Request(out var go, wood_DoubleLog_AA, vector, rotation);
				NetworkServer.Spawn(go, wood_DoubleLog_AA.AssetId);
				go.GetOrAddComponent<AffiliationComponent>().faction = base.affiliation.faction;
				CallRpcFallingLogsPostprocess(go, tree._rigidbody.GetPointVelocity(vector));
			}
		}
		base.gameObject.SetActive(value: false);
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		SoundEngine.Play("Tree_Desintegrates", base.gameObject);
		TryStartParticleSystem();
	}

	[ClientRpc]
	public void RpcFallingLogsPostprocess(GameObject Log, Vector3 velocity)
	{
		if ((bool)Log)
		{
			Log.GetComponent<Rigidbody>().velocity = velocity;
		}
		_logsPostProcessed++;
		if (_logsPostProcessed >= spawnedQty)
		{
			if (NetworkServer.active)
			{
				DestroyAt(_pos);
			}
			GameSignals.Invoke(GameSignals.TreeChanged, _pos);
		}
	}

	private static void DestroyAt(XYZ pos)
	{
		if (AllDamagedTrees.ContainsKey(pos))
		{
			ObjectPoolSingleton.Release(AllDamagedTrees[pos].gameObject);
		}
	}

	private void TryPlayAnimation(string name)
	{
		try
		{
			GetComponentInChildren<Animation>().Play(name);
		}
		catch (Exception e)
		{
			logger.ErrorFormat(e, "DamagedTree failed to play animation '{0}'.", name);
		}
	}

	private void TryStartParticleSystem()
	{
		try
		{
			ParticleSystem componentInChildren = GetComponentInChildren<ParticleSystem>();
			componentInChildren.transform.SetParent(null);
			componentInChildren.Play();
			UnityEngine.Object.Destroy(componentInChildren.gameObject, 2f);
		}
		catch (Exception exception)
		{
			logger.Error(exception, "DamagedTree failed to start particle system.");
		}
	}

	static DamagedTree()
	{
		logger = new BrixLogger(typeof(DamagedTree).FullName);
		AllDamagedTrees = new Dictionary<XYZ, DamagedTree>();
		kRpcRpcStrike = -1468014821;
		NetworkBehaviour.RegisterRpcDelegate(typeof(DamagedTree), kRpcRpcStrike, InvokeRpcRpcStrike);
		kRpcRpcCut = -587701383;
		NetworkBehaviour.RegisterRpcDelegate(typeof(DamagedTree), kRpcRpcCut, InvokeRpcRpcCut);
		kRpcRpcFallingLogsPostprocess = -1613292414;
		NetworkBehaviour.RegisterRpcDelegate(typeof(DamagedTree), kRpcRpcFallingLogsPostprocess, InvokeRpcRpcFallingLogsPostprocess);
		NetworkCRC.RegisterBehaviour("DamagedTree", 0);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcStrike(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcStrike called on server.");
		}
		else
		{
			((DamagedTree)obj).RpcStrike(reader.ReadGameObject());
		}
	}

	protected static void InvokeRpcRpcCut(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcCut called on server.");
		}
		else
		{
			((DamagedTree)obj).RpcCut(reader.ReadGameObject(), (int)reader.ReadPackedUInt32(), reader.ReadBoolean());
		}
	}

	protected static void InvokeRpcRpcFallingLogsPostprocess(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcFallingLogsPostprocess called on server.");
		}
		else
		{
			((DamagedTree)obj).RpcFallingLogsPostprocess(reader.ReadGameObject(), reader.ReadVector3());
		}
	}

	public void CallRpcStrike(GameObject TreeGO)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcStrike called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcStrike);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(TreeGO);
		SendRPCInternal(networkWriter, 0, "RpcStrike");
	}

	public void CallRpcCut(GameObject TreeGO, int origine, bool explosion)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcCut called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcCut);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(TreeGO);
		networkWriter.WritePackedUInt32((uint)origine);
		networkWriter.Write(explosion);
		SendRPCInternal(networkWriter, 0, "RpcCut");
	}

	public void CallRpcFallingLogsPostprocess(GameObject Log, Vector3 velocity)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcFallingLogsPostprocess called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcFallingLogsPostprocess);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(Log);
		networkWriter.Write(velocity);
		SendRPCInternal(networkWriter, 0, "RpcFallingLogsPostprocess");
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
