using System.Collections;

namespace Brix.DebugTools;

public class GlobalCoroutineRunner
{
	public static CoroutineRunner Make(IEnumerator _routine)
	{
		return InGameCoroutineRunner.Make(_routine);
	}

	public static void Launch(IEnumerator coroutine)
	{
		Make(coroutine).Start();
	}
}
