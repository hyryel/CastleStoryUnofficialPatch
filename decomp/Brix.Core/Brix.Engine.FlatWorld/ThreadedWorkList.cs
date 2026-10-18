using System;
using System.Collections.Generic;

namespace Brix.Engine.FlatWorld;

[Serializable]
public class ThreadedWorkList
{
	public Queue<XYZ> DirtyMvs = new Queue<XYZ>();

	public HashSet<XYZ> BusyMvs = new HashSet<XYZ>();

	public HashSet<XYZ> WaitingMvs = new HashSet<XYZ>();

	public int TotalVoxels;

	public bool HasWork => DirtyMvs.Count > 0 || BusyMvs.Count > 0;

	public void Clear()
	{
		DirtyMvs.Clear();
		BusyMvs.Clear();
		WaitingMvs.Clear();
		TotalVoxels = 0;
	}

	public void SetDirty(XYZ q, int lod)
	{
		if (BusyMvs.Contains(q))
		{
			WaitingMvs.Add(q);
		}
		else
		{
			DirtyMvs.Enqueue(q);
		}
	}

	public bool TryReserve(out XYZ q)
	{
		if (DirtyMvs.Count == 0)
		{
			q = default(XYZ);
			return false;
		}
		q = DirtyMvs.Dequeue();
		BusyMvs.Add(q);
		return true;
	}

	public void Release(XYZ q)
	{
		BusyMvs.Remove(q);
		if (WaitingMvs.Remove(q))
		{
			DirtyMvs.Enqueue(q);
		}
	}
}
