using System;
using System.Collections.Generic;
using System.Reflection;
using Brix.Engine;
using Brix.External.Signals;
using Smooth.Slinq;

namespace Brix.Lua;

public class LuaHookCollapsed : LuaHook
{
	private bool _hasWitnessed;

	private SignalConnection[] _collapsedConnections;

	private LuaHookCollapsed()
	{
	}

	private LuaHookCollapsed(IReadOnlySignal signal)
		: base(signal)
	{
	}

	private LuaHookCollapsed(GameSignals.GameSignal signal)
		: base(signal)
	{
	}

	public LuaHookCollapsed(IReadOnlySignal pollSignal, Signal[] sigs)
	{
		CreateCollapsed(pollSignal, sigs);
	}

	public LuaHookCollapsed(IReadOnlySignal pollSignal, LuaHook[] sigs)
	{
		Signal[] array = new Signal[sigs.Length];
		for (int i = 0; i != sigs.Length; i++)
		{
			array[i] = sigs[i]._signal;
		}
		CreateCollapsed(pollSignal, array);
	}

	public LuaHookCollapsed(IReadOnlySignal pollSignal, GameSignals.GameSignal[] sigs)
	{
		FieldInfo field = typeof(GameSignals.GameSignal).GetField("_signal");
		Signal[] array = new Signal[sigs.Length];
		for (int i = 0; i != sigs.Length; i++)
		{
			array[i] = (Signal)field.GetValue(sigs[i]);
		}
		CreateCollapsed(pollSignal, array);
	}

	private void CreateCollapsed(IReadOnlySignal pollSignal, Signal[] sigs)
	{
		_collapsedConnections = new SignalConnection[sigs.Length + 1];
		for (int i = 0; i != sigs.Length; i++)
		{
			ref SignalConnection reference = ref _collapsedConnections[i];
			reference = sigs[i].Connect(delegate(LuaHookCollapsed self)
			{
				self._hasWitnessed = true;
			}, this);
		}
		ref SignalConnection reference2 = ref _collapsedConnections[sigs.Length];
		reference2 = pollSignal.Connect(delegate(LuaHookCollapsed hook)
		{
			try
			{
				if (hook._hasWitnessed)
				{
					LuaHookBase.InvokeFromCLR(hook);
				}
				hook._hasWitnessed = false;
			}
			catch (Exception e)
			{
				LuaLibs.LuaExceptionCatcher(e);
			}
		}, this);
	}

	~LuaHookCollapsed()
	{
		if (_hasWitnessed)
		{
			SignalConnection[] collapsedConnections = _collapsedConnections;
			int i = 0;
			for (; i < collapsedConnections.Length; i++)
			{
				SignalConnection signalConnection = collapsedConnections[i];
				signalConnection.Disconnect();
			}
		}
	}
}
public class LuaHookCollapsed<T1> : LuaHook<List<T1>>
{
	private ICollection<T1> _collapsedWitnesses;

	private SignalConnection[] _collapsedConnections;

	private LuaHookCollapsed()
	{
	}

	private LuaHookCollapsed(IReadOnlySignal<T1> signal)
	{
	}

	private LuaHookCollapsed(GameSignals.GameSignal<T1> signal)
	{
	}

	public LuaHookCollapsed(IReadOnlySignal pollSignal, Signal<T1>[] sigs, bool noDoubles = false)
	{
		CreateCollapsed(pollSignal, sigs, noDoubles);
	}

	public LuaHookCollapsed(IReadOnlySignal pollSignal, LuaHook<T1>[] sigs, bool noDoubles = false)
	{
		Signal<T1>[] array = new Signal<T1>[sigs.Length];
		for (int i = 0; i != sigs.Length; i++)
		{
			array[i] = sigs[i]._signal;
		}
		CreateCollapsed(pollSignal, array, noDoubles);
	}

	public LuaHookCollapsed(IReadOnlySignal pollSignal, GameSignals.GameSignal<T1>[] sigs, bool noDoubles = false)
	{
		FieldInfo field = typeof(GameSignals.GameSignal<T1>).GetField("_signal");
		Signal<T1>[] array = new Signal<T1>[sigs.Length];
		for (int i = 0; i != sigs.Length; i++)
		{
			array[i] = (Signal<T1>)field.GetValue(sigs[i]);
		}
		CreateCollapsed(pollSignal, array, noDoubles);
	}

	private void CreateCollapsed(IReadOnlySignal pollSignal, Signal<T1>[] sigs, bool noDoubles)
	{
		_collapsedConnections = new SignalConnection[sigs.Length + 1];
		object collapsedWitnesses;
		if (noDoubles)
		{
			ICollection<T1> collection = new HashSet<T1>();
			collapsedWitnesses = collection;
		}
		else
		{
			collapsedWitnesses = new List<T1>(sigs.Length);
		}
		_collapsedWitnesses = (ICollection<T1>)collapsedWitnesses;
		for (int i = 0; i != sigs.Length; i++)
		{
			ref SignalConnection reference = ref _collapsedConnections[i];
			reference = sigs[i].Connect(delegate(T1 arg, LuaHookCollapsed<T1> self)
			{
				self._collapsedWitnesses.Add(arg);
			}, this);
		}
		ref SignalConnection reference2 = ref _collapsedConnections[sigs.Length];
		reference2 = pollSignal.Connect(delegate(LuaHookCollapsed<T1> hook, bool isHash)
		{
			try
			{
				if (hook._collapsedWitnesses.Count > 0)
				{
					LuaHookBase.InvokeFromCLR(hook, (!isHash) ? ((List<T1>)hook._collapsedWitnesses) : hook._collapsedWitnesses.Slinq().ToList());
					hook._collapsedWitnesses.Clear();
				}
			}
			catch (Exception e)
			{
				LuaLibs.LuaExceptionCatcher(e);
			}
		}, this, noDoubles);
	}

	~LuaHookCollapsed()
	{
		if (_collapsedWitnesses != null)
		{
			SignalConnection[] collapsedConnections = _collapsedConnections;
			int i = 0;
			for (; i < collapsedConnections.Length; i++)
			{
				SignalConnection signalConnection = collapsedConnections[i];
				signalConnection.Disconnect();
			}
		}
	}
}
