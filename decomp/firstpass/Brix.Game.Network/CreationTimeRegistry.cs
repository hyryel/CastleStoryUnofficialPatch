using System.Collections.Generic;

namespace Brix.Game.Network;

public class CreationTimeRegistry<T> : Registry<T>
{
	protected Dictionary<int, T> id2t = new Dictionary<int, T>();

	protected int _nextId;

	public int RegisterObject(T t)
	{
		id2t[++_nextId] = t;
		return _nextId;
	}

	public void ForgetObject(int id)
	{
		id2t.Remove(id);
	}

	public virtual T GetObjectById(int id)
	{
		return id2t[id];
	}

	public virtual bool TryGetObject(int id, out T t)
	{
		return id2t.TryGetValue(id, out t);
	}

	public virtual void SetObjectWithId(T t, int id)
	{
		id2t[id] = t;
	}
}
