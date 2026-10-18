using Brix.Utils.Logging;

namespace Brix.Game.Network;

public class LooseCreationTimeRegistry<T> : CreationTimeRegistry<T> where T : new()
{
	private static BrixLogger logger = new BrixLogger(typeof(LooseCreationTimeRegistry<T>).FullName);

	public bool Locked;

	public int NextId => _nextId;

	public int GetAndIncreaseNextId()
	{
		return ++_nextId;
	}

	public T PeekObject()
	{
		return id2t.GetValueOrDefault(_nextId + 1);
	}

	public override T GetObjectById(int id)
	{
		return id2t.NewKey(id);
	}

	public void Reset()
	{
		id2t.Clear();
		_nextId = 0;
	}

	public void Skip(int nextId)
	{
		if (nextId < _nextId)
		{
			logger.Warn($"Network Registry attempted to skip by a negative offset (current={_nextId}, wanted={nextId}). This usually means a slave has done more work than its master.");
		}
		else
		{
			_nextId = nextId;
		}
	}
}
