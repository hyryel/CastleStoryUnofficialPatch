using System.Collections;
using UnityEngine;

namespace Brix.DebugTools;

public class InGameCoroutineRunner : CoroutineRunner
{
	private Coroutine coroutineExecutor;

	private static MonoBehaviour _coroutineContext;

	private static MonoBehaviour CoroutineContext
	{
		get
		{
			if (_coroutineContext == null)
			{
				GameObject gameObject = new GameObject("CoroutineRunner");
				gameObject.hideFlags = HideFlags.HideInHierarchy;
				_coroutineContext = gameObject.GetOrAddComponent<DummyMonoBehaviour>();
			}
			return _coroutineContext;
		}
	}

	public static CoroutineRunner Make(IEnumerator coroutine)
	{
		return new InGameCoroutineRunner(coroutine);
	}

	public static void Launch(IEnumerator coroutine)
	{
		Make(coroutine).Start();
	}

	protected InGameCoroutineRunner(IEnumerator coroutine)
		: base(coroutine)
	{
	}

	public override void Start()
	{
		if (!base.IsRunning)
		{
			base.Start();
			coroutineExecutor = CoroutineContext.StartCoroutine(Coroutine);
		}
	}

	public override void Stop()
	{
		if (base.IsRunning)
		{
			base.Stop();
			CoroutineContext.StopCoroutine(coroutineExecutor);
		}
	}
}
