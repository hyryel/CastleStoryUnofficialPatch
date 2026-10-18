using System;
using System.Collections;

public static class GA_ContinuationManager
{
	private class EditorCoroutine
	{
		public IEnumerator Routine { get; private set; }

		public Func<bool> Done { get; private set; }

		public Action ContinueWith { get; private set; }

		public EditorCoroutine(IEnumerator routine, Func<bool> done)
		{
			Routine = routine;
			Done = done;
		}
	}
}
