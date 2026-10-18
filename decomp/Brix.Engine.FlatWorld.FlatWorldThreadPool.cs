using System.Linq;
using Smooth.Slinq;

namespace Brix.Engine.FlatWorld;

public class FlatWorldThreadPool
{
	private FlatWorldThread[] _threads;

	public int Count => _threads.Length;

	public FlatWorldThread this[int i]
	{
		get
		{
			return _threads[i];
		}
		set
		{
			_threads[i] = value;
		}
	}

	public bool IsAlive => _threads.Slinq().Any((FlatWorldThread t) => t.IsAlive);

	public FlatWorldThreadPool(int threadCount)
	{
		Reset(threadCount);
	}

	public void Reset(int threadCount)
	{
		_threads = (from i in Enumerable.Range(0, threadCount)
			select new FlatWorldThread()).ToArray();
	}

	public bool ContainsLiveThreads(int start, int count)
	{
		return _threads.Slinq().Skip(start).Take(count)
			.Any((FlatWorldThread t) => t.IsAlive);
	}

	public bool ContainsFreeThreads(int start, int count)
	{
		return _threads.Slinq().Skip(start).Take(count)
			.Any((FlatWorldThread t) => !t.IsAlive);
	}

	public FlatWorldThread GetFreeThread()
	{
		for (int i = 0; i < _threads.Length; i++)
		{
			if (!_threads[i].IsAlive)
			{
				return _threads[i];
			}
		}
		return null;
	}

	public void StopAllThreads()
	{
		if (_threads == null)
		{
			return;
		}
		for (int i = 0; i < _threads.Length; i++)
		{
			if (_threads[i].Thread != null && _threads[i].Thread.IsAlive)
			{
				_threads[i].Thread.Abort();
			}
			_threads[i].Thread = null;
		}
	}

	public void Destroy()
	{
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.1.0.9782' (yours is '9.1.0.7988')
