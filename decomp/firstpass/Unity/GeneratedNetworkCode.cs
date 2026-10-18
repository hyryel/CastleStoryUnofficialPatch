using System.Runtime.InteropServices;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.Beacons;
using Brix.Game.Components.Tunnel;
using Brix.Game.Network;
using UnityEngine;
using UnityEngine.Networking;

namespace Unity;

[StructLayout(LayoutKind.Auto, CharSet = CharSet.Auto)]
public class GeneratedNetworkCode
{
	public static void _ReadStructBoundsSyncList_BeaconDriver(NetworkReader reader, BeaconDriver.BoundsSyncList instance)
	{
		ushort num = reader.ReadUInt16();
		instance.Clear();
		for (ushort num2 = 0; num2 < num; num2++)
		{
			instance.AddInternal(instance.DeserializeItem(reader));
		}
	}

	public static void _WriteStructBoundsSyncList_BeaconDriver(NetworkWriter writer, BeaconDriver.BoundsSyncList value)
	{
		ushort count = value.Count;
		writer.Write(count);
		for (ushort num = 0; num < count; num++)
		{
			value.SerializeItem(writer, value.GetItem(num));
		}
	}

	public static void _ReadStructSyncListVector_TunnelNetworkDeserialization(NetworkReader reader, TunnelNetworkDeserialization.SyncListVector instance)
	{
		ushort num = reader.ReadUInt16();
		instance.Clear();
		for (ushort num2 = 0; num2 < num; num2++)
		{
			instance.AddInternal(instance.DeserializeItem(reader));
		}
	}

	public static void _WriteStructSyncListVector_TunnelNetworkDeserialization(NetworkWriter writer, TunnelNetworkDeserialization.SyncListVector value)
	{
		ushort count = value.Count;
		writer.Write(count);
		for (ushort num = 0; num < count; num++)
		{
			value.SerializeItem(writer, value.GetItem(num));
		}
	}

	public static void _WriteGameParameters_GameParam(NetworkWriter writer, GameParam.GameParameters value)
	{
		writer.Write(value.MapCodeName);
		writer.Write(value.ModeCodeName);
		writer.Write(value.PresetCodeName);
		writer.WritePackedUInt64((ulong)value.AssetLastWriteTime);
		writer.WritePackedUInt32((uint)value.MinTeams);
		writer.WritePackedUInt32((uint)value.MaxTeams);
		writer.WritePackedUInt32((uint)value.MinPlayersPerTeam);
		writer.WritePackedUInt32((uint)value.MaxPlayersPerTeam);
		writer.WritePackedUInt32((uint)value.MinFactions);
		writer.WritePackedUInt32((uint)value.MaxFactions);
		writer.WritePackedUInt32((uint)value.MinPlayersPerFaction);
		writer.WritePackedUInt32((uint)value.MaxPlayersPerFactions);
		writer.Write(value.AllowSharingTeam);
		writer.Write(value.AllowSharingFaction);
	}

	public static GameParam.GameParameters _ReadGameParameters_GameParam(NetworkReader reader)
	{
		return new GameParam.GameParameters
		{
			MapCodeName = reader.ReadString(),
			ModeCodeName = reader.ReadString(),
			PresetCodeName = reader.ReadString(),
			AssetLastWriteTime = (long)reader.ReadPackedUInt64(),
			MinTeams = (int)reader.ReadPackedUInt32(),
			MaxTeams = (int)reader.ReadPackedUInt32(),
			MinPlayersPerTeam = (int)reader.ReadPackedUInt32(),
			MaxPlayersPerTeam = (int)reader.ReadPackedUInt32(),
			MinFactions = (int)reader.ReadPackedUInt32(),
			MaxFactions = (int)reader.ReadPackedUInt32(),
			MinPlayersPerFaction = (int)reader.ReadPackedUInt32(),
			MaxPlayersPerFactions = (int)reader.ReadPackedUInt32(),
			AllowSharingTeam = reader.ReadBoolean(),
			AllowSharingFaction = reader.ReadBoolean()
		};
	}

	public static Locomotion4.Impact _ReadImpact_Locomotion4(NetworkReader reader)
	{
		return new Locomotion4.Impact
		{
			Origin = reader.ReadVector3(),
			Force = reader.ReadVector3(),
			UpwardsEffect = reader.ReadSingle()
		};
	}

	public static void _WriteImpact_Locomotion4(NetworkWriter writer, Locomotion4.Impact value)
	{
		writer.Write(value.Origin);
		writer.Write(value.Force);
		writer.Write(value.UpwardsEffect);
	}

