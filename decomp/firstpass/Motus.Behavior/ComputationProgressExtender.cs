using System;
using System.Collections.Generic;

namespace Motus.Behavior;

public static class ComputationProgressExtender
{
	public static IEnumerator<ComputationProgress<T>> WithWrappedFinally<T>(this IEnumerator<ComputationProgress<T>> computation, Action finalize)
	{
		T result = default(T);
		yield return ComputationProgress<T>.Sub((StaticNode)Node.Sequence.Do((IEnumerator<ComputationProgress<T>> c) => c, computation).Process((T r) => result = r).Finally(finalize));
		yield return ComputationProgress<T>.Return(result);
	}
}
