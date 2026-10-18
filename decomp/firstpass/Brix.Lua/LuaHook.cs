using Brix.Engine;
using Brix.External.Signals;
using MoonSharp.Interpreter;

namespace Brix.Lua;

public class LuaHook : LuaHookBase
{
	public Signal _signal;

	public LuaHook()
	{
		_signal = new Signal
		{
			MaximumConnections = 1000
		};
	}

	public LuaHook(Signal signal)
	{
		_signal = signal;
	}

	public LuaHook(IReadOnlySignal signal)
	{
		_signal = (Signal)signal;
	}

	public LuaHook(GameSignals.GameSignal signal)
	{
		_signal = signal._signal;
	}

	protected override void ConnectTemplated(Closure act, LuaHook dishook)
	{
		LuaHookBase.ConnectFromCLR(this, act, dishook);
	}

	protected override void ConnectOnceTemplated(Closure act, LuaHook dishook)
	{
		LuaHookBase.ConnectOnceFromCLR(this, act, dishook);
	}

	protected override void InvokeTemplated(Table args)
	{
		LuaHookBase.InvokeFromCLR(this);
	}
}
public class LuaHook<T1> : LuaHookBase
{
	public Signal<T1> _signal;

	public LuaHook()
	{
		_signal = new Signal<T1>
		{
			MaximumConnections = 1000
		};
	}

	public LuaHook(Signal<T1> signal)
	{
		_signal = signal;
	}

	public LuaHook(IReadOnlySignal<T1> signal)
	{
		_signal = (Signal<T1>)signal;
	}

	public LuaHook(GameSignals.GameSignal<T1> signal)
	{
		_signal = signal._signal;
	}

	protected override void ConnectTemplated(Closure act, LuaHook dishook)
	{
		LuaHookBase.ConnectFromCLR(this, act, dishook);
	}

	protected override void ConnectOnceTemplated(Closure act, LuaHook dishook)
	{
		LuaHookBase.ConnectOnceFromCLR(this, act, dishook);
	}

	protected override void InvokeTemplated(Table args)
	{
		LuaHookBase.InvokeFromCLR(this, args.RawGet(1).ToObject<T1>());
	}
}
public class LuaHook<T1, T2> : LuaHookBase
{
	public Signal<T1, T2> _signal;

	public LuaHook()
	{
		_signal = new Signal<T1, T2>
		{
			MaximumConnections = 1000
		};
	}

	public LuaHook(Signal<T1, T2> signal)
	{
		_signal = signal;
	}

	public LuaHook(IReadOnlySignal<T1, T2> signal)
	{
		_signal = (Signal<T1, T2>)signal;
	}

	public LuaHook(GameSignals.GameSignal<T1, T2> signal)
	{
		_signal = signal._signal;
	}

	protected override void ConnectTemplated(Closure act, LuaHook dishook)
	{
		LuaHookBase.ConnectFromCLR(this, act, dishook);
	}

	protected override void ConnectOnceTemplated(Closure act, LuaHook dishook)
	{
		LuaHookBase.ConnectOnceFromCLR(this, act, dishook);
	}

	protected override void InvokeTemplated(Table args)
	{
		LuaHookBase.InvokeFromCLR(this, args.RawGet(1).ToObject<T1>(), args.RawGet(2).ToObject<T2>());
	}
}
public class LuaHook<T1, T2, T3> : LuaHookBase
{
	public Signal<T1, T2, T3> _signal;

	public LuaHook()
	{
		_signal = new Signal<T1, T2, T3>
		{
			MaximumConnections = 1000
		};
	}

	public LuaHook(Signal<T1, T2, T3> signal)
	{
		_signal = signal;
	}

	public LuaHook(IReadOnlySignal<T1, T2, T3> signal)
	{
		_signal = (Signal<T1, T2, T3>)signal;
	}

	public LuaHook(GameSignals.GameSignal<T1, T2, T3> signal)
	{
		_signal = signal._signal;
	}

	protected override void ConnectTemplated(Closure act, LuaHook dishook)
	{
		LuaHookBase.ConnectFromCLR(this, act, dishook);
	}

	protected override void ConnectOnceTemplated(Closure act, LuaHook dishook)
	{
		LuaHookBase.ConnectOnceFromCLR(this, act, dishook);
	}

	protected override void InvokeTemplated(Table args)
	{
		LuaHookBase.InvokeFromCLR(this, args.RawGet(1).ToObject<T1>(), args.RawGet(2).ToObject<T2>(), args.RawGet(3).ToObject<T3>());
	}
}
public class LuaHook<T1, T2, T3, T4> : LuaHookBase
{
	public Signal<T1, T2, T3, T4> _signal;

	public LuaHook()
	{
		_signal = new Signal<T1, T2, T3, T4>
		{
			MaximumConnections = 1000
		};
	}

	public LuaHook(Signal<T1, T2, T3, T4> signal)
	{
		_signal = signal;
	}

	public LuaHook(IReadOnlySignal<T1, T2, T3, T4> signal)
	{
		_signal = (Signal<T1, T2, T3, T4>)signal;
	}

	public LuaHook(GameSignals.GameSignal<T1, T2, T3, T4> signal)
	{
		_signal = signal._signal;
	}

	protected override void ConnectTemplated(Closure act, LuaHook dishook)
	{
		LuaHookBase.ConnectFromCLR(this, act, dishook);
	}

	protected override void ConnectOnceTemplated(Closure act, LuaHook dishook)
	{
		LuaHookBase.ConnectOnceFromCLR(this, act, dishook);
	}

	protected override void InvokeTemplated(Table args)
	{
		LuaHookBase.InvokeFromCLR(this, args.RawGet(1).ToObject<T1>(), args.RawGet(2).ToObject<T2>(), args.RawGet(3).ToObject<T3>(), args.RawGet(4).ToObject<T4>());
	}
}
public class LuaHook<T1, T2, T3, T4, T5> : LuaHookBase
{
	public Signal<T1, T2, T3, T4, T5> _signal;

	public LuaHook()
	{
		_signal = new Signal<T1, T2, T3, T4, T5>
		{
			MaximumConnections = 1000
		};
	}

	public LuaHook(Signal<T1, T2, T3, T4, T5> signal)
	{
		_signal = signal;
	}

	public LuaHook(IReadOnlySignal<T1, T2, T3, T4, T5> signal)
	{
		_signal = (Signal<T1, T2, T3, T4, T5>)signal;
	}

	protected override void ConnectTemplated(Closure act, LuaHook dishook)
	{
		LuaHookBase.ConnectFromCLR(this, act, dishook);
	}

	protected override void ConnectOnceTemplated(Closure act, LuaHook dishook)
	{
		LuaHookBase.ConnectOnceFromCLR(this, act, dishook);
	}

	protected override void InvokeTemplated(Table args)
	{
		LuaHookBase.InvokeFromCLR(this, args.RawGet(1).ToObject<T1>(), args.RawGet(2).ToObject<T2>(), args.RawGet(3).ToObject<T3>(), args.RawGet(4).ToObject<T4>(), args.RawGet(5).ToObject<T5>());
	}
}
