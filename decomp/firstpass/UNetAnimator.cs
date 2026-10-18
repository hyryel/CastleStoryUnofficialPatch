using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Network;
using UnityEngine;
using UnityEngine.Networking;

[NetworkSettings(channel = 1, sendInterval = 0.1f)]
public class UNetAnimator : UpdatableNetworkGameComponent
{
	public class UnetAnimatorPlay : MessageBase
	{
		public NetworkInstanceId netId;

		public int hash;

		public UnetAnimatorPlay()
		{
		}

		public UnetAnimatorPlay(int argHash)
		{
			hash = argHash;
		}

		public override void Serialize(NetworkWriter writer)
		{
			writer.Write(netId);
			writer.WritePackedUInt32((uint)hash);
		}

		public override void Deserialize(NetworkReader reader)
		{
			netId = reader.ReadNetworkId();
			hash = (int)reader.ReadPackedUInt32();
		}
	}

	public class UnetAnimatorTrigger : MessageBase
	{
		public NetworkInstanceId netId;

		public int hash;

		public UnetAnimatorTrigger()
		{
		}

		public UnetAnimatorTrigger(int argHash)
		{
			hash = argHash;
		}

		public override void Serialize(NetworkWriter writer)
		{
			writer.Write(netId);
			writer.WritePackedUInt32((uint)hash);
		}

		public override void Deserialize(NetworkReader reader)
		{
			netId = reader.ReadNetworkId();
			hash = (int)reader.ReadPackedUInt32();
		}
	}

	public class UnetAnimatorResetTrigger : MessageBase
	{
		public NetworkInstanceId netId;

		public int hash;

		public UnetAnimatorResetTrigger()
		{
		}

		public UnetAnimatorResetTrigger(int argHash)
		{
			hash = argHash;
		}

		public override void Serialize(NetworkWriter writer)
		{
			writer.Write(netId);
			writer.WritePackedUInt32((uint)hash);
		}

		public override void Deserialize(NetworkReader reader)
		{
			netId = reader.ReadNetworkId();
			hash = (int)reader.ReadPackedUInt32();
		}
	}

	public class UnetAnimatorFloat : MessageBase
	{
		public NetworkInstanceId netId;

		public int hash;

		public float value;

		public float DampTime;

		public float DeltaTime;

		public UnetAnimatorFloat()
		{
		}

		public UnetAnimatorFloat(int argHash, float argValue, float argDampTime = 0f, float argDeltaTime = 0f)
		{
			hash = argHash;
			value = argValue;
			DampTime = argDampTime;
			DeltaTime = argDeltaTime;
		}

		public override void Serialize(NetworkWriter writer)
		{
			writer.Write(netId);
			writer.WritePackedUInt32((uint)hash);
			writer.Write(value);
			writer.Write(DampTime);
			writer.Write(DeltaTime);
		}

		public override void Deserialize(NetworkReader reader)
		{
			netId = reader.ReadNetworkId();
			hash = (int)reader.ReadPackedUInt32();
			value = reader.ReadSingle();
			DampTime = reader.ReadSingle();
			DeltaTime = reader.ReadSingle();
		}
	}

	public class UnetAnimatorInt : MessageBase
	{
		public NetworkInstanceId netId;

		public int hash;

		public int value;

		public UnetAnimatorInt()
		{
		}

		public UnetAnimatorInt(int argHash, int argValue)
		{
			hash = argHash;
			value = argValue;
		}

		public override void Serialize(NetworkWriter writer)
		{
			writer.Write(netId);
			writer.WritePackedUInt32((uint)hash);
			writer.WritePackedUInt32((uint)value);
		}

		public override void Deserialize(NetworkReader reader)
		{
			netId = reader.ReadNetworkId();
			hash = (int)reader.ReadPackedUInt32();
			value = (int)reader.ReadPackedUInt32();
		}
	}

	public class UnetAnimatorBool : MessageBase
	{
		public NetworkInstanceId netId;

		public int hash;

		public bool value;

		public UnetAnimatorBool()
		{
		}

		public UnetAnimatorBool(int argHash, bool argValue)
		{
			hash = argHash;
			value = argValue;
		}

		public override void Serialize(NetworkWriter writer)
		{
			writer.Write(netId);
			writer.WritePackedUInt32((uint)hash);
			writer.Write(value);
		}

