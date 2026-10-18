using System.Collections.Generic;
using UnityEngine;

namespace Brix.Game.AI;

public class DestinationIterator
{
	private IEnumerator<WalkNode> NodesEnumerator;

	private readonly List<WalkNode> Nodes = new List<WalkNode>();

	public int CurrentIndex;

	public IPath Path;

	public bool HasCurrent => GetNth(CurrentIndex) != null;

	public bool Active => GetNth(CurrentIndex) != null || Destination != null;

	public WalkNode Current
	{
		get
		{
			GetNth(CurrentIndex + 10);
			return GetNth(CurrentIndex) ?? Destination;
		}
	}

	public WalkNode CurrentActive
	{
		get
		{
			GetNth(CurrentIndex + 10);
			return GetNth(CurrentIndex);
		}
	}

	public WalkNode Peek
	{
		get
		{
			GetNth(CurrentIndex + 10);
			return GetNth(CurrentIndex + 1);
		}
	}

	public WalkNode Destination
	{
		get
		{
			GetNth(10000);
			return (Nodes.Count <= 0) ? null : Nodes[Nodes.Count - 1];
		}
	}

	public void Clear()
	{
		Path = null;
		CurrentIndex = 0;
		Nodes.Clear();
		if (NodesEnumerator != null)
		{
			NodesEnumerator.Dispose();
		}
		NodesEnumerator = null;
	}

	public void MoveNext(int amount = 1)
	{
		CurrentIndex += amount;
	}

	public WalkNode GetNth(int i)
	{
		if (i < 0)
		{
			return null;
		}
		if (i < Nodes.Count)
		{
			return Nodes[i];
		}
		if (NodesEnumerator == null)
		{
			return null;
		}
		do
		{
			if (!NodesEnumerator.MoveNext())
			{
				NodesEnumerator.Dispose();
				NodesEnumerator = null;
				return null;
			}
			Nodes.Add(NodesEnumerator.Current);
		}
		while (Nodes.Count <= i);
		return Nodes[i];
	}

	public WalkNode GetNext(int offsetFromCurrent = 1)
	{
		return GetNth(CurrentIndex + offsetFromCurrent);
	}

	public void Replace(IPath path)
	{
		Path = path;
		Nodes.Clear();
		if (NodesEnumerator != null)
		{
			NodesEnumerator.Dispose();
		}
		NodesEnumerator = path.NodesEnumerator;
		GetNth(10000);
		CurrentIndex = 0;
	}

	public Vector3 GetPositionInTimeForSpeed(Vector3 origin, float time, float speed)
	{
		if (!HasCurrent)
		{
			return origin;
		}
		float num = time;
		Vector3 vector = origin;
		int num2 = CurrentIndex;
		WalkNode nth = GetNth(num2);
		if (nth == null)
		{
			return vector;
		}
		while (num * speed > Vector3.Distance(vector, nth.Position))
		{
			num -= Vector3.Distance(vector, nth.Position) / speed;
			vector = nth.Position;
			num2++;
			nth = GetNth(num2);
			if (nth == null)
			{
				return vector;
			}
		}
		return Vector3.Lerp(vector, nth.Position, num * speed / Vector3.Distance(vector, nth.Position));
	}
}
