using System.Runtime.InteropServices;
using Brix.External.Signals;
using Brix.Game.AI;
using Newtonsoft.Json;
using UnityEngine.Networking;

public class RecepteurState : NetworkBehaviour
{
	private Signal changed = new Signal();

	[JsonProperty]
	[SyncVar(hook = "ExludeAllChanged")]
	public bool ExcludeAll;

	[JsonProperty]
	[SyncVar(hook = "ExclusiveResourceChanged")]
	private string exlusiveResource;

	public IReadOnlySignal Changed => changed;

	public Ressource ExclusiveResource
	{
		get
		{
			return string.IsNullOrEmpty(exlusiveResource) ? null : (Adjectif.StringToAdjectif(exlusiveResource) as Ressource);
		}
		set
		{
			NetworkexlusiveResource = ((value == null) ? string.Empty : value.GetKey().codeName);
		}
	}

	public bool NetworkExcludeAll
	{
		get
		{
			return ExcludeAll;
		}
		[param: In]
		set
		{
			ref bool excludeAll = ref ExcludeAll;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				ExludeAllChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref excludeAll, 1u);
		}
	}

	public string NetworkexlusiveResource
	{
		get
		{
			return exlusiveResource;
		}
		[param: In]
		set
		{
			ref string fieldValue = ref exlusiveResource;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				ExclusiveResourceChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 2u);
		}
	}

	private void ExludeAllChanged(bool value)
	{
		NetworkExcludeAll = value;
		changed.Invoke();
	}

	private void ExclusiveResourceChanged(string r)
	{
		NetworkexlusiveResource = r;
		changed.Invoke();
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(ExcludeAll);
			writer.Write(exlusiveResource);
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
			writer.Write(ExcludeAll);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(exlusiveResource);
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
			ExcludeAll = reader.ReadBoolean();
			exlusiveResource = reader.ReadString();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			ExludeAllChanged(reader.ReadBoolean());
		}
		if ((num & 2) != 0)
		{
			ExclusiveResourceChanged(reader.ReadString());
		}
	}
}
