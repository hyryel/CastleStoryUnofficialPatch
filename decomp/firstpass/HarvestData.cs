using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Brix.Engine;
using Brix.Engine.Nature;
using Brix.External.Factories;
using Brix.External.Signals;
using Newtonsoft.Json;
using UnityEngine.Networking;

public class HarvestData : NetworkBehaviour
{
	public Signal NetworkUpdate = new Signal();

	[JsonProperty]
	[SyncVar(hook = "SignalChange")]
	private int updateId;

	[SyncVar]
	[JsonProperty]
	private int acceptedTypes = 31;

	public NatureType AcceptedTypes => (NatureType)acceptedTypes;

	public bool Initialized => updateId != 0;

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

	public int NetworkacceptedTypes
	{
		get
		{
			return acceptedTypes;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref acceptedTypes, 2u);
		}
	}

	private void SignalChange(int value)
	{
		NetworkupdateId = updateId + 1;
		Neo.Relais(delegate
		{
			NetworkUpdate.Invoke();
		}, "HarvestData signal change");
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		NetworkUpdate.Invoke();
	}

	public void Set(int type)
	{
		NetworkacceptedTypes = type;
		NetworkupdateId = updateId + 1;
	}

	public static Factory.AssetKey GetHarvestMeta(NatureType type)
	{
		return type switch
		{
			NatureType.plant => Balises.HarvestPlantBeaconMeta, 
			NatureType.tree => Balises.HarvestTreeBeaconMeta, 
			NatureType.boulder => Balises.HarvestBoulderBeaconMeta, 
			NatureType.iron => Balises.HarvestIronBeaconMeta, 
			NatureType.pyrite => Balises.HarvestBrimstoneBeaconMeta, 
			NatureType.none => Balises.HarvestNoneBeaconMeta, 
			_ => Balises.HarvestBeaconMeta, 
		};
	}

	public void Toggle(NatureType type)
	{
		NatureType natureType = AcceptedTypes;
		natureType = ((!natureType.HasFlag(type)) ? (natureType | type) : (natureType & ~type));
		Set((int)natureType);
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
			writer.WritePackedUInt32((uint)acceptedTypes);
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
			writer.WritePackedUInt32((uint)acceptedTypes);
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
			acceptedTypes = (int)reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			SignalChange((int)reader.ReadPackedUInt32());
		}
		if ((num & 2) != 0)
		{
			acceptedTypes = (int)reader.ReadPackedUInt32();
		}
	}
}