		public override void Deserialize(NetworkReader reader)
		{
			netId = reader.ReadNetworkId();
			hash = (int)reader.ReadPackedUInt32();
			value = reader.ReadBoolean();
		}
	}

	private static readonly UpdatableNetworkProfiler<UNetAnimator> UpdatableProfiler = new UpdatableNetworkProfiler<UNetAnimator>();

	[SyncVar]
	public int serverAnimatorStateOnSpawn;

	[SyncVar]
	public float serverAnimatorCarryModeOnSpawn;

	private ulong previous_state;

	[SyncVar(hook = "stateChanged")]
	public ulong state;

	public Animator animator;

	private Queue<int> triggerQueue = new Queue<int>();

	protected override UpdatableProfiler Updatable => UpdatableProfiler;

	public int NetworkserverAnimatorStateOnSpawn
	{
		get
		{
			return serverAnimatorStateOnSpawn;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref serverAnimatorStateOnSpawn, 1u);
		}
	}

	public float NetworkserverAnimatorCarryModeOnSpawn
	{
		get
		{
			return serverAnimatorCarryModeOnSpawn;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref serverAnimatorCarryModeOnSpawn, 2u);
		}
	}

	public ulong Networkstate
	{
		get
		{
			return state;
		}
		[param: In]
		set
		{
			ref ulong fieldValue = ref state;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				stateChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 4u);
		}
	}

	public void SetFlagForId(int id, bool value)
	{
		if (value)
		{
			Networkstate = state | (ulong)(1L << id);
		}
		else
		{
			Networkstate = state & (ulong)(~(1L << id));
		}
	}

	public bool GetFlagFromState(ulong state, int id)
	{
		return (state & (ulong)(1L << id)) != 0;
	}

	private void stateChanged(ulong state)
	{
		if (!NetworkServer.active)
		{
			for (int i = 0; i <= BoolParameter.HightestId; i++)
			{
				if (GetFlagFromState(previous_state, i) ^ GetFlagFromState(state, i))
				{
					BoolParameter boolParameter = BoolParameter.boolParamForID[i];
					animator.SetBool(boolParameter.ParameterHash, GetFlagFromState(state, i));
					if (boolParameter.ParameterString == "SoftShieldBlock")
					{
						ToggleAnimLayer(AnimatorLayerActivator.CastleStoryLayers.shield, GetFlagFromState(state, i), 1f, 10f);
					}
				}
			}
		}
		previous_state = state;
	}

	public void ToggleAnimLayer(AnimatorLayerActivator.CastleStoryLayers layer, bool toggle, float limit, float speed)
	{
		Locomotion4 component = GetComponent<Locomotion4>();
		if (toggle)
		{
			GetComponent<Locomotion4>().layerActivator.SetLimit((int)layer, limit);
			component.layerActivator.SetSpeed((int)layer, speed);
			component.layerActivator.Activate((int)layer);
		}
		else
		{
			component.layerActivator.ResetLimit((int)layer);
			component.layerActivator.ResetSpeed((int)layer);
			component.layerActivator.Deactivate((int)layer);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		animator = base.gameObject.GetComponent<Animator>();
		if (!NetworkServer.active)
		{
			UNetManager.instance.LocalNetworkClient.RegisterHandler(58, boolCallBack);
			UNetManager.instance.LocalNetworkClient.RegisterHandler(56, floatCallback);
			UNetManager.instance.LocalNetworkClient.RegisterHandler(60, intCallback);
			UNetManager.instance.LocalNetworkClient.RegisterHandler(59, resetTriggerCallback);
			UNetManager.instance.LocalNetworkClient.RegisterHandler(62, resetExitTriggerCallback);
			UNetManager.instance.LocalNetworkClient.RegisterHandler(57, triggerCallBack);
			UNetManager.instance.LocalNetworkClient.RegisterHandler(61, playCallback);
		}
	}

	protected override void Start()
	{
		base.Start();
		StartCoroutine(ApplyTriggerQueue());
	}

	public override void OnStartPureClient()
	{
		base.OnStartAnywhere();
		animator.SetFloat("CarryMode", serverAnimatorCarryModeOnSpawn);
		animator.Play(serverAnimatorStateOnSpawn);
		stateChanged(state);
	}

	private static void playCallback(NetworkMessage message)
	{
		UnetAnimatorPlay unetAnimatorPlay = message.ReadMessage<UnetAnimatorPlay>();
		GameObject gameObject = ClientScene.FindLocalObject(unetAnimatorPlay.netId);
		if (!(gameObject == null))
		{
			UNetAnimator component = gameObject.GetComponent<UNetAnimator>();
			component.animator.Play(unetAnimatorPlay.hash);
		}
	}

	private static void resetExitTriggerCallback(NetworkMessage message)
	{
		UnetAnimatorResetTrigger unetAnimatorResetTrigger = message.ReadMessage<UnetAnimatorResetTrigger>();
		GameObject gameObject = ClientScene.FindLocalObject(unetAnimatorResetTrigger.netId);
		if (!(gameObject == null))
		{
			UNetAnimator component = gameObject.GetComponent<UNetAnimator>();
			component.triggerQueue.Enqueue(62);
		}
	}

	private static void resetTriggerCallback(NetworkMessage message)
	{
		UnetAnimatorResetTrigger unetAnimatorResetTrigger = message.ReadMessage<UnetAnimatorResetTrigger>();
		GameObject gameObject = ClientScene.FindLocalObject(unetAnimatorResetTrigger.netId);
		if (!(gameObject == null))
		{
			UNetAnimator component = gameObject.GetComponent<UNetAnimator>();
			component.triggerQueue.Enqueue(59);
		}
	}

	private static void floatCallback(NetworkMessage message)
	{
		UnetAnimatorFloat unetAnimatorFloat = message.ReadMessage<UnetAnimatorFloat>();
		GameObject gameObject = ClientScene.FindLocalObject(unetAnimatorFloat.netId);
		if (!(gameObject == null))
		{
			UNetAnimator component = gameObject.GetComponent<UNetAnimator>();
			component.animator.SetFloat(unetAnimatorFloat.hash, unetAnimatorFloat.value, unetAnimatorFloat.DampTime, unetAnimatorFloat.DeltaTime);
		}
	}

	private static void intCallback(NetworkMessage message)
	{
		UnetAnimatorInt unetAnimatorInt = message.ReadMessage<UnetAnimatorInt>();
		GameObject gameObject = ClientScene.FindLocalObject(unetAnimatorInt.netId);
		if (!(gameObject == null))
		{
			UNetAnimator component = gameObject.GetComponent<UNetAnimator>();
			component.animator.SetInteger(unetAnimatorInt.hash, unetAnimatorInt.value);
		}
	}

	private static void boolCallBack(NetworkMessage message)
	{
		UnetAnimatorBool unetAnimatorBool = message.ReadMessage<UnetAnimatorBool>();
		GameObject gameObject = ClientScene.FindLocalObject(unetAnimatorBool.netId);
		if (!(gameObject == null))
		{
			UNetAnimator component = gameObject.GetComponent<UNetAnimator>();
			component.animator.SetBool(unetAnimatorBool.hash, unetAnimatorBool.value);
		}
	}

	private static void triggerCallBack(NetworkMessage message)
	{
		UnetAnimatorTrigger unetAnimatorTrigger = message.ReadMessage<UnetAnimatorTrigger>();
		GameObject gameObject = ClientScene.FindLocalObject(unetAnimatorTrigger.netId);
		if (!(gameObject == null))
		{
			UNetAnimator component = gameObject.GetComponent<UNetAnimator>();
			component.triggerQueue.Enqueue(unetAnimatorTrigger.hash);
		}
	}

	private IEnumerator ApplyTriggerQueue()
	{
		while (true)
		{
			if (triggerQueue.Count != 0 && !animator.IsInTransition(0))
			{
				int hash = triggerQueue.Dequeue();
				if (hash != 59 && hash != 62)
				{
					animator.SetTrigger(hash);
				}
			}
			else
			{
				GetComponent<Locomotion4>().layerActivator.AnimatorUpdate(Time.deltaTime);
				yield return GameClock.WaitForEndOfFrame;
			}
		}
	}

	public void Play(int hash)
	{
		UnetAnimatorBool unetAnimatorBool = new UnetAnimatorBool();
		unetAnimatorBool.netId = base.netId;
		unetAnimatorBool.hash = hash;
		NetworkServer.SendByChannelToReady(base.gameObject, 61, unetAnimatorBool, 1);
	}

	public void SetFlag(int id, bool value)
	{
		SetFlagForId(id, value);
	}

	public void SetBool(int hash, bool value)
	{
		UnetAnimatorBool unetAnimatorBool = new UnetAnimatorBool();
		unetAnimatorBool.netId = base.netId;
		unetAnimatorBool.hash = hash;
		unetAnimatorBool.value = value;
		NetworkServer.SendByChannelToReady(base.gameObject, 58, unetAnimatorBool, 1);
	}

	public void SetFloat(int hash, float value)
	{
		UnetAnimatorFloat unetAnimatorFloat = new UnetAnimatorFloat();
		unetAnimatorFloat.netId = base.netId;
		unetAnimatorFloat.hash = hash;
		unetAnimatorFloat.value = value;
		NetworkServer.SendByChannelToReady(base.gameObject, 56, unetAnimatorFloat, 1);
	}

	public void SetInteger(int hash, int value)
	{
		UnetAnimatorInt unetAnimatorInt = new UnetAnimatorInt();
		unetAnimatorInt.netId = base.netId;
		unetAnimatorInt.hash = hash;
		unetAnimatorInt.value = value;
		NetworkServer.SendByChannelToReady(base.gameObject, 60, unetAnimatorInt, 1);
	}

	public void SetTrigger(int hash)
	{
		UnetAnimatorTrigger unetAnimatorTrigger = new UnetAnimatorTrigger();
		unetAnimatorTrigger.netId = base.netId;
		unetAnimatorTrigger.hash = hash;
		NetworkServer.SendByChannelToReady(base.gameObject, 57, unetAnimatorTrigger, 1);
	}

	public void ApplyDamp(int hash, float Target, float DampTime, float deltaTime)
	{
		UnetAnimatorFloat unetAnimatorFloat = new UnetAnimatorFloat();
		unetAnimatorFloat.netId = base.netId;
		unetAnimatorFloat.hash = hash;
		unetAnimatorFloat.DampTime = DampTime;
		unetAnimatorFloat.DeltaTime = deltaTime;
		unetAnimatorFloat.value = Target;
		NetworkServer.SendByChannelToReady(base.gameObject, 56, unetAnimatorFloat, 1);
	}

	public void ResetAllTriggers()
	{
		UnetAnimatorResetTrigger unetAnimatorResetTrigger = new UnetAnimatorResetTrigger();
		unetAnimatorResetTrigger.netId = base.netId;
		NetworkServer.SendByChannelToReady(base.gameObject, 59, unetAnimatorResetTrigger, 1);
	}

	public void ResetAllExitTriggers()
	{
		UnetAnimatorResetTrigger unetAnimatorResetTrigger = new UnetAnimatorResetTrigger();
		unetAnimatorResetTrigger.netId = base.netId;
		NetworkServer.SendByChannelToReady(base.gameObject, 62, unetAnimatorResetTrigger, 1);
	}

	private void OnReceivedState(int state)
	{
	}

	private void UNetVersion()
	{
	}

	public override int GetNetworkChannel()
	{
		return 1;
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.WritePackedUInt32((uint)serverAnimatorStateOnSpawn);
			writer.Write(serverAnimatorCarryModeOnSpawn);
			writer.WritePackedUInt64(state);
			return true;
		}
		bool flag2 = false;
		if ((base.syncVarDirtyBits & 1) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.WritePackedUInt32((uint)serverAnimatorStateOnSpawn);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(serverAnimatorCarryModeOnSpawn);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.WritePackedUInt64(state);
		}
		if (!flag2)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
		if (initialState)
		{
			serverAnimatorStateOnSpawn = (int)reader.ReadPackedUInt32();
			serverAnimatorCarryModeOnSpawn = reader.ReadSingle();
			state = reader.ReadPackedUInt64();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			serverAnimatorStateOnSpawn = (int)reader.ReadPackedUInt32();
		}
		if ((num & 2) != 0)
		{
			serverAnimatorCarryModeOnSpawn = reader.ReadSingle();
		}
		if ((num & 4) != 0)
		{
			stateChanged(reader.ReadPackedUInt64());
		}
	}
}
