using System.Collections.Generic;
using System.Runtime.InteropServices;
using Brix.External.Signals;
using Brix.Game.AI;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components;

public class CraftingState : NetworkBehaviour
{
	public delegate void ProcessDelegate(int ms);

	private Signal updatedSignal = new Signal();

	private Signal queueChangedSignal = new Signal();

	private bool queueChanged;

	private Signal loopChangedSignal = new Signal();

	private bool loopChanged;

	private EdgeSignal<CraftingState> working = new EdgeSignal<CraftingState>();

	private Signal<int> processingSignal = new Signal<int>();

	[JsonProperty]
	private bool initialized;

	[JsonProperty]
	[SyncVar(hook = "RefreshLoop")]
	private bool loop;

	[JsonProperty]
	[SyncVar(hook = "RefreshAutoStore")]
	private bool autoStore;

	[SyncVar(hook = "RefreshOperator")]
	private GameObject _operator;

	[JsonProperty]
	[SyncVar(hook = "RefreshRecipes")]
	private ulong recipes;

	[SyncVar(hook = "RefreshWorking")]
	private bool _working;

	private readonly int[] ids = new int[8];

	private NetworkInstanceId ____operatorNetId;

	private static int kEventEventProcess;

	public IReadOnlySignal Updated => updatedSignal;

	public IReadOnlySignal QueueChanged => queueChangedSignal;

	public IReadOnlySignal LoopChanged => loopChangedSignal;

	public IReadOnlyEdgeSignal<CraftingState> WorkStarted => working;

	public IReadOnlySignal<int> Processing => processingSignal;

	public bool Working
	{
		get
		{
			return _working;
		}
		set
		{
			Network_working = value;
		}
	}

	public bool Initilized => initialized;

	public bool AutoStore
	{
		get
		{
			return autoStore;
		}
		set
		{
			NetworkautoStore = value;
		}
	}

	public bool IsLoop
	{
		get
		{
			return loop;
		}
		set
		{
			Networkloop = value;
		}
	}

	public Labor Operator
	{
		get
		{
			return (!_operator.IsNullOrReleased()) ? _operator.GetComponent<Labor>() : null;
		}
		set
		{
			Network_operator = ((!value.IsNullOrReleased()) ? value.gameObject : null);
		}
	}

	public ulong Hash => recipes;

	public bool Networkloop
	{
		get
		{
			return loop;
		}
		[param: In]
		set
		{
			ref bool fieldValue = ref loop;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				RefreshLoop(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 1u);
		}
	}

	public bool NetworkautoStore
	{
		get
		{
			return autoStore;
		}
		[param: In]
		set
		{
			ref bool fieldValue = ref autoStore;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				RefreshAutoStore(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 2u);
		}
	}

