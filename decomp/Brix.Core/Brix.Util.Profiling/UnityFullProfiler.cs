using System.Threading;

namespace Brix.Util.Profiling;

public class UnityFullProfiler : UnityLogProfiler
{
	private Thread _currentThread;

	public UnityFullProfiler(string name)
		: base(name)
	{
		_currentThread = Thread.CurrentThread;
	}

	public override void BeginSample(string s)
	{
	}

	public override void EndSample()
	{
	}

	public override void PrintSamples()
	{
	}
}
