using System.Runtime.InteropServices;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Game.AI;
using Brix.Input;
using Brix.Network;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class QuarryData : NetworkGameComponent, IDigGoalResourceSetting
{
	public Signal NetworkUpdate = new Signal();

	[SyncVar(hook = "SignalChange")]
	[JsonProperty]
	private int updateId;

	[SyncVar]
	[JsonProperty]
	private bool aGoalWasCompleted;

	[JsonProperty]
	[SyncVar]
	private Vector3 boundsMin;

	[JsonProperty]
	[SyncVar]
	private Vector3 boundsMax;

	[JsonProperty]
	[SyncVar]
	private Vector3 stairCorner;

	[JsonProperty]
	[SyncVar]
	private Vector3 stairDirection;

	[SyncVar]
	[JsonProperty]
	private int stairStartY;

	[SyncVar]
	[JsonProperty]
	private int toHarvest = 7;

	public GroundResource TypesToHarvest => (GroundResource)toHarvest;

	public bool Commited => aGoalWasCompleted;

	public StairOrientation StairOrientation => StairOrientation.From(XYZ.FromVector3(stairCorner), XYZ.FromVector3(stairDirection));

	public VoxelBounds Bounds => new VoxelBounds(XYZ.FromVector3(boundsMin), XYZ.FromVector3(boundsMax));

	public int StairY => stairStartY;

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

	public bool NetworkaGoalWasCompleted
	{
		get
		{
			return aGoalWasCompleted;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref aGoalWasCompleted, 2u);
		}
	}

	public Vector3 NetworkboundsMin
	{
		get
		{
			return boundsMin;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref boundsMin, 4u);
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
			SetSyncVar(value, ref boundsMax, 8u);
		}
	}

	public Vector3 NetworkstairCorner
	{
		get
		{
			return stairCorner;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref stairCorner, 16u);
		}
	}

	public Vector3 NetworkstairDirection
	{
		get
		{
			return stairDirection;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref stairDirection, 32u);
		}
	}

	public int NetworkstairStartY
	{
		get
		{
			return stairStartY;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref stairStartY, 64u);
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
			SetSyncVar(value, ref toHarvest, 128u);
		}
	}

	private void SignalChange(int value)
	{
		Neo.Relais(delegate
		{
			NetworkUpdate.Invoke();
		}, "QuarryData signal change");
		Neo.Relais(delegate
		{
			GameSignals.Invoke(MineProject.ResourceTypeKeptChanged);
		}, "Quarry data UI refresh");
	}

	public override void OnStartAnywhere()
	{
		base.OnStartAnywhere();
		NetworkUpdate.Invoke();
	}

	public void Set(Vector3 min, Vector3 max, Vector3 corner, Vector3 direction, int y, bool goalCompleted)
	{
		NetworkboundsMin = min;
		NetworkboundsMax = max;
		NetworkstairCorner = corner;
		NetworkstairDirection = direction;
		NetworkstairStartY = y;
		NetworkaGoalWasCompleted = goalCompleted;
		NetworkupdateId = updateId + 1;
	}

	public void SetTypesToHarvest(int typesToHarvest)
	{
		NetworktoHarvest = typesToHarvest;
		NetworkupdateId = updateId + 1;
	}

	public void Toggle(GroundResource type)
	{
		GroundResource typesToHarvest = TypesToHarvest;
		typesToHarvest = ((!typesToHarvest.HasFlag(type)) ? (typesToHarvest | type) : (typesToHarvest & ~type));
		SetTypesToHarvest((int)typesToHarvest);
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
			writer.Write(aGoalWasCompleted);
			writer.Write(boundsMin);
			writer.Write(boundsMax);
			writer.Write(stairCorner);
			writer.Write(stairDirection);
			writer.WritePackedUInt32((uint)stairStartY);
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
			writer.Write(aGoalWasCompleted);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(boundsMin);
		}
		if ((base.syncVarDirtyBits & 8) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(boundsMax);
		}
		if ((base.syncVarDirtyBits & 0x10) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(stairCorner);
		}
		if ((base.syncVarDirtyBits & 0x20) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(stairDirection);
		}
		if ((base.syncVarDirtyBits & 0x40) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.WritePackedUInt32((uint)stairStartY);
		}
		if ((base.syncVarDirtyBits & 0x80) != 0)
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
			aGoalWasCompleted = reader.ReadBoolean();
			boundsMin = reader.ReadVector3();
			boundsMax = reader.ReadVector3();
			stairCorner = reader.ReadVector3();
			stairDirection = reader.ReadVector3();
			stairStartY = (int)reader.ReadPackedUInt32();
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
			aGoalWasCompleted = reader.ReadBoolean();
		}
		if ((num & 4) != 0)
		{
			boundsMin = reader.ReadVector3();
		}
		if ((num & 8) != 0)
		{
			boundsMax = reader.ReadVector3();
		}
		if ((num & 0x10) != 0)
		{
			stairCorner = reader.ReadVector3();
		}
		if ((num & 0x20) != 0)
		{
			stairDirection = reader.ReadVector3();
		}
		if ((num & 0x40) != 0)
		{
			stairStartY = (int)reader.ReadPackedUInt32();
		}
		if ((num & 0x80) != 0)
		{
			toHarvest = (int)reader.ReadPackedUInt32();
		}
	}
}
