using System.Threading;

namespace Brix.Util.Profiling;

public class UnityProfiler : IProfiler
{
	private readonly Thread _currentThread;

	public UnityProfiler(string name)
	{
		_currentThread = Thread.CurrentThread;
	}

	public void BeginSample(string s)
	{
	}

	public void EndSample()
	{
	}

	public void PrintSamples()
	{
	}
}
