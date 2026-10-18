using System;
using System.Collections.Generic;
using Brix.External.Signals;
using Brix.UI.Icons;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Network;

public class NetworkIcons : NetworkBehaviour
{
	public enum Tracks
	{
		one,
		two
	}

	public Signal Changed = new Signal();

	private SyncListInt iconsIDs = new SyncListInt();

	private SyncListInt iconsIDs2 = new SyncListInt();

	private static int kListiconsIDs;

	private static int kListiconsIDs2;

	public override void OnStartClient()
	{
		SyncListInt syncListInt = iconsIDs;
		syncListInt.Callback = (SyncList<int>.SyncListChanged)Delegate.Combine(syncListInt.Callback, new SyncList<int>.SyncListChanged(SignalChange));
		SyncListInt syncListInt2 = iconsIDs2;
		syncListInt2.Callback = (SyncList<int>.SyncListChanged)Delegate.Combine(syncListInt2.Callback, new SyncList<int>.SyncListChanged(SignalChange));
	}

	private void SignalChange(SyncList<int>.Operation op, int index)
	{
		Changed.Invoke();
	}

	private SyncListInt Get(Tracks index)
	{
		return (index != Tracks.two) ? iconsIDs : iconsIDs2;
	}

	public IconKey At(int i, Tracks track)
	{
		SyncListInt syncListInt = Get(track);
		if (syncListInt.Count < i + 1)
		{
			return null;
		}
		return IconKey.FromId(syncListInt[i]);
	}

	public void Add(IconKey key, Tracks track)
	{
		Get(track).Add(key.Id);
	}

	public void Pop(Tracks track)
	{
		SyncListInt syncListInt = Get(track);
		if (syncListInt.Count > 0)
		{
			syncListInt.RemoveAt(0);
		}
	}

	public void Clear(Tracks track)
	{
		Get(track).Clear();
	}

	public void Keys(ref List<IconKey> keys, Tracks track)
	{
		Get(track).Slinq().AddTo<IconKey, List<IconKey>>(keys, IconKey.FromId);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeSyncListiconsIDs(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("SyncList iconsIDs called on server.");
		}
		else
		{
			((NetworkIcons)obj).iconsIDs.HandleMsg(reader);
		}
	}

	protected static void InvokeSyncListiconsIDs2(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("SyncList iconsIDs2 called on server.");
		}
		else
		{
			((NetworkIcons)obj).iconsIDs2.HandleMsg(reader);
		}
	}

	static NetworkIcons()
	{
		kListiconsIDs = 910494540;
		NetworkBehaviour.RegisterSyncListDelegate(typeof(NetworkIcons), kListiconsIDs, InvokeSyncListiconsIDs);
		kListiconsIDs2 = -1839440282;
		NetworkBehaviour.RegisterSyncListDelegate(typeof(NetworkIcons), kListiconsIDs2, InvokeSyncListiconsIDs2);
		NetworkCRC.RegisterBehaviour("NetworkIcons", 0);
	}

	private void Awake()
	{
		iconsIDs.InitializeBehaviour(this, kListiconsIDs);
		iconsIDs2.InitializeBehaviour(this, kListiconsIDs2);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			SyncListInt.WriteInstance(writer, iconsIDs);
			SyncListInt.WriteInstance(writer, iconsIDs2);
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
			SyncListInt.WriteInstance(writer, iconsIDs);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			SyncListInt.WriteInstance(writer, iconsIDs2);
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
			SyncListInt.ReadReference(reader, iconsIDs);
			SyncListInt.ReadReference(reader, iconsIDs2);
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			SyncListInt.ReadReference(reader, iconsIDs);
		}
		if ((num & 2) != 0)
		{
			SyncListInt.ReadReference(reader, iconsIDs2);
		}
	}
}
