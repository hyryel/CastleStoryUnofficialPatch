using System;
using System.Collections.Generic;
using Smooth.Algebraics;
using Smooth.Slinq;
using Smooth.Slinq.Context;

namespace Brix.Game.Components.Volumes;

public struct TemplatedEnumeratorContext<T, TEnumerator> where TEnumerator : IEnumerator<T>
{
	private TEnumerator enumerator;

	private static readonly Mutator<T, TemplatedEnumeratorContext<T, TEnumerator>> skip = Skip;

	private static readonly Mutator<T, TemplatedEnumeratorContext<T, TEnumerator>> remove = Remove;

	private static readonly Mutator<T, TemplatedEnumeratorContext<T, TEnumerator>> dispose = Dispose;

	private TemplatedEnumeratorContext(TEnumerator enumerator)
	{
		this.enumerator = enumerator;
	}

	public static Slinq<T, TemplatedEnumeratorContext<T, TEnumerator>> Slinq(TEnumerator enumerator)
	{
		return new Slinq<T, TemplatedEnumeratorContext<T, TEnumerator>>(context: new TemplatedEnumeratorContext<T, TEnumerator>(enumerator), skip: skip, remove: remove, dispose: dispose);
	}

	private static void Skip(ref TemplatedEnumeratorContext<T, TEnumerator> context, out Option<T> next)
	{
		if (context.enumerator.MoveNext())
		{
			T current = context.enumerator.Current;
			Option<T> option = new Option<T>(current);
			next = option;
		}
		else
		{
			next = Option<T>.None;
			context.enumerator.Dispose();
		}
	}

	private static void Remove(ref TemplatedEnumeratorContext<T, TEnumerator> context, out Option<T> next)
	{
		throw new NotSupportedException();
	}

	private static void Dispose(ref TemplatedEnumeratorContext<T, TEnumerator> context, out Option<T> next)
	{
		next = default(Option<T>);
		context.enumerator.Dispose();
	}
}
