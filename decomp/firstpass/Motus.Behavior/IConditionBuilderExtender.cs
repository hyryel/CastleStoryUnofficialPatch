using System;
using Brix.External.Signals;
using Brix.Utils.Closures;

namespace Motus.Behavior;

public static class IConditionBuilderExtender
{
	private static Func<bool> _returnTrue = () => true;

	public static ConditionAppenderBuilder<TBuilder> Milliseconds<TBuilder>(this TBuilder b, int ms) where TBuilder : IConditionBuilder, IBuilderBase
	{
		b.Condition = TimeCondition.Request((float)ms / 1000f);
		return new ConditionAppenderBuilder<TBuilder>(b);
	}

	public static ConditionAppenderBuilder<TBuilder> Ticks<TBuilder>(this TBuilder b, int ticks) where TBuilder : IConditionBuilder, IBuilderBase
	{
		b.Condition = TickCondition.Request(ticks);
		return new ConditionAppenderBuilder<TBuilder>(b);
	}

	public static ConditionAppenderBuilder<TBuilder> Ever<TBuilder>(this TBuilder b) where TBuilder : IConditionBuilder, IBuilderBase
	{
		b.Condition = FalseCondition.Instance;
		return new ConditionAppenderBuilder<TBuilder>(b);
	}

	private static ConditionAppenderBuilder<TBuilder> Condition<TBuilder>(this TBuilder b, IClosure<IFunc<bool>> f) where TBuilder : IConditionBuilder, IBuilderBase
	{
		b.Condition = PredicateCondition.Request(f);
		return new ConditionAppenderBuilder<TBuilder>(b);
	}

