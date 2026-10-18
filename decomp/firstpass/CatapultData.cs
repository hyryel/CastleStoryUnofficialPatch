using System.Runtime.InteropServices;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Game.Components.CatapultSpace;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class CatapultData : NetworkBehaviour
{
	public Signal NetworkUpdate = new Signal();

	[SerializeField]
	private CatapultOperations catapult;

	[SyncVar(hook = "SignalChange")]
	[JsonProperty]
	public int updateId;

	[SyncVar]
	private int flags;

	public bool Initialized => updateId != 0;

	public bool HasAmmo => (flags & 2) != 0;

	public bool HasTarget => (flags & 4) != 0;

	public bool ShotCalled => (flags & 8) != 0;

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

	public int Networkflags
	{
		get
		{
			return flags;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref flags, 2u);
		}
	}

	private void SignalChange(int value)
	{
		Neo.Relais(delegate
		{
			NetworkUpdate.Invoke();
		}, "CatapultData signal change");
	}

	public override void OnStartServer()
	{
		base.OnStartServer();
		NetworkUpdate.Invoke();
	}

	[ServerCallback]
	private void Update()
	{
		if (NetworkServer.active)
		{
			Set(canFindAmmo: false, catapult.CatapultHasAmmo(), catapult.CatapultHasTarget(), catapult.CatapultWantsToFire());
		}
	}

	public void Set(bool canFindAmmo, bool hasAmmo, bool hasTarget, bool shotCalled)
	{
		int num = 0;
		num |= (hasAmmo ? 2 : 0);
		num |= (hasTarget ? 4 : 0);
		num |= (shotCalled ? 8 : 0);
		if (num != flags)
		{
			Networkflags = num;
			NetworkupdateId = updateId + 1;
		}
	}

	private void Reset()
	{
		catapult = GetComponent<CatapultOperations>();
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.WritePackedUInt32((uint)updateId);
			writer.WritePackedUInt32((uint)flags);
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
			writer.WritePackedUInt32((uint)flags);
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
			flags = (int)reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			SignalChange((int)reader.ReadPackedUInt32());
		}
		if ((num & 2) != 0)
		{
			flags = (int)reader.ReadPackedUInt32();
		}
	}
}