	public GameObject Network_operator
	{
		get
		{
			return _operator;
		}
		[param: In]
		set
		{
			ref GameObject gameObjectField = ref _operator;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				RefreshOperator(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVarGameObject(value, ref gameObjectField, 4u, ref ____operatorNetId);
		}
	}

	public ulong Networkrecipes
	{
		get
		{
			return recipes;
		}
		[param: In]
		set
		{
			ref ulong fieldValue = ref recipes;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				RefreshRecipes(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 8u);
		}
	}

	public bool Network_working
	{
		get
		{
			return _working;
		}
		[param: In]
		set
		{
			ref bool fieldValue = ref _working;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				RefreshWorking(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 16u);
		}
	}

	[SyncEvent]
	private event ProcessDelegate EventProcess;

	private void RefreshLoop(bool b)
	{
		Networkloop = b;
		base.enabled = true;
		loopChanged = true;
	}

	private void RefreshAutoStore(bool b)
	{
		NetworkautoStore = b;
		base.enabled = true;
	}

	private void RefreshOperator(GameObject op)
	{
		Network_operator = op;
		base.enabled = true;
	}

	private void RefreshRecipes(ulong u)
	{
		Networkrecipes = u;
		base.enabled = true;
		queueChanged = true;
	}

	private void RefreshWorking(bool b)
	{
		working.InvokeEdge(_working, b, this);
		Network_working = b;
	}

	public void Process(int ms)
	{
		CallEventProcess(ms);
	}

	private void Update()
	{
		base.enabled = false;
		initialized = true;
		updatedSignal.Invoke();
		if (loopChanged)
		{
			loopChangedSignal.Invoke();
		}
		if (queueChanged)
		{
			queueChangedSignal.Invoke();
		}
		loopChanged = false;
		queueChanged = false;
	}

	private void Awake()
	{
		EventProcess += delegate(int ms)
		{
			processingSignal.Invoke(ms);
		};
	}

	public void SetRecipes(List<RecipeInfo> recipes)
	{
		Networkrecipes = RecipesToLong(recipes);
	}

	public void GetRecipe(List<RecipeInfo> results)
	{
		FromLong(recipes, ids);
		ArrayToRecipe(ids, results);
	}

	public bool HasRecipe()
	{
		return recipes != 0;
	}

	private ulong RecipesToLong(List<RecipeInfo> input)
	{
		RecipeToArray(input, ids);
		return ToLong(ids);
	}

	private static void RecipeToArray(List<RecipeInfo> source, int[] result)
	{
		int i;
		for (i = 0; i < Mathf.Min(8, source.Count); i++)
		{
			result[i] = source[i].id;
		}
		for (; i < 8; i++)
		{
			result[i] = 0;
		}
	}

	private static void ArrayToRecipe(int[] source, List<RecipeInfo> result)
	{
		result.Clear();
		for (int i = 0; i < 8; i++)
		{
			if (source[i] != 0)
			{
				result.Add(CookBook.GetById(source[i]));
			}
		}
	}

	public static ulong ToLong(int[] ints)
	{
		ulong num = 0uL;
		for (int i = 0; i < 8; i++)
		{
			num += (ulong)((long)ints[i] << 8 * i);
		}
		return num;
	}

	public static void FromLong(ulong value, int[] result)
	{
		for (int i = 0; i < 8; i++)
		{
			ulong num = value >> 8 * i;
			result[i] = (int)(num & 0xFF);
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeSyncEventEventProcess(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("Event EventProcess called on server.");
		}
		else if (((CraftingState)obj).EventProcess != null)
		{
			((CraftingState)obj).EventProcess((int)reader.ReadPackedUInt32());
		}
	}

	public void CallEventProcess(int ms)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Event EventProcess called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)7);
		networkWriter.WritePackedUInt32((uint)kEventEventProcess);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)ms);
		SendEventInternal(networkWriter, 0, "EventProcess");
	}

	static CraftingState()
	{
		kEventEventProcess = -1747323616;
		NetworkBehaviour.RegisterEventDelegate(typeof(CraftingState), kEventEventProcess, InvokeSyncEventEventProcess);
		NetworkCRC.RegisterBehaviour("CraftingState", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(loop);
			writer.Write(autoStore);
			writer.Write(_operator);
			writer.WritePackedUInt64(recipes);
			writer.Write(_working);
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
			writer.Write(loop);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(autoStore);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(_operator);
		}
		if ((base.syncVarDirtyBits & 8) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt64(recipes);
		}
		if ((base.syncVarDirtyBits & 0x10) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(_working);
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
			loop = reader.ReadBoolean();
			autoStore = reader.ReadBoolean();
			____operatorNetId = reader.ReadNetworkId();
			recipes = reader.ReadPackedUInt64();
			_working = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			RefreshLoop(reader.ReadBoolean());
		}
		if ((num & 2) != 0)
		{
			RefreshAutoStore(reader.ReadBoolean());
		}
		if ((num & 4) != 0)
		{
			RefreshOperator(reader.ReadGameObject());
		}
		if ((num & 8) != 0)
		{
			RefreshRecipes(reader.ReadPackedUInt64());
		}
		if ((num & 0x10) != 0)
		{
			RefreshWorking(reader.ReadBoolean());
		}
	}

	public override void PreStartClient()
	{
		if (!____operatorNetId.IsEmpty())
		{
			Network_operator = ClientScene.FindLocalObject(____operatorNetId);
		}
	}
}