	public static ConditionAppenderBuilder<TBuilder> Condition<TBuilder>(this TBuilder b, Func<bool> f) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Condition(f.Make().Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Condition<TBuilder, T1>(this TBuilder b, Func<T1, bool> f, T1 t1) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Condition(f.Make().Close(t1).Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Condition<TBuilder, T1, T2>(this TBuilder b, Func<T1, T2, bool> f, T1 t1, T2 t2) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Condition(f.Make().Close(t2).Close(t1)
			.Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Condition<TBuilder, T1, T2, T3>(this TBuilder b, Func<T1, T2, T3, bool> f, T1 t1, T2 t2, T3 t3) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Condition(f.Make().Close(t3).Close(t2)
			.Close(t1)
			.Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Condition<TBuilder, T1, T2, T3, T4>(this TBuilder b, Func<T1, T2, T3, T4, bool> f, T1 t1, T2 t2, T3 t3, T4 t4) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Condition(f.Make().Close(t4).Close(t3)
			.Close(t2)
			.Close(t1)
			.Pooled2());
	}

	private static ConditionAppenderBuilder<TBuilder> Signal<TBuilder>(this TBuilder b, IReadOnlySignal s, IClosure<IFunc<bool>> filter) where TBuilder : IConditionBuilder, IBuilderBase
	{
		b.Condition = SignalCondition.Request(s, filter);
		return new ConditionAppenderBuilder<TBuilder>(b);
	}

	private static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1>(this TBuilder b, IReadOnlySignal<T1> s, IClosure<IFunc<T1, bool>> filter) where TBuilder : IConditionBuilder, IBuilderBase
	{
		b.Condition = SignalCondition.Request(s, filter);
		return new ConditionAppenderBuilder<TBuilder>(b);
	}

	private static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1, T2>(this TBuilder b, IReadOnlySignal<T1, T2> s, IClosure<IFunc<T1, T2, bool>> filter) where TBuilder : IConditionBuilder, IBuilderBase
	{
		b.Condition = SignalCondition.Request(s, filter);
		return new ConditionAppenderBuilder<TBuilder>(b);
	}

	private static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1, T2, T3>(this TBuilder b, IReadOnlySignal<T1, T2, T3> s, IClosure<IFunc<T1, T2, T3, bool>> filter) where TBuilder : IConditionBuilder, IBuilderBase
	{
		b.Condition = SignalCondition.Request(s, filter);
		return new ConditionAppenderBuilder<TBuilder>(b);
	}

	private static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1, T2, T3, T4>(this TBuilder b, IReadOnlySignal<T1, T2, T3, T4> s, IClosure<IFunc<T1, T2, T3, T4, bool>> filter) where TBuilder : IConditionBuilder, IBuilderBase
	{
		b.Condition = SignalCondition.Request(s, filter);
		return new ConditionAppenderBuilder<TBuilder>(b);
	}

	public static ConditionAppenderBuilder<TBuilder> Signal<TBuilder>(this TBuilder b, IReadOnlySignal s) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Signal(s, _returnTrue.Make().Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Signal<TBuilder>(this TBuilder b, IReadOnlySignal s, Func<bool> filter) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Signal(s, filter.Make().Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1>(this TBuilder b, IReadOnlySignal s, Func<T1, bool> filter, T1 t1) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Signal(s, filter.Make().Close(t1).Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1, T2>(this TBuilder b, IReadOnlySignal s, Func<T1, T2, bool> filter, T1 t1, T2 t2) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Signal(s, filter.Make().Close(t2).Close(t1)
			.Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1, T2, T3>(this TBuilder b, IReadOnlySignal s, Func<T1, T2, T3, bool> filter, T1 t1, T2 t2, T3 t3) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Signal(s, filter.Make().Close(t3).Close(t2)
			.Close(t1)
			.Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1, T2, T3, T4>(this TBuilder b, IReadOnlySignal s, Func<T1, T2, T3, T4, bool> filter, T1 t1, T2 t2, T3 t3, T4 t4) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Signal(s, filter.Make().Close(t4).Close(t3)
			.Close(t2)
			.Close(t1)
			.Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1, T2, T3, T4, T5>(this TBuilder b, IReadOnlySignal s, Func<T1, T2, T3, T4, T5, bool> filter, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Signal(s, filter.Make().Close(t5).Close(t4)
			.Close(t3)
			.Close(t2)
			.Close(t1)
			.Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1>(this TBuilder b, IReadOnlySignal<T1> s) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Signal(s, _returnTrue.Make().Pad<T1>().Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1>(this TBuilder b, IReadOnlySignal<T1> s, Func<bool> filter) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Signal(s, filter.Make().Pad<T1>().Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1>(this TBuilder b, IReadOnlySignal<T1> s, Func<T1, bool> filter) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Signal(s, filter.Make().Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1, T2>(this TBuilder b, IReadOnlySignal<T1> s, Func<T1, T2, bool> filter, T2 t2) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Signal(s, filter.Make().Close(t2).Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1, T2, T3>(this TBuilder b, IReadOnlySignal<T1> s, Func<T1, T2, T3, bool> filter, T2 t2, T3 t3) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Signal(s, filter.Make().Close(t3).Close(t2)
			.Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1, T2, T3, T4>(this TBuilder b, IReadOnlySignal<T1> s, Func<T1, T2, T3, T4, bool> filter, T2 t2, T3 t3, T4 t4) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Signal(s, filter.Make().Close(t4).Close(t3)
			.Close(t2)
			.Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1, T2, T3, T4, T5>(this TBuilder b, IReadOnlySignal<T1> s, Func<T1, T2, T3, T4, T5, bool> filter, T2 t2, T3 t3, T4 t4, T5 t5) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Signal(s, filter.Make().Close(t5).Close(t4)
			.Close(t3)
			.Close(t2)
			.Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1, T2>(this TBuilder b, IReadOnlySignal<T1, T2> s) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Signal(s, _returnTrue.Make().Pad<T1>().Pad<T2>()
			.Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1, T2>(this TBuilder b, IReadOnlySignal<T1, T2> s, Func<bool> filter) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Signal(s, filter.Make().Pad<T1>().Pad<T2>()
			.Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1, T2>(this TBuilder b, IReadOnlySignal<T1, T2> s, Func<T1, bool> filter) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Signal(s, filter.Make().Pad<T2>().Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1, T2>(this TBuilder b, IReadOnlySignal<T1, T2> s, Func<T1, T2, bool> filter) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Signal(s, filter.Make().Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1, T2, T3>(this TBuilder b, IReadOnlySignal<T1, T2> s, Func<T1, T2, T3, bool> filter, T3 t3) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Signal(s, filter.Make().Close(t3).Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1, T2, T3, T4>(this TBuilder b, IReadOnlySignal<T1, T2> s, Func<T1, T2, T3, T4, bool> filter, T3 t3, T4 t4) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Signal(s, filter.Make().Close(t4).Close(t3)
			.Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1, T2, T3, T4, T5>(this TBuilder b, IReadOnlySignal<T1, T2> s, Func<T1, T2, T3, T4, T5, bool> filter, T3 t3, T4 t4, T5 t5) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Signal(s, filter.Make().Close(t5).Close(t4)
			.Close(t3)
			.Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1, T2, T3>(this TBuilder b, IReadOnlySignal<T1, T2, T3> s) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Signal(s, _returnTrue.Make().Pad<T1>().Pad<T2>()
			.Pad<T3>()
			.Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1, T2, T3>(this TBuilder b, IReadOnlySignal<T1, T2, T3> s, Func<bool> filter) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Signal(s, filter.Make().Pad<T1>().Pad<T2>()
			.Pad<T3>()
			.Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1, T2, T3>(this TBuilder b, IReadOnlySignal<T1, T2, T3> s, Func<T1, bool> filter) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Signal(s, filter.Make().Pad<T2>().Pad<T3>()
			.Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1, T2, T3>(this TBuilder b, IReadOnlySignal<T1, T2, T3> s, Func<T1, T2, bool> filter) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Signal(s, filter.Make().Pad<T3>().Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1, T2, T3>(this TBuilder b, IReadOnlySignal<T1, T2, T3> s, Func<T1, T2, T3, bool> filter) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Signal(s, filter.Make().Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1, T2, T3, T4>(this TBuilder b, IReadOnlySignal<T1, T2, T3> s, Func<T1, T2, T3, T4, bool> filter, T4 t4) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Signal(s, filter.Make().Close(t4).Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1, T2, T3, T4, T5>(this TBuilder b, IReadOnlySignal<T1, T2, T3> s, Func<T1, T2, T3, T4, T5, bool> filter, T4 t4, T5 t5) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Signal(s, filter.Make().Close(t5).Close(t4)
			.Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1, T2, T3, T4>(this TBuilder b, IReadOnlySignal<T1, T2, T3, T4> s) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Signal(s, _returnTrue.Make().Pad<T1>().Pad<T2>()
			.Pad<T3>()
			.Pad<T4>()
			.Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1, T2, T3, T4>(this TBuilder b, IReadOnlySignal<T1, T2, T3, T4> s, Func<bool> filter) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Signal(s, filter.Make().Pad<T1>().Pad<T2>()
			.Pad<T3>()
			.Pad<T4>()
			.Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1, T2, T3, T4>(this TBuilder b, IReadOnlySignal<T1, T2, T3, T4> s, Func<T1, bool> filter) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Signal(s, filter.Make().Pad<T2>().Pad<T3>()
			.Pad<T4>()
			.Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1, T2, T3, T4>(this TBuilder b, IReadOnlySignal<T1, T2, T3, T4> s, Func<T1, T2, bool> filter) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Signal(s, filter.Make().Pad<T3>().Pad<T4>()
			.Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1, T2, T3, T4>(this TBuilder b, IReadOnlySignal<T1, T2, T3, T4> s, Func<T1, T2, T3, bool> filter) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Signal(s, filter.Make().Pad<T4>().Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1, T2, T3, T4>(this TBuilder b, IReadOnlySignal<T1, T2, T3, T4> s, Func<T1, T2, T3, T4, bool> filter) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Signal(s, filter.Make().Pooled2());
	}

	public static ConditionAppenderBuilder<TBuilder> Signal<TBuilder, T1, T2, T3, T4, T5>(this TBuilder b, IReadOnlySignal<T1, T2, T3, T4> s, Func<T1, T2, T3, T4, T5, bool> filter, T5 t5) where TBuilder : IConditionBuilder, IBuilderBase
	{
		return b.Signal(s, filter.Make().Close(t5).Pooled2());
	}
}
