using System.Runtime.InteropServices;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace Brix.Game.AI;

public class RaidProjectData : NetworkBehaviour
{
	[SyncVar]
	[JsonProperty]
	public int WaveNumber;

	[JsonProperty]
	[SyncVar]
	public int minitronTotal;

	[JsonProperty]
	[SyncVar]
	public int corruptronTotal;

	[SyncVar]
	[JsonProperty]
	public int biftronTotal;

	[SyncVar]
	public int minitronCurrent;

	[SyncVar]
	public int corruptronCurrent;

	[SyncVar]
	public int biftronCurrent;

	public int NetworkWaveNumber
	{
		get
		{
			return WaveNumber;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref WaveNumber, 1u);
		}
	}

	public int NetworkminitronTotal
	{
		get
		{
			return minitronTotal;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref minitronTotal, 2u);
		}
	}

	public int NetworkcorruptronTotal
	{
		get
		{
			return corruptronTotal;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref corruptronTotal, 4u);
		}
	}

	public int NetworkbiftronTotal
	{
		get
		{
			return biftronTotal;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref biftronTotal, 8u);
		}
	}

	public int NetworkminitronCurrent
	{
		get
		{
			return minitronCurrent;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref minitronCurrent, 16u);
		}
	}

	public int NetworkcorruptronCurrent
	{
		get
		{
			return corruptronCurrent;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref corruptronCurrent, 32u);
		}
	}

	public int NetworkbiftronCurrent
	{
		get
		{
			return biftronCurrent;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref biftronCurrent, 64u);
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.WritePackedUInt32((uint)WaveNumber);
			writer.WritePackedUInt32((uint)minitronTotal);
			writer.WritePackedUInt32((uint)corruptronTotal);
			writer.WritePackedUInt32((uint)biftronTotal);
			writer.WritePackedUInt32((uint)minitronCurrent);
			writer.WritePackedUInt32((uint)corruptronCurrent);
			writer.WritePackedUInt32((uint)biftronCurrent);
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
			writer.WritePackedUInt32((uint)WaveNumber);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)minitronTotal);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)corruptronTotal);
		}
		if ((base.syncVarDirtyBits & 8) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)biftronTotal);
		}
		if ((base.syncVarDirtyBits & 0x10) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)minitronCurrent);
		}
		if ((base.syncVarDirtyBits & 0x20) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)corruptronCurrent);
		}
		if ((base.syncVarDirtyBits & 0x40) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)biftronCurrent);
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
			WaveNumber = (int)reader.ReadPackedUInt32();
			minitronTotal = (int)reader.ReadPackedUInt32();
			corruptronTotal = (int)reader.ReadPackedUInt32();
			biftronTotal = (int)reader.ReadPackedUInt32();
			minitronCurrent = (int)reader.ReadPackedUInt32();
			corruptronCurrent = (int)reader.ReadPackedUInt32();
			biftronCurrent = (int)reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			WaveNumber = (int)reader.ReadPackedUInt32();
		}
		if ((num & 2) != 0)
		{
			minitronTotal = (int)reader.ReadPackedUInt32();
		}
		if ((num & 4) != 0)
		{
			corruptronTotal = (int)reader.ReadPackedUInt32();
		}
		if ((num & 8) != 0)
		{
			biftronTotal = (int)reader.ReadPackedUInt32();
		}
		if ((num & 0x10) != 0)
		{
			minitronCurrent = (int)reader.ReadPackedUInt32();
		}
		if ((num & 0x20) != 0)
		{
			corruptronCurrent = (int)reader.ReadPackedUInt32();
		}
		if ((num & 0x40) != 0)
		{
			biftronCurrent = (int)reader.ReadPackedUInt32();
		}
	}
}
