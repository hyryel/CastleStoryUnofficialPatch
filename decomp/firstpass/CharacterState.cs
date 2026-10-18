using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Brix.External.Signals;
using Brix.Game.AI;
using Brix.Game.Components;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class CharacterState : NetworkBehaviour
{
	public Signal NetworkUpdate = new Signal();

	[SyncVar(hook = "SignalChange")]
	[JsonProperty]
	private int updateId;

	[SyncVar]
	[JsonProperty]
	private int autonomyID;

	[JsonProperty]
	[SyncVar]
	private int thoughtId;

	[SyncVar]
	[JsonProperty]
	private GameObject operable;

	[SyncVar]
	[JsonProperty]
	private int ammoCount;

	private NetworkInstanceId ___operableNetId;

	public bool Initialized => updateId != 0;

	public AutonomyStatus Autonomy
	{
		get
		{
			return AutonomyStatus.FromId((AutonomyStatus.Ids)autonomyID);
		}
		set
		{
			if (value.Id != (AutonomyStatus.Ids)autonomyID)
			{
				NetworkautonomyID = (int)value.Id;
				NetworkupdateId = updateId + 1;
			}
		}
	}

	public Thought Thought
	{
		get
		{
			return Thought.FromId(thoughtId);
		}
		set
		{
			if (thoughtId != value.id)
			{
				NetworkthoughtId = value.id;
				NetworkupdateId = updateId + 1;
			}
		}
	}

	public IOperable<Labor> Operable
	{
		get
		{
			return (!operable.IsNullOrReleased()) ? operable.GetInterface<IOperable<Labor>>() : null;
		}
		set
		{
			Networkoperable = value?.gameObject;
			NetworkupdateId = updateId + 1;
		}
	}

	public int AmmoCount
	{
		get
		{
			return ammoCount;
		}
		set
		{
			NetworkammoCount = value;
			NetworkupdateId = updateId + 1;
		}
	}

	public int NetworkupdateId
	{
		get
		{
			return updateId;
		}
		[param: In]
		set
		{
			ref int fieldValue = ref updateId;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SignalChange(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 1u);
		}
	}

	public int NetworkautonomyID
	{
		get
		{
			return autonomyID;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref autonomyID, 2u);
		}
	}

	public int NetworkthoughtId
	{
		get
		{
			return thoughtId;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref thoughtId, 4u);
		}
	}

	public GameObject Networkoperable
	{
		get
		{
			return operable;
		}
		[param: In]
		set
		{
			SetSyncVarGameObject(value, ref operable, 8u, ref ___operableNetId);
		}
	}

	public int NetworkammoCount
	{
		get
		{
			return ammoCount;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref ammoCount, 16u);
		}
	}

	private void SignalChange(int value)
	{
		base.enabled = true;
	}

	private void Update()
	{
		NetworkUpdate.Invoke();
		base.enabled = false;
	}

	[OnDeserialized]
	private void OnDeserialized(StreamingContext sc)
	{
		SetDirtyBit(uint.MaxValue);
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.WritePackedUInt32((uint)updateId);
			writer.WritePackedUInt32((uint)autonomyID);
			writer.WritePackedUInt32((uint)thoughtId);
			writer.Write(operable);
			writer.WritePackedUInt32((uint)ammoCount);
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
			writer.WritePackedUInt32((uint)updateId);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)autonomyID);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)thoughtId);
		}
		if ((base.syncVarDirtyBits & 8) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(operable);
		}
		if ((base.syncVarDirtyBits & 0x10) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)ammoCount);
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
			updateId = (int)reader.ReadPackedUInt32();
			autonomyID = (int)reader.ReadPackedUInt32();
			thoughtId = (int)reader.ReadPackedUInt32();
			___operableNetId = reader.ReadNetworkId();
			ammoCount = (int)reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			SignalChange((int)reader.ReadPackedUInt32());
		}
		if ((num & 2) != 0)
		{
			autonomyID = (int)reader.ReadPackedUInt32();
		}
		if ((num & 4) != 0)
		{
			thoughtId = (int)reader.ReadPackedUInt32();
		}
		if ((num & 8) != 0)
		{
			operable = reader.ReadGameObject();
		}
		if ((num & 0x10) != 0)
		{
			ammoCount = (int)reader.ReadPackedUInt32();
		}
	}

	public override void PreStartClient()
	{
		if (!___operableNetId.IsEmpty())
		{
			Networkoperable = ClientScene.FindLocalObject(___operableNetId);
		}
	}
}
