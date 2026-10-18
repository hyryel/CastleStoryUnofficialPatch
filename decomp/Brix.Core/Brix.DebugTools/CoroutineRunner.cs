using System.Collections;

namespace Brix.DebugTools;

public abstract class CoroutineRunner
{
	protected readonly IEnumerator Coroutine;

	protected bool IsRunning { get; set; }

	protected CoroutineRunner(IEnumerator coroutine)
	{
		Coroutine = coroutine;
		IsRunning = false;
	}

	public virtual void Start()
	{
		IsRunning = true;
	}

	public virtual void Stop()
	{
		IsRunning = false;
	}
}