	public static int[] _ReadArrayInt32_None(NetworkReader reader)
	{
		int num = reader.ReadUInt16();
		if (num == 0)
		{
			return new int[0];
		}
		int[] array = new int[num];
		for (int i = 0; i < num; i++)
		{
			ref int reference = ref array[i];
			reference = (int)reader.ReadPackedUInt32();
		}
		return array;
	}

	public static void _WriteArrayInt32_None(NetworkWriter writer, int[] value)
	{
		if (value == null)
		{
			writer.Write((ushort)0);
			return;
		}
		ushort value2 = (ushort)value.Length;
		writer.Write(value2);
		for (ushort num = 0; num < value.Length; num++)
		{
			writer.WritePackedUInt32((uint)value[num]);
		}
	}

	public static void _WriteArrayInt32_None(NetworkWriter writer, int[] value)
	{
		if (value == null)
		{
			writer.Write((ushort)0);
			return;
		}
		ushort value2 = (ushort)value.Length;
		writer.Write(value2);
		for (ushort num = 0; num < value.Length; num++)
		{
			writer.WritePackedUInt32((uint)value[num]);
		}
	}

	public static void _WriteArrayInt32_None(NetworkWriter writer, int[] value)
	{
		if (value == null)
		{
			writer.Write((ushort)0);
			return;
		}
		ushort value2 = (ushort)value.Length;
		writer.Write(value2);
		for (ushort num = 0; num < value.Length; num++)
		{
			writer.WritePackedUInt32((uint)value[num]);
		}
	}

	public static void _WriteArrayInt32_None(NetworkWriter writer, int[] value)
	{
		if (value == null)
		{
			writer.Write((ushort)0);
			return;
		}
		ushort value2 = (ushort)value.Length;
		writer.Write(value2);
		for (ushort num = 0; num < value.Length; num++)
		{
			writer.WritePackedUInt32((uint)value[num]);
		}
	}

	public static void _WriteArrayInt32_None(NetworkWriter writer, int[] value)
	{
		if (value == null)
		{
			writer.Write((ushort)0);
			return;
		}
		ushort value2 = (ushort)value.Length;
		writer.Write(value2);
		for (ushort num = 0; num < value.Length; num++)
		{
			writer.WritePackedUInt32((uint)value[num]);
		}
	}

	public static void _WriteSyncListGameObject_Faction(NetworkWriter writer, Faction.SyncListGameObject value)
	{
	}

	public static Faction.SyncListGameObject _ReadSyncListGameObject_Faction(NetworkReader reader)
	{
		return new Faction.SyncListGameObject();
	}

	public static void _WriteTrackingPair_None(NetworkWriter writer, TrackingPair value)
	{
		writer.Write(value.go);
		writer.WritePackedUInt32((uint)value.type);
	}

	public static TrackingPair _ReadTrackingPair_None(NetworkReader reader)
	{
		return new TrackingPair
		{
			go = reader.ReadGameObject(),
			type = (int)reader.ReadPackedUInt32()
		};
	}

	public static GameObject[] _ReadArrayGameObject_None(NetworkReader reader)
	{
		int num = reader.ReadUInt16();
		if (num == 0)
		{
			return new GameObject[0];
		}
		GameObject[] array = new GameObject[num];
		for (int i = 0; i < num; i++)
		{
			ref GameObject reference = ref array[i];
			reference = reader.ReadGameObject();
		}
		return array;
	}

	public static void _WriteArrayGameObject_None(NetworkWriter writer, GameObject[] value)
	{
		if (value == null)
		{
			writer.Write((ushort)0);
			return;
		}
		ushort value2 = (ushort)value.Length;
		writer.Write(value2);
		for (ushort num = 0; num < value.Length; num++)
		{
			writer.Write(value[num]);
		}
	}

	public static void _WriteArrayGameObject_None(NetworkWriter writer, GameObject[] value)
	{
		if (value == null)
		{
			writer.Write((ushort)0);
			return;
		}
		ushort value2 = (ushort)value.Length;
		writer.Write(value2);
		for (ushort num = 0; num < value.Length; num++)
		{
			writer.Write(value[num]);
		}
	}

	public static void _WriteArrayGameObject_None(NetworkWriter writer, GameObject[] value)
	{
		if (value == null)
		{
			writer.Write((ushort)0);
			return;
		}
		ushort value2 = (ushort)value.Length;
		writer.Write(value2);
		for (ushort num = 0; num < value.Length; num++)
		{
			writer.Write(value[num]);
		}
	}

	public static void _WriteArrayGameObject_None(NetworkWriter writer, GameObject[] value)
	{
		if (value == null)
		{
			writer.Write((ushort)0);
			return;
		}
		ushort value2 = (ushort)value.Length;
		writer.Write(value2);
		for (ushort num = 0; num < value.Length; num++)
		{
			writer.Write(value[num]);
		}
	}
}
