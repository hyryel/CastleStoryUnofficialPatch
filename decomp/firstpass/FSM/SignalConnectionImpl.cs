using Brix.External.Signals;

namespace FSM;

internal class SignalConnectionImpl : SignalConnection
{
	private Brix.External.Signals.SignalConnection _connection;

	public SignalConnectionImpl(Signal sig, SignalStateRun run)
		: base(sig)
	{
		_connection = sig.Connect(delegate(Signal _sig, SignalStateRun _run)
		{
			_run.HandleSignal(_sig);
		}, sig, run);
	}

	public override void Disconnect()
	{
		_connection.Disconnect();
	}
}
internal class SignalConnectionImpl<P1> : SignalConnection
{
	private Brix.External.Signals.SignalConnection _connection;

	public SignalConnectionImpl(Signal<P1> sig, SignalStateRun run)
		: base(sig)
	{
		_connection = sig.Connect(delegate(P1 p1, Signal<P1> _sig, SignalStateRun _run)
		{
			_run.HandleSignal(_sig, p1);
		}, sig, run);
	}

	public override void Disconnect()
	{
		_connection.Disconnect();
	}
}
internal class SignalConnectionImpl<P1, P2> : SignalConnection
{
	private Brix.External.Signals.SignalConnection _connection;

	public SignalConnectionImpl(Signal<P1, P2> sig, SignalStateRun run)
		: base(sig)
	{
		_connection = sig.Connect(delegate(P1 p1, P2 p2, Signal<P1, P2> _sig, SignalStateRun _run)
		{
			_run.HandleSignal(_sig, p1, p2);
		}, sig, run);
	}

	public override void Disconnect()
	{
		_connection.Disconnect();
	}
}
internal class SignalConnectionImpl<P1, P2, P3> : SignalConnection
{
	private Brix.External.Signals.SignalConnection _connection;

	public SignalConnectionImpl(Signal<P1, P2, P3> sig, SignalStateRun run)
		: base(sig)
	{
		_connection = sig.Connect(delegate(P1 p1, P2 p2, P3 p3, Signal<P1, P2, P3> _sig, SignalStateRun _run)
		{
			_run.HandleSignal(_sig, p1, p2, p3);
		}, sig, run);
	}

	public override void Disconnect()
	{
		_connection.Disconnect();
	}
}
internal class SignalConnectionImpl<P1, P2, P3, P4> : SignalConnection
{
	private Brix.External.Signals.SignalConnection _connection;

	public SignalConnectionImpl(Signal<P1, P2, P3, P4> sig, SignalStateRun run)
		: base(sig)
	{
		_connection = sig.Connect(delegate(P1 p1, P2 p2, P3 p3, P4 p4, Signal<P1, P2, P3, P4> _sig)
		{
			run.HandleSignal(_sig, p1, p2, p3, p4);
		}, sig);
	}

	public override void Disconnect()
	{
		_connection.Disconnect();
	}
}
