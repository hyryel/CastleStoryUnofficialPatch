using System.Runtime.InteropServices;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Network;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class LandscapeData : NetworkGameComponent
{
	public Signal NetworkUpdate = new Signal();

	[SyncVar]
	[JsonProperty]
	private Vector3 boundsMin;

	[SyncVar]
	[JsonProperty]
	private Vector3 boundsMax;

	[JsonProperty]
	[SyncVar]
	private int threshold;

	[SyncVar(hook = "SignalChange")]
	[JsonProperty]
	public int updateId;

	public int Threshold => threshold;

	public VoxelBounds Bounds => new VoxelBounds(XYZ.FromVector3(boundsMin), XYZ.FromVector3(boundsMax));

	public Vector3 NetworkboundsMin
	{
		get
		{
			return boundsMin;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref boundsMin, 1u);
		}
	}

	public Vector3 NetworkboundsMax
	{
		get
		{
			return boundsMax;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref boundsMax, 2u);
		}
	}

	public int Networkthreshold
	{
		get
		{
			return threshold;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref threshold, 4u);
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
			SetSyncVar(value, ref fieldValue, 8u);
		}
	}

	private void SignalChange(int value)
	{
		Neo.Relais(delegate
		{
			NetworkUpdate.Invoke();
		}, "LandscapeData signal change");
	}

	public override void OnStartAnywhere()
	{
		base.OnStartAnywhere();
		NetworkUpdate.Invoke();
	}

	public void Set(Vector3 min, Vector3 max, int _threshold)
	{
		NetworkboundsMin = min;
		NetworkboundsMax = max;
		Networkthreshold = _threshold;
		NetworkupdateId = updateId + 1;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(boundsMin);
			writer.Write(boundsMax);
			writer.WritePackedUInt32((uint)threshold);
			writer.WritePackedUInt32((uint)updateId);
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
			writer.Write(boundsMin);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(boundsMax);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.WritePackedUInt32((uint)threshold);
		}
		if ((base.syncVarDirtyBits & 8) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.WritePackedUInt32((uint)updateId);
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
			boundsMin = reader.ReadVector3();
			boundsMax = reader.ReadVector3();
			threshold = (int)reader.ReadPackedUInt32();
			updateId = (int)reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			boundsMin = reader.ReadVector3();
		}
		if ((num & 2) != 0)
		{
			boundsMax = reader.ReadVector3();
		}
		if ((num & 4) != 0)
		{
			threshold = (int)reader.ReadPackedUInt32();
		}
		if ((num & 8) != 0)
		{
			SignalChange((int)reader.ReadPackedUInt32());
		}
	}
}
