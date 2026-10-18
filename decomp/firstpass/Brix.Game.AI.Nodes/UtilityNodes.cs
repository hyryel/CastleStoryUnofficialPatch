using System;
using Brix.External.Signals;
using Brix.Game.Components;
using Motus.Behavior;
using UnityEngine;

namespace Brix.Game.AI.Nodes;

public static class UtilityNodes
{
	public delegate bool BoolFunc();

	public static Node AndFail(this Node node, Exceptions.WorkFailedException e)
	{
		return (StaticNode)Node.Sequence.Do(node as StaticNode).Do(Node.Fail(e));
	}

	public static Node ShieldOrNot(this Node node, bool shield)
	{
		return (!shield) ? node : (node as StaticNode).ShieldNode().Root;
	}

	public static Node FailOnFalse<T>(Func<bool> f) where T : SharedWorkException<T>, new()
	{
		if (f())
		{
			return Node.Empty;
		}
		return Node.FailWork<T>();
	}

	public static Node FailOnFalse<T1, T2>(Func<T2, bool> f, T2 t2) where T1 : SharedWorkException<T1>, new()
	{
		if (f(t2))
		{
			return Node.Empty;
		}
		return Node.FailWork<T1>();
	}

	public static Node FailOnFalse<T1, T2, T3>(Func<T2, T3, bool> f, T2 t2, T3 t3) where T1 : SharedWorkException<T1>, new()
	{
		if (f(t2, t3))
		{
			return Node.Empty;
		}
		return Node.FailWork<T1>();
	}

	public static StaticNode PullOutIf(this StaticNode node, BoolFunc condition)
	{
		return Node.Selector.When((BoolFunc f) => f(), condition).Do(Node.Empty).Else()
			.Do(node);
	}

	public static StaticNode WaitForTimeOrSignal(IReadOnlySignal signal, float delay = 0.5f, bool failOnDelay = false)
	{
		if (failOnDelay)
		{
			return Node.Parallel.EndingCriteria(EndCriteria.WhenAnyDone).Do(Node.WaitFor.Signal(signal)).Do(Node.Sequence.Do(Node.WaitFor.Milliseconds(Mathf.RoundToInt(delay * 1000f))).Do(Node.Fail(FailException.Generic)));
		}
		return Node.Parallel.EndingCriteria(EndCriteria.WhenAnyDone).Do(Node.WaitFor.Signal(signal)).Do(Node.WaitFor.Milliseconds(Mathf.RoundToInt(delay * 1000f)));
	}

	public static StaticNode WaitForTimeOrSignal<T>(IReadOnlySignal<T> signal, float delay = 0.5f, bool failOnDelay = false)
	{
		if (failOnDelay)
		{
			return Node.Parallel.EndingCriteria(EndCriteria.WhenAnyDone).Do(Node.WaitFor.Signal(signal)).Do(Node.Sequence.Do(Node.WaitFor.Milliseconds(Mathf.RoundToInt(delay * 1000f))).Do(Node.Fail(FailException.Generic)));
		}
		return Node.Parallel.EndingCriteria(EndCriteria.WhenAnyDone).Do(Node.WaitFor.Signal(signal)).Do(Node.WaitFor.Milliseconds(Mathf.RoundToInt(delay * 1000f)));
	}

	public static StaticNode WaitForDelayOrCondition(Func<bool> condition, int delay, bool failOnDelay = false)
	{
		if (failOnDelay)
		{
			return Node.Parallel.EndingCriteria(EndCriteria.WhenAnyDone).Do(Node.WaitFor.Condition(condition)).Do(Node.Sequence.Do(Node.WaitFor.Ticks(delay)).Do(Node.Fail(FailException.Generic)));
		}
		return Node.Parallel.EndingCriteria(EndCriteria.WhenAnyDone).Do(Node.WaitFor.Condition(condition)).Do(Node.WaitFor.Ticks(delay));
	}

	public static StaticNode WaitForTimeOrCondition(Func<bool> condition, float delay, bool failOnDelay = false)
	{
		if (failOnDelay)
		{
			return Node.Parallel.EndingCriteria(EndCriteria.WhenAnyDone).Do(Node.WaitFor.Condition(condition)).Do(Node.Sequence.Do(Node.WaitFor.Milliseconds(Mathf.RoundToInt(delay * 1000f))).Do(Node.Fail(FailException.Generic)));
		}
		return Node.Parallel.EndingCriteria(EndCriteria.WhenAnyDone).Do(Node.WaitFor.Condition(condition)).Do(Node.WaitFor.Milliseconds(Mathf.RoundToInt(delay * 1000f)));
	}

	public static StaticNode WaitForTimeOrCondition<T>(Func<T, bool> condition, T arg, float delay, bool failOnDelay = false)
	{
		if (failOnDelay)
		{
			return Node.Parallel.EndingCriteria(EndCriteria.WhenAnyDone).Do(Node.WaitFor.Condition(condition, arg)).Do(Node.Sequence.Do(Node.WaitFor.Milliseconds(Mathf.RoundToInt(delay * 1000f))).Do(Node.Fail(FailException.Generic)));
		}
		return Node.Parallel.EndingCriteria(EndCriteria.WhenAnyDone).Do(Node.WaitFor.Condition(condition, arg)).Do(Node.WaitFor.Milliseconds(Mathf.RoundToInt(delay * 1000f)));
	}

	public static StaticNode WaitForTimeOrCondition<T1, T2>(Func<T1, T2, bool> condition, T1 t1, T2 t2, float delay, bool failOnDelay = false)
	{
		if (failOnDelay)
		{
			return Node.Parallel.EndingCriteria(EndCriteria.WhenAnyDone).Do(Node.WaitFor.Condition(condition, t1, t2)).Do(Node.Sequence.Do(Node.WaitFor.Milliseconds(Mathf.RoundToInt(delay * 1000f))).Do(Node.Fail(FailException.Generic)));
		}
		return Node.Parallel.EndingCriteria(EndCriteria.WhenAnyDone).Do(Node.WaitFor.Condition(condition, t1, t2)).Do(Node.WaitFor.Milliseconds(Mathf.RoundToInt(delay * 1000f)));
	}

	public static StaticNode RemoveObjectIf(GameObject go, BoolFunc condition)
	{
		return Node.Selector.If((BoolFunc f) => f(), condition).Do(delegate(GameObject _go)
		{
			_go.SafeNetworkDestroy();
		}, go);
	}
}
