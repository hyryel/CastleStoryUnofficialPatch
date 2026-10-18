using System.Runtime.InteropServices;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Game.Components;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace Brix.Game.AI;

public class ProjectState : UpdatableNetworkGameComponent
{
	private static readonly UpdatableNetworkProfiler<ProjectState> UpdatableProfiler = new UpdatableNetworkProfiler<ProjectState>();

	public Signal NetworkUpdate = new Signal();

	public Project project;

	public CraftingStation furnace;

	[JsonProperty]
	private bool initialized;

	[SyncVar(hook = "hk_flagsChanged")]
	[JsonProperty]
	private int flags;

	[SyncVar(hook = "hk_biasChanged")]
	[JsonProperty]
	private int bias;

	protected override UpdatableProfiler Updatable => UpdatableProfiler;

	public ProjectPriority.Bias Bias
	{
		get
		{
			return (ProjectPriority.Bias)bias;
		}
		set
		{
			Networkbias = (int)value;
			initialized = true;
		}
	}

	public bool ProjectStateInitialized => initialized;

	public bool HasGoals => (flags & 2) != 0;

	public bool Commited
	{
		get
		{
			return (flags & 8) != 0;
		}
		set
		{
			Networkflags = flags | 8;
		}
	}

	public bool ProblematicProject => ProjectStateInitialized && !HasGoals;

	public int Networkflags
	{
		get
		{
			return flags;
		}
		[param: In]
		set
		{
			ref int fieldValue = ref flags;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				hk_flagsChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 1u);
		}
	}

	public int Networkbias
	{
		get
		{
			return bias;
		}
		[param: In]
		set
		{
			ref int fieldValue = ref bias;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				hk_biasChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 2u);
		}
	}

	private void hk_flagsChanged(int value)
	{
		Networkflags = value;
		NetworkUpdate.Invoke();
	}

	private void hk_biasChanged(int value)
	{
		Networkbias = value;
		NetworkUpdate.Invoke();
	}

	public override void OnStartServerOrHost()
	{
		base.OnStartServerOrHost();
		GameSignals.LateConnectOnce(GameSignals.LevelReady, delegate
		{
			SetDirtyBit(uint.MaxValue);
		});
	}

	private void Set(bool full, bool goals, bool editing, bool storage, bool resource)
	{
		int num = 0;
		num |= (full ? 1 : 0);
		num |= (goals ? 2 : 0);
		num |= (editing ? 4 : 0);
		num |= (Commited ? 8 : 0);
		num |= (storage ? 16 : 0);
		num |= (resource ? 32 : 0);
		if (num != flags)
		{
			Networkflags = num;
			initialized = true;
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.WritePackedUInt32((uint)flags);
			writer.WritePackedUInt32((uint)bias);
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
			writer.WritePackedUInt32((uint)flags);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.WritePackedUInt32((uint)bias);
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
			flags = (int)reader.ReadPackedUInt32();
			bias = (int)reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			hk_flagsChanged((int)reader.ReadPackedUInt32());
		}
		if ((num & 2) != 0)
		{
			hk_biasChanged((int)reader.ReadPackedUInt32());
		}
	}
}
