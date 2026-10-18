using System.Runtime.InteropServices;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Network;
using Newtonsoft.Json;
using UnityEngine.Networking;

public class TunnelData : NetworkGameComponent, IDigGoalResourceSetting
{
	public Signal NetworkUpdate = new Signal();

	[SyncVar(hook = "SignalChange")]
	[JsonProperty]
	private int updateId;

	[JsonProperty]
	[SyncVar]
	private int toHarvest = 7;

	public GroundResource TypesToHarvest => (GroundResource)toHarvest;

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

	public int NetworktoHarvest
	{
		get
		{
			return toHarvest;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref toHarvest, 2u);
		}
	}

	private void SignalChange(int value)
	{
		Neo.Relais(delegate
		{
			NetworkUpdate.Invoke();
		}, "TunnelData signal change");
		Neo.Relais(delegate
		{
			GameSignals.Invoke(TunnelProject.ResourceTypeKeptChanged);
		}, "Quarry data UI refresh");
	}

	public override void OnStartAnywhere()
	{
		base.OnStartAnywhere();
		NetworkUpdate.Invoke();
	}

	public void Set(int typesToHarvest)
	{
		NetworktoHarvest = typesToHarvest;
		NetworkupdateId = updateId + 1;
	}

	public void Toggle(GroundResource type)
	{
		GroundResource typesToHarvest = TypesToHarvest;
		typesToHarvest = ((!typesToHarvest.HasFlag(type)) ? (typesToHarvest | type) : (typesToHarvest & ~type));
		Set((int)typesToHarvest);
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.WritePackedUInt32((uint)updateId);
			writer.WritePackedUInt32((uint)toHarvest);
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
			writer.WritePackedUInt32((uint)updateId);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.WritePackedUInt32((uint)toHarvest);
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
			updateId = (int)reader.ReadPackedUInt32();
			toHarvest = (int)reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			SignalChange((int)reader.ReadPackedUInt32());
		}
		if ((num & 2) != 0)
		{
			toHarvest = (int)reader.ReadPackedUInt32();
		}
	}
}
